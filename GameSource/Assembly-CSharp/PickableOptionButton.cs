using System.Collections;
using I2.Loc;
using UnityEngine;

public class PickableOptionButton : PickableButton
{
	public enum OptionButtonJobs
	{
		GoToControlPage,
		GoToLangauages,
		WindowedToggle,
		ResolutionUp,
		ResolutionDown,
		ResolutionText,
		ResolutionApply,
		VSyncToggle,
		MusicVolume,
		SFXVolume,
		ResetKeyboard,
		ShowVersionToggle,
		TwitchVotingToggle,
		TwitchChatDisplayToggle,
		TwitchChatHostOnlyToggle,
		GoToTwitchPage,
		Back,
		TwitchHelpULR,
		QualityLabelCurrent,
		QualityUp,
		QualityDown,
		BackgroundAudio
	}

	public OptionButtonJobs job;

	public static Resolution nextResolution;

	protected override void Start()
	{
		base.Start();
		nextResolution = Screen.currentResolution;
	}

	public override void OnAccept(PickCursor pickCursor)
	{
		base.OnAccept(pickCursor);
		switch (job)
		{
		case OptionButtonJobs.GoToControlPage:
			inventoryBook.GotoPage(fakeVariable: true, InventoryPage.PageTypes.ControlsPage, enableBack: true);
			break;
		case OptionButtonJobs.GoToLangauages:
			inventoryBook.GotoPage(fakeVariable: true, InventoryPage.PageTypes.LanguagesPage, enableBack: true);
			inventoryBook.backEnabled = true;
			break;
		case OptionButtonJobs.WindowedToggle:
			Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, !Screen.fullScreen);
			UnityEngine.Cursor.lockState = CursorLockMode.None;
			StartCoroutine(resetCursorLock());
			break;
		case OptionButtonJobs.ResolutionUp:
		{
			bool flag = false;
			for (int i = 0; i < TabletOptionsScreen.ValidResolutions.Count; i++)
			{
				if (TabletOptionsScreen.ValidResolutions[i].ToString() == nextResolution.ToString())
				{
					int num = i + 1;
					if (num >= TabletOptionsScreen.ValidResolutions.Count)
					{
						num = TabletOptionsScreen.ValidResolutions.Count - 1;
					}
					nextResolution = TabletOptionsScreen.ValidResolutions[num];
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				nextResolution = TabletOptionsScreen.ValidResolutions[0];
			}
			break;
		}
		case OptionButtonJobs.ResolutionDown:
		{
			bool flag2 = false;
			for (int j = 0; j < TabletOptionsScreen.ValidResolutions.Count; j++)
			{
				if (TabletOptionsScreen.ValidResolutions[j].ToString() == nextResolution.ToString())
				{
					int num2 = j - 1;
					if (num2 <= -1)
					{
						num2 = 0;
					}
					nextResolution = TabletOptionsScreen.ValidResolutions[num2];
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				nextResolution = TabletOptionsScreen.ValidResolutions[0];
			}
			break;
		}
		case OptionButtonJobs.ResolutionApply:
			Screen.SetResolution(nextResolution.width, nextResolution.height, Screen.fullScreen);
			UnityEngine.Cursor.lockState = CursorLockMode.None;
			StartCoroutine(resetCursorLock());
			break;
		case OptionButtonJobs.VSyncToggle:
			if (QualitySettings.vSyncCount == 1)
			{
				QualitySettings.vSyncCount = 0;
			}
			else
			{
				QualitySettings.vSyncCount = 1;
			}
			if (ControllerMonitor.Instance.IsMainControllerSet)
			{
				StatTracker.Instance.GetSaveFileDataForMainUser().VSync = QualitySettings.vSyncCount == 1;
			}
			break;
		case OptionButtonJobs.ResetKeyboard:
			StatTracker.Instance.ClearKeybindings();
			break;
		case OptionButtonJobs.ShowVersionToggle:
		{
			SaveFileData saveFileDataForMainUser2 = StatTracker.Instance.GetSaveFileDataForMainUser();
			saveFileDataForMainUser2.HideVersion = !saveFileDataForMainUser2.HideVersion;
			break;
		}
		case OptionButtonJobs.TwitchVotingToggle:
		{
			GameSettings instance2 = GameSettings.GetInstance();
			instance2.enableTwitchVoting = !instance2.enableTwitchVoting;
			break;
		}
		case OptionButtonJobs.TwitchChatDisplayToggle:
		{
			GameSettings instance = GameSettings.GetInstance();
			instance.showTwitchChat = !instance.showTwitchChat;
			break;
		}
		case OptionButtonJobs.GoToTwitchPage:
			inventoryBook.GotoPage(fakeVariable: true, InventoryPage.PageTypes.TwitchOptionsPage, enableBack: true);
			break;
		case OptionButtonJobs.Back:
			inventoryBook.GotoPage(inventoryBook.backPage);
			break;
		case OptionButtonJobs.TwitchHelpULR:
			OpenURLWrapper.Open("http://www.partybox.horse");
			AnalyticEvent.LinkClickedEvent(AnalyticEvent.SocialLink.Twitch, "http://www.partybox.horse");
			break;
		case OptionButtonJobs.QualityUp:
		{
			int vSyncCount2 = QualitySettings.vSyncCount;
			QualitySettings.DecreaseLevel();
			QualitySettings.vSyncCount = vSyncCount2;
			break;
		}
		case OptionButtonJobs.QualityDown:
		{
			int vSyncCount = QualitySettings.vSyncCount;
			QualitySettings.IncreaseLevel();
			QualitySettings.vSyncCount = vSyncCount;
			break;
		}
		case OptionButtonJobs.BackgroundAudio:
		{
			SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
			saveFileDataForMainUser.BackgroundAudio = !saveFileDataForMainUser.BackgroundAudio;
			break;
		}
		case OptionButtonJobs.ResolutionText:
		case OptionButtonJobs.MusicVolume:
		case OptionButtonJobs.SFXVolume:
		case OptionButtonJobs.TwitchChatHostOnlyToggle:
		case OptionButtonJobs.QualityLabelCurrent:
			break;
		}
	}

	private IEnumerator resetCursorLock()
	{
		yield return null;
		yield return new WaitForEndOfFrame();
		UnityEngine.Cursor.lockState = CursorLockMode.Confined;
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
		case OptionButtonJobs.WindowedToggle:
			if (Screen.fullScreen)
			{
				buttonText.text = ScriptLocalization.RuleBook.Off;
			}
			else
			{
				buttonText.text = ScriptLocalization.RuleBook.On;
			}
			break;
		case OptionButtonJobs.ResolutionText:
			buttonText.text = nextResolution.ToString();
			break;
		case OptionButtonJobs.ResolutionApply:
			if (nextResolution.width == Screen.width && nextResolution.height == Screen.height)
			{
				buttonText.enabled = false;
				Collider2D[] pickColliders = PickColliders;
				for (int i = 0; i < pickColliders.Length; i++)
				{
					pickColliders[i].enabled = false;
				}
			}
			else
			{
				buttonText.enabled = true;
				Collider2D[] pickColliders = PickColliders;
				for (int i = 0; i < pickColliders.Length; i++)
				{
					pickColliders[i].enabled = true;
				}
			}
			break;
		case OptionButtonJobs.VSyncToggle:
			if (QualitySettings.vSyncCount == 0)
			{
				buttonText.text = ScriptLocalization.Options_Options.VSyncOff;
			}
			else if (QualitySettings.vSyncCount == 1)
			{
				buttonText.text = ScriptLocalization.Options_Options.VSyncnOn;
			}
			break;
		case OptionButtonJobs.ShowVersionToggle:
			if (ControllerMonitor.Instance.IsMainControllerSet)
			{
				if (!StatTracker.Instance.GetSaveFileDataForMainUser().HideVersion)
				{
					buttonText.text = ScriptLocalization.RuleBook.On;
				}
				else
				{
					buttonText.text = ScriptLocalization.RuleBook.Off;
				}
			}
			break;
		case OptionButtonJobs.TwitchVotingToggle:
			if (GameSettings.GetInstance().enableTwitchVoting)
			{
				buttonText.text = ScriptLocalization.RuleBook.On;
			}
			else
			{
				buttonText.text = ScriptLocalization.RuleBook.Off;
			}
			break;
		case OptionButtonJobs.TwitchChatDisplayToggle:
			if (GameSettings.GetInstance().showTwitchChat)
			{
				buttonText.text = ScriptLocalization.RuleBook.On;
			}
			else
			{
				buttonText.text = ScriptLocalization.RuleBook.Off;
			}
			break;
		case OptionButtonJobs.QualityLabelCurrent:
			if (QualitySettings.masterTextureLimit == 0)
			{
				buttonText.text = ScriptLocalization.Options_Options.QualityHi;
			}
			else if (QualitySettings.masterTextureLimit == 1)
			{
				buttonText.text = ScriptLocalization.Options_Options.QualityMed;
			}
			else if (QualitySettings.masterTextureLimit == 2)
			{
				buttonText.text = ScriptLocalization.Options_Options.QualityLo;
			}
			break;
		case OptionButtonJobs.BackgroundAudio:
			if (ControllerMonitor.Instance.IsMainControllerSet)
			{
				if (StatTracker.Instance.GetSaveFileDataForMainUser().BackgroundAudio)
				{
					buttonText.text = ScriptLocalization.RuleBook.On;
				}
				else
				{
					buttonText.text = ScriptLocalization.RuleBook.Off;
				}
			}
			break;
		case OptionButtonJobs.GoToControlPage:
		case OptionButtonJobs.GoToLangauages:
		case OptionButtonJobs.ResolutionUp:
		case OptionButtonJobs.ResolutionDown:
		case OptionButtonJobs.MusicVolume:
		case OptionButtonJobs.SFXVolume:
		case OptionButtonJobs.ResetKeyboard:
		case OptionButtonJobs.TwitchChatHostOnlyToggle:
		case OptionButtonJobs.GoToTwitchPage:
		case OptionButtonJobs.Back:
		case OptionButtonJobs.TwitchHelpULR:
		case OptionButtonJobs.QualityUp:
		case OptionButtonJobs.QualityDown:
			break;
		}
	}
}
