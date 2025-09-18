using System;
using System.Collections.Generic;
using BrainCloud.Common;
using BrainCloud.Entity;
using BrainCloud.Internal;
using UnityEngine;

namespace BrainCloud;

public class BrainCloudClient
{
	private string s_defaultServerURL = "https://sharedprod.braincloudservers.com/dispatcherv2";

	private string _appVersion = "";

	private Platform _platform;

	private string _languageCode;

	private string _countryCode;

	private bool _initialized;

	private bool _loggingEnabled;

	private object _loggingMutex = new object();

	private LogCallback _logDelegate;

	private BCEntityFactory _entityFactory;

	private BrainCloudComms _comms;

	private RTTComms _rttComms;

	private RelayComms _rsComms;

	private BrainCloudEntity _entityService;

	private BrainCloudGlobalEntity _globalEntityService;

	private BrainCloudGlobalApp _globalAppService;

	private BrainCloudPresence _presenceService;

	private BrainCloudVirtualCurrency _virtualCurrencyService;

	private BrainCloudAppStore _appStore;

	private BrainCloudPlayerStatistics _playerStatisticsService;

	private BrainCloudGlobalStatistics _globalStatisticsService;

	private BrainCloudIdentity _identityService;

	private BrainCloudItemCatalog _itemCatalogService;

	private BrainCloudUserItems _userItemsService;

	private BrainCloudScript _scriptService;

	private BrainCloudMatchMaking _matchMakingService;

	private BrainCloudOneWayMatch _oneWayMatchService;

	private BrainCloudPlaybackStream _playbackStreamService;

	private BrainCloudGamification _gamificationService;

	private BrainCloudPlayerState _playerStateService;

	private BrainCloudFriend _friendService;

	private BrainCloudEvent _eventService;

	private BrainCloudSocialLeaderboard _leaderboardService;

	private BrainCloudAsyncMatch _asyncMatchService;

	private BrainCloudTime _timeService;

	private BrainCloudTournament _tournamentService;

	private BrainCloudGlobalFile _globalFileService;

	private BrainCloudCustomEntity _customEntityService;

	private BrainCloudAuthentication _authenticationService;

	private BrainCloudPushNotification _pushNotificationService;

	private BrainCloudPlayerStatisticsEvent _playerStatisticsEventService;

	private BrainCloudS3Handling _s3HandlingService;

	private BrainCloudRedemptionCode _redemptionCodeService;

	private BrainCloudDataStream _dataStreamService;

	private BrainCloudProfanity _profanityService;

	private BrainCloudFile _fileService;

	private BrainCloudGroup _groupService;

	private BrainCloudMail _mailService;

	private BrainCloudMessaging _messagingService;

	private BrainCloudLobby _lobbyService;

	private BrainCloudChat _chatService;

	private BrainCloudRTT _rttService;

	private BrainCloudRelay _rsService;

	public bool Authenticated => _comms.Authenticated;

	public bool Initialized => _initialized;

	public string SessionID
	{
		get
		{
			if (_comms == null)
			{
				return "";
			}
			return _comms.SessionID;
		}
	}

	public string AppId
	{
		get
		{
			if (_comms == null)
			{
				return "";
			}
			return _comms.AppId;
		}
	}

	public string ProfileId
	{
		get
		{
			if (AuthenticationService == null)
			{
				return "";
			}
			return AuthenticationService.ProfileId;
		}
	}

	public string RTTConnectionID
	{
		get
		{
			if (_rttComms == null)
			{
				return "";
			}
			return _rttComms.RTTConnectionID;
		}
	}

	public string RTTEventServer
	{
		get
		{
			if (_rttComms == null)
			{
				return "";
			}
			return _rttComms.RTTEventServer;
		}
	}

	public string AppVersion => _appVersion;

	public string BrainCloudClientVersion => Version.GetVersion();

	public Platform ReleasePlatform => _platform;

