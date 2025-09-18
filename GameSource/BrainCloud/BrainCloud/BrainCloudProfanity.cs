using System.Collections.Generic;
using BrainCloud.Internal;

namespace BrainCloud;

public class BrainCloudProfanity
{
	private BrainCloudClient _client;

	public BrainCloudProfanity(BrainCloudClient client)
	{
		_client = client;
	}

	public void ProfanityCheck(string text, string languages, bool flagEmail, bool flagPhone, bool flagUrls, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.ProfanityText.Value] = text;
		if (languages != null)
		{
			dictionary[OperationParam.ProfanityLanguages.Value] = languages;
		}
		dictionary[OperationParam.ProfanityFlagEmail.Value] = flagEmail;
		dictionary[OperationParam.ProfanityFlagPhone.Value] = flagPhone;
		dictionary[OperationParam.ProfanityFlagUrls.Value] = flagUrls;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Profanity, ServiceOperation.ProfanityCheck, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ProfanityReplaceText(string text, string replaceSymbol, string languages, bool flagEmail, bool flagPhone, bool flagUrls, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.ProfanityText.Value] = text;
		dictionary[OperationParam.ProfanityReplaceSymbol.Value] = replaceSymbol;
		if (languages != null)
		{
			dictionary[OperationParam.ProfanityLanguages.Value] = languages;
		}
		dictionary[OperationParam.ProfanityFlagEmail.Value] = flagEmail;
		dictionary[OperationParam.ProfanityFlagPhone.Value] = flagPhone;
		dictionary[OperationParam.ProfanityFlagUrls.Value] = flagUrls;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Profanity, ServiceOperation.ProfanityReplaceText, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void ProfanityIdentifyBadWords(string text, string languages, bool flagEmail, bool flagPhone, bool flagUrls, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.ProfanityText.Value] = text;
		if (languages != null)
		{
			dictionary[OperationParam.ProfanityLanguages.Value] = languages;
		}
		dictionary[OperationParam.ProfanityFlagEmail.Value] = flagEmail;
		dictionary[OperationParam.ProfanityFlagPhone.Value] = flagPhone;
		dictionary[OperationParam.ProfanityFlagUrls.Value] = flagUrls;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Profanity, ServiceOperation.ProfanityIdentifyBadWords, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}
}
