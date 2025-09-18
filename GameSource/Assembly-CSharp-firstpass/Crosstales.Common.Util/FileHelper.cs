using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Crosstales.Common.Util;

public static class FileHelper
{
	private static string _applicationDataPath;

	private static string _applicationTempPath;

	private static string _applicationPersistentPath;

	private static char[] _invalidFilenameChars;

	private static char[] _invalidPathChars;

	public static string StreamingAssetsPath
	{
		get
		{
			if (BaseHelper.isAndroidPlatform && !BaseHelper.isEditor)
			{
				return "jar:file://" + ApplicationDataPath + "!/assets/";
			}
			if (BaseHelper.isIOSBasedPlatform && !BaseHelper.isEditor)
			{
				return ApplicationDataPath + "/Raw/";
			}
			return ApplicationDataPath + "/StreamingAssets/";
		}
	}

	public static string ApplicationDataPath => _applicationDataPath;

	public static string ApplicationTempPath => _applicationTempPath;

	public static string ApplicationPersistentPath => _applicationPersistentPath;

	public static string TempFile => Path.GetTempFileName();

	public static string TempPath => Path.GetTempPath();

	[RuntimeInitializeOnLoadMethod]
	private static void initialize()
	{
		_applicationDataPath = ValidatePath(Application.dataPath);
		_applicationTempPath = ValidatePath(Application.temporaryCachePath);
		_applicationPersistentPath = ValidatePath(Application.persistentDataPath);
		_invalidFilenameChars = new HashSet<char>(Path.GetInvalidFileNameChars())
		{
			Path.DirectorySeparatorChar,
			Path.AltDirectorySeparatorChar,
			'<',
			'>',
			':',
			'"',
			'/',
			'\\',
			'|',
			'?',
			'*'
		}.ToArray();
		_invalidPathChars = new HashSet<char>(Path.GetInvalidPathChars()) { '<', '>', '"', '|', '?', '*' }.ToArray();
	}

	public static bool isUnixPath(string path)
	{
		if (!string.IsNullOrEmpty(path))
		{
			return path.StartsWith("/");
		}
		return false;
	}

	public static bool isWindowsPath(string path)
	{
		if (!string.IsNullOrEmpty(path))
		{
			return BaseConstants.REGEX_DRIVE_LETTERS.IsMatch(path);
		}
		return false;
	}

	public static bool isUNCPath(string path)
	{
		if (!string.IsNullOrEmpty(path))
		{
			return path.StartsWith("\\\\");
		}
		return false;
	}

	public static bool isURL(string path)
	{
		return NetworkHelper.isURL(path);
	}

	public static string ValidatePath(string path, bool addEndDelimiter = true, bool preserveFile = true, bool removeInvalidChars = true)
	{
		if (string.IsNullOrEmpty(path))
		{
			return path;
		}
		if (isURL(path))
		{
			if (addEndDelimiter && !path.EndsWith("/"))
			{
				path += "/";
			}
			return path;
		}
		string text = ((!preserveFile && ExistsFile(path.Trim())) ? GetDirectoryName(path.Trim()) : path.Trim());
		string text2 = text;
		if (isWindowsPath(text) || isUNCPath(text))
		{
			text2 = text.Replace('/', '\\');
			if (addEndDelimiter && !text2.EndsWith("\\"))
			{
				text2 += "\\";
			}
		}
		else
		{
			text2 = text.Replace('\\', '/');
			if (addEndDelimiter && !text2.EndsWith("/"))
			{
				text2 += "/";
			}
		}
		if (removeInvalidChars)
		{
			return string.Join(string.Empty, text2.Split(_invalidPathChars));
		}
		return text2;
	}

