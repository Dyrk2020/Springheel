using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;
using UnityEngine;

public static class RamFS
{
	public enum FSOperationType
	{
		Unknown,
		Mount,
		Unmount,
		AddFile,
		GetExistingFilenames,
		RenameFile,
		FileExists,
		ClassifySnapshot,
		DeleteFile,
		ReadFile,
		RunFunc,
		CountFiles
	}

	public enum FSOperationReturnCode
	{
		OK,
		IncorrectHeader,
		UnreadableIndexFileSize,
		BlobTooSmall,
		InvalidPath,
		NoFSMounted,
		FileAlreadyExists,
		FileNotFound
	}

	public class FSOperation
	{
		public FSOperationType type;

		public Action<FSOperationReturnCode> OnFinish;
	}

	public class FSMountOperation : FSOperation
	{
		public byte[] image;

		public string userIdentifier;
	}

	public class FSAddFileOperation : FSOperation
	{
		public string filenameWithPath;

		public long timestamp;

		public byte[] blob;
	}

	public class FSGetExistingFilenamesOperation : FSOperation
	{
		public string path = "/";

		public bool ordered = true;

		public IEnumerable<string> filenames;

		public string extensionFilter;
	}

	public class FSRenameFileOperation : FSOperation
	{
		public string originalPath;

		public string newPath;
	}

	public class FSFileExistsOperation : FSOperation
	{
		public string path;
	}

	public class FSClassifySnapshotOperation : FSOperation
	{
		public string rootFilename;

		public Action<FSOperationReturnCode, string> OnResult;
	}

	public class FSDeleteFileOperation : FSOperation
	{
		public string path;
	}

	public class FSReadFileOperation : FSOperation
	{
		public string path;

		public Action<FSOperationReturnCode, byte[]> OnResult;
	}

	public class FSRunFuncOperation : FSOperation
	{
		public Action op;
	}

	public class FSCountFilesOperation : FSOperation
	{
		public string path;

		public Action<FSOperationReturnCode, int> OnResult;
	}

	public class FSFileEntry
	{
		public string parentPath = "/";

		public string filename = "";

		public long timestamp;

		public byte[] blob;
	}

	private const bool TestRamFSInEditor = false;

	private static bool mounted = false;

	private static string mountedForUserId = null;

	private static bool dirty = false;

	private static int cyclesSinceIdle = 0;

	private static byte[] loadedBytes;

	private static bool waitingForLoadedData = false;

	private static List<FSFileEntry> fileEntries;

	private static ConcurrentQueue<FSOperation> pendingOperations = new ConcurrentQueue<FSOperation>();

	private static ConcurrentQueue<Action> mainThreadActions = new ConcurrentQueue<Action>();

	private static string onFlushToDiskMessage;

	public static bool PlatformUsesRamFS => false;

	private static void EnqueueOperation(FSOperation op)
	{
		Debug.Log("DEBUG: Enqueuing " + op.type);
		pendingOperations.Enqueue(op);
	}

	public static void AddMountOperation(byte[] image, string userIdentifier, Action<FSOperationReturnCode> OnFinish)
	{
		EnqueueOperation(new FSMountOperation
		{
			type = FSOperationType.Mount,
			image = image,
			OnFinish = OnFinish,
			userIdentifier = userIdentifier
		});
	}

	private static void Mount(FSMountOperation mountOp)
	{
		if (mounted)
		{
			RunOnMainThread(delegate
			{
				Debug.LogError("RamFS.Mount: A volume is already mounted. Forcing unmount.");
			});
			Unmount(new FSOperation
			{
				type = FSOperationType.Unmount
			});
		}
		if (mountOp.image != null)
		{
			FSOperationReturnCode returnCode = ReadFSFile(mountOp.image, out fileEntries);
			if (returnCode == FSOperationReturnCode.OK)
			{
				mounted = true;
				mountedForUserId = mountOp.userIdentifier;
			}
			else
			{
				RunOnMainThread(delegate
				{
					Debug.LogError("RamFS.Mount: Error reading FS file: " + returnCode);
				});
			}
			if (mountOp.OnFinish != null)
			{
				RunOnMainThread(delegate
				{
					mountOp.OnFinish(returnCode);
				});
			}
			return;
		}
		FSOperationReturnCode returnCode2 = CreateNewBlankVFS(out fileEntries);
		if (returnCode2 != FSOperationReturnCode.OK)
		{
			return;
		}
		mounted = true;
		mountedForUserId = mountOp.userIdentifier;
		dirty = false;
		Debug.Log("Created new blank VFS for current user.");
		if (mountOp.OnFinish != null)
		{
			RunOnMainThread(delegate
			{
				mountOp.OnFinish(returnCode2);
			});
		}
	}

