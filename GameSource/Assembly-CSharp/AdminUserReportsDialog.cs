using System;
using System.Collections.Generic;
using GameSparks.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AdminUserReportsDialog : MonoBehaviour
{
	public class UserReportData
	{
		public string recordID;

		public string reporterID;

		public string reporterDisplayName;

		public string reportedID;

		public string reportedDisplayName;

		public long timestamp;

		public int reportReason;

		public int acknowledged;

		private UserReportData(GSData gsData)
		{
			recordID = gsData.GetString("recordID");
			reporterID = gsData.GetString("reporterID");
			reporterDisplayName = gsData.GetString("reporterUsername");
			reportedID = gsData.GetString("reportedID");
			reportedDisplayName = gsData.GetString("reportedUsername");
			timestamp = gsData.GetLong("date").GetValueOrDefault();
			reportReason = gsData.GetInt("reportReason").GetValueOrDefault();
			acknowledged = gsData.GetInt("acknowledged").GetValueOrDefault();
		}

		public static UserReportData CreateFromGSData(GSData gsData)
		{
			try
			{
				return new UserReportData(gsData);
			}
			catch (Exception ex)
			{
				Debug.LogError("Exception while parsing in GS Data: " + ex.Message);
				return null;
			}
		}
	}

	public class MostReportData
	{
		public string playerID;

		public string playerDisplayName;

		public int reportCount;

		public int acknowledgedReportCount;

		public long latestTimestamp;

		public MostReportData(GSData gsData)
		{
			playerID = gsData.GetString("playerID");
			playerDisplayName = gsData.GetString("playerDisplayName");
			latestTimestamp = gsData.GetLong("latestReport").GetValueOrDefault();
			reportCount = gsData.GetInt("totalReports").GetValueOrDefault();
			acknowledgedReportCount = gsData.GetInt("totalAcknowledged").GetValueOrDefault();
		}

		public static MostReportData CreateFromGSData(GSData gsData)
		{
			try
			{
				return new MostReportData(gsData);
			}
			catch (Exception ex)
			{
				Debug.LogError("Exception while parsing in GS Data: " + ex.Message);
				return null;
			}
		}
	}

	public AdminPanelDialog adminPanelDialog;

	public DropdownMenu showDropdown;

	public Image hideAckCheckmark;

	public DropdownMenu rppDropdown;

	public Image loadingSpinner;

	public bool hideAcknowledged = true;

	public bool loadingOperation;

	public Transform mainReportContainer;

	public Transform userReportContainer;

	public AdminReportList mainReportList;

	public AdminReportList userReportList;

	public string currentPlayerId;

	private AdminReportList CurrentReportList
	{
		get
		{
			if (currentPlayerId.NullOrEmpty())
			{
				return mainReportList;
			}
			return userReportList;
		}
	}

	public void Initialize(AdminPanelDialog adminPanelDialog)
	{
		this.adminPanelDialog = adminPanelDialog;
		hideAckCheckmark.enabled = hideAcknowledged;
		loadingSpinner.enabled = false;
		mainReportList.Initialize(this);
		userReportList.Initialize(this);
		OnClickRefresh();
	}

	public static string GetReportReasonString(int reportReason)
	{
		UserReports.ReportReason reportReason2 = (UserReports.ReportReason)reportReason;
		return reportReason2.ToString();
	}

	private string GetCurrentListingType()
	{
		if (showDropdown.selectedDropdownEntry != null)
		{
			if (!currentPlayerId.NullOrEmpty())
			{
				return "latestReports";
			}
			switch (showDropdown.selectedDropdownEntry.EntryValue)
			{
			case 0:
				return "latestReports";
			case 1:
				return "mostReportedUsers";
			case 2:
				return "biggestSubmitters";
			case 3:
				return "mostReportedWithNew";
			}
		}
		return null;
	}

	public void OnClickRefresh()
	{
		loadingOperation = true;
		PickableButton.maskAll = true;
		bool flag = !currentPlayerId.NullOrEmpty();
		AdminReportList reportList = (flag ? userReportList : mainReportList);
		mainReportContainer.gameObject.SetActive(!flag);
		userReportContainer.gameObject.SetActive(flag);
		int rpp = ((rppDropdown.selectedDropdownEntry != null) ? rppDropdown.selectedDropdownEntry.EntryValue : 20);
		GameSparksQuery query = GameSparksManager.Instance.CreateQuery();
		query.SendSimpleRequest("adminGetUserReports", new Dictionary<string, object>
		{
			{
				"startIndex",
				reportList.currentPage * rpp
			},
			{ "resultsPerPage", rpp },
			{
				"listingType",
				GetCurrentListingType()
			},
			{
				"hideAcknowledged",
				hideAcknowledged ? 1 : 0
			},
			{
				"playerID",
				currentPlayerId.NullOrEmpty() ? "NULL" : currentPlayerId
			}
		}, returnScriptData: true);
		reportList.SetDisplayMode(GetCurrentListingType());
		reportList.SetPageData(reportList.currentPage, reportList.totalPages);
		GameSparksQuery gameSparksQuery = query;
		gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate
		{
			PickableButton.ResetMasks();
			loadingOperation = false;
			reportList.ClearContents();
			if (!query.HasError)
			{
				GSData resultDataGSData = query.GetResultDataGSData("scriptData");
				if (resultDataGSData != null)
				{
					int valueOrDefault = resultDataGSData.GetInt("totalEntries").GetValueOrDefault();
					if (reportList.SetPageData(reportList.currentPage, Mathf.Max(1, Mathf.CeilToInt((float)valueOrDefault / (float)rpp))))
					{
						RefreshView(resetPage: false);
					}
					else
					{
						List<GSData> gSDataList = resultDataGSData.GetGSDataList("returnedEntries");
						if (gSDataList != null)
						{
							switch (GetCurrentListingType())
							{
							default:
								foreach (GSData item in gSDataList)
								{
									UserReportData userReportData = UserReportData.CreateFromGSData(item);
									if (userReportData != null)
									{
										try
										{
											reportList.AddEntry(userReportData);
										}
										catch (Exception ex2)
										{
											Debug.LogError("Error adding entry: " + ex2.Message);
										}
									}
									else
									{
										Debug.LogError("Error parsing report data from response script data");
									}
								}
								break;
							case "mostReportedUsers":
							case "biggestSubmitters":
							case "mostReportedWithNew":
								foreach (GSData item2 in gSDataList)
								{
									MostReportData mostReportData = MostReportData.CreateFromGSData(item2);
									if (mostReportData != null)
									{
										try
										{
											reportList.AddEntry(mostReportData);
										}
										catch (Exception ex)
										{
											Debug.LogError("Error adding entry: " + ex.Message);
										}
									}
									else
									{
										Debug.LogError("Error parsing report data from response script data");
									}
								}
								break;
							}
							reportList.scrollArrowController.ResetScrolling();
						}
						else
						{
							Debug.LogError("No returned entries in script data");
						}
					}
				}
				else
				{
					Debug.LogError("Script data is null");
				}
			}
			else
			{
				Debug.LogError("Error while refreshing report list: " + query.Error);
			}
		});
		if (!currentPlayerId.NullOrEmpty())
		{
			reportList.RefreshBanInfo();
		}
	}

	public void RefreshView(bool resetPage)
	{
		if (resetPage)
		{
			CurrentReportList.SetPageData(0, 1);
		}
		OnClickRefresh();
	}

	public void OnShowDropdownValueChange()
	{
		CurrentReportList.SetDisplayMode(GetCurrentListingType());
		RefreshView(resetPage: true);
	}

	public void OnRPPDropdownValueChange()
	{
		RefreshView(resetPage: true);
	}

	public void OnClickHideAcknowledged(PickCursor pickCursor)
	{
		hideAcknowledged = !hideAcknowledged;
		hideAckCheckmark.enabled = hideAcknowledged;
		RefreshView(resetPage: false);
	}

	public void Update()
	{
		loadingSpinner.enabled = loadingOperation;
	}

	public void GoToUserProfile(string profileGSID, string profileDisplayName)
	{
		currentPlayerId = profileGSID;
		userReportList.SetDisplayMode("latestReports");
		userReportList.SetUserName(profileDisplayName);
		RefreshView(resetPage: true);
	}

	public void OnScrollPlus(PickCursor pickCursor)
	{
		if (!CurrentReportList.scrollArrowController.OnPickCursorScrollPlus(pickCursor))
		{
			CurrentReportList.reportContents.scrollArrowController.OnPickCursorScrollPlus(pickCursor);
		}
	}

	public void OnScrollMinus(PickCursor pickCursor)
	{
		if (!CurrentReportList.scrollArrowController.OnPickCursorScrollMinus(pickCursor))
		{
			CurrentReportList.reportContents.scrollArrowController.OnPickCursorScrollMinus(pickCursor);
		}
	}

	public bool OnBack()
	{
		if (!currentPlayerId.NullOrEmpty())
		{
			GoToUserProfile(null, null);
			return true;
		}
		return false;
	}

	public void BanCurrentUser(int hours, string reason, UnityAction<bool> onFinish)
	{
		if (!currentPlayerId.NullOrEmpty())
		{
			GameSparksQuery query = GameSparksManager.Instance.CreateQuery();
			query.SendSimpleRequest("BanUser", new Dictionary<string, object>
			{
				{ "playerID", currentPlayerId },
				{
					"durationSeconds",
					hours * 3600
				},
				{
					"permanent",
					(hours == 0) ? 1 : 0
				},
				{ "reason", reason }
			}, returnScriptData: true);
			GameSparksQuery gameSparksQuery = query;
			gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate
			{
				if (query.HasError)
				{
					Debug.LogError("Could not ban user: " + query.Error);
					onFinish(arg0: false);
				}
				else
				{
					onFinish(arg0: true);
				}
			});
		}
		else
		{
			Debug.LogError("Current player ID not set");
		}
	}

	public void UnbanCurrentUser(UnityAction<bool> onFinish)
	{
		if (!currentPlayerId.NullOrEmpty())
		{
			GameSparksQuery query = GameSparksManager.Instance.CreateQuery();
			query.SendSimpleRequest("UnbanUser", new Dictionary<string, object> { { "playerID", currentPlayerId } }, returnScriptData: true);
			GameSparksQuery gameSparksQuery = query;
			gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate
			{
				if (query.HasError)
				{
					Debug.LogError("Could not unban user: " + query.Error);
					onFinish(arg0: false);
				}
				else
				{
					onFinish(arg0: true);
				}
			});
		}
		else
		{
			Debug.LogError("Current player ID not set");
		}
	}
}
