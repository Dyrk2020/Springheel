using GameEvent;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class ReportUserDialog : MonoBehaviour
{
	public OnlinePlayerUISystem onlinePlayerUISystem;

	public PickableButton submitButton;

	public PickableButton cancelButton;

	public InputField commentInputField;

	public Text reportedUserName;

	public Image UCHNetIcon;

	public Image PSNVerifiedIcon;

	public DropdownMenu reasonDropdown;

	private InventoryPage page;

	private UserReports.ReportInformation reportInformation;

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnDestroy()
	{
		if (page != null)
		{
			page.imagesOnPage.Remove(UCHNetIcon);
			page.imagesOnPage.Remove(PSNVerifiedIcon);
		}
	}

	public void SetInventoryPage(InventoryPage page)
	{
		this.page = page;
		foreach (DropdownEntry dropdownEntry in reasonDropdown.dropdownEntries)
		{
			dropdownEntry.pageNumber = page.pageNumber;
		}
		page.imagesOnPage.Add(UCHNetIcon);
		page.imagesOnPage.Add(PSNVerifiedIcon);
	}

	public void Initialize(LobbyPlayer reporter, LobbyPlayer reportedUser)
	{
		NameTag.UpdateIcons(reportedUser, UCHNetIcon, PSNVerifiedIcon, usePlayerColor: false);
		reportedUserName.text = reportedUser.playerName;
		commentInputField.text = "";
		reasonDropdown.mainLabel.text = "";
		reasonDropdown.selectedDropdownEntry = null;
		reportInformation = new UserReports.ReportInformation
		{
			reporterUsername = reporter.playerName,
			reporterGSID = reporter.GSID,
			reporterPlatform = reporter.platform,
			reporterPlatformID = reporter.platformUniqueID,
			reportedUsername = reportedUser.playerName,
			reportedGSID = reportedUser.GSID,
			reportedPlatform = reportedUser.platform,
			reportedPlatformID = reportedUser.platformUniqueID,
			reportChatlog = ChatDisplay.GetChatLogAsString(),
			reportLevelCode = GameState.GetInstance().currentSnapshotInfo.snapshotCode
		};
	}

	public void ActivateCommentInputField(PickCursor cursor)
	{
		PickableButton.AllowOnlyButtons(submitButton, cancelButton);
		Controller.LockInputField(commentInputField, delegate
		{
			PickableButton.ResetMasks();
		});
	}

	public void OnClickSubmit()
	{
		if (reasonDropdown.selectedDropdownEntry == null)
		{
			UserMessageManager.Instance.UserMessage(LocalizationManager.GetTranslation("Network/ReportPlayerSelectReason"), 3f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: true);
			return;
		}
		reportInformation.reportReason = (UserReports.ReportReason)reasonDropdown.selectedDropdownEntry.EntryValue;
		reportInformation.reportComments = commentInputField.text;
		GameSparksManager.Instance.CreateQuery().SubmitUserReport(reportInformation);
		UserReports.NotifyReportedUser(reportInformation.reportedGSID);
		onlinePlayerUISystem.HideReportDialog();
		UserMessageManager.Instance.UserMessage(LocalizationManager.GetTranslation("Network/ReportSubmitted"), 3f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: true);
	}

	public void OnClickCancel()
	{
		onlinePlayerUISystem.HideReportDialog();
	}

	public void OnDialogClosed()
	{
		GameEventManager.SendEvent(new PickCursorClickedBackgroundEvent());
	}
}
