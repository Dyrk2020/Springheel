using System;
using I2.Loc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ReportDialog : MonoBehaviour
{
	public UndergroundComputer undergroundComputer;

	public ThumbnailDisplaySlot thumbnailDisplaySlot;

	public Text levelNameText;

	public Text loadingIndicatorText;

	public Transform reportArea;

	public PickableBuildButton deleteButton;

	public InputField commentInputField;

	public ReportReason[] reasons;

	public ReportReason.Reason selectedReason;

	private string code;

	private void Awake()
	{
		HideAllPanes();
	}

	private void HideAllPanes()
	{
		reportArea.gameObject.SetActive(value: false);
		loadingIndicatorText.gameObject.SetActive(value: false);
	}

	private void Start()
	{
		OnClickReason(reasons[0]);
	}

	public void OnClickReason(ReportReason clickedReason)
	{
		ReportReason[] array = reasons;
		foreach (ReportReason obj in array)
		{
			obj.SetChecked(obj == clickedReason);
		}
		selectedReason = clickedReason.reason;
	}

	public void OnClose()
	{
		HideAllPanes();
	}

	public void DisplayLevelInfo(string code, string levelName)
	{
		this.code = code;
		OnClickReason(reasons[0]);
		commentInputField.text = "";
		deleteButton.gameObject.SetActive(value: false);
		levelNameText.text = levelName;
		thumbnailDisplaySlot.LoadThumbnail(code, levelName);
		reportArea.gameObject.SetActive(value: false);
		loadingIndicatorText.gameObject.SetActive(value: true);
		GameSparksQuery gameSparksQuery = GameSparksManager.Instance.CreateQuery();
		gameSparksQuery.GetMyLevelReport(code);
		gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate(GameSparksQuery q)
		{
			if (PickableBuildButton.buildMenuCurrentState == PickableBuildButton.BuildScreenStates.ReportDialog)
			{
				if (q.ResultData != null)
				{
					ReportReason.Reason reason = (ReportReason.Reason)(int)q.ResultData["reportReason"];
					ReportReason[] array = reasons;
					foreach (ReportReason reportReason in array)
					{
						if (reportReason.reason == reason)
						{
							OnClickReason(reportReason);
							break;
						}
					}
					commentInputField.text = (string)q.ResultData["reportComment"];
					deleteButton.gameObject.SetActive(value: true);
				}
				loadingIndicatorText.gameObject.SetActive(value: false);
				reportArea.gameObject.SetActive(value: true);
			}
		});
	}

	public void OnClickSubmit(bool delete)
	{
		PickableButton.maskAll = true;
		undergroundComputer.AddLoadingOperation();
		GameSparksQuery query = GameSparksManager.Instance.CreateQuery();
		query.SubmitLevelReport(code, (int)selectedReason, commentInputField.text, delete);
		GameSparksQuery gameSparksQuery = query;
		gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate
		{
			PickableButton.ResetMasks();
			undergroundComputer.RemoveLoadingOperation();
			if (!query.HasError)
			{
				UserMessageManager.Instance.UserMessage(delete ? LocalizationManager.GetTranslation("UndergroundComputer/ReportDeleted") : LocalizationManager.GetTranslation("UndergroundComputer/ReportSubmitted"), 2f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: true);
				undergroundComputer.CloseReportDialog();
			}
			else
			{
				UserMessageManager.Instance.UserMessage(LocalizationManager.GetTranslation("UndergroundComputer/ErrorSubmittingReport"), 2f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
			}
		});
	}

	public void OnClickDelete()
	{
		OnClickSubmit(delete: true);
	}

	public void ActivateCommentInputField(Cursor pickCursor)
	{
		Controller.LockInputField(commentInputField, delegate
		{
			PickableButton.ResetMasks();
		});
		PickableButton.maskAll = true;
		SteamDeck.OpenVirtualKeyboard(pickCursor);
	}
}
