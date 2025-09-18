using System;
using System.Collections.Generic;
using BrainCloud.Common;
using BrainCloud.Internal;
using BrainCloud.JsonFx.Json;

namespace BrainCloud;

public class BrainCloudPushNotification
{
	private BrainCloudClient _client;

	public BrainCloudPushNotification(BrainCloudClient client)
	{
		_client = client;
	}

	public bool RegisterPushNotificationDeviceToken(byte[] token, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		if (token != null || token.Length < 1)
		{
			Platform platform = Platform.FromUnityRuntime();
			string token2 = BitConverter.ToString(token).Replace("-", "").ToLower();
			RegisterPushNotificationDeviceToken(platform, token2, success, failure, cbObject);
			return true;
		}
		return false;
	}

	public bool RegisterPushNotificationDeviceToken(string token, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		if (token != null || token.Length < 1)
		{
			Platform platform = Platform.FromUnityRuntime();
			RegisterPushNotificationDeviceToken(platform, token, success, failure, cbObject);
			return true;
		}
		return false;
	}

	public void DeregisterAllPushNotificationDeviceTokens(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> jsonData = new Dictionary<string, object>();
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PushNotification, ServiceOperation.DeregisterAll, jsonData, callback);
		_client.SendRequest(serviceMessage);
	}

	public void DeregisterPushNotificationDeviceToken(Platform platform, string token, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		string value = platform.ToString();
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PushNotificationRegisterParamDeviceType.Value] = value;
		dictionary[OperationParam.PushNotificationRegisterParamDeviceToken.Value] = token;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PushNotification, ServiceOperation.Deregister, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void RegisterPushNotificationDeviceToken(Platform platform, string token, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		string value = platform.ToString();
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PushNotificationRegisterParamDeviceType.Value] = value;
		dictionary[OperationParam.PushNotificationRegisterParamDeviceToken.Value] = token;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PushNotification, ServiceOperation.Register, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void SendSimplePushNotification(string toProfileId, string message, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PushNotificationSendParamToPlayerId.Value] = toProfileId;
		dictionary[OperationParam.PushNotificationSendParamMessage.Value] = message;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PushNotification, ServiceOperation.SendSimple, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void SendRichPushNotification(string toProfileId, int notificationTemplateId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		SendRichPushNotification(toProfileId, notificationTemplateId, null, success, failure, cbObject);
	}

	public void SendRichPushNotificationWithParams(string toProfileId, int notificationTemplateId, string substitutionJson, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		SendRichPushNotification(toProfileId, notificationTemplateId, substitutionJson, success, failure, cbObject);
	}

	public void SendTemplatedPushNotificationToGroup(string groupId, int notificationTemplateId, string substitutionsJson, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GroupId.Value] = groupId;
		dictionary[OperationParam.PushNotificationSendParamNotificationTemplateId.Value] = notificationTemplateId;
		if (Util.IsOptionalParameterValid(substitutionsJson))
		{
			dictionary[OperationParam.PushNotificationSendParamSubstitutions.Value] = JsonReader.Deserialize<Dictionary<string, object>>(substitutionsJson);
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PushNotification, ServiceOperation.SendTemplatedToGroup, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void SendNormalizedPushNotificationToGroup(string groupId, string alertContentJson, string customDataJson, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GroupId.Value] = groupId;
		dictionary[OperationParam.AlertContent.Value] = JsonReader.Deserialize<Dictionary<string, object>>(alertContentJson);
		if (Util.IsOptionalParameterValid(customDataJson))
		{
			dictionary[OperationParam.CustomData.Value] = JsonReader.Deserialize<Dictionary<string, object>>(customDataJson);
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PushNotification, ServiceOperation.SendNormalizedToGroup, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ScheduleRawPushNotificationUTC(string profileId, string fcmContent, string iosContent, string facebookContent, ulong startTimeUTC, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.ProfileId.Value] = profileId;
		if (Util.IsOptionalParameterValid(fcmContent))
		{
			dictionary[OperationParam.PushNotificationSendParamFcmContent.Value] = JsonReader.Deserialize<Dictionary<string, object>>(fcmContent);
		}
		if (Util.IsOptionalParameterValid(iosContent))
		{
			dictionary[OperationParam.PushNotificationSendParamIosContent.Value] = JsonReader.Deserialize<Dictionary<string, object>>(iosContent);
		}
		if (Util.IsOptionalParameterValid(facebookContent))
		{
			dictionary[OperationParam.PushNotificationSendParamFacebookContent.Value] = JsonReader.Deserialize<Dictionary<string, object>>(facebookContent);
		}
		dictionary[OperationParam.StartDateUTC.Value] = startTimeUTC;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PushNotification, ServiceOperation.ScheduleRawNotification, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ScheduleRawPushNotificationMinutes(string profileId, string fcmContent, string iosContent, string facebookContent, int minutesFromNow, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.ProfileId.Value] = profileId;
		if (Util.IsOptionalParameterValid(fcmContent))
		{
			dictionary[OperationParam.PushNotificationSendParamFcmContent.Value] = JsonReader.Deserialize<Dictionary<string, object>>(fcmContent);
		}
		if (Util.IsOptionalParameterValid(iosContent))
		{
			dictionary[OperationParam.PushNotificationSendParamIosContent.Value] = JsonReader.Deserialize<Dictionary<string, object>>(iosContent);
		}
		if (Util.IsOptionalParameterValid(facebookContent))
		{
			dictionary[OperationParam.PushNotificationSendParamFacebookContent.Value] = JsonReader.Deserialize<Dictionary<string, object>>(facebookContent);
		}
		dictionary[OperationParam.MinutesFromNow.Value] = minutesFromNow;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PushNotification, ServiceOperation.ScheduleRawNotification, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void SendRawPushNotification(string toProfileId, string fcmContent, string iosContent, string facebookContent, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PushNotificationSendParamToPlayerId.Value] = toProfileId;
		if (Util.IsOptionalParameterValid(fcmContent))
		{
			dictionary[OperationParam.PushNotificationSendParamFcmContent.Value] = JsonReader.Deserialize<Dictionary<string, object>>(fcmContent);
		}
		if (Util.IsOptionalParameterValid(iosContent))
		{
			dictionary[OperationParam.PushNotificationSendParamIosContent.Value] = JsonReader.Deserialize<Dictionary<string, object>>(iosContent);
		}
		if (Util.IsOptionalParameterValid(facebookContent))
		{
			dictionary[OperationParam.PushNotificationSendParamFacebookContent.Value] = JsonReader.Deserialize<Dictionary<string, object>>(facebookContent);
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PushNotification, ServiceOperation.SendRaw, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void SendRawPushNotificationBatch(IList<string> profileIds, string fcmContent, string iosContent, string facebookContent, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PushNotificationSendParamProfileIds.Value] = profileIds;
		if (Util.IsOptionalParameterValid(fcmContent))
		{
			dictionary[OperationParam.PushNotificationSendParamFcmContent.Value] = JsonReader.Deserialize<Dictionary<string, object>>(fcmContent);
		}
		if (Util.IsOptionalParameterValid(iosContent))
		{
			dictionary[OperationParam.PushNotificationSendParamIosContent.Value] = JsonReader.Deserialize<Dictionary<string, object>>(iosContent);
		}
		if (Util.IsOptionalParameterValid(facebookContent))
		{
			dictionary[OperationParam.PushNotificationSendParamFacebookContent.Value] = JsonReader.Deserialize<Dictionary<string, object>>(facebookContent);
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PushNotification, ServiceOperation.SendRawBatch, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void SendRawPushNotificationToGroup(string groupId, string fcmContent, string iosContent, string facebookContent, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.GroupId.Value] = groupId;
		if (Util.IsOptionalParameterValid(fcmContent))
		{
			dictionary[OperationParam.PushNotificationSendParamFcmContent.Value] = JsonReader.Deserialize<Dictionary<string, object>>(fcmContent);
		}
		if (Util.IsOptionalParameterValid(iosContent))
		{
			dictionary[OperationParam.PushNotificationSendParamIosContent.Value] = JsonReader.Deserialize<Dictionary<string, object>>(iosContent);
		}
		if (Util.IsOptionalParameterValid(facebookContent))
		{
			dictionary[OperationParam.PushNotificationSendParamFacebookContent.Value] = JsonReader.Deserialize<Dictionary<string, object>>(facebookContent);
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PushNotification, ServiceOperation.SendRawToGroup, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ScheduleNormalizedPushNotificationUTC(string profileId, string alertContentJson, string customDataJson, ulong startTimeUTC, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PushNotificationSendParamProfileId.Value] = profileId;
		dictionary[OperationParam.AlertContent.Value] = JsonReader.Deserialize<Dictionary<string, object>>(alertContentJson);
		if (Util.IsOptionalParameterValid(customDataJson))
		{
			dictionary[OperationParam.CustomData.Value] = JsonReader.Deserialize<Dictionary<string, object>>(customDataJson);
		}
		dictionary[OperationParam.StartDateUTC.Value] = startTimeUTC;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PushNotification, ServiceOperation.ScheduleNormalizedNotification, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ScheduleNormalizedPushNotificationMinutes(string profileId, string alertContentJson, string customDataJson, int minutesFromNow, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PushNotificationSendParamProfileId.Value] = profileId;
		dictionary[OperationParam.AlertContent.Value] = JsonReader.Deserialize<Dictionary<string, object>>(alertContentJson);
		if (Util.IsOptionalParameterValid(customDataJson))
		{
			dictionary[OperationParam.CustomData.Value] = JsonReader.Deserialize<Dictionary<string, object>>(customDataJson);
		}
		dictionary[OperationParam.MinutesFromNow.Value] = minutesFromNow;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PushNotification, ServiceOperation.ScheduleNormalizedNotification, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ScheduleRichPushNotificationUTC(string profileId, int notificationTemplateId, string substitutionsJson, ulong startTimeUTC, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PushNotificationSendParamProfileId.Value] = profileId;
		dictionary[OperationParam.PushNotificationSendParamNotificationTemplateId.Value] = notificationTemplateId;
		if (Util.IsOptionalParameterValid(substitutionsJson))
		{
			dictionary[OperationParam.PushNotificationSendParamSubstitutions.Value] = JsonReader.Deserialize<Dictionary<string, object>>(substitutionsJson);
		}
		dictionary[OperationParam.StartDateUTC.Value] = startTimeUTC;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PushNotification, ServiceOperation.ScheduleRichNotification, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ScheduleRichPushNotificationMinutes(string profileId, int notificationTemplateId, string substitutionsJson, int minutesFromNow, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PushNotificationSendParamProfileId.Value] = profileId;
		dictionary[OperationParam.PushNotificationSendParamNotificationTemplateId.Value] = notificationTemplateId;
		if (Util.IsOptionalParameterValid(substitutionsJson))
		{
			dictionary[OperationParam.PushNotificationSendParamSubstitutions.Value] = JsonReader.Deserialize<Dictionary<string, object>>(substitutionsJson);
		}
		dictionary[OperationParam.MinutesFromNow.Value] = minutesFromNow;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PushNotification, ServiceOperation.ScheduleRichNotification, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void SendNormalizedPushNotification(string toProfileId, string alertContentJson, string customDataJson, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PushNotificationSendParamToPlayerId.Value] = toProfileId;
		dictionary[OperationParam.AlertContent.Value] = JsonReader.Deserialize<Dictionary<string, object>>(alertContentJson);
		if (Util.IsOptionalParameterValid(customDataJson))
		{
			dictionary[OperationParam.CustomData.Value] = JsonReader.Deserialize<Dictionary<string, object>>(customDataJson);
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PushNotification, ServiceOperation.SendNormalized, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void SendNormalizedPushNotificationBatch(IList<string> profileIds, string alertContentJson, string customDataJson, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PushNotificationSendParamProfileIds.Value] = profileIds;
		dictionary[OperationParam.AlertContent.Value] = JsonReader.Deserialize<Dictionary<string, object>>(alertContentJson);
		if (Util.IsOptionalParameterValid(customDataJson))
		{
			dictionary[OperationParam.CustomData.Value] = JsonReader.Deserialize<Dictionary<string, object>>(customDataJson);
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PushNotification, ServiceOperation.SendNormalizedBatch, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	private void SendRichPushNotification(string toProfileId, int notificationTemplateId, string substitutionJson, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.PushNotificationSendParamToPlayerId.Value] = toProfileId;
		dictionary[OperationParam.PushNotificationSendParamNotificationTemplateId.Value] = notificationTemplateId;
		if (Util.IsOptionalParameterValid(substitutionJson))
		{
			dictionary[OperationParam.PushNotificationSendParamSubstitutions.Value] = JsonReader.Deserialize<Dictionary<string, object>>(substitutionJson);
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.PushNotification, ServiceOperation.SendRich, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}
}
