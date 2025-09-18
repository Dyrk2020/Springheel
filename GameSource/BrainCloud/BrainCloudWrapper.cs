using System.Collections.Generic;
using BrainCloud;
using BrainCloud.Common;
using BrainCloud.Entity;
using BrainCloud.Internal;
using BrainCloud.JsonFx.Json;
using BrainCloud.Plugin;
using UnityEngine;

public class BrainCloudWrapper : MonoBehaviour
{
	private class WrapperData
	{
		public string ProfileId = "";

		public string AnonymousId = "";

		public string AuthenticationType = "";

		public static readonly string FileName = "BrainCloudWrapper.json";
	}

	public static string PREFS_PROFILE_ID = "brainCloud.profileId";

	public static string PREFS_ANONYMOUS_ID = "brainCloud.anonymousId";

	public static string PREFS_AUTHENTICATION_TYPE = "brainCloud.authenticationType";

	public static string GAMEOBJECT_BRAINCLOUD = "BrainCloudWrapper";

	public static string AUTHENTICATION_ANONYMOUS = "anonymous";

	private static BrainCloudWrapper _instance = null;

	private string _lastUrl = "";

	private string _lastSecretKey = "";

	private string _lastAppId = "";

	private string _lastAppVersion = "";

	private bool _alwaysAllowProfileSwitch = true;

	private WrapperData _wrapperData = new WrapperData();

	public BrainCloudClient Client { get; private set; }

	public bool AlwaysAllowProfileSwitch
	{
		get
		{
			return _alwaysAllowProfileSwitch;
		}
		set
		{
			_alwaysAllowProfileSwitch = value;
		}
	}

	public string WrapperName { get; set; }

	public BrainCloudEntity EntityService => Client.EntityService;

	public BCEntityFactory EntityFactory => Client.EntityFactory;

	public BrainCloudGlobalEntity GlobalEntityService => Client.GlobalEntityService;

	public BrainCloudGlobalApp GlobalAppService => Client.GlobalAppService;

	public BrainCloudVirtualCurrency VirtualCurrencyService => Client.VirtualCurrencyService;

	public BrainCloudAppStore AppStoreService => Client.AppStoreService;

	public BrainCloudPlayerStatistics PlayerStatisticsService => Client.PlayerStatisticsService;

	public BrainCloudGlobalStatistics GlobalStatisticsService => Client.GlobalStatisticsService;

	public BrainCloudIdentity IdentityService => Client.IdentityService;

	public BrainCloudItemCatalog ItemCatalogService => Client.ItemCatalogService;

	public BrainCloudUserItems UserItemsService => Client.UserItemsService;

	public BrainCloudScript ScriptService => Client.ScriptService;

	public BrainCloudMatchMaking MatchMakingService => Client.MatchMakingService;

	public BrainCloudOneWayMatch OneWayMatchService => Client.OneWayMatchService;

	public BrainCloudPlaybackStream PlaybackStreamService => Client.PlaybackStreamService;

	public BrainCloudPresence PresenceService => Client.PresenceService;

	public BrainCloudGamification GamificationService => Client.GamificationService;

	public BrainCloudPlayerState PlayerStateService => Client.PlayerStateService;

	public BrainCloudFriend FriendService => Client.FriendService;

	public BrainCloudEvent EventService => Client.EventService;

	public BrainCloudSocialLeaderboard SocialLeaderboardService => Client.SocialLeaderboardService;

	public BrainCloudSocialLeaderboard LeaderboardService => Client.LeaderboardService;

	public BrainCloudAsyncMatch AsyncMatchService => Client.AsyncMatchService;

	public BrainCloudTime TimeService => Client.TimeService;

	public BrainCloudTournament TournamentService => Client.TournamentService;

	public BrainCloudGlobalFile GlobalFileService => Client.GlobalFileService;

	public BrainCloudCustomEntity CustomEntityService => Client.CustomEntityService;

	public BrainCloudPushNotification PushNotificationService => Client.PushNotificationService;