	public static void AddUnmountOperation(Action<FSOperationReturnCode> OnFinish)
	{
		EnqueueOperation(new FSMountOperation
		{
			type = FSOperationType.Unmount,
			OnFinish = OnFinish
		});
	}

	private static void Unmount(FSOperation unmountOp)
	{
		if (mounted)
		{
			if (dirty)
			{
				FlushToDisk();
			}
			fileEntries = null;
			mounted = false;
			mountedForUserId = null;
		}
		if (unmountOp.OnFinish != null)
		{
			RunOnMainThread(delegate
			{
				unmountOp.OnFinish(FSOperationReturnCode.OK);
			});
		}
	}

	public static void AddAddFileOperation(string filenameWithPath, byte[] blob, Action<FSOperationReturnCode> OnFinish)
	{
		EnqueueOperation(new FSAddFileOperation
		{
			type = FSOperationType.AddFile,
			OnFinish = OnFinish,
			filenameWithPath = filenameWithPath,
			timestamp = DateTime.UtcNow.Ticks,
			blob = blob
		});
	}

	private static void AddFile(FSAddFileOperation op)
	{
		if (!mounted)
		{
			RunOnMainThread(delegate
			{
				Debug.LogError("RamFS.AddFile: Error while adding file: \"" + op.filenameWithPath + "\" -- no filesystem is currently mounted");
				if (op.OnFinish != null)
				{
					op.OnFinish(FSOperationReturnCode.NoFSMounted);
				}
			});
			return;
		}
		FSFileEntry fileEntry = new FSFileEntry
		{
			blob = op.blob,
			timestamp = op.timestamp
		};
		if (ExtractParentPathAndFilename(op.filenameWithPath, out fileEntry.parentPath, out fileEntry.filename) != FSOperationReturnCode.OK)
		{
			RunOnMainThread(delegate
			{
				Debug.LogError("RamFS.AddFile: Error while adding file: \"" + op.filenameWithPath + "\" -- path is invalid");
				if (op.OnFinish != null)
				{
					op.OnFinish(FSOperationReturnCode.InvalidPath);
				}
			});
			return;
		}
		if (fileEntries.Find((FSFileEntry entry) => entry.parentPath == fileEntry.parentPath && entry.filename == fileEntry.filename) != null)
		{
			RunOnMainThread(delegate
			{
				Debug.LogError("RamFS.AddFile: Error while adding file: \"" + op.filenameWithPath + "\" -- file already exists");
				if (op.OnFinish != null)
				{
					op.OnFinish(FSOperationReturnCode.FileAlreadyExists);
				}
			});
			return;
		}
		fileEntries.Add(fileEntry);
		dirty = true;
		if (op.OnFinish != null)
		{
			RunOnMainThread(delegate
			{
				op.OnFinish(FSOperationReturnCode.OK);
			});
		}
	}

	public static void AddRunFuncOperation(Action func)
	{
		EnqueueOperation(new FSOperation
		{
			type = FSOperationType.RunFunc,
			OnFinish = delegate
			{
				func();
			}
		});
	}

	private static void RunFunc(FSOperation op)
	{
		if (op.OnFinish != null)
		{
			RunOnMainThread(delegate
			{
				op.OnFinish(FSOperationReturnCode.OK);
			});
		}
	}

	public static void AddCountFilesOperation(string path, Action<FSOperationReturnCode, int> OnResult)
	{
		EnqueueOperation(new FSCountFilesOperation
		{
			type = FSOperationType.CountFiles,
			OnResult = OnResult,
			path = path
		});
	}

	private static void CountFiles(FSCountFilesOperation op)
	{
		if (!mounted)
		{
			RunOnMainThread(delegate
			{
				Debug.LogError("RamFS.CountFiles: Error while counting files in: \"" + op.path + "\" -- no filesystem is currently mounted");
				if (op.OnResult != null)
				{
					op.OnResult(FSOperationReturnCode.NoFSMounted, 0);
				}
			});
		}
		else if (op.OnResult != null)
		{
			int count = 0;
			foreach (FSFileEntry fileEntry in fileEntries)
			{
				if (fileEntry.parentPath == op.path)
				{
					count++;
				}
			}
			RunOnMainThread(delegate
			{
				op.OnResult(FSOperationReturnCode.OK, count);
			});
		}
		else
		{
			Debug.LogError("RamFS.CountFiles: No OnResult callback set!");
		}
	}