	public static string ValidateFile(string path, bool removeInvalidChars = true)
	{
		if (string.IsNullOrEmpty(path))
		{
			return path;
		}
		if (isURL(path))
		{
			return path;
		}
		bool flag = isWindowsPath(path);
		bool flag2 = isUNCPath(path);
		string text = ValidatePath(path, addEndDelimiter: false, removeInvalidChars);
		if (text.EndsWith("\\") || text.EndsWith("/"))
		{
			text = text.Substring(0, text.Length - 1);
		}
		string text2 = ((!(flag || flag2)) ? text.Substring(text.CTLastIndexOf("/") + 1) : text.Substring(text.CTLastIndexOf("\\") + 1));
		string text3 = string.Empty;
		if (removeInvalidChars)
		{
			text3 = string.Join(string.Empty, text2.Split(_invalidFilenameChars));
			if ((flag || flag2) && text3.EndsWith("."))
			{
				text3 = text3.Substring(0, text2.Length - 1);
			}
		}
		return text.Substring(0, text.Length - text2.Length) + text3;
	}

	public static bool HasPathInvalidChars(string path, bool ignoreNullOrEmpty = true)
	{
		if (string.IsNullOrEmpty(path))
		{
			return !ignoreNullOrEmpty;
		}
		return path.IndexOfAny(_invalidPathChars) >= 0;
	}

	public static bool HasFileInvalidChars(string file, bool ignoreNullOrEmpty = true)
	{
		if (string.IsNullOrEmpty(file))
		{
			return !ignoreNullOrEmpty;
		}
		if (GetFileName(file, removeInvalidChars: false).IndexOfAny(_invalidFilenameChars) < 0)
		{
			return file.EndsWith(".");
		}
		return true;
	}

