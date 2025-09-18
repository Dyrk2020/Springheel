using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class LevelThumbnailCache : MonoBehaviour
{
	private const int alwaysKeepTextures = 20;

	private bool initialized;

	private static LevelThumbnailCache instance;

	private Dictionary<string, Texture2D> cachedTextures = new Dictionary<string, Texture2D>();

	private Dictionary<Texture2D, HashSet<MonoBehaviour>> textureUsers = new Dictionary<Texture2D, HashSet<MonoBehaviour>>();

	private List<string> cachedTextureList = new List<string>();

	public static LevelThumbnailCache Instance
	{
		get
		{
			if (instance == null)
			{
				new GameObject("LevelThumbnailCache").AddComponent<LevelThumbnailCache>();
			}
			return instance;
		}
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		_ = instance == this;
	}

	private void Update()
	{
		ClearOldTextures();
	}

	public void Initialize()
	{
		if (!initialized)
		{
			initialized = true;
			PurgeOldThumbnails(7);
		}
	}

	public void LoadLocalSaveThumbnail(string snapshotName, UnityAction<Texture2D> onThumbnailLoaded)
	{
		string thumbnailFilename = QuickSaver.GetThumbnailFilenameForLocalSave(snapshotName);
		Texture2D value = null;
		if (!cachedTextures.TryGetValue(thumbnailFilename, out value))
		{
			Action<byte[]> OnFileContentsRetrieved = delegate(byte[] array)
			{
				if (array != null)
				{
					Texture2D texture2D = new Texture2D(1, 1);
					texture2D.LoadImage(array);
					AddCachedTexture(thumbnailFilename, texture2D);
					onThumbnailLoaded(texture2D);
				}
				else
				{
					onThumbnailLoaded(null);
				}
			};
			if (RamFS.PlatformUsesRamFS)
			{
				RamFS.AddReadFileOperation(thumbnailFilename, delegate(RamFS.FSOperationReturnCode returnCode, byte[] obj)
				{
					if (returnCode != RamFS.FSOperationReturnCode.OK && returnCode != RamFS.FSOperationReturnCode.FileNotFound)
					{
						Debug.LogError("Error reading thumbnail \"" + thumbnailFilename + "\" from RamFS");
					}
					OnFileContentsRetrieved(obj);
				});
				return;
			}
			byte[] fileContents = null;
			WorkerThreadManager.Instance.AddFileOpJob(delegate
			{
				if (File.Exists(thumbnailFilename))
				{
					try
					{
						FileStream fileStream = File.OpenRead(thumbnailFilename);
						fileContents = new byte[fileStream.Length];
						fileStream.Read(fileContents, 0, (int)fileStream.Length);
						fileStream.Close();
					}
					catch (Exception ex)
					{
						Debug.LogError("Exception while reading thumbnail: " + ex.Message + "\n" + ex.StackTrace);
					}
				}
			}, delegate
			{
				OnFileContentsRetrieved(fileContents);
			});
		}
		else
		{
			onThumbnailLoaded(value);
		}
	}

	private void AddCachedTexture(string textureFilename, Texture2D texture)
	{
		if (!cachedTextures.ContainsKey(textureFilename))
		{
			cachedTextures.Add(textureFilename, texture);
			cachedTextureList.Add(textureFilename);
			textureUsers.Add(texture, new HashSet<MonoBehaviour>());
		}
	}

	private void ClearOldTextures()
	{
		if (cachedTextureList.Count <= 20)
		{
			return;
		}
		List<int> list = new List<int>();
		for (int i = 0; i < cachedTextureList.Count; i++)
		{
			string key = cachedTextureList[i];
			if (cachedTextures.TryGetValue(key, out var value) && value != null)
			{
				bool flag = false;
				if (textureUsers.TryGetValue(value, out var value2))
				{
					foreach (MonoBehaviour item in value2)
					{
						if (item != null)
						{
							flag = true;
							break;
						}
					}
				}
				if (flag)
				{
					continue;
				}
				textureUsers.Remove(value);
				UnityEngine.Object.Destroy(value);
				list.Add(i);
			}
			if (cachedTextureList.Count - list.Count <= 20)
			{
				break;
			}
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			int index = list[num];
			cachedTextures.Remove(cachedTextureList[index]);
			cachedTextureList.RemoveAt(index);
		}
	}

	private void RemoveCachedTexture(string filename)
	{
		if (cachedTextures.TryGetValue(filename, out var value) && value != null)
		{
			UnityEngine.Object.Destroy(value);
		}
		cachedTextures.Remove(filename);
		cachedTextureList.Remove(filename);
	}

	public void LoadThumbnailFromCloud(string snapshotCode, UnityAction<Texture2D> onThumbnailLoaded)
	{
		string thumbnailFilenameForCode = QuickSaver.GetThumbnailFilenameForCode(GameSparksQuery.GetFormattedSnapshotCode(snapshotCode));
		DownloadThumbnailFromCloud(snapshotCode, thumbnailFilenameForCode, onThumbnailLoaded, 1);
	}

	public void DownloadThumbnailFromCloud(string snapshotCode, string thumbnailFilename, UnityAction<Texture2D> onThumbnailLoaded, int numRetries)
	{
		Texture2D value = null;
		if (!cachedTextures.TryGetValue(thumbnailFilename, out value))
		{
			GameSparksQuery gameSparksQuery = GameSparksManager.Instance.CreateQuery();
			gameSparksQuery.GetThumbnailForSnapshotCode(snapshotCode);
			gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate(GameSparksQuery q)
			{
				if (!q.HasError)
				{
					byte[] bytes = q.ResultData["bytes"] as byte[];
					Texture2D texture2D = new Texture2D(1, 1);
					texture2D.LoadImage(bytes);
					bool thumbnailSaveSuccessful = false;
					WorkerThreadManager.Instance.AddFileOpJob(delegate
					{
						thumbnailSaveSuccessful = QuickSaver.SaveBytesToFile(bytes, thumbnailFilename);
					}, delegate
					{
						if (!thumbnailSaveSuccessful)
						{
							Debug.LogError("Could not save thumbnail to: " + thumbnailFilename);
						}
					});
					AddCachedTexture(thumbnailFilename, texture2D);
					onThumbnailLoaded(texture2D);
				}
				else if (numRetries > 0)
				{
					Debug.LogError("Failed to grab level thumbnail, retrying... Error: " + q.Error);
					DownloadThumbnailFromCloud(snapshotCode, thumbnailFilename, onThumbnailLoaded, numRetries - 1);
				}
				else
				{
					Debug.LogError("Failed to grab level thumbnail: " + q.Error);
					onThumbnailLoaded(null);
				}
			});
		}
		else
		{
			onThumbnailLoaded(value);
		}
	}

	public void OnLocalThumbnailRenamed(string oldPath, string newPath)
	{
		if (cachedTextures.ContainsKey(oldPath))
		{
			if (cachedTextures.ContainsKey(newPath))
			{
				RemoveCachedTexture(newPath);
			}
			cachedTextures.Add(newPath, cachedTextures[oldPath]);
			cachedTextures.Remove(oldPath);
			cachedTextureList.Remove(oldPath);
		}
	}

	public void OnLocalThumbnailDeleted(string path)
	{
		RemoveCachedTexture(path);
	}

	public void PurgeOldThumbnails(int olderThanDays)
	{
		QuickSaver.CheckSaveFolders();
		string[] files = Directory.GetFiles(QuickSaver.RemoteThumbnailsFolder, "c_*" + QuickSaver.PreferredThumbnailFormatExtension);
		DateTime utcNow = DateTime.UtcNow;
		string[] array = files;
		foreach (string text in array)
		{
			DateTime creationTimeUtc = File.GetCreationTimeUtc(text);
			if ((utcNow - creationTimeUtc).Days >= olderThanDays)
			{
				Debug.Log("Deleting thumbnail " + text + " because it is over " + olderThanDays + " days old.");
				File.Delete(text);
			}
		}
	}

	public void AddTextureUser(Texture2D texture, MonoBehaviour user)
	{
		if (textureUsers.TryGetValue(texture, out var value))
		{
			value.Add(user);
		}
	}

	public void RemoveTextureUser(Texture2D texture, MonoBehaviour user)
	{
		if (textureUsers.TryGetValue(texture, out var value))
		{
			value.Remove(user);
		}
	}
}
