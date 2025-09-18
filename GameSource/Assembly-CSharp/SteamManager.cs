using System;
using System.Text;
using AOT;
using Steamworks;
using UnityEngine;

[DisallowMultipleComponent]
public class SteamManager : MonoBehaviour
{
	protected static bool s_EverInitialized;

	private static bool destroyed;

	private static bool destroyedByEditor;

	protected static SteamManager s_instance;

	protected bool m_bInitialized;

	protected SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;

	private Callback<GetAuthSessionTicketResponse_t> m_GethAuthSessionTicketHook;

	public static bool EverInitialized => s_EverInitialized;

	public static bool Destroyed => destroyed;

	public static bool DestroyedByEditor => destroyedByEditor;

	public static SteamManager Instance
	{
		get
		{
			if (s_instance == null)
			{
				return new GameObject("SteamManager").AddComponent<SteamManager>();
			}
			return s_instance;
		}
	}

	public static bool Initialized => Instance.m_bInitialized;

	public event Action<bool> OnSessionTicketValidAndReady;

	[MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
	protected static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
	{
		Debug.LogWarning(pchDebugText);
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void InitOnPlayMode()
	{
		s_EverInitialized = false;
		s_instance = null;
	}

	protected virtual void Awake()
	{
		if (s_instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		s_instance = this;
		if (s_EverInitialized)
		{
			throw new Exception("Tried to Initialize the SteamAPI twice in one session!");
		}
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		if (!Packsize.Test())
		{
			Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", this);
		}
		if (!DllCheck.Test())
		{
			Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", this);
		}
		try
		{
			if (SteamAPI.RestartAppIfNecessary(AppId_t.Invalid))
			{
				Debug.Log("[Steamworks.NET] Shutting down because RestartAppIfNecessary returned true. Steam will restart the application.");
				Application.Quit();
				return;
			}
		}
		catch (DllNotFoundException ex)
		{
			Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + ex, this);
			Application.Quit();
			return;
		}
		m_bInitialized = SteamAPI.Init();
		if (!m_bInitialized)
		{
			Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", this);
			return;
		}
		m_GethAuthSessionTicketHook = Callback<GetAuthSessionTicketResponse_t>.Create(OnGetAuthSessionTicketResponse);
		s_EverInitialized = true;
		destroyed = false;
	}

	public string GetSteamAuthSessionTicket()
	{
		byte[] array = new byte[1024];
		if (SteamUser.GetAuthSessionTicket(array, 1024, out var pcbTicket) != HAuthTicket.Invalid)
		{
			string text = "";
			for (int i = 0; i < pcbTicket; i++)
			{
				text += $"{array[i]:X2}";
			}
			return text;
		}
		Debug.LogError("Couldn't get Steam Session Auth Ticket - returned ticket was invalid.");
		return null;
	}

	private void OnGetAuthSessionTicketResponse(GetAuthSessionTicketResponse_t param)
	{
		this.OnSessionTicketValidAndReady?.Invoke(param.m_eResult == EResult.k_EResultOK);
	}

	protected virtual void OnEnable()
	{
		if (s_instance == null)
		{
			s_instance = this;
		}
		if (m_bInitialized && m_SteamAPIWarningMessageHook == null)
		{
			m_SteamAPIWarningMessageHook = SteamAPIDebugTextHook;
			SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
		}
	}

	protected virtual void OnDestroy()
	{
		if (!(s_instance != this))
		{
			s_instance = null;
			if (m_bInitialized)
			{
				SteamAPI.Shutdown();
				destroyedByEditor = true;
			}
		}
	}

	public static void PostDestroy()
	{
		if (Matchmaker.IsInstantiated)
		{
			Matchmaker.Instance.LeaveLobby();
		}
		bool bInitialized = s_instance.m_bInitialized;
		s_instance = null;
		destroyed = true;
		if (bInitialized)
		{
			SteamAPI.Shutdown();
		}
	}

	protected virtual void Update()
	{
		if (m_bInitialized)
		{
			SteamAPI.RunCallbacks();
		}
	}
}
