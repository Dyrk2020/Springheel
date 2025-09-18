using System;
using I2.Loc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ScoreboardReportDialog : MonoBehaviour
{
	private ChallengeScoreboard challengeScoreboard;

	private string levelCode;

	public ReportReason[] reasons;

	public ReportReason.Reason selectedReason;

	public InputField commentInputField;

	public GenericButton submitButton;

	public GenericButton deleteButton;

	public Transform reportBoxContainer;

	private void Awake()
	{
		challengeScoreboard = GetComponent<ChallengeScoreboard>();
	}

	private void Start()
	{
		OnClickReason(reasons[0]);
	}

	public void SetData(string levelCode)
	{
		this.levelCode = levelCode;
	}

	public void Initialize()
	{
		if (levelCode == null)
		{
			Debug.LogError("Level code isn't set!!!");
			return;
		}
		OnClickReason(reasons[0]);
		commentInputField.text = "";
		deleteButton.gameObject.SetActive(value: false);
		challengeScoreboard.ShowLoadingIndicator(onOff: true);
		reportBoxContainer.gameObject.SetActive(value: false);
		GameSparksQuery gameSparksQuery = GameSparksManager.Instance.CreateQuery();
		gameSparksQuery.GetMyLevelReport(levelCode);
		gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate(GameSparksQuery q)
		{
			if (!q.HasError)
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
				challengeScoreboard.ShowLoadingIndicator(onOff: false);
				reportBoxContainer.gameObject.SetActive(value: true);
			}
		});
	}

	public void OnClickBack()
	{
		challengeScoreboard.OnClickReportDialogBack();
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

	public void OnClickSubmit(bool delete)
	{
		PickableButton.maskAll = true;
		challengeScoreboard.ShowLoadingIndicator(onOff: true);
		GameSparksQuery query = GameSparksManager.Instance.CreateQuery();
		query.SubmitLevelReport(levelCode, (int)selectedReason, commentInputField.text, delete);
		GameSparksQuery gameSparksQuery = query;
		gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate
		{
			PickableButton.ResetMasks();
			challengeScoreboard.ShowLoadingIndicator(onOff: false);
			if (!query.HasError)
			{
				UserMessageManager.Instance.UserMessage(delete ? LocalizationManager.GetTranslation("UndergroundComputer/ReportDeleted") : LocalizationManager.GetTranslation("UndergroundComputer/ReportSubmitted"), 2f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: true);
				challengeScoreboard.SetFlaggedState(!delete);
				OnClickBack();
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

	public void ActivateCommentInputField(PickCursor cursor)
	{
		PickableButton.AllowOnlyButtons(submitButton, deleteButton);
		Controller.LockInputField(commentInputField, delegate
		{
			PickableButton.ResetMasks();
		});
		SteamDeck.OpenVirtualKeyboard(cursor);
	}
}
