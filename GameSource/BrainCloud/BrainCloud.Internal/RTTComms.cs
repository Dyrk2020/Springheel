using System;
using System.Collections.Generic;
using System.Text;
using BrainCloud.JsonFx.Json;

namespace BrainCloud.Internal;

internal sealed class RTTComms
{
	private struct RTTCommandResponse
	{
		public string Service { get; set; }

		public string Operation { get; set; }

		public string JsonMessage { get; set; }

		public RTTCommandResponse(string in_service, string in_op, string in_msg)
		{
			Service = in_service;
			Operation = in_op;
			JsonMessage = in_msg;
		}
	}

	private bool m_disconnectedWithReason;

	private Dictionary<string, object> m_disconnectJson = new Dictionary<string, object>();

	private Dictionary<string, object> m_endpoint;

	private RTTConnectionType m_currentConnectionType;

	private BrainCloudWebSocket m_webSocket;

	private TimeSpan m_sinceLastHeartbeat;

	private const int MAX_PACKETSIZE = 1024;

	private TimeSpan m_heartBeatTime = TimeSpan.FromMilliseconds(10000.0);

	private BrainCloudClient m_clientRef;

	private SuccessCallback m_connectedSuccessCallback;

	private FailureCallback m_connectionFailureCallback;

	private object m_connectedObj;

	private Dictionary<string, object> m_rttHeaders = new Dictionary<string, object>();

	private Dictionary<string, RTTCallback> m_registeredCallbacks = new Dictionary<string, RTTCallback>();

	private List<RTTCommandResponse> m_queuedRTTCommands = new List<RTTCommandResponse>();

	private WebsocketStatus m_webSocketStatus = WebsocketStatus.NONE;

	private RTTConnectionStatus m_rttConnectionStatus = RTTConnectionStatus.DISCONNECTED;

	public string RTTConnectionID { get; private set; }

	public string RTTEventServer { get; private set; }

	public RTTComms(BrainCloudClient in_client)
	{
		m_clientRef = in_client;
	}

	public void EnableRTT(RTTConnectionType in_connectionType = RTTConnectionType.WEBSOCKET, SuccessCallback in_success = null, FailureCallback in_failure = null, object cb_object = null)
	{
		m_disconnectedWithReason = false;
		if (!IsRTTEnabled() && m_rttConnectionStatus != RTTConnectionStatus.CONNECTING)
		{
			m_connectedSuccessCallback = in_success;
			m_connectionFailureCallback = in_failure;
			m_connectedObj = cb_object;
			m_currentConnectionType = in_connectionType;
			m_clientRef.RTTService.RequestClientConnection(rttConnectionServerSuccess, rttConnectionServerError, cb_object);
		}
	}

	public void DisableRTT()
	{
		if (IsRTTEnabled() && m_rttConnectionStatus != RTTConnectionStatus.DISCONNECTING)
		{
			addRTTCommandResponse(new RTTCommandResponse(ServiceName.RTTRegistration.Value.ToLower(), "disconnect", "DisableRTT Called"));
		}
	}

	public bool IsRTTEnabled()
	{
		return m_rttConnectionStatus == RTTConnectionStatus.CONNECTED;
	}

	public RTTConnectionStatus GetConnectionStatus()
	{
		return m_rttConnectionStatus;
	}

	public void RegisterRTTCallback(ServiceName in_serviceName, RTTCallback in_callback)
	{
		m_registeredCallbacks[in_serviceName.Value.ToLower()] = in_callback;
	}

	public void DeregisterRTTCallback(ServiceName in_serviceName)
	{
		string key = in_serviceName.Value.ToLower();
		if (m_registeredCallbacks.ContainsKey(key))
		{
			m_registeredCallbacks.Remove(key);
		}
	}

	public void DeregisterAllRTTCallbacks()
	{
		m_registeredCallbacks.Clear();
	}

	public void SetRTTHeartBeatSeconds(int in_value)
	{
		m_heartBeatTime = TimeSpan.FromMilliseconds(in_value * 1000);
	}

