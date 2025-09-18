using System;
using System.Collections.Generic;
using GameSparks.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AdminReportList : MonoBehaviour
{
	public AdminUserReportsDialog userReportsDialog;

	public AdminReportContents reportContents;

	public Text pageIndicatorText;

	public UnityEngine.Object latestReportEntryPrefab;

	public Transform latestReportHeaderContainer;

	public UnityEngine.Object mostReportEntryPrefab;

	public Transform mostReportHeaderContainer;

	public Transform reportListContainer;

	public ScrollArrowController scrollArrowController;

	public Text playerInfoText;

	public Image banInfoSpinner;

	public Text banInfoText;

	public GenericButton banPlayerButton;

	public GenericButton unbanPlayerButton;

	private string lastBanReason;

	private long lastBanHours;

	private AdminReportEntry currentEntry;

	public int currentPage;

	public int totalPages;

	public void Initialize(AdminUserReportsDialog userReportsDialog)
	{
		this.userReportsDialog = userReportsDialog;
		reportContents.ShowContents(onOff: false);
	}

	public bool SetPageData(int current, int total)
	{
		int num = currentPage;
		currentPage = current;
		totalPages = total;
		if (currentPage > totalPages - 1 && totalPages >= 1)
		{
			currentPage = totalPages - 1;
		}
		pageIndicatorText.text = "Page " + (currentPage + 1) + "/" + totalPages;
		return num != currentPage;
	}

	public void ClearContents()
	{
		if (currentEntry != null)
		{
			DeselectCurrentEntry();
		}
		reportListContainer.DestroyAllChildren();
	}

	public void AddEntry(AdminUserReportsDialog.UserReportData reportData)
	{
		reportListContainer.gameObject.AddPrefabAsChild<AdminReportEntry>(latestReportEntryPrefab).Initialize(this, reportData);
	}

	public void AddEntry(AdminUserReportsDialog.MostReportData reportData)
	{
		reportListContainer.gameObject.AddPrefabAsChild<AdminMostReportedEntry>(mostReportEntryPrefab).Initialize(this, reportData);
	}

	public void SetDisplayMode(string listingType)
	{
		switch (listingType)
		{
		default:
			latestReportHeaderContainer.gameObject.SetActive(value: true);
			mostReportHeaderContainer.gameObject.SetActive(value: false);
			break;
		case "mostReportedUsers":
		case "biggestSubmitters":
		case "mostReportedWithNew":
			latestReportHeaderContainer.gameObject.SetActive(value: false);
			mostReportHeaderContainer.gameObject.SetActive(value: true);
			break;
		}
	}

	public void OnClickNextPage(PickCursor pickCursor)
	{
		int num = currentPage;
		currentPage++;
		if (currentPage > totalPages - 1)
		{
			currentPage = totalPages - 1;
		}
		if (currentPage != num)
		{
			userReportsDialog.RefreshView(resetPage: false);
		}
	}

	public void OnClickPreviousPage(PickCursor pickCursor)
	{
		int num = currentPage;
		currentPage--;
		if (currentPage < 0)
		{
			currentPage = 0;
		}
		if (currentPage != num)
		{
			userReportsDialog.RefreshView(resetPage: false);
		}
	}

	public void DeselectCurrentEntry()
	{
		reportContents.ShowContents(onOff: false);
		currentEntry.OnDeselect();
		currentEntry = null;
	}

	public void OnClickEntry(PickCursor pickCursor, AdminReportEntry entry)
	{
		if (currentEntry != null)
		{
			DeselectCurrentEntry();
		}
		currentEntry = entry;
		currentEntry.OnSelect();
		reportContents.ShowContents(onOff: false);
		userReportsDialog.loadingOperation = true;
		PickableButton.maskAll = true;
		reportContents.Initialize(userReportsDialog, entry.reportData, delegate(bool success)
		{
			if (!success)
			{
				Debug.LogError("Retrieving report data failed");
				UserMessageManager.Instance.UserMessage("Could not retrieve report data...");
				DeselectCurrentEntry();
			}
			reportContents.ShowContents(success);
			reportContents.scrollArrowController.ResetScrolling();
			userReportsDialog.loadingOperation = false;
			PickableButton.ResetMasks();
		});
	}

	public void OnClickEntry(PickCursor pickCursor, AdminMostReportedEntry entry)
	{
		userReportsDialog.GoToUserProfile(entry.reportData.playerID, entry.reportData.playerDisplayName);
	}

	public void OnClickReportedUsername(PickCursor pickCursor)
	{
		if (currentEntry != null)
		{
			userReportsDialog.GoToUserProfile(currentEntry.reportData.reportedID, currentEntry.reportData.reportedDisplayName);
		}
	}

	public void OnClickReporterUsername(PickCursor pickCursor)
	{
		if (currentEntry != null)
		{
			userReportsDialog.GoToUserProfile(currentEntry.reportData.reporterID, currentEntry.reportData.reporterDisplayName);
		}
	}

	public void SetUserName(string userName)
	{
		if (playerInfoText != null)
		{
			playerInfoText.text = "Reports for " + userName;
		}
	}

	public void OnClickBanCurrentPlayer(PickCursor pickCursor)
	{
		AdminPanelDialog adminPanel = userReportsDialog.adminPanelDialog;
		adminPanel.PopupModalDialog_Input(pickCursor.localNumber, "Set ban duration in hours (0 = permanent)", lastBanHours.ToString(), "Enter duration...", delegate
		{
			int result = 0;
			if (int.TryParse(adminPanel.modalInputField.text, out result))
			{
				OnBanDurationSet(pickCursor.localNumber, result);
			}
			else
			{
				Debug.LogError("Could not parse ban hours");
				UserMessageManager.Instance.UserMessage("Could not parse value");
			}
		}, delegate
		{
		});
	}

	public void OnBanDurationSet(int playerNumber, int hours)
	{
		AdminPanelDialog adminPanel = userReportsDialog.adminPanelDialog;
		adminPanel.PopupModalDialog_Input(playerNumber, "Set a reason for the ban", lastBanReason, "Enter reason...", delegate
		{
			PickableButton.maskAll = true;
			banInfoSpinner.enabled = true;
			SetBanInfoUILoading();
			string reasonValue = adminPanel.modalInputField.text;
			userReportsDialog.BanCurrentUser(hours, reasonValue, delegate(bool success)
			{
				banInfoSpinner.enabled = false;
				PickableButton.ResetMasks();
				if (success)
				{
					SetBanInfo(hours * 3600, reasonValue);
					UserMessageManager.Instance.UserMessage("Player banned");
				}
				else
				{
					ResetBanInfo();
					Debug.LogError("Failed to ban user...");
					UserMessageManager.Instance.UserMessage("Failed to ban player");
				}
			});
		}, delegate
		{
		});
	}

	public void OnClickUnbanCurrentPlayer(PickCursor pickCursor)
	{
		PickableButton.maskAll = true;
		banInfoSpinner.enabled = true;
		SetBanInfoUILoading();
		userReportsDialog.UnbanCurrentUser(delegate(bool success)
		{
			PickableButton.ResetMasks();
			banInfoSpinner.enabled = false;
			if (success)
			{
				UserMessageManager.Instance.UserMessage("Player unbanned");
			}
			else
			{
				UserMessageManager.Instance.UserMessage("Failed to unban player");
			}
			RefreshBanInfo();
		});
	}

	public void RefreshBanInfo()
	{
		PickableButton.maskAll = true;
		banInfoSpinner.enabled = true;
		SetBanInfoUILoading();
		GameSparksQuery query = GameSparksManager.Instance.CreateQuery();
		query.SendSimpleRequest("GetBanInfo", new Dictionary<string, object> { { "playerID", userReportsDialog.currentPlayerId } }, returnScriptData: true);
		GameSparksQuery gameSparksQuery = query;
		gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate
		{
			PickableButton.ResetMasks();
			banInfoSpinner.enabled = false;
			if (query.HasError)
			{
				ResetBanInfo();
				Debug.LogError("Error while fetching ban info: " + query.Error);
			}
			else
			{
				GSData resultDataGSData = query.GetResultDataGSData("scriptData");
				if (resultDataGSData != null)
				{
					long valueOrDefault = resultDataGSData.GetLong("hoursToGo").GetValueOrDefault();
					string reason = resultDataGSData.GetString("reason");
					SetBanInfo(valueOrDefault * 3600, reason);
				}
				else
				{
					Debug.LogError("Could not parse script data");
				}
			}
		});
	}

	private void ResetBanInfo()
	{
		lastBanHours = 0L;
		lastBanReason = null;
		banInfoText.text = "Not Banned";
		banPlayerButton.buttonText.text = "Ban Player";
		banPlayerButton.gameObject.SetActive(value: true);
		unbanPlayerButton.gameObject.SetActive(value: false);
	}

	private void SetBanInfo(long durationSeconds, string reason)
	{
		lastBanReason = reason;
		lastBanHours = durationSeconds / 3600;
		string text = "Banned for:\n";
		text = ((durationSeconds != 0L) ? (text + AdminPanelDialog.DurationToStringEnglish(durationSeconds)) : (text + "Permanent"));
		text = text + "\n\nReason:\n" + reason;
		banInfoText.text = text;
		banPlayerButton.buttonText.text = "Update Ban";
		banPlayerButton.gameObject.SetActive(value: true);
		unbanPlayerButton.gameObject.SetActive(value: true);
	}

	private void SetBanInfoUILoading()
	{
		banInfoText.text = "Loading...";
		banPlayerButton.buttonText.text = "Ban Player";
		banPlayerButton.gameObject.SetActive(value: false);
		unbanPlayerButton.gameObject.SetActive(value: false);
	}
}
