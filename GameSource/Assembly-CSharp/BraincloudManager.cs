using System;
using System.Collections;
using System.Collections.Generic;
using BCGSComponents;
using BCGSComponents.DataModels;
using BrainCloud;
using BrainCloud.Common;
using GameSparks.Api.Responses;
using GameSparks.Core;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;

public class BraincloudManager : GameSparksManager
{
	[Serializable]
	private class GetUserAttributeResponse
	{
		[Serializable]
		public class Data
		{
			public GameSettings.UserAttributes attributes;
		}

		public Data data;

		public int status;
	}

	private static BrainCloudWrapper brainCloudClient;

	private bool ready;

	private bool isWorking;

	private Action<bool> brainCloudAvailable;

	public static BrainCloudWrapper BC
	{
		get
		{
			if (brainCloudClient == null)
			{
				createInstance();
			}
			return brainCloudClient;
		}
	}

	public override bool Available
	{
		get
		{
			if (ready && brainCloudClient != null)
			{
				return brainCloudClient.Client.Initialized;
			}
			return false;
		}
	}

	public bool Authenticated
	{
		get
		{
			if (ready && brainCloudClient != null)
			{
				return brainCloudClient.Client.Authenticated;
			}
			return false;
		}
	}

	private static void createInstance()
	{
		Debug.Log("Creating Braincloud Wrapper instance");
		GameObject obj = new GameObject("BrainCloud Wrapper");
		brainCloudClient = obj.AddComponent<BrainCloudWrapper>();
		brainCloudClient.WrapperName = "BrainCloudWrapper";
		brainCloudClient.Init();
		UnityEngine.Object.DontDestroyOnLoad(obj);
	}

	protected override IEnumerator GetFrozenLobbyCode()
	{
		float startTime = Time.realtimeSinceStartup;
		while (!BC.Client.Authenticated || startTime + 20f < Time.realtimeSinceStartup)
		{
			yield return null;
		}
		if (!BC.Client.Authenticated)
		{
			waitToGetAttributesCoroutine = null;
			yield break;
		}
		GameSparksManager.Instance.CreateQuery().GetFrozenLobbyCode(delegate(GameSparks.Api.Responses.LogEventResponse response)
		{
			if (response.HasErrors)
			{
				Debug.LogError("Error frozen lobby code : " + response.Errors.JSON);
			}
			else
			{
				if (response.ScriptData.ContainsKey("frozenCode"))
				{
					GameSettings.GetInstance().frozenLobbyCode = response.ScriptData.GetString("frozenCode");
				}
				else
				{
					GameSettings.GetInstance().frozenLobbyCode = "";
				}
				Debug.Log("Frozen Lobby Code : " + GameSettings.GetInstance().frozenLobbyCode);
			}
		});
	}

	public void FreezeLobbyCode(Action<bool> callback, string lobbyCode)
	{
		GameSparksManager.Instance.CreateQuery().FreezeLobbyCode(delegate(GameSparks.Api.Responses.LogEventResponse response)
		{
			if (response.HasErrors)
			{
				Debug.LogError("Error frozen lobby code : " + response.Errors.JSON);
				callback?.Invoke(obj: false);
			}
			else
			{
				GameSettings.GetInstance().frozenLobbyCode = lobbyCode;
				Debug.Log("Frozen Lobby Code : " + GameSettings.GetInstance().frozenLobbyCode);
				callback?.Invoke(obj: true);
			}
		}, lobbyCode);
	}

	public void UnfreezeLobbyCode(Action<bool> callback)
	{
		GameSparksManager.Instance.CreateQuery().UnfreezeLobbyCode(delegate(GameSparks.Api.Responses.LogEventResponse response)
		{
			if (response.HasErrors)
			{
				Debug.LogError("Error frozen lobby code : " + response.Errors.JSON);
				callback?.Invoke(obj: false);
			}
			else
			{
				GameSettings.GetInstance().frozenLobbyCode = "";
				Debug.Log("UnFrozen Lobby Code");
				callback?.Invoke(obj: true);
			}
		});
	}

