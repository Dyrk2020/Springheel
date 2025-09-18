using System.Collections;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class PickableMainMenuButton : PickableButton
{
	public enum MenuButtonJobs
	{
		StartGame,
		JoinOnline,
		Options,
		Credits,
		ExitProgram,
		ExitProgramMessage,
		ExitProgramYes,
		ExitProgramNo,
		VersionNumber,
		Twitter,
		Reddit,
		Store,
		SpecialMessage,
		ChallengeTimesWarning,
		Gamertag,
		BanMessage,
		Discord
	}

	public MenuButtonJobs job;

	public InventoryPage.PageTypes targetPageType;

	public static bool showExitgameMessage;

	public string SceneName;

	public Localize localizeComponent;

	public RawImage rawImage;

	protected override void Start()
	{
		base.Start();
		if ((bool)localizeComponent)
		{
			StartCoroutine(WaitToTryLocalizingUpdate());
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
	}

	private IEnumerator WaitToTryLocalizingUpdate()
	{
		yield return new WaitForSeconds(10f);
		localizeComponent.OnLocalize(Force: true);
	}

	private IEnumerator waitForUserLogin()
	{
		if (job == MenuButtonJobs.Gamertag)
		{
			while (!PlayerManager.GetInstance().FirstUserLoggedIn)
			{
				yield return null;
			}
		}
	}

	public override void OnAccept(PickCursor pickCursor)
	{
		if (job == MenuButtonJobs.SpecialMessage && ScriptLocalization.SpecialMessageURL == "" && ScriptLocalization.CurrentVersion == GameSettings.GetInstance().VersionNumber)
		{
			return;
		}
		base.OnAccept(pickCursor);
		if (!Visible)
		{
			return;
		}
		showExitgameMessage = false;
		switch (job)
		{
		case MenuButtonJobs.Options:
			inventoryBook.GotoPage(fakeVariable: true, InventoryPage.PageTypes.OptionsPage, enableBack: true);
			break;
		case MenuButtonJobs.Credits:
			inventoryBook.GotoPage(fakeVariable: true, InventoryPage.PageTypes.Credits, enableBack: true);
			break;
		case MenuButtonJobs.ExitProgram:
			showExitgameMessage = true;
			break;
		case MenuButtonJobs.ExitProgramYes:
			showExitgameMessage = false;
			QuitGame();
			break;
		case MenuButtonJobs.ExitProgramNo:
			showExitgameMessage = false;
			break;
		case MenuButtonJobs.Twitter:
			OpenURLWrapper.Open("https://twitter.com/ClevEndeavGames");
			AnalyticEvent.LinkClickedEvent(AnalyticEvent.SocialLink.Twitter, "https://twitter.com/ClevEndeavGames");
			break;
		case MenuButtonJobs.Discord:
			OpenURLWrapper.Open(" https://discord.gg/Gb2tUgA");
			AnalyticEvent.LinkClickedEvent(AnalyticEvent.SocialLink.Discord, "https://discord.gg/Gb2tUgA");
			break;
		case MenuButtonJobs.Reddit:
			OpenURLWrapper.Open("https://www.reddit.com/r/ultimatechickenhorse");
			AnalyticEvent.LinkClickedEvent(AnalyticEvent.SocialLink.Reddit, "https://www.reddit.com/r/ultimatechickenhorse");
			break;
		case MenuButtonJobs.Store:
			OpenURLWrapper.Open("http://www.cleverendeavourgames.com/shop/");
			AnalyticEvent.LinkClickedEvent(AnalyticEvent.SocialLink.Shop, "http://www.cleverendeavourgames.com/shop/");
			break;
		case MenuButtonJobs.SpecialMessage:
			if (ScriptLocalization.CurrentVersion != GameSettings.GetInstance().VersionNumber)
			{
				OpenURLWrapper.Open(ScriptLocalization.New_Version_Available_URL);
				AnalyticEvent.LinkClickedEvent(AnalyticEvent.SocialLink.Update, ScriptLocalization.New_Version_Available_URL);
			}
			else if (ScriptLocalization.SpecialMessageURL != "" && ScriptLocalization.SpecialMessage != "")
			{
				OpenURLWrapper.Open(ScriptLocalization.SpecialMessageURL);
				AnalyticEvent.LinkClickedEvent(AnalyticEvent.SocialLink.Announcement, ScriptLocalization.SpecialMessageURL);
			}
			break;
		case MenuButtonJobs.StartGame:
		case MenuButtonJobs.JoinOnline:
		case MenuButtonJobs.ExitProgramMessage:
		case MenuButtonJobs.VersionNumber:
		case MenuButtonJobs.ChallengeTimesWarning:
		case MenuButtonJobs.Gamertag:
		case MenuButtonJobs.BanMessage:
			break;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!Visible || !initialized)
		{
			return;
		}
		switch (job)
		{
		case MenuButtonJobs.ExitProgramMessage:
			Show(showExitgameMessage);
			break;
		case MenuButtonJobs.ExitProgramYes:
			Show(showExitgameMessage);
			break;
		case MenuButtonJobs.ExitProgramNo:
			Show(showExitgameMessage);
			break;
		case MenuButtonJobs.SpecialMessage:
			if (ScriptLocalization.CurrentVersion != GameSettings.GetInstance().VersionNumber)
			{
				buttonText.text = ScriptLocalization.New_Version_Available;
			}
			break;
		case MenuButtonJobs.ChallengeTimesWarning:
			Show(showExitgameMessage && ChallengeTimeCache.HasDataLeftToUpload);
			break;
		case MenuButtonJobs.StartGame:
		case MenuButtonJobs.JoinOnline:
		case MenuButtonJobs.Options:
		case MenuButtonJobs.Credits:
		case MenuButtonJobs.ExitProgram:
		case MenuButtonJobs.VersionNumber:
		case MenuButtonJobs.Twitter:
		case MenuButtonJobs.Reddit:
		case MenuButtonJobs.Store:
		case MenuButtonJobs.Gamertag:
		case MenuButtonJobs.BanMessage:
			break;
		}
	}

	public override void Enable(bool onOff = true)
	{
		base.Enable(onOff);
		showExitgameMessage = false;
		switch (job)
		{
		case MenuButtonJobs.ExitProgramMessage:
			buttonText.enabled = false;
			break;
		case MenuButtonJobs.ExitProgramYes:
			buttonText.enabled = false;
			break;
		case MenuButtonJobs.ExitProgramNo:
			buttonText.enabled = false;
			break;
		case MenuButtonJobs.VersionNumber:
			buttonText.text = GameSettings.GetInstance().VersionNumber.ToString();
			break;
		case MenuButtonJobs.StartGame:
		case MenuButtonJobs.JoinOnline:
		case MenuButtonJobs.Options:
		case MenuButtonJobs.Credits:
		case MenuButtonJobs.ExitProgram:
			break;
		}
	}

	protected void Show(bool show)
	{
		if (buttonText != null)
		{
			buttonText.enabled = show;
		}
		Collider2D[] pickColliders = PickColliders;
		for (int i = 0; i < pickColliders.Length; i++)
		{
			pickColliders[i].enabled = show;
		}
		if (image != null)
		{
			image.enabled = show;
		}
		if (sprite != null)
		{
			sprite.enabled = show;
		}
	}

	public void QuitGame(Controller sentBy = null)
	{
		GameState.GetInstance().CleanupBeforeQuit();
		Application.Quit();
	}
}
