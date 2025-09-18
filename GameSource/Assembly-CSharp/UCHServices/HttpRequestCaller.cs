using System;
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using Relay;
using UnityEngine;
using UnityEngine.Networking;

namespace UCHServices;

public class HttpRequestCaller
{
	private const string HTTP_REQUEST_CALLER_TAG = "HttpRequestCaller";

	private const int HTTP_NUMBER_ATTEMPTS = 3;

	private static int[] TIME_BETWEEN_REQUESTS = new int[3] { 1000, 5000, 10000 };

	public static async UniTask<T> AsyncSendHttpRequest<T>(AbstractUCHServiceRequest<T> aRequest) where T : IMessage<T>, new()
	{
		return await DoSendHttpRequestAsync(aRequest, 3);
	}

	private static async UniTask<T> DoSendHttpRequestAsync<T>(AbstractUCHServiceRequest<T> aRequest, int aNumberAttempts) where T : IMessage<T>, new()
	{
		for (int i = 0; i < aNumberAttempts; i++)
		{
			using UnityWebRequest www = aRequest.GenerateRequest();
			Debug.Log("Sending http request to " + aRequest.Url + ", attempt # " + i);
			await www.SendWebRequest();
			if (www.responseCode >= 200 && www.responseCode < 300)
			{
				Debug.Log("Http request to " + aRequest.Url + ", responsed with http code " + www.responseCode);
				return www.GenerateResponse<T>();
			}
			if (www.responseCode >= 400 && www.responseCode <= 500)
			{
				ServiceError serviceError;
				try
				{
					serviceError = www.GenerateResponse<ServiceError>();
					Debug.LogError($"Received error from server. Status code {www.responseCode}, error : {serviceError.ErrorCode} Service : {UCHOnlineConnector.Service}");
				}
				catch (Exception)
				{
					Debug.LogError($"Call {aRequest.Url} failed. Unable to parse error from server. Status code {www.responseCode} Service : {UCHOnlineConnector.Service}");
					DebugLogCallFailed(aRequest, www);
					throw new ErrorMessageException(new ServiceError
					{
						ErrorCode = 0
					});
				}
				throw new ErrorMessageException(serviceError);
			}
			aNumberAttempts--;
			DebugLogCallFailed(aRequest, www);
			await UniTask.Delay(TIME_BETWEEN_REQUESTS[i]);
		}
		throw new ErrorMessageException(new ServiceError
		{
			ErrorCode = 0
		});
	}

	private static void DebugLogCallFailed<T>(AbstractUCHServiceRequest<T> aRequest, UnityWebRequest www) where T : IMessage<T>, new()
	{
		Debug.LogError("Call " + aRequest.Url + " failed. Response code = " + www.responseCode);
	}
}
