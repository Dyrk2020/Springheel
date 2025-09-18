using System;
using System.Collections.Generic;
using GameSparks.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AdminReportContents : MonoBehaviour
{
	private AdminUserReportsDialog userReportsDialog;

	private AdminUserReportsDialog.UserReportData reportData;

	public Transform contentRect;

	public Text reporterButtonText;

	public Text reportedButtonText;

	public Text commentText;

	public Text chatLogText;

	public Image acknowledgedCheckmark;

	public ScrollArrowController scrollArrowController;

	private string chatLog;

	private string comments;

	public void Initialize(AdminUserReportsDialog userReportsDialog, AdminUserReportsDialog.UserReportData reportData, UnityAction<bool> onDataLoaded)
	{
		this.userReportsDialog = userReportsDialog;
		this.reportData = reportData;
		GameSparksQuery query = GameSparksManager.Instance.CreateQuery();
		query.SendSimpleRequest("adminGetUserReportDetails", new Dictionary<string, object> { { "recordID", reportData.recordID } }, returnScriptData: true);
		GameSparksQuery gameSparksQuery = query;
		gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate
		{
			if (query.HasError)
			{
				Debug.LogError("Error in adminGetUserReportDetails response: " + query.Error);
				onDataLoaded(arg0: false);
			}
			else
			{
				GSData resultDataGSData = query.GetResultDataGSData("scriptData");
				if (resultDataGSData != null)
				{
					GSData gSData = resultDataGSData.GetGSData("foundRecord");
					if (gSData != null)
					{
						comments = gSData.GetString("reportComments");
						chatLog = gSData.GetString("reportChatLog");
						reporterButtonText.text = "Reported by: " + reportData.reporterDisplayName;
						reportedButtonText.text = "Reported user: " + reportData.reportedDisplayName;
						commentText.text = "Comments: " + comments;
						chatLogText.text = "Chat Log:\n" + chatLog;
						acknowledgedCheckmark.enabled = reportData.acknowledged == 1;
						onDataLoaded(arg0: true);
					}
					else
					{
						Debug.LogError("Could not get data record from response");
						onDataLoaded(arg0: false);
					}
				}
				else
				{
					Debug.LogError("Could not get scriptdata from response");
					onDataLoaded(arg0: false);
				}
			}
		});
	}

	public void OnClickAcknowledgeCheckbox(PickCursor pickCursor)
	{
		userReportsDialog.loadingOperation = true;
		PickableButton.maskAll = true;
		int newAckValue = ((reportData.acknowledged != 1) ? 1 : 0);
		GameSparksQuery query = GameSparksManager.Instance.CreateQuery();
		query.SendSimpleRequest("adminSetUserReportAcknowledged", new Dictionary<string, object>
		{
			{ "recordID", reportData.recordID },
			{ "acknowledged", newAckValue }
		}, returnScriptData: true);
		GameSparksQuery gameSparksQuery = query;
		gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate
		{
			userReportsDialog.loadingOperation = false;
			PickableButton.ResetMasks();
			if (query.HasError)
			{
				Debug.LogError("Error with adminSetUserReportAcknowledged query: " + query.Error);
			}
			else
			{
				reportData.acknowledged = newAckValue;
			}
		});
	}

	private void Update()
	{
		if (reportData != null)
		{
			acknowledgedCheckmark.enabled = reportData.acknowledged == 1;
		}
	}

	public void ShowContents(bool onOff)
	{
		contentRect.gameObject.SetActive(onOff);
	}

	public void OnClickReported(PickCursor pickCursor)
	{
		userReportsDialog.GoToUserProfile(reportData.reportedID, reportData.reportedDisplayName);
	}

	public void OnClickReporter(PickCursor pickCursor)
	{
		userReportsDialog.GoToUserProfile(reportData.reporterID, reportData.reporterDisplayName);
	}

	public void OnClickCopyComment(PickCursor pickCursor)
	{
		QuickSaver.CopyStringToClipboard(commentText.text);
		UserMessageManager.Instance.UserMessage("Copied comment to clipboard", 2f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: true);
	}

	public void OnClickCopyChatLog(PickCursor pickCursor)
	{
		QuickSaver.CopyStringToClipboard(chatLogText.text);
		UserMessageManager.Instance.UserMessage("Copied chat log to clipboard", 2f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: true);
	}
}