	public void Update()
	{
		lock (m_queuedRTTCommands)
		{
			for (int i = 0; i < m_queuedRTTCommands.Count; i++)
			{
				RTTCommandResponse rTTCommandResponse = m_queuedRTTCommands[i];
				if (m_webSocketStatus == WebsocketStatus.CLOSED)
				{
					m_connectionFailureCallback(400, -1, "RTT Connection has been closed. Re-Enable RTT to re-establish connection : " + rTTCommandResponse.JsonMessage, m_connectedObj);
					m_rttConnectionStatus = RTTConnectionStatus.DISCONNECTING;
					disconnect();
					break;
				}
				if (m_webSocketStatus == WebsocketStatus.CLOSED)
				{
					m_connectionFailureCallback(400, -1, "RTT Connection has been closed. Re-Enable RTT to re-establish connection : " + rTTCommandResponse.JsonMessage, m_connectedObj);
					m_rttConnectionStatus = RTTConnectionStatus.DISCONNECTING;
					disconnect();
					break;
				}
				if (m_registeredCallbacks.ContainsKey(rTTCommandResponse.Service))
				{
					m_registeredCallbacks[rTTCommandResponse.Service](rTTCommandResponse.JsonMessage);
				}
				else if (m_rttConnectionStatus == RTTConnectionStatus.CONNECTING && m_connectedSuccessCallback != null && rTTCommandResponse.Operation == "connect")
				{
					m_sinceLastHeartbeat = DateTime.Now.TimeOfDay;
					m_connectedSuccessCallback(rTTCommandResponse.JsonMessage, m_connectedObj);
					m_rttConnectionStatus = RTTConnectionStatus.CONNECTED;
				}
				else if (m_rttConnectionStatus == RTTConnectionStatus.CONNECTED && m_connectionFailureCallback != null && rTTCommandResponse.Operation == "disconnect")
				{
					m_rttConnectionStatus = RTTConnectionStatus.DISCONNECTING;
					disconnect();
				}
				else if (m_connectionFailureCallback != null && rTTCommandResponse.Operation == "error")
				{
					if (rTTCommandResponse.JsonMessage != null)
					{
						Dictionary<string, object> dictionary = (Dictionary<string, object>)JsonReader.Deserialize(rTTCommandResponse.JsonMessage);
						if (dictionary.ContainsKey("status") && dictionary.ContainsKey("reason_code"))
						{
							m_connectionFailureCallback((int)dictionary["status"], (int)dictionary["reason_code"], rTTCommandResponse.JsonMessage, m_connectedObj);
						}
						else
						{
							m_connectionFailureCallback(400, -1, rTTCommandResponse.JsonMessage, m_connectedObj);
						}
					}
					else
					{
						m_connectionFailureCallback(400, -1, "Error - No Response from Server", m_connectedObj);
					}
				}
				else if (m_rttConnectionStatus == RTTConnectionStatus.DISCONNECTED && rTTCommandResponse.Operation == "connect")
				{
					m_rttConnectionStatus = RTTConnectionStatus.CONNECTING;
					send(buildConnectionRequest());
				}
				else if (m_clientRef.LoggingEnabled)
				{
					m_clientRef.Log("WARNING no handler registered for RTT callbacks ");
				}
			}
			m_queuedRTTCommands.Clear();
		}
		if (m_rttConnectionStatus == RTTConnectionStatus.CONNECTED && DateTime.Now.TimeOfDay - m_sinceLastHeartbeat >= m_heartBeatTime)
		{
			m_sinceLastHeartbeat = DateTime.Now.TimeOfDay;
			send(buildHeartbeatRequest());
		}
	}

	private void connectWebSocket()
	{
		if (m_rttConnectionStatus == RTTConnectionStatus.DISCONNECTED)
		{
			startReceivingWebSocket();
		}
	}

	private void disconnect()
	{
		if (m_webSocket != null)
		{
			m_webSocket.Close();
		}
		RTTConnectionID = "";
		RTTEventServer = "";
		m_webSocket = null;
		if (m_disconnectedWithReason)
		{
			if (m_clientRef.LoggingEnabled)
			{
				m_clientRef.Log("RTT: Disconnect: " + JsonWriter.Serialize(m_disconnectJson));
			}
			if (m_connectionFailureCallback != null)
			{
				m_connectionFailureCallback(400, (int)m_disconnectJson["reason_code"], (string)m_disconnectJson["reason"], m_connectedObj);
			}
		}
		m_rttConnectionStatus = RTTConnectionStatus.DISCONNECTED;
	}

