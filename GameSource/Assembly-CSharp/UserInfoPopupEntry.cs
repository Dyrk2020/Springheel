using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class UserInfoPopupEntry : MonoBehaviour
{
	public enum EntryType
	{
		ShowProfileFor,
		ShowLevelsBy
	}

	public Text headerText;

	public UGCNameTag nameTag;

	private UserInfoPopup userInfoPopup;

	public UserInfoPopup.UserInfo userInfo;

	public EntryType entryType;

	private int refreshColliderSizeIn = 3;

	public void Initialize(UserInfoPopup userInfoPopup, EntryType entryType, UserInfoPopup.UserInfo userInfo)
	{
		this.userInfoPopup = userInfoPopup;
		this.entryType = entryType;
		this.userInfo = userInfo;
		switch (entryType)
		{
		case EntryType.ShowLevelsBy:
			headerText.text = LocalizationManager.GetTranslation("Snapshot/UserInfo/ShowLevelsBy");
			break;
		case EntryType.ShowProfileFor:
			headerText.text = LocalizationManager.GetTranslation("Snapshot/UserInfo/ShowProfileFor");
			break;
		}
		nameTag.Initialize(userInfo.username, userInfo.platformID, userInfo.GSID, userInfo.platform, userInfo.shouldBeAnonymous);
		nameTag.GSID_old = userInfo.GSID_old;
		refreshColliderSizeIn = 3;
	}

	public void OnClickWithCursor(PickCursor pickCursor)
	{
		userInfoPopup.OnClickEntry(pickCursor.localNumber, this);
	}

	private void LateUpdate()
	{
		if (refreshColliderSizeIn > 0)
		{
			refreshColliderSizeIn--;
			if (refreshColliderSizeIn == 0)
			{
				GetComponent<BoxCollider2D>().size = GetComponent<RectTransform>().sizeDelta;
			}
		}
	}
}
