using System;
using System.Collections;
using System.Collections.Generic;
using BCGSComponents;
using GameSparks.Api.Responses;
using GameSparks.Core;
using I2.Loc;
using UnityEngine;
using UnityEngine.Events;

public abstract class GameSparksManager : MonoBehaviour
{
	public enum AuthSource
	{
		NONE,
		DEVICE,
		STEAM,
		XBOX,
		PSN,
		SWITCH,
		ORIGIN,
		CUSTOM
	}

	protected bool connecting;

	protected bool connected;

	protected bool retryConnect;

	protected bool allowAutoRetry = true;

	private bool waitingForGSAvailable;

	protected float autoRetryDelay;

	private float stalledTimer;

	private const float purgeTimeout = 20f;

	private List<GameSparksQuery> queries = new List<GameSparksQuery>();

	public bool ShowDebug;

	private static GameSparksManager instance;

	public bool MainUserBanned;

	public bool usingAlternateQAServer;

	public GameSparksSettings alternateGSSettings;

	private bool switchWasConnected;

	private int maxQueriesInWebRequest = 8;

	private HashSet<GameSparksQuery> queriesInWebRequest = new HashSet<GameSparksQuery>();

	protected Coroutine waitToGetAttributesCoroutine;

	public static GameSparksManager Instance => instance;

	public abstract bool Available { get; }

	public bool Connected => connected;

	public bool Connecting => connecting;

	public bool AllowAutoRetry => allowAutoRetry;

	public AuthSource AuthenticatedUsing { get; protected set; }

	public string MainUserDisplayName { get; protected set; }

	public string MainUserGSID { get; protected set; }

	public int MainUserPermissionLevel { get; protected set; }

	public ControllerMonitor.JoinedControllerEntry MainUserControllerEntry { get; protected set; }

	public bool MainUserIsAdmin => MainUserPermissionLevel >= 10;

	public abstract void Reconnect();

	public bool SecureWebRequestLock(GameSparksQuery query)
	{
		if (maxQueriesInWebRequest == 0)
		{
			return true;
		}
		if (queriesInWebRequest.Contains(query))
		{
			return true;
		}
		if (queriesInWebRequest.Count >= maxQueriesInWebRequest)
		{
			return false;
		}
		queriesInWebRequest.Add(query);
		return true;
	}

	public void ReleaseWebRequestLock(GameSparksQuery query)
	{
		queriesInWebRequest.Remove(query);
	}

	public abstract void Disconnect();

	public abstract void ResetBackend();