	private string buildConnectionRequest()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["platform"] = m_clientRef.ReleasePlatform.ToString();
		dictionary["protocol"] = "ws";
		Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
		dictionary2["appId"] = m_clientRef.AppId;
		dictionary2["sessionId"] = m_clientRef.SessionID;
		dictionary2["profileId"] = m_clientRef.ProfileId;
		dictionary2["system"] = dictionary;
		dictionary2["auth"] = m_rttHeaders;
		return JsonWriter.Serialize(new Dictionary<string, object>
		{
			["service"] = ServiceName.RTT.Value,
			["operation"] = "CONNECT",
			["data"] = dictionary2
		});
	}

	private string buildHeartbeatRequest()
	{
		return JsonWriter.Serialize(new Dictionary<string, object>
		{
			["service"] = ServiceName.RTT.Value,
			["operation"] = "HEARTBEAT",
			["data"] = null
		});
	}

	private bool send(string in_message, bool in_bLogMessage = true)
	{
		bool result = false;
		bool flag = m_currentConnectionType == RTTConnectionType.WEBSOCKET;
		if (flag && m_webSocket == null)
		{
			return result;
		}
		try
		{
			if (in_bLogMessage && m_clientRef.LoggingEnabled)
			{
				m_clientRef.Log("RTT SEND: " + in_message);
			}
			if (flag)
			{
				byte[] bytes = Encoding.ASCII.GetBytes(in_message);
				m_webSocket.SendAsync(bytes);
			}
		}
		catch (Exception ex)
		{
			if (m_clientRef.LoggingEnabled)
			{
				m_clientRef.Log("send exception: " + ex);
			}
			addRTTCommandResponse(new RTTCommandResponse(ServiceName.RTTRegistration.Value.ToLower(), "error", buildRTTRequestError(ex.ToString())));
		}
		return result;
	}

	private void startReceivingWebSocket()
	{
		bool flag = (bool)m_endpoint["ssl"];
		string in_url = (flag ? "wss://" : "ws://") + m_endpoint["host"]?.ToString() + ":" + (int)m_endpoint["port"] + getUrlQueryParameters();
		setupWebSocket(in_url);
	}

	private string getUrlQueryParameters()
	{
		string text = "?";
		int num = 0;
		foreach (KeyValuePair<string, object> rttHeader in m_rttHeaders)
		{
			if (num > 0)
			{
				text += "&";
			}
			text = text + rttHeader.Key + "=" + rttHeader.Value;
			num++;
		}
		return text;
	}

	private void setupWebSocket(string in_url)
	{
		m_webSocket = new BrainCloudWebSocket(in_url);
		m_webSocket.OnClose += WebSocket_OnClose;
		m_webSocket.OnOpen += Websocket_OnOpen;
		m_webSocket.OnMessage += WebSocket_OnMessage;
		m_webSocket.OnError += WebSocket_OnError;
	}

	private void WebSocket_OnClose(BrainCloudWebSocket sender, int code, string reason)
	{
		if (m_clientRef.LoggingEnabled)
		{
			m_clientRef.Log("RTT: Connection closed: " + reason);
		}
		m_webSocketStatus = WebsocketStatus.CLOSED;
		addRTTCommandResponse(new RTTCommandResponse(ServiceName.RTTRegistration.Value.ToLower(), "disconnect", reason));
	}

	private void Websocket_OnOpen(BrainCloudWebSocket accepted)
	{
		if (m_clientRef.LoggingEnabled)
		{
			m_clientRef.Log("RTT: Connection established.");
		}
		m_webSocketStatus = WebsocketStatus.OPEN;
		addRTTCommandResponse(new RTTCommandResponse(ServiceName.RTTRegistration.Value.ToLower(), "connect", ""));
	}

	private void WebSocket_OnMessage(BrainCloudWebSocket sender, byte[] data)
	{
		if (data.Length != 0)
		{
			m_webSocketStatus = WebsocketStatus.MESSAGE;
			string in_message = Encoding.UTF8.GetString(data);
			onRecv(in_message);
		}
	}

	private void WebSocket_OnError(BrainCloudWebSocket sender, string message)
	{
		if (m_clientRef.LoggingEnabled)
		{
			m_clientRef.Log("RTT Error: " + message);
		}
		m_webSocketStatus = WebsocketStatus.ERROR;
		addRTTCommandResponse(new RTTCommandResponse(ServiceName.RTTRegistration.Value.ToLower(), "error", buildRTTRequestError(message)));
	}

	private void onRecv(string in_message)
	{
		if (m_clientRef.LoggingEnabled)
		{
			m_clientRef.Log("RTT RECV: " + in_message);
		}
		Dictionary<string, object> dictionary = (Dictionary<string, object>)JsonReader.Deserialize(in_message);
		string text = (string)dictionary["service"];
		string text2 = (string)dictionary["operation"];
		Dictionary<string, object> dictionary2 = null;
		if (dictionary.ContainsKey("data"))
		{
			dictionary2 = (Dictionary<string, object>)dictionary["data"];
		}
		if (text2 == "CONNECT")
		{
			int num = m_heartBeatTime.Milliseconds / 1000;
			try
			{
				num = (int)dictionary2["heartbeatSeconds"];
			}
			catch (Exception)
			{
				num = (int)dictionary2["wsHeartbeatSecs"];
			}
			SetRTTHeartBeatSeconds(num);
		}
		else if (text2 == "DISCONNECT")
		{
			m_disconnectedWithReason = true;
			m_disconnectJson["reason_code"] = (int)dictionary2["reasonCode"];
			m_disconnectJson["reason"] = (string)dictionary2["reason"];
			m_disconnectJson["severity"] = "ERROR";
		}
		if (dictionary2 != null)
		{
			if (dictionary2.ContainsKey("cxId"))
			{
				RTTConnectionID = (string)dictionary2["cxId"];
			}
			if (dictionary2.ContainsKey("evs"))
			{
				RTTEventServer = (string)dictionary2["evs"];
			}
		}
		if (text2 != "HEARTBEAT")
		{
			addRTTCommandResponse(new RTTCommandResponse(text.ToLower(), text2.ToLower(), in_message));
		}
	}

	private void rttConnectionServerSuccess(string jsonResponse, object cbObject)
	{
		Dictionary<string, object> dictionary = (Dictionary<string, object>)((Dictionary<string, object>)JsonReader.Deserialize(jsonResponse))["data"];
		Array endpoints = (Array)dictionary["endpoints"];
		m_rttHeaders = (Dictionary<string, object>)dictionary["auth"];
		if (m_currentConnectionType == RTTConnectionType.WEBSOCKET)
		{
			m_endpoint = getEndpointForType(endpoints, "ws", in_bWantSsl: true);
			if (m_endpoint == null)
			{
				m_endpoint = getEndpointForType(endpoints, "ws", in_bWantSsl: false);
			}
			connectWebSocket();
		}
	}

	private Dictionary<string, object> getEndpointForType(Array endpoints, string type, bool in_bWantSsl)
	{
		Dictionary<string, object> result = null;
		Dictionary<string, object> dictionary = null;
		for (int i = 0; i < endpoints.Length; i++)
		{
			dictionary = endpoints.GetValue(i) as Dictionary<string, object>;
			if (dictionary["protocol"] as string == type)
			{
				if (!in_bWantSsl)
				{
					result = dictionary;
					break;
				}
				if ((bool)dictionary["ssl"])
				{
					result = dictionary;
					break;
				}
			}
		}
		return result;
	}

	private void rttConnectionServerError(int status, int reasonCode, string jsonError, object cbObject)
	{
		m_rttConnectionStatus = RTTConnectionStatus.DISCONNECTED;
		if (m_clientRef.LoggingEnabled)
		{
			m_clientRef.Log("RTT Connection Server Error: \n" + jsonError);
		}
		addRTTCommandResponse(new RTTCommandResponse(ServiceName.RTTRegistration.Value.ToLower(), "error", jsonError));
	}

	private void addRTTCommandResponse(RTTCommandResponse in_command)
	{
		lock (m_queuedRTTCommands)
		{
			m_queuedRTTCommands.Add(in_command);
		}
	}

	private string buildRTTRequestError(string in_statusMessage)
	{
		return JsonWriter.Serialize(new Dictionary<string, object>
		{
			["status"] = 403,
			["reason_code"] = 80300,
			["status_message"] = in_statusMessage,
			["severity"] = "ERROR"
		});
	}
}
