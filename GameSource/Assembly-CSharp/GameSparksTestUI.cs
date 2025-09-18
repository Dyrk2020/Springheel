using System.Collections.Generic;
using GameSparks.Api;
using GameSparks.Api.Messages;
using GameSparks.Api.Requests;
using GameSparks.Api.Responses;
using GameSparks.Core;
using UnityEngine;

public class GameSparksTestUI : MonoBehaviour
{
	private Queue<string> myLogQueue = new Queue<string>();

	private string myLog = "";

	private string fbToken = "accessToken";

	private string dismissMessageId = "messageId";

	private const int itemHeight = 30;

	private const int itemWidth = 200;

	private bool testing;

	private bool working;

	private bool result;

	private int counter;

	private int numTest;

	public Texture cursor;

	private void Awake()
	{
		Application.logMessageReceivedThreaded += HandleLog;
		Screen.orientation = ScreenOrientation.AutoRotation;
	}

	private void Start()
	{
		GSMessageHandler._AllMessages = HandleGameSparksMessageReceived;
	}

	private void HandleGameSparksMessageReceived(GSMessage message)
	{
		HandleLog("MSG:" + message.JSONString);
	}

	private void HandleLog(string logString)
	{
		GS.GSPlatform.ExecuteOnMainThread(delegate
		{
			HandleLog(logString, null, LogType.Log);
		});
	}

	private void HandleLog(string logString, string stackTrace, LogType logType)
	{
		if (myLogQueue.Count > 30)
		{
			myLogQueue.Dequeue();
		}
		myLogQueue.Enqueue(logString);
		myLog = "";
		string[] array = myLogQueue.ToArray();
		foreach (string text in array)
		{
			myLog = myLog + "\n\n" + text;
		}
	}