	public string LanguageCode
	{
		get
		{
			if (!string.IsNullOrEmpty(_languageCode))
			{
				return _languageCode;
			}
			return Util.GetIsoCodeForCurrentLanguage();
		}
		set
		{
			_languageCode = value;
		}
	}

	public string CountryCode
	{
		get
		{
			if (!string.IsNullOrEmpty(_countryCode))
			{
				return _countryCode;
			}
			return Util.GetCurrentCountryCode();
		}
		set
		{
			_countryCode = value;
		}
	}

	public BrainCloudWrapper Wrapper { get; set; }

	internal BrainCloudComms Comms => _comms;

	public BrainCloudEntity EntityService => _entityService;

	public BCEntityFactory EntityFactory => _entityFactory;

	public BrainCloudGlobalEntity GlobalEntityService => _globalEntityService;

	public BrainCloudGlobalApp GlobalAppService => _globalAppService;

	public BrainCloudPresence PresenceService => _presenceService;

	public BrainCloudVirtualCurrency VirtualCurrencyService => _virtualCurrencyService;

	public BrainCloudAppStore AppStoreService => _appStore;

	public BrainCloudPlayerStatistics PlayerStatisticsService => _playerStatisticsService;

	public BrainCloudGlobalStatistics GlobalStatisticsService => _globalStatisticsService;

	public BrainCloudIdentity IdentityService => _identityService;

	public BrainCloudItemCatalog ItemCatalogService => _itemCatalogService;

	public BrainCloudUserItems UserItemsService => _userItemsService;

	public BrainCloudScript ScriptService => _scriptService;

	public BrainCloudMatchMaking MatchMakingService => _matchMakingService;

	public BrainCloudOneWayMatch OneWayMatchService => _oneWayMatchService;

	public BrainCloudPlaybackStream PlaybackStreamService => _playbackStreamService;

	public BrainCloudGamification GamificationService => _gamificationService;

	public BrainCloudPlayerState PlayerStateService => _playerStateService;

	public BrainCloudFriend FriendService => _friendService;

	public BrainCloudEvent EventService => _eventService;

	public BrainCloudSocialLeaderboard SocialLeaderboardService => _leaderboardService;

	public BrainCloudSocialLeaderboard LeaderboardService => _leaderboardService;

	public BrainCloudAsyncMatch AsyncMatchService => _asyncMatchService;

	public BrainCloudTime TimeService => _timeService;

	public BrainCloudTournament TournamentService => _tournamentService;

	public BrainCloudGlobalFile GlobalFileService => _globalFileService;

	public BrainCloudCustomEntity CustomEntityService => _customEntityService;

	public BrainCloudAuthentication AuthenticationService => _authenticationService;

	public BrainCloudPushNotification PushNotificationService => _pushNotificationService;

	public BrainCloudPlayerStatisticsEvent PlayerStatisticsEventService => _playerStatisticsEventService;

	public BrainCloudS3Handling S3HandlingService => _s3HandlingService;

	public BrainCloudRedemptionCode RedemptionCodeService => _redemptionCodeService;

	public BrainCloudDataStream DataStreamService => _dataStreamService;

	public BrainCloudProfanity ProfanityService => _profanityService;

	public BrainCloudFile FileService => _fileService;

	public BrainCloudGroup GroupService => _groupService;

	public BrainCloudMail MailService => _mailService;

	public BrainCloudRTT RTTService => _rttService;

	public BrainCloudLobby LobbyService => _lobbyService;

	public BrainCloudChat ChatService => _chatService;

	public BrainCloudMessaging MessagingService => _messagingService;

	public BrainCloudRelay RelayService => _rsService;

	public BrainCloudRedemptionCode GetRedemptionCodeService => _redemptionCodeService;

	public BrainCloudDataStream GetDataStreamService => _dataStreamService;

	public BrainCloudProfanity GetProfanityService => _profanityService;