	public static void SendScriptRequest(BCGSRequest requestData, Action<BCGSObject> onResponse)
	{
		if (!BC.Client.Authenticated)
		{
			SuccessCallback onAuthSuccess = delegate
			{
				doSendScriptRequest(requestData, onResponse);
			};
			FailureCallback onAuthFail = delegate(int status, int code, string error, object cbObject)
			{
				Debug.LogWarning($"Authentication Failed: code:{code}, status:{status}, error:{error}");
				Debug.LogWarning($"BCGSInstance: SendScriptRequest Failed: code:{code}, status:{status}, error:{error}");
				BCGSObject obj = new BCGSObject(jsonToGSData(error, isError: true).BaseData);
				onResponse(obj);
			};
			Debug.Log("Lost connection. Trying to reconnect to BrainCloud");
			if (GameSparksManager.Instance.AuthenticatedUsing != AuthSource.NONE)
			{
				BC.Reconnect(onAuthSuccess, onAuthFail);
				return;
			}
			GameSparksManager.Instance.ConnectNow(delegate(bool success)
			{
				if (success)
				{
					onAuthSuccess("", null);
				}
				else
				{
					onAuthFail(-1, -1, "", null);
				}
			});
		}
		else
		{
			doSendScriptRequest(requestData, onResponse);
		}
	}

	private static void doSendScriptRequest(BCGSRequest requestData, Action<BCGSObject> onResponse)
	{
		BCGSData bCGSData = requestData.GetObject("scriptData");
		requestData.AddJSONStringAsObject("scriptData", bCGSData.JSON);
		if (Application.isEditor)
		{
			Debug.Log("BCGSInstance: SendScriptRequest: OUT >>>>>> " + requestData.JSON);
		}
		if (requestData.GetString("scriptName") == "LogEventRequest")
		{
			BC.ScriptService.RunScript(requestData.GetString("eventKey"), requestData.JSON, delegate(string response, object cbObject)
			{
				BCGSObject obj = BCGSObject.FromJson(response);
				onResponse(obj);
			}, delegate(int status, int code, string error, object cbObject)
			{
				Debug.LogWarning($"BCGSInstance: SendScriptRequest Failed: code:{code}, status:{status}, error:{error}");
				BCGSObject obj = new BCGSObject(jsonToGSData(error, isError: true).BaseData);
				onResponse(obj);
			});
			return;
		}
		Debug.Log("BCGSInstance: Processing OOTB Request...");
		BC.ScriptService.RunScript(requestData.Type, requestData.JSON, delegate(string response, object cbObject)
		{
			BCGSObject obj = BCGSObject.FromJson(response);
			onResponse(obj);
		}, delegate(int status, int code, string error, object cbObject)
		{
			Debug.LogWarning($"BCGSInstance: SendScriptRequest Failed: code:{code}, status:{status}, error:{error}");
			BCGSObject obj = new BCGSObject(jsonToGSData(error, isError: true).BaseData);
			onResponse(obj);
		});
	}

	public static void SendLogEventRequest(LogEventRequest request, Action<BCGSComponents.LogEventResponse> callback)
	{
		request.JSONData["eventKey"] = string.Format("events/{0}", request.JSONData["eventKey"]);
		request.SendVia(SendScriptRequest, callback);
	}

	public static void SendOOTBRequest(LogEventRequest request, Action<BCGSComponents.LogEventResponse> callback)
	{
		request.JSONData["eventKey"] = string.Format("ootb/OOTB_{0}", request.JSONData["eventKey"]);
		request.SendVia(SendScriptRequest, callback);
	}

	private void setAvailable(bool available)
	{
		if (ready != available)
		{
			ready = available;
			if (brainCloudAvailable != null)
			{
				isWorking = false;
				brainCloudAvailable(available);
			}
		}
	}

	protected override GameSparksQuery createQuery(bool debugOuput)
	{
		return new BraincloudQuery(debugOuput);
	}

	public override void Reconnect()
	{
		if (base.AuthenticatedUsing != AuthSource.NONE)
		{
			brainCloudClient.Reconnect();
		}
	}

	public override void Disconnect()
	{
		brainCloudClient.Client.ShutDown();
		setAvailable(available: false);
	}

	public override void ResetBackend()
	{
		brainCloudClient.resetWrapper();
		brainCloudClient.Init();
	}

	public override void RetryReadingMainUserGSID(UnityAction<bool> OnResponse)
	{
		SendOOTBRequest(new LogEventRequest().SetEventKey("AccountDetailsRequest").SetScriptData(new BCGSRequestData()), delegate(BCGSComponents.LogEventResponse response)
		{
			if (!response.HasErrors)
			{
				Debug.Log("Successfully got main user GSID through details request.");
				base.MainUserGSID = response.BaseData.GetString("UserId");
				OnResponse(arg0: true);
			}
			else
			{
				Debug.LogError("Could not read Account Details Response...");
				OnResponse(arg0: false);
			}
		});
	}