	public static string[] GetFilesForName(string path, bool isRecursive = false, params string[] filenames)
	{
		if (BaseHelper.isWebPlatform && !BaseHelper.isEditor)
		{
			UnityEngine.Debug.LogWarning("'GetFilesForName' is not supported for the current platform!");
		}
		else if (!string.IsNullOrEmpty(path))
		{
			if (BaseHelper.isWSABasedPlatform && !BaseHelper.isEditor)
			{
				UnityEngine.Debug.LogWarning("'GetFilesForName' under UWP (WSA) is supported in combination with 'File Browser PRO'. For more, please see: https://assetstore.unity.com/packages/slug/98713?aid=1011lNGT");
			}
			else
			{
				try
				{
					string path2 = ValidatePath(path);
					if (filenames == null || filenames.Length == 0 || filenames.Any((string extension) => extension.Equals("*") || extension.Equals("*.*")))
					{
						return Directory.EnumerateFiles(path2, "*", isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToArray();
					}
					List<string> list = new List<string>();
					foreach (string text in filenames)
					{
						list.AddRange(Directory.EnumerateFiles(path2, text.StartsWith("*.") ? text : ("*" + text + "*"), isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
					}
					return list.OrderBy((string q) => q).ToArray();
				}
				catch (Exception arg)
				{
					UnityEngine.Debug.LogWarning($"Could not scan the path '{path}' for files: {arg}");
				}
			}
		}
		return Array.Empty<string>();
	}

	public static string[] GetFiles(string path, bool isRecursive = false, params string[] extensions)
	{
		if (BaseHelper.isWSABasedPlatform && !BaseHelper.isEditor)
		{
			UnityEngine.Debug.LogWarning("'GetFiles' under UWP (WSA) is supported in combination with 'File Browser PRO'. For more, please see: https://assetstore.unity.com/packages/slug/98713?aid=1011lNGT");
			return Array.Empty<string>();
		}
		if (extensions != null && extensions.Length != 0)
		{
			string[] array = new string[extensions.Length];
			for (int i = 0; i < extensions.Length; i++)
			{
				array[i] = "*." + extensions[i];
			}
			return GetFilesForName(path, isRecursive, array);
		}
		return GetFilesForName(path, isRecursive, extensions);
	}

	public static string[] GetDirectories(string path, bool isRecursive = false)
	{
		if (BaseHelper.isWebPlatform && !BaseHelper.isEditor)
		{
			UnityEngine.Debug.LogWarning("'GetDirectories' is not supported for the current platform!");
		}
		else if (BaseHelper.isWSABasedPlatform && !BaseHelper.isEditor)
		{
			UnityEngine.Debug.LogWarning("'GetDirectories' under UWP (WSA) is supported in combination with 'File Browser PRO'. For more, please see: https://assetstore.unity.com/packages/slug/98713?aid=1011lNGT");
		}
		else if (!string.IsNullOrEmpty(path))
		{
			try
			{
				return Directory.EnumerateDirectories(ValidatePath(path), "*", isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToArray();
			}
			catch (Exception arg)
			{
				UnityEngine.Debug.LogWarning($"Could not scan the path '{path}' for directories: {arg}");
			}
		}
		return Array.Empty<string>();
	}

	public static string[] GetDrives()
	{
		if (BaseHelper.isWebPlatform && !BaseHelper.isEditor)
		{
			UnityEngine.Debug.LogWarning("'GetDrives' is not supported for the current platform!");
		}
		else if (BaseHelper.isWSABasedPlatform && !BaseHelper.isEditor)
		{
			UnityEngine.Debug.LogWarning("'GetDrives' under UWP (WSA) is supported in combination with 'File Browser PRO'. For more, please see: https://assetstore.unity.com/packages/slug/98713?aid=1011lNGT");
		}
		else
		{
			try
			{
				return Directory.GetLogicalDrives();
			}
			catch (Exception arg)
			{
				UnityEngine.Debug.LogWarning($"Could not scan for drives: {arg}");
			}
		}
		return Array.Empty<string>();
	}

	public static bool CopyDirectory(string sourceDir, string destDir, bool move = false)
	{
		if (string.IsNullOrEmpty(destDir))
		{
			return false;
		}
		bool result = false;
		if ((BaseHelper.isWSABasedPlatform || BaseHelper.isWebPlatform) && !BaseHelper.isEditor)
		{
			UnityEngine.Debug.LogWarning("'CopyDirectory' is not supported for the current platform!");
		}
		else
		{
			try
			{
				string text = ValidatePath(sourceDir);
				string text2 = ValidatePath(destDir);
				if (!ExistsDirectory(text))
				{
					UnityEngine.Debug.LogWarning("Source directory does not exists: " + text);
				}
				else
				{
					if (ExistsDirectory(text2))
					{
						if (BaseConstants.DEV_DEBUG)
						{
							UnityEngine.Debug.LogWarning("Overwrite destination directory: " + text2);
						}
						DeleteDirectory(text2);
					}
					if (move)
					{
						Directory.Move(text, text2);
					}
					else
					{
						copyAll(new DirectoryInfo(text), new DirectoryInfo(text2));
					}
					result = true;
				}
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogError(string.Format("Could not {0} directory '{1}' to '{2}': {3}", move ? "move" : "copy", sourceDir, destDir, ex));
				throw;
			}
		}
		return result;
	}

	public static bool CopyFile(string sourceFile, string destFile, bool move = false)
	{
		if (string.IsNullOrEmpty(destFile))
		{
			return false;
		}
		bool result = false;
		if ((BaseHelper.isWSABasedPlatform || BaseHelper.isWebPlatform) && !BaseHelper.isEditor)
		{
			UnityEngine.Debug.LogWarning("'CopyFile' is not supported for the current platform!");
		}
		else
		{
			try
			{
				if (!ExistsFile(sourceFile))
				{
					UnityEngine.Debug.LogWarning("Source file does not exists: " + sourceFile);
				}
				else
				{
					string text = ValidateFile(destFile);
					CreateDirectory(GetDirectoryName(text));
					if (ExistsFile(text))
					{
						if (BaseConstants.DEV_DEBUG)
						{
							UnityEngine.Debug.LogWarning("Overwrite destination file: " + text);
						}
						DeleteFile(text);
					}
					if (move)
					{
						File.Move(sourceFile, text);
					}
					else
					{
						File.Copy(sourceFile, text);
					}
					result = true;
				}
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogError(string.Format("Could not {0} file '{1}' to '{2}': {3}", move ? "move" : "copy", sourceFile, destFile, ex));
				throw;
			}
		}
		return result;
	}

	public static bool MoveDirectory(string sourceDir, string destDir)
	{
		return CopyDirectory(sourceDir, destDir, move: true);
	}

	public static bool MoveFile(string sourceFile, string destFile)
	{
		return CopyFile(sourceFile, destFile, move: true);
	}

	public static string RenameDirectory(string path, string newName)
	{
		if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(newName))
		{
			return path;
		}
		if ((BaseHelper.isWSABasedPlatform || BaseHelper.isWebPlatform) && !BaseHelper.isEditor)
		{
			UnityEngine.Debug.LogWarning("'RenameDirectory' is not supported for the current platform!");
			return null;
		}
		try
		{
			string text = Path.Combine(new DirectoryInfo(path).Parent.FullName, newName);
			Directory.Move(path, text);
			return text;
		}
		catch (Exception arg)
		{
			UnityEngine.Debug.LogError($"Could not rename directory '{path}' to '{newName}': {arg}");
			throw;
		}
	}

	public static string RenameFile(string path, string newName)
	{
		if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(newName))
		{
			return path;
		}
		if ((BaseHelper.isWSABasedPlatform || BaseHelper.isWebPlatform) && !BaseHelper.isEditor)
		{
			UnityEngine.Debug.LogWarning("'RenameFile' is not supported for the current platform!");
			return null;
		}
		try
		{
			string text = Path.Combine(Path.GetDirectoryName(path), newName);
			File.Move(path, text);
			return text;
		}
		catch (Exception arg)
		{
			UnityEngine.Debug.LogError($"Could not rename file '{path}' to '{newName}': {arg}");
			throw;
		}
	}

	public static bool DeleteFile(string file)
	{
		bool result = false;
		if ((BaseHelper.isWSABasedPlatform || BaseHelper.isWebPlatform) && !BaseHelper.isEditor)
		{
			UnityEngine.Debug.LogWarning("'DeleteFile' is not supported for the current platform!");
		}
		else
		{
			try
			{
				if (!ExistsFile(file))
				{
					UnityEngine.Debug.LogWarning("File does not exists: " + file);
				}
				else
				{
					File.Delete(file);
					result = true;
				}
			}
			catch (Exception arg)
			{
				UnityEngine.Debug.LogError($"Could not delete file '{file}': {arg}");
				throw;
			}
		}
		return result;
	}

	public static bool DeleteDirectory(string dir)
	{
		bool result = false;
		if ((BaseHelper.isWSABasedPlatform || BaseHelper.isWebPlatform) && !BaseHelper.isEditor)
		{
			UnityEngine.Debug.LogWarning("'DeleteDirectory' is not supported for the current platform!");
		}
		else
		{
			try
			{
				if (!ExistsDirectory(dir))
				{
					UnityEngine.Debug.LogWarning("Source directory does not exists: " + dir);
				}
				else
				{
					Directory.Delete(dir, recursive: true);
					result = true;
				}
			}
			catch (Exception arg)
			{
				UnityEngine.Debug.LogError($"Could not delete directory '{dir}': {arg}");
				throw;
			}
		}
		return result;
	}

	public static bool ExistsFile(string file)
	{
		if ((BaseHelper.isWSABasedPlatform || BaseHelper.isWebPlatform) && !BaseHelper.isEditor)
		{
			UnityEngine.Debug.LogWarning("'ExistsFile' is not supported for the current platform!");
			return false;
		}
		return File.Exists(file);
	}

	public static bool ExistsDirectory(string path)
	{
		if ((BaseHelper.isWSABasedPlatform || BaseHelper.isWebPlatform) && !BaseHelper.isEditor)
		{
			UnityEngine.Debug.LogWarning("'ExistsPath' is not supported for the current platform!");
			return false;
		}
		return Directory.Exists(path);
	}

	public static string CreateDirectory(string path, string folderName)
	{
		if (string.IsNullOrEmpty(folderName))
		{
			return path;
		}
		if ((BaseHelper.isWSABasedPlatform || BaseHelper.isWebPlatform) && !BaseHelper.isEditor)
		{
			UnityEngine.Debug.LogWarning("'CreateDirectory' is not supported for the current platform!");
		}
		else
		{
			try
			{
				if (ExistsDirectory(path))
				{
					string text = Path.Combine(path, folderName);
					Directory.CreateDirectory(text);
					return text;
				}
				UnityEngine.Debug.LogWarning("Path directory does not exists: " + path);
			}
			catch (Exception arg)
			{
				UnityEngine.Debug.LogError($"Could not create directory at '{path}' with name '{folderName}': {arg}");
				throw;
			}
		}
		return null;
	}

	public static bool CreateDirectory(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return false;
		}
		bool result = false;
		if ((BaseHelper.isWSABasedPlatform || BaseHelper.isWebPlatform) && !BaseHelper.isEditor)
		{
			UnityEngine.Debug.LogWarning("'CreateDirectory' is not supported for the current platform!");
		}
		else
		{
			try
			{
				Directory.CreateDirectory(path);
				result = true;
			}
			catch (Exception arg)
			{
				UnityEngine.Debug.LogError($"Could not create directory '{path}': {arg}");
				throw;
			}
		}
		return result;
	}

	public static string CreateFile(string path, string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			return path;
		}
		if ((BaseHelper.isWSABasedPlatform || BaseHelper.isWebPlatform) && !BaseHelper.isEditor)
		{
			UnityEngine.Debug.LogWarning("'CreateFile' is not supported for the current platform!");
		}
		else
		{
			try
			{
				if (ExistsDirectory(path))
				{
					string text = Path.Combine(path, fileName);
					using (File.Create(text))
					{
					}
					return text;
				}
				UnityEngine.Debug.LogWarning("Path directory does not exists: " + path);
			}
			catch (Exception arg)
			{
				UnityEngine.Debug.LogError($"Could not create file at '{path}' with name '{fileName}': {arg}");
				throw;
			}
		}
		return null;
	}

