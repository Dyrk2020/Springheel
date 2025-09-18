using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using GameEvent;
using GameSparks.Core;
using I2.Loc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TabletSaveAndShareScreen : TabletScreen, IGameEventListener
{
	public enum UploadError
	{
		None,
		Generic,
		NoConnection,
		NotSignedIn
	}

	public RectTransform codeDisplayContainer;

	public TabletTextLabel codeDisplayText;

	public InputField nameEntryField;

	public Image nameEntryFieldBackground;

	public TabletSimpleAnimator nameEntryFieldAnimator;

	public TabletDisableGroup nameEntryFieldDisableGroup;

	public TabletTextLabel levelFullnessText;

	public TabletTextLabel levelFullnessWarningText;

	private bool levelDirty;

	private GameSparksQuery currentQuery;

	private bool clipboardCopyEnabled;

	private string lastUploadedSnapshotName;

	private string lastUploadedSnapshotCode;

	private int lastLevelFullnessScore;

	public bool Uploadscreenshot = true;

	public TabletButton saveModifiersButton;

	public Image saveModifiersXImage;

	public bool saveModifiers = true;

	public TabletButton saveOnlineButton;

	public TabletButton publishButton;

	public TabletTextLabel publishButtonText;

	public TabletToggleButtonGroup levelTypeToggleGroup;

	public TabletToggleButtonGroup shareLocalToggleGroup;

	public TabletToggleButtonGroup publicUnlistedToggleGroup;

	public TabletDisableGroup levelTypeDisableGroup;

	public TabletDisableGroup shareLocalDisableGroup;

	public TabletDisableGroup publicUnlistedDisableGroup;

	public Transform shareLocalContainer;

	public Transform publicUnlistedContainer;

	public Transform publishButtonsContainer;

	public Transform localSaveButtonContainer;

	public Transform PlayLevelButtonContainer;

	public Transform ShareLevelContainer;

	public Transform NameInputContainer;

	public Transform formSectionContainer;

	public Transform restrictUGCOfflineMessage;

	public Transform restrictUGCOnlineMessage;

	public Transform localScoreSaveWarning;

	public TabletTextLabel localSaveStorageCalculating;

	public TabletTextLabel localSaveStorageValue;

	public Transform localSaveStorageLine;

	public TabletPlayButtonController playButtons;

	public GameObject connectButton;

	private SwitchConnectButton switchConnectButton;

	private int levelTypeValue = -1;

	private int shareLocalValue = -1;

	private int publicUnlistedValue = -1;

	private bool hide_online_ugc_features;

	private bool hide_offline_ugc_features;

	public bool DebugSimulateUGCBlocked;

	public bool DebugSimulateNotSignedIn;

	private bool PlayingAlone
	{
		get
		{
			int num = 0;
			NetworkLobbyPlayer[] lobbySlots = LobbyManager.instance.lobbySlots;
			for (int i = 0; i < lobbySlots.Length; i++)
			{
				if (lobbySlots[i] != null)
				{
					num++;
					if (num > 1)
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	private void Awake()
	{
		ChangeListener(adding: true);
		levelFullnessWarningText.gameObject.SetActive(value: false);
		levelFullnessText.text = "0/" + GameSettings.GetInstance().LevelFullnessScoreLimit;
		codeDisplayContainer.gameObject.SetActive(value: false);
	}

	private void Start()
	{
		RecalculateLevelFullness();
		ResetButton(eraseName: true, resetToggles: true);
	}

	private void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<NoteBookDisplayEvent>(this, adding);
		GameEventManager.ChangeListener<PiecePlacedEvent>(this, adding);
		GameEventManager.ChangeListener<DestroyPieceEvent>(this, adding);
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, adding);
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(NoteBookDisplayEvent) && (e as NoteBookDisplayEvent).Opened && levelDirty)
		{
			RecalculateLevelFullness();
			ResetButton(eraseName: true, resetToggles: true);
			levelDirty = false;
		}
		if (type == typeof(PiecePlacedEvent))
		{
			PiecePlacedEvent piecePlacedEvent = e as PiecePlacedEvent;
			levelDirty = true;
			int num = lastLevelFullnessScore;
			lastLevelFullnessScore += piecePlacedEvent.PlacedBlock.placementCost;
			int levelFullnessScoreLimit = GameSettings.GetInstance().LevelFullnessScoreLimit;
			float num2 = (float)num / (float)levelFullnessScoreLimit;
			float num3 = (float)lastLevelFullnessScore / (float)levelFullnessScoreLimit;
			if (GameSettings.GetInstance().GameMode == GameState.GameMode.FREEPLAY)
			{
				if (num2 < 0.5f && num3 >= 0.5f)
				{
					UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.ShareLimitMessage + " 50%", 2f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: true);
				}
				else if (num2 < 0.75f && num3 >= 0.75f)
				{
					UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.ShareLimitMessage + " 75%", 2f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: true);
				}
				else if (num2 < 0.95f && num3 >= 0.95f)
				{
					UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.ShareLimitMessage + " 95%", 2f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: true);
				}
				else if (num2 < 1f && num3 >= 1f)
				{
					UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.ShareLimitMaxMessage, 2f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: true);
				}
			}
		}
		if (type == typeof(DestroyPieceEvent))
		{
			DestroyPieceEvent destroyPieceEvent = e as DestroyPieceEvent;
			levelDirty = true;
			if (!destroyPieceEvent.Piece.PickedUp)
			{
				lastLevelFullnessScore -= destroyPieceEvent.Piece.placementCost;
			}
		}
		if (!(type == typeof(NetworkMessageReceivedEvent)))
		{
			return;
		}
		NetworkMessageReceivedEvent networkMessageReceivedEvent = e as NetworkMessageReceivedEvent;
		if (networkMessageReceivedEvent.Message.msgType != NetMsgTypes.PiecePickedUp)
		{
			return;
		}
		MsgPiecePickedUp msgPiecePickedUp = networkMessageReceivedEvent.ReadMessage as MsgPiecePickedUp;
		Placeable placeable = null;
		if (msgPiecePickedUp.PieceID != 0)
		{
			foreach (Placeable allPlaceable in Placeable.AllPlaceables)
			{
				if (allPlaceable != null && allPlaceable.ID == msgPiecePickedUp.PieceID)
				{
					placeable = allPlaceable;
					break;
				}
			}
		}
		if (placeable != null)
		{
			lastLevelFullnessScore -= placeable.placementCost;
		}
	}

	private void ResetButton(bool eraseName, bool resetToggles)
	{
		publishButtonText.Term = "Snapshot/GetShareableCode";
		RecalculateLevelFullness();
		clipboardCopyEnabled = false;
		lastUploadedSnapshotName = null;
		codeDisplayContainer.gameObject.SetActive(value: false);
		codeDisplayText.text = "";
		if (eraseName)
		{
			nameEntryField.text = "";
		}
		if (resetToggles)
		{
			shareLocalToggleGroup.Deselect();
			shareLocalValue = -1;
			levelTypeToggleGroup.Deselect();
			levelTypeToggleGroup.gameObject.SetActive(value: false);
			levelTypeValue = -1;
			publicUnlistedToggleGroup.Deselect();
			publicUnlistedContainer.gameObject.SetActive(value: false);
			publicUnlistedValue = -1;
		}
		OnClickShareLocalToggle();
		playButtons.DisablePlayButtons();
		ShareLevelContainer.gameObject.SetActive(value: false);
		bool active = levelTypeValue != -1 && shareLocalValue == 1;
		bool active2 = levelTypeValue != -1 && shareLocalValue == 0 && publicUnlistedValue != -1;
		localSaveButtonContainer.gameObject.SetActive(active);
		publishButtonsContainer.gameObject.SetActive(active2);
		SetNameInputFieldInteractable(onOff: true);
		levelTypeDisableGroup.SetDisabled(disabled: false);
		shareLocalDisableGroup.SetDisabled(disabled: false);
		publicUnlistedDisableGroup.SetDisabled(disabled: false);
		bool flag = GameSettings.GetInstance().GameMode == GameState.GameMode.FREEPLAY;
		saveModifiersButton.gameObject.SetActive(flag);
		if (flag)
		{
			UpdateSaveModifiersButton();
		}
	}

	public void OnClickGetShareableCodeButton()
	{
		if (currentQuery != null)
		{
			return;
		}
		if (clipboardCopyEnabled)
		{
			QuickSaver.CopyStringToClipboard(codeDisplayText.text);
			UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.ShareableCodeClipboard, 2f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: true);
			return;
		}
		if (!LobbyManager.instance.CurrentGameController.CurrentLevelHasGoal())
		{
			UserMessageManager.Instance.UserMessage(LocalizationManager.GetTranslation("Snapshot/NoGoalBlock"), 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
			Debug.LogError("Can't save because there is no goal block!");
			return;
		}
		SetNameInputFieldInteractable(onOff: false);
		levelTypeDisableGroup.SetDisabled(disabled: true);
		shareLocalDisableGroup.SetDisabled(disabled: true);
		publicUnlistedDisableGroup.SetDisabled(disabled: true);
		UnityAction performUpload = delegate
		{
			UploadCurrentSnapshotBase(includeScreenshot: false, delegate(string snapshotName, string formattedCode, string imageUrl)
			{
				nameEntryField.text = snapshotName;
				QuickSaver.CopyStringToClipboard(formattedCode);
				UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.ShareableCodeClipboard, 2f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: true);
			});
		};
		if (WordFilter.PlatformHasWordFilter)
		{
			WordFilter.FilterText(this, nameEntryField.text, delegate(string filteredName)
			{
				nameEntryField.text = filteredName;
				performUpload();
			});
		}
		else
		{
			performUpload();
		}
	}

	private FeaturedQuickFilter.LevelTypes GetSelectedLevelType()
	{
		FeaturedQuickFilter.LevelTypes result = FeaturedQuickFilter.LevelTypes.Any;
		switch (levelTypeValue)
		{
		case 0:
			result = FeaturedQuickFilter.LevelTypes.Challenge;
			break;
		case 1:
			result = FeaturedQuickFilter.LevelTypes.Versus;
			break;
		}
		return result;
	}

	private void UploadCurrentSnapshotBase(bool includeScreenshot, UnityAction<string, string, string> shareFunc)
	{
		RecalculateLevelFullness();
		if (publicUnlistedValue == -1 || levelTypeValue == -1)
		{
			Debug.LogError("ERROR: level type or public/unlisted is not set.");
			return;
		}
		bool published = publicUnlistedValue == 0;
		FeaturedQuickFilter.LevelTypes levelType = FeaturedQuickFilter.LevelTypes.Any;
		if (published)
		{
			publishButtonText.Term = "Snapshot/Publishing";
		}
		else
		{
			publishButtonText.Term = "Snapshot/Uploading";
		}
		switch (levelTypeValue)
		{
		case 0:
			levelType = FeaturedQuickFilter.LevelTypes.Challenge;
			break;
		case 1:
			levelType = FeaturedQuickFilter.LevelTypes.Versus;
			break;
		}
		if (lastLevelFullnessScore > GameSettings.GetInstance().LevelFullnessScoreLimit)
		{
			return;
		}
		QuickSaver component = LobbyManager.instance.CurrentGameController.GetComponent<QuickSaver>();
		if (!(component != null))
		{
			return;
		}
		string snapshotName = nameEntryField.text.Trim();
		if (snapshotName.NullOrEmpty())
		{
			snapshotName = component.GetNewLocalSaveName();
			nameEntryField.text = snapshotName;
		}
		publishButton.SetDisabled(disabled: true);
		if (NetworkConnectivityStatus.Connected)
		{
			if (!PlatformFeatureRestrictions.IsNotConnected)
			{
				bool flag = false;
				if (GameSettings.GetInstance().GameMode == GameState.GameMode.FREEPLAY)
				{
					flag = !saveModifiers;
				}
				XmlDocument currentXmlSnapshot = component.GetCurrentXmlSnapshot(flag);
				byte[] bytes = component.GetCurrentSceneThumbnailBytes();
				currentQuery = GameSparksManager.Instance.CreateQuery();
				currentQuery.UploadStringAsFile(currentXmlSnapshot.OuterXml, snapshotName, published, levelType, !flag && Modifiers.GetInstance().AppliedAndNonDefault);
				GameSparksQuery gameSparksQuery = currentQuery;
				gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate(GameSparksQuery q)
				{
					if (!q.HasError)
					{
						string text = q.ResultData["code"] as string;
						if (!text.NullOrEmpty())
						{
							if (text.Length == 8)
							{
								string formattedCode = GameSparksQuery.GetFormattedSnapshotCode(text);
								Debug.Log("Got upload code: " + formattedCode);
								lastUploadedSnapshotCode = formattedCode;
								GameSparksQuery gameSparksQuery2 = GameSparksManager.Instance.CreateQuery();
								gameSparksQuery2.UploadLevelThumbnail(GameSparksQuery.SanitizeSnapshotCode(formattedCode), bytes);
								gameSparksQuery2.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery2.FinishListeners, (UnityAction<GameSparksQuery>)delegate
								{
									Debug.Log("Thumbnail successfully uploaded");
								});
								UnityAction OnReadyToDisplayCode = delegate
								{
									codeDisplayContainer.gameObject.SetActive(value: true);
									codeDisplayText.text = formattedCode;
									if (published)
									{
										publishButtonText.Term = "Snapshot/Published";
									}
									else
									{
										publishButtonText.Term = "Snapshot/Uploaded";
									}
									clipboardCopyEnabled = true;
									lastUploadedSnapshotName = snapshotName;
									AkSoundEngine.PostEvent("UI_Snapshot_GetCode", base.gameObject);
									currentQuery = null;
									if (!GameState.GetInstance().UsingHotSeat)
									{
										playButtons.EnablePlayButtons();
									}
									bool active = false;
									AllowedOnPlatform component2 = ShareLevelContainer.GetComponent<AllowedOnPlatform>();
									if (component2 != null)
									{
										active = component2.GetAllowed;
									}
									ShareLevelContainer.gameObject.SetActive(active);
									if (AnalyticsWrapper.EnabledOnPlatform)
									{
										GameControl gameControl = UnityEngine.Object.FindObjectOfType<GameControl>();
										if (gameControl != null)
										{
											Level levelLayout = LobbyManager.instance.CurrentGameController.LevelLayout;
											int num = 0;
											Coin[] array = UnityEngine.Object.FindObjectsOfType<Coin>();
											foreach (Coin coin in array)
											{
												if (coin.Placed && !coin.MarkedForDestruction)
												{
													num++;
												}
											}
											BackgroundType background = BackgroundType.None;
											if (levelLayout.currentCustomBackground != null)
											{
												background = levelLayout.currentCustomBackground.background;
											}
											AnalyticEvent.LevelSavedEvent(gameControl.MatchGuid, GetSelectedLevelType(), savedOnline: true, publicUnlistedValue == 0, (float)lastLevelFullnessScore / (float)GameSettings.GetInstance().LevelFullnessScoreLimit, background, levelLayout.currentCustomMusic, levelLayout.currentCustomAmbience, num);
											AnalyticEvent.LevelModifiersEvent(gameControl.MatchGuid, Modifiers.GetInstance());
										}
									}
								};
								if (includeScreenshot)
								{
									UploadBigScreenshotToImgur(snapshotName, formattedCode, linkImageDirectly: true, delegate(string imageURL)
									{
										shareFunc(snapshotName, formattedCode, imageURL);
										OnReadyToDisplayCode();
									});
								}
								else
								{
									OnReadyToDisplayCode();
								}
								return;
							}
						}
						else
						{
							Debug.LogError("Received upload code was null or empty...");
							OnUploadFailed(UploadError.Generic);
						}
					}
					else
					{
						Debug.LogError("Error while uploading snapshot: " + q.Error);
						OnUploadFailed(UploadError.Generic);
					}
					currentQuery = null;
				});
			}
			else
			{
				Debug.LogError("Skipped uploading snapshot: Social account not signed in");
				OnUploadFailed(UploadError.NotSignedIn);
			}
		}
		else
		{
			Debug.LogError("Skipped uploading snapshot: No network connection");
			OnUploadFailed(UploadError.NoConnection);
		}
	}

	public void ReloadLevelInNewMode()
	{
		if (LobbyManager.instance.CurrentGameController.GetComponent<QuickSaver>() != null)
		{
			StartCoroutine(FadeAndReloadScene());
		}
	}

	private IEnumerator FadeAndReloadScene()
	{
		GameState instance = GameState.GetInstance();
		QuickSaver component = LobbyManager.instance.CurrentGameController.GetComponent<QuickSaver>();
		PickableButton.maskAll = true;
		bool omitModifiers = false;
		if (GameSettings.GetInstance().GameMode == GameState.GameMode.FREEPLAY)
		{
			omitModifiers = !saveModifiers;
		}
		QuickSaver.levelPortalXml = component.GetCurrentXmlSnapshot(omitModifiers).OuterXml;
		LevelSelectController.PlayedSnapshotInfo currentSnapshotInfo = instance.currentSnapshotInfo;
		currentSnapshotInfo.snapshotName = lastUploadedSnapshotName;
		switch (levelTypeToggleGroup.selectedIndex)
		{
		case -1:
			currentSnapshotInfo.snapshotType = FeaturedQuickFilter.LevelTypes.Any;
			break;
		case 0:
			currentSnapshotInfo.snapshotType = FeaturedQuickFilter.LevelTypes.Challenge;
			break;
		default:
			currentSnapshotInfo.snapshotType = FeaturedQuickFilter.LevelTypes.Versus;
			break;
		}
		if (shareLocalToggleGroup.selectedIndex != -1)
		{
			currentSnapshotInfo.snapshotCode = lastUploadedSnapshotCode;
			LobbyPlayer firstLocalLobbyPlayer = LevelSelectController.GetFirstLocalLobbyPlayer();
			if (firstLocalLobbyPlayer != null)
			{
				currentSnapshotInfo.authorID = firstLocalLobbyPlayer.GSID;
				currentSnapshotInfo.authorDisplayName = firstLocalLobbyPlayer.playerName;
				currentSnapshotInfo.authorPlatform = firstLocalLobbyPlayer.platform;
				currentSnapshotInfo.authorPlatformID = firstLocalLobbyPlayer.platformUniqueID;
			}
			GameSparksManager.Instance.CreateQuery().NotifySnapshotPlayed(lastUploadedSnapshotCode);
			Debug.Log("Playing snapshot code " + GameSparksQuery.GetFormattedSnapshotCode(lastUploadedSnapshotCode));
		}
		else
		{
			currentSnapshotInfo.snapshotCode = "";
			currentSnapshotInfo.authorDisplayName = null;
			currentSnapshotInfo.authorID = null;
			currentSnapshotInfo.authorPlatform = LobbyPlayer.SocialPlatform.Undefined;
			currentSnapshotInfo.authorPlatformID = null;
		}
		instance.currentSnapshotInfo = currentSnapshotInfo;
		MsgPrepareToReloadScene msgPrepareToReloadScene = new MsgPrepareToReloadScene();
		msgPrepareToReloadScene.reloadToMode = GameSettings.GetInstance().GameMode;
		msgPrepareToReloadScene.snapshotInfo = instance.currentSnapshotInfo;
		NetworkServer.SendToAll(NetMsgTypes.PrepareToReloadScene, msgPrepareToReloadScene);
		LoadingInterstitialSplash.Instance.showLevelInfoNextLoad = true;
		LoadingInterstitialSplash.Instance.FadeIn();
		while (LoadingInterstitialSplash.Instance.State != UISplashScreen.STATE.SHOW)
		{
			yield return null;
		}
		PickableButton.ResetMasks();
		Placeable.SetInitialSequenceID(0);
		LobbyManager.instance.ReloadScene(GameSettings.GetInstance().GameMode);
	}

	public void OnSnapshotNameChanged(string value)
	{
		ResetButton(eraseName: false, resetToggles: false);
	}

	private void RecalculateLevelFullness()
	{
		if (LobbyManager.instance != null && LobbyManager.instance.CurrentGameController != null)
		{
			int num = (lastLevelFullnessScore = LobbyManager.instance.CurrentGameController.GetComponent<QuickSaver>().CalculateLevelFullness());
			levelFullnessText.text = num + "/" + GameSettings.GetInstance().LevelFullnessScoreLimit;
			bool active = num > GameSettings.GetInstance().LevelFullnessScoreLimit;
			levelFullnessWarningText.gameObject.SetActive(active);
			publishButton.SetDisabled(active);
		}
	}

	public void OnClickTwitterShareButton()
	{
		if (clipboardCopyEnabled && !Uploadscreenshot)
		{
			Debug.LogError("This isn't possible anymore!");
			UndergroundComputer.ShareSnapshotCodeOnTwitter(lastUploadedSnapshotName, codeDisplayText.text, null);
		}
		else
		{
			UploadCurrentSnapshotBase(includeScreenshot: true, UndergroundComputer.ShareSnapshotCodeOnTwitter);
		}
	}

	public void OnClickRedditShareButton()
	{
		if (clipboardCopyEnabled && !Uploadscreenshot)
		{
			Debug.LogError("This isn't possible anymore!");
			UndergroundComputer.ShareSnapshotCodeOnReddit(lastUploadedSnapshotName, codeDisplayText.text, null);
		}
		else
		{
			UploadCurrentSnapshotBase(includeScreenshot: true, UndergroundComputer.ShareSnapshotCodeOnReddit);
		}
	}

	public void OnClickImgurShareButton()
	{
		if (clipboardCopyEnabled)
		{
			UploadBigScreenshotToImgur(lastUploadedSnapshotName, lastUploadedSnapshotCode, linkImageDirectly: false, UndergroundComputer.ShareSnapshotCodeOnImgur);
		}
		else
		{
			Debug.LogError("This isn't possible anymore!");
		}
	}

	public static void UploadBigScreenshotToImgur(string snapshotName, string formattedCode, bool linkImageDirectly, UnityAction<string> onImgurURL, bool ChallengeScoreboardScreenShot = false)
	{
		byte[] array = null;
		try
		{
			QuickSaver component = LobbyManager.instance.CurrentGameController.GetComponent<QuickSaver>();
			Texture2D texture2D = ((!ChallengeScoreboardScreenShot) ? component.GetCurrentSceneScreenshotHighRes(50f, showGraphpaper: false, withPlayer: false, 2048, 2048) : component.GetMergeLevelAndScoreboard());
			array = texture2D.EncodeToJPG(95);
			UnityEngine.Object.Destroy(texture2D);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to generate screenshot: " + ex.Message + "\n" + ex.StackTrace);
		}
		if (array != null)
		{
			GameState.GetInstance().StartCoroutine(DoImgurUpload(array, snapshotName, formattedCode, linkImageDirectly, onImgurURL));
		}
		else
		{
			onImgurURL(null);
		}
	}

	private static IEnumerator DoImgurUpload(byte[] bytes, string snapshotName, string formattedCode, bool linkImageDirectly, UnityAction<string> onImgurURL)
	{
		WWWForm wWWForm = new WWWForm();
		wWWForm.AddBinaryData("image", bytes);
		if (snapshotName != null)
		{
			wWWForm.AddField("title", snapshotName);
		}
		if (formattedCode != null)
		{
			wWWForm.AddField("description", formattedCode);
		}
		Dictionary<string, string> headers = wWWForm.headers;
		headers.Add("Authorization", "Client-ID 20f9d5ac906ea1c");
		WWW w = new WWW("https://api.imgur.com/3/image", wWWForm.data, headers);
		while (!w.isDone)
		{
			yield return null;
		}
		Dictionary<string, object> dictionary = GSJson.From(w.text) as Dictionary<string, object>;
		if (dictionary.TryGetValue("success", out var value))
		{
			if ((bool)value)
			{
				if (dictionary.TryGetValue("data", out var value2))
				{
					if ((value2 as Dictionary<string, object>).TryGetValue("id", out var value3))
					{
						string text = value3 as string;
						if (linkImageDirectly)
						{
							onImgurURL("https://i.imgur.com/" + text + ".jpg");
						}
						else
						{
							onImgurURL("https://www.imgur.com/" + text);
						}
						yield break;
					}
					Debug.LogError("Error grabbing ID");
				}
				else
				{
					Debug.LogError("Error grabbing Data");
				}
			}
			else
			{
				Debug.LogError("Uploading the screenshot to Imgur failed.");
			}
		}
		onImgurURL(null);
	}

	public void OnClickLevelTypeToggle()
	{
		int selectedIndex = levelTypeToggleGroup.selectedIndex;
		switch (selectedIndex)
		{
		case -1:
			ShowPublicUnlisted(shareLocalValue == 0, reset: false);
			break;
		case 0:
		case 1:
			if (shareLocalValue == 0)
			{
				ShowPublicUnlisted(onOff: true, reset: false);
			}
			break;
		}
		levelTypeValue = selectedIndex;
		UpdatePublishButtons();
	}

	public void OnClickShareLocalToggle()
	{
		int selectedIndex = shareLocalToggleGroup.selectedIndex;
		switch (selectedIndex)
		{
		case -1:
			ShowNameInput(onOff: false, reset: false);
			ShowLevelType(onOff: false, reset: false);
			ShowPublicUnlisted(onOff: false, reset: false);
			break;
		case 0:
			ShowNameInput(onOff: true, reset: false);
			ShowLevelType(onOff: true, reset: false);
			ShowPublicUnlisted(onOff: true, reset: false);
			break;
		case 1:
			ShowNameInput(onOff: true, reset: false);
			ShowLevelType(onOff: true, reset: false);
			ShowPublicUnlisted(onOff: false, reset: false);
			break;
		}
		shareLocalValue = selectedIndex;
		UpdatePublishButtons();
	}

	public void OnClickPublicUnlistedToggle()
	{
		int selectedIndex = publicUnlistedToggleGroup.selectedIndex;
		switch (selectedIndex)
		{
		case 0:
			ShareLevelContainer.gameObject.SetActive(value: false);
			break;
		case 1:
			ShareLevelContainer.gameObject.SetActive(value: false);
			break;
		}
		publicUnlistedValue = selectedIndex;
		UpdatePublishButtons();
	}

	private void UpdatePublishButtons()
	{
		if (hide_online_ugc_features && shareLocalValue == 0)
		{
			formSectionContainer.gameObject.SetActive(value: false);
			restrictUGCOnlineMessage.gameObject.SetActive(value: true);
			restrictUGCOfflineMessage.gameObject.SetActive(value: false);
		}
		else if (hide_offline_ugc_features && shareLocalValue == 1)
		{
			formSectionContainer.gameObject.SetActive(value: false);
			restrictUGCOnlineMessage.gameObject.SetActive(value: false);
			restrictUGCOfflineMessage.gameObject.SetActive(value: true);
			localScoreSaveWarning.gameObject.SetActive(value: false);
		}
		else
		{
			formSectionContainer.gameObject.SetActive(value: true);
			restrictUGCOnlineMessage.gameObject.SetActive(value: false);
			restrictUGCOfflineMessage.gameObject.SetActive(value: false);
			localScoreSaveWarning.gameObject.SetActive(shareLocalValue == 1);
		}
		if (shareLocalValue == -1 || levelTypeValue == -1 || (shareLocalValue == 0 && publicUnlistedValue == -1))
		{
			localSaveButtonContainer.gameObject.SetActive(value: false);
			publishButtonsContainer.gameObject.SetActive(value: false);
			codeDisplayContainer.gameObject.SetActive(value: false);
			return;
		}
		switch (shareLocalValue)
		{
		case 0:
			localSaveButtonContainer.gameObject.SetActive(value: false);
			publishButtonsContainer.gameObject.SetActive(value: true);
			codeDisplayContainer.gameObject.SetActive(clipboardCopyEnabled);
			publishButtonText.Term = ((publicUnlistedValue == 0) ? "Snapshot/Publish" : "Snapshot/Upload");
			break;
		case 1:
			localSaveButtonContainer.gameObject.SetActive(value: true);
			publishButtonsContainer.gameObject.SetActive(value: false);
			codeDisplayContainer.gameObject.SetActive(value: false);
			break;
		}
	}

	private void ShowNameInput(bool onOff, bool reset)
	{
		if (reset)
		{
			nameEntryField.text = "";
		}
		NameInputContainer.gameObject.SetActive(onOff);
	}

	private void ShowLevelType(bool onOff, bool reset)
	{
		ShowFormPart(levelTypeToggleGroup.transform, levelTypeToggleGroup, onOff, reset);
	}

	private void ShowShareLocal(bool onOff, bool reset)
	{
		ShowFormPart(shareLocalContainer, shareLocalToggleGroup, onOff, reset);
	}

	private void ShowPublicUnlisted(bool onOff, bool reset)
	{
		ShowFormPart(publicUnlistedContainer, publicUnlistedToggleGroup, onOff, reset);
	}

	private void ShowFormPart(Transform container, TabletToggleButtonGroup toggleGroup, bool onOff, bool reset)
	{
		if (reset)
		{
			toggleGroup.Deselect();
		}
		container.gameObject.SetActive(onOff);
		playButtons.DisablePlayButtons();
		ShareLevelContainer.gameObject.SetActive(value: false);
	}

	private void OnUploadFailed(UploadError error)
	{
		AkSoundEngine.PostEvent("UI_Snapshot_Error", base.gameObject);
		ShowUploadError(error);
		SetNameInputFieldInteractable(onOff: true);
		levelTypeDisableGroup.SetDisabled(disabled: false);
		shareLocalDisableGroup.SetDisabled(disabled: false);
		publicUnlistedDisableGroup.SetDisabled(disabled: false);
		publishButton.SetDisabled(disabled: false);
		UpdatePublishButtons();
	}

	public static void ShowUploadError(UploadError error)
	{
		switch (error)
		{
		case UploadError.Generic:
			UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.ErrorUploadingSnapshot, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
			break;
		case UploadError.NoConnection:
			UserMessageManager.Instance.UserMessage(LocalizationManager.GetTranslation("Snapshot/ErrorUploadingSnapshotNoConnection"), 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
			break;
		case UploadError.NotSignedIn:
			UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.ErrorUploadingSnapshot, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
			break;
		}
	}

	public void OnClickSaveLocally(PickCursor pickCursor)
	{
		if (levelTypeValue == -1)
		{
			Debug.LogError("Level type toggle not set");
			PickableButton.ResetMasks();
			return;
		}
		if (!LobbyManager.instance.CurrentGameController.CurrentLevelHasGoal())
		{
			UserMessageManager.Instance.UserMessage(LocalizationManager.GetTranslation("Snapshot/NoGoalBlock"), 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
			Debug.LogError("Can't save because there is no goal block!");
			PickableButton.ResetMasks();
			return;
		}
		QuickSaver quickSaver = LobbyManager.instance.CurrentGameController.GetComponent<QuickSaver>();
		if (quickSaver != null)
		{
			UnityAction<string> unityAction = delegate(string filteredName)
			{
				QuickSaver.RecountLocalSaves(delegate
				{
					int maxLocalSnapshots = GameSettings.GetInstance().maxLocalSnapshots;
					if (QuickSaver.numLocalSaves >= maxLocalSnapshots)
					{
						UserMessageManager.Instance.UserMessage(string.Format(LocalizationManager.GetTranslation("Snapshot/SaveShare/SnapshotLimitReached"), maxLocalSnapshots), 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
						Debug.LogError("Too many snapshots! (max: " + maxLocalSnapshots + " current: " + QuickSaver.numLocalSaves + ")");
						PickableButton.ResetMasks();
					}
					else
					{
						filteredName = filteredName.Replace('*', '-');
						nameEntryField.text = filteredName;
						bool omitModifiers = false;
						if (GameSettings.GetInstance().GameMode == GameState.GameMode.FREEPLAY)
						{
							omitModifiers = !saveModifiers;
						}
						quickSaver.DoLocalSave(filteredName, GetSelectedLevelType(), pickCursor.localNumber, omitModifiers, delegate(bool success, string filename)
						{
							if (success)
							{
								string snapshotNameWithoutSuffix = QuickSaver.GetSnapshotNameWithoutSuffix(Path.GetFileNameWithoutExtension(filename));
								nameEntryField.text = snapshotNameWithoutSuffix;
								if (RamFS.PlatformUsesRamFS)
								{
									UserMessageManager.Instance.UserMessage(LocalizationManager.GetTranslation("Snapshot/SavingSnapshot") + " " + snapshotNameWithoutSuffix, 2f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: true);
									RamFS.PostUserMessageOnFlushToDisk(ScriptLocalization.Snapshot.SavedSnapshot + " " + snapshotNameWithoutSuffix);
									RamFS.AddRunFuncOperation(delegate
									{
										PickableButton.ResetMasks();
										QuickSaver.numLocalSaves++;
									});
								}
								else
								{
									UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.SavedSnapshot + " " + snapshotNameWithoutSuffix, 2f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: true);
									PickableButton.ResetMasks();
									QuickSaver.numLocalSaves++;
								}
								quickSaver.SaveLocalThumbnail(snapshotNameWithoutSuffix);
								lastUploadedSnapshotName = snapshotNameWithoutSuffix;
								if (!GameState.GetInstance().UsingHotSeat)
								{
									playButtons.EnablePlayButtons();
								}
								if (AnalyticsWrapper.EnabledOnPlatform)
								{
									GameControl gameControl = UnityEngine.Object.FindObjectOfType<GameControl>();
									if (gameControl != null)
									{
										Level levelLayout = LobbyManager.instance.CurrentGameController.LevelLayout;
										int num = 0;
										Coin[] array = UnityEngine.Object.FindObjectsOfType<Coin>();
										foreach (Coin coin in array)
										{
											if (coin != null && coin.Placed && !coin.MarkedForDestruction)
											{
												num++;
											}
										}
										AnalyticEvent.LevelSavedEvent(gameControl.MatchGuid, GetSelectedLevelType(), savedOnline: false, isPublic: false, (float)lastLevelFullnessScore / (float)GameSettings.GetInstance().LevelFullnessScoreLimit, (levelLayout.currentCustomBackground != null) ? levelLayout.currentCustomBackground.background : BackgroundType.None, levelLayout.currentCustomMusic, levelLayout.currentCustomAmbience, num);
										AnalyticEvent.LevelModifiersEvent(gameControl.MatchGuid, Modifiers.GetInstance());
									}
								}
							}
							else
							{
								UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.ErrorSavingFile, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
								Debug.LogError("Failed to make local save.");
								PickableButton.ResetMasks();
							}
						});
					}
				});
			};
			PickableButton.maskAll = true;
			string text = QuickSaver.SanitizePath(nameEntryField.text);
			if (WordFilter.PlatformHasWordFilter)
			{
				WordFilter.FilterText(this, text, unityAction);
			}
			else
			{
				unityAction(text);
			}
		}
		else
		{
			UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.ErrorSavingFile, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
			Debug.LogError("Error: where is QuickSaver?");
			PickableButton.ResetMasks();
		}
	}

	private void SetNameInputFieldInteractable(bool onOff)
	{
		nameEntryFieldDisableGroup.SetDisabled(!onOff);
		nameEntryField.interactable = onOff;
	}

	public void OnClickReset()
	{
		ResetButton(eraseName: true, resetToggles: true);
	}

	public override void Update()
	{
		base.Update();
		bool flag = PlatformFeatureRestrictions.MustHideAllUGC;
		bool flag2 = PlatformFeatureRestrictions.HideOnlineContent;
		if (DebugSimulateNotSignedIn)
		{
			flag = true;
		}
		if (DebugSimulateUGCBlocked)
		{
			flag2 = true;
		}
		if (hide_online_ugc_features != flag2)
		{
			hide_online_ugc_features = flag2;
			UpdatePublishButtons();
		}
		if (hide_offline_ugc_features != flag)
		{
			hide_offline_ugc_features = flag;
			UpdatePublishButtons();
		}
		if (QuickSaver.numLocalSaves == -1)
		{
			localSaveStorageLine.gameObject.SetActive(value: true);
			localSaveStorageCalculating.gameObject.SetActive(value: true);
			localSaveStorageValue.gameObject.SetActive(value: false);
			if (!QuickSaver.numLocalSavesQueried)
			{
				QuickSaver.RecountLocalSaves(delegate
				{
				});
			}
			return;
		}
		int num = Mathf.CeilToInt((float)GameSettings.GetInstance().maxLocalSnapshots * 0.9f);
		if (QuickSaver.numLocalSaves > num)
		{
			localSaveStorageLine.gameObject.SetActive(value: true);
			localSaveStorageCalculating.gameObject.SetActive(value: false);
			localSaveStorageValue.gameObject.SetActive(value: true);
			localSaveStorageValue.text = QuickSaver.numLocalSaves + "/" + GameSettings.GetInstance().maxLocalSnapshots;
		}
		else
		{
			localSaveStorageLine.gameObject.SetActive(value: false);
		}
	}

	public void OnClickNameEntryField(PickCursor pickCursor)
	{
		Color originalColor = nameEntryFieldBackground.color;
		Color buttonBgColor_TransparentHighlight = colorScheme.buttonBgColor_TransparentHighlight;
		nameEntryFieldAnimator.FadeColor(nameEntryFieldBackground.color, buttonBgColor_TransparentHighlight, 0.25f, Easings.Functions.CubicEaseOut);
		Tablet.ActivateInputField(pickCursor, nameEntryField, LocalizationManager.GetTranslation("Snapshot/Name"), delegate(string str)
		{
			nameEntryField.text = str;
			nameEntryFieldAnimator.FadeColor(nameEntryFieldBackground.color, originalColor, 0.25f, Easings.Functions.CubicEaseOut);
		});
	}

	public void OnClickSaveModifiersButton(PickCursor pickCursor)
	{
		saveModifiers = !saveModifiers;
		UpdateSaveModifiersButton();
	}

	private void UpdateSaveModifiersButton()
	{
		saveModifiersXImage.gameObject.SetActive(!saveModifiers);
		Text componentInChildren = saveModifiersButton.toolTip.GetComponentInChildren<Text>();
		if (componentInChildren != null)
		{
			componentInChildren.text = (saveModifiers ? ScriptLocalization.Snapshot.SaveModifiersTooltipOn : ScriptLocalization.Snapshot.SaveModifiersTooltipOff);
		}
	}
}
