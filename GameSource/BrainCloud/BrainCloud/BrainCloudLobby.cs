using System;
using System.Collections;
using System.Collections.Generic;
using BrainCloud.Internal;
using BrainCloud.JsonFx.Json;
using UnityEngine.Networking;

namespace BrainCloud;

public class BrainCloudLobby
{
	private struct Failure
	{
		public FailureCallback callback;

		public int status;

		public int reasonCode;

		public string jsonError;

		public object cbObject;
	}

	private Dictionary<string, object> m_regionPingData = new Dictionary<string, object>();

	private Dictionary<string, object> m_lobbyTypeRegions = new Dictionary<string, object>();

	private Dictionary<string, List<long>> m_cachedPingResponses = new Dictionary<string, List<long>>();

	private List<KeyValuePair<string, string>> m_regionTargetsToProcess = new List<KeyValuePair<string, string>>();

	private SuccessCallback m_pingRegionSuccessCallback;

	private object m_pingRegionObject;

	private const int MAX_PING_CALLS = 4;

	private const int NUM_PING_CALLS_IN_PARRALLEL = 2;

	private List<Failure> m_failureQueue = new List<Failure>();

	private BrainCloudClient m_clientRef;

	public Dictionary<string, long> PingData { get; private set; }

	public BrainCloudLobby(BrainCloudClient in_client)
	{
		m_clientRef = in_client;
	}