	private void OnGUI()
	{
		GUI.skin.label.alignment = TextAnchor.MiddleCenter;
		GUI.skin.textField.alignment = TextAnchor.MiddleCenter;
		GUI.skin.textArea.alignment = TextAnchor.LowerLeft;
		GUILayout.BeginHorizontal();
		GUILayout.Label(GS.Available ? "AVAILABLE" : "NOT AVAILABLE", GUILayout.Width(200f), GUILayout.Height(30f));
		GUILayout.Label("SDK Version: " + GS.Version.ToString(), GUILayout.Width(200f), GUILayout.Height(30f));
		GUILayout.EndHorizontal();
		GUILayout.Label(GS.Authenticated ? "AUTHENTICATED" : "NOT AUTHENTICATED", GUILayout.Width(200f), GUILayout.Height(30f));
		if (GUILayout.Button("Clear Log", GUILayout.Width(200f), GUILayout.Height(30f)))
		{
			myLog = "";
			myLogQueue.Clear();
		}
		if (GUILayout.Button("Logout", GUILayout.Width(200f), GUILayout.Height(30f)))
		{
			GS.Reset();
		}
		if (GUILayout.Button("Disconnect", GUILayout.Width(200f), GUILayout.Height(30f)))
		{
			GS.Disconnect();
		}
		if (!GS.Available && GUILayout.Button("Reconnect", GUILayout.Width(200f), GUILayout.Height(30f)))
		{
			GS.Reconnect();
		}
		if (GUILayout.Button("DeviceAuthenticationRequest", GUILayout.Width(200f), GUILayout.Height(30f)))
		{
			new DeviceAuthenticationRequest().Send(delegate(AuthenticationResponse response)
			{
				HandleLog("DeviceAuthenticationRequest.JSON:" + response.JSONString);
				HandleLog("DeviceAuthenticationRequest.HasErrors:" + response.HasErrors);
				HandleLog("DeviceAuthenticationRequest.UserId:" + response.UserId);
			});
		}
		if (GUILayout.Button("durableAccountDetailsRequest", GUILayout.Width(200f), GUILayout.Height(30f)))
		{
			new AccountDetailsRequest().SetDurable(durable: true).Send(null);
		}
		if (GUILayout.Button("accountDetailsRequest", GUILayout.Width(200f), GUILayout.Height(30f)))
		{
			new AccountDetailsRequest().Send(delegate(AccountDetailsResponse response)
			{
				HandleLog("AccountDetailsRequest.UserId:" + response.UserId);
			});
		}
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("facebookConnectRequest", GUILayout.Width(200f), GUILayout.Height(30f)))
		{
			new FacebookConnectRequest().SetAccessToken(fbToken).Send(delegate(AuthenticationResponse response)
			{
				HandleLog("FacebookConnectRequest.HasErrors:" + response.HasErrors);
				HandleLog("FacebookConnectRequest.UserId:" + response.UserId);
			});
		}
		fbToken = GUILayout.TextField(fbToken, GUILayout.Width(200f), GUILayout.Height(30f));
		GUILayout.EndHorizontal();
		if (GUILayout.Button("listAchievementsRequest", GUILayout.Width(200f), GUILayout.Height(30f)))
		{
			new ListAchievementsRequest().Send(delegate(ListAchievementsResponse response)
			{
				foreach (ListAchievementsResponse._Achievement achievement in response.Achievements)
				{
					HandleLog("ListAchievementsRequest:shortCode:" + achievement.ShortCode);
				}
			});
		}
		if (GUILayout.Button("listGameFriendsRequest", GUILayout.Width(200f), GUILayout.Height(30f)))
		{
			new ListGameFriendsRequest().Send(delegate(ListGameFriendsResponse response)
			{
				foreach (ListGameFriendsResponse._Player friend in response.Friends)
				{
					HandleLog("ListGameFriendsRequest.DisplayName:" + friend.DisplayName);
				}
			});
		}
		if (GUILayout.Button("listVirtualGoodsRequest", GUILayout.Width(200f), GUILayout.Height(30f)))
		{
			new ListVirtualGoodsRequest().Send(delegate(ListVirtualGoodsResponse response)
			{
				foreach (ListVirtualGoodsResponse._VirtualGood virtualGood in response.VirtualGoods)
				{
					HandleLog("ListVirtualGoodsRequest.Description:" + virtualGood.Description);
				}
			});
		}
		if (GUILayout.Button("listChallengeTypeRequest", GUILayout.Width(200f), GUILayout.Height(30f)))
		{
			new ListChallengeTypeRequest().Send(delegate(ListChallengeTypeResponse response)
			{
				foreach (ListChallengeTypeResponse._ChallengeType challengeTemplate in response.ChallengeTemplates)
				{
					HandleLog("ListAchievementsRequest.Challenge:" + challengeTemplate.ChallengeShortCode);
				}
			});
		}
		if (GUILayout.Button("authenticationRequest", GUILayout.Width(200f), GUILayout.Height(30f)))
		{
			new AuthenticationRequest().SetUserName("gabs").SetPassword("gabs").Send(delegate(AuthenticationResponse AR)
			{
				if (AR.HasErrors)
				{
					Debug.Log("Didnt Work");
				}
				else
				{
					Debug.Log("Worked");
				}
			});
		}
		if (GUILayout.Button("leaderboardData", GUILayout.Width(200f), GUILayout.Height(30f)))
		{
			new LeaderboardDataRequest().SetLeaderboardShortCode("HSCORE").SetEntryCount(10L).Send(delegate(LeaderboardDataResponse leadResponse)
			{
				if (leadResponse.HasErrors)
				{
					Debug.Log("Leaderboard data retrieval failed ...");
					return;
				}
				Debug.Log("Leaderboard data retrieval succeeded ..." + leadResponse);
				foreach (LeaderboardDataResponse._LeaderboardData datum in leadResponse.Data)
				{
					Debug.Log("Rank: " + datum.Rank + "    UserName: " + datum.UserName + "    Score: " + datum.GetNumberValue("SCORE"));
				}
			});
		}
		if (GUILayout.Button("listMessageRequest", GUILayout.Width(200f), GUILayout.Height(30f)))
		{
			new ListMessageRequest().Send(delegate(ListMessageResponse response)
			{
				foreach (GSData message in response.MessageList)
				{
					HandleLog("ListMessageRequest.MessageList:" + message.GetString("messageId"));
				}
			});
		}
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("dismissMessageRequest", GUILayout.Width(200f), GUILayout.Height(30f)))
		{
			new DismissMessageRequest().SetMessageId(dismissMessageId).Send(delegate(DismissMessageResponse response)
			{
				HandleLog("DismissMessageRequest.HasErrors:" + response.HasErrors);
			});
		}
		dismissMessageId = GUILayout.TextField(dismissMessageId, GUILayout.Width(200f), GUILayout.Height(30f));
		GUILayout.EndHorizontal();
		if (GUILayout.Button("TRACE " + (GS.TraceMessages ? "ON" : "OFF"), GUILayout.Width(200f), GUILayout.Height(30f)))
		{
			GS.TraceMessages = !GS.TraceMessages;
		}
		GUI.TextArea(new Rect(420f, 5f, Screen.width - 425, Screen.height - 10), myLog);
	}

	public void Update()
	{
	}
}
