using System;
using System.Collections.Generic;
using BrainCloud.Common;
using BrainCloud.Internal;
using BrainCloud.JsonFx.Json;
using UnityEngine;

namespace BrainCloud;

public class BrainCloudAuthentication
{
	private BrainCloudClient _client;

	public bool CompressResponses { get; set; }

	public string AnonymousId { get; set; }

	public string ProfileId { get; set; }

	public BrainCloudAuthentication(BrainCloudClient client)
	{
		_client = client;
	}

	public string GenerateAnonymousId()
	{
		return Guid.NewGuid().ToString();
	}

	public void Initialize(string profileId, string anonymousId)
	{
		ProfileId = profileId;
		AnonymousId = anonymousId;
		CompressResponses = false;
	}

	public void ClearSavedProfileID()
	{
		ProfileId = null;
	}

	public void AuthenticateAnonymous(bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Debug.Log("Doing Anonymous authentication");
		Authenticate(AnonymousId, "", AuthenticationType.Anonymous, null, forceCreate, null, success, failure, cbObject);
	}

	public void AuthenticateAnonymous(string anonymousId, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		AnonymousId = anonymousId;
		AuthenticateAnonymous(forceCreate, success, failure, cbObject);
	}

	public void AuthenticateEmailPassword(string email, string password, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Authenticate(email, password, AuthenticationType.Email, null, forceCreate, null, success, failure, cbObject);
	}

	public void AuthenticateUniversal(string userId, string password, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Authenticate(userId, password, AuthenticationType.Universal, null, forceCreate, null, success, failure, cbObject);
	}

	public void AuthenticateFacebook(string externalId, string authenticationToken, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Authenticate(externalId, authenticationToken, AuthenticationType.Facebook, null, forceCreate, null, success, failure, cbObject);
	}

	public void AuthenticateFacebookLimited(string externalId, string authenticationToken, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Authenticate(externalId, authenticationToken, AuthenticationType.FacebookLimited, null, forceCreate, null, success, failure, cbObject);
	}

	public void AuthenticateOculus(string oculusId, string oculusNonce, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Authenticate(oculusId, oculusNonce, AuthenticationType.Oculus, null, forceCreate, null, success, failure, cbObject);
	}

	public void AuthenticatePlaystationNetwork(string accountId, string authToken, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Authenticate(accountId, authToken, AuthenticationType.PlaystationNetwork, null, forceCreate, null, success, failure, cbObject);
	}

	public void AuthenticateGameCenter(string gameCenterId, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Authenticate(gameCenterId, "", AuthenticationType.GameCenter, null, forceCreate, null, success, failure, cbObject);
	}

	public void AuthenticateSteam(string userId, string sessionticket, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Authenticate(userId, sessionticket, AuthenticationType.Steam, null, forceCreate, null, success, failure, cbObject);
	}

	public void AuthenticateApple(string appleUserId, string identityToken, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Authenticate(appleUserId, identityToken, AuthenticationType.Apple, null, forceCreate, null, success, failure, cbObject);
	}

	public void AuthenticateGoogle(string googleUserId, string serverAuthCode, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Authenticate(googleUserId, serverAuthCode, AuthenticationType.Google, null, forceCreate, null, success, failure, cbObject);
	}

	public void AuthenticateGoogleOpenId(string googleUserAccountEmail, string IdToken, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Authenticate(googleUserAccountEmail, IdToken, AuthenticationType.GoogleOpenId, null, forceCreate, null, success, failure, cbObject);
	}

	public void AuthenticateTwitter(string userId, string token, string secret, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Authenticate(userId, token + ":" + secret, AuthenticationType.Twitter, null, forceCreate, null, success, failure, cbObject);
	}

	public void AuthenticateParse(string userId, string token, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Authenticate(userId, token, AuthenticationType.Parse, null, forceCreate, null, success, failure, cbObject);
	}

	public void AuthenticateSettopHandoff(string handoffCode, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Authenticate(handoffCode, "", AuthenticationType.SettopHandoff, null, forceCreate: false, null, success, failure, cbObject);
	}