	public static FSOperationReturnCode ExtractParentPathAndFilename(string input, out string parentPath, out string filename)
	{
		if (input == null || input[0] != '/' || input.Length < 2)
		{
			parentPath = null;
			filename = null;
			return FSOperationReturnCode.InvalidPath;
		}
		int num = input.LastIndexOf('/');
		if (num != -1 && num < input.Length - 1)
		{
			parentPath = input.Substring(0, num + 1);
			filename = input.Substring(num + 1);
			return FSOperationReturnCode.OK;
		}
		parentPath = null;
		filename = null;
		return FSOperationReturnCode.InvalidPath;
	}

	public static void MainThreadUpdate()
	{
		if (mainThreadActions.TryDequeue(out var item))
		{
			item();
		}
	}

	private static byte[] GetFSFileAsBytes()
	{
		XmlDocument xmlDocument = new XmlDocument();
		XmlElement xmlElement = xmlDocument.CreateElement("files");
		List<byte[]> list = new List<byte[]>();
		int num = 0;
		foreach (FSFileEntry fileEntry in fileEntries)
		{
			XmlElement xmlElement2 = xmlDocument.CreateElement("file");
			QuickSaver.AddAttribute(xmlDocument, xmlElement2, "parentPath", fileEntry.parentPath);
			QuickSaver.AddAttribute(xmlDocument, xmlElement2, "filename", fileEntry.filename);
			QuickSaver.AddAttribute(xmlDocument, xmlElement2, "timestamp", fileEntry.timestamp.ToString());
			QuickSaver.AddAttribute(xmlDocument, xmlElement2, "blobIndex", num.ToString());
			QuickSaver.AddAttribute(xmlDocument, xmlElement2, "blobSize", fileEntry.blob.Length.ToString());
			list.Add(fileEntry.blob);
			num += fileEntry.blob.Length;
			xmlElement.AppendChild(xmlElement2);
		}
		xmlDocument.AppendChild(xmlElement);
		byte[] compressedBytesFromXmlDoc = QuickSaver.GetCompressedBytesFromXmlDoc(xmlDocument);
		byte[] array = new byte[8] { 85, 67, 72, 48, 0, 0, 0, 0 };
		BitConverter.GetBytes(compressedBytesFromXmlDoc.Length).CopyTo(array, 4);
		int num2 = array.Length + compressedBytesFromXmlDoc.Length;
		byte[] array2 = new byte[num2 + num];
		array.CopyTo(array2, 0);
		compressedBytesFromXmlDoc.CopyTo(array2, array.Length);
		int num3 = num2;
		foreach (byte[] item in list)
		{
			item.CopyTo(array2, num3);
			num3 += item.Length;
		}
		return array2;
	}

	private static FSOperationReturnCode ReadFSFile(byte[] blob, out List<FSFileEntry> outputFileEntries)
	{
		outputFileEntries = null;
		if (blob == null || blob.Length < 8)
		{
			return FSOperationReturnCode.BlobTooSmall;
		}
		byte[] array = new byte[4];
		Array.Copy(blob, 0, array, 0, 4);
		if (array[0] == 85 && array[1] == 67 && array[2] == 72 && array[3] == 48)
		{
			int num = -1;
			try
			{
				num = BitConverter.ToInt32(blob, 4);
			}
			catch (Exception)
			{
				return FSOperationReturnCode.UnreadableIndexFileSize;
			}
			byte[] array2 = new byte[num];
			Array.Copy(blob, 8, array2, 0, num);
			XmlElement documentElement = QuickSaver.GetXmlDocFromBytes(array2).DocumentElement;
			outputFileEntries = new List<FSFileEntry>(documentElement.ChildNodes.Count);
			int num2 = 8 + array2.Length;
			foreach (XmlNode childNode in documentElement.ChildNodes)
			{
				if (!(childNode.Name == "file"))
				{
					continue;
				}
				FSFileEntry fileEntry = new FSFileEntry
				{
					filename = QuickSaver.ParseAttrStr(childNode, "filename"),
					parentPath = QuickSaver.ParseAttrStr(childNode, "parentPath"),
					timestamp = QuickSaver.ParseAttrLong(childNode, "timestamp", -1L)
				};
				long num3 = QuickSaver.ParseAttrLong(childNode, "blobSize", -1L);
				long num4 = QuickSaver.ParseAttrLong(childNode, "blobIndex", -1L);
				fileEntry.blob = new byte[num3];
				try
				{
					Array.Copy(blob, num2 + num4, fileEntry.blob, 0L, num3);
				}
				catch (Exception ex2)
				{
					Exception e = ex2;
					RunOnMainThread(delegate
					{
						Debug.LogError("Error copying contents of file " + fileEntry.filename + ": " + e.Message + "\n" + e.StackTrace);
					});
					fileEntry.blob = null;
				}
				if (fileEntry.blob != null)
				{
					outputFileEntries.Add(fileEntry);
				}
				else
				{
					Debug.LogError("Discarded unreadable file " + fileEntry.filename);
				}
			}
			return FSOperationReturnCode.OK;
		}
		return FSOperationReturnCode.IncorrectHeader;
	}

