using System.Collections.Generic;
using BrainCloud.Internal;
using BrainCloud.JsonFx.Json;

namespace BrainCloud;

public class BrainCloudRedemptionCode
{
	private BrainCloudClient _client;

	public BrainCloudRedemptionCode(BrainCloudClient client)
	{
		_client = client;
	}

	public void RedeemCode(string scanCode, string codeType, string jsonCustomRedemptionInfo, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.RedemptionCodeServiceScanCode.Value] = scanCode;
		dictionary[OperationParam.RedemptionCodeServiceCodeType.Value] = codeType;
		if (Util.IsOptionalParameterValid(jsonCustomRedemptionInfo))
		{
			Dictionary<string, object> value = JsonReader.Deserialize<Dictionary<string, object>>(jsonCustomRedemptionInfo);
			dictionary[OperationParam.RedemptionCodeServiceCustomRedemptionInfo.Value] = value;
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.RedemptionCode, ServiceOperation.RedeemCode, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}

	public void GetRedeemedCodes(string codeType, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = null;
		if (Util.IsOptionalParameterValid(codeType))
		{
			dictionary = new Dictionary<string, object>();
			dictionary[OperationParam.RedemptionCodeServiceCodeType.Value] = codeType;
		}
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.RedemptionCode, ServiceOperation.GetRedeemedCodes, dictionary, callback);
		_client.SendRequest(serviceMessage);
	}
}