	public BrainCloudFile GetFileService => _fileService;

	public BrainCloudGroup GetGroupService => _groupService;

	public bool LoggingEnabled => _loggingEnabled;

	public static ServerCallback CreateServerCallback(SuccessCallback success, FailureCallback failure, object cbObject = null)
	{
		ServerCallback result = null;
		if (success != null || failure != null)
		{
			result = new ServerCallback(success, failure, cbObject);
		}
		return result;
	}

	public BrainCloudClient()
	{
		init();
	}

	public BrainCloudClient(BrainCloudWrapper in_wrapper)
	{
		Wrapper = in_wrapper;
		init();
	}

	private void init()
	{
		_comms = new BrainCloudComms(this);
		_rttComms = new RTTComms(this);
		_rsComms = new RelayComms(this);
		_entityService = new BrainCloudEntity(this);
		_entityFactory = new BCEntityFactory(_entityService);
		_globalEntityService = new BrainCloudGlobalEntity(this);
		_globalAppService = new BrainCloudGlobalApp(this);
		_presenceService = new BrainCloudPresence(this);
		_virtualCurrencyService = new BrainCloudVirtualCurrency(this);
		_appStore = new BrainCloudAppStore(this);
		_playerStatisticsService = new BrainCloudPlayerStatistics(this);
		_globalStatisticsService = new BrainCloudGlobalStatistics(this);
		_identityService = new BrainCloudIdentity(this);
		_itemCatalogService = new BrainCloudItemCatalog(this);
		_userItemsService = new BrainCloudUserItems(this);
		_scriptService = new BrainCloudScript(this);
		_matchMakingService = new BrainCloudMatchMaking(this);
		_oneWayMatchService = new BrainCloudOneWayMatch(this);
		_playbackStreamService = new BrainCloudPlaybackStream(this);
		_gamificationService = new BrainCloudGamification(this);
		_playerStateService = new BrainCloudPlayerState(this);
		_friendService = new BrainCloudFriend(this);
		_eventService = new BrainCloudEvent(this);
		_leaderboardService = new BrainCloudSocialLeaderboard(this);
		_asyncMatchService = new BrainCloudAsyncMatch(this);
		_timeService = new BrainCloudTime(this);
		_tournamentService = new BrainCloudTournament(this);
		_globalFileService = new BrainCloudGlobalFile(this);
		_customEntityService = new BrainCloudCustomEntity(this);
		_authenticationService = new BrainCloudAuthentication(this);
		_pushNotificationService = new BrainCloudPushNotification(this);
		_playerStatisticsEventService = new BrainCloudPlayerStatisticsEvent(this);
		_s3HandlingService = new BrainCloudS3Handling(this);
		_redemptionCodeService = new BrainCloudRedemptionCode(this);
		_dataStreamService = new BrainCloudDataStream(this);
		_profanityService = new BrainCloudProfanity(this);
		_fileService = new BrainCloudFile(this);
		_groupService = new BrainCloudGroup(this);
		_mailService = new BrainCloudMail(this);
		_messagingService = new BrainCloudMessaging(this);
		_lobbyService = new BrainCloudLobby(this);
		_chatService = new BrainCloudChat(this);
		_rttService = new BrainCloudRTT(_rttComms, this);
		_rsService = new BrainCloudRelay(_rsComms, this);
	}

	public void EnableCompressedRequests(bool isEnabled)
	{
		_comms.EnableCompression(isEnabled);
	}

	public void EnableCompressedResponses(bool isEnabled)
	{
		_authenticationService.CompressResponses = isEnabled;
	}

	public string GetAppId()
	{
		return AppId;
	}

	public string GetAppVersion()
	{
		return AppVersion;
	}

	public BrainCloudEntity GetEntityService()
	{
		return EntityService;
	}

	public BCEntityFactory GetEntityFactory()
	{
		return EntityFactory;
	}

