using UnityEngine;
using UnityEngine.UI;

public class AdminLevelReportEntry : MonoBehaviour
{
	public Text reasonText;

	public string reason;

	public void Initialize(string reportType, string dateStr, string reason)
	{
		this.reason = reason;
		reasonText.text = dateStr + "\nType: " + reportType + "\nComment: " + reason;
	}

	public void OnClickCopyReason(PickCursor pickCursor)
	{
		QuickSaver.CopyStringToClipboard(reason);
		UserMessageManager.Instance.UserMessage("Copied comment to clipboard", 2f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: true);
	}
}