	public static bool CreateFile(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return false;
		}
		bool result = false;
		if ((BaseHelper.isWSABasedPlatform || BaseHelper.isWebPlatform) && !BaseHelper.isEditor)
		{
			UnityEngine.Debug.LogWarning("'CreateFile' is not supported for the current platform!");
		}
		else
		{
			try
			{
				using (File.Create(path))
				{
				}
				result = true;
			}
			catch (Exception arg)
			{
				UnityEngine.Debug.LogError($"Could not create file '{path}': {arg}");
				throw;
			}
		}
		return result;
	}

	public static bool isDirectory(string path, bool checkForExtensions = true)
	{
		if (string.IsNullOrEmpty(path))
		{
			return false;
		}
		if ((BaseHelper.isWSABasedPlatform || BaseHelper.isWebPlatform) && !BaseHelper.isEditor)
		{
			UnityEngine.Debug.LogWarning("'isDirectory' is not supported for the current platform!");
		}
		else
		{
			if (ExistsDirectory(path))
			{
				return true;
			}
			if (ExistsFile(path))
			{
				return false;
			}
			if (checkForExtensions)
			{
				string extension = GetExtension(path);
				if (extension != null)
				{
					return extension.Length <= 1;
				}
				return true;
			}
		}
		return false;
	}

	public static bool isFile(string path, bool checkForExtensions = true)
	{
		if (!string.IsNullOrEmpty(path))
		{
			return !isDirectory(path, checkForExtensions);
		}
		return false;
	}

	public static string GetFileName(string path, bool removeInvalidChars = true)
	{
		string text = ValidatePath(path, addEndDelimiter: false, removeInvalidChars);
		string text2 = text;
		if (!string.IsNullOrEmpty(text))
		{
			try
			{
				text2 = Path.GetFileName(text);
			}
			catch (Exception)
			{
			}
			if (string.IsNullOrEmpty(text2) || text2 == text)
			{
				text2 = ((!isWindowsPath(text)) ? text.Substring(text.CTLastIndexOf("/") + 1) : text.Substring(text.CTLastIndexOf("\\") + 1));
			}
			if (removeInvalidChars)
			{
				text2 = string.Join(string.Empty, text2.Split(_invalidFilenameChars));
			}
		}
		return text2;
	}

	public static string GetCurrentDirectoryName(string path)
	{
		string text = ValidatePath(path, addEndDelimiter: false);
		string text2 = text;
		if (!string.IsNullOrEmpty(text))
		{
			try
			{
				text2 = new DirectoryInfo(text).Name;
			}
			catch (Exception)
			{
			}
			if (string.IsNullOrEmpty(text2) || text2 == text)
			{
				text2 = string.Join(value: ((!isWindowsPath(text)) ? text.Substring(text.CTLastIndexOf("/") + 1) : text.Substring(text.CTLastIndexOf("\\") + 1)).Split(_invalidPathChars), separator: string.Empty);
			}
		}
		return text2;
	}

	public static string GetDirectoryName(string path)
	{
		string text = path;
		if (!string.IsNullOrEmpty(path))
		{
			bool flag = isUNCPath(path);
			bool flag2 = isWindowsPath(path);
			try
			{
				bool flag3 = (!string.IsNullOrEmpty(path) && path.EndsWith("\\")) || path.EndsWith("/");
				string text2 = ValidatePath(flag2 ? ("/" + path) : path, !BaseConstants.REGEX_FILE.IsMatch(path));
				if (!isURL(text2))
				{
					text = Path.GetDirectoryName(text2);
				}
				if (string.IsNullOrEmpty(text))
				{
					text = text2;
				}
				if (flag2)
				{
					text = text.Substring(1);
				}
				text = ((flag2 || flag) ? text.Replace('/', '\\') : text.Replace('\\', '/'));
				bool flag4 = (!string.IsNullOrEmpty(text) && text.EndsWith("\\")) || text.EndsWith("/");
				if (flag3 && !flag4)
				{
					text = ValidatePath(text);
				}
				else if (!flag3 && flag4)
				{
					text = text.Substring(0, text.Length - 1);
				}
			}
			catch (Exception arg)
			{
				UnityEngine.Debug.LogError($"Could not get directory name for '{path}': {arg}");
				throw;
			}
		}
		return text;
	}

	public static long GetFilesize(string path)
	{
		if (ExistsFile(path))
		{
			if ((!BaseHelper.isWSABasedPlatform && !BaseHelper.isWebPlatform) || BaseHelper.isEditor)
			{
				try
				{
					return new FileInfo(path).Length;
				}
				catch (Exception arg)
				{
					UnityEngine.Debug.LogError($"Could not get file size for '{path}': {arg}");
					throw;
				}
			}
			UnityEngine.Debug.LogWarning("'GetFilesize' is not supported for the current platform!");
		}
		UnityEngine.Debug.LogWarning("Path is not a file: " + path);
		return -1L;
	}

	public static string GetExtension(string path)
	{
		if (isFile(path, checkForExtensions: false))
		{
			if ((BaseHelper.isWSABasedPlatform || BaseHelper.isWebPlatform) && !BaseHelper.isEditor)
			{
				return path.Substring(path.LastIndexOf("."));
			}
			try
			{
				string extension = Path.GetExtension(path);
				return (!string.IsNullOrEmpty(extension)) ? extension.Substring(1) : null;
			}
			catch (Exception arg)
			{
				UnityEngine.Debug.LogError($"Could not get extension for file '{path}': {arg}");
				throw;
			}
		}
		UnityEngine.Debug.LogWarning("File does not exists: " + path);
		return null;
	}

	public static DateTime GetLastModifiedDate(string path)
	{
		if (ExistsFile(path))
		{
			if ((!BaseHelper.isWSABasedPlatform && !BaseHelper.isWebPlatform) || BaseHelper.isEditor)
			{
				try
				{
					return new FileInfo(path).LastWriteTime;
				}
				catch (Exception arg)
				{
					UnityEngine.Debug.LogError($"Could not get last modify date for '{path}': {arg}");
					throw;
				}
			}
			UnityEngine.Debug.LogWarning("'GetLastModifiedDate' is not supported for the current platform!");
		}
		UnityEngine.Debug.LogWarning("File does not exists: " + path);
		return DateTime.MinValue;
	}

	public static string ReadAllText(string sourceFile, Encoding encoding = null)
	{
		if ((BaseHelper.isWSABasedPlatform || BaseHelper.isWebPlatform) && !BaseHelper.isEditor)
		{
			UnityEngine.Debug.LogWarning("'ReadAllText' is not supported for the current platform!");
		}
		else
		{
			try
			{
				if (ExistsFile(sourceFile))
				{
					return File.ReadAllText(sourceFile, encoding ?? Encoding.UTF8);
				}
				UnityEngine.Debug.LogWarning("Source file does not exists: " + sourceFile);
			}
			catch (Exception arg)
			{
				UnityEngine.Debug.LogError($"Could not read file '{sourceFile}': {arg}");
				throw;
			}
		}
		return null;
	}

	public static string[] ReadAllLines(string sourceFile, Encoding encoding = null)
	{
		if ((BaseHelper.isWSABasedPlatform || BaseHelper.isWebPlatform) && !BaseHelper.isEditor)
		{
			UnityEngine.Debug.LogWarning("'ReadAllLines' is not supported for the current platform!");
		}
		else
		{
			try
			{
				if (ExistsFile(sourceFile))
				{
					return File.ReadAllLines(sourceFile, encoding ?? Encoding.UTF8);
				}
				UnityEngine.Debug.LogWarning("Source file does not exists: " + sourceFile);
			}
			catch (Exception arg)
			{
				UnityEngine.Debug.LogError($"Could not read file '{sourceFile}': {arg}");
				throw;
			}
		}
		return null;
	}

	public static byte[] ReadAllBytes(string sourceFile)
	{
		if ((BaseHelper.isWSABasedPlatform || BaseHelper.isWebPlatform) && !BaseHelper.isEditor)
		{
			UnityEngine.Debug.LogWarning("'ReadAllBytes' is not supported for the current platform!");
		}
		else
		{
			try
			{
				if (ExistsFile(sourceFile))
				{
					return File.ReadAllBytes(sourceFile);
				}
				UnityEngine.Debug.LogWarning("Source file does not exists: " + sourceFile);
			}
			catch (Exception arg)
			{
				UnityEngine.Debug.LogError($"Could not read file '{sourceFile}': {arg}");
				throw;
			}
		}
		return null;
	}

	public static bool WriteAllText(string destFile, string text, Encoding encoding = null)
	{
		if (string.IsNullOrEmpty(destFile))
		{
			return false;
		}
		bool result = false;
		if ((BaseHelper.isWSABasedPlatform || BaseHelper.isWebPlatform) && !BaseHelper.isEditor)
		{
			UnityEngine.Debug.LogWarning("'WriteAllText' is not supported for the current platform!");
		}
		else
		{
			try
			{
				File.WriteAllText(destFile, text, encoding ?? Encoding.UTF8);
				result = true;
			}
			catch (Exception arg)
			{
				UnityEngine.Debug.LogError($"Could not write file '{destFile}': {arg}");
				throw;
			}
		}
		return result;
	}

	public static bool WriteAllLines(string destFile, string[] lines, Encoding encoding = null)
	{
		if (string.IsNullOrEmpty(destFile) || lines == null)
		{
			return false;
		}
		bool result = false;
		if ((BaseHelper.isWSABasedPlatform || BaseHelper.isWebPlatform) && !BaseHelper.isEditor)
		{
			UnityEngine.Debug.LogWarning("'WriteAllLines' is not supported for the current platform!");
		}
		else
		{
			try
			{
				File.WriteAllLines(destFile, lines, encoding ?? Encoding.UTF8);
				result = true;
			}
			catch (Exception arg)
			{
				UnityEngine.Debug.LogError($"Could not write file '{destFile}': {arg}");
				throw;
			}
		}
		return result;
	}

	public static bool WriteAllBytes(string destFile, byte[] data)
	{
		if (string.IsNullOrEmpty(destFile) || data == null)
		{
			return false;
		}
		bool result = false;
		if ((BaseHelper.isWSABasedPlatform || BaseHelper.isWebPlatform) && !BaseHelper.isEditor)
		{
			UnityEngine.Debug.LogWarning("'WriteAllBytes' is not supported for the current platform!");
		}
		else
		{
			try
			{
				File.WriteAllBytes(destFile, data);
				result = true;
			}
			catch (Exception arg)
			{
				UnityEngine.Debug.LogError($"Could not write file '{destFile}': {arg}");
				throw;
			}
		}
		return result;
	}

	public static bool ShowPath(string path)
	{
		return ShowFile(path);
	}

	public static bool ShowFile(string file)
	{
		bool result = false;
		if (BaseHelper.isStandalonePlatform || BaseHelper.isEditor)
		{
			string text = ((string.IsNullOrEmpty(file) || file.Equals(".")) ? "." : (((!BaseHelper.isWindowsPlatform && !BaseHelper.isWindowsEditor) || file.Length >= 4) ? ValidatePath(GetDirectoryName(file)) : file));
			try
			{
				if (ExistsDirectory(text))
				{
					using Process process = new Process();
					process.StartInfo.Arguments = "\"" + text + "\"";
					if (BaseHelper.isWindowsPlatform || BaseHelper.isWindowsEditor)
					{
						process.StartInfo.FileName = "explorer.exe";
						process.StartInfo.CreateNoWindow = true;
					}
					else if (BaseHelper.isMacOSPlatform || BaseHelper.isMacOSEditor)
					{
						process.StartInfo.FileName = "open";
					}
					else
					{
						process.StartInfo.FileName = "xdg-open";
					}
					process.Start();
					result = true;
				}
				else
				{
					UnityEngine.Debug.LogWarning("Path to file doesn't exist: " + text);
				}
			}
			catch (Exception arg)
			{
				UnityEngine.Debug.LogError($"Could not show file location '{file}': {arg}");
			}
		}
		else
		{
			UnityEngine.Debug.LogWarning("'ShowFileLocation' is not supported on the current platform!");
		}
		return result;
	}

	public static bool OpenFile(string file)
	{
		bool result = false;
		if (BaseHelper.isStandalonePlatform || BaseHelper.isEditor)
		{
			try
			{
				if (ExistsFile(file))
				{
					using (Process process = new Process())
					{
						if (BaseHelper.isMacOSPlatform || BaseHelper.isMacOSEditor)
						{
							process.StartInfo.FileName = "open";
							process.StartInfo.WorkingDirectory = GetDirectoryName(file) + "/";
							process.StartInfo.Arguments = "-t \"" + GetFileName(file) + "\"";
						}
						else if (BaseHelper.isLinuxPlatform || BaseHelper.isLinuxEditor)
						{
							process.StartInfo.FileName = "xdg-open";
							process.StartInfo.WorkingDirectory = GetDirectoryName(file) + "/";
							process.StartInfo.Arguments = GetFileName(file);
						}
						else
						{
							process.StartInfo.FileName = file;
						}
						process.Start();
					}
					result = true;
				}
				else
				{
					UnityEngine.Debug.LogWarning("File doesn't exist: " + file);
				}
			}
			catch (Exception arg)
			{
				UnityEngine.Debug.LogError($"Could not open file '{file}': {arg}");
			}
		}
		else
		{
			UnityEngine.Debug.LogWarning("'OpenFile' is not supported on the current platform!");
		}
		return result;
	}

	[Obsolete("Please use 'HasPathInvalidChars' instead.")]
	public static bool PathHasInvalidChars(string path)
	{
		return HasPathInvalidChars(path);
	}

	[Obsolete("Please use 'HasFileInvalidChars' instead.")]
	public static bool FileHasInvalidChars(string file)
	{
		return HasFileInvalidChars(file);
	}

	[Obsolete("Please use 'CopyDirectory' instead.")]
	public static bool CopyPath(string sourceDir, string destDir, bool move = false)
	{
		return CopyDirectory(sourceDir, destDir, move);
	}

	[Obsolete("Please use 'MoveDirectory' instead.")]
	public static bool MovePath(string sourceDir, string destDir)
	{
		return MoveDirectory(sourceDir, destDir);
	}

	private static void copyAll(DirectoryInfo source, DirectoryInfo target)
	{
		CreateDirectory(target.FullName);
		FileInfo[] files = source.GetFiles();
		foreach (FileInfo fileInfo in files)
		{
			fileInfo.CopyTo(Path.Combine(target.FullName, fileInfo.Name), overwrite: true);
		}
		DirectoryInfo[] directories = source.GetDirectories();
		foreach (DirectoryInfo directoryInfo in directories)
		{
			DirectoryInfo target2 = target.CreateSubdirectory(directoryInfo.Name);
			copyAll(directoryInfo, target2);
		}
	}
}
