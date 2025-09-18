using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;

public class LogCatcher : MonoBehaviour
{
	private class DebugLogsDTO
	{
		public string sessionId;

		public string currentLogs;

		public string platform;

		public string buildVersion;
	}

	private class DebugLogsResponseDTO
	{
		public string uploadCode;
	}

	private const string LOG_CATCHER_URL = "127.0.0.1:3000";

	private const int LOG_FILE_COUNT = 5;

	private static LogCatcher _instance;

	private const string LOG_FILE_TEMPLATE = "uch-log-file-{0}.log";

	private const string LOG_FILE = "uch-log-file-1.log";

	private string logFilesPath;

	private string contextId;

	private Regex regexp = new Regex("\n+");

	public static LogCatcher Instance => _instance;

	private void Awake()
	{
		_instance = this;
		contextId = Guid.NewGuid().ToString();
		logFilesPath = Application.persistentDataPath;
		logFilesPath += "/Logs";
		Debug.Log("Log catcher folder = " + logFilesPath + "/uch-log-file-{0}.log");
		try
		{
			OrganizeLogFiles();
			Application.logMessageReceivedThreaded += LogMessageReceived;
		}
		catch (Exception)
		{
			Debug.Log("Error organizing log files, won't log in log catcher.");
		}
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	private async UniTaskVoid OrganizeLogFiles()
	{
		if (!Directory.Exists(logFilesPath))
		{
			Directory.CreateDirectory(logFilesPath);
		}
		for (int num = 5; num >= 1; num--)
		{
			if (File.Exists(logFilesPath + "/" + $"uch-log-file-{num.ToString()}.log"))
			{
				File.Move(logFilesPath + "/" + $"uch-log-file-{num.ToString()}.log", logFilesPath + "/" + $"uch-log-file-{(num + 1).ToString()}.log");
			}
		}
		if (File.Exists(logFilesPath + "/" + $"uch-log-file-{6.ToString()}.log"))
		{
			File.Delete(logFilesPath + "/" + $"uch-log-file-{6.ToString()}.log");
		}
		string infoStamp = $"UTC Time offset : {DateTime.Now:zzz}\n\t\t\t\t\t\t  Game Version = {GameSettings.GetInstance().VersionNumber}";
		try
		{
			UnityWebRequest countryRequest = UnityWebRequest.Get("https://ifconfig.co/country");
			await countryRequest.SendWebRequest();
			infoStamp = infoStamp + "\n\t\t\t\t\t\t  Country = " + countryRequest.downloadHandler.text;
		}
		catch (Exception)
		{
		}
		infoStamp = infoStamp + "\t\t\t\t\t\t  User = " + SteamFriends.GetPersonaName();
		File.AppendAllText(logFilesPath + "/uch-log-file-1.log", $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss.fff}] {infoStamp}\n");
	}

	private void LogMessageReceived(string condition, string stacktrace, LogType type)
	{
		string text = $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss.fff}] {condition}\n";
		if (type != LogType.Log && type != LogType.Warning)
		{
			stacktrace = "\t" + stacktrace;
			text += regexp.Replace(stacktrace, "\n\t");
			text += "\n";
		}
		File.AppendAllText(logFilesPath + "/uch-log-file-1.log", text);
	}

	private void OnDestroy()
	{
		Application.logMessageReceivedThreaded -= LogMessageReceived;
	}

	public async UniTask<string> UploadLogFile()
	{
		long errorCode = 0L;
		for (int i = 0; i < 3; i++)
		{
			UnityWebRequest req = await GenerateUploadLogsRequest();
			await req.SendWebRequest();
			if (req.isHttpError || req.isNetworkError)
			{
				errorCode = req.responseCode;
				await UniTask.Delay(i * 1000);
				continue;
			}
			return JsonUtility.FromJson<DebugLogsResponseDTO>(req.downloadHandler.text).uploadCode;
		}
		throw new Exception($"Error uploading logs {errorCode}");
	}

	private async UniTask<UnityWebRequest> GenerateUploadLogsRequest()
	{
		await UniTask.SwitchToThreadPool();
		string logs = File.ReadAllText(logFilesPath + "/uch-log-file-{0}.log");
		await UniTask.SwitchToMainThread();
		string text = await CompressString(logs);
		Debug.Log($"Normal log size = {logs.Length} Compressed log size = {text.Length}");
		DebugLogsDTO obj = new DebugLogsDTO
		{
			sessionId = contextId,
			currentLogs = text,
			platform = GetPlatform(),
			buildVersion = GameSettings.GetInstance().VersionNumber
		};
		Debug.Log($"Before creating post request time : {Time.realtimeSinceStartup}");
		UnityWebRequest result = CreatePostRequest("http://127.0.0.1:3000/logs/upload-debug-logs", JsonUtility.ToJson(obj));
		Debug.Log($"After creating post request time : {Time.realtimeSinceStartup}");
		return result;
	}

	private async UniTask<string> CompressString(string text)
	{
		Debug.Log($"Time before compression : {Time.realtimeSinceStartup}");
		await UniTask.SwitchToThreadPool();
		byte[] bytes = Encoding.UTF8.GetBytes(text);
		MemoryStream memoryStream = new MemoryStream();
		using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true))
		{
			await gZipStream.WriteAsync(bytes, 0, bytes.Length);
		}
		memoryStream.Position = 0L;
		byte[] compressedData = new byte[memoryStream.Length];
		await memoryStream.ReadAsync(compressedData, 0, compressedData.Length);
		byte[] gZipBuffer = new byte[compressedData.Length];
		Buffer.BlockCopy(compressedData, 0, gZipBuffer, 0, compressedData.Length);
		await UniTask.SwitchToMainThread();
		Debug.Log($"Time after compression : {Time.realtimeSinceStartup}");
		return Convert.ToBase64String(gZipBuffer);
	}

	private UnityWebRequest CreatePostRequest(string aURL, string aData)
	{
		UnityWebRequest unityWebRequest = new UnityWebRequest(aURL, "POST");
		UploadHandler uploadHandler = GenerateUploadHandler(aData);
		if (uploadHandler != null)
		{
			unityWebRequest.uploadHandler = uploadHandler;
		}
		DownloadHandlerBuffer downloadHandlerBuffer = new DownloadHandlerBuffer();
		if (downloadHandlerBuffer != null)
		{
			unityWebRequest.downloadHandler = downloadHandlerBuffer;
		}
		unityWebRequest.SetRequestHeader("Content-Type", "application/json");
		return unityWebRequest;
	}

	private string GetPlatform()
	{
		return "steam";
	}

	private UploadHandler GenerateUploadHandler(string aData)
	{
		if (aData.Length <= 2)
		{
			return null;
		}
		return new UploadHandlerRaw(Encoding.UTF8.GetBytes(aData))
		{
			contentType = "application/json"
		};
	}
}
