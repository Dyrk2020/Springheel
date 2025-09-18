using System;
using System.Collections.Generic;
using BrainCloud.Common;
using BrainCloud.Internal;

namespace BrainCloud;

public class BrainCloudIdentity
{
	private BrainCloudClient _client;

	public BrainCloudIdentity(BrainCloudClient client)
	{
		_client = client;
	}

	public void AttachFacebookIdentity(string facebookId, string authenticationToken, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		AttachIdentity(facebookId, authenticationToken, AuthenticationType.Facebook, success, failure, cbObject);
	}

	public void MergeFacebookIdentity(string facebookId, string authenticationToken, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		MergeIdentity(facebookId, authenticationToken, AuthenticationType.Facebook, success, failure, cbObject);
	}

	public void DetachFacebookIdentity(string facebookId, bool continueAnon, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		DetachIdentity(facebookId, AuthenticationType.Facebook, continueAnon, success, failure, cbObject);
	}

	public void AttachAdvancedIdentity(AuthenticationType authenticationType, AuthenticationIds ids, Dictionary<string, object> extraJson, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.IdentityServiceExternalId.Value] = ids.externalId;
		dictionary[OperationParam.IdentityServiceAuthenticationType.Value] = authenticationType.ToString();
		dictionary[OperationParam.AuthenticateServiceAuthenticateAuthenticationToken.Value] = ids.authenticationToken;
		if (Util.IsOptionalParameterValid(ids.authenticationSubType))
		{
			dictionary[OperationParam.AuthenticateServiceAuthenticateExternalId.Value] = ids.authenticationSubType;
		}
		if (extraJson != null)
		{
			dictionary[OperationParam.AuthenticateServiceAuthenticateExtraJson.Value] = extraJson;
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Identity, ServiceOperation.Attach, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void MergeAdvancedIdentity(AuthenticationType authenticationType, AuthenticationIds ids, Dictionary<string, object> extraJson, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.IdentityServiceExternalId.Value] = ids.externalId;
		dictionary[OperationParam.IdentityServiceAuthenticationType.Value] = authenticationType.ToString();
		dictionary[OperationParam.AuthenticateServiceAuthenticateAuthenticationToken.Value] = ids.authenticationToken;
		if (Util.IsOptionalParameterValid(ids.authenticationSubType))
		{
			dictionary[OperationParam.AuthenticateServiceAuthenticateExternalId.Value] = ids.authenticationSubType;
		}
		if (extraJson != null)
		{
			dictionary[OperationParam.AuthenticateServiceAuthenticateExtraJson.Value] = extraJson;
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Identity, ServiceOperation.Merge, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void DetachAdvancedIdentity(AuthenticationType authenticationType, string externalId, bool continueAnon, Dictionary<string, object> extraJson, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.IdentityServiceExternalId.Value] = externalId;
		dictionary[OperationParam.IdentityServiceAuthenticationType.Value] = authenticationType.ToString();
		dictionary[OperationParam.IdentityServiceConfirmAnonymous.Value] = continueAnon;
		if (extraJson != null)
		{
			dictionary[OperationParam.AuthenticateServiceAuthenticateExtraJson.Value] = extraJson;
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Identity, ServiceOperation.Detach, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void AttachUltraIdentity(string ultraUsername, string ultraIdToken, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		AttachIdentity(ultraUsername, ultraIdToken, AuthenticationType.Ultra, success, failure, cbObject);
	}

	public void MergeUltraIdentity(string ultraUsername, string ultraIdToken, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		MergeIdentity(ultraUsername, ultraIdToken, AuthenticationType.Ultra, success, failure, cbObject);
	}

	public void DetachUltraIdentity(string ultraUsername, bool continueAnon, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		DetachIdentity(ultraUsername, AuthenticationType.Ultra, continueAnon, success, failure, cbObject);
	}

	public void AttachOculusIdentity(string oculusId, string oculusNonce, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		AttachIdentity(oculusId, oculusNonce, AuthenticationType.Oculus, success, failure, cbObject);
	}

	public void MergeOculusIdentity(string oculusId, string oculusNonce, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		MergeIdentity(oculusId, oculusNonce, AuthenticationType.Oculus, success, failure, cbObject);
	}

	public void DetachOculusIdentity(string oculusId, bool continueAnon, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		DetachIdentity(oculusId, AuthenticationType.Oculus, continueAnon, success, failure, cbObject);
	}

	public void AttachFacebookLimitedIdentity(string facebookLimitedId, string authenticationToken, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		AttachIdentity(facebookLimitedId, authenticationToken, AuthenticationType.FacebookLimited, success, failure, cbObject);
	}

	public void MergeFacebookLimitedIdentity(string facebookLimitedId, string authenticationToken, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		MergeIdentity(facebookLimitedId, authenticationToken, AuthenticationType.FacebookLimited, success, failure, cbObject);
	}

	public void DetachFacebookLimitedIdentity(string facebookLimitedId, bool continueAnon, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		DetachIdentity(facebookLimitedId, AuthenticationType.FacebookLimited, continueAnon, success, failure, cbObject);
	}

	public void AttachPlaystationNetworkIdentity(string psnAccountId, string authenticationToken, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		AttachIdentity(psnAccountId, authenticationToken, AuthenticationType.PlaystationNetwork, success, failure, cbObject);
	}

	public void MergePlaystationNetworkIdentity(string psnAccountId, string authenticationToken, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		MergeIdentity(psnAccountId, authenticationToken, AuthenticationType.PlaystationNetwork, success, failure, cbObject);
	}

	public void DetachPlaystationNetworkIdentity(string psnAccountId, bool continueAnon, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		DetachIdentity(psnAccountId, AuthenticationType.PlaystationNetwork, continueAnon, success, failure, cbObject);
	}

	public void AttachGameCenterIdentity(string gameCenterId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		AttachIdentity(gameCenterId, "", AuthenticationType.GameCenter, success, failure, cbObject);
	}

	public void MergeGameCenterIdentity(string gameCenterId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		MergeIdentity(gameCenterId, "", AuthenticationType.GameCenter, success, failure, cbObject);
	}

	public void DetachGameCenterIdentity(string gameCenterId, bool continueAnon, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		DetachIdentity(gameCenterId, AuthenticationType.GameCenter, continueAnon, success, failure, cbObject);
	}

	public void AttachEmailIdentity(string email, string password, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		AttachIdentity(email, password, AuthenticationType.Email, success, failure, cbObject);
	}

	public void MergeEmailIdentity(string email, string password, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		MergeIdentity(email, password, AuthenticationType.Email, success, failure, cbObject);
	}

	public void DetachEmailIdentity(string email, bool continueAnon, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		DetachIdentity(email, AuthenticationType.Email, continueAnon, success, failure, cbObject);
	}

	public void AttachUniversalIdentity(string userId, string password, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		AttachIdentity(userId, password, AuthenticationType.Universal, success, failure, cbObject);
	}

	public void MergeUniversalIdentity(string userId, string password, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		MergeIdentity(userId, password, AuthenticationType.Universal, success, failure, cbObject);
	}

	public void DetachUniversalIdentity(string userId, bool continueAnon, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		DetachIdentity(userId, AuthenticationType.Universal, continueAnon, success, failure, cbObject);
	}

	public void AttachSteamIdentity(string steamId, string sessionTicket, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		AttachIdentity(steamId, sessionTicket, AuthenticationType.Steam, success, failure, cbObject);
	}

	public void MergeSteamIdentity(string steamId, string sessionTicket, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		MergeIdentity(steamId, sessionTicket, AuthenticationType.Steam, success, failure, cbObject);
	}

	public void DetachSteamIdentity(string steamId, bool continueAnon, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		DetachIdentity(steamId, AuthenticationType.Steam, continueAnon, success, failure, cbObject);
	}

	public void AttachGoogleIdentity(string googleUserId, string serverAuthCode, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		AttachIdentity(googleUserId, serverAuthCode, AuthenticationType.Google, success, failure, cbObject);
	}

	public void MergeGoogleIdentity(string googleUserId, string serverAuthCode, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		MergeIdentity(googleUserId, serverAuthCode, AuthenticationType.Google, success, failure, cbObject);
	}

	public void DetachGoogleIdentity(string googleUserId, bool continueAnon, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		DetachIdentity(googleUserId, AuthenticationType.Google, continueAnon, success, failure, cbObject);
	}

	public void AttachGoogleOpenIdIdentity(string googleUserAccountEmail, string IdToken, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		AttachIdentity(googleUserAccountEmail, IdToken, AuthenticationType.GoogleOpenId, success, failure, cbObject);
	}

	public void MergeGoogleOpenIdIdentity(string googleUserAccountEmail, string IdToken, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		MergeIdentity(googleUserAccountEmail, IdToken, AuthenticationType.GoogleOpenId, success, failure, cbObject);
	}

	public void DetachGoogleOpenIdIdentity(string googleUserAccountEmail, bool continueAnon, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		DetachIdentity(googleUserAccountEmail, AuthenticationType.GoogleOpenId, continueAnon, success, failure, cbObject);
	}

	public void AttachAppleIdentity(string appleUserId, string identityToken, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		AttachIdentity(appleUserId, identityToken, AuthenticationType.Apple, success, failure, cbObject);
	}

	public void MergeAppleIdentity(string appleUserId, string identityToken, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		MergeIdentity(appleUserId, identityToken, AuthenticationType.Apple, success, failure, cbObject);
	}

	public void DetachAppleIdentity(string appleUserId, bool continueAnon, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		DetachIdentity(appleUserId, AuthenticationType.Apple, continueAnon, success, failure, cbObject);
	}

	public void AttachTwitterIdentity(string twitterId, string authenticationToken, string secret, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		AttachIdentity(twitterId, authenticationToken + ":" + secret, AuthenticationType.Twitter, success, failure, cbObject);
	}

	public void MergeTwitterIdentity(string twitterId, string authenticationToken, string secret, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		MergeIdentity(twitterId, authenticationToken + ":" + secret, AuthenticationType.Twitter, success, failure, cbObject);
	}

	public void DetachTwitterIdentity(string twitterId, bool continueAnon, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		DetachIdentity(twitterId, AuthenticationType.Twitter, continueAnon, success, failure, cbObject);
	}

	public void AttachParseIdentity(string parseId, string authenticationToken, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		AttachIdentity(parseId, authenticationToken, AuthenticationType.Parse, success, failure, cbObject);
	}

	public void MergeParseIdentity(string parseId, string authenticationToken, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		MergeIdentity(parseId, authenticationToken, AuthenticationType.Parse, success, failure, cbObject);
	}

	public void DetachParseIdentity(string parseId, bool continueAnon, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		DetachIdentity(parseId, AuthenticationType.Parse, continueAnon, success, failure, cbObject);
	}

	public void AttachNintendoIdentity(string nintendoAccountId, string authenticationToken, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		AttachIdentity(nintendoAccountId, authenticationToken, AuthenticationType.Nintendo, success, failure, cbObject);
	}

	public void MergeNintendoIdentity(string nintendoAccountId, string authenticationToken, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		MergeIdentity(nintendoAccountId, authenticationToken, AuthenticationType.Nintendo, success, failure, cbObject);
	}

	public void DetachNintendoIdentity(string nintendoAccountId, bool continueAnon, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		DetachIdentity(nintendoAccountId, AuthenticationType.Nintendo, continueAnon, success, failure, cbObject);
	}

	public void SwitchToChildProfile(string childProfileId, string childAppId, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		SwitchToChildProfile(childProfileId, childAppId, forceCreate, forceSingleton: false, success, failure, cbObject);
	}

	public void SwitchToSingletonChildProfile(string childAppId, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		SwitchToChildProfile(null, childAppId, forceCreate, forceSingleton: true, success, failure, cbObject);
	}

	public void AttachNonLoginUniversalId(string externalId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.IdentityServiceExternalId.Value] = externalId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Identity, ServiceOperation.AttachNonLoginUniversalId, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void UpdateUniversalIdLogin(string externalId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.IdentityServiceExternalId.Value] = externalId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Identity, ServiceOperation.UpdateUniversalIdLogin, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void AttachParentWithIdentity(string externalId, string authenticationToken, AuthenticationType authenticationType, string externalAuthName, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.IdentityServiceExternalId.Value] = externalId;
		dictionary[OperationParam.AuthenticateServiceAuthenticateAuthenticationToken.Value] = authenticationToken;
		dictionary[OperationParam.IdentityServiceAuthenticationType.Value] = authenticationType.ToString();
		if (Util.IsOptionalParameterValid(externalAuthName))
		{
			dictionary[OperationParam.AuthenticateServiceAuthenticateExternalAuthName.Value] = externalAuthName;
		}
		dictionary[OperationParam.AuthenticateServiceAuthenticateForceCreate.Value] = forceCreate;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Identity, ServiceOperation.AttachParentWithIdentity, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void SwitchToParentProfile(string parentLevelName, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.AuthenticateServiceAuthenticateLevelName.Value] = parentLevelName;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Identity, ServiceOperation.SwitchToParentProfile, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void DetachParent(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Identity, ServiceOperation.DetachParent, null, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetChildProfiles(bool includeSummaryData, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PlayerStateServiceIncludeSummaryData.Value] = includeSummaryData;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Identity, ServiceOperation.GetChildProfiles, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetIdentities(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Identity, ServiceOperation.GetIdentities, null, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetExpiredIdentities(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Identity, ServiceOperation.GetExpiredIdentities, null, callback);
		_client.SendRequest(serviceMessage);
	}

	public void RefreshIdentity(string externalId, string authenticationToken, AuthenticationType authenticationType, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.IdentityServiceExternalId.Value] = externalId;
		dictionary[OperationParam.AuthenticateServiceAuthenticateAuthenticationToken.Value] = authenticationToken;
		dictionary[OperationParam.IdentityServiceAuthenticationType.Value] = authenticationType.ToString();
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Identity, ServiceOperation.RefreshIdentity, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ChangeEmailIdentity(string oldEmailAddress, string password, string newEmailAddress, bool updateContactEmail, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.IdentityServiceOldEmailAddress.Value] = oldEmailAddress;
		dictionary[OperationParam.AuthenticateServiceAuthenticateAuthenticationToken.Value] = password;
		dictionary[OperationParam.IdentityServiceNewEmailAddress.Value] = newEmailAddress;
		dictionary[OperationParam.IdentityServiceUpdateContactEmail.Value] = updateContactEmail;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Identity, ServiceOperation.ChangeEmailIdentity, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void AttachPeerProfile(string peer, string externalId, string authenticationToken, AuthenticationType authenticationType, string externalAuthName, bool forceCreate, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.IdentityServiceExternalId.Value] = externalId;
		dictionary[OperationParam.AuthenticateServiceAuthenticateAuthenticationToken.Value] = authenticationToken;
		dictionary[OperationParam.IdentityServiceAuthenticationType.Value] = authenticationType.ToString();
		if (Util.IsOptionalParameterValid(externalAuthName))
		{
			dictionary[OperationParam.AuthenticateServiceAuthenticateExternalAuthName.Value] = externalAuthName;
		}
		dictionary[OperationParam.Peer.Value] = peer;
		dictionary[OperationParam.AuthenticateServiceAuthenticateForceCreate.Value] = forceCreate;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Identity, ServiceOperation.AttachPeerProfile, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void DetachPeer(string peer, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.Peer.Value] = peer;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Identity, ServiceOperation.DetachPeer, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetPeerProfiles(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Identity, ServiceOperation.GetPeerProfiles, null, callback);
		_client.SendRequest(serviceMessage);
	}

	[Obsolete("This has been deprecated, use AttachBlockChainIdentity instead. Removal on Match 1, 2022")]
	public void AttachBlockChain(string blockchainConfig, string publicKey, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.BlockChainConfig.Value] = blockchainConfig;
		dictionary[OperationParam.PublicKey.Value] = publicKey;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Identity, ServiceOperation.AttachBlockChain, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	[Obsolete("This has been deprecated, use DetachBlockChainIdentity instead. Removal on Match 1, 2022")]
	public void DetachBlockChain(string blockchainConfig, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.BlockChainConfig.Value] = blockchainConfig;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Identity, ServiceOperation.DetachBlockChain, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void AttachBlockChainIdentity(string blockchainConfig, string publicKey, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.BlockChainConfig.Value] = blockchainConfig;
		dictionary[OperationParam.PublicKey.Value] = publicKey;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Identity, ServiceOperation.AttachBlockChain, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void DetachBlockChainIdentity(string blockchainConfig, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.BlockChainConfig.Value] = blockchainConfig;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Identity, ServiceOperation.DetachBlockChain, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	private void AttachIdentity(string externalId, string authenticationToken, AuthenticationType authenticationType, SuccessCallback success, FailureCallback failure, object cbObject)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.IdentityServiceExternalId.Value] = externalId;
		dictionary[OperationParam.IdentityServiceAuthenticationType.Value] = authenticationType.ToString();
		dictionary[OperationParam.AuthenticateServiceAuthenticateAuthenticationToken.Value] = authenticationToken;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Identity, ServiceOperation.Attach, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	private void MergeIdentity(string externalId, string authenticationToken, AuthenticationType authenticationType, SuccessCallback success, FailureCallback failure, object cbObject)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.IdentityServiceExternalId.Value] = externalId;
		dictionary[OperationParam.IdentityServiceAuthenticationType.Value] = authenticationType.ToString();
		dictionary[OperationParam.AuthenticateServiceAuthenticateAuthenticationToken.Value] = authenticationToken;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Identity, ServiceOperation.Merge, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	private void DetachIdentity(string externalId, AuthenticationType authenticationType, bool continueAnon, SuccessCallback success, FailureCallback failure, object cbObject)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.IdentityServiceExternalId.Value] = externalId;
		dictionary[OperationParam.IdentityServiceAuthenticationType.Value] = authenticationType.ToString();
		dictionary[OperationParam.IdentityServiceConfirmAnonymous.Value] = continueAnon;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Identity, ServiceOperation.Detach, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	private void SwitchToChildProfile(string childProfileId, string childAppd, bool forceCreate, bool forceSingleton, SuccessCallback success, FailureCallback failure, object cbObject)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (Util.IsOptionalParameterValid(childProfileId))
		{
			dictionary[OperationParam.ProfileId.Value] = childProfileId;
		}
		dictionary[OperationParam.AuthenticateServiceAuthenticateGameId.Value] = childAppd;
		dictionary[OperationParam.AuthenticateServiceAuthenticateForceCreate.Value] = forceCreate;
		dictionary[OperationParam.IdentityServiceForceSingleton.Value] = forceSingleton;
		dictionary[OperationParam.AuthenticateServiceAuthenticateReleasePlatform.Value] = _client.ReleasePlatform.ToString();
		dictionary[OperationParam.AuthenticateServiceAuthenticateCountryCode.Value] = Util.GetCurrentCountryCode();
		dictionary[OperationParam.AuthenticateServiceAuthenticateLanguageCode.Value] = Util.GetIsoCodeForCurrentLanguage();
		dictionary[OperationParam.AuthenticateServiceAuthenticateTimeZoneOffset.Value] = Util.GetUTCOffsetForCurrentTimeZone();
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Identity, ServiceOperation.SwitchToChildProfile, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}
}
