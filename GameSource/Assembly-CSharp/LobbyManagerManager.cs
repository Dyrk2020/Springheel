using System;
using System.Collections;
using System.Collections.Generic;
using GameEvent;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class LobbyManagerManager : MonoBehaviour
{
	private static LobbyManagerManager instance;

	private HashSet<LobbyPlayer> playersToRemove = new HashSet<LobbyPlayer>();

	private HashSet<int> removedNetworkNumbers = new HashSet<int>();

	private bool abortGameRequested;

	private bool shuttingDown;

	private string abortReason = "";

	private static List<Action> mainMenuActions = new List<Action>();

	public static List<string> expectedSceneLoads = new List<string>();

	public int SceneLoadCounter;

	public static LobbyManagerManager Instance
	{
		get
		{
			if (instance == null)
			{
				new GameObject("LobbyManager Manager").AddComponent<LobbyManagerManager>();
			}
			return instance;
		}
	}

	public bool IsStopping
	{
		get
		{
			if (!abortGameRequested)
			{
				return shuttingDown;
			}
			return true;
		}
	}

	public static string LastSceneLoaded { get; protected set; }

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		if (instance == this)
		{
			Debug.Log("LobbyManagerManager shutting down...");
			SceneManager.activeSceneChanged -= OnSceneChange;
			SceneManager.sceneLoaded -= OnSceneLoaded;
			SceneManager.sceneUnloaded -= OnSceneUnloaded;
		}
	}

	public void Initialize()
	{
		SceneManager.activeSceneChanged += OnSceneChange;
		SceneManager.sceneLoaded += OnSceneLoaded;
		SceneManager.sceneUnloaded += OnSceneUnloaded;
	}

	public void Update()
	{
		foreach (LobbyPlayer item in playersToRemove)
		{
			if (item != null)
			{
				Debug.LogWarning("Destroying LobbyPlayer Object");
				RemoveLobbyPlayerFromSlots(item);
				UnityEngine.Object.Destroy(item.gameObject);
			}
			else
			{
				Debug.LogWarning("Marked LobbyPlayer Object was already destroyed!");
			}
		}
		playersToRemove.Clear();
		foreach (int removedNetworkNumber in removedNetworkNumbers)
		{
			NotifyPlayerRemoved(removedNetworkNumber);
		}
		removedNetworkNumbers.Clear();
		if (abortGameRequested)
		{
			AbortGame();
		}
	}

	private static void RemoveLobbyPlayerFromSlots(LobbyPlayer playerToRemove)
	{
		int num = -1;
		for (int i = 0; i < LobbyManager.instance.lobbySlots.Length; i++)
		{
			if (playerToRemove == LobbyManager.instance.lobbySlots[i])
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			Debug.LogWarning($"Removing lobby player from slots at index : {num}");
			LobbyManager.instance.lobbySlots[num] = null;
		}
	}

	public void OnLobbyPlayerObjectDestroyed(LobbyPlayer lobbyPl)
	{
		MarkLobbyPlayerRemoved(lobbyPl.networkNumber);
	}

	public void MarkLobbyPlayerToRemove(LobbyPlayer lobbyPl)
	{
		if (lobbyPl != null)
		{
			playersToRemove.Add(lobbyPl);
			MarkLobbyPlayerRemoved(lobbyPl.networkNumber);
		}
	}

	private void MarkLobbyPlayerRemoved(int networkNumber)
	{
		if (networkNumber > 0 && networkNumber <= PlayerManager.maxPlayers)
		{
			removedNetworkNumbers.Add(networkNumber);
		}
		else
		{
			Debug.LogError("Invalid network number...");
		}
	}

	private void NotifyPlayerRemoved(int networkNumber)
	{
		Debug.LogWarning("Sending LobbyPlayerRemovedEvent for player " + networkNumber);
		GameEventManager.SendEvent(new LobbyPlayerRemovedEvent(networkNumber));
	}

	public void OnSceneLoaded(Scene newScene, LoadSceneMode mode)
	{
		GameState.OnSceneChange();
	}

	public void OnSceneUnloaded(Scene scene)
	{
	}

	public void OnSceneChange(Scene previousScene, Scene newScene)
	{
		if (!(instance == this))
		{
			return;
		}
		LastSceneLoaded = newScene.name;
		SceneLoadCounter++;
		Debug.Log("Scene change: " + SceneManager.GetSceneAt(0).name + ((newScene.buildIndex == 1) ? ", destroying lobby manager" : ""));
		SaveSystemProtector.Protect();
		if (newScene.name == "TreeHouseLobby")
		{
			if (LobbyManager.instance != null)
			{
				LobbyManager.instance.lobbyScene = "TreeHouseLobby";
				NetworkManager.networkSceneName = null;
				LobbyManager.instance.onlineScene = null;
				LobbyManager.instance.playScene = "Empty";
				LobbyManager.instance.offlineScene = null;
			}
			SceneLoadCounter = 0;
		}
		if (newScene.name == "MainMenu")
		{
			LobbyPlayer[] array = UnityEngine.Object.FindObjectsOfType<LobbyPlayer>();
			GamePlayer[] array2 = UnityEngine.Object.FindObjectsOfType<GamePlayer>();
			LobbyPlayer[] array3 = array;
			foreach (LobbyPlayer lobbyPl in array3)
			{
				MarkLobbyPlayerToRemove(lobbyPl);
			}
			GamePlayer[] array4 = array2;
			for (int i = 0; i < array4.Length; i++)
			{
				UnityEngine.Object.Destroy(array4[i].gameObject);
			}
			if (Matchmaker.Instance.CurrentLobby != null)
			{
				Matchmaker.Instance.LeaveLobby();
			}
			if (NetworkServer.active)
			{
				NetworkServer.Shutdown();
			}
			if (NetworkClient.active)
			{
				NetworkClient.ShutdownAll();
			}
			LoadingInterstitialSplash.Instance.FadeOut();
		}
	}

	public void AbortGameInProgress(string abortReason = null, LobbyManager.KickReasons kickReason = LobbyManager.KickReasons.NONE)
	{
		Debug.LogWarning("AbortGameInProgress called with reason " + ((abortReason != null) ? abortReason : "None"));
		if (AnalyticsWrapper.EnabledOnPlatform)
		{
			GameControl gameControl = UnityEngine.Object.FindObjectOfType<GameControl>();
			if (gameControl != null)
			{
				VersusControl component = gameControl.GetComponent<VersusControl>();
				int playerRank = 0;
				int pointSpread = 0;
				if (component != null)
				{
					int[] scores = component.Scores;
					int num = int.MaxValue;
					int num2 = int.MinValue;
					for (int i = 0; i < scores.Length; i++)
					{
						if (scores[i] < num)
						{
							num = scores[i];
						}
						if (scores[i] > num2)
						{
							num2 = scores[i];
						}
					}
					pointSpread = num2 - num;
				}
				foreach (Player item in PlayerManager.GetInstance())
				{
					if (item != null)
					{
						if (component != null)
						{
							playerRank = component.GetRank(item.AssociatedLobbyPlayer.networkNumber);
						}
						AnalyticEvent.PlayerLeftMatchEvent(gameControl.MatchGuid, playerRank, pointSpread, kickReason != LobbyManager.KickReasons.NONE);
					}
				}
			}
			else
			{
				LevelSelectController levelSelectController = UnityEngine.Object.FindObjectOfType<LevelSelectController>();
				if (levelSelectController != null)
				{
					int matchesPlayed = Matchmaker.Instance.MatchesPlayed;
					foreach (Player item2 in PlayerManager.GetInstance())
					{
						if (item2 != null)
						{
							if (Matchmaker.CurrentMatchmakingLobby != null)
							{
								AnalyticEvent.PlayerLeftTreehouseEvent(Matchmaker.CurrentMatchmakingLobby.GetLobbyGuid(), matchesPlayed, levelSelectController.TimeInTreehouse, kickReason != LobbyManager.KickReasons.NONE);
							}
							else
							{
								Debug.LogError("ERROR: Matchmaker.CurrentMatchmakingLobby is null - could not send PlayerLeftTreehouseEvent");
							}
						}
					}
					Matchmaker.Instance.ResetMatchesPlayed();
				}
				else
				{
					Debug.LogWarning("Quitting without a treehouse or game controller!");
				}
			}
		}
		if (!shuttingDown && !abortGameRequested)
		{
			SaveSystemProtector.Protect();
			SetAbortReason(abortReason);
			abortGameRequested = true;
			Debug.Log("Abort game requested!");
		}
	}

	private void AbortGame()
	{
		abortGameRequested = false;
		if (SceneManager.GetActiveScene().name != "MainMenu")
		{
			Debug.LogWarning("Aborting game and returning to main menu");
			if (LobbyManager.instance != null)
			{
				LobbyManager.instance.Disconnect();
				UnityEngine.Object.Destroy(LobbyManager.instance.gameObject);
			}
			if (abortReason != null)
			{
				UserMessageManager.Instance.UserMessage(abortReason, 5f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: false);
				abortReason = null;
			}
			if (SceneManager.GetActiveScene().name != "MainMenu")
			{
				StartCoroutine(FadeOutToLoad());
			}
		}
	}

	private IEnumerator FadeOutToLoad()
	{
		LoadingInterstitialSplash FadeOut = LoadingInterstitialSplash.Instance;
		if (FadeOut != null)
		{
			FadeOut.SkipOff();
			FadeOut.FadeIn();
			while (FadeOut.State != UISplashScreen.STATE.SHOW)
			{
				yield return null;
			}
		}
		IEnumerator gentleLoad = SceneManagerWrapper.DoGentleSceneLoad("MainMenu");
		while (gentleLoad.MoveNext())
		{
			yield return null;
		}
	}

	public static void AbortGameInProgressGracefully(string abortReason = null)
	{
		Debug.LogWarning("AbortGameInProgressGracefully called with reason " + ((abortReason != null) ? abortReason : "None"));
		SaveSystemProtector.Protect();
		if (LobbyManager.instance.CurrentGameController != null)
		{
			LobbyManager.instance.CurrentGameController.DeclareSessionDead();
			LobbyManager.instance.CurrentGameController.BackToMainMenu(abortReason);
		}
		else if (LobbyManager.instance.CurrentLevelSelectController != null)
		{
			LobbyManager.instance.CurrentLevelSelectController.BackToMainMenu(abortReason);
		}
		else
		{
			Instance.AbortGameInProgress(abortReason);
		}
	}

	public static void WaitForMainMenu(Action onMainMenuLoaded)
	{
		mainMenuActions.Add(onMainMenuLoaded);
	}

	public static void OnMainMenuUpdate()
	{
		foreach (Action mainMenuAction in mainMenuActions)
		{
			mainMenuAction();
		}
		mainMenuActions.Clear();
	}

	public void SetAbortReason(string abortReason)
	{
		if (this.abortReason == null)
		{
			Debug.Log("Abort reason set to " + abortReason);
			this.abortReason = abortReason;
		}
		else
		{
			Debug.LogWarning("Did not set abort reason to " + abortReason);
		}
	}

	public void ClearAbortReason()
	{
		abortReason = null;
	}

	public static void AddExpectedSceneLoad(string sceneName)
	{
		if (expectedSceneLoads.Contains(sceneName))
		{
			Debug.LogError("WARNING! Scene load added more than once: " + sceneName);
		}
		expectedSceneLoads.Add(sceneName);
	}

	public static void AcknowledgeSceneLoad(string sceneName)
	{
		int num = expectedSceneLoads.FindIndex((string s) => s == sceneName);
		if (num != -1)
		{
			expectedSceneLoads.RemoveAt(num);
		}
	}

	public static void BeforeLoadingTreehouse()
	{
		NetworkManager.networkSceneName = "";
	}
}