	public BrainCloudGlobalApp GetGlobalAppService()
	{
		return GlobalAppService;
	}

	public BrainCloudGlobalEntity GetGlobalEntityService()
	{
		return GlobalEntityService;
	}

	public BrainCloudPresence GetPresenceService()
	{
		return PresenceService;
	}

	public BrainCloudPlayerStatistics GetPlayerStatisticsService()
	{
		return PlayerStatisticsService;
	}

	public BrainCloudGlobalStatistics GetGlobalStatisticsService()
	{
		return GlobalStatisticsService;
	}

	public BrainCloudIdentity GetIdentityService()
	{
		return IdentityService;
	}

	public BrainCloudItemCatalog GetItemCatalogService()
	{
		return ItemCatalogService;
	}

	public BrainCloudUserItems GetUserItemsService()
	{
		return UserItemsService;
	}

	public BrainCloudScript GetScriptService()
	{
		return ScriptService;
	}

	public BrainCloudMatchMaking GetMatchMakingService()
	{
		return MatchMakingService;
	}

	public BrainCloudOneWayMatch GetOneWayMatchService()
	{
		return OneWayMatchService;
	}

	public BrainCloudPlaybackStream GetPlaybackStreamService()
	{
		return PlaybackStreamService;
	}

	public BrainCloudGamification GetGamificationService()
	{
		return GamificationService;
	}

	public BrainCloudPlayerState GetPlayerStateService()
	{
		return _playerStateService;
	}

	public BrainCloudAsyncMatch GetAsyncMatchService()
	{
		return _asyncMatchService;
	}

	public BrainCloudFriend GetFriendService()
	{
		return _friendService;
	}

	public BrainCloudEvent GetEventService()
	{
		return _eventService;
	}

	public BrainCloudSocialLeaderboard GetSocialLeaderboardService()
	{
		return _leaderboardService;
	}

	public BrainCloudTime GetTimeService()
	{
		return _timeService;
	}

	public BrainCloudTournament GetTournamentService()
	{
		return _tournamentService;
	}

	public BrainCloudGlobalFile GetGlobalFileService()
	{
		return _globalFileService;
	}

	public BrainCloudCustomEntity GetCustomEntityService()
	{
		return _customEntityService;
	}

	public BrainCloudAuthentication GetAuthenticationService()
	{
		return _authenticationService;
	}

	public BrainCloudPushNotification GetPushNotificationService()
	{
		return _pushNotificationService;
	}

	public BrainCloudPlayerStatisticsEvent GetPlayerStatisticsEventService()
	{
		return _playerStatisticsEventService;
	}

	public BrainCloudS3Handling GetS3HandlingService()
	{
		return _s3HandlingService;
	}

	public string GetSessionId()
	{
		return SessionID;
	}

	public bool IsAuthenticated()
	{
		return Authenticated;
	}

	public long GetReceivedPacketId()
	{
		return _comms.GetReceivedPacketId();
	}

	public bool IsInitialized()
	{
		return Initialized;
	}

	public void Initialize(string secretKey, string appId, string appVersion)
	{
		Initialize(s_defaultServerURL, secretKey, appId, appVersion);
	}

	public void InitializeWithApps(string defaultAppId, Dictionary<string, string> appIdSecrectMap, string appVersion)
	{
		InitializeWithApps(s_defaultServerURL, defaultAppId, appIdSecrectMap, appVersion);
	}

	public void InitializeWithApps(string serverURL, string defaultAppId, Dictionary<string, string> appIdSecrectMap, string appVersion)
	{
		initializeHelper(serverURL, appIdSecrectMap[defaultAppId], defaultAppId, appVersion);
		_comms.InitializeWithApps(serverURL, defaultAppId, appIdSecrectMap);
		_initialized = true;
	}

	public void Initialize(string serverURL, string secretKey, string appId, string appVersion)
	{
		initializeHelper(serverURL, secretKey, appId, appVersion);
		_comms.Initialize(serverURL, appId, secretKey);
		_initialized = true;
	}

