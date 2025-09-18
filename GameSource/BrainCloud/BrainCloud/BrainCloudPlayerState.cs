using System;
using System.Collections.Generic;
using BrainCloud.Internal;
using BrainCloud.JsonFx.Json;

namespace BrainCloud;

public class BrainCloudPlayerState
{
	private BrainCloudClient _client;

	public BrainCloudPlayerState(BrainCloudClient client)
	{
		_client = client;
	}

	public void ReadUserState(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlayerState, ServiceOperation.Read, null, callback);
		_client.SendRequest(serviceMessage);
	}

	public void DeleteUser(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		ServerCallback callback = BrainCloudClient.CreateServerCallback((SuccessCallback)Delegate.Combine((SuccessCallback)delegate
		{
			_client.Wrapper.ResetStoredAnonymousId();
			_client.Wrapper.ResetStoredProfileId();
		}, success), failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlayerState, ServiceOperation.FullReset, null, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ResetUser(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlayerState, ServiceOperation.DataReset, null, callback);
		_client.SendRequest(serviceMessage);
	}

	public void Logout(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlayerState, ServiceOperation.Logout, null, callback);
		_client.SendRequest(serviceMessage);
	}

	[Obsolete("This has been deprecated use UpdateName instead - removal after September 1 2021")]
	public void UpdateUserName(string userName, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PlayerStateServiceUpdateNameData.Value] = userName;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlayerState, ServiceOperation.UpdateName, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void UpdateName(string userName, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PlayerStateServiceUpdateNameData.Value] = userName;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlayerState, ServiceOperation.UpdateName, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void UpdateSummaryFriendData(string jsonSummaryData, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (Util.IsOptionalParameterValid(jsonSummaryData))
		{
			Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonSummaryData);
			dictionary[OperationParam.PlayerStateServiceUpdateSummaryFriendData.Value] = value;
		}
		else
		{
			dictionary = null;
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlayerState, ServiceOperation.UpdateSummary, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetAttributes(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlayerState, ServiceOperation.GetAttributes, null, callback);
		_client.SendRequest(serviceMessage);
	}

	public void UpdateAttributes(string jsonAttributes, bool wipeExisting, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonAttributes);
		dictionary[OperationParam.PlayerStateServiceAttributes.Value] = value;
		dictionary[OperationParam.PlayerStateServiceWipeExisting.Value] = wipeExisting;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlayerState, ServiceOperation.UpdateAttributes, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void RemoveAttributes(IList<string> attributeNames, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PlayerStateServiceAttributes.Value] = attributeNames;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlayerState, ServiceOperation.RemoveAttributes, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void UpdateUserPictureUrl(string pictureUrl, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PlayerStateServicePlayerPictureUrl.Value] = pictureUrl;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlayerState, ServiceOperation.UpdatePictureUrl, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void UpdateContactEmail(string contactEmail, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PlayerStateServiceContactEmail.Value] = contactEmail;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlayerState, ServiceOperation.UpdateContactEmail, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ClearUserStatus(string statusName, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PlayerStateServiceStatusName.Value] = statusName;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlayerState, ServiceOperation.ClearUserStatus, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ExtendUserStatus(string statusName, int additionalSecs, string details, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(details);
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PlayerStateServiceStatusName.Value] = statusName;
		dictionary[OperationParam.PlayerStateServiceAdditionalSecs.Value] = additionalSecs;
		dictionary[OperationParam.PlayerStateServiceDetails.Value] = value;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlayerState, ServiceOperation.ExtendUserStatus, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetUserStatus(string statusName, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PlayerStateServiceStatusName.Value] = statusName;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlayerState, ServiceOperation.GetUserStatus, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void SetUserStatus(string statusName, int durationSecs, string details, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(details);
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PlayerStateServiceStatusName.Value] = statusName;
		dictionary[OperationParam.PlayerStateServiceDurationSecs.Value] = durationSecs;
		dictionary[OperationParam.PlayerStateServiceDetails.Value] = value;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlayerState, ServiceOperation.SetUserStatus, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void UpdateLanguageCode(string languageCode, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PlayerStateServiceLanguageCode.Value] = languageCode;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlayerState, ServiceOperation.UpdateLanguageCode, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void UpdateTimeZoneOffset(string timeZoneOffset, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PlayerStateServiceTimeZoneOffset.Value] = timeZoneOffset;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PlayerState, ServiceOperation.UpdateTimeZoneOffset, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}
}
