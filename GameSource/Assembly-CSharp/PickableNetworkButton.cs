using System;
using System.Collections;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using GameEvent;
using I2.Loc;
using Moserware.Skills;
using Moserware.Skills.TrueSkill;
using Steamworks;
using UCHServices;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PickableNetworkButton : PickableButton
{
	public enum NetworkButtonJobs
	{
		HostGame = 0,
		PublicToggle = 1,
		JoinFriends = 2,
		FindMatch = 3,
		ManualConnection = 4,
		RegionCycle = 23,
		RegionText = 24,
		RegionDetect = 34,
		LobbyTagCycle = 37,
		JoinLobbyByCode = 38,
		EnterLobbyCode = 39,
		PasteLobbyCode = 50,
		FindPublicGameTitle = 5,
		RefreshSearch = 6,
		SearchResult = 7,
		Next = 8,
		Prev = 9,
		BackgroundImage = 31,
		RegionFilterButton = 32,
		NumPublicGamesFound = 33,
		LoadingSpinner = 36,
		HostGameP2P = 10,
		IpAddress = 11,
		Port = 12,
		JoinGame = 13,
		Help = 14,
		ToggleHosting = 15,
		InviteFriends = 16,
		MyIP = 17,
		HostingPort = 18,
		IpFriendMessage = 19,
		TwitchChannelName = 26,
		TwitchChannelConnectedMessage = 27,
		TwitchChannelConnectedCheckMark = 28,
		TwitchMustBePartyModeMessage = 29,
		ShowIP = 40,
		ShowLobbyCode = 41,
		MyLobbyCode = 42,
		LobbyCodeHelpMessage = 43,
		MyInternalIP = 20,
		BugReport = 21,
		Back = 22,
		FunReport = 25,
		HelpAlwaysOn = 30,
		DebugBackground = 35
	}

	public enum NetworkPageStates
	{
		MainNetworkMenu,
		FindMatch,
		ManualConnectionMenu,
		AlwaysOn
	}

	private const float TIME_BETWEEN_REFRESH_HIT = 3f;

	public NetworkButtonJobs job;

	public NetworkPageStates networkPage;

	private bool isGettingRelayServerInfo;

	public static NetworkPageStates networkMenuCurrentState;

	public static PickableNetworkButton backButton;

	public string SceneName;

	protected bool clicked;

	private bool waitingForPings;

	public InputField inputField;

	public bool needsnetworkIdentity;

	public int resultNumber;

	public CSteamID SteamLobby;

	public string lobbyID;

	public static int currentResultPage;

	public int NumberOfPlayersInSocialLobby;

	public int MatchProgress;

	public bool isAFK;

	public uint lastHeartBeat;

	[Header("SearchResult Connections")]
	public Text ResultNumber;

	public Text NumPlayersText;

	public Text RegionText;

	public Text MatchProgressText;

	public Text GameModeText;

	public Text PointLimitText;

	public Text LengthLimitText;

	public Text LengthTypeText;

	public Text LobbyHealth;

	public Text DebugText;

	public Text LobbyTagText;

	public Image usingModsImage;

	public UGCNameTag hostNameTag;

	public static bool showIp;

	public static bool showCode;

	protected bool currentlyShowing;

	private static bool uiLockedForJoin;

	public bool LockWhileConnecting;

	public bool TryingToConnect
	{
		get
		{
			Matchmaker instance = Matchmaker.Instance;
			if (instance == null)
			{
				return false;
			}
			if (!instance.WaitingForLobby && !instance.IsInLobby() && !instance.StartingLobby())
			{
				return isGettingRelayServerInfo;
			}
			return true;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (job == NetworkButtonJobs.Back)
		{
			backButton = this;
		}
		if (needsnetworkIdentity && !GetComponent<NetworkIdentity>())
		{
			base.gameObject.AddComponent<NetworkIdentity>();
		}
	}

	protected override void Start()
	{
		base.Start();
		switch (job)
		{
		case NetworkButtonJobs.IpAddress:
		{
			string networkAddress = GameSettings.GetInstance().NetworkAddress;
			if (!networkAddress.NullOrEmpty() && networkAddress != "localhost")
			{
				inputField.text = GameSettings.GetInstance().NetworkAddress;
			}
			break;
		}
		case NetworkButtonJobs.Port:
			inputField.placeholder.GetComponent<Text>().text = GameSettings.GetInstance().NetworkPort.ToString();
			break;
		case NetworkButtonJobs.HostingPort:
			buttonText.text = GameSettings.GetInstance().NetworkPort.ToString();
			break;
		case NetworkButtonJobs.MyIP:
			buttonText.text = Matchmaker.Instance.myExternalIP;
			break;
		case NetworkButtonJobs.MyInternalIP:
			buttonText.text = Matchmaker.Instance.myInternalIP;
			break;
		case NetworkButtonJobs.TwitchChannelName:
			inputField.text = GameSettings.GetInstance().twitchChannelName;
			break;
		case NetworkButtonJobs.RegionCycle:
			FindDefaultRegion();
			break;
		}
	}

	private async UniTaskVoid FindDefaultRegion()
	{
		await RelayConstants.PopulateAvailableRegions();
		await RelayConstants.LoadDynamicConfigs();
		await RegionPinger.PingAllRegions();
	}

	public override void OnAccept(PickCursor pickCursor)
	{
		switch (job)
		{
		case NetworkButtonJobs.HostGame:
			if (TryingToConnect)
			{
				return;
			}
			GameSettings.GetInstance().UseUnityRelay = true;
			StartHosting();
			break;
		case NetworkButtonJobs.PublicToggle:
		{
			MatchmakingLobby.Visibility[] array = new MatchmakingLobby.Visibility[3]
			{
				MatchmakingLobby.Visibility.PUBLIC,
				MatchmakingLobby.Visibility.FRIENDS,
				MatchmakingLobby.Visibility.PRIVATE
			};
			int num = -1;
			for (int i = 0; i < array.Length; i++)
			{
				if (GameSettings.GetInstance().lobbyPrivacy == array[i])
				{
					num = i;
				}
			}
			if (num == -1)
			{
				GameSettings.GetInstance().lobbyPrivacy = array[0];
			}
			else
			{
				GameSettings.GetInstance().lobbyPrivacy = array[(num + 1) % array.Length];
			}
			if (Matchmaker.CurrentMatchmakingLobby != null)
			{
				Matchmaker.CurrentMatchmakingLobby.LobbyVisibility = GameSettings.GetInstance().lobbyPrivacy;
			}
			break;
		}
		case NetworkButtonJobs.LobbyTagCycle:
		{
			LobbyTags lobbyTag = GameSettings.GetInstance().lobbyTag switch
			{
				LobbyTags.Fun => LobbyTags.Competitive, 
				LobbyTags.Competitive => LobbyTags.Beginner, 
				LobbyTags.Beginner => LobbyTags.CustomLevels, 
				LobbyTags.CustomLevels => LobbyTags.Fun, 
				_ => LobbyTags.Fun, 
			};
			GameSettings.GetInstance().lobbyTag = lobbyTag;
			if (Matchmaker.CurrentMatchmakingLobby != null && Matchmaker.Instance.IsLobbyOwner())
			{
				Matchmaker.CurrentMatchmakingLobby.SetLobbyTag(GameSettings.GetInstance().lobbyTag);
			}
			break;
		}
		case NetworkButtonJobs.JoinFriends:
			if (TryingToConnect || !SteamManager.Initialized)
			{
				return;
			}
			SteamFriends.ActivateGameOverlay("Friends");
			break;
		case NetworkButtonJobs.FindMatch:
		{
			if (TryingToConnect)
			{
				return;
			}
			networkMenuCurrentState = NetworkPageStates.FindMatch;
			SteamLobbySearchList steamLobbySearchList = UnityEngine.Object.FindObjectOfType<SteamLobbySearchList>();
			steamLobbySearchList.ClearList();
			currentResultPage = 0;
			Matchmaker.Instance.FindLobbies(steamLobbySearchList.AddFoundLobby);
			break;
		}
		case NetworkButtonJobs.ManualConnection:
			if (TryingToConnect)
			{
				return;
			}
			networkMenuCurrentState = NetworkPageStates.ManualConnectionMenu;
			break;
		case NetworkButtonJobs.RegionCycle:
		{
			if (TryingToConnect)
			{
				return;
			}
			GameSettings instance = GameSettings.GetInstance();
			int index = (int)Mathf.Repeat(RelayConstants.AVAILABLE_REGIONS.IndexOf(instance.SelectedRegion) + 1, RelayConstants.AVAILABLE_REGIONS.Count);
			instance.SelectedRegion = RelayConstants.AVAILABLE_REGIONS[index];
			break;
		}
		case NetworkButtonJobs.RegionDetect:
			if (!waitingForPings)
			{
				FindDefaultRegion();
			}
			break;
		case NetworkButtonJobs.RefreshSearch:
			if (TryingToConnect)
			{
				return;
			}
			deactivatedInBook = true;
			StartCoroutine(ReactivateButton());
			RefreshSearch();
			break;
		case NetworkButtonJobs.SearchResult:
			if (TryingToConnect)
			{
				return;
			}
			if (NumberOfPlayersInSocialLobby > 0 && NumberOfPlayersInSocialLobby < 4 && MatchProgress == 0)
			{
				JoinGame();
			}
			else
			{
				UserMessageManager.Instance.UserMessage(ScriptLocalization.Network.Game_In_Progress_No_Join, 3f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: false);
			}
			break;
		case NetworkButtonJobs.Next:
			currentResultPage++;
			break;
		case NetworkButtonJobs.Prev:
			currentResultPage--;
			if (currentResultPage < 0)
			{
				currentResultPage = 0;
			}
			break;
		case NetworkButtonJobs.RegionFilterButton:
		{
			if (TryingToConnect)
			{
				return;
			}
			int regionFilterIndex = GameSettings.GetInstance().RegionFilterIndex;
			regionFilterIndex++;
			if (regionFilterIndex >= RelayConstants.AVAILABLE_REGIONS.Count)
			{
				regionFilterIndex = -1;
			}
			GameSettings.GetInstance().RegionFilterIndex = regionFilterIndex;
			RefreshSearch();
			break;
		}
		case NetworkButtonJobs.HostGameP2P:
			if (TryingToConnect)
			{
				return;
			}
			GameSettings.GetInstance().UseUnityRelay = false;
			GameSettings.GetInstance().matchInfo = null;
			StartHosting();
			break;
		case NetworkButtonJobs.IpAddress:
			if (TryingToConnect)
			{
				return;
			}
			SteamDeck.OpenVirtualKeyboard(pickCursor);
			inputField.text = GameSettings.GetInstance().NetworkAddress;
			Controller.LockInputField(inputField, SetIP);
			break;
		case NetworkButtonJobs.Port:
			if (TryingToConnect)
			{
				return;
			}
			SteamDeck.OpenVirtualKeyboard(pickCursor);
			inputField.text = GameSettings.GetInstance().NetworkPort.ToString();
			Controller.LockInputField(inputField, SetPort);
			break;
		case NetworkButtonJobs.JoinGame:
			if (TryingToConnect)
			{
				return;
			}
			JoinGameManual();
			break;
		case NetworkButtonJobs.Help:
			OpenURLWrapper.Open("http://www.ultimatechickenHorse.com/onlineBetaHelp");
			break;
		case NetworkButtonJobs.HelpAlwaysOn:
			OpenURLWrapper.Open("http://www.ultimatechickenHorse.com/onlineBetaHelp");
			break;
		case NetworkButtonJobs.TwitchChannelName:
			Controller.LockInputField(inputField, SetTwitchChannelName);
			break;
		case NetworkButtonJobs.ToggleHosting:
			if (GetComponent<NetworkIdentity>().isServer)
			{
				if (NetworkServer.dontListen)
				{
					NetworkServer.dontListen = false;
					StartHostingInTreehouse();
				}
				else
				{
					NetworkServer.dontListen = true;
					StopHosting();
				}
			}
			break;
		case NetworkButtonJobs.InviteFriends:
			if (LobbyManager.instance.IsInOnlineGame)
			{
				SteamMatchmaker steamMatchmaker = (SteamMatchmaker)Matchmaker.Instance;
				if (steamMatchmaker.CurrentLobby != null && steamMatchmaker.SteamLobby != null)
				{
					Debug.LogError("[Net] Opening Friend Invite Overlay");
					SteamFriends.ActivateGameOverlayInviteDialog(steamMatchmaker.SteamLobby.LobbyID);
				}
			}
			break;
		case NetworkButtonJobs.BugReport:
			OpenURLWrapper.Open("https://docs.google.com/forms/d/1mOGX_DPyc0KR2qDHxXMXZQiTaPdCeEunBV3okaJDtSQ/viewform?entry.1945186752=" + GameSettings.GetInstance().VersionNumber + "&entry.173496600&entry.1116656086&entry.1198694514&entry.1233078894");
			break;
		case NetworkButtonJobs.FunReport:
			OpenURLWrapper.Open("https://docs.google.com/forms/d/1k6tiBS0YfitNWdTwLBckwpXBS-7f5lVhNTij109Lhbw/viewform?entry.1251884474&entry.776792862&entry.845986739&entry.891621479=" + GameSettings.GetInstance().VersionNumber);
			break;
		case NetworkButtonJobs.ShowIP:
			showIp = !showIp;
			break;
		case NetworkButtonJobs.ShowLobbyCode:
			showCode = !showCode;
			break;
		case NetworkButtonJobs.Back:
			if (networkMenuCurrentState == NetworkPageStates.MainNetworkMenu)
			{
				inventoryBook.GotoPage(fakeVariable: false, InventoryPage.PageTypes.TabletInterface);
			}
			BackToMainNetworkMenu();
			break;
		case NetworkButtonJobs.JoinLobbyByCode:
			JoinGameByCode();
			break;
		case NetworkButtonJobs.EnterLobbyCode:
			if (TryingToConnect)
			{
				return;
			}
			SteamDeck.OpenVirtualKeyboard(pickCursor);
			Controller.LockInputField(inputField, null);
			break;
		case NetworkButtonJobs.MyLobbyCode:
			if (Matchmaker.CurrentMatchmakingLobby != null)
			{
				TextEditor textEditor = new TextEditor();
				textEditor.text = Matchmaker.CurrentMatchmakingLobby.GetLobbyCode();
				textEditor.SelectAll();
				textEditor.Copy();
				UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.ShareableCodeClipboard, 2f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: true);
			}
			break;
		case NetworkButtonJobs.PasteLobbyCode:
		{
			string text = CleanCode(GUIUtility.systemCopyBuffer);
			if (!text.NullOrEmpty())
			{
				inputField.text = text;
				JoinGameByCode();
			}
			else
			{
				UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.InvalidCode, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
			}
			break;
		}
		}
		base.OnAccept(pickCursor);
	}

	private IEnumerator ReactivateButton()
	{
		yield return new WaitForSeconds(3f);
		deactivatedInBook = false;
	}

	protected string CleanCode(string input)
	{
		if (input.NullOrEmpty())
		{
			return null;
		}
		string text = Regex.Replace(input.ToUpper(), "[^A-Za-z]", "");
		if (text.Length != 4)
		{
			return null;
		}
		return text;
	}

	protected override void Update()
	{
		if (!TryingToConnect || !LockWhileConnecting)
		{
			base.Update();
		}
		if (uiLockedForJoin)
		{
			UnityMatchmaker unityMatchmaker = (UnityMatchmaker)Matchmaker.Instance;
			if (unityMatchmaker != null && !unityMatchmaker.joiningLobby && !TryingToConnect)
			{
				uiLockedForJoin = false;
				PickableButton.ResetMasks();
			}
		}
		if (!Visible || !initialized)
		{
			return;
		}
		if (deactivatedInBook || (TryingToConnect && LockWhileConnecting))
		{
			SetAlpha(0.5f);
		}
		else if (isAFK)
		{
			SetAlpha(0.5f);
		}
		else
		{
			SetAlpha(1f);
		}
		switch (job)
		{
		case NetworkButtonJobs.HostGame:
			Show(show: true);
			break;
		case NetworkButtonJobs.PublicToggle:
			Show(LobbyManager.instance == null || LobbyManager.instance.IsHost);
			switch (GameSettings.GetInstance().lobbyPrivacy)
			{
			case MatchmakingLobby.Visibility.PUBLIC:
				buttonText.text = ScriptLocalization.Network.Public;
				break;
			case MatchmakingLobby.Visibility.FRIENDS:
				buttonText.text = ScriptLocalization.Network.FriendsOnly;
				break;
			case MatchmakingLobby.Visibility.PRIVATE:
				buttonText.text = ScriptLocalization.Network.InviteOnly;
				break;
			}
			break;
		case NetworkButtonJobs.LobbyTagCycle:
			Show(LobbyManager.instance == null || LobbyManager.instance.IsHost);
			buttonText.text = GameSettings.ConvertLobbyTag(GameSettings.GetInstance().lobbyTag);
			break;
		case NetworkButtonJobs.JoinFriends:
			Show(show: true);
			break;
		case NetworkButtonJobs.FindMatch:
			Show(show: true);
			break;
		case NetworkButtonJobs.ManualConnection:
			Show(show: true);
			break;
		case NetworkButtonJobs.RegionCycle:
			Show(show: true);
			buttonText.text = GameSettings.GetInstance().SelectedRegion.LocalizedName;
			break;
		case NetworkButtonJobs.RegionDetect:
			Show(show: true);
			if (waitingForPings)
			{
				buttonText.text = "...";
			}
			else
			{
				buttonText.text = ScriptLocalization.Network.AutoDetect;
			}
			break;
		case NetworkButtonJobs.RegionText:
			Show(LobbyManager.instance == null || LobbyManager.instance.IsHost);
			break;
		case NetworkButtonJobs.FindPublicGameTitle:
			Show(show: true);
			break;
		case NetworkButtonJobs.RefreshSearch:
			Show(show: true);
			break;
		case NetworkButtonJobs.SearchResult:
			if (networkMenuCurrentState != NetworkPageStates.FindMatch)
			{
				Show(show: false);
			}
			else if (MatchProgress != 0)
			{
				deactivatedInBook = true;
			}
			else if (NumberOfPlayersInSocialLobby >= 4 || NumberOfPlayersInSocialLobby <= 0)
			{
				deactivatedInBook = true;
			}
			else
			{
				deactivatedInBook = false;
			}
			break;
		case NetworkButtonJobs.Next:
		{
			SteamLobbySearchList steamLobbySearchList = UnityEngine.Object.FindObjectOfType<SteamLobbySearchList>();
			if (steamLobbySearchList.SortedLobbiesCount > (currentResultPage + 1) * steamLobbySearchList.ResultListPositions.Length)
			{
				Show(show: true);
			}
			else
			{
				Show(show: false);
			}
			break;
		}
		case NetworkButtonJobs.Prev:
			if (currentResultPage > 0)
			{
				Show(show: true);
			}
			else
			{
				Show(show: false);
			}
			break;
		case NetworkButtonJobs.RegionFilterButton:
		{
			Show(show: true);
			int regionFilterIndex = GameSettings.GetInstance().RegionFilterIndex;
			if (regionFilterIndex == -1)
			{
				buttonText.text = ScriptLocalization.Network.All;
				break;
			}
			AvailableRegion availableRegion = RelayConstants.AVAILABLE_REGIONS[regionFilterIndex];
			buttonText.text = availableRegion.LocalizedName;
			break;
		}
		case NetworkButtonJobs.NumPublicGamesFound:
			Show(show: true);
			break;
		case NetworkButtonJobs.BackgroundImage:
			Show(show: true);
			break;
		case NetworkButtonJobs.LoadingSpinner:
			if (!currentlyShowing && Matchmaker.Instance.LoadingSearchIndicator && animator != null)
			{
				animator.SetTrigger("Reset");
			}
			Show(Matchmaker.Instance.LoadingSearchIndicator);
			break;
		case NetworkButtonJobs.HostGameP2P:
			Show(show: true);
			break;
		case NetworkButtonJobs.IpAddress:
			Show(show: true);
			break;
		case NetworkButtonJobs.Port:
			Show(show: true);
			break;
		case NetworkButtonJobs.JoinGame:
			Show(show: true);
			break;
		case NetworkButtonJobs.Help:
			Show(!GameSettings.GetInstance().UseUnityRelay);
			break;
		case NetworkButtonJobs.HelpAlwaysOn:
			Show(show: true);
			break;
		case NetworkButtonJobs.TwitchChannelName:
			Show(show: true);
			break;
		case NetworkButtonJobs.TwitchChannelConnectedMessage:
			if (TwitchChatController.instance != null)
			{
				if (TwitchChatController.instance.currentChannelConnected)
				{
					buttonText.text = ScriptLocalization.Options_Twitch.Joined_Successfully;
				}
				else
				{
					buttonText.text = ScriptLocalization.Options_Twitch.Awaiting_Valid_Channel;
				}
			}
			break;
		case NetworkButtonJobs.TwitchChannelConnectedCheckMark:
			if (TwitchChatController.instance != null)
			{
				if (GameSettings.GetInstance().twitchChannelName == "" && !TwitchChatController.instance.invalidChannelFlag)
				{
					buttonText.text = "";
				}
				else if (TwitchChatController.instance.currentChannelConnected)
				{
					buttonText.text = "<color=#ccff99> ✓</color>";
				}
				else if (TwitchChatController.instance.tryingToConnect)
				{
					buttonText.text = "<color=#eeee00>...</color>";
				}
				else
				{
					buttonText.text = "<color=#ffaaaa>✗</color>";
				}
			}
			break;
		case NetworkButtonJobs.TwitchMustBePartyModeMessage:
			if (TwitchChatController.instance != null)
			{
				if (LobbyManager.instance == null)
				{
					Show(show: false);
				}
				else
				{
					Show(GameSettings.GetInstance().GameMode != GameState.GameMode.PARTY);
				}
			}
			break;
		case NetworkButtonJobs.Back:
			Show(show: true);
			break;
		case NetworkButtonJobs.ToggleHosting:
			Show(GetComponent<NetworkIdentity>().isServer);
			if (GetComponent<NetworkIdentity>().isServer)
			{
				if (NetworkServer.dontListen)
				{
					buttonText.text = ScriptLocalization.Network.Start_Hosting;
				}
				else
				{
					buttonText.text = ScriptLocalization.Network.Stop_Hosting;
				}
			}
			break;
		case NetworkButtonJobs.InviteFriends:
			Show(Matchmaker.CurrentMatchmakingLobby != null && Matchmaker.CurrentMatchmakingLobby.IsValid());
			break;
		case NetworkButtonJobs.HostingPort:
			Show(GetComponent<NetworkIdentity>().isServer && !GameSettings.GetInstance().UseUnityRelay);
			break;
		case NetworkButtonJobs.ShowIP:
			Show(!GameSettings.GetInstance().UseUnityRelay);
			break;
		case NetworkButtonJobs.ShowLobbyCode:
			Show(GameSettings.GetInstance().UseUnityRelay);
			break;
		case NetworkButtonJobs.MyIP:
			Show(!GameSettings.GetInstance().UseUnityRelay);
			if (showIp)
			{
				buttonText.text = Matchmaker.Instance.myExternalIP;
			}
			else
			{
				buttonText.text = ScriptLocalization.Network.ipHidden;
			}
			break;
		case NetworkButtonJobs.MyLobbyCode:
			Show(GameSettings.GetInstance().UseUnityRelay);
			if (showCode && Matchmaker.CurrentMatchmakingLobby != null)
			{
				if (buttonText != null)
				{
					buttonText.text = Matchmaker.CurrentMatchmakingLobby.GetLobbyCode();
				}
			}
			else if (buttonText != null)
			{
				buttonText.text = "****";
			}
			break;
		case NetworkButtonJobs.IpFriendMessage:
			Show(GetComponent<NetworkIdentity>().isServer && !GameSettings.GetInstance().UseUnityRelay);
			break;
		case NetworkButtonJobs.LobbyCodeHelpMessage:
			Show(GameSettings.GetInstance().UseUnityRelay);
			break;
		case NetworkButtonJobs.MyInternalIP:
			Show(GetComponent<NetworkIdentity>().isServer && !GameSettings.GetInstance().UseUnityRelay);
			break;
		case NetworkButtonJobs.BugReport:
			Show(show: true);
			break;
		case NetworkButtonJobs.DebugBackground:
			Show(GameState.DebugMode);
			break;
		case NetworkButtonJobs.EnterLobbyCode:
			Show(show: true);
			break;
		case NetworkButtonJobs.JoinLobbyByCode:
			Show(inputField.text.Length == 4);
			break;
		case NetworkButtonJobs.PasteLobbyCode:
			Show(show: true);
			break;
		case NetworkButtonJobs.FunReport:
		case (NetworkButtonJobs)44:
		case (NetworkButtonJobs)45:
		case (NetworkButtonJobs)46:
		case (NetworkButtonJobs)47:
		case (NetworkButtonJobs)48:
		case (NetworkButtonJobs)49:
			break;
		}
	}

	protected void Show(bool show)
	{
		currentlyShowing = show;
		if (show && networkPage != networkMenuCurrentState && networkPage != NetworkPageStates.AlwaysOn)
		{
			Show(show: false);
			return;
		}
		if ((bool)buttonText)
		{
			buttonText.enabled = show;
		}
		Collider2D[] pickColliders = PickColliders;
		for (int i = 0; i < pickColliders.Length; i++)
		{
			pickColliders[i].enabled = show;
		}
		if ((bool)inputField && job != NetworkButtonJobs.JoinLobbyByCode)
		{
			inputField.gameObject.SetActive(show);
			if (show)
			{
				inputField.placeholder.gameObject.SetActive(inputField.text.NullOrEmpty());
			}
		}
		if ((bool)image)
		{
			image.enabled = show;
		}
		if ((bool)sprite)
		{
			sprite.enabled = show;
		}
		if ((bool)ResultNumber)
		{
			ResultNumber.enabled = show;
		}
		if ((bool)NumPlayersText)
		{
			NumPlayersText.enabled = show;
		}
		if ((bool)RegionText)
		{
			RegionText.enabled = show;
		}
		if ((bool)MatchProgressText)
		{
			MatchProgressText.enabled = show;
		}
		if ((bool)GameModeText)
		{
			GameModeText.enabled = show;
		}
		if ((bool)PointLimitText)
		{
			PointLimitText.enabled = show;
		}
		if ((bool)usingModsImage)
		{
			usingModsImage.enabled = show;
		}
		if ((bool)LengthLimitText)
		{
			LengthLimitText.enabled = show;
		}
		if ((bool)LengthTypeText)
		{
			LengthTypeText.enabled = show;
		}
		if ((bool)hostNameTag)
		{
			hostNameTag.gameObject.SetActive(show);
		}
		if ((bool)LobbyHealth)
		{
			LobbyHealth.enabled = show;
		}
		if ((bool)LobbyTagText)
		{
			LobbyTagText.enabled = show;
		}
	}

	public override void Enable(bool onOff = true)
	{
		base.Enable(onOff);
		if (onOff && job == NetworkButtonJobs.RefreshSearch && networkMenuCurrentState == NetworkPageStates.FindMatch && pageNumber == inventoryBook.currentPage)
		{
			RefreshSearch();
		}
		if (inputField != null)
		{
			inputField.enabled = onOff;
		}
		if (job == NetworkButtonJobs.TwitchChannelName && inputField != null)
		{
			inputField.onValueChanged.RemoveAllListeners();
			inputField.onValueChanged.AddListener(OnTwitchChannelNameChange);
		}
	}

	protected async void StartHosting()
	{
		Debug.LogWarning("================================ StartHosting ================================");
		GameSettings instance = GameSettings.GetInstance();
		instance.StartAsHost = true;
		instance.StartLocal = false;
		instance.GameMode = instance.DefaultGameMode;
		try
		{
			isGettingRelayServerInfo = true;
			GameSettings instance2 = GameSettings.GetInstance();
			instance2.RelayServerConnectionData = await UCHOnlineConnector.Service.SendGetNextAvailableServer();
			Matchmaker.Instance.CreateLobby();
		}
		catch (Exception ex)
		{
			Debug.LogError("Error getting next available server. " + ex.Message);
		}
		finally
		{
			isGettingRelayServerInfo = false;
		}
	}

	protected void StartHostingInTreehouse()
	{
		Debug.LogWarning("================================ StartHostingInTreehouse ================================");
		GameSettings.GetInstance().StartAsHost = true;
		GameSettings.GetInstance().StartLocal = false;
		Matchmaker.Instance.CreateLobby();
	}

	protected void StopHosting()
	{
		Debug.LogWarning("================================ StopHosting ================================");
		Matchmaker.Instance.LeaveLobby();
	}

	protected async UniTaskVoid JoinGameByCode()
	{
		if (!(inputField != null))
		{
			return;
		}
		GameSettings.GetInstance().StartAsHost = false;
		GameSettings.GetInstance().StartLocal = false;
		Matchmaker.Instance.JoinLobby(inputField.text, useCode: true, delegate(bool success)
		{
			if (success)
			{
				if (Matchmaker.CurrentMatchmakingLobby != null)
				{
					AnalyticEvent.JoinMatchEvent(Matchmaker.CurrentMatchmakingLobby.GetLobbyGuid(), AnalyticEvent.JoinMethod.CODE, Matchmaker.CurrentMatchmakingLobby.LobbyIsCrossplay(Application.platform));
				}
				else
				{
					Debug.LogError("ERROR: Matchmaker.CurrentMatchmakingLobby is null - JoinMatchEvent could not be sent");
				}
			}
		});
	}

	protected async void JoinGame()
	{
		if ((Matchmaker.Instance as UnityMatchmaker).joiningLobby)
		{
			return;
		}
		Debug.LogWarning("================================ JoinGame ================================");
		uiLockedForJoin = true;
		PickableButton.maskAll = true;
		GameSettings.GetInstance().StartAsHost = false;
		GameSettings.GetInstance().StartLocal = false;
		Matchmaker.Instance.JoinLobby(lobbyID, useCode: false, delegate(bool success)
		{
			if (success)
			{
				AnalyticEvent.JoinMatchEvent(Matchmaker.CurrentMatchmakingLobby.GetLobbyGuid(), AnalyticEvent.JoinMethod.LIST, Matchmaker.CurrentMatchmakingLobby.LobbyIsCrossplay(Application.platform));
			}
		});
	}

	protected void JoinGameManual()
	{
		Debug.LogWarning("================================ JoinGameManual ================================");
		GameSettings.GetInstance().StartAsHost = false;
		GameSettings.GetInstance().StartLocal = false;
		GameSettings.GetInstance().UseUnityRelay = false;
		Matchmaker.Instance.JoinGameManual();
	}

	public void SetIP(string input)
	{
		GameSettings.GetInstance().NetworkAddress = input;
		Debug.Log("Ip set to:" + GameSettings.GetInstance().NetworkAddress.ToString());
	}

	public void SetPort(string input)
	{
		try
		{
			GameSettings.GetInstance().NetworkPort = int.Parse(input);
		}
		catch
		{
			GameSettings.GetInstance().NetworkPort = 17778;
		}
		Debug.Log("port set to:" + GameSettings.GetInstance().NetworkPort);
	}

	public override void SetAlpha(float newAlpha)
	{
		if ((bool)buttonText)
		{
			buttonText.color = new Color(buttonText.color.r, buttonText.color.g, buttonText.color.b, newAlpha);
		}
		if ((bool)image)
		{
			image.color = new Color(image.color.r, image.color.g, image.color.b, newAlpha);
		}
		if ((bool)sprite)
		{
			sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, newAlpha);
		}
		SetTextAlpha(ResultNumber, newAlpha);
		SetTextAlpha(NumPlayersText, newAlpha);
		SetTextAlpha(RegionText, newAlpha);
		SetTextAlpha(MatchProgressText, newAlpha);
		SetTextAlpha(GameModeText, newAlpha);
		SetTextAlpha(PointLimitText, newAlpha);
		if ((bool)usingModsImage)
		{
			usingModsImage.SetAlpha(newAlpha);
		}
		SetTextAlpha(LengthLimitText, newAlpha);
		SetTextAlpha(LengthTypeText, newAlpha);
		if (hostNameTag != null)
		{
			hostNameTag.GetComponent<CanvasGroup>().alpha = newAlpha;
		}
		SetTextAlpha(LobbyHealth, newAlpha);
		SetTextAlpha(LobbyTagText, newAlpha);
	}

	public void SetTextAlpha(Text textItem, float newAlpha)
	{
		if (textItem != null)
		{
			textItem.color = new Color(textItem.color.r, textItem.color.g, textItem.color.b, newAlpha);
		}
	}

	public override void handleEvent(global::GameEvent.GameEvent e)
	{
		base.handleEvent(e);
		Type type = e.GetType();
		if (job == NetworkButtonJobs.RefreshSearch && type == typeof(SpecialUIEvent) && (e as SpecialUIEvent).SpecialUIType == SpecialUIEvent.SpecialUI.REFRESHSEARCH && networkMenuCurrentState == NetworkPageStates.FindMatch)
		{
			RefreshSearch();
		}
	}

	public void RefreshSearch()
	{
		SteamLobbySearchList steamLobbySearchList = UnityEngine.Object.FindObjectOfType<SteamLobbySearchList>();
		Matchmaker.Instance.FindLobbies(steamLobbySearchList.AddFoundLobby);
		steamLobbySearchList.ClearList();
		currentResultPage = 0;
	}

	public static void BackToMainNetworkMenu()
	{
		SteamLobbySearchList steamLobbySearchList = UnityEngine.Object.FindObjectOfType<SteamLobbySearchList>();
		networkMenuCurrentState = NetworkPageStates.MainNetworkMenu;
		steamLobbySearchList.ClearList();
		currentResultPage = 0;
	}

	public void SetSearchResultInfo(Matchmaker.LobbyListInfo lobbyInfo, int resultNumber)
	{
		SteamLobby = new CSteamID(lobbyInfo.ulLobbyID);
		lobbyID = lobbyInfo.sLobbyID;
		this.resultNumber = resultNumber;
		ResultNumber.text = resultNumber + ".";
		NumberOfPlayersInSocialLobby = lobbyInfo.Players;
		NumPlayersText.text = lobbyInfo.Players + "/4";
		RegionText.text = lobbyInfo.UnityServerRegion.LocalizedShortName;
		MatchProgress = lobbyInfo.matchProgress;
		isAFK = lobbyInfo.isAFK;
		if (lobbyInfo.matchProgress == 0)
		{
			if (lobbyInfo.isAFK)
			{
				MatchProgressText.text = LocalizationManager.GetTranslation("Network/MatchProgressAFK");
			}
			else
			{
				MatchProgressText.text = LocalizationManager.GetTranslation("Network/MatchProgressLobby");
			}
		}
		else
		{
			MatchProgressText.text = 100 - lobbyInfo.matchProgress + "%";
		}
		if (lobbyInfo.rulePreset == GameSettings.GetInstance().DefaultRuleset.Name)
		{
			GameModeText.text = GameState.GetLocalizedGameModeName(lobbyInfo.gameMode);
		}
		else
		{
			GameModeText.text = LocalizationManager.GetTranslation("RuleBook/Presets/" + lobbyInfo.rulePreset);
		}
		if (lobbyInfo.gameMode == GameState.GameMode.FREEPLAY || lobbyInfo.gameMode == GameState.GameMode.CHALLENGE)
		{
			PointLimitText.text = "--";
		}
		else
		{
			PointLimitText.text = (lobbyInfo.pointLimit / 50).ToString();
		}
		if (lobbyInfo.gameMode == GameState.GameMode.FREEPLAY || lobbyInfo.gameMode == GameState.GameMode.CHALLENGE)
		{
			LengthTypeText.text = "";
			LengthLimitText.text = ScriptLocalization.RuleBook.NoLimit;
		}
		else
		{
			switch (lobbyInfo.limitType)
			{
			case GameLimitType.NONE:
				LengthTypeText.text = "";
				LengthLimitText.text = ScriptLocalization.RuleBook.NoLimit;
				break;
			case GameLimitType.TIME:
				LengthTypeText.text = ScriptLocalization.RuleBook.minutesAbbreviation;
				LengthLimitText.text = (lobbyInfo.limitAmount / 60).ToString();
				break;
			case GameLimitType.ROUNDS:
				LengthTypeText.text = ScriptLocalization.RuleBook.Rounds;
				LengthLimitText.text = lobbyInfo.limitAmount.ToString();
				break;
			}
		}
		LobbyTagText.text = GameSettings.ConvertLobbyTag(lobbyInfo.lobbyTag);
		usingModsImage.enabled = lobbyInfo.usingMods;
		hostNameTag.Initialize(lobbyInfo.LobbyOwner, lobbyInfo.hostPlatformId, lobbyInfo.hostGSID, lobbyInfo.hostPlatform, isAnonymous: false);
		if (GameState.DebugMode)
		{
			double num = 25.0;
			SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
			if (saveFileDataForMainUser != null)
			{
				num = saveFileDataForMainUser.SkillMean;
			}
			DebugText.text = lobbyInfo.DebugString + " " + lobbyInfo.LobbyHealthNum + " " + lobbyInfo.CalculatedSkillMatchQuality.ToString("F2") + "   " + num.ToString("F0") + " " + readableSkill(lobbyInfo.PlayerSkills);
		}
		else
		{
			DebugText.text = "";
		}
		if (lobbyInfo.LobbyHealthNum >= GameSettings.GetInstance().GoodMatchScore)
		{
			LobbyHealth.text = "✓✓";
		}
		else if (lobbyInfo.LobbyHealthNum >= GameSettings.GetInstance().OkMatchScore)
		{
			LobbyHealth.text = "✓";
		}
		else
		{
			LobbyHealth.text = "";
		}
		if (lobbyInfo.CalculatedSkillMatchQuality >= GameSettings.GetInstance().SkillMatchQualityThreshold)
		{
			LobbyHealth.text += "✓";
		}
	}

	private double calcMatchQuality(string playerSkillString)
	{
		if (playerSkillString == null || playerSkillString == "")
		{
			return -1.0;
		}
		int result = 0;
		string[] array = playerSkillString.Split(',');
		if (array.Length == 0 || !int.TryParse(array[0], out result))
		{
			return -1.0;
		}
		if (array.Length - 1 < 2 * result)
		{
			return -1.0;
		}
		Team<int>[] array2 = new Team<int>[result + 1];
		for (int i = 0; i < result; i++)
		{
			if (!Parsing.TryParseDouble(array[2 * i + 1], out var result2) || !Parsing.TryParseDouble(array[2 * i + 2], out var result3))
			{
				return -1.0;
			}
			array2[i] = new Team<int>(i, new Rating(result2, result3));
		}
		double mean = 25.0;
		double standardDeviation = 8.333333333333334;
		SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
		if (saveFileDataForMainUser != null)
		{
			mean = saveFileDataForMainUser.SkillMean;
			standardDeviation = saveFileDataForMainUser.SkillStdDev;
		}
		array2[result] = new Team<int>(result, new Rating(mean, standardDeviation));
		return new FactorGraphTrueSkillCalculator().CalculateMatchQuality(GameInfo.DefaultGameInfo, Teams.Concat(array2));
	}

	private string readableSkill(string playerSkillString)
	{
		if (playerSkillString == null || playerSkillString == "")
		{
			return "fail";
		}
		int result = 0;
		string[] array = playerSkillString.Split(',');
		if (array.Length == 0 || !int.TryParse(array[0], out result))
		{
			return "fail";
		}
		if (array.Length - 1 < 2 * result)
		{
			return "fail";
		}
		string text = "";
		for (int i = 0; i < result; i++)
		{
			if (!Parsing.TryParseDouble(array[2 * i + 1], out var result2) || !Parsing.TryParseDouble(array[2 * i + 2], out var _))
			{
				return "fail";
			}
			text = text + result2.ToString("F0") + " ";
		}
		return text;
	}

	public void SetTwitchChannelName(string input)
	{
		GameSettings.GetInstance().twitchChannelName = input;
		Debug.Log("Twitch channel name set to: " + input);
	}

	public void OnTwitchChannelNameChange(string input)
	{
		TwitchChatController.instance.invalidChannelFlag = false;
	}
}
