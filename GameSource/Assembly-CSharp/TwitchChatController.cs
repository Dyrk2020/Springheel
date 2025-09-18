using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameEvent;
using GameSparks.Core;
using I2.Loc;
using TwitchChatterUCH;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TwitchChatController : MonoBehaviour, IGameEventListener
{
	public static readonly int NumVoteDisplayWidgets = 8;

	public static TwitchChatController instance;

	public PlaceableMetadataList MetaList;

	public UnityEngine.Object twitchChatterPrefab;

	public GameObject twitchChatClientStatePrefab;

	public UnityEngine.Object twitchNameFireworksPrefab;

	public KeyCode debugPrintChannelsKeyCode = KeyCode.F7;

	public KeyCode debugFakeVoteKeyCode = KeyCode.F8;

	public KeyCode debugPrintVotesKeyCode = KeyCode.F9;

	public KeyCode debugFakeVote100KeyCode = KeyCode.F6;

	public KeyCode debugTestFireworksKeyCode = KeyCode.F10;

	public Text userVotedText;

	public TwitchVoteListCanvasLogic voteListCanvasLogic;

	[HideInInspector]
	public TwitchChatClientState twitchChatClientState;

	public UnityEngine.Object versusControllerPrefab;

	private TwitchChatClient chatClient;

	protected bool joinedInLobby;

	public static string[] itemShortNames;

	private HashSet<string> validItemNames = new HashSet<string>();

	private HashSet<string> allowedItems = new HashSet<string>();

	private Dictionary<string, List<int>> itemGroups = new Dictionary<string, List<int>>();

	private Dictionary<string, string> userVotes = new Dictionary<string, string>();

	private Dictionary<string, int> voteTally = new Dictionary<string, int>();

	private Dictionary<string, float> firstVoteTimestamps = new Dictionary<string, float>();

	private List<string> newVoteForAnimation = new List<string>();

	private float currentTimestamp;

	private Dictionary<int, string[]> votedItemUserNames = new Dictionary<int, string[]>();

	private Dictionary<string, int> pickableNameToIndex = new Dictionary<string, int>();

	private string currentChannel;

	[HideInInspector]
	public bool currentChannelConnected;

	private HashSet<string> joinedChannels = new HashSet<string>();

	private bool twitchChatShown = true;

	private bool mustSendUpdate;

	[HideInInspector]
	public bool tryingToConnect;

	[HideInInspector]
	public bool invalidChannelFlag;

	private float lastUsernameDisplayTime;

	private Queue<string> votedUsernames = new Queue<string>(128);

	private static bool isInGame;

	public static bool PlatformHasTwitchIntegration => true;

	private bool ShouldTallyTwitchVotes
	{
		get
		{
			if (LobbyManager.instance != null && LobbyManager.instance.IsHost && GameSettings.GetInstance().enableTwitchVoting && GameSettings.GetInstance().GameMode == GameState.GameMode.PARTY)
			{
				return GameSettings.GetInstance().partyBoxMode != PartyBoxMode.Disabled;
			}
			return false;
		}
	}

	private bool ShouldDisplayWidget
	{
		get
		{
			if (LobbyManager.instance != null && LobbyManager.instance.IsHost && GameSettings.GetInstance().enableTwitchVoting)
			{
				return currentChannelConnected;
			}
			return false;
		}
	}

	public virtual void ChangeListener(bool addRemove)
	{
		GameEventManager.ChangeListener<StartPhaseEvent>(this, addRemove);
		GameEventManager.ChangeListener<PickBlockEvent>(this, addRemove);
		GameEventManager.ChangeListener<PiecePlacedEvent>(this, addRemove);
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, addRemove);
		GameEventManager.ChangeListener<LanguageChangeEvent>(this, addRemove);
	}

	private void Awake()
	{
		if (instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			instance = this;
		}
		if (itemShortNames == null)
		{
			itemShortNames = new string[MetaList.AllBlockListLength()];
			for (int i = 0; i < itemShortNames.Length; i++)
			{
				itemShortNames[i] = MetaList.GetPlaceableByIndex(i).TwitchShortName;
			}
		}
		TwitchTermMapper.Initialize();
		SceneManager.activeSceneChanged += onSceneChanged;
	}

	private static void onSceneChanged(Scene scene, Scene newScene)
	{
		isInGame = !newScene.name.Equals("TreeHouseLobby") && !newScene.name.Equals("MainMenu");
	}

	private void Start()
	{
		for (int i = 0; i < itemShortNames.Length; i++)
		{
			string text = itemShortNames[i];
			validItemNames.Add(text);
			voteTally[text] = 0;
			if (!itemGroups.ContainsKey(text))
			{
				itemGroups[text] = new List<int>();
			}
			itemGroups[text].Add(i);
		}
		VersusControl component = ((GameObject)versusControllerPrefab).GetComponent<VersusControl>();
		for (int j = 0; j < component.MetaList.AllBlockListLength(); j++)
		{
			PickableBlock pickableByIndex = component.MetaList.GetPickableByIndex(j);
			if (!(pickableByIndex == null))
			{
				try
				{
					pickableNameToIndex.Add(pickableByIndex.name, j);
				}
				catch (Exception ex)
				{
					Debug.LogError("The pickable block " + pickableByIndex.name + " is used in multiple placeables.\n" + ex.Message + "\n" + ex.StackTrace);
				}
			}
		}
		ChangeListener(addRemove: true);
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(twitchChatterPrefab);
		gameObject.transform.SetParent(base.transform);
		chatClient = gameObject.GetComponent<TwitchChatClient>();
		chatClient.AddChatListener(OnReceiveChatMessage);
		chatClient.AddServerListener(OnReceiveServerMessage);
		SetShowTwitchChat(show: false);
		voteListCanvasLogic.gameObject.SetActive(value: false);
		ClientScene.RegisterPrefab(twitchChatClientStatePrefab);
	}

	private void OnDestroy()
	{
		ChangeListener(addRemove: false);
	}

	private void Update()
	{
		currentTimestamp += Time.unscaledDeltaTime;
		GameSettings gameSettings = GameSettings.GetInstance();
		if (LobbyManager.instance != null && LobbyManager.instance.IsHost)
		{
			if (gameSettings.enableTwitchVoting)
			{
				InitializeTwitchChatClientState();
				string text = gameSettings.twitchChannelName.ToLower();
				if (text != currentChannel || joinedInLobby)
				{
					joinedInLobby = false;
					LeaveCurrentChannel();
					currentChannel = text;
					if (twitchChatClientState != null)
					{
						twitchChatClientState.NetworkchannelName = currentChannel;
					}
					if (!currentChannel.NullOrEmpty())
					{
						chatClient.JoinChannel(currentChannel);
						joinedChannels.Add(currentChannel);
						tryingToConnect = true;
						InitializeTwitchChatClientState();
						StartCoroutine(CheckChannelNameValidity(currentChannel, GetOnChannelNameValidityFinished(currentChannel)));
					}
				}
			}
			else if (!currentChannel.NullOrEmpty())
			{
				LeaveCurrentChannel();
				if (twitchChatClientState != null)
				{
					UnityEngine.Object.Destroy(twitchChatClientState.gameObject);
					twitchChatClientState = null;
				}
			}
			bool flag = gameSettings.showTwitchChat && gameSettings.enableTwitchVoting;
			if (flag != twitchChatShown)
			{
				SetShowTwitchChat(flag);
			}
			if (mustSendUpdate && twitchChatClientState != null)
			{
				List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>(voteTally.Count);
				foreach (KeyValuePair<string, int> item in voteTally)
				{
					if (item.Value != 0)
					{
						list.Add(new KeyValuePair<int, string>(item.Value, item.Key));
					}
				}
				Comparison<KeyValuePair<int, string>> comparison = delegate(KeyValuePair<int, string> a, KeyValuePair<int, string> b)
				{
					int num7 = b.Key.CompareTo(a.Key);
					if (num7 == 0)
					{
						float value2 = firstVoteTimestamps[a.Value];
						num7 = firstVoteTimestamps[b.Value].CompareTo(value2);
						if (num7 == 0)
						{
							return b.Key.CompareTo(a.Key);
						}
						return num7;
					}
					return num7;
				};
				list.Sort(comparison);
				int num = Mathf.Min(list.Count, NumVoteDisplayWidgets);
				list.RemoveRange(num, list.Count - num);
				if (GameState.DebugMode)
				{
					string text2 = "";
					foreach (KeyValuePair<int, string> item2 in list)
					{
						text2 = text2 + item2.Key + ": " + item2.Value + "\n";
					}
					Debug.Log(text2);
				}
				for (int num2 = 0; num2 < NumVoteDisplayWidgets; num2++)
				{
					TwitchChatClientState.VoteState voteState = twitchChatClientState.SyncListVoteStates[num2];
					if (num2 < list.Count)
					{
						KeyValuePair<int, string> keyValuePair = list[num2];
						int num3 = -1;
						if (itemGroups.TryGetValue(keyValuePair.Value, out var value))
						{
							num3 = value[0];
						}
						if (voteState.votes != keyValuePair.Key || voteState.pickableIndex != num3)
						{
							bool newVotes = ((!newVoteForAnimation.Contains(keyValuePair.Value)) ? twitchChatClientState.SyncListVoteStates[num2].newVotes : (!twitchChatClientState.SyncListVoteStates[num2].newVotes));
							twitchChatClientState.SyncListVoteStates[num2] = new TwitchChatClientState.VoteState(num3, keyValuePair.Key, newVotes);
						}
					}
					else if (voteState.pickableIndex != -1 || voteState.votes != 0)
					{
						twitchChatClientState.SyncListVoteStates[num2] = new TwitchChatClientState.VoteState(-1, 0, newVotes: false);
					}
				}
				newVoteForAnimation.Clear();
				mustSendUpdate = false;
			}
			if (GameState.DebugMode)
			{
				if (Input.GetKeyUp(debugPrintChannelsKeyCode))
				{
					string text3 = "";
					foreach (string joinedChannel in joinedChannels)
					{
						text3 = text3 + joinedChannel + "\n";
					}
					Debug.Log("Joined channels:\n" + text3);
				}
				if (Input.GetKeyUp(debugFakeVoteKeyCode))
				{
					AddFakeVote();
				}
				if (Input.GetKeyUp(debugFakeVote100KeyCode))
				{
					for (int num4 = 0; num4 < 100; num4++)
					{
						AddFakeVote();
					}
				}
				if (Input.GetKeyUp(debugPrintVotesKeyCode))
				{
					string text4 = "";
					foreach (KeyValuePair<string, string> userVote in userVotes)
					{
						text4 = text4 + userVote.Key + " : " + userVote.Value + "\n";
					}
					Debug.Log("Votes:\n" + text4);
				}
				if (Input.GetKeyUp(debugTestFireworksKeyCode))
				{
					List<string> list2 = new List<string>();
					for (int num5 = 0; num5 < 20; num5++)
					{
						list2.Add(GetRandomFakeUsername());
					}
					voteListCanvasLogic.gameObject.AddPrefabAsChild<TwitchNameFireworks>(twitchNameFireworksPrefab).Initialize(list2);
				}
			}
		}
		else if (gameSettings.enableTwitchVoting)
		{
			string text5 = gameSettings.twitchChannelName.ToLower();
			if (text5 != currentChannel)
			{
				LeaveCurrentChannel();
				currentChannel = text5;
				joinedInLobby = true;
				if (!currentChannel.NullOrEmpty())
				{
					chatClient.JoinChannel(currentChannel);
					joinedChannels.Add(currentChannel);
					tryingToConnect = true;
					StartCoroutine(CheckChannelNameValidity(currentChannel, GetOnChannelNameValidityFinished(currentChannel)));
				}
			}
		}
		if (twitchChatClientState != null)
		{
			if (twitchChatClientState.hasAuthority && ShouldDisplayWidget != twitchChatClientState.showTwitchVoteWidget)
			{
				twitchChatClientState.NetworkshowTwitchVoteWidget = ShouldDisplayWidget;
			}
			voteListCanvasLogic.SetVisible(twitchChatClientState.showTwitchVoteWidget, gameSettings.GameMode == GameState.GameMode.PARTY, isInGame);
			if (twitchChatClientState.showTwitchVoteWidget)
			{
				voteListCanvasLogic.UpdateVotesFromClientState(twitchChatClientState);
			}
		}
		else
		{
			voteListCanvasLogic.SetVisible(show: false, isInPartyMode: false, IsInGame: false);
		}
		lastUsernameDisplayTime += Time.unscaledDeltaTime;
		if (votedUsernames.Count > 0 && twitchChatClientState != null && twitchChatClientState.showTwitchVoteWidget)
		{
			float num6 = 0.333f;
			if (votedUsernames.Count > 10)
			{
				num6 = Mathf.Min(5f / (float)votedUsernames.Count, num6);
			}
			if (!(lastUsernameDisplayTime < num6))
			{
				string arg = votedUsernames.Dequeue();
				userVotedText.text = string.Format(ScriptLocalization.Twitch_Voting.User_Voted, arg);
				userVotedText.GetComponent<Animator>().Play("UserVotedShow", 0, 0f);
				lastUsernameDisplayTime = 0f;
			}
		}
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		VersusControl component = ((GameObject)versusControllerPrefab).GetComponent<VersusControl>();
		if (e is LanguageChangeEvent)
		{
			OnLocalizationLanguageChange();
		}
		else if (e is PiecePlacedEvent piecePlacedEvent && twitchChatClientState != null && twitchChatClientState.hasAuthority)
		{
			if (twitchChatClientState.playersWithTwitchItem.Contains(piecePlacedEvent.PlayerNumber))
			{
				twitchChatClientState.CallRpcUserPlacedTwitchPiece(votedItemUserNames[component.MetaList.GetIndexForPlaceable(piecePlacedEvent.PlacedBlock.Name)], piecePlacedEvent.PlacedBlock.transform.position);
			}
		}
		else if (e is NetworkMessageReceivedEvent networkMessageReceivedEvent)
		{
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.SetBlockFrequency && networkMessageReceivedEvent.ReadMessage is MsgSetBlockFrequency msgSetBlockFrequency)
			{
				if (msgSetBlockFrequency.frequency == 0)
				{
					ClearVotesForItem(msgSetBlockFrequency.blockIndex);
				}
				UpdateAllowedItems();
			}
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.SetAllBlockFrequencies && networkMessageReceivedEvent.ReadMessage is MsgSetAllBlockFrequencies { frequency: 0 })
			{
				ClearVotes();
			}
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.SendAllBlockFrequencies && networkMessageReceivedEvent.ReadMessage is MsgSendAllBlockFrequencies msgSendAllBlockFrequencies)
			{
				for (int i = 0; i < msgSendAllBlockFrequencies.frequencies.Length; i++)
				{
					if (msgSendAllBlockFrequencies.frequencies[i] == 0)
					{
						ClearVotesForItem(i);
					}
				}
				ClearVotes();
			}
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.ApplyRuleset && networkMessageReceivedEvent.ReadMessage is MsgApplyRuleset)
			{
				ClearVotes();
			}
		}
		else
		{
			if (!ShouldTallyTwitchVotes)
			{
				return;
			}
			if (e is StartPhaseEvent startPhaseEvent)
			{
				switch (startPhaseEvent.Phase)
				{
				case GameControl.GamePhase.PLAY:
					if (twitchChatClientState != null)
					{
						twitchChatClientState.playersWithTwitchItem.Clear();
						twitchChatClientState.CallRpcClearUserVotedMessages();
					}
					break;
				case GameControl.GamePhase.PLACE:
					InitializeTwitchChatClientState();
					if (userVotes.Count > 0)
					{
						List<KeyValuePair<string, int>> source = voteTally.OrderByDescending((KeyValuePair<string, int> x) => x.Value).ToList();
						int num = UnityEngine.Random.Range(1, 5);
						if (GameSettings.GetInstance().partyBoxMode == PartyBoxMode.AutoRandom)
						{
							num = component.CurrentPlayerQueue.Count();
							switch (GameSettings.GetInstance().DoublePartyBox)
							{
							case DoublePartyBox.TwoPlayers:
								if (num == 2)
								{
									num *= 2;
								}
								break;
							case DoublePartyBox.Always:
								num *= 2;
								break;
							}
						}
						List<string> list = new List<string>();
						for (int num2 = 0; num2 <= num && num2 < source.Count(); num2++)
						{
							if (source.ElementAt(num2).Value > 0)
							{
								list.Add(source.ElementAt(num2).Key);
							}
							else
							{
								num++;
							}
						}
						twitchChatClientState.CallRpcDistributeVoteIntoPartyBox(list.Count);
						votedItemUserNames.Clear();
						{
							foreach (string item in list)
							{
								if (!itemGroups.TryGetValue(item, out var value))
								{
									continue;
								}
								int index = UnityEngine.Random.Range(0, value.Count);
								int num3 = value[index];
								string[] array = new string[voteTally[item]];
								int num4 = 0;
								foreach (KeyValuePair<string, string> userVote in userVotes)
								{
									if (userVote.Value == item)
									{
										array[num4++] = userVote.Key;
									}
								}
								votedItemUserNames.Add(num3, array);
								GameEventManager.SendEvent(new TwitchItemVoteEvent(num3));
							}
							break;
						}
					}
					if (GameState.DebugMode)
					{
						Debug.Log("(Twitch) No Twitch-selected block this round.");
					}
					break;
				case GameControl.GamePhase.START:
					break;
				}
			}
			else
			{
				if (!(e is PickBlockEvent pickBlockEvent) || !(pickBlockEvent.PickablePiece != null) || !(twitchChatClientState != null))
				{
					return;
				}
				if (pickBlockEvent.PickablePiece.isTwitchItem)
				{
					if (GameState.DebugMode)
					{
						Debug.Log("Player " + pickBlockEvent.PlayerNumber + " picked twitch item");
					}
					if (!twitchChatClientState.playersWithTwitchItem.Contains(pickBlockEvent.PlayerNumber))
					{
						twitchChatClientState.playersWithTwitchItem.Add(pickBlockEvent.PlayerNumber);
					}
				}
				else if (GameState.DebugMode)
				{
					Debug.Log("Player " + pickBlockEvent.PlayerNumber + " picked non-twitch item");
				}
			}
		}
	}

	public void EndAnimation()
	{
		ClearVotes();
	}

	private void ClearVotes()
	{
		currentTimestamp = 0f;
		firstVoteTimestamps.Clear();
		userVotes.Clear();
		foreach (string item in voteTally.Keys.ToList())
		{
			voteTally[item] = 0;
		}
		UpdateAllowedItems();
		mustSendUpdate = true;
	}

	private void UpdateAllowedItems()
	{
		VersusControl component = ((GameObject)versusControllerPrefab).GetComponent<VersusControl>();
		if (component != null)
		{
			if (itemShortNames.Length != component.MetaList.AllBlockListLength())
			{
				Debug.Log("Twitch Chat Controller: The short item name list is not the same size as allPickableBlocks!! This is now coming up as some pickable blocks aren't twitch votable (the set pieces)");
			}
			bool[] array = new bool[component.MetaList.AllBlockListLength()];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = true;
			}
			foreach (KeyValuePair<Placeable, GameRulePreset.BlockData> item in GameSettings.GetInstance().itemFilter)
			{
				int indexForPlaceable = component.MetaList.GetIndexForPlaceable(item.Key.Name);
				array[indexForPlaceable] = item.Value.Enabled;
			}
			allowedItems.Clear();
			foreach (KeyValuePair<string, List<int>> itemGroup in itemGroups)
			{
				foreach (int item2 in itemGroup.Value)
				{
					if (array[item2])
					{
						allowedItems.Add(itemGroup.Key);
					}
				}
			}
		}
		mustSendUpdate = true;
	}

	private void OnReceiveChatMessage(ref TwitchChatMessage message)
	{
		string[] array = message.chatMessagePlainText.Trim().Split(' ');
		if (array.Length == 1)
		{
			string localizedToken = array[0].ToLower();
			AddVote(message.userName, localizedToken);
		}
	}

	private void OnReceiveServerMessage(ref TwitchServerMessage message)
	{
		string text = "ROOMSTATE ";
		string rawText = message.rawText;
		int num = rawText.IndexOf(text);
		if (num == -1)
		{
			return;
		}
		if (GameState.DebugMode)
		{
			Debug.Log("Received ROOMSTATE from server.");
		}
		if (rawText.Substring(num + text.Length) == "#" + currentChannel)
		{
			if (GameState.DebugMode)
			{
				Debug.Log("Channel " + currentChannel + " connected successfully.");
			}
			currentChannelConnected = true;
			tryingToConnect = false;
		}
	}

	private void ClearVotesForItem(int pickableIndex)
	{
		string text = itemShortNames[pickableIndex];
		HashSet<string> hashSet = new HashSet<string>();
		foreach (KeyValuePair<string, string> userVote in userVotes)
		{
			if (userVote.Value == text)
			{
				hashSet.Add(userVote.Key);
			}
		}
		if (hashSet.Count <= 0)
		{
			return;
		}
		voteTally[text] = 0;
		foreach (string item in hashSet)
		{
			userVotes.Remove(item);
		}
		if (twitchChatClientState != null)
		{
			twitchChatClientState.NetworkNumberOfVotes = userVotes.Count;
		}
		if (firstVoteTimestamps.ContainsKey(text))
		{
			firstVoteTimestamps.Remove(text);
		}
		mustSendUpdate = true;
	}

	private void AddVote(string username, string localizedToken)
	{
		bool flag = false;
		string masterTerm = TwitchTermMapper.GetMasterTerm(localizedToken);
		if (masterTerm != null && validItemNames.Contains(masterTerm))
		{
			if (allowedItems.Contains(masterTerm))
			{
				if (userVotes.TryGetValue(username, out var value))
				{
					if (value != masterTerm)
					{
						userVotes[username] = masterTerm;
						voteTally[value]--;
						voteTally[masterTerm]++;
						flag = true;
						if (!newVoteForAnimation.Contains(masterTerm))
						{
							newVoteForAnimation.Add(masterTerm);
						}
					}
				}
				else
				{
					userVotes[username] = masterTerm;
					voteTally[masterTerm]++;
					flag = true;
					if (twitchChatClientState != null)
					{
						twitchChatClientState.NetworkNumberOfVotes = userVotes.Count;
					}
					if (!newVoteForAnimation.Contains(masterTerm))
					{
						newVoteForAnimation.Add(masterTerm);
					}
				}
				if (voteTally[masterTerm] == 0)
				{
					if (firstVoteTimestamps.ContainsKey(masterTerm))
					{
						firstVoteTimestamps.Remove(masterTerm);
					}
				}
				else if (!firstVoteTimestamps.ContainsKey(masterTerm))
				{
					firstVoteTimestamps.Add(masterTerm, currentTimestamp);
				}
				if (GameState.DebugMode)
				{
					Debug.Log("(Twitch) User " + username + " voted for: " + masterTerm);
				}
				if (value == null && twitchChatClientState != null)
				{
					twitchChatClientState.CallRpcUserVotedMessage(username);
				}
			}
			else if (GameState.DebugMode)
			{
				Debug.Log("(Twitch) User " + username + " voted for disallowed item: " + masterTerm + " (ignored)");
			}
		}
		if (flag)
		{
			mustSendUpdate = true;
		}
	}

	private void SetShowTwitchChat(bool show)
	{
		twitchChatShown = show;
		chatClient.transform.GetChild(0).gameObject.SetActive(show);
	}

	private void InitializeTwitchChatClientState()
	{
		if (twitchChatClientState == null && NetworkServer.active)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(twitchChatClientStatePrefab);
			twitchChatClientState = gameObject.GetComponent<TwitchChatClientState>();
			if (twitchChatClientState.SyncListVoteStates == null)
			{
				twitchChatClientState.SyncListVoteStates = new TwitchChatClientState.SyncListVoteState();
			}
			for (int i = 0; i < NumVoteDisplayWidgets; i++)
			{
				twitchChatClientState.SyncListVoteStates?.Add(new TwitchChatClientState.VoteState(-1, 0, newVotes: false));
			}
			twitchChatClientState.transform.SetParent(base.transform);
			NetworkServer.Spawn(gameObject);
			ClearVotes();
			twitchChatClientState.NetworkchannelName = currentChannel;
		}
	}

	public IEnumerator CheckChannelNameValidity(string channelName, UnityAction<WWW> OnQueryFinished)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("Client-ID", "dj4xalf8dt373ybx972v48mn9uasqq");
		WWW www = new WWW("https://api.twitch.tv/helix/users?login=" + WWW.EscapeURL(channelName), null, dictionary);
		while (!www.isDone)
		{
			yield return null;
		}
		OnQueryFinished(www);
	}

	private void LeaveCurrentChannel()
	{
		if (!currentChannel.NullOrEmpty())
		{
			chatClient.LeaveChannel(currentChannel);
			joinedChannels.Remove(currentChannel);
			currentChannel = null;
			currentChannelConnected = false;
			tryingToConnect = false;
		}
	}

	private bool UserWasFound(string channelName, string jsonResponse)
	{
		List<object> list = (GSJson.From(jsonResponse) as Dictionary<string, object>)["data"] as List<object>;
		if (list.Count > 0)
		{
			return (list[0] as Dictionary<string, object>)["login"] as string == channelName;
		}
		return false;
	}

	private UnityAction<WWW> GetOnChannelNameValidityFinished(string channelName)
	{
		return delegate(WWW www)
		{
			if (currentChannel == channelName && www.error.NullOrEmpty())
			{
				UserWasFound(channelName, www.text);
			}
		};
	}

	private string GetRandomFakeUsername()
	{
		return "FakeUser" + UnityEngine.Random.Range(9999, 99999);
	}

	private void AddFakeVote()
	{
		string randomFakeUsername = GetRandomFakeUsername();
		string localizedToken = TwitchTermMapper.AllLanguageShortNames[LocalizationManager.CurrentLanguage][UnityEngine.Random.Range(0, TwitchTermMapper.AllLanguageShortNames[LocalizationManager.CurrentLanguage].Count)];
		AddVote(randomFakeUsername, localizedToken);
	}

	public void EnqueueVotedUsername(string username)
	{
		votedUsernames.Enqueue(username);
	}

	public void ClearVotedUsernames()
	{
		votedUsernames.Clear();
		if (userVotedText != null)
		{
			Animator component = userVotedText.GetComponent<Animator>();
			if (component != null)
			{
				component.Play("UserVotedShow", 0, 1f);
			}
		}
	}

	public void SpawnNameFireworks(string[] names, Vector3 pos)
	{
		TwitchNameFireworks component = ((GameObject)UnityEngine.Object.Instantiate(twitchNameFireworksPrefab)).GetComponent<TwitchNameFireworks>();
		component.transform.position = pos;
		component.Initialize(names);
	}

	public void DistributeVotes(int numberToPutIntoPartyBox)
	{
		voteListCanvasLogic.DistributeVotes(numberToPutIntoPartyBox);
	}

	private int GetPickableNumberFromPrefabName(string name)
	{
		if (pickableNameToIndex.TryGetValue(name, out var value))
		{
			return value;
		}
		return -1;
	}

	public static void OnLocalizationLanguageChange()
	{
		instance.voteListCanvasLogic.OnLocalizationLanguageChange();
	}
}
