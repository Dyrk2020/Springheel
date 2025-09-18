using System.Collections.Generic;
using BrainCloud.Internal;
using BrainCloud.JsonFx.Json;

namespace BrainCloud;

public class BrainCloudChat
{
	private BrainCloudClient m_clientRef;

	public BrainCloudChat(BrainCloudClient in_client)
	{
		m_clientRef = in_client;
	}

	public void ChannelConnect(string in_channelId, int in_maxToReturn, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.ChatChannelId.Value] = in_channelId;
		dictionary[OperationParam.ChatMaxReturn.Value] = in_maxToReturn;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Chat, ServiceOperation.ChannelConnect, dictionary, callback);
		m_clientRef.SendRequest(serviceMessage);
	}

	public void ChannelDisconnect(string in_channelId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.ChatChannelId.Value] = in_channelId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Chat, ServiceOperation.ChannelDisconnect, dictionary, callback);
		m_clientRef.SendRequest(serviceMessage);
	}

	public void DeleteChatMessage(string in_channelId, string in_messageId, int in_version, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.ChatChannelId.Value] = in_channelId;
		dictionary[OperationParam.ChatMessageId.Value] = in_messageId;
		dictionary[OperationParam.ChatVersion.Value] = in_version;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Chat, ServiceOperation.DeleteChatMessage, dictionary, callback);
		m_clientRef.SendRequest(serviceMessage);
	}

	public void GetChannelId(string in_channelType, string in_channelSubId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.ChatChannelType.Value] = in_channelType;
		dictionary[OperationParam.ChatChannelSubId.Value] = in_channelSubId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Chat, ServiceOperation.GetChannelId, dictionary, callback);
		m_clientRef.SendRequest(serviceMessage);
	}

	public void GetChannelInfo(string in_channelId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.ChatChannelId.Value] = in_channelId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Chat, ServiceOperation.GetChannelInfo, dictionary, callback);
		m_clientRef.SendRequest(serviceMessage);
	}

	public void GetChatMessage(string in_channelId, string in_messageId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.ChatChannelId.Value] = in_channelId;
		dictionary[OperationParam.ChatMessageId.Value] = in_messageId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Chat, ServiceOperation.GetChatMessage, dictionary, callback);
		m_clientRef.SendRequest(serviceMessage);
	}

	public void GetRecentChatMessages(string in_channelId, int in_maxToReturn, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.ChatChannelId.Value] = in_channelId;
		dictionary[OperationParam.ChatMaxReturn.Value] = in_maxToReturn;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Chat, ServiceOperation.GetRecentChatMessages, dictionary, callback);
		m_clientRef.SendRequest(serviceMessage);
	}

	public void GetSubscribedChannels(string in_channelType, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.ChatChannelType.Value] = in_channelType;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Chat, ServiceOperation.GetSubscribedChannels, dictionary, callback);
		m_clientRef.SendRequest(serviceMessage);
	}

	public void PostChatMessage(string in_channelId, string in_contentJson, bool in_recordInHistory = true, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.ChatChannelId.Value] = in_channelId;
		dictionary[OperationParam.ChatContent.Value] = JsonReader.Deserialize<Dictionary<string, object>>(in_contentJson);
		dictionary[OperationParam.ChatRecordInHistory.Value] = in_recordInHistory;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Chat, ServiceOperation.PostChatMessage, dictionary, callback);
		m_clientRef.SendRequest(serviceMessage);
	}

	public void PostChatMessage(string in_channelId, string in_plain, string in_jsonRich, bool in_recordInHistory = true, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
		dictionary2[OperationParam.ChatText.Value] = in_plain;
		if (Util.IsOptionalParameterValid(in_jsonRich))
		{
			Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(in_jsonRich);
			dictionary2[OperationParam.ChatRich.Value] = value;
		}
		else
		{
			Dictionary<string, object> value2 = JsonReader.Deserialize<Dictionary<string, object>>("{}");
			dictionary2[OperationParam.ChatRich.Value] = value2;
		}
		dictionary[OperationParam.ChatChannelId.Value] = in_channelId;
		dictionary[OperationParam.ChatContent.Value] = dictionary2;
		dictionary[OperationParam.ChatRecordInHistory.Value] = in_recordInHistory;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Chat, ServiceOperation.PostChatMessage, dictionary, callback);
		m_clientRef.SendRequest(serviceMessage);
	}

	public void PostChatMessageSimple(string in_channelId, string in_plain, bool in_recordInHistory = true, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.ChatChannelId.Value] = in_channelId;
		dictionary[OperationParam.ChatText.Value] = in_plain;
		dictionary[OperationParam.ChatRecordInHistory.Value] = in_recordInHistory;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Chat, ServiceOperation.PostChatMessageSimple, dictionary, callback);
		m_clientRef.SendRequest(serviceMessage);
	}

	public void UpdateChatMessage(string in_channelId, string in_messageId, int in_version, string in_contentJson, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.ChatChannelId.Value] = in_channelId;
		dictionary[OperationParam.ChatMessageId.Value] = in_messageId;
		dictionary[OperationParam.ChatVersion.Value] = in_version;
		dictionary[OperationParam.ChatContent.Value] = JsonReader.Deserialize<Dictionary<string, object>>(in_contentJson);
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Chat, ServiceOperation.UpdateChatMessage, dictionary, callback);
		m_clientRef.SendRequest(serviceMessage);
	}

	public void UpdateChatMessage(string in_channelId, string in_messageId, int in_version, string in_plain, string in_jsonRich, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.ChatText.Value] = in_plain;
		if (Util.IsOptionalParameterValid(in_jsonRich))
		{
			Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(in_jsonRich);
			dictionary[OperationParam.ChatRich.Value] = value;
		}
		else
		{
			Dictionary<string, object> value2 = JsonReader.Deserialize<Dictionary<string, object>>("{}");
			dictionary[OperationParam.ChatRich.Value] = value2;
		}
		Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
		dictionary2[OperationParam.ChatChannelId.Value] = in_channelId;
		dictionary2[OperationParam.ChatMessageId.Value] = in_messageId;
		dictionary2[OperationParam.ChatVersion.Value] = in_version;
		dictionary2[OperationParam.ChatContent.Value] = dictionary;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Chat, ServiceOperation.UpdateChatMessage, dictionary2, callback);
		m_clientRef.SendRequest(serviceMessage);
	}
}