	public BrainCloudPlayerStatisticsEvent PlayerStatisticsEventService => Client.PlayerStatisticsEventService;

	public BrainCloudS3Handling S3HandlingService => Client.S3HandlingService;

	public BrainCloudRedemptionCode RedemptionCodeService => Client.RedemptionCodeService;

	public BrainCloudDataStream DataStreamService => Client.DataStreamService;

	public BrainCloudProfanity ProfanityService => Client.ProfanityService;

	public BrainCloudFile FileService => Client.FileService;

	public BrainCloudGroup GroupService => Client.GroupService;

	public BrainCloudMail MailService => Client.MailService;

	public BrainCloudRTT RTTService => Client.RTTService;

	public BrainCloudLobby LobbyService => Client.LobbyService;

	public BrainCloudChat ChatService => Client.ChatService;

	public BrainCloudMessaging MessagingService => Client.MessagingService;

	public BrainCloudRelay RelayService => Client.RelayService;

	private void OnApplicationQuit()
	{
		RTTService.DisableRTT();
		RelayService.Disconnect();
		Client.Update();
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
	}

	public BrainCloudWrapper()
	{
		Client = new BrainCloudClient(this);
	}

	private BrainCloudWrapper(BrainCloudClient client)
	{
		Client = client;
		Client.Wrapper = this;
	}

	public BrainCloudWrapper(string wrapperName)
	{
		Client = new BrainCloudClient(this);
		WrapperName = wrapperName;
	}

	public void RunCallbacks()
	{
		if (Client != null)
		{
			Client.Update();
		}
	}

	public void Update()
	{
		RunCallbacks();
	}

	public void Init()
	{
		resetWrapper();
		Init(Interface.DispatcherURL, Interface.AppSecret, Interface.AppId, Interface.AppVersion);
		Client.EnableLogging(Interface.EnableLogging);
	}

	public void InitWithApps()
	{
		resetWrapper();
		InitWithApps(Interface.DispatcherURL, Interface.AppId, Interface.AppIdSecrets, Interface.AppVersion);
		Client.EnableLogging(Interface.EnableLogging);
	}

	public void Init(string url, string secretKey, string appId, string version)
	{
		resetWrapper();
		_lastUrl = url;
		_lastSecretKey = secretKey;
		_lastAppId = appId;
		_lastAppVersion = version;
		Client.Initialize(url, secretKey, appId, version);
		LoadData();
	}

	public void InitWithApps(string url, string defaultAppId, Dictionary<string, string> appIdSecretMap, string version)
	{
		resetWrapper();
		_lastUrl = url;
		_lastSecretKey = appIdSecretMap[defaultAppId];
		_lastAppId = defaultAppId;
		_lastAppVersion = version;
		Client.InitializeWithApps(url, defaultAppId, appIdSecretMap, version);
		LoadData();
	}

	public void resetWrapper(bool resetWrapperName = false)
	{
		_wrapperData = new WrapperData();
		Client.ResetCommunication();
		Client.Wrapper = null;
		Client = null;
		Client = new BrainCloudClient(this);
		Client.Wrapper = this;
		if (resetWrapperName)
		{
			WrapperName = "";
		}
	}

	public void SetAlwaysAllowProfileSwitch(bool enabled)
	{
		AlwaysAllowProfileSwitch = enabled;
	}

	public void AuthenticateAnonymous(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		WrapperAuthCallbackObject cbObject2 = MakeWrapperAuthCallback(success, failure, cbObject, isAnonymousAuth: true);
		Client.AuthenticationService.AuthenticateAnonymous(forceCreate: true, AuthSuccessCallback, AuthFailureCallback, cbObject2);
	}

	public void AuthenticateHandoff(string handoffId, string securityToken, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		WrapperAuthCallbackObject cbObject2 = MakeWrapperAuthCallback(success, failure, cbObject, isAnonymousAuth: true);
		Client.AuthenticationService.AuthenticateHandoff(handoffId, securityToken, AuthSuccessCallback, AuthFailureCallback, cbObject2);
	}

