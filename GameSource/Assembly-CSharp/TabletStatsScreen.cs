using System.Collections.Generic;
using System.Globalization;
using GameEvent;
using I2.Loc;
using Steamworks;
using UnityEngine;

public class TabletStatsScreen : TabletScreen, IGameEventListener
{
	public TabletSubdialogController subdialogController;

	public RectTransform statsDialog;

	public RectTransform animalStatsDialog;

	public RectTransform itemStatsDialog;

	public RectTransform levelStatsDialog;

	public RectTransform resetStatsDialog;

	private SaveFileData saveFileData;

	public TabletTextLabel VersionNumber;

	[Header("AnimalsStats")]
	public TabletStatData AnimalStatPrefab;

	public RectTransform AnimalStatDataArrayContainer;

	public List<TabletStatData> AnimalStatsList = new List<TabletStatData>();

	public TabletTextLabel TotalWins;

	public TabletTextLabel TotalGoals;

	public TabletTextLabel TotalDeaths;

	public TabletSimpleScroll animalStatsScroller;

	[Header("LevelsStats")]
	public TabletStatData LevelsStatPrefab;

	public RectTransform LevelsStatDataArrayContainer;

	public List<TabletStatData> LevelsStatsList = new List<TabletStatData>();

	public TabletTextLabel TotalPlays;

	public TabletTextLabel TotalLevelTime;

	public TabletTextLabel TotalRounds;

	public TabletTextLabel PartyModePercent;

	public TabletTextLabel CreativeModePercent;

	public TabletSimpleScroll levelStatsScroller;

	[Header("AllCharactersStats")]
	public TabletTextLabel Jumps;

	public TabletTextLabel DistanceRun;

	public TabletTextLabel ComebackPoints;

	public TabletTextLabel SoloPoints;

	public TabletTextLabel PostMortemPoints;

	[Header("ItemsStats")]
	public TabletTextLabel PiecesPlaced;

	public TabletTextLabel PiecesDestroyed;

	public TabletTextLabel TrapsPlaced;

	public TabletTextLabel BombsUsed;

	public TabletTextLabel PiecesGlued;

	public TabletTextLabel LargeContraptionsMade;

	public TabletTextLabel TimesTeleported;

	public TabletTextLabel SpringBounces;

	public TabletTextLabel DeathsBySpikeBall;

	public TabletTextLabel DeathsByArrow;

	public TabletTextLabel DeathsByTennisBall;

	public TabletTextLabel DeathsBySpinningSaw;

	public TabletTextLabel DeathsByLinearSaw;

	public TabletTextLabel DeathsByPropeller;

	public TabletTextLabel DeathsByFlippingBlock;

	public TabletTextLabel DeathsByBlackHole;

	public TabletTextLabel DeathsByHockeyPuck;

	public TabletTextLabel DeathsByPunchingPlant;

	public TabletTextLabel CoinsCollected;

	public TabletTextLabel TrapPoints;

	public TabletTextLabel DeathsByPressureTriggerSpikes;

	public TabletTextLabel DeathsByWreckingBall;

	public TabletSimpleScroll itemStatsScroller;

	private StatBoolArray charactersUnlocked;

	private StatCountArray characterWins;

	private StatCountArray characterSuccess;

	private StatCountArray characterDeaths;

	private StatBoolArray levelsUnlocked;

	private StatCountArray levelsPlayed;

	private StatFloatArray totalLevelTime;

	private StatCountArray totalLevelRounds;

	private RectTransform lastEnteredSubdialog;

	public TabletTextLabel currentUsername;

	private void Awake()
	{
		ChangeListener(adding: true);
	}