	protected override void sendAccountDetailsRequest(Action<GameSparks.Api.Responses.AccountDetailsResponse> responseHandler)
	{
		SendOOTBRequest(new LogEventRequest().SetEventKey("AccountDetailsRequest").SetScriptData(new BCGSRequestData()), delegate(BCGSComponents.LogEventResponse response)
		{
			IDictionary<string, object> data = (response.JSONData["data"] as IDictionary<string, object>)?["response"] as IDictionary<string, object>;
			responseHandler?.Invoke(new GameSparks.Api.Responses.AccountDetailsResponse(new GSData(data)));
		});
	}

	protected override void sendAuthenticationRequest(string username, string password, GSRequestData scriptData, Action<GameSparks.Api.Responses.AuthenticationResponse> responseHandler, string className = ".AuthenticationRequest")
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("@class", className);
		dictionary.Add("displayName", scriptData.GetString("displayName"));
		AuthenticationIds ids = new AuthenticationIds
		{
			authenticationToken = password,
			externalId = username
		};
		BC.AuthenticateAdvanced(AuthenticationType.Universal, ids, forceCreate: true, dictionary, delegate(string jsonResponse, object cbObject)
		{
			responseHandler?.Invoke(new GameSparks.Api.Responses.AuthenticationResponse(jsonToGSData(jsonResponse)));
		}, delegate(int status, int reasonCode, string jsonError, object cbObject)
		{
			responseHandler?.Invoke(new GameSparks.Api.Responses.AuthenticationResponse(jsonToGSData(jsonError, isError: true)));
		});
	}

	protected override void sendRegistrationRequest(string username, string password, string displayName, Action<GameSparks.Api.Responses.RegistrationResponse> responseHandler)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("@class", ".RegistrationRequest");
		dictionary.Add("displayName", displayName);
		AuthenticationIds ids = new AuthenticationIds
		{
			authenticationToken = password,
			externalId = username
		};
		BC.AuthenticateAdvanced(AuthenticationType.Universal, ids, forceCreate: true, dictionary, delegate(string jsonResponse, object cbObject)
		{
			responseHandler?.Invoke(new GameSparks.Api.Responses.RegistrationResponse(jsonToGSData(jsonResponse)));
		}, delegate(int status, int reasonCode, string jsonError, object cbObject)
		{
			responseHandler?.Invoke(new GameSparks.Api.Responses.RegistrationResponse(jsonToGSData(jsonError, isError: true)));
		});
	}

	private static GSData jsonToGSData(string jsonResponse, bool isError = false)
	{
		if (isError)
		{
			BCGSData bCGSData = new BCGSData();
			BCGSData value = new BCGSData(jsonResponse);
			bCGSData.BaseData.Add("error", value);
			return new GSData(bCGSData.BaseData);
		}
		return new GSData(new BCGSData(jsonResponse).GetBCGSData("data").BaseData);
	}

	protected override void sendSteamConnectRequest(string sessionTicket, Action<GameSparks.Api.Responses.AuthenticationResponse> connectResponse)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("@class", ".SteamConnectRequest");
		dictionary.Add("displayName", SteamFriends.GetPersonaName());
		AuthenticationIds ids = new AuthenticationIds
		{
			authenticationToken = sessionTicket,
			externalId = SteamUser.GetSteamID().ToString()
		};
		BC.AuthenticateAdvanced(AuthenticationType.Steam, ids, forceCreate: true, dictionary, delegate(string jsonResponse, object cbObject)
		{
			connectResponse?.Invoke(new GameSparks.Api.Responses.AuthenticationResponse(jsonToGSData(jsonResponse)));
		}, delegate(int status, int reasonCode, string jsonError, object cbObject)
		{
			connectResponse?.Invoke(new GameSparks.Api.Responses.AuthenticationResponse(jsonToGSData(jsonError, isError: true)));
		});
	}

	protected override void cleanup()
	{
		brainCloudClient.Client.ShutDown();
	}

	protected override void onAwake()
	{
		createInstance();
	}

	protected override void onUpdate()
	{
		if (BC != null && BC.Client.Initialized && !Available)
		{
			setAvailable(available: true);
		}
	}

	protected override IEnumerator SwitchToAlternateBackendService()
	{
		yield return null;
	}
}
