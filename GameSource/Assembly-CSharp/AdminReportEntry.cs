using System;
using UnityEngine;
using UnityEngine.UI;

public class AdminReportEntry : MonoBehaviour
{
	public AdminReportList reportList;

	public AdminUserReportsDialog.UserReportData reportData;

	public Text dateText;

	public Text reporterText;

	public Text reportedText;

	public Text typeText;

	public Image bgImage;

	public Color bgColorNormal;

	public Color bgColorSelected;

	public Color fontColorNormal;

	public Color fontColorAcknowledged;

	private bool colorAck;

	public void Initialize(AdminReportList reportList, AdminUserReportsDialog.UserReportData reportData)
	{
		this.reportList = reportList;
		this.reportData = reportData;
		bgImage.color = bgColorNormal;
		colorAck = reportData.acknowledged == 1;
		dateText.color = (colorAck ? fontColorAcknowledged : fontColorNormal);
		reporterText.color = (colorAck ? fontColorAcknowledged : fontColorNormal);
		reportedText.color = (colorAck ? fontColorAcknowledged : fontColorNormal);
		typeText.color = (colorAck ? fontColorAcknowledged : fontColorNormal);
		dateText.text = AdminPanelDialog.DateToStr(new DateTime(reportData.timestamp));
		reporterText.text = reportData.reporterDisplayName;
		reportedText.text = reportData.reportedDisplayName;
		typeText.text = AdminUserReportsDialog.GetReportReasonString(reportData.reportReason);
	}

	public void OnClick(PickCursor pickCursor)
	{
		reportList.OnClickEntry(pickCursor, this);
	}

	public void OnSelect()
	{
		bgImage.color = bgColorSelected;
	}

	public void OnDeselect()
	{
		bgImage.color = bgColorNormal;
	}

	private void Update()
	{
		bool flag = reportData.acknowledged == 1;
		if (flag != colorAck)
		{
			colorAck = flag;
			dateText.color = (colorAck ? fontColorAcknowledged : fontColorNormal);
			reporterText.color = (colorAck ? fontColorAcknowledged : fontColorNormal);
			reportedText.color = (colorAck ? fontColorAcknowledged : fontColorNormal);
			typeText.color = (colorAck ? fontColorAcknowledged : fontColorNormal);
		}
	}
}