	private static FSOperationReturnCode CreateNewBlankVFS(out List<FSFileEntry> outputFileEntries)
	{
		outputFileEntries = new List<FSFileEntry>();
		return FSOperationReturnCode.OK;
	}

	private static void RunOnMainThread(Action action)
	{
		mainThreadActions.Enqueue(action);
	}

	public static void WorkerThreadUpdate()
	{
		try
		{
			if (waitingForLoadedData)
			{
				Thread.Sleep(1);
				return;
			}
			int num = 0;
			for (int i = 0; i < 8; i++)
			{
				if (!pendingOperations.TryDequeue(out var item))
				{
					break;
				}
				Debug.Log("DEBUG: " + item.type);
				switch (item.type)
				{
				case FSOperationType.Unknown:
					RunOnMainThread(delegate
					{
						Debug.LogError("Unknown RamFS Operation!");
					});
					break;
				case FSOperationType.Mount:
					Mount(item as FSMountOperation);
					break;
				case FSOperationType.Unmount:
					Unmount(item);
					break;
				case FSOperationType.AddFile:
					AddFile(item as FSAddFileOperation);
					break;
				case FSOperationType.GetExistingFilenames:
					GetExistingFilenames(item as FSGetExistingFilenamesOperation);
					break;
				case FSOperationType.RenameFile:
					RenameFile(item as FSRenameFileOperation);
					break;
				case FSOperationType.FileExists:
					CheckFileExists(item as FSFileExistsOperation);
					break;
				case FSOperationType.ClassifySnapshot:
					ClassifySnapshot(item as FSClassifySnapshotOperation);
					break;
				case FSOperationType.DeleteFile:
					DeleteFile(item as FSDeleteFileOperation);
					break;
				case FSOperationType.ReadFile:
					ReadFile(item as FSReadFileOperation);
					break;
				case FSOperationType.RunFunc:
					RunFunc(item);
					break;
				case FSOperationType.CountFiles:
					CountFiles(item as FSCountFilesOperation);
					break;
				}
				num++;
			}
			if (num > 0)
			{
				cyclesSinceIdle = 0;
			}
			else
			{
				cyclesSinceIdle++;
			}
			if (cyclesSinceIdle > 8 && dirty)
			{
				FlushToDisk();
				cyclesSinceIdle = 0;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception in RAMFS worker thread: " + ex.Message + "\n" + ex.StackTrace);
		}
	}

	private static void LoadFromDisk(string platformUserId, Action<FSOperationReturnCode> OnFinish)
	{
		string ramFSFolder = Application.persistentDataPath + "/ramfs";
		WorkerThreadManager.Instance.AddFileOpJob(delegate
		{
			if (!Directory.Exists(ramFSFolder))
			{
				Debug.Log("Creating RamFS folder at " + ramFSFolder);
				Directory.CreateDirectory(ramFSFolder);
			}
			bool flag = false;
			FSOperationReturnCode returnCode = FSOperationReturnCode.OK;
			try
			{
				string path = ramFSFolder + "/snapshots.fs";
				if (File.Exists(path))
				{
					FileStream fileStream = File.OpenRead(path);
					byte[] array = new byte[fileStream.Length];
					fileStream.Read(array, 0, (int)fileStream.Length);
					fileStream.Close();
					Debug.Log("Loading successful -- mounting snapshot VFS image (" + array.Length + " bytes)");
					AddMountOperation(array, null, delegate(FSOperationReturnCode mountReturnCode)
					{
						returnCode = mountReturnCode;
					});
					flag = true;
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("Exception while trying to read RamFS from disk: " + ex.Message + "\n" + ex.StackTrace);
			}
			if (!flag)
			{
				Debug.Log("No VFS image found or ran into a load error... Creating new VFS...");
				AddMountOperation(null, null, delegate(FSOperationReturnCode mountReturnCode)
				{
					returnCode = mountReturnCode;
				});
			}
			RunOnMainThread(delegate
			{
				OnFinish(returnCode);
			});
		});
	}

	public static void OnDataLoaded(byte[] data)
	{
		loadedBytes = data;
	}

	private static void FlushToDisk()
	{
		byte[] bytes = GetFSFileAsBytes();
		dirty = false;
		Debug.Log("Flushing RamFS to disk...");
		RunOnMainThread(delegate
		{
			string text = Application.persistentDataPath + "/ramfs";
			if (!Directory.Exists(text))
			{
				Debug.Log("Creating RamFS folder at " + text);
				Directory.CreateDirectory(text);
			}
			try
			{
				FileStream fileStream = File.OpenWrite(text + "/snapshots.fs");
				fileStream.Write(bytes, 0, bytes.Length);
				fileStream.Close();
			}
			catch (Exception ex)
			{
				Debug.LogError("Exception while trying to flush RamFS to disk: " + ex.Message + "\n" + ex.StackTrace);
			}
		});
	}

	public static void OnMainUserGameLoaded(Action<FSOperationReturnCode> OnFinish)
	{
		if (ControllerMonitor.Instance.IsMainControllerSet)
		{
			LoadFromDisk(null, OnFinish);
		}
		else
		{
			Debug.LogError("ERROR in RamFS.OnMainUserGameLoaded: Main controller is not set");
		}
	}

	public static void AddGetExistingFilenamesOperation(string path, string extensionFilter, bool ordered, Action<IEnumerable<string>> OnGetExistingFilenames)
	{
		FSGetExistingFilenamesOperation op = new FSGetExistingFilenamesOperation
		{
			type = FSOperationType.GetExistingFilenames,
			path = path,
			extensionFilter = extensionFilter,
			ordered = ordered
		};
		op.OnFinish = delegate(FSOperationReturnCode returnCode)
		{
			if (returnCode == FSOperationReturnCode.OK)
			{
				OnGetExistingFilenames(op.filenames);
			}
			else
			{
				Debug.LogError("Could not get existing filenames for path \"" + path + "\" -- " + returnCode);
				OnGetExistingFilenames(null);
			}
		};
		EnqueueOperation(op);
	}

	private static void GetExistingFilenames(FSGetExistingFilenamesOperation filenamesOp)
	{
		if (!mounted)
		{
			RunOnMainThread(delegate
			{
				Debug.LogError("RamFS.GetExistingFilenames: Error while retrieving filenames -- no filesystem is currently mounted");
				if (filenamesOp.OnFinish != null)
				{
					filenamesOp.OnFinish(FSOperationReturnCode.NoFSMounted);
				}
			});
			return;
		}
		IEnumerable<string> enumerable = ((!filenamesOp.ordered) ? ((IEnumerable<string>)new HashSet<string>()) : ((IEnumerable<string>)new List<string>()));
		foreach (FSFileEntry fileEntry in fileEntries)
		{
			if (fileEntry.parentPath == filenamesOp.path && (filenamesOp.extensionFilter == null || fileEntry.filename.EndsWith(filenamesOp.extensionFilter)))
			{
				if (filenamesOp.ordered)
				{
					((List<string>)enumerable).Add(fileEntry.parentPath + fileEntry.filename);
				}
				else
				{
					((HashSet<string>)enumerable).Add(fileEntry.parentPath + fileEntry.filename);
				}
			}
		}
		filenamesOp.filenames = enumerable;
		if (filenamesOp.OnFinish != null)
		{
			RunOnMainThread(delegate
			{
				filenamesOp.OnFinish(FSOperationReturnCode.OK);
			});
		}
	}

	private static FSFileEntry FindFile(string parentPath, string filename)
	{
		foreach (FSFileEntry fileEntry in fileEntries)
		{
			if (fileEntry.parentPath == parentPath && fileEntry.filename == filename)
			{
				return fileEntry;
			}
		}
		return null;
	}

	private static int FindFileIndex(string parentPath, string filename)
	{
		for (int i = 0; i < fileEntries.Count; i++)
		{
			FSFileEntry fSFileEntry = fileEntries[i];
			if (fSFileEntry.parentPath == parentPath && fSFileEntry.filename == filename)
			{
				return i;
			}
		}
		return -1;
	}

	public static void AddRenameFileOperation(string originalPath, string newPath, Action<FSOperationReturnCode> OnFinish)
	{
		EnqueueOperation(new FSRenameFileOperation
		{
			type = FSOperationType.RenameFile,
			originalPath = originalPath,
			newPath = newPath,
			OnFinish = OnFinish
		});
	}

	private static void RenameFile(FSRenameFileOperation op)
	{
		if (!mounted)
		{
			RunOnMainThread(delegate
			{
				Debug.LogError("RamFS.RenameFile: Error while renaming file -- no filesystem is currently mounted");
				if (op.OnFinish != null)
				{
					op.OnFinish(FSOperationReturnCode.NoFSMounted);
				}
			});
			return;
		}
		FSOperationReturnCode resultCode = DoRenameFile(op.originalPath, op.newPath);
		if (op.OnFinish != null)
		{
			RunOnMainThread(delegate
			{
				op.OnFinish(resultCode);
			});
		}
	}

	private static FSOperationReturnCode DoRenameFile(string originalPath, string newPath)
	{
		if (ExtractParentPathAndFilename(originalPath, out var parentPath, out var filename) != FSOperationReturnCode.OK)
		{
			return FSOperationReturnCode.InvalidPath;
		}
		if (ExtractParentPathAndFilename(newPath, out var parentPath2, out var filename2) != FSOperationReturnCode.OK)
		{
			return FSOperationReturnCode.InvalidPath;
		}
		if (FindFile(parentPath2, filename2) != null)
		{
			return FSOperationReturnCode.FileAlreadyExists;
		}
		FSFileEntry fSFileEntry = FindFile(parentPath, filename);
		if (fSFileEntry != null)
		{
			fSFileEntry.parentPath = parentPath2;
			fSFileEntry.filename = filename2;
			dirty = true;
			return FSOperationReturnCode.OK;
		}
		return FSOperationReturnCode.FileNotFound;
	}

	public static void AddFileExistsOperation(string filePath, Action<FSOperationReturnCode> OnFinish)
	{
		EnqueueOperation(new FSFileExistsOperation
		{
			type = FSOperationType.FileExists,
			path = filePath,
			OnFinish = OnFinish
		});
	}

	private static void CheckFileExists(FSFileExistsOperation op)
	{
		string parentPath;
		string filename;
		if (!mounted)
		{
			RunOnMainThread(delegate
			{
				Debug.LogError("RamFS.CheckFileExists: Error while checking file for existence -- no filesystem is currently mounted");
				if (op.OnFinish != null)
				{
					op.OnFinish(FSOperationReturnCode.NoFSMounted);
				}
			});
		}
		else if (ExtractParentPathAndFilename(op.path, out parentPath, out filename) != FSOperationReturnCode.OK)
		{
			if (op.OnFinish != null)
			{
				RunOnMainThread(delegate
				{
					op.OnFinish(FSOperationReturnCode.InvalidPath);
				});
			}
		}
		else if (FindFile(parentPath, filename) != null)
		{
			if (op.OnFinish != null)
			{
				RunOnMainThread(delegate
				{
					op.OnFinish(FSOperationReturnCode.OK);
				});
			}
		}
		else if (op.OnFinish != null)
		{
			RunOnMainThread(delegate
			{
				op.OnFinish(FSOperationReturnCode.FileNotFound);
			});
		}
	}

	public static void AddClassifySnapshotOperation(string rootFilename, Action<FSOperationReturnCode, string> OnResult)
	{
		EnqueueOperation(new FSClassifySnapshotOperation
		{
			type = FSOperationType.ClassifySnapshot,
			rootFilename = rootFilename,
			OnResult = OnResult
		});
	}

	private static void ClassifySnapshot(FSClassifySnapshotOperation op)
	{
		string parentPath;
		string filename;
		if (!mounted)
		{
			RunOnMainThread(delegate
			{
				Debug.LogError("RamFS.ClassifySnapshot: Error while classifying snapshot -- no filesystem is currently mounted");
				if (op.OnFinish != null)
				{
					op.OnFinish(FSOperationReturnCode.NoFSMounted);
				}
			});
		}
		else if (ExtractParentPathAndFilename(op.rootFilename, out parentPath, out filename) != FSOperationReturnCode.OK)
		{
			RunOnMainThread(delegate
			{
				op.OnResult(FSOperationReturnCode.InvalidPath, null);
			});
		}
		else if (FindFile(parentPath, filename + ".snapshot") != null)
		{
			RunOnMainThread(delegate
			{
				op.OnResult(FSOperationReturnCode.OK, "");
			});
		}
		else if (FindFile(parentPath, filename + ".c.snapshot") != null)
		{
			RunOnMainThread(delegate
			{
				op.OnResult(FSOperationReturnCode.OK, ".c");
			});
		}
		else if (FindFile(parentPath, filename + ".v.snapshot") != null)
		{
			RunOnMainThread(delegate
			{
				op.OnResult(FSOperationReturnCode.OK, ".v");
			});
		}
		else
		{
			RunOnMainThread(delegate
			{
				op.OnResult(FSOperationReturnCode.FileNotFound, null);
			});
		}
	}

	public static void AddDeleteFileOperation(string path, Action<FSOperationReturnCode> OnFinish)
	{
		EnqueueOperation(new FSDeleteFileOperation
		{
			type = FSOperationType.DeleteFile,
			path = path,
			OnFinish = OnFinish
		});
	}

	private static void DeleteFile(FSDeleteFileOperation op)
	{
		if (!mounted)
		{
			RunOnMainThread(delegate
			{
				Debug.LogError("RamFS.DeleteFile: Error while deleting file -- no filesystem is currently mounted");
				if (op.OnFinish != null)
				{
					op.OnFinish(FSOperationReturnCode.NoFSMounted);
				}
			});
			return;
		}
		FSOperationReturnCode returnCode = DoDeleteFile(op.path);
		if (op.OnFinish != null)
		{
			RunOnMainThread(delegate
			{
				op.OnFinish(returnCode);
			});
		}
	}

	private static FSOperationReturnCode DoDeleteFile(string path)
	{
		if (ExtractParentPathAndFilename(path, out var parentPath, out var filename) != FSOperationReturnCode.OK)
		{
			return FSOperationReturnCode.InvalidPath;
		}
		int num = FindFileIndex(parentPath, filename);
		if (num == -1)
		{
			return FSOperationReturnCode.FileNotFound;
		}
		fileEntries.RemoveAt(num);
		dirty = true;
		return FSOperationReturnCode.OK;
	}

	public static void AddReadFileOperation(string path, Action<FSOperationReturnCode, byte[]> OnResult)
	{
		EnqueueOperation(new FSReadFileOperation
		{
			type = FSOperationType.ReadFile,
			path = path,
			OnResult = OnResult
		});
	}

	private static void ReadFile(FSReadFileOperation op)
	{
		if (!mounted)
		{
			RunOnMainThread(delegate
			{
				Debug.LogError("RamFS.ReadFile: Error while attempting to read file -- no filesystem is currently mounted");
				if (op.OnFinish != null)
				{
					op.OnFinish(FSOperationReturnCode.NoFSMounted);
				}
			});
			return;
		}
		byte[] bytes;
		FSOperationReturnCode returnCode = DoReadFile(op.path, out bytes);
		if (op.OnResult != null)
		{
			RunOnMainThread(delegate
			{
				op.OnResult(returnCode, bytes);
			});
		}
	}

	private static FSOperationReturnCode DoReadFile(string path, out byte[] bytes)
	{
		if (ExtractParentPathAndFilename(path, out var parentPath, out var filename) != FSOperationReturnCode.OK)
		{
			bytes = null;
			return FSOperationReturnCode.InvalidPath;
		}
		FSFileEntry fSFileEntry = FindFile(parentPath, filename);
		if (fSFileEntry != null)
		{
			bytes = (byte[])fSFileEntry.blob.Clone();
			return FSOperationReturnCode.OK;
		}
		bytes = null;
		return FSOperationReturnCode.FileNotFound;
	}

	public static void PostUserMessageOnFlushToDisk(string message)
	{
		onFlushToDiskMessage = message;
	}
}