	private void Awake()
	{
		Debug.Log($"GSManager Awake. Instance: {instance}");
		if (instance == null)
		{
			instance = this;
			AuthenticatedUsing = AuthSource.NONE;
			onAwake();
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	protected abstract void onAwake();

	private void Start()
	{
		ConnectNow();
	}

	public void ConnectNow(UnityAction<bool> callback = null)
	{
		MainUserBanned = false;
		ConnectWithSteam();
		if (waitToGetAttributesCoroutine != null)
		{
			StopCoroutine(waitToGetAttributesCoroutine);
		}
		waitToGetAttributesCoroutine = StartCoroutine(GetFrozenLobbyCode());
	}

	protected virtual IEnumerator GetFrozenLobbyCode()
	{
		yield break;
	}

	private void OnDestroy()
	{
		if (instance == this)
		{
			Debug.Log("[Net] Shutting down Backend Manager");
			cleanup();
		}
	}

	protected abstract void cleanup();

	private void Update()
	{
		onUpdate();
		if (connected)
		{
			stalledTimer = 0f;
			GameSparksQuery[] array = queries.ToArray();
			foreach (GameSparksQuery gameSparksQuery in array)
			{
				gameSparksQuery.Update();
				if (gameSparksQuery.IsDone)
				{
					queries.Remove(gameSparksQuery);
					queriesInWebRequest.Remove(gameSparksQuery);
				}
			}
			if (!Available)
			{
				connected = false;
				connecting = false;
				Debug.Log($"connecting = {connecting}");
			}
		}
		else if (allowAutoRetry)
		{
			if (queries.Count > 0 && !connecting)
			{
				if (autoRetryDelay > 0f)
				{
					autoRetryDelay -= Time.unscaledDeltaTime;
				}
				else
				{
					autoRetryDelay = 0f;
					retryConnect = true;
				}
			}
			if (queries.Count > 0)
			{
				stalledTimer += Time.unscaledDeltaTime;
			}
			else
			{
				stalledTimer = 0f;
			}
		}
		else if (queries.Count > 0)
		{
			purgeAllQueries();
		}
		if (stalledTimer > 20f)
		{
			stalledTimer = 0f;
			Debug.Log("Backend queries were stalled for more than " + 20f + " seconds - purging all queries.");
			purgeAllQueries();
		}
		if (!connected && !connecting && retryConnect && allowAutoRetry)
		{
			ConnectNow();
		}
	}

	protected abstract void onUpdate();

	private IEnumerator timeoutLobby()
	{
		float time = 0f;
		while (time < 4f)
		{
			time += Time.unscaledDeltaTime;
			yield return null;
		}
		if (!connected && LobbyManager.instance != null && !LobbyManager.instance.AllLocal && !Matchmaker.InTreehouse)
		{
			Debug.LogError("Backend lost while playing online. Going back to main menu.");
			LobbyManagerManager.AbortGameInProgressGracefully(LocalizationManager.GetTranslation("Network/XB1/LostConnection"));
		}
	}

	private void purgeAllQueries()
	{
		foreach (GameSparksQuery query in queries)
		{
			Debug.Log("Purging " + query.name + "...");
			query.ForcePurge();
			queriesInWebRequest.Remove(query);
		}
		queries.Clear();
	}

	private IEnumerator waitForLoggedInPlayer(Action callback)
	{
		while (!PlayerManager.GetInstance().FirstUserLoggedIn)
		{
			yield return null;
		}
		callback();
	}

	private IEnumerator waitForGSAvailable(Action callback)
	{
		waitingForGSAvailable = true;
		while (!Available && waitingForGSAvailable)
		{
			yield return null;
		}
		if (waitingForGSAvailable)
		{
			callback();
		}
		waitingForGSAvailable = false;
	}

	public void AuthenticateByDevice()
	{
		if (connected)
		{
			return;
		}
		if (!Available)
		{
			if (!waitingForGSAvailable)
			{
				StartCoroutine(waitForGSAvailable(AuthenticateByDevice));
			}
			return;
		}
		new DeviceAuthenticationRequest().Send(delegate(BCGSComponents.AuthenticationResponse response)
		{
			if (response.HasErrors)
			{
				Debug.LogError("Could not authenticate with device.");
			}
			else
			{
				connected = true;
				AuthenticatedUsing = AuthSource.DEVICE;
				MainUserDisplayName = response.DisplayName;
				MainUserGSID = response.UserId;
			}
			connecting = false;
			retryConnect = false;
		});
	}

	protected abstract IEnumerator SwitchToAlternateBackendService();

	protected abstract void sendAccountDetailsRequest(Action<GameSparks.Api.Responses.AccountDetailsResponse> response);

	protected abstract void sendAuthenticationRequest(string username, string password, GSRequestData scriptData, Action<GameSparks.Api.Responses.AuthenticationResponse> responseHandler, string className = ".AuthenticationRequest");

	protected abstract void sendRegistrationRequest(string username, string password, string displayName, Action<GameSparks.Api.Responses.RegistrationResponse> responseHandler);

	public void ConnectWithSteam()
	{
		autoRetryDelay = 0f;
		if (!connected && !connecting)
		{
			StartCoroutine(doSteamConnection());
		}
	}

	private IEnumerator doSteamConnection()
	{
		connecting = true;
		while (!Available)
		{
			yield return null;
		}
		bool flag = false;
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			Debug.Log("ARG " + i + ": " + commandLineArgs[i]);
			if (commandLineArgs[i] == "-GameSparksFQADebug")
			{
				flag = true;
			}
		}
		if (!usingAlternateQAServer && (GameSettings.GetInstance().UseGameSparksFQAServer || flag))
		{
			usingAlternateQAServer = true;
			yield return SwitchToAlternateBackendService();
			while (!Available)
			{
				yield return null;
			}
		}
		if (SteamManager.Initialized)
		{
			Debug.Log("Backend: attempting to connect with Steam");
			bool isTicketValid = false;
			bool readyToContinue = false;
			Action<bool> callback = delegate(bool ticketValid)
			{
				isTicketValid = ticketValid;
				readyToContinue = true;
			};
			SteamManager.Instance.OnSessionTicketValidAndReady += callback;
			string sessionTicket = SteamManager.Instance.GetSteamAuthSessionTicket();
			yield return new WaitUntil(() => readyToContinue);
			SteamManager.Instance.OnSessionTicketValidAndReady -= callback;
			if (sessionTicket != null && isTicketValid)
			{
				Debug.Log("Ticket is valid, proceeding with connection...");
				connected = false;
				connecting = true;
				retryConnect = true;
				sendSteamConnectRequest(sessionTicket, OnSteamConnectResponse);
			}
			else
			{
				Debug.LogError("Connection to backend aborted - couldn't get Steam Session Auth Ticket");
				autoRetryDelay = 2f;
				retryConnect = false;
			}
		}
		else
		{
			Debug.LogError("Connection to backend aborted - Steam Manager not initialized");
			autoRetryDelay = 2f;
			retryConnect = false;
		}
	}

	protected abstract void sendSteamConnectRequest(string sessionTicket, Action<GameSparks.Api.Responses.AuthenticationResponse> connectResponse);

	private void OnSteamConnectResponse(GameSparks.Api.Responses.AuthenticationResponse response)
	{
		if (!response.HasErrors)
		{
			Debug.Log("Successfully connected backend to Steam (" + response.DisplayName + ")");
			connected = true;
			autoRetryDelay = 0f;
			AuthenticatedUsing = AuthSource.STEAM;
			MainUserDisplayName = response.DisplayName;
			MainUserGSID = response.UserId;
			if (!ReadPermissionLevel(response.ScriptData))
			{
				sendAccountDetailsRequest(onAccountDetailsResponse);
			}
		}
		else
		{
			Debug.LogWarning("Couldn't connect backend to Steam: " + response.JSONString);
			autoRetryDelay = 2f;
		}
		retryConnect = false;
		connecting = false;
	}

	protected void onAccountDetailsResponse(GameSparks.Api.Responses.AccountDetailsResponse response)
	{
		ReadPermissionLevel(response.ScriptData);
	}

	protected bool ReadPermissionLevel(GSData scriptData)
	{
		int num = 0;
		if (scriptData != null)
		{
			num = GameSparksQuery.ParseValueToInt(scriptData, "permissionLevel");
		}
		MainUserPermissionLevel = num;
		if (num == 0)
		{
			Debug.Log("[Net] Backend Auth: Could not read permission level for main user.");
			return false;
		}
		return true;
	}

	public abstract void RetryReadingMainUserGSID(UnityAction<bool> OnResponse);

	public GameSparksQuery CreateQuery(bool debugOutput = false)
	{
		GameSparksQuery gameSparksQuery = createQuery(debugOutput);
		queries.Add(gameSparksQuery);
		return gameSparksQuery;
	}

	protected abstract GameSparksQuery createQuery(bool debugOuput);

	public void EnableRetry(float delay)
	{
		if (!retryConnect)
		{
			retryConnect = true;
			autoRetryDelay = delay;
		}
	}

	public void OnMainControllerChanged()
	{
	}

	private void LogOut()
	{
		StopAllCoroutines();
		Debug.Log("Force-purging all queries before logging out...");
		foreach (GameSparksQuery query in queries)
		{
			Debug.Log("Purging " + query.name + "...");
			query.ForcePurge();
			queriesInWebRequest.Remove(query);
		}
		queries.Clear();
		ResetBackend();
		connected = false;
		connecting = false;
		Debug.Log($"connecting = {connecting}");
		waitingForGSAvailable = false;
		stalledTimer = 0f;
	}

	public void InvalidateExistingQueries(GameSparksQuery.UniqueQueryTag uniqueTag)
	{
		int num = 0;
		foreach (GameSparksQuery query in queries)
		{
			if (query.uniqueTag == uniqueTag)
			{
				query.ForcePurge();
				queriesInWebRequest.Remove(query);
				num++;
			}
		}
		if (num > 0)
		{
			Debug.LogWarning(num + " existing quer" + ((num != 1) ? "ies were" : "y was") + " purged (Tag: " + uniqueTag.ToString() + ")");
		}
	}

	public void WakeUp()
	{
		CreateQuery().WakeUp();
	}

	public void CheckPlatformUserBan(LobbyPlayer.SocialPlatform platform, string platformID, Action<bool> OnResult)
	{
		GameSparksQuery query = CreateQuery();
		query.SendSimpleRequest("GetBanInfoFromExternalId", new Dictionary<string, object>
		{
			{
				"playerPlatform",
				UGCNameTag.GetGSPlatformStringFromPlatform(platform)
			},
			{ "playerPlatformId", platformID }
		}, returnScriptData: true);
		GameSparksQuery gameSparksQuery = query;
		gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate
		{
			object value;
			if (query.HasError)
			{
				Debug.LogError("Error while checking platform user ban: " + query.Error);
				OnResult(obj: false);
			}
			else if (query.ResultData.TryGetValue("scriptData", out value))
			{
				if (value is GSData data)
				{
					int num = GameSparksQuery.ParseValueToInt(data, "isBanned");
					Debug.Log("Found value for isBanned: " + num);
					OnResult(num != 0);
				}
				else
				{
					Debug.LogError("Could not cast ScriptData");
					OnResult(obj: false);
				}
			}
			else
			{
				Debug.LogError("No ScriptData was returned");
				OnResult(obj: false);
			}
		});
	}

	public void FindAndSetPlayerGSID(LobbyPlayer player, UnityAction<bool> OnResult)
	{
		if (!player.GSID.NullOrEmpty())
		{
			return;
		}
		Debug.Log("FindAndSetPlayerGSID: Finding GSID for " + player.playerName + "...");
		StartCoroutine(WaitForMainUserGSID(delegate
		{
			if (MainUserGSID.NullOrEmpty())
			{
				Debug.LogError("No main user GSID set...");
				OnResult(arg0: false);
			}
			else if (player != null)
			{
				Debug.Log("FindAndSetPlayerGSID: Using main user GSID for Player " + player.playerName);
				player.CallCmdSetGSID(MainUserGSID);
				OnResult(arg0: true);
			}
		}));
	}

	public IEnumerator WaitForMainUserGSID(UnityAction OnGetMainUserGSID)
	{
		float timeout = 10f;
		while (MainUserGSID.NullOrEmpty())
		{
			timeout -= Time.unscaledDeltaTime;
			if (timeout <= 0f)
			{
				Debug.LogError("Timeout while waiting for main user GSID, aborting...");
				yield break;
			}
			yield return null;
		}
		OnGetMainUserGSID();
	}
}
