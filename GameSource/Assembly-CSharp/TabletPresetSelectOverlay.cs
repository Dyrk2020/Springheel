using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TabletPresetSelectOverlay : MonoBehaviour
{
	public enum Mode
	{
		Open,
		Save,
		PromptOnly
	}

	public enum PromptMode
	{
		None,
		AreYouSureOpen,
		AreYouSureOverwrite,
		AreYouSureDelete,
		ResetAll
	}

	public TabletRulesScreen rulesScreen;

	public TabletTextLabel titleText;

	[Header("Subdialogs")]
	public TabletSubdialogController subdialogController;

	public RectTransform fileSubdialog;

	public RectTransform confirmSubdialog;

	public RectTransform emptySubdialog;

	public RectTransform nameEntrySubdialog;

	public RectTransform selectElementsSubdialog;

	[Header("File Dialog")]
	public TabletTextLabel fileListTitleText;

	public TabletButton prevPageButton;

	public TabletButton nextPageButton;

	public TabletTextLabel pageLabel;

	public RectTransform fileListContainer;

	public TabletPresetEntry[] presetEntries;

	[Header("Confirm Prompt")]
	public TabletTextLabel promptMessageLabel;

	public TabletTextLabel promptConfirmLabel;

	public TabletButton promptConfirmButton;

	[Header("Name Entry")]
	public InputField rulesetNameInput;

	public Image rulesetNameBackground;

	public TabletSimpleAnimator rulesetNameInputAnimator;

	public InputField rulesetDescriptionInput;

	public Image rulesetDescriptionBackground;

	public TabletSimpleAnimator rulesetDescInputAnimator;

	[Header("Load Elements")]
	public TabletTextLabel selectElementsRulesetName;

	public TabletTextLabel selectElementsRulesetDescription;

	public TabletCheckbox selectElementsRulesCheckbox;

	public TabletCheckbox selectElementsBlocksCheckbox;

	public TabletCheckbox selectElementsPointsCheckbox;

	public TabletCheckbox selectElementsModifiersCheckbox;

	public TabletDisableGroup selectElementsConfirmDisableGroup;

	[Header("Background Fade")]
	public Color overlayBackgroundColor;

	public AnimationCurve fadeCurve;

	[Header("Misc")]
	public bool isOpen;

	private Mode mode;

	private PromptMode promptMode;

	private int currentPage;

	private List<GameRulePreset> listedPresets;

	private Dictionary<GameRulePreset, int> presetIdxDict = new Dictionary<GameRulePreset, int>();

	private int overwritingPreset = -1;

	private int selectElementsPreset = -1;

	private UnityAction onPromptCancel;

	private UnityAction onPromptConfirm;

	private IEnumerator anim;

	private bool fadingBackground;

	private int EntriesPerPage => presetEntries.Length;

	private int NumPages => Mathf.CeilToInt((float)listedPresets.Count / (float)EntriesPerPage);

	private void Awake()
	{
		((RectTransform)base.transform).anchoredPosition = Vector2.zero;
		base.gameObject.SetActive(value: false);
		isOpen = false;
	}

	public void Initialize(Mode mode, UnityAction OnBackgroundAppear)
	{
		base.gameObject.SetActive(value: true);
		isOpen = true;
		this.mode = mode;
		subdialogController.ForceSubdialog(emptySubdialog);
		FadeBackground(fadeIn: true, OnBackgroundAppear);
	}

	public void Close()
	{
		base.gameObject.SetActive(value: false);
		isOpen = false;
	}

	private void UpdatePresetList(int startIdx)
	{
		int num = Mathf.Min(startIdx + EntriesPerPage, listedPresets.Count);
		int i;
		for (i = 0; i < EntriesPerPage && startIdx + i < num; i++)
		{
			int index = startIdx + i;
			GameRulePreset gameRulePreset = listedPresets[index];
			presetEntries[i].gameObject.SetActive(value: true);
			if (gameRulePreset != null)
			{
				presetEntries[i].Initialize(presetIdxDict[gameRulePreset], gameRulePreset.GetNameString(), !gameRulePreset.IsPremade);
				continue;
			}
			string translation = LocalizationManager.GetTranslation("RuleBook/Presets/NewPreset");
			if (mode == Mode.Save && startIdx == 0 && i == 0)
			{
				presetEntries[i].Initialize(-1, translation, showDelete: false);
			}
		}
		for (; i < EntriesPerPage; i++)
		{
			presetEntries[i].gameObject.SetActive(value: false);
		}
	}

	public void OnClickDeletePreset(int presetIdx)
	{
		ShowPrompt(PromptMode.AreYouSureDelete, delegate
		{
			GameSettings gs = GameSettings.GetInstance();
			GameRulePreset gameRulePreset = gs.rulePresetList[presetIdx];
			bool shouldRefreshRulesetName = !gs.HasDirtyRuleset && gameRulePreset == gs.GetCurrentRuleset();
			GameRulePreset.DeleteRuleset(gameRulePreset, delegate(bool success)
			{
				if (success)
				{
					if (shouldRefreshRulesetName)
					{
						rulesScreen.AnimateChangeToRuleset(gs.GetCurrentRuleset());
					}
				}
				else
				{
					Debug.LogError("ERROR");
				}
				RebuildFileList();
				currentPage = Mathf.Min(currentPage, NumPages - 1);
				UpdatePresetList(EntriesPerPage * currentPage);
				UpdatePageDisplay();
				subdialogController.TransitionRightTo(fileSubdialog, GetTransitionSound());
			});
		}, delegate
		{
			subdialogController.TransitionRightTo(fileSubdialog, GetTransitionSound());
		});
	}

	public void OnSelectPreset(int presetIdx)
	{
		switch (mode)
		{
		case Mode.Open:
		{
			GameRulePreset gameRulePreset = GameSettings.GetInstance().rulePresetList[presetIdx];
			subdialogController.TransitionLeftTo(selectElementsSubdialog, GetTransitionSound());
			selectElementsRulesetName.text = gameRulePreset.GetNameString();
			string descriptionString = gameRulePreset.GetDescriptionString();
			if (!descriptionString.NullOrEmpty())
			{
				if (descriptionString.Length > 80)
				{
					selectElementsRulesetDescription.GetComponent<Text>().fontSize = 40;
				}
				else
				{
					selectElementsRulesetDescription.GetComponent<Text>().fontSize = 50;
				}
				selectElementsRulesetDescription.text = descriptionString;
			}
			else
			{
				selectElementsRulesetDescription.GetComponent<Text>().fontSize = 50;
				selectElementsRulesetDescription.text = LocalizationManager.GetTranslation("RuleBook/Presets/NoDescription");
			}
			selectElementsRulesCheckbox.SetValue(val: true, triggerCallback: false);
			selectElementsPointsCheckbox.SetValue(val: true, triggerCallback: false);
			selectElementsBlocksCheckbox.SetValue(val: true, triggerCallback: false);
			selectElementsModifiersCheckbox.SetValue(val: true, triggerCallback: false);
			selectElementsConfirmDisableGroup.SetDisabled(disabled: false);
			selectElementsPreset = presetIdx;
			break;
		}
		case Mode.Save:
		{
			if (presetIdx == -1)
			{
				subdialogController.TransitionLeftTo(nameEntrySubdialog, GetTransitionSound());
				rulesetNameInput.text = "";
				rulesetDescriptionInput.text = "";
				overwritingPreset = -1;
				break;
			}
			GameSettings instance = GameSettings.GetInstance();
			GameRulePreset preset = instance.rulePresetList[presetIdx];
			ShowPrompt(PromptMode.AreYouSureOverwrite, delegate
			{
				subdialogController.TransitionLeftTo(nameEntrySubdialog, GetTransitionSound());
				rulesetNameInput.text = preset.Name;
				rulesetDescriptionInput.text = preset.Description;
				overwritingPreset = presetIdx;
			}, delegate
			{
				subdialogController.TransitionRightTo(fileSubdialog, GetTransitionSound());
			});
			break;
		}
		}
	}

	public void OnClickNextPage(PickCursor pickCursor)
	{
		_ = NumPages;
		if (currentPage < NumPages - 1)
		{
			currentPage++;
			UpdatePresetList(currentPage * EntriesPerPage);
			UpdatePageDisplay();
		}
	}

	public void OnClickPreviousPage(PickCursor pickCursor)
	{
		if (currentPage > 0)
		{
			currentPage--;
			UpdatePresetList(currentPage * EntriesPerPage);
			UpdatePageDisplay();
		}
	}

	private void UpdatePageDisplay()
	{
		int numPages = NumPages;
		if (numPages > 1)
		{
			prevPageButton.gameObject.SetActive(value: true);
			nextPageButton.gameObject.SetActive(value: true);
			pageLabel.gameObject.SetActive(value: true);
			prevPageButton.SetDisabled(currentPage == 0);
			nextPageButton.SetDisabled(currentPage == numPages - 1);
			pageLabel.text = currentPage + 1 + "/" + numPages;
		}
		else
		{
			prevPageButton.gameObject.SetActive(value: false);
			nextPageButton.gameObject.SetActive(value: false);
			pageLabel.gameObject.SetActive(value: false);
		}
	}

	public void ShowPrompt(PromptMode promptMode, UnityAction onPromptConfirm, UnityAction onPromptCancel)
	{
		this.promptMode = promptMode;
		subdialogController.TransitionLeftTo(confirmSubdialog, GetTransitionSound());
		this.onPromptConfirm = onPromptConfirm;
		this.onPromptCancel = onPromptCancel;
		switch (promptMode)
		{
		case PromptMode.AreYouSureOpen:
			promptMessageLabel.Term = "RuleBook/Presets/AreYouSureLoad";
			promptConfirmLabel.Term = "RuleBook/Presets/ConfirmContinue";
			promptConfirmButton.clickSound = "UI_UPad_Button_Click_Ok_Soft";
			break;
		case PromptMode.AreYouSureOverwrite:
			promptMessageLabel.Term = "RuleBook/Presets/AreYouSureOverwrite";
			promptConfirmLabel.Term = "RuleBook/Presets/ConfirmOverwrite";
			promptConfirmButton.clickSound = "UI_UPad_Button_Click_Ok_Soft";
			break;
		case PromptMode.ResetAll:
			promptMessageLabel.Term = "RuleBook/Presets/AreYouSureResetAll";
			promptConfirmLabel.Term = "RuleBook/Presets/ConfirmReset";
			promptConfirmButton.clickSound = "UI_UPad_Button_Click_Reset";
			break;
		case PromptMode.AreYouSureDelete:
			promptMessageLabel.Term = "RuleBook/Presets/AreYouSureDelete";
			promptConfirmLabel.Term = "RuleBook/Presets/ConfirmDelete";
			promptConfirmButton.clickSound = "UI_UPad_Button_Click_Reset";
			break;
		}
		promptConfirmButton.buttonType = TabletButton.ButtonType.Dangerous;
		promptConfirmButton.SetDisabled(disabled: false);
	}

	public void OnPressBack(PickCursor pickCursor)
	{
		if (subdialogController.currentSubdialog == confirmSubdialog)
		{
			OnClickPromptCancel(pickCursor);
		}
		else if (subdialogController.currentSubdialog == fileSubdialog)
		{
			subdialogController.ForceSubdialog(fileSubdialog);
			FadeBackground(fadeIn: false, Close);
			subdialogController.TransitionRightTo(emptySubdialog, GetTransitionSound());
		}
		else if (subdialogController.currentSubdialog == nameEntrySubdialog)
		{
			OnClickNameEntryCancel(pickCursor);
		}
		else if (subdialogController.currentSubdialog == selectElementsSubdialog)
		{
			OnClickSelectElementCancel(pickCursor);
		}
	}

	public bool OnRotateLeft(PickCursor pickCursor)
	{
		if (!pickCursor.lastRotateWasMouseWheel && subdialogController.currentSubdialog == fileSubdialog && currentPage > 0)
		{
			prevPageButton.OnAccept(pickCursor);
			return true;
		}
		return false;
	}

	public bool OnRotateRight(PickCursor pickCursor)
	{
		if (!pickCursor.lastRotateWasMouseWheel && subdialogController.currentSubdialog == fileSubdialog && currentPage < NumPages - 1)
		{
			nextPageButton.OnAccept(pickCursor);
			return true;
		}
		return false;
	}

	public void OnClickPromptConfirm(PickCursor pickCursor)
	{
		onPromptConfirm();
		onPromptConfirm = null;
		onPromptCancel = null;
	}

	public void OnClickPromptCancel(PickCursor pickCursor)
	{
		switch (promptMode)
		{
		case PromptMode.AreYouSureOpen:
		case PromptMode.ResetAll:
			TransitionOut(TabletSubdialogController.TransitionDirection.Right, onPromptCancel);
			break;
		case PromptMode.AreYouSureOverwrite:
		case PromptMode.AreYouSureDelete:
			subdialogController.TransitionRightTo(fileSubdialog, GetTransitionSound());
			if (onPromptCancel != null)
			{
				onPromptCancel();
			}
			break;
		}
		onPromptConfirm = null;
		onPromptCancel = null;
	}

	public void OnClickNameEntryCancel(PickCursor pickCursor)
	{
		subdialogController.TransitionRightTo(fileSubdialog, GetTransitionSound());
	}

	public void OnClickNameEntryConfirm(PickCursor pickCursor)
	{
		GameSettings gs = GameSettings.GetInstance();
		if (rulesetNameInput.text.Length > rulesetNameInput.characterLimit)
		{
			UserMessageManager.Instance.UserMessage(ScriptLocalization.RuleBook_Presets.RulesetNameTooLong, 3f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: true);
			return;
		}
		if (overwritingPreset == -1)
		{
			GameRulePreset.SaveRules(rulesetNameInput.text, rulesetDescriptionInput.text, delegate(string str)
			{
				rulesetNameInput.text = str;
				Debug.Log("Input sanitized to: " + str);
			}, delegate(GameRulePreset ruleset)
			{
				if (ruleset != null)
				{
					TransitionOut(TabletSubdialogController.TransitionDirection.Left, Close);
					rulesScreen.NotifyPresetLoad(gs.rulePresetList.Count - 1, loadRules: true, loadPoints: true, loadBlocks: true, loadMods: true);
					rulesScreen.AnimateChangeToRuleset(ruleset);
				}
				else
				{
					Debug.LogError("There was an error saving the ruleset");
				}
			});
			return;
		}
		GameRulePreset.DeleteRuleset(gs.rulePresetList[overwritingPreset], delegate(bool success)
		{
			if (success)
			{
				_ = rulesetNameInput.text;
				GameRulePreset.SaveRules(rulesetNameInput.text, rulesetDescriptionInput.text, delegate(string str)
				{
					rulesetNameInput.text = str;
					Debug.Log("Input sanitized to: " + str);
				}, delegate(GameRulePreset ruleset)
				{
					if (ruleset != null)
					{
						TransitionOut(TabletSubdialogController.TransitionDirection.Left, Close);
						int rulesetIndex = gs.GetRulesetIndex(ruleset);
						rulesScreen.NotifyPresetLoad(rulesetIndex, loadRules: true, loadPoints: true, loadBlocks: true, loadMods: true);
						rulesScreen.AnimateChangeToRuleset(ruleset);
					}
					else
					{
						Debug.LogError("There was an error saving the ruleset");
					}
				});
			}
			else
			{
				Debug.LogError("There was a problem deleting the old ruleset.");
			}
		});
	}

	public void OnClickSelectElementCancel(PickCursor pickCursor)
	{
		subdialogController.TransitionRightTo(fileSubdialog, GetTransitionSound());
	}

	public void OnClickSelectElementConfirm(PickCursor pickCursor)
	{
		bool value = selectElementsRulesCheckbox.Value;
		bool value2 = selectElementsPointsCheckbox.Value;
		bool value3 = selectElementsBlocksCheckbox.Value;
		bool value4 = selectElementsModifiersCheckbox.Value;
		rulesScreen.LoadPreset(selectElementsPreset, value, value2, value3, value4);
		TransitionOut(TabletSubdialogController.TransitionDirection.Left, Close);
	}

	public void OnSelectElementCheckboxValueChange()
	{
		bool value = selectElementsRulesCheckbox.Value;
		bool value2 = selectElementsPointsCheckbox.Value;
		bool value3 = selectElementsBlocksCheckbox.Value;
		bool value4 = selectElementsModifiersCheckbox.Value;
		bool flag = value || value2 || value3 || value4;
		selectElementsConfirmDisableGroup.SetDisabled(!flag);
	}

	public void OnClickNameEntryNameField(PickCursor pickCursor)
	{
		string translation = LocalizationManager.GetTranslation("RuleBook/Presets/NamePlaceholder");
		Color originalColor = rulesetNameBackground.color;
		Color buttonBgColor_TransparentHighlight = rulesScreen.colorScheme.buttonBgColor_TransparentHighlight;
		rulesetNameInputAnimator.FadeColor(originalColor, buttonBgColor_TransparentHighlight, 0.25f, Easings.Functions.CubicEaseOut);
		Tablet.ActivateInputField(pickCursor, rulesetNameInput, translation, delegate(string str)
		{
			rulesetNameInput.text = str;
			rulesetNameInputAnimator.FadeColor(rulesetNameBackground.color, originalColor, 0.25f, Easings.Functions.CubicEaseOut);
		});
	}

	public void OnClickNameEntryDescriptionField(PickCursor pickCursor)
	{
		string translation = LocalizationManager.GetTranslation("RuleBook/Presets/DescriptionPlaceholder");
		Color originalColor = rulesetDescriptionBackground.color;
		Color buttonBgColor_TransparentHighlight = rulesScreen.colorScheme.buttonBgColor_TransparentHighlight;
		rulesetDescInputAnimator.FadeColor(originalColor, buttonBgColor_TransparentHighlight, 0.25f, Easings.Functions.CubicEaseOut);
		Tablet.ActivateInputField(pickCursor, rulesetDescriptionInput, translation, delegate(string str)
		{
			rulesetDescriptionInput.text = str;
			rulesetDescInputAnimator.FadeColor(rulesetDescriptionBackground.color, originalColor, 0.25f, Easings.Functions.CubicEaseOut);
		});
	}

	private void RebuildFileList()
	{
		GameSettings instance = GameSettings.GetInstance();
		listedPresets = new List<GameRulePreset>();
		presetIdxDict.Clear();
		if (mode == Mode.Save)
		{
			listedPresets.Add(null);
		}
		for (int i = 0; i < instance.rulePresetList.Count; i++)
		{
			GameRulePreset gameRulePreset = instance.rulePresetList[i];
			presetIdxDict.Add(gameRulePreset, i);
			if (mode != Mode.Save || !gameRulePreset.IsPremade)
			{
				listedPresets.Add(gameRulePreset);
			}
		}
		listedPresets.Sort(delegate(GameRulePreset a, GameRulePreset b)
		{
			if (a == null)
			{
				return -1;
			}
			if (b == null)
			{
				return 1;
			}
			if (a.IsPremade && !b.IsPremade)
			{
				return -1;
			}
			return (b.IsPremade && !a.IsPremade) ? 1 : a.name.CompareTo(b.name);
		});
	}

	public void SwitchToFileDialog()
	{
		switch (this.mode)
		{
		case Mode.Open:
			titleText.Term = "RuleBook/Presets/LoadTitle";
			break;
		case Mode.Save:
			titleText.Term = "RuleBook/Presets/SaveTitle";
			break;
		}
		Mode mode = this.mode;
		if ((uint)mode <= 1u)
		{
			subdialogController.TransitionLeftTo(fileSubdialog, GetTransitionSound());
			RebuildFileList();
			currentPage = 0;
			UpdatePresetList(0);
			UpdatePageDisplay();
		}
	}

	public void TransitionOut(TabletSubdialogController.TransitionDirection dir, UnityAction onFinish)
	{
		FadeBackground(fadeIn: false, onFinish);
		switch (dir)
		{
		case TabletSubdialogController.TransitionDirection.Left:
			subdialogController.TransitionLeftTo(emptySubdialog, GetTransitionSound());
			break;
		case TabletSubdialogController.TransitionDirection.Right:
			subdialogController.TransitionRightTo(emptySubdialog, GetTransitionSound());
			break;
		}
	}

	public void FadeBackground(bool fadeIn, UnityAction OnTransitionFinished)
	{
		if (anim != null)
		{
			Debug.LogWarning("Warning: background was already animating");
		}
		fadingBackground = true;
		if (fadeIn)
		{
			AkSoundEngine.PostEvent("UI_UPad_Modal_Open", base.gameObject);
		}
		else
		{
			AkSoundEngine.PostEvent("UI_UPad_Modal_Close", base.gameObject);
		}
		Image component = GetComponent<Image>();
		if (fadeIn)
		{
			component.SetAlpha(0f);
		}
		else
		{
			component.SetAlpha(overlayBackgroundColor.a);
		}
		anim = AnimateFadeIn(fadeIn, OnTransitionFinished);
	}

	private IEnumerator AnimateFadeIn(bool fadeIn, UnityAction OnTransitionFinished)
	{
		Image bgImage = GetComponent<Image>();
		float duration = 0.2f;
		float timer = 0f;
		while (timer < duration)
		{
			timer += Time.deltaTime;
			float time = ((!fadeIn) ? (1f - timer / duration) : (timer / duration));
			bgImage.SetAlpha(fadeCurve.Evaluate(time) * overlayBackgroundColor.a);
			yield return null;
		}
		bgImage.SetAlpha(fadeIn ? overlayBackgroundColor.a : 0f);
		OnTransitionFinished();
		fadingBackground = false;
	}

	private void Update()
	{
		if (anim != null && !anim.MoveNext())
		{
			anim = null;
		}
		if (Controller.lockedInputField == rulesetNameInput && Input.GetKeyDown(KeyCode.Tab))
		{
			Controller.UnlockInputField();
			OnClickNameEntryDescriptionField(null);
		}
	}

	private TabletScreen.TransitionSound GetTransitionSound()
	{
		if (fadingBackground)
		{
			return TabletScreen.TransitionSound.None;
		}
		return TabletScreen.TransitionSound.Modal;
	}
}