	public void AuthenticateSettopHandoff(string handoffCode, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		WrapperAuthCallbackObject cbObject2 = MakeWrapperAuthCallback(success, failure, cbObject, isAnonymousAuth: true);
		Client.AuthenticationService.AuthenticateSettopHandoff(handoffCode, AuthSuccessCallback, AuthFailureCallback, cbObject2);
	}

	public void AuthenticateEmailPassword(string email, string password, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		WrapperAuthCallbackObject cbObject2 = MakeWrapperAuthCallback(success, failure, cbObject);
		Client.AuthenticationService.AuthenticateEmailPassword(email, password, forceCreate, AuthSuccessCallback, AuthFailureCallback, cbObject2);
	}

	public void AuthenticateExternal(string userid, string token, string externalAuthName, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		WrapperAuthCallbackObject cbObject2 = MakeWrapperAuthCallback(success, failure, cbObject);
		Client.AuthenticationService.AuthenticateExternal(userid, token, externalAuthName, forceCreate, AuthSuccessCallback, AuthFailureCallback, cbObject2);
	}

	public void AuthenticateFacebook(string fbUserId, string fbAuthToken, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		WrapperAuthCallbackObject cbObject2 = MakeWrapperAuthCallback(success, failure, cbObject);
		Client.AuthenticationService.AuthenticateFacebook(fbUserId, fbAuthToken, forceCreate, AuthSuccessCallback, AuthFailureCallback, cbObject2);
	}

	public void AuthenticateFacebookLimited(string fbLimitedUserId, string fbAuthToken, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		WrapperAuthCallbackObject cbObject2 = MakeWrapperAuthCallback(success, failure, cbObject);
		Client.AuthenticationService.AuthenticateFacebookLimited(fbLimitedUserId, fbAuthToken, forceCreate, AuthSuccessCallback, AuthFailureCallback, cbObject2);
	}

	public void AuthenticateOculus(string oculusUserId, string oculusNonce, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		WrapperAuthCallbackObject cbObject2 = MakeWrapperAuthCallback(success, failure, cbObject);
		Client.AuthenticationService.AuthenticateOculus(oculusUserId, oculusNonce, forceCreate, AuthSuccessCallback, AuthFailureCallback, cbObject2);
	}

	public void AuthenticatePlaystationNetwork(string accountId, string authToken, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		WrapperAuthCallbackObject cbObject2 = MakeWrapperAuthCallback(success, failure, cbObject);
		Client.AuthenticationService.AuthenticatePlaystationNetwork(accountId, authToken, forceCreate, AuthSuccessCallback, AuthFailureCallback, cbObject2);
	}

	public void AuthenticateGameCenter(string gameCenterId, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		WrapperAuthCallbackObject cbObject2 = MakeWrapperAuthCallback(success, failure, cbObject);
		Client.AuthenticationService.AuthenticateGameCenter(gameCenterId, forceCreate, AuthSuccessCallback, AuthFailureCallback, cbObject2);
	}

	public void AuthenticateApple(string appleUserId, string identityToken, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		WrapperAuthCallbackObject cbObject2 = MakeWrapperAuthCallback(success, failure, cbObject);
		Client.AuthenticationService.AuthenticateApple(appleUserId, identityToken, forceCreate, AuthSuccessCallback, AuthFailureCallback, cbObject2);
	}

	public void AuthenticateGoogle(string googleUserId, string serverAuthCode, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		WrapperAuthCallbackObject cbObject2 = MakeWrapperAuthCallback(success, failure, cbObject);
		Client.AuthenticationService.AuthenticateGoogle(googleUserId, serverAuthCode, forceCreate, AuthSuccessCallback, AuthFailureCallback, cbObject2);
	}

	public void AuthenticateGoogleOpenId(string googleUserAccountEmail, string IdToken, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		WrapperAuthCallbackObject cbObject2 = MakeWrapperAuthCallback(success, failure, cbObject);
		Client.AuthenticationService.AuthenticateGoogleOpenId(googleUserAccountEmail, IdToken, forceCreate, AuthSuccessCallback, AuthFailureCallback, cbObject2);
	}