	public void FindLobby(string in_roomType, int in_rating, int in_maxSteps, Dictionary<string, object> in_algo, Dictionary<string, object> in_filterJson, int in_timeoutSecs, bool in_isReady, Dictionary<string, object> in_extraJson, string in_teamCode, string[] in_otherUserCxIds = null, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.LobbyRoomType.Value] = in_roomType;
		dictionary[OperationParam.LobbyRating.Value] = in_rating;
		dictionary[OperationParam.LobbyMaxSteps.Value] = in_maxSteps;
		dictionary[OperationParam.LobbyAlgorithm.Value] = in_algo;
		dictionary[OperationParam.LobbyFilterJson.Value] = in_filterJson;
		dictionary[OperationParam.LobbyTimeoutSeconds.Value] = in_timeoutSecs;
		dictionary[OperationParam.LobbyIsReady.Value] = in_isReady;
		if (in_otherUserCxIds != null)
		{
			dictionary[OperationParam.LobbyOtherUserCxIds.Value] = in_otherUserCxIds;
		}
		dictionary[OperationParam.LobbyExtraJson.Value] = in_extraJson;
		dictionary[OperationParam.LobbyTeamCode.Value] = in_teamCode;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Lobby, ServiceOperation.FindLobby, dictionary, callback);
		m_clientRef.SendRequest(serviceMessage);
	}

	public void FindLobbyWithPingData(string in_roomType, int in_rating, int in_maxSteps, Dictionary<string, object> in_algo, Dictionary<string, object> in_filterJson, int in_timeoutSecs, bool in_isReady, Dictionary<string, object> in_extraJson, string in_teamCode, string[] in_otherUserCxIds = null, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.LobbyRoomType.Value] = in_roomType;
		dictionary[OperationParam.LobbyRating.Value] = in_rating;
		dictionary[OperationParam.LobbyMaxSteps.Value] = in_maxSteps;
		dictionary[OperationParam.LobbyAlgorithm.Value] = in_algo;
		dictionary[OperationParam.LobbyFilterJson.Value] = in_filterJson;
		dictionary[OperationParam.LobbyTimeoutSeconds.Value] = in_timeoutSecs;
		dictionary[OperationParam.LobbyIsReady.Value] = in_isReady;
		if (in_otherUserCxIds != null)
		{
			dictionary[OperationParam.LobbyOtherUserCxIds.Value] = in_otherUserCxIds;
		}
		dictionary[OperationParam.LobbyExtraJson.Value] = in_extraJson;
		dictionary[OperationParam.LobbyTeamCode.Value] = in_teamCode;
		attachPingDataAndSend(dictionary, ServiceOperation.FindLobbyWithPingData, success, failure, cbObject);
	}

	public void CreateLobby(string in_roomType, int in_rating, bool in_isReady, Dictionary<string, object> in_extraJson, string in_teamCode, Dictionary<string, object> in_settings, string[] in_otherUserCxIds = null, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.LobbyRoomType.Value] = in_roomType;
		dictionary[OperationParam.LobbyRating.Value] = in_rating;
		dictionary[OperationParam.LobbySettings.Value] = in_settings;
		dictionary[OperationParam.LobbyIsReady.Value] = in_isReady;
		if (in_otherUserCxIds != null)
		{
			dictionary[OperationParam.LobbyOtherUserCxIds.Value] = in_otherUserCxIds;
		}
		dictionary[OperationParam.LobbyExtraJson.Value] = in_extraJson;
		dictionary[OperationParam.LobbyTeamCode.Value] = in_teamCode;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Lobby, ServiceOperation.CreateLobby, dictionary, callback);
		m_clientRef.SendRequest(serviceMessage);
	}

	public void CreateLobbyWithPingData(string in_roomType, int in_rating, bool in_isReady, Dictionary<string, object> in_extraJson, string in_teamCode, Dictionary<string, object> in_settings, string[] in_otherUserCxIds = null, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.LobbyRoomType.Value] = in_roomType;
		dictionary[OperationParam.LobbyRating.Value] = in_rating;
		dictionary[OperationParam.LobbySettings.Value] = in_settings;
		dictionary[OperationParam.LobbyIsReady.Value] = in_isReady;
		if (in_otherUserCxIds != null)
		{
			dictionary[OperationParam.LobbyOtherUserCxIds.Value] = in_otherUserCxIds;
		}
		dictionary[OperationParam.LobbyExtraJson.Value] = in_extraJson;
		dictionary[OperationParam.LobbyTeamCode.Value] = in_teamCode;
		attachPingDataAndSend(dictionary, ServiceOperation.CreateLobbyWithPingData, success, failure, cbObject);
	}

	public void FindOrCreateLobby(string in_roomType, int in_rating, int in_maxSteps, Dictionary<string, object> in_algo, Dictionary<string, object> in_filterJson, int in_timeoutSecs, bool in_isReady, Dictionary<string, object> in_extraJson, string in_teamCode, Dictionary<string, object> in_settings, string[] in_otherUserCxIds = null, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.LobbyRoomType.Value] = in_roomType;
		dictionary[OperationParam.LobbyRating.Value] = in_rating;
		dictionary[OperationParam.LobbyMaxSteps.Value] = in_maxSteps;
		dictionary[OperationParam.LobbyAlgorithm.Value] = in_algo;
		dictionary[OperationParam.LobbyFilterJson.Value] = in_filterJson;
		dictionary[OperationParam.LobbyTimeoutSeconds.Value] = in_timeoutSecs;
		dictionary[OperationParam.LobbySettings.Value] = in_settings;
		dictionary[OperationParam.LobbyIsReady.Value] = in_isReady;
		if (in_otherUserCxIds != null)
		{
			dictionary[OperationParam.LobbyOtherUserCxIds.Value] = in_otherUserCxIds;
		}
		dictionary[OperationParam.LobbyExtraJson.Value] = in_extraJson;
		dictionary[OperationParam.LobbyTeamCode.Value] = in_teamCode;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Lobby, ServiceOperation.FindOrCreateLobby, dictionary, callback);
		m_clientRef.SendRequest(serviceMessage);
	}

	public void FindOrCreateLobbyWithPingData(string in_roomType, int in_rating, int in_maxSteps, Dictionary<string, object> in_algo, Dictionary<string, object> in_filterJson, int in_timeoutSecs, bool in_isReady, Dictionary<string, object> in_extraJson, string in_teamCode, Dictionary<string, object> in_settings, string[] in_otherUserCxIds = null, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.LobbyRoomType.Value] = in_roomType;
		dictionary[OperationParam.LobbyRating.Value] = in_rating;
		dictionary[OperationParam.LobbyMaxSteps.Value] = in_maxSteps;
		dictionary[OperationParam.LobbyAlgorithm.Value] = in_algo;
		dictionary[OperationParam.LobbyFilterJson.Value] = in_filterJson;
		dictionary[OperationParam.LobbyTimeoutSeconds.Value] = in_timeoutSecs;
		dictionary[OperationParam.LobbySettings.Value] = in_settings;
		dictionary[OperationParam.LobbyIsReady.Value] = in_isReady;
		if (in_otherUserCxIds != null)
		{
			dictionary[OperationParam.LobbyOtherUserCxIds.Value] = in_otherUserCxIds;
		}
		dictionary[OperationParam.LobbyExtraJson.Value] = in_extraJson;
		dictionary[OperationParam.LobbyTeamCode.Value] = in_teamCode;
		attachPingDataAndSend(dictionary, ServiceOperation.FindOrCreateLobbyWithPingData, success, failure, cbObject);
	}

	public void GetLobbyData(string in_lobbyID, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.LobbyIdentifier.Value] = in_lobbyID;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Lobby, ServiceOperation.GetLobbyData, dictionary, callback);
		m_clientRef.SendRequest(serviceMessage);
	}

	public void UpdateReady(string in_lobbyID, bool in_isReady, Dictionary<string, object> in_extraJson, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.LobbyIdentifier.Value] = in_lobbyID;
		dictionary[OperationParam.LobbyIsReady.Value] = in_isReady;
		dictionary[OperationParam.LobbyExtraJson.Value] = in_extraJson;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Lobby, ServiceOperation.UpdateReady, dictionary, callback);
		m_clientRef.SendRequest(serviceMessage);
	}

	public void UpdateSettings(string in_lobbyID, Dictionary<string, object> in_settings, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.LobbyIdentifier.Value] = in_lobbyID;
		dictionary[OperationParam.LobbySettings.Value] = in_settings;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Lobby, ServiceOperation.UpdateSettings, dictionary, callback);
		m_clientRef.SendRequest(serviceMessage);
	}

	public void SwitchTeam(string in_lobbyID, string in_toTeamName, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.LobbyIdentifier.Value] = in_lobbyID;
		dictionary[OperationParam.LobbyToTeamName.Value] = in_toTeamName;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Lobby, ServiceOperation.SwitchTeam, dictionary, callback);
		m_clientRef.SendRequest(serviceMessage);
	}

	public void SendSignal(string in_lobbyID, Dictionary<string, object> in_signalData, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.LobbyIdentifier.Value] = in_lobbyID;
		dictionary[OperationParam.LobbySignalData.Value] = in_signalData;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Lobby, ServiceOperation.SendSignal, dictionary, callback);
		m_clientRef.SendRequest(serviceMessage);
	}

	public void JoinLobby(string in_lobbyID, bool in_isReady, Dictionary<string, object> in_extraJson, string in_teamCode, string[] in_otherUserCxIds = null, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (in_otherUserCxIds != null)
		{
			dictionary[OperationParam.LobbyOtherUserCxIds.Value] = in_otherUserCxIds;
		}
		dictionary[OperationParam.LobbyExtraJson.Value] = in_extraJson;
		dictionary[OperationParam.LobbyTeamCode.Value] = in_teamCode;
		dictionary[OperationParam.LobbyIdentifier.Value] = in_lobbyID;
		dictionary[OperationParam.LobbyIsReady.Value] = in_isReady;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Lobby, ServiceOperation.JoinLobby, dictionary, callback);
		m_clientRef.SendRequest(serviceMessage);
	}

	public void JoinLobbyWithPingData(string in_lobbyID, bool in_isReady, Dictionary<string, object> in_extraJson, string in_teamCode, string[] in_otherUserCxIds = null, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (in_otherUserCxIds != null)
		{
			dictionary[OperationParam.LobbyOtherUserCxIds.Value] = in_otherUserCxIds;
		}
		dictionary[OperationParam.LobbyExtraJson.Value] = in_extraJson;
		dictionary[OperationParam.LobbyTeamCode.Value] = in_teamCode;
		dictionary[OperationParam.LobbyIdentifier.Value] = in_lobbyID;
		dictionary[OperationParam.LobbyIsReady.Value] = in_isReady;
		attachPingDataAndSend(dictionary, ServiceOperation.JoinLobbyWithPingData, success, failure, cbObject);
	}

	public void LeaveLobby(string in_lobbyID, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.LobbyIdentifier.Value] = in_lobbyID;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Lobby, ServiceOperation.LeaveLobby, dictionary, callback);
		m_clientRef.SendRequest(serviceMessage);
	}

	public void RemoveMember(string in_lobbyID, string in_connectionId, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.LobbyIdentifier.Value] = in_lobbyID;
		dictionary[OperationParam.LobbyConnectionId.Value] = in_connectionId;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Lobby, ServiceOperation.RemoveMember, dictionary, callback);
		m_clientRef.SendRequest(serviceMessage);
	}

	public void CancelFindRequest(string in_roomType, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.LobbyRoomType.Value] = in_roomType;
		dictionary[OperationParam.LobbyConnectionId.Value] = m_clientRef.RTTConnectionID;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Lobby, ServiceOperation.CancelFindRequest, dictionary, callback);
		m_clientRef.SendRequest(serviceMessage);
	}

	public void GetRegionsForLobbies(string[] in_roomTypes, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.LobbyTypes.Value] = in_roomTypes;
		ServerCallback callback = BrainCloudClient.CreateServerCallback((SuccessCallback)Delegate.Combine(new SuccessCallback(onRegionForLobbiesSuccess), success), failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Lobby, ServiceOperation.GetRegionsForLobbies, dictionary, callback);
		m_clientRef.SendRequest(serviceMessage);
	}

	public void GetLobbyInstances(string in_lobbyType, Dictionary<string, object> criteriaJson, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.LobbyRoomType.Value] = in_lobbyType;
		dictionary[OperationParam.LobbyCritera.Value] = criteriaJson;
		ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
		ServerCall serviceMessage = new ServerCall(ServiceName.Lobby, ServiceOperation.GetLobbyInstances, dictionary, callback);
		m_clientRef.SendRequest(serviceMessage);
	}

	public void GetLobbyInstancesWithPingData(string in_lobbyType, Dictionary<string, object> criteriaJson, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[OperationParam.LobbyRoomType.Value] = in_lobbyType;
		dictionary[OperationParam.LobbyCritera.Value] = criteriaJson;
		attachPingDataAndSend(dictionary, ServiceOperation.GetLobbyInstancesWithPingData, success, failure, cbObject);
	}

	public void PingRegions(SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		if (m_pingRegionSuccessCallback != null)
		{
			queueFailure(failure, 40358, "Ping is already happening.", cbObject);
			return;
		}
		PingData = new Dictionary<string, long>();
		Dictionary<string, object> dictionary = null;
		string text = "";
		if (m_regionPingData.Count > 0)
		{
			m_pingRegionSuccessCallback = success;
			m_pingRegionObject = cbObject;
			foreach (KeyValuePair<string, object> regionPingDatum in m_regionPingData)
			{
				dictionary = (Dictionary<string, object>)regionPingDatum.Value;
				if (!dictionary.ContainsKey("type") || !(dictionary["type"] as string == "PING"))
				{
					continue;
				}
				m_cachedPingResponses[regionPingDatum.Key] = new List<long>();
				text = (string)dictionary["target"];
				lock (m_regionTargetsToProcess)
				{
					for (int i = 0; i < 4; i++)
					{
						m_regionTargetsToProcess.Add(new KeyValuePair<string, string>(regionPingDatum.Key, text));
					}
				}
			}
			pingNextItemToProcess();
		}
		else
		{
			queueFailure(failure, 40358, "No Regions to Ping. Please call GetRegionsForLobbies and await the response before calling PingRegions.", cbObject);
		}
	}

	private void pingNextItemToProcess()
	{
		lock (m_regionTargetsToProcess)
		{
			if (m_regionTargetsToProcess.Count > 0)
			{
				for (int i = 0; i < 2; i++)
				{
					if (m_regionTargetsToProcess.Count <= 0)
					{
						break;
					}
					KeyValuePair<string, string> keyValuePair = m_regionTargetsToProcess[0];
					m_regionTargetsToProcess.RemoveAt(0);
					pingHost(keyValuePair.Key, keyValuePair.Value);
				}
			}
			else if (m_regionPingData.Count == PingData.Count && m_pingRegionSuccessCallback != null)
			{
				string text = JsonWriter.Serialize(PingData);
				if (m_clientRef.LoggingEnabled)
				{
					m_clientRef.Log("PINGS: " + text);
				}
				m_pingRegionSuccessCallback(text, m_pingRegionObject);
				m_pingRegionSuccessCallback = null;
			}
		}
	}

	private void attachPingDataAndSend(Dictionary<string, object> in_data, ServiceOperation in_operation, SuccessCallback success = null, FailureCallback failure = null, object cbObject = null)
	{
		if (PingData != null && PingData.Count > 0)
		{
			in_data[OperationParam.PingData.Value] = PingData;
			ServerCallback callback = BrainCloudClient.CreateServerCallback(success, failure, cbObject);
			ServerCall serviceMessage = new ServerCall(ServiceName.Lobby, in_operation, in_data, callback);
			m_clientRef.SendRequest(serviceMessage);
		}
		else
		{
			queueFailure(failure, 40358, "Processing exception (message): Required message parameter 'pingData' is missing.  Please ensure PingData exists by first calling GetRegionsForLobbies and PingRegions, and waiting for response before proceeding.", cbObject);
		}
	}

	private void queueFailure(FailureCallback in_failure, int reasonCode, string status_message, object cbObject = null)
	{
		if (in_failure != null)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary["reason_code"] = reasonCode;
			dictionary["status"] = 400;
			dictionary["status_message"] = status_message;
			dictionary["severity"] = "ERROR";
			Failure item = new Failure
			{
				callback = in_failure,
				status = 400,
				reasonCode = reasonCode,
				jsonError = JsonWriter.Serialize(dictionary),
				cbObject = cbObject
			};
			m_failureQueue.Add(item);
		}
	}

	public void Update()
	{
		for (int i = 0; i < m_failureQueue.Count; i++)
		{
			Failure failure = m_failureQueue[i];
			failure.callback(failure.status, failure.reasonCode, failure.jsonError, failure.cbObject);
		}
		m_failureQueue.Clear();
	}

	private void onRegionForLobbiesSuccess(string in_json, object in_obj)
	{
		PingData = new Dictionary<string, long>();
		Dictionary<string, object> dictionary = (Dictionary<string, object>)((Dictionary<string, object>)JsonReader.Deserialize(in_json))["data"];
		m_regionPingData = (Dictionary<string, object>)dictionary["regionPingData"];
		m_lobbyTypeRegions = (Dictionary<string, object>)dictionary["lobbyTypeRegions"];
	}

	private void pingHost(string in_region, string in_target)
	{
		in_target = "http://" + in_target;
		if (m_clientRef.Wrapper != null)
		{
			m_clientRef.Wrapper.StartCoroutine(HandlePingReponse(in_region, in_target));
		}
	}

	private IEnumerator HandlePingReponse(string in_region, string in_target)
	{
		long sentPing = DateTime.Now.Ticks;
		UnityWebRequest _request = UnityWebRequest.Get(in_target);
		yield return _request.SendWebRequest();
		if (_request.error == null && !_request.isNetworkError)
		{
			handlePingTimeResponse((DateTime.Now.Ticks - sentPing) / 10000, in_region);
		}
	}

	private void handlePingTimeResponse(long in_responseTime, string in_region)
	{
		m_cachedPingResponses[in_region].Add(in_responseTime);
		if (m_cachedPingResponses[in_region].Count == 4)
		{
			long num = 0L;
			long num2 = 0L;
			foreach (long item in m_cachedPingResponses[in_region])
			{
				num += item;
				if (item > num2)
				{
					num2 = item;
				}
			}
			num -= num2;
			PingData[in_region] = num / (m_cachedPingResponses[in_region].Count - 1);
		}
		pingNextItemToProcess();
	}
}
