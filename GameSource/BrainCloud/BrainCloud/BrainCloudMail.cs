using System.Collections.Generic;
using BrainCloud.Internal;
using BrainCloud.JsonFx.Json;

namespace BrainCloud;

public class BrainCloudMail
{
	private BrainCloudClient _clientRef;

	public BrainCloudMail(BrainCloudClient client)
	{
		_clientRef = client;
	}

	public void SendBasicEmail(string profileId, string subject, string body, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.ProfileId.Value] = profileId;
		dictionary[OperationParam.Subject.Value] = subject;
		dictionary[OperationParam.Body.Value] = body;
		SendMessage(ServiceOperation.SendBasicEmail, dictionary, success, failure, cbObject);
	}

	public void SendAdvancedEmail(string profileId, string jsonServiceParams, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.ProfileId.Value] = profileId;
		dictionary[OperationParam.ServiceParams.Value] = JsonReader.Deserialize<Dictionary<string, object>>(jsonServiceParams);
		SendMessage(ServiceOperation.SendAdvancedEmail, dictionary, success, failure, cbObject);
	}

	public void SendAdvancedEmailByAddress(string emailAddress, string jsonServiceParams, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.EmailAddress.Value] = emailAddress;
		dictionary[OperationParam.ServiceParams.Value] = JsonReader.Deserialize<Dictionary<string, object>>(jsonServiceParams);
		SendMessage(ServiceOperation.SendAdvancedEmailByAddress, dictionary, success, failure, cbObject);
	}

	private void SendMessage(ServiceOperation operation, Dictionary<string, object> data, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		_clientRef.SendRequest(new ServerCall(ServiceName.Mail, operation, data, callback));
	}
}