	public void AuthenticateSteam(string userid, string sessionticket, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		WrapperAuthCallbackObject cbObject2 = MakeWrapperAuthCallback(success, failure, cbObject);
		Client.AuthenticationService.AuthenticateSteam(userid, sessionticket, forceCreate, AuthSuccessCallback, AuthFailureCallback, cbObject2);
	}

	public void AuthenticateTwitter(string userid, string token, string secret, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		WrapperAuthCallbackObject cbObject2 = MakeWrapperAuthCallback(success, failure, cbObject);
		Client.AuthenticationService.AuthenticateTwitter(userid, token, secret, forceCreate, AuthSuccessCallback, AuthFailureCallback, cbObject2);
	}

	public void AuthenticateUniversal(string username, string password, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		WrapperAuthCallbackObject cbObject2 = MakeWrapperAuthCallback(success, failure, cbObject);
		Client.AuthenticationService.AuthenticateUniversal(username, password, forceCreate, AuthSuccessCallback, AuthFailureCallback, cbObject2);
	}

	public void AuthenticateAdvanced(AuthenticationType authenticationType, AuthenticationIds ids, bool forceCreate, Dictionary<string, object> extraJson, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		bool flag = authenticationType == AuthenticationType.Anonymous;
		WrapperAuthCallbackObject cbObject2 = MakeWrapperAuthCallback(success, failure, cbObject, flag);
		ids.externalId = (flag ? GetStoredAnonymousId() : ids.externalId);
		ids.authenticationToken = (flag ? "" : ids.authenticationToken);
		Client.AuthenticationService.AuthenticateAdvanced(authenticationType, ids, forceCreate, extraJson, AuthSuccessCallback, AuthFailureCallback, cbObject2);
	}

	public void AuthenticateUltra(string ultraUsername, string ultraIdToken, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		WrapperAuthCallbackObject cbObject2 = MakeWrapperAuthCallback(success, failure, cbObject);
		Client.AuthenticationService.AuthenticateUltra(ultraUsername, ultraIdToken, forceCreate, AuthSuccessCallback, AuthFailureCallback, cbObject2);
	}

	public void AuthenticateNintendo(string accountId, string authToken, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		WrapperAuthCallbackObject cbObject2 = MakeWrapperAuthCallback(success, failure, cbObject);
		Client.AuthenticationService.AuthenticateNintendo(accountId, authToken, forceCreate, AuthSuccessCallback, AuthFailureCallback, cbObject2);
	}

	public virtual void SmartSwitchAuthenticateEmail(string email, string password, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		SuccessCallback authenticateCallback = delegate
		{
			AuthenticateEmailPassword(email, password, forceCreate, success, failure, cbObject);
		};
		SmartSwitchAuthentication(authenticateCallback, failure);
	}

	public virtual void SmartSwitchAuthenticateExternal(string userid, string token, string externalAuthName, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		SuccessCallback authenticateCallback = delegate
		{
			AuthenticateExternal(userid, token, externalAuthName, forceCreate, success, failure, cbObject);
		};
		SmartSwitchAuthentication(authenticateCallback, failure);
	}

	public virtual void SmartSwitchAuthenticateFacebook(string fbUserId, string fbAuthToken, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		SuccessCallback authenticateCallback = delegate
		{
			AuthenticateFacebook(fbUserId, fbAuthToken, forceCreate, success, failure, cbObject);
		};
		SmartSwitchAuthentication(authenticateCallback, failure);
	}

	public virtual void SmartSwitchAuthenticateFacebookLimited(string fbLimitedUserId, string fbAuthToken, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		SuccessCallback authenticateCallback = delegate
		{
			AuthenticateFacebookLimited(fbLimitedUserId, fbAuthToken, forceCreate, success, failure, cbObject);
		};
		SmartSwitchAuthentication(authenticateCallback, failure);
	}