	public void AuthenticateHandoff(string handoffId, string securityToken, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Authenticate(handoffId, securityToken, AuthenticationType.Handoff, null, forceCreate: false, null, success, failure, cbObject);
	}

	public void AuthenticateExternal(string userId, string token, string externalAuthName, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Authenticate(userId, token, AuthenticationType.External, externalAuthName, forceCreate, null, success, failure, cbObject);
	}

	public void AuthenticateAdvanced(AuthenticationType authenticationType, AuthenticationIds ids, bool forceCreate, Dictionary<string, object> extraJson, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Authenticate(ids.externalId, ids.authenticationToken, authenticationType, ids.authenticationSubType, forceCreate, extraJson, success, failure, cbObject);
	}

	public void AuthenticateUltra(string ultraUsername, string ultraIdToken, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Authenticate(ultraUsername, ultraIdToken, AuthenticationType.Ultra, null, forceCreate, null, success, failure, cbObject);
	}

	public void AuthenticateNintendo(string accountId, string authToken, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Authenticate(accountId, authToken, AuthenticationType.Nintendo, null, forceCreate, null, success, failure, cbObject);
	}

	public void ResetEmailPassword(string externalId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.AuthenticateServiceAuthenticateExternalId.Value] = externalId;
		dictionary[OperationParam.AuthenticateServiceAuthenticateGameId.Value] = _client.AppId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure);
		ServerCall serviceMessage = new ServerCall(ServiceName.Authenticate, ServiceOperation.ResetEmailPassword, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ResetEmailPasswordWithExpiry(string externalId, int tokenTtlInMinutes, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.AuthenticateServiceAuthenticateExternalId.Value] = externalId;
		dictionary[OperationParam.AuthenticateServiceAuthenticateGameId.Value] = _client.AppId;
		dictionary[OperationParam.AuthenticateServiceAuthenticateTokenTtlInMinutes.Value] = tokenTtlInMinutes;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure);
		ServerCall serviceMessage = new ServerCall(ServiceName.Authenticate, ServiceOperation.ResetEmailPasswordWithExpiry, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ResetEmailPasswordAdvanced(string emailAddress, string serviceParams, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.AuthenticateServiceAuthenticateGameId.Value] = _client.AppId;
		dictionary[OperationParam.AuthenticateServiceAuthenticateEmailAddress.Value] = emailAddress;
		Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(serviceParams);
		dictionary[OperationParam.AuthenticateServiceAuthenticateServiceParams.Value] = value;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure);
		ServerCall serviceMessage = new ServerCall(ServiceName.Authenticate, ServiceOperation.ResetEmailPasswordAdvanced, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ResetEmailPasswordAdvancedWithExpiry(string emailAddress, string serviceParams, int tokenTtlInMinutes, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.AuthenticateServiceAuthenticateGameId.Value] = _client.AppId;
		dictionary[OperationParam.AuthenticateServiceAuthenticateEmailAddress.Value] = emailAddress;
		Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(serviceParams);
		dictionary[OperationParam.AuthenticateServiceAuthenticateServiceParams.Value] = value;
		dictionary[OperationParam.AuthenticateServiceAuthenticateTokenTtlInMinutes.Value] = tokenTtlInMinutes;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure);
		ServerCall serviceMessage = new ServerCall(ServiceName.Authenticate, ServiceOperation.ResetEmailPasswordAdvancedWithExpiry, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ResetUniversalIdPassword(string universalId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.AuthenticateServiceAuthenticateGameId.Value] = _client.AppId;
		dictionary[OperationParam.AuthenticateServiceAuthenticateUniversalId.Value] = universalId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure);
		ServerCall serviceMessage = new ServerCall(ServiceName.Authenticate, ServiceOperation.ResetUniversalIdPassword, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ResetUniversalIdPasswordWithExpiry(string universalId, int tokenTtlInMinutes, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.AuthenticateServiceAuthenticateGameId.Value] = _client.AppId;
		dictionary[OperationParam.AuthenticateServiceAuthenticateUniversalId.Value] = universalId;
		dictionary[OperationParam.AuthenticateServiceAuthenticateTokenTtlInMinutes.Value] = tokenTtlInMinutes;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure);
		ServerCall serviceMessage = new ServerCall(ServiceName.Authenticate, ServiceOperation.ResetUniversalIdPasswordWithExpiry, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ResetUniversalIdPasswordAdvanced(string universalId, string serviceParams, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.AuthenticateServiceAuthenticateGameId.Value] = _client.AppId;
		dictionary[OperationParam.AuthenticateServiceAuthenticateUniversalId.Value] = universalId;
		Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(serviceParams);
		dictionary[OperationParam.AuthenticateServiceAuthenticateServiceParams.Value] = value;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure);
		ServerCall serviceMessage = new ServerCall(ServiceName.Authenticate, ServiceOperation.ResetUniversalIdPasswordAdvanced, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ResetUniversalIdPasswordAdvancedWithExpiry(string universalId, string serviceParams, int tokenTtlInMinutes, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.AuthenticateServiceAuthenticateGameId.Value] = _client.AppId;
		dictionary[OperationParam.AuthenticateServiceAuthenticateUniversalId.Value] = universalId;
		Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(serviceParams);
		dictionary[OperationParam.AuthenticateServiceAuthenticateServiceParams.Value] = value;
		dictionary[OperationParam.AuthenticateServiceAuthenticateTokenTtlInMinutes.Value] = tokenTtlInMinutes;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure);
		ServerCall serviceMessage = new ServerCall(ServiceName.Authenticate, ServiceOperation.ResetUniversalIdPasswordAdvancedWithExpiry, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	private void Authenticate(string externalId, string authenticationToken, AuthenticationType authenticationType, string externalAuthName, bool forceCreate, Dictionary<string, object> extraJson, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		string languageCode = _client.LanguageCode;
		double uTCOffsetForCurrentTimeZone = Util.GetUTCOffsetForCurrentTimeZone();
		string countryCode = _client.CountryCode;
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.AuthenticateServiceAuthenticateExternalId.Value] = externalId;
		dictionary[OperationParam.AuthenticateServiceAuthenticateAuthenticationToken.Value] = authenticationToken;
		dictionary[OperationParam.AuthenticateServiceAuthenticateAuthenticationType.Value] = authenticationType.ToString();
		dictionary[OperationParam.AuthenticateServiceAuthenticateForceCreate.Value] = forceCreate;
		dictionary[OperationParam.AuthenticateServiceAuthenticateCompressResponses.Value] = CompressResponses;
		dictionary[OperationParam.AuthenticateServiceAuthenticateProfileId.Value] = ProfileId;
		dictionary[OperationParam.AuthenticateServiceAuthenticateAnonymousId.Value] = AnonymousId;
		dictionary[OperationParam.AuthenticateServiceAuthenticateGameId.Value] = _client.AppId;
		dictionary[OperationParam.AuthenticateServiceAuthenticateReleasePlatform.Value] = _client.ReleasePlatform.ToString();
		dictionary[OperationParam.AuthenticateServiceAuthenticateGameVersion.Value] = _client.AppVersion;
		dictionary[OperationParam.AuthenticateServiceAuthenticateBrainCloudVersion.Value] = Version.GetVersion();
		dictionary["clientLib"] = "csharp-unity";
		if (Util.IsOptionalParameterValid(externalAuthName))
		{
			dictionary[OperationParam.AuthenticateServiceAuthenticateExternalAuthName.Value] = externalAuthName;
		}
		if (extraJson != null)
		{
			dictionary[OperationParam.AuthenticateServiceAuthenticateExtraJson.Value] = extraJson;
		}
		dictionary[OperationParam.AuthenticateServiceAuthenticateCountryCode.Value] = countryCode;
		dictionary[OperationParam.AuthenticateServiceAuthenticateLanguageCode.Value] = languageCode;
		dictionary[OperationParam.AuthenticateServiceAuthenticateTimeZoneOffset.Value] = uTCOffsetForCurrentTimeZone;
		ServerCallback serverCallback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Authenticate, ServiceOperation.Authenticate, dictionary, serverCallback);
		if (_client.Comms.IsAuthenticateRequestInProgress())
		{
			_client.Comms.AddCallbackToAuthenticateRequest(serverCallback);
		}
		else
		{
			_client.SendRequest(serviceMessage);
		}
	}
}