	private void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public virtual void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, adding);
	}

	public override void OnTransitionInBegin()
	{
		if (ControllerMonitor.Instance.IsMainControllerSet)
		{
			if (SteamManager.Initialized)
			{
				currentUsername.text = SteamFriends.GetPersonaName();
			}
			else
			{
				currentUsername.text = "";
			}
		}
		else
		{
			currentUsername.text = "";
		}
	}

	private void Start()
	{
	}

	private void OnEnable()
	{
		if (ControllerMonitor.Instance.IsMainControllerSet)
		{
			UpdateStats();
		}
	}

	public override void OnModalOverlayClosed()
	{
		base.OnModalOverlayClosed();
	}

	private void UpdateButtonValue(TabletRule overlayType)
	{
	}

	private void SlideToSubDialog(RectTransform dialog)
	{
		if (!subdialogController.IsAnimating)
		{
			lastEnteredSubdialog = dialog;
			subdialogController.TransitionLeftTo(dialog);
		}
	}

	public void OnClickAnimalStats(PickCursor pickCursor)
	{
		SlideToSubDialog(animalStatsDialog);
	}

	public void OnClickItemStats(PickCursor pickCursor)
	{
		SlideToSubDialog(itemStatsDialog);
	}

	public void OnClickLevelStats(PickCursor pickCursor)
	{
		SlideToSubDialog(levelStatsDialog);
	}

	public void OnClickResetStats(PickCursor pickCursor)
	{
		AkSoundEngine.PostEvent("UI_UPad_Warning", base.gameObject);
		SlideToSubDialog(resetStatsDialog);
		lastEnteredSubdialog = null;
	}

	public void OnClickConfirmResetStats(PickCursor pickCursor)
	{
		StatTracker.Instance.ClearStatsAndUnlocks();
		subdialogController.TransitionRightTo(statsDialog);
		UpdateStats();
	}

	public void OnClickStatsStats(PickCursor pickCursor)
	{
		subdialogController.TransitionRightTo(statsDialog);
	}

	public void UpdateLiveStates()
	{
		Jumps.text = (saveFileData.GetStat<StatCount>("Jumps").count + saveFileData.GetStat<StatCount>("WallJumps").count).ToString();
		DistanceRun.text = saveFileData.GetStat<StatFloat>("DistanceRun").value.ToString("F", CultureInfo.CreateSpecificCulture(LocalizationManager.CurrentLanguageCode));
	}

	public void UpdateStats()
	{
		VersionNumber.text = "v" + GameSettings.GetInstance().VersionNumberToShow;
		saveFileData = StatTracker.Instance.GetSaveFileDataForMainUser();
		charactersUnlocked = saveFileData.GetStat<StatBoolArray>("CharactersUnlocked");
		characterWins = saveFileData.GetStat<StatCountArray>("CharacterWins");
		characterSuccess = saveFileData.GetStat<StatCountArray>("CharacterSuccess");
		characterDeaths = saveFileData.GetStat<StatCountArray>("CharacterDeaths");
		levelsUnlocked = saveFileData.GetStat<StatBoolArray>("LevelsUnlocked");
		levelsPlayed = saveFileData.GetStat<StatCountArray>("LevelsPlayed");
		totalLevelTime = saveFileData.GetStat<StatFloatArray>("TotalLevelTime");
		totalLevelRounds = saveFileData.GetStat<StatCountArray>("TotalLevelRounds");
		UpdateLiveStates();
		UpdateAnimals();
		UpdateLevels();
		UpdateItems();
		ComebackPoints.text = saveFileData.GetStat<StatCount>("ComebackPointsEarned").count.ToString();
		SoloPoints.text = saveFileData.GetStat<StatCount>("SoloPointsEarned").count.ToString();
		PostMortemPoints.text = saveFileData.GetStat<StatCount>("PostmortemVictories").count.ToString();
		int num = 0;
		int[] values = characterWins.values;
		foreach (int num2 in values)
		{
			num += num2;
		}
		TotalWins.text = num.ToString();
		int num3 = 0;
		values = characterSuccess.values;
		foreach (int num4 in values)
		{
			num3 += num4;
		}
		TotalGoals.text = num3.ToString();
		TotalDeaths.text = saveFileData.GetStat<StatCount>("TotalDeaths").count.ToString();
		TotalPlays.text = saveFileData.GetStat<StatCount>("GamesPlayed").count.ToString();
		TotalRounds.text = saveFileData.GetStat<StatCount>("TotalRounds").count.ToString();
		float num5 = 0f;
		foreach (TabletStatData levelsStats in LevelsStatsList)
		{
			num5 += totalLevelTime.values[(int)levelsStats.levelType];
		}
		TotalLevelTime.text = SecondsToHoursAndMinutes((int)num5);
	}

	public void UpdateAnimals()
	{
		foreach (TabletStatData animalStats in AnimalStatsList)
		{
			animalStats.gameObject.SetActive(charactersUnlocked.values[(int)animalStats.animalType]);
			animalStats.DataSlots1.text = characterWins.values[(int)animalStats.animalType].ToString();
			animalStats.DataSlots2.text = characterSuccess.values[(int)animalStats.animalType].ToString();
			animalStats.DataSlots3.text = characterDeaths.values[(int)animalStats.animalType].ToString();
		}
	}

	private string SecondsToHoursAndMinutes(int seconds)
	{
		int num = seconds / 60 % 60;
		return ((seconds - num * 60) / 3600).ToString("00") + ":" + num.ToString("00");
	}

	public void UpdateLevels()
	{
		foreach (TabletStatData levelsStats in LevelsStatsList)
		{
			levelsStats.gameObject.SetActive(levelsUnlocked.values[(int)levelsStats.levelType]);
			levelsStats.DataSlots1.text = levelsPlayed.values[(int)levelsStats.levelType].ToString();
			levelsStats.DataSlots2.text = SecondsToHoursAndMinutes((int)totalLevelTime.values[(int)levelsStats.levelType]);
			levelsStats.DataSlots3.text = totalLevelRounds.values[(int)levelsStats.levelType].ToString();
		}
		if (saveFileData.GetStat<StatCount>("GamesPlayed").count > 0)
		{
			PartyModePercent.text = ((float)saveFileData.GetStat<StatCount>("PartyModeGamesPlayed").count / (float)saveFileData.GetStat<StatCount>("GamesPlayed").count).ToString("P1", CultureInfo.CreateSpecificCulture(LocalizationManager.CurrentLanguageCode));
		}
		else
		{
			PartyModePercent.text = "0%";
		}
		if (saveFileData.GetStat<StatCount>("GamesPlayed").count > 0)
		{
			CreativeModePercent.text = ((float)saveFileData.GetStat<StatCount>("CreativeModeGamesPlayed").count / (float)saveFileData.GetStat<StatCount>("GamesPlayed").count).ToString("P1", CultureInfo.CreateSpecificCulture(LocalizationManager.CurrentLanguageCode));
		}
		else
		{
			CreativeModePercent.text = "0%";
		}
	}

	public void UpdateItems()
	{
		PiecesPlaced.text = saveFileData.GetStat<StatCount>("PiecesPlaced").count.ToString();
		PiecesDestroyed.text = saveFileData.GetStat<StatCount>("PiecesDestroyed").count.ToString();
		TrapsPlaced.text = saveFileData.GetStat<StatCount>("TrapsPlaced").count.ToString();
		BombsUsed.text = saveFileData.GetStat<StatCount>("BombsPlaced").count.ToString();
		PiecesGlued.text = saveFileData.GetStat<StatCount>("PiecesGlued").count.ToString();
		LargeContraptionsMade.text = saveFileData.GetStat<StatCount>("LargeContraptionsMade").count.ToString();
		TimesTeleported.text = saveFileData.GetStat<StatCount>("TimesTeleported").count.ToString();
		SpringBounces.text = saveFileData.GetStat<StatCount>("SpringBounces").count.ToString();
		DeathsBySpikeBall.text = saveFileData.GetStat<StatCount>("DeathsBySpikeBall").count.ToString();
		DeathsByArrow.text = saveFileData.GetStat<StatCount>("DeathsByArrow").count.ToString();
		DeathsByTennisBall.text = saveFileData.GetStat<StatCount>("DeathsByTennisBall").count.ToString();
		DeathsBySpinningSaw.text = saveFileData.GetStat<StatCount>("DeathsBySpinningSaw").count.ToString();
		DeathsByLinearSaw.text = saveFileData.GetStat<StatCount>("DeathsByLinearSaw").count.ToString();
		DeathsByPropeller.text = saveFileData.GetStat<StatCount>("DeathsByPropeller").count.ToString();
		DeathsByFlippingBlock.text = saveFileData.GetStat<StatCount>("DeathsByFlippingBlock").count.ToString();
		DeathsByBlackHole.text = saveFileData.GetStat<StatCount>("DeathsByBlackHole").count.ToString();
		DeathsByHockeyPuck.text = saveFileData.GetStat<StatCount>("DeathsByHockeyPuck").count.ToString();
		DeathsByPunchingPlant.text = saveFileData.GetStat<StatCount>("DeathsByPunchingPlant").count.ToString();
		CoinsCollected.text = saveFileData.GetStat<StatCount>("CoinsCollected").count.ToString();
		TrapPoints.text = saveFileData.GetStat<StatCount>("TrapPointsEarned").count.ToString();
		DeathsByPressureTriggerSpikes.text = saveFileData.GetStat<StatCount>("DeathsByPressureTriggerSpikes").count.ToString();
		DeathsByWreckingBall.text = saveFileData.GetStat<StatCount>("DeathsByWreckingBall").count.ToString();
	}

	public override bool OnPressBack(PickCursor pickCursor)
	{
		if (!subdialogController.IsOnMainSubdialog)
		{
			subdialogController.PopSubdialog();
			return true;
		}
		return base.OnPressBack(pickCursor);
	}

	public virtual void handleEvent(global::GameEvent.GameEvent e)
	{
		_ = e is NetworkMessageReceivedEvent;
	}

	public override void OnCursorScroll(Vector2 scrollAmount)
	{
		if (subdialogController.currentSubdialog == itemStatsDialog)
		{
			itemStatsScroller.ApplyScrolling(scrollAmount.y);
		}
		else if (subdialogController.currentSubdialog == levelStatsDialog)
		{
			levelStatsScroller.ApplyScrolling(scrollAmount.y);
		}
		else if (subdialogController.currentSubdialog == animalStatsDialog)
		{
			animalStatsScroller.ApplyScrolling(scrollAmount.y);
		}
	}

	public override bool OnRotateLeft(PickCursor pickCursor)
	{
		if (pickCursor.lastRotateWasMouseWheel)
		{
			if (subdialogController.currentSubdialog == itemStatsDialog)
			{
				if (Modifiers.GetInstance().CameraFlippedOnX)
				{
					itemStatsScroller.OnClickScrollPlus(pickCursor);
				}
				else
				{
					itemStatsScroller.OnClickScrollMinus(pickCursor);
				}
				return true;
			}
			if (subdialogController.currentSubdialog == levelStatsDialog)
			{
				if (Modifiers.GetInstance().CameraFlippedOnX)
				{
					levelStatsScroller.OnClickScrollPlus(pickCursor);
				}
				else
				{
					levelStatsScroller.OnClickScrollMinus(pickCursor);
				}
				return true;
			}
			if (subdialogController.currentSubdialog == animalStatsDialog)
			{
				if (Modifiers.GetInstance().CameraFlippedOnX)
				{
					animalStatsScroller.OnClickScrollPlus(pickCursor);
				}
				else
				{
					animalStatsScroller.OnClickScrollMinus(pickCursor);
				}
				return true;
			}
		}
		return false;
	}

	public override bool OnRotateRight(PickCursor pickCursor)
	{
		if (pickCursor.lastRotateWasMouseWheel)
		{
			if (subdialogController.currentSubdialog == itemStatsDialog)
			{
				if (Modifiers.GetInstance().CameraFlippedOnX)
				{
					itemStatsScroller.OnClickScrollMinus(pickCursor);
				}
				else
				{
					itemStatsScroller.OnClickScrollPlus(pickCursor);
				}
				return true;
			}
			if (subdialogController.currentSubdialog == levelStatsDialog)
			{
				if (Modifiers.GetInstance().CameraFlippedOnX)
				{
					levelStatsScroller.OnClickScrollMinus(pickCursor);
				}
				else
				{
					levelStatsScroller.OnClickScrollPlus(pickCursor);
				}
				return true;
			}
			if (subdialogController.currentSubdialog == animalStatsDialog)
			{
				if (Modifiers.GetInstance().CameraFlippedOnX)
				{
					animalStatsScroller.OnClickScrollMinus(pickCursor);
				}
				else
				{
					animalStatsScroller.OnClickScrollPlus(pickCursor);
				}
				return true;
			}
		}
		else if (subdialogController.currentSubdialog == statsDialog && lastEnteredSubdialog != null)
		{
			subdialogController.TransitionLeftTo(lastEnteredSubdialog);
			return true;
		}
		return false;
	}
}