	public void InitializeIdentity(string profileId, string anonymousId)
	{
		AuthenticationService.Initialize(profileId, anonymousId);
	}

	public void ShutDown()
	{
		_comms.ShutDown();
	}

	public void RunCallbacks(eBrainCloudUpdateType in_updateType = eBrainCloudUpdateType.ALL)
	{
		Update(in_updateType);
	}

	public void Update(eBrainCloudUpdateType in_updateType = eBrainCloudUpdateType.ALL)
	{
		switch (in_updateType)
		{
		case eBrainCloudUpdateType.REST:
			if (_comms != null)
			{
				_comms.Update();
			}
			return;
		case eBrainCloudUpdateType.RTT:
			if (_rttComms != null)
			{
				_rttComms.Update();
			}
			return;
		case eBrainCloudUpdateType.RS:
			if (_rsComms != null)
			{
				_rsComms.Update();
			}
			return;
		case eBrainCloudUpdateType.PING:
			if (_lobbyService != null)
			{
				_lobbyService.Update();
			}
			return;
		}
		if (_rttComms != null)
		{
			_rttComms.Update();
		}
		if (_comms != null)
		{
			_comms.Update();
		}
		if (_rsComms != null)
		{
			_rsComms.Update();
		}
		if (_lobbyService != null)
		{
			_lobbyService.Update();
		}
	}

	public void RegisterEventCallback(EventCallback cb)
	{
		_comms.RegisterEventCallback(cb);
	}

	public void DeregisterEventCallback()
	{
		_comms.DeregisterEventCallback();
	}

	public void RegisterRewardCallback(RewardCallback cb)
	{
		_comms.RegisterRewardCallback(cb);
	}

	public void DeregisterRewardCallback()
	{
		_comms.DeregisterRewardCallback();
	}

	[Obsolete("This has been deprecated, use RegisterFileUploadCallback instead")]
	public void RegisterFileUploadCallbacks(FileUploadSuccessCallback success, FileUploadFailedCallback failure)
	{
		_comms.RegisterFileUploadCallbacks(success, failure);
	}

	[Obsolete("This has been deprecated, use DeregisterFileUploadCallback instead")]
	public void DeregisterFileUploadCallbacks()
	{
		_comms.DeregisterFileUploadCallbacks();
	}

	public void RegisterFileUploadCallback(FileUploadSuccessCallback success, FileUploadFailedCallback failure)
	{
		_comms.RegisterFileUploadCallbacks(success, failure);
	}

	public void DeregisterFileUploadCallback()
	{
		_comms.DeregisterFileUploadCallbacks();
	}

	public void RegisterGlobalErrorCallback(FailureCallback callback)
	{
		_comms.RegisterGlobalErrorCallback(callback);
	}

	public void DeregisterGlobalErrorCallback()
	{
		_comms.DeregisterGlobalErrorCallback();
	}

	public void RegisterNetworkErrorCallback(NetworkErrorCallback callback)
	{
		_comms.RegisterNetworkErrorCallback(callback);
	}

	public void DeregisterNetworkErrorCallback()
	{
		_comms.DeregisterNetworkErrorCallback();
	}

	public void EnableLogging(bool enable)
	{
		_loggingEnabled = enable;
	}

	public void RegisterLogDelegate(LogCallback logDelegate)
	{
		_logDelegate = logDelegate;
	}

	public string GetUrl()
	{
		return _comms.ServerURL;
	}

	public void ResetCommunication()
	{
		_comms.ResetCommunication();
		_rttComms.DisableRTT();
		_rsComms.Disconnect();
		Update();
		AuthenticationService.ClearSavedProfileID();
	}

	public void EnableCommunications(bool value)
	{
		_comms.EnableComms(value);
	}

