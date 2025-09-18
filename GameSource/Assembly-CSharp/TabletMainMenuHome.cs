using System.Collections;
using Cysharp.Threading.Tasks;
using I2.Loc;
using UnityEngine;

public class TabletMainMenuHome : MonoBehaviour
{
	private MainMenuControl mainMenuControl;

	public TabletMainMenuOnlineIndicator onlineIndicator;

	public TabletButton pleaseUpdateButton;

	private bool startGameClicked;

	private bool showingPleaseUpdate = true;

	public void Initialize(MainMenuControl mainMenuControl)
	{
		this.mainMenuControl = mainMenuControl;
		onlineIndicator.enabled = true;
	}

	public void OnClickStartGame(PickCursor pickCursor)
	{
		LoadingInterstitialSplash instance = LoadingInterstitialSplash.Instance;
		if (instance != null && !startGameClicked)
		{
			Debug.Log("Starting Local Play");
			GameSettings instance2 = GameSettings.GetInstance();
			instance2.StartAsHost = true;
			instance2.StartLocal = true;
			instance2.matchInfo = null;
			instance2.GameMode = instance2.DefaultGameMode;
			mainMenuControl.Starting = true;
			LobbyManagerManager.Instance.StartCoroutine(FadeOutToLoad());
			startGameClicked = true;
			instance.SkipOff();
			instance.FadeIn();
		}
	}

	public void OnClickPlayOnline(PickCursor pickCursor)
	{
		Debug.Log("Going for Online Play");
		onlineIndicator.OnClickPlayOnlineButton(delegate(bool success)
		{
			if (success)
			{
				StartCoroutine(WaitToOpenNetworkMenu());
			}
		});
	}

	private IEnumerator WaitToOpenNetworkMenu()
	{
		onlineIndicator.ForceSetOnlineButtonState(spinnerActive: true, buttonActive: false);
		if (SaveSystemProtector.WaitingForSavefileOperations)
		{
			Debug.Log("Waiting for save file operations to finish before opening Network menu");
			while (SaveSystemProtector.WaitingForSavefileOperations)
			{
				yield return null;
			}
		}
		UniTask task = RelayConstants.PopulateAvailableRegions();
		while (!task.Status.IsCompleted())
		{
			yield return null;
		}
		task = RelayConstants.LoadDynamicConfigs();
		while (!task.Status.IsCompleted())
		{
			yield return null;
		}
		task = RegionPinger.PingAllRegions();
		while (!task.Status.IsCompleted())
		{
			yield return null;
		}
		InventoryBook componentInParent = GetComponentInParent<InventoryBook>();
		componentInParent.TurnScreenOn(componentInParent.ScreenPage);
		AkSoundEngine.PostEvent("UI_UPad_StartMenu_PlayOnline", base.gameObject);
		onlineIndicator.ResetForcedSet();
	}

	private IEnumerator FadeOutToLoad()
	{
		LoadingInterstitialSplash FadeOut = LoadingInterstitialSplash.Instance;
		bool autoFade = FadeOut.FadeOutAutomatically;
		FadeOut.FadeOutAutomatically = false;
		while (FadeOut != null && FadeOut.State != UISplashScreen.STATE.SHOW)
		{
			yield return null;
		}
		if (SaveSystemProtector.WaitingForSavefileOperations)
		{
			Debug.Log("Waiting for savefile operations to complete before starting...");
			while (SaveSystemProtector.WaitingForSavefileOperations)
			{
				yield return null;
			}
		}
		switch (GameSettings.GetInstance().GameMode)
		{
		case GameState.GameMode.FREEPLAY:
			AkSoundEngine.PostEvent("Lobby_Freeplay", base.gameObject);
			break;
		case GameState.GameMode.CREATIVE:
			AkSoundEngine.PostEvent("Lobby_Normal", base.gameObject);
			break;
		case GameState.GameMode.PARTY:
			AkSoundEngine.PostEvent("Lobby_PartyMode", base.gameObject);
			break;
		case GameState.GameMode.CHALLENGE:
			AkSoundEngine.PostEvent("Lobby_Challenge", base.gameObject);
			break;
		}
		IEnumerator gentleLoad = SceneManagerWrapper.DoGentleSceneLoad("TreeHouseLobby");
		while (gentleLoad.MoveNext())
		{
			yield return null;
		}
		FadeOut.FadeOutAutomatically = autoFade;
	}

	public void OnClickReddit(PickCursor pickCursor)
	{
		OpenExternalLink("https://www.reddit.com/r/ultimatechickenhorse", AnalyticEvent.SocialLink.Reddit);
	}

	public void OnClickTwitter(PickCursor pickCursor)
	{
		OpenExternalLink("https://www.twitter.com/ClevEndeavGames", AnalyticEvent.SocialLink.Twitter);
	}

	public void OnClickDiscord(PickCursor pickCursor)
	{
		OpenExternalLink("https://www.discord.gg/uch", AnalyticEvent.SocialLink.Discord);
	}

	public void OnClickStore(PickCursor pickCursor)
	{
		OpenExternalLink("http://www.cleverendeavourgames.com/shop/", AnalyticEvent.SocialLink.Shop);
	}

	public void OnClickFunReport(PickCursor pickCursor)
	{
		OpenExternalLink("https://docs.google.com/forms/d/1k6tiBS0YfitNWdTwLBckwpXBS-7f5lVhNTij109Lhbw/viewform?entry.1251884474&entry.776792862&entry.845986739&entry.891621479=" + GameSettings.GetInstance().VersionNumber, AnalyticEvent.SocialLink.FunReport);
	}

	public void OnClickBugReport(PickCursor pickCursor)
	{
		OpenExternalLink("https://docs.google.com/forms/d/1mOGX_DPyc0KR2qDHxXMXZQiTaPdCeEunBV3okaJDtSQ/viewform?entry.1945186752=" + GameSettings.GetInstance().VersionNumber + "&entry.173496600&entry.1116656086&entry.1198694514&entry.1233078894", AnalyticEvent.SocialLink.BugReport);
	}

	public void OnClickPleaseUpdate(PickCursor pickCursor)
	{
		OpenExternalLink(ScriptLocalization.New_Version_Available_URL, AnalyticEvent.SocialLink.Update);
	}

	private void OpenExternalLink(string url, AnalyticEvent.SocialLink linkType)
	{
		OpenURLWrapper.Open(url);
		AnalyticEvent.LinkClickedEvent(linkType, url);
	}

	private void Update()
	{
		if (showingPleaseUpdate)
		{
			if (GameState.GetLocalizationVersionNumber() == GameSettings.GetInstance().VersionNumber)
			{
				showingPleaseUpdate = false;
				pleaseUpdateButton.gameObject.SetActive(value: false);
			}
		}
		else if (GameState.GetLocalizationVersionNumber() != GameSettings.GetInstance().VersionNumber)
		{
			showingPleaseUpdate = true;
			pleaseUpdateButton.gameObject.SetActive(value: true);
		}
	}
}
