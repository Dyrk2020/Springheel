using System.Collections.Generic;
using BrainCloud.Internal;
using BrainCloud.JsonFx.Json;

namespace BrainCloud;

public class BrainCloudMessaging
{
	private BrainCloudClient m_clientRef;

	public BrainCloudMessaging(BrainCloudClient in_client)
	{
		m_clientRef = in_client;
	}

	public void DeleteMessages(string in_msgBox, string[] in_msgsIds, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.MessagingMessageBox.Value] = in_msgBox;
		dictionary[OperationParam.MessagingMessageIds.Value] = in_msgsIds;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Messaging, ServiceOperation.DeleteMessages, dictionary, callback);
		m_clientRef.SendRequest(serviceMessage);
	}

	public void GetMessageboxes(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Messaging, ServiceOperation.GetMessageBoxes, null, callback);
		m_clientRef.SendRequest(serviceMessage);
	}

	public void GetMessageCounts(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Messaging, ServiceOperation.GetMessageCounts, null, callback);
		m_clientRef.SendRequest(serviceMessage);
	}

	public void GetMessages(string in_msgBox, string[] in_msgsIds, bool markAsRead, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.MessagingMessageBox.Value] = in_msgBox;
		dictionary[OperationParam.MessagingMessageIds.Value] = in_msgsIds;
		dictionary[OperationParam.MessagingMarkAsRead.Value] = markAsRead;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Messaging, ServiceOperation.GetMessages, dictionary, callback);
		m_clientRef.SendRequest(serviceMessage);
	}

	public void GetMessagesPage(string in_context, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(in_context);
		dictionary[OperationParam.MessagingContext.Value] = value;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Messaging, ServiceOperation.GetMessagesPage, dictionary, callback);
		m_clientRef.SendRequest(serviceMessage);
	}

	public void GetMessagesPageOffset(string in_context, int pageOffset, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.MessagingContext.Value] = in_context;
		dictionary[OperationParam.MessagingPageOffset.Value] = pageOffset;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Messaging, ServiceOperation.GetMessagesPageOffset, dictionary, callback);
		m_clientRef.SendRequest(serviceMessage);
	}

	public void MarkMessagesRead(string in_msgBox, string[] in_msgsIds, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.MessagingMessageBox.Value] = in_msgBox;
		dictionary[OperationParam.MessagingMessageIds.Value] = in_msgsIds;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Messaging, ServiceOperation.MarkMessagesRead, dictionary, callback);
		m_clientRef.SendRequest(serviceMessage);
	}

	public void SendMessage(string[] in_toProfileIds, string in_contentJson, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.MessagingToProfileIds.Value] = in_toProfileIds;
		Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(in_contentJson);
		dictionary[OperationParam.MessagingContent.Value] = value;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Messaging, ServiceOperation.SendMessage, dictionary, callback);
		m_clientRef.SendRequest(serviceMessage);
	}

	public void SendMessageSimple(string[] in_toProfileIds, string in_messageText, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.MessagingToProfileIds.Value] = in_toProfileIds;
		dictionary[OperationParam.MessagingText.Value] = in_messageText;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Messaging, ServiceOperation.SendMessageSimple, dictionary, callback);
		m_clientRef.SendRequest(serviceMessage);
	}
}