	public virtual void SmartSwitchAuthenticateOculus(string oculusUserId, string oculusNonce, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		SuccessCallback authenticateCallback = delegate
		{
			AuthenticateOculus(oculusUserId, oculusNonce, forceCreate, success, failure, cbObject);
		};
		SmartSwitchAuthentication(authenticateCallback, failure);
	}

	public virtual void SmartSwitchAuthenticatePlaystationNetwork(string psnAccountId, string psnAuthToken, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		SuccessCallback authenticateCallback = delegate
		{
			AuthenticatePlaystationNetwork(psnAccountId, psnAuthToken, forceCreate, success, failure, cbObject);
		};
		SmartSwitchAuthentication(authenticateCallback, failure);
	}

	public virtual void SmartSwitchAuthenticateApple(string appleUserId, string appleAuthToken, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		SuccessCallback authenticateCallback = delegate
		{
			AuthenticateApple(appleUserId, appleAuthToken, forceCreate, success, failure, cbObject);
		};
		SmartSwitchAuthentication(authenticateCallback, failure);
	}

	public virtual void SmartSwitchAuthenticateGameCenter(string gameCenterId, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		SuccessCallback authenticateCallback = delegate
		{
			AuthenticateGameCenter(gameCenterId, forceCreate, success, failure, cbObject);
		};
		SmartSwitchAuthentication(authenticateCallback, failure);
	}

	public virtual void SmartSwitchAuthenticateGoogle(string userid, string token, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		SuccessCallback authenticateCallback = delegate
		{
			AuthenticateGoogle(userid, token, forceCreate, success, failure, cbObject);
		};
		SmartSwitchAuthentication(authenticateCallback, failure);
	}

	public virtual void SmartSwitchAuthenticateGoogleOpenId(string userid, string token, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		SuccessCallback authenticateCallback = delegate
		{
			AuthenticateGoogleOpenId(userid, token, forceCreate, success, failure, cbObject);
		};
		SmartSwitchAuthentication(authenticateCallback, failure);
	}

	public virtual void SmartSwitchAuthenticateSteam(string userid, string sessionticket, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		SuccessCallback authenticateCallback = delegate
		{
			AuthenticateSteam(userid, sessionticket, forceCreate, success, failure, cbObject);
		};
		SmartSwitchAuthentication(authenticateCallback, failure);
	}

	public virtual void SmartSwitchAuthenticateTwitter(string userid, string token, string secret, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		SuccessCallback authenticateCallback = delegate
		{
			AuthenticateTwitter(userid, token, secret, forceCreate, success, failure, cbObject);
		};
		SmartSwitchAuthentication(authenticateCallback, failure);
	}

	public virtual void SmartSwitchAuthenticateUniversal(string username, string password, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		SuccessCallback authenticateCallback = delegate
		{
			AuthenticateUniversal(username, password, forceCreate, success, failure, cbObject);
		};
		SmartSwitchAuthentication(authenticateCallback, failure);
	}

	public virtual void SmartSwitchAuthenticateAdvanced(AuthenticationType authenticationType, AuthenticationIds ids, bool forceCreate, Dictionary<string, object> extraJson, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		SuccessCallback authenticateCallback = delegate
		{
			AuthenticateAdvanced(authenticationType, ids, forceCreate, extraJson, success, failure, cbObject);
		};
		SmartSwitchAuthentication(authenticateCallback, failure);
	}

	public virtual void SmartSwitchAuthenticateUltra(string ultraUsername, string ultraIdToken, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		SuccessCallback authenticateCallback = delegate
		{
			AuthenticateUltra(ultraUsername, ultraIdToken, forceCreate, success, failure, cbObject);
		};
		SmartSwitchAuthentication(authenticateCallback, failure);
	}

	public virtual void SmartSwitchAuthenticateNintendo(string nintendoAccountId, string nintendoAuthToken, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		SuccessCallback authenticateCallback = delegate
		{
			AuthenticateNintendo(nintendoAccountId, nintendoAuthToken, forceCreate, success, failure, cbObject);
		};
		SmartSwitchAuthentication(authenticateCallback, failure);
	}

