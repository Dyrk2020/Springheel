using System;
using UnityEngine;
using UnityEngine.UI;

public class AdminMostReportedEntry : MonoBehaviour
{
	public AdminReportList reportList;

	public AdminUserReportsDialog.MostReportData reportData;

	public Text playerNameText;

	public Text reportCountText;

	public Text lastReportDateText;

	public Image bgImage;

	public Color bgColorNormal;

	public Color bgColorSelected;

	public void Initialize(AdminReportList reportList, AdminUserReportsDialog.MostReportData reportData)
	{
		this.reportList = reportList;
		this.reportData = reportData;
		bgImage.color = bgColorNormal;
		playerNameText.text = reportData.playerDisplayName;
		if (reportData.acknowledgedReportCount == 0)
		{
			reportCountText.text = reportData.reportCount.ToString();
		}
		else
		{
			reportCountText.text = reportData.reportCount - reportData.acknowledgedReportCount + "/" + reportData.reportCount;
		}
		lastReportDateText.text = AdminPanelDialog.DateToStr(new DateTime(reportData.latestTimestamp));
	}

	public void OnClick(PickCursor pickCursor)
	{
		reportList.OnClickEntry(pickCursor, this);
	}
}
