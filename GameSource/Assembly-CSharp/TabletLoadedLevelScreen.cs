using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using I2.Loc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TabletLoadedLevelScreen : TabletScreen
{
	public TabletTextLabel levelNameText;

	public TabletTextLabel snapshotCodeText;

	public TabletSubdialogController publishAreaController;

	public RectTransform publishAreaEmptyRect;

	public RectTransform publishAreaSpinnerRect;

	public RectTransform publishAreaPublishButtonRect;

	public RectTransform publishAreaCodeRect;

	public TabletButton toggleFavoriteButton;

	public TabletTextLabel toggleFavoriteButtonText;

	public Image favStar;

	public TabletButton saveLocalCopyButton;

	public TabletButton uploadToLevelNETButton;

	public UGCNameTag authorNametag;

	public string localSaveName;

	private bool currentlyInSnapshot;

	public bool usingCode;

	public PlaceableMetadataList metadataList;

	public TabletPlayButtonController playButtons;

	public static TabletLoadedLevelScreen Instance;

	public GameObject connectButton;

	public SwitchConnectButton switchConnectButton;

	private bool initialized;

	public bool CurrentlyInSnapshot => currentlyInSnapshot;

	private void Awake()
	{
		Instance = this;
		if (!(LobbyManager.instance == null))
		{
			GameState instance = GameState.GetInstance();
			currentlyInSnapshot = !instance.currentSnapshotInfo.snapshotName.NullOrEmpty();
			usingCode = !instance.currentSnapshotInfo.snapshotCode.NullOrEmpty();
		}
	}

	private void Start()
	{
		if (!initialized)
		{
			Initialize();
		}
	}

	private void Initialize()
	{
		initialized = true;
		if (LobbyManager.instance == null)
		{
			return;
		}
		GameState instance = GameState.GetInstance();
		if (!instance.currentSnapshotInfo.authorID.NullOrEmpty())
		{
			LevelSelectController.PlayedSnapshotInfo currentSnapshotInfo = instance.currentSnapshotInfo;
			authorNametag.InitializeAsync(currentSnapshotInfo.authorDisplayName, currentSnapshotInfo.authorPlatformID, currentSnapshotInfo.authorID, currentSnapshotInfo.authorPlatform);
		}
		else
		{
			authorNametag.gameObject.SetActive(value: false);
		}
		if (currentlyInSnapshot)
		{
			levelNameText.text = instance.currentSnapshotInfo.snapshotName;
			bool isHost = LobbyManager.instance.IsHost;
			if (isHost && usingCode)
			{
				playButtons.EnablePlayButtons();
			}
			else
			{
				playButtons.DisablePlayButtons();
			}
			if (!usingCode && !isHost && localSaveName.NullOrEmpty())
			{
				toggleFavoriteButton.SetDisabled(disabled: true);
			}
			if (!usingCode && isHost)
			{
				saveLocalCopyButton.SetDisabled(disabled: true);
				localSaveName = instance.currentSnapshotInfo.snapshotName;
			}
			if (usingCode && isHost && StatTracker.Instance.GetSaveFileDataForMainUser().IsLocalSnapshotWithCode(instance.currentSnapshotInfo.snapshotName, instance.currentSnapshotInfo.snapshotCode))
			{
				saveLocalCopyButton.SetDisabled(disabled: true);
				localSaveName = instance.currentSnapshotInfo.snapshotName;
			}
			UpdateFavoriteButtonText();
			if (!usingCode)
			{
				publishAreaController.ForceSubdialog(publishAreaPublishButtonRect);
				return;
			}
			publishAreaController.ForceSubdialog(publishAreaCodeRect);
			snapshotCodeText.text = GameSparksQuery.GetFormattedSnapshotCode(instance.currentSnapshotInfo.snapshotCode);
		}
	}

	public override void Update()
	{
		base.Update();
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
		PickableButton.maskAll = true;
		GameState instance = GameState.GetInstance();
		QuickSaver.levelPortalXml = QuickSaver.lastLoadedXml;
		if (usingCode)
		{
			GameSparksManager.Instance.CreateQuery().NotifySnapshotPlayed(instance.currentSnapshotInfo.snapshotCode);
		}
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

	private void UpdateFavoriteButtonText()
	{
		GameState instance = GameState.GetInstance();
		SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
		if ((usingCode && saveFileDataForMainUser.IsFavorite(instance.currentSnapshotInfo.snapshotName, instance.currentSnapshotInfo.snapshotCode)) || (!usingCode && !localSaveName.NullOrEmpty() && saveFileDataForMainUser.IsFavorite(localSaveName, null)))
		{
			toggleFavoriteButtonText.Term = "Snapshot/RemoveFavorite";
			favStar.sprite = GameSettings.GetInstance().FavStarFilled;
		}
		else
		{
			toggleFavoriteButtonText.Term = "Snapshot/AddToFavorites";
			favStar.sprite = GameSettings.GetInstance().FavStarEmpty;
		}
	}

	public void OnClickToggleFavorite(PickCursor pickCursor)
	{
		GameState instance = GameState.GetInstance();
		if (usingCode || !localSaveName.NullOrEmpty())
		{
			SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
			if (usingCode)
			{
				if (!saveFileDataForMainUser.IsFavorite(instance.currentSnapshotInfo.snapshotName, instance.currentSnapshotInfo.snapshotCode))
				{
					saveFileDataForMainUser.AddFavoriteSnapshotCode(instance.currentSnapshotInfo.snapshotName, instance.currentSnapshotInfo.snapshotCode);
				}
				else
				{
					saveFileDataForMainUser.RemoveFavoriteSnapshotCode(instance.currentSnapshotInfo.snapshotName, instance.currentSnapshotInfo.snapshotCode);
				}
			}
			else if (!saveFileDataForMainUser.IsFavorite(localSaveName, null))
			{
				saveFileDataForMainUser.AddFavoriteSnapshotCode(localSaveName, null);
			}
			else
			{
				saveFileDataForMainUser.RemoveFavoriteSnapshotCode(localSaveName, null);
			}
		}
		UpdateFavoriteButtonText();
	}

	public void OnClickCopyToClipboard(PickCursor pickCursor)
	{
		if (usingCode)
		{
			QuickSaver.CopyStringToClipboard(snapshotCodeText.text);
			UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.ShareableCodeClipboard, 2f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: true);
		}
	}

	public void OnClickSaveLocally(PickCursor pickCursor)
	{
		GameState instance = GameState.GetInstance();
		if (!localSaveName.NullOrEmpty() || QuickSaver.lastLoadedXml.NullOrEmpty())
		{
			return;
		}
		PickableButton.maskAll = true;
		XmlDocument xmlDoc = new XmlDocument();
		try
		{
			xmlDoc.LoadXml(QuickSaver.lastLoadedXml);
		}
		catch (Exception ex)
		{
			Debug.LogError("Could not load in XML: " + ex.Message);
			UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.ErrorSavingFile, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
			AkSoundEngine.PostEvent("UI_Snapshot_Error", base.gameObject);
			PickableButton.ResetMasks();
			return;
		}
		QuickSaver.CheckSaveFolders();
		string text = QuickSaver.SanitizePath(instance.currentSnapshotInfo.snapshotName);
		string extraSuffix = QuickSaver.GetLocalSaveSuffixForLevelType(instance.currentSnapshotInfo.snapshotType);
		string tentativeFilename = QuickSaver.LocalSavesFolder + "/" + text;
		Action OnFilenameFiltered = delegate
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
					Action<IEnumerable<string>> action = delegate(IEnumerable<string> existingFilenames)
					{
						string actualFilename = QuickSaver.EnsureUniqueLocalLevelName(tentativeFilename, existingFilenames);
						if (actualFilename != null)
						{
							actualFilename = actualFilename + extraSuffix + ".snapshot";
							byte[] compressedBytesFromXmlDoc = QuickSaver.GetCompressedBytesFromXmlDoc(xmlDoc);
							UnityAction onSaveComplete = delegate
							{
								string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(actualFilename);
								localSaveName = fileNameWithoutExtension;
								saveLocalCopyButton.SetDisabled(disabled: true);
								toggleFavoriteButton.SetDisabled(disabled: false);
								string fileName = Path.GetFileName(actualFilename);
								if (RamFS.PlatformUsesRamFS)
								{
									UserMessageManager.Instance.UserMessage(LocalizationManager.GetTranslation("Snapshot/SavingFileAs") + " " + fileName, 2f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: true);
									RamFS.PostUserMessageOnFlushToDisk(ScriptLocalization.Snapshot.SavedFileAs + " " + fileName);
									RamFS.AddRunFuncOperation(delegate
									{
										PickableButton.ResetMasks();
										QuickSaver.numLocalSaves++;
									});
								}
								else
								{
									UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.SavedFileAs + " " + fileName, 2f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: true);
									PickableButton.ResetMasks();
									QuickSaver.numLocalSaves++;
								}
							};
							if (RamFS.PlatformUsesRamFS)
							{
								RamFS.AddAddFileOperation(actualFilename, compressedBytesFromXmlDoc, delegate(RamFS.FSOperationReturnCode returnCode)
								{
									if (returnCode == RamFS.FSOperationReturnCode.OK)
									{
										onSaveComplete();
									}
									else
									{
										Debug.LogError("Could not add file to RamFS (" + returnCode.ToString() + ")");
										UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.ErrorSavingFile, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
										AkSoundEngine.PostEvent("UI_Snapshot_Error", base.gameObject);
										PickableButton.ResetMasks();
									}
								});
							}
							else
							{
								FileStream fileStream = null;
								try
								{
									fileStream = File.OpenWrite(actualFilename);
									fileStream.Write(compressedBytesFromXmlDoc, 0, compressedBytesFromXmlDoc.Length);
									fileStream.Close();
									onSaveComplete();
								}
								catch (Exception ex2)
								{
									Debug.LogError("Could not save the file: " + ex2.Message);
									UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.ErrorSavingFile, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
									AkSoundEngine.PostEvent("UI_Snapshot_Error", base.gameObject);
									fileStream?.Close();
									return;
								}
								PickableButton.ResetMasks();
							}
						}
						else
						{
							Debug.LogError("Could not save the file!");
							UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.ErrorSavingFile, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
							AkSoundEngine.PostEvent("UI_Snapshot_Error", base.gameObject);
							PickableButton.ResetMasks();
						}
					};
					if (RamFS.PlatformUsesRamFS)
					{
						RamFS.AddGetExistingFilenamesOperation("/snapshots", null, ordered: false, action);
					}
					else
					{
						action(null);
					}
				}
			});
		};
		if (WordFilter.PlatformHasWordFilter)
		{
			WordFilter.FilterText(this, tentativeFilename, delegate(string filteredText)
			{
				tentativeFilename = filteredText.Replace('*', '-');
				OnFilenameFiltered();
			});
		}
		else
		{
			OnFilenameFiltered();
		}
	}

	public void OnClickShareButtonBase(UnityAction<string, string, string> shareFunc, bool ChallengeScoreboardScreenshot = false)
	{
		if (!initialized)
		{
			Initialize();
		}
		if (!NetworkConnectivityStatus.Connected)
		{
			Debug.LogError("Could not get code for local save: No network connection");
			TabletSaveAndShareScreen.ShowUploadError(TabletSaveAndShareScreen.UploadError.NoConnection);
		}
		else if (PlatformFeatureRestrictions.IsNotConnected)
		{
			Debug.LogError("Could not get code for local save: Not signed in");
			TabletSaveAndShareScreen.ShowUploadError(TabletSaveAndShareScreen.UploadError.NotSignedIn);
		}
		else if (usingCode)
		{
			string snapshotName = levelNameText.text;
			string formattedCode = GameSparksQuery.GetFormattedSnapshotCode(snapshotCodeText.text);
			TabletSaveAndShareScreen.UploadBigScreenshotToImgur(snapshotName, formattedCode, linkImageDirectly: false, delegate(string url)
			{
				shareFunc(snapshotName, formattedCode, url);
			}, ChallengeScoreboardScreenshot);
		}
		else
		{
			if (QuickSaver.lastLoadedXml.NullOrEmpty())
			{
				return;
			}
			XmlDocument xmlDoc = new XmlDocument();
			Action onFinish = delegate
			{
				int num = QuickSaver.CalculateLevelFullnessFromXML(xmlDoc, metadataList);
				int levelFullnessScoreLimit = GameSettings.GetInstance().LevelFullnessScoreLimit;
				if (num <= levelFullnessScoreLimit)
				{
					UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.UploadingSnapshot, 2f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: true);
					string snapshotName2 = levelNameText.text;
					bool hasMods = QuickSaver.CheckNonDefaultModsFromXML(xmlDoc);
					GameSparksQuery query = GameSparksManager.Instance.CreateQuery();
					query.UploadStringAsFile(QuickSaver.lastLoadedXml, snapshotName2, published: false, FeaturedQuickFilter.LevelTypes.Versus, hasMods);
					GameSparksQuery gameSparksQuery = query;
					gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate
					{
						if (!query.HasError)
						{
							string input = query.ResultData["code"] as string;
							string formattedCode2 = GameSparksQuery.GetFormattedSnapshotCode(input);
							TabletSaveAndShareScreen.UploadBigScreenshotToImgur(snapshotName2, formattedCode2, linkImageDirectly: false, delegate(string url)
							{
								shareFunc(snapshotName2, formattedCode2, url);
							});
							EnableCodeSection(formattedCode2);
							AkSoundEngine.PostEvent("UI_Snapshot_GetCode", base.gameObject);
						}
						else
						{
							Debug.LogError("Error while uploading snapshot: " + query.Error);
							UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.ErrorUploadingSnapshot, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
							AkSoundEngine.PostEvent("UI_Snapshot_Error", base.gameObject);
							publishAreaController.ForceSubdialog(publishAreaPublishButtonRect);
						}
					});
				}
				else
				{
					Debug.LogError("Could not upload snapshot: Too big!");
					UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.ErrorTooBig, 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
					publishAreaController.ForceSubdialog(publishAreaPublishButtonRect);
				}
				PickableButton.ResetMasks();
			};
			publishAreaController.ForceSubdialog(publishAreaSpinnerRect);
			PickableButton.maskAll = true;
			WorkerThreadManager.Instance.AddFileOpJob(delegate
			{
				xmlDoc.LoadXml(QuickSaver.lastLoadedXml);
			}, onFinish);
		}
	}

	private void EnableCodeSection(string formattedCode)
	{
		usingCode = true;
		snapshotCodeText.text = formattedCode;
		publishAreaController.ForceSubdialog(publishAreaCodeRect);
	}

	public void OnClickRedditShareButton(PickCursor pickCursor)
	{
		OnClickShareButtonBase(UndergroundComputer.ShareSnapshotCodeOnReddit);
	}

	public void OnClickTwitterShareButton(PickCursor pickCursor)
	{
		OnClickShareButtonBase(UndergroundComputer.ShareSnapshotCodeOnTwitter);
	}

	public void OnClickChallengeTwitterShareButton(ChallengeScoreboard challengeScoreboard)
	{
		OnClickShareButtonBase(challengeScoreboard.ShareSnapshotCodeOnTwitter, ChallengeScoreboardScreenshot: true);
	}

	public void OnClickChallengeRedditShareButton(ChallengeScoreboard challengeScoreboard)
	{
		OnClickShareButtonBase(challengeScoreboard.ShareSnapshotCodeOnReddit, ChallengeScoreboardScreenshot: true);
	}

	public void OnClickImgurShareButton(PickCursor pickCursor)
	{
		OnClickShareButtonBase(delegate(string name, string code, string url)
		{
			UndergroundComputer.ShareSnapshotCodeOnImgur(url);
		});
	}

	public void OnClickGetCurrentSnapshotCode(PickCursor pickCursor)
	{
		OnClickShareButtonBase(delegate(string name, string code, string imageURL)
		{
			QuickSaver.CopyStringToClipboard(code);
			UserMessageManager.Instance.UserMessage(ScriptLocalization.Snapshot.ShareableCodeClipboard, 2f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: true);
			if (GameSettings.GetInstance().GameMode == GameState.GameMode.CHALLENGE && LobbyManager.instance.IsHost)
			{
				GameState.GetInstance().currentSnapshotInfo.snapshotCode = code;
				ReloadLevelInNewMode();
			}
		});
	}
}