	private void SmartSwitchAuthentication(SuccessCallback authenticateCallback, FailureCallback failureCallback)
	{
		SuccessCallback identitiesCallback = GetIdentitiesCallback(authenticateCallback, failureCallback);
		if (Client.Authenticated)
		{
			Client.IdentityService.GetIdentities(identitiesCallback);
		}
		else
		{
			authenticateCallback("", null);
		}
	}

	private SuccessCallback GetIdentitiesCallback(SuccessCallback success, FailureCallback failure)
	{
		return delegate(string response, object cbObject)
		{
			Dictionary<string, object> dictionary = (Dictionary<string, object>)((Dictionary<string, object>)JsonReader.Deserialize(response))["data"];
			if (dictionary.ContainsKey("identities"))
			{
				if (((Dictionary<string, object>)dictionary["identities"]).Count == 0)
				{
					Client.PlayerStateService.DeleteUser(success, failure);
				}
				else
				{
					Client.PlayerStateService.Logout(success, failure);
				}
			}
		};
	}

	public void Reconnect(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		WrapperAuthCallbackObject cbObject2 = MakeWrapperAuthCallback(success, failure, cbObject, isAnonymousAuth: true);
		Client.AuthenticationService.AuthenticateAnonymous(forceCreate: false, AuthSuccessCallback, AuthFailureCallback, cbObject2);
	}

	protected virtual void InitializeIdentity(bool isAnonymousAuth = false)
	{
		string text = GetStoredProfileId();
		string text2 = GetStoredAnonymousId();
		if ((text2 != "" && text == "") || text2 == "")
		{
			text2 = Client.AuthenticationService.GenerateAnonymousId();
			text = "";
			SetStoredAnonymousId(text2);
			SetStoredProfileId(text);
		}
		string profileId = text;
		if (!isAnonymousAuth && AlwaysAllowProfileSwitch)
		{
			profileId = "";
		}
		SetStoredAuthenticationType(isAnonymousAuth ? AUTHENTICATION_ANONYMOUS : "");
		Client.InitializeIdentity(profileId, text2);
	}

	public void ResetEmailPassword(string externalId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Client.AuthenticationService.ResetEmailPassword(externalId, success, failure);
	}

	public void ResetEmailPasswordAdvanced(string emailAddress, string serviceParams, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Client.AuthenticationService.ResetEmailPasswordAdvanced(emailAddress, serviceParams, success, failure);
	}

	public void ResetEmailPasswordWithExpiry(string externalId, int tokenTtlInMinutes, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Client.AuthenticationService.ResetEmailPasswordWithExpiry(externalId, tokenTtlInMinutes, success, failure);
	}

	public void ResetEmailPasswordAdvancedWithExpiry(string emailAddress, string serviceParams, int tokenTtlInMinutes, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Client.AuthenticationService.ResetEmailPasswordAdvancedWithExpiry(emailAddress, serviceParams, tokenTtlInMinutes, success, failure);
	}

	public void ResetUniversalIdPassword(string externalId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Client.AuthenticationService.ResetUniversalIdPassword(externalId, success, failure);
	}

	public void ResetUniversalIdPasswordAdvanced(string emailAddress, string serviceParams, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Client.AuthenticationService.ResetUniversalIdPasswordAdvanced(emailAddress, serviceParams, success, failure);
	}

	public void ResetUniversalIdPasswordWithExpiry(string externalId, int tokenTtlInMinutes, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Client.AuthenticationService.ResetUniversalIdPasswordWithExpiry(externalId, tokenTtlInMinutes, success, failure);
	}

	public void ResetUniversalIdPasswordAdvancedWithExpiry(string emailAddress, string serviceParams, int tokenTtlInMinutes, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Client.AuthenticationService.ResetUniversalIdPasswordAdvancedWithExpiry(emailAddress, serviceParams, tokenTtlInMinutes, success, failure);
	}

	public virtual string GetStoredProfileId()
	{
		return _wrapperData.ProfileId;
	}

	public virtual void SetStoredProfileId(string profileId)
	{
		_wrapperData.ProfileId = profileId;
		SaveData();
	}

