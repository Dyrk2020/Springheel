using System;
using System.Collections.Generic;
using GameEvent;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ViewReportsDialog : MonoBehaviour, IGameEventListener
{
	public class ReportData
	{
		public int reportReason;

		public string reportComment;

		public long timestamp;
	}

	public UndergroundComputer undergroundComputer;

	public Transform reportArea;

	public Text levelNameText;

	public Text loadingDataText;

	public Text levelReportsText;

	public ScrollArrowController scrollArrowContainer;

	public Transform breakdownGrid;

	public UnityEngine.Object breakdownItemPrefab;

	public Transform commentList;

	public UnityEngine.Object commentPrefab;

	private Dictionary<int, string> reportReasonDict = new Dictionary<int, string>
	{
		{ 0, "Offensive/Inappropriate Title" },
		{ 1, "Offensive/Inappropriate Content" },
		{ 2, "Spam" },
		{ 3, "Personal Information" },
		{ 4, "Copyright Infringement" },
		{ 5, "Broken Level" },
		{ 6, "Other" }
	};

	private void Awake()
	{
		ChangeListener(adding: true);
		HideAllPanes();
	}

	private void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<PlayerInGameRuleEvent>(this, adding);
	}

	public void DisplayLevelData(string code, string snapshotName)
	{
		levelNameText.text = snapshotName + " (" + GameSparksQuery.GetFormattedSnapshotCode(code) + ")";
		PickableButton.maskAll = true;
		loadingDataText.gameObject.SetActive(value: true);
		reportArea.gameObject.SetActive(value: true);
		levelReportsText.text = "";
		GameSparksQuery query = GameSparksManager.Instance.CreateQuery();
		query.GetLevelReports(code);
		GameSparksQuery gameSparksQuery = query;
		gameSparksQuery.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(gameSparksQuery.FinishListeners, (UnityAction<GameSparksQuery>)delegate
		{
			PickableButton.ResetMasks();
			loadingDataText.gameObject.SetActive(value: false);
			if (query.HasError)
			{
				Debug.LogError(query.Error);
			}
			else
			{
				long timestamp = (long)query.ResultData["date"];
				List<ReportData> reportDataList = query.ResultData["reports"] as List<ReportData>;
				DisplayResultData(reportDataList, timestamp);
			}
		});
	}

	private void DisplayResultData(List<ReportData> reportDataList, long timestamp)
	{
		scrollArrowContainer.ResetScrolling();
		if (reportDataList.Count <= 0)
		{
			return;
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		Dictionary<int, List<ReportData>> dictionary2 = new Dictionary<int, List<ReportData>>();
		foreach (ReportData reportData in reportDataList)
		{
			if (!dictionary.ContainsKey(reportData.reportReason))
			{
				dictionary.Add(reportData.reportReason, 1);
			}
			else
			{
				dictionary[reportData.reportReason]++;
			}
			if (!dictionary2.ContainsKey(reportData.reportReason))
			{
				dictionary2.Add(reportData.reportReason, new List<ReportData>());
			}
			dictionary2[reportData.reportReason].Add(reportData);
		}
		breakdownGrid.DestroyAllChildren();
		foreach (KeyValuePair<int, int> item in dictionary)
		{
			string reason = reportReasonDict[item.Key];
			breakdownGrid.gameObject.AddPrefabAsChild<AdminReportBreakdownItem>(breakdownItemPrefab).Initialize(reason, item.Value);
		}
		List<ReportData> list = new List<ReportData>();
		list.AddRange(reportDataList);
		list.Sort((ReportData a, ReportData b) => b.timestamp.CompareTo(a.timestamp));
		commentList.DestroyAllChildren();
		foreach (ReportData item2 in list)
		{
			if (!item2.reportComment.NullOrEmpty())
			{
				string dateStr = UndergroundComputer.TimeToString((timestamp - item2.timestamp) / 1000);
				string reportType = reportReasonDict[item2.reportReason];
				commentList.gameObject.AddPrefabAsChild<AdminLevelReportEntry>(commentPrefab).Initialize(reportType, dateStr, item2.reportComment);
			}
		}
	}

	private void HideAllPanes()
	{
		loadingDataText.gameObject.SetActive(value: false);
		reportArea.gameObject.SetActive(value: false);
	}

	public void OnClose()
	{
		HideAllPanes();
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		if (e.GetType() == typeof(PlayerInGameRuleEvent) && (e as PlayerInGameRuleEvent).Entered && PickableBuildButton.buildMenuCurrentState == PickableBuildButton.BuildScreenStates.ViewReportsDialog)
		{
			if (PickableBuildButton.SelectedEntry != null)
			{
				SnapshotEntry component = PickableBuildButton.SelectedEntry.GetComponent<SnapshotEntry>();
				DisplayLevelData(component.Code, component.SnapshotName);
			}
			else
			{
				undergroundComputer.CloseViewReportsDialog();
			}
		}
	}

	private void Update()
	{
	}
}