	public void SetPacketTimeouts(List<int> timeouts)
	{
		_comms.PacketTimeouts = timeouts;
	}

	public void SetPacketTimeoutsToDefault()
	{
		_comms.SetPacketTimeoutsToDefault();
	}

	public List<int> GetPacketTimeouts()
	{
		return _comms.PacketTimeouts;
	}

	public void SetAuthenticationPacketTimeout(int timeoutSecs)
	{
		_comms.AuthenticationPacketTimeoutSecs = timeoutSecs;
	}

	public int GetAuthenticationPacketTimeout()
	{
		return _comms.AuthenticationPacketTimeoutSecs;
	}

	public void SetOldStyleStatusMessageErrorCallback(bool enabled)
	{
		_comms.OldStyleStatusResponseInErrorCallback = enabled;
	}

	public int GetUploadLowTransferRateTimeout()
	{
		return _comms.UploadLowTransferRateTimeout;
	}

	public void SetUploadLowTransferRateTimeout(int timeoutSecs)
	{
		_comms.UploadLowTransferRateTimeout = timeoutSecs;
	}

	public int GetUploadLowTransferRateThreshold()
	{
		return _comms.UploadLowTransferRateThreshold;
	}

	public void SetUploadLowTransferRateThreshold(int bytesPerSec)
	{
		_comms.UploadLowTransferRateThreshold = bytesPerSec;
	}

	public void EnableNetworkErrorMessageCaching(bool enabled)
	{
		_comms.EnableNetworkErrorMessageCaching(enabled);
	}

	public void RetryCachedMessages()
	{
		_comms.RetryCachedMessages();
	}

	public void FlushCachedMessages(bool sendApiErrorCallbacks)
	{
		_comms.FlushCachedMessages(sendApiErrorCallbacks);
	}

	public void InsertEndOfMessageBundleMarker()
	{
		_comms.InsertEndOfMessageBundleMarker();
	}

	public void OverrideCountryCode(string countryCode)
	{
		_countryCode = countryCode;
	}

	public void OverrideLanguageCode(string languageCode)
	{
		_languageCode = languageCode;
	}

	public void SendHeartbeat(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		ServerCall call = new ServerCall(ServiceName.HeartBeat, ServiceOperation.Read, null, new ServerCallback(success, failure, cbObject));
		_comms.AddToQueue(call);
	}

	internal void Log(string log)
	{
		if (!_loggingEnabled)
		{
			return;
		}
		string text = DateTime.Now.ToString("HH:mm:ss.fff") + " #BCC " + ((log.Length < 14000) ? log : (log.Substring(0, 14000) + " << (LOG TRUNCATED)"));
		lock (_loggingMutex)
		{
			if (_logDelegate != null)
			{
				_logDelegate(text);
			}
			else
			{
				Debug.Log(text);
			}
		}
	}

	internal void SendRequest(ServerCall serviceMessage)
	{
		_comms.AddToQueue(serviceMessage);
	}

	private void initializeHelper(string serverURL, string secretKey, string appId, string appVersion)
	{
		string text = null;
		if (string.IsNullOrEmpty(serverURL))
		{
			text = "serverURL was null or empty";
		}
		else if (string.IsNullOrEmpty(secretKey))
		{
			text = "secretKey was null or empty";
		}
		else if (string.IsNullOrEmpty(appId))
		{
			text = "appId was null or empty";
		}
		else if (string.IsNullOrEmpty(appVersion))
		{
			text = "appVerson was null or empty";
		}
		if (text != null)
		{
			Debug.LogError("ERROR | Failed to initialize brainCloud - " + text);
			return;
		}
		Platform windows = Platform.Windows;
		windows = Platform.FromUnityRuntime();
		_appVersion = appVersion;
		_platform = windows;
		if (Util.GetCurrentCountryCode() == string.Empty)
		{
			Util.SetCurrentCountryCode(RegionLocale.UsersCountryLocale);
		}
	}
}
