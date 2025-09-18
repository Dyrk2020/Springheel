using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GameSparks.Core;
using UnityEngine;

namespace GameSparks.Platforms;

public abstract class PlatformBase : MonoBehaviour, IGSPlatform
{
	public GSInstance gsInstance;

	private List<Action> _actions = new List<Action>();

	private List<Action> _currentActions = new List<Action>();

	private bool _allowQuitting;

	private string m_authToken = "0";

	private string m_userId = "";

	public string DeviceOS
	{
		get
		{
			switch (Application.platform)
			{
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.OSXPlayer:
			case RuntimePlatform.WindowsPlayer:
			case RuntimePlatform.WindowsEditor:
				return "WINDOWS";
			case RuntimePlatform.IPhonePlayer:
				return "IOS";
			case RuntimePlatform.Android:
				return "ANDROID";
			case RuntimePlatform.LinuxPlayer:
				return "LINUX";
			case RuntimePlatform.WebGLPlayer:
				return "WEBGL";
			case RuntimePlatform.MetroPlayerX86:
			case RuntimePlatform.MetroPlayerX64:
			case RuntimePlatform.MetroPlayerARM:
				return "WSA";
			case RuntimePlatform.TizenPlayer:
				return "TIZEN";
			case RuntimePlatform.PS4:
				return "PS4";
			case RuntimePlatform.XboxOne:
				return "XBOXONE";
			case RuntimePlatform.WiiU:
				return "WIIU";
			case RuntimePlatform.tvOS:
				return "TVOS";
			default:
				return "UNKNOWN";
			}
		}
	}

	public string DeviceName { get; private set; }

	public string DeviceType { get; private set; }

	public GSData DeviceStats { get; private set; }

	public virtual string DeviceId { get; private set; }

	public string Platform { get; private set; }

	public bool ExtraDebug { get; private set; }

	public string ApiKey => GameSparksSettings.ApiKey;

	public string ApiSecret => GameSparksSettings.ApiSecret;

	public string ApiCredential => GameSparksSettings.Credential;

	public string ApiStage
	{
		get
		{
			if (!GameSparksSettings.PreviewBuild)
			{
				return "live";
			}
			return "preview";
		}
	}

	public string ApiDomain => null;

	public string PersistentDataPath { get; private set; }

	public string SDK => "Unity";

	public string AuthToken
	{
		get
		{
			return m_authToken;
		}
		set
		{
			m_authToken = value;
		}
	}

	public string UserId
	{
		get
		{
			return m_userId;
		}
		set
		{
			m_userId = value;
		}
	}

	public Action<Exception> ExceptionReporter { get; set; }