	public virtual void ResetStoredProfileId()
	{
		_wrapperData.ProfileId = "";
		SaveData();
	}

	public virtual string GetStoredAnonymousId()
	{
		return _wrapperData.AnonymousId;
	}

	public virtual void SetStoredAnonymousId(string anonymousId)
	{
		_wrapperData.AnonymousId = anonymousId;
		SaveData();
	}

	public virtual void ResetStoredAnonymousId()
	{
		_wrapperData.AnonymousId = "";
		SaveData();
	}

	public virtual string GetStoredAuthenticationType()
	{
		return _wrapperData.AuthenticationType;
	}

	public virtual void SetStoredAuthenticationType(string authenticationType)
	{
		_wrapperData.AuthenticationType = authenticationType;
		SaveData();
	}

	public virtual void ResetStoredAuthenticationType()
	{
		_wrapperData.AuthenticationType = "";
		SaveData();
	}

	protected virtual void Reauthenticate()
	{
		Init(_instance._lastUrl, _instance._lastSecretKey, _instance._lastAppId, _instance._lastAppVersion);
		if (GetStoredAuthenticationType() == AUTHENTICATION_ANONYMOUS)
		{
			AuthenticateAnonymous();
		}
	}

	protected virtual void AuthSuccessCallback(string json, object cbObject)
	{
		Dictionary<string, object> dictionary = (Dictionary<string, object>)((Dictionary<string, object>)JsonReader.Deserialize(json))["data"];
		string text = "";
		if (dictionary.ContainsKey("profileId"))
		{
			text = (string)dictionary["profileId"];
		}
		if (text != "")
		{
			SetStoredProfileId(text);
		}
		if (cbObject != null)
		{
			WrapperAuthCallbackObject wrapperAuthCallbackObject = (WrapperAuthCallbackObject)cbObject;
			if (wrapperAuthCallbackObject._successCallback != null)
			{
				wrapperAuthCallbackObject._successCallback(json, wrapperAuthCallbackObject._cbObject);
			}
		}
	}

	protected virtual void AuthFailureCallback(int statusCode, int reasonCode, string errorJson, object cbObject)
	{
		if (cbObject != null)
		{
			WrapperAuthCallbackObject wrapperAuthCallbackObject = (WrapperAuthCallbackObject)cbObject;
			if (wrapperAuthCallbackObject._failureCallback != null)
			{
				wrapperAuthCallbackObject._failureCallback(statusCode, reasonCode, errorJson, wrapperAuthCallbackObject._cbObject);
			}
		}
	}

	private void SaveData()
	{
		string obj = (string.IsNullOrEmpty(WrapperName) ? "" : (WrapperName + "."));
		PlayerPrefs.SetString(obj + PREFS_PROFILE_ID, _wrapperData.ProfileId);
		PlayerPrefs.SetString(obj + PREFS_ANONYMOUS_ID, _wrapperData.AnonymousId);
		PlayerPrefs.SetString(obj + PREFS_AUTHENTICATION_TYPE, _wrapperData.AuthenticationType);
		PlayerPrefs.Save();
	}

	private void LoadData()
	{
		string text = (string.IsNullOrEmpty(WrapperName) ? "" : (WrapperName + "."));
		_wrapperData.ProfileId = PlayerPrefs.GetString(text + PREFS_PROFILE_ID);
		_wrapperData.AnonymousId = PlayerPrefs.GetString(text + PREFS_ANONYMOUS_ID);
		_wrapperData.AuthenticationType = PlayerPrefs.GetString(text + PREFS_AUTHENTICATION_TYPE);
	}

	private WrapperAuthCallbackObject MakeWrapperAuthCallback(SuccessCallback successCallback, FailureCallback failureCallback, object cbObject = null, bool isAnonymousAuth = false)
	{
		WrapperAuthCallbackObject result = new WrapperAuthCallbackObject
		{
			_successCallback = successCallback,
			_failureCallback = failureCallback,
			_cbObject = cbObject
		};
		InitializeIdentity(isAnonymousAuth);
		return result;
	}
}