	protected virtual void Start()
	{
		DeviceName = SystemInfo.deviceName.ToString();
		DeviceType = SystemInfo.deviceType.ToString();
		if (Application.platform == RuntimePlatform.PS4 || Application.platform == RuntimePlatform.XboxOne || "n/a" == SystemInfo.deviceUniqueIdentifier)
		{
			if ("n/a" == SystemInfo.deviceUniqueIdentifier)
			{
				DeviceId = Guid.NewGuid().ToString();
			}
			else
			{
				DeviceId = SystemInfo.deviceUniqueIdentifier.ToString();
			}
		}
		else
		{
			DeviceId = SystemInfo.deviceUniqueIdentifier.ToString();
		}
		char[] separator = new char[8] { ' ', ',', '.', ':', '-', '_', '(', ')' };
		int processorCount = SystemInfo.processorCount;
		string text = "Unknown";
		string value = SystemInfo.deviceModel;
		string value2 = SystemInfo.systemMemorySize + " MB";
		string text2 = SystemInfo.operatingSystem;
		string value3 = SystemInfo.operatingSystem;
		string text3 = SystemInfo.processorType;
		string value4 = Screen.width + "x" + Screen.height;
		string version = GS.Version;
		string sDK = SDK;
		string unityVersion = Application.unityVersion;
		switch (DeviceOS)
		{
		case "MACOS":
		case "IOS":
		case "TVOS":
		{
			text = "Apple";
			string[] array = SystemInfo.operatingSystem.Split(separator);
			if (DeviceOS.Equals("MACOS"))
			{
				text2 = array[0] + " " + array[1] + " " + array[2];
				value3 = array[3] + "." + array[4] + "." + array[5];
			}
			else
			{
				text2 = array[0];
				value3 = array[1] + "." + array[2];
			}
			break;
		}
		case "WINDOWS":
		case "WSA":
		case "XBOXONE":
		{
			text = "Microsoft";
			if (DeviceOS.Equals("XBOXONE"))
			{
				value = "Xbox One";
				value2 = SystemInfo.systemMemorySize / 1000 + " MB";
				value3 = "Unknown";
			}
			else
			{
				value = "PC";
				string[] array = SystemInfo.operatingSystem.Split(separator, StringSplitOptions.RemoveEmptyEntries);
				text2 = array[0] + " " + array[1];
				value3 = array[2] + "." + array[3] + "." + array[4];
			}
			text3 = text3 + " " + SystemInfo.processorFrequency + "MHz";
			RegexOptions options = RegexOptions.None;
			text3 = new Regex("[ ]{2,}", options).Replace(text3, " ");
			break;
		}
		case "ANDROID":
		{
			string[] array = SystemInfo.deviceModel.Split(separator);
			text = array[0];
			value = SystemInfo.deviceModel.Replace(text, "").Substring(1);
			array = SystemInfo.operatingSystem.Split(separator);
			text2 = array[0] + " " + array[1];
			value3 = array[7];
			text3 = text3 + " " + SystemInfo.processorFrequency + "MHz";
			break;
		}
		case "WIIU":
			text = "Nintendo";
			value = "WiiU";
			break;
		case "SWITCH":
			text = "Nintendo";
			value = "Switch";
			value3 = "Unknown";
			break;
		case "PS4":
		{
			text = "Sony";
			value = "PS4";
			value2 = SystemInfo.systemMemorySize / 1000000 + " MB";
			string[] array = SystemInfo.operatingSystem.Split(separator);
			text2 = array[0];
			value3 = array[1] + "." + array[2] + "." + array[3];
			text3 = text3 + " " + SystemInfo.processorFrequency + "MHz";
			break;
		}
		case "TIZEN":
			text = "Tizen";
			break;
		case "WEBGL":
		{
			string[] array = SystemInfo.deviceModel.Split(separator);
			value = array[0];
			array = SystemInfo.operatingSystem.Split(separator);
			text2 = array[0];
			if (text2.Equals("Mac"))
			{
				text2 = text2 + " " + array[1] + " " + array[2];
				value3 = array[3] + "." + array[4] + "." + array[5];
			}
			else
			{
				value3 = array[1];
			}
			break;
		}
		}
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("manufacturer", text);
		dictionary.Add("model", value);
		dictionary.Add("memory", value2);
		dictionary.Add("os.name", text2);
		dictionary.Add("os.version", value3);
		dictionary.Add("cpu.cores", processorCount.ToString());
		dictionary.Add("cpu.vendor", text3);
		dictionary.Add("resolution", value4);
		dictionary.Add("gssdk", version);
		dictionary.Add("engine", sDK);
		dictionary.Add("engine.version", unityVersion);
		DeviceStats = new GSData(dictionary);
		Platform = Application.platform.ToString();
		GameSparksUnity component = GetComponent<GameSparksUnity>();
		if (component != null)
		{
			GameSparksSettings.SetInstance(component.settings);
		}
		ExtraDebug = GameSparksSettings.DebugBuild;
		PersistentDataPath = Application.persistentDataPath;
		if (gsInstance != null)
		{
			gsInstance.Initialise(this);
		}
		else
		{
			GS.Initialise(this);
		}
		UnityEngine.Object.DontDestroyOnLoad(this);
	}

	public void ExecuteOnMainThread(Action action)
	{
		lock (_actions)
		{
			_actions.Add(action);
		}
	}

	protected virtual void Update()
	{
		lock (_actions)
		{
			if (_actions.Count > 0)
			{
				_currentActions.AddRange(_actions);
				_actions.Clear();
			}
		}
		int count = _currentActions.Count;
		if (count <= 0)
		{
			return;
		}
		for (int i = 0; i < count; i++)
		{
			Action action = _currentActions[i];
			if (action == null)
			{
				continue;
			}
			try
			{
				action();
			}
			catch (Exception ex)
			{
				if (ExceptionReporter != null)
				{
					ExceptionReporter(ex);
				}
				else
				{
					Debug.Log(ex);
				}
			}
		}
		_currentActions.Clear();
	}

	protected virtual void OnApplicationPause(bool paused)
	{
		if (paused)
		{
			return;
		}
		try
		{
			if (gsInstance != null)
			{
				gsInstance.Reconnect();
			}
			else
			{
				GS.Reconnect();
			}
		}
		catch (Exception obj)
		{
			if (ExceptionReporter != null)
			{
				ExceptionReporter(obj);
			}
		}
	}

	protected virtual void OnApplicationQuit()
	{
		if (gsInstance != null)
		{
			gsInstance.ShutDown(delegate
			{
			});
		}
		else
		{
			GS.ShutDown();
		}
		StartCoroutine("DelayedQuit");
		if (!_allowQuitting)
		{
			Application.CancelQuit();
		}
	}

	private IEnumerator DelayedQuit()
	{
		yield return new WaitForSeconds(1f);
		while (GS.Available)
		{
			yield return new WaitForSeconds(0.1f);
		}
		_allowQuitting = true;
		Application.Quit();
	}

	public void DebugMsg(string message)
	{
		ExecuteOnMainThread(delegate
		{
			if (GameSparksSettings.DebugBuild)
			{
				if (message.Length < 1500)
				{
					Debug.Log("GS: " + message);
				}
				else
				{
					Debug.Log("GS: " + message.Substring(0, 1500) + "...");
				}
			}
		});
	}

	public abstract IGameSparksTimer GetTimer();

	public abstract string MakeHmac(string stringToHmac, string secret);

	public abstract IGameSparksWebSocket GetSocket(string url, Action<string> messageReceived, Action closed, Action opened, Action<string> error);
}
