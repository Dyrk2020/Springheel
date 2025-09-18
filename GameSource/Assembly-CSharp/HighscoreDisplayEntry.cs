using System.Collections.Generic;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class HighscoreDisplayEntry : MonoBehaviour
{
	public Text rankText;

	public RectTransform nameBox;

	public Text timeText;

	public Object userInfoPopupPrefab;

	public Object ugcNameTagPrefab;

	private List<UserInfoPopup.UserInfo> users;

	private int refreshColliderSizeIn = 3;

	private bool hasUsersFromPlatform;

	private bool shownInComputer;

	public static string GetTimeString(float time)
	{
		if (time == 0f)
		{
			return "--:--.--";
		}
		time = Mathf.Round(time * 100f) / 100f;
		float num = time % 1f;
		string text = $"{num:.00}";
		int num2 = Mathf.FloorToInt(time);
		int num3 = num2 / 60;
		num2 %= 60;
		return num3.ToString("D2") + ":" + num2.ToString("D2") + text;
	}

	public void Initialize(int rank, string time, Color textColor, List<UserInfoPopup.UserInfo> users, bool shownInComputer)
	{
		this.users = users;
		this.shownInComputer = shownInComputer;
		if (rank == 0)
		{
			rankText.gameObject.SetActive(value: false);
		}
		else
		{
			rankText.text = rank + ".";
		}
		rankText.color = textColor;
		if (!time.NullOrEmpty())
		{
			timeText.text = time;
			timeText.color = textColor;
		}
		else
		{
			timeText.gameObject.SetActive(value: false);
		}
		nameBox.transform.DestroyAllChildren();
		if (users != null)
		{
			List<UserInfoPopup.UserInfo> list = new List<UserInfoPopup.UserInfo>();
			Dictionary<string, int> dictionary = new Dictionary<string, int>();
			foreach (UserInfoPopup.UserInfo user in users)
			{
				if (dictionary.ContainsKey(user.GSID))
				{
					dictionary[user.GSID]++;
					continue;
				}
				dictionary.Add(user.GSID, 1);
				list.Add(user);
			}
			hasUsersFromPlatform = false;
			for (int i = 0; i < list.Count; i++)
			{
				UserInfoPopup.UserInfo userInfo = list[i];
				if (userInfo.platform == LobbyPlayer.LocalMachinePlatform && !userInfo.shouldBeAnonymous)
				{
					hasUsersFromPlatform = true;
				}
				UGCNameTag uGCNameTag = nameBox.gameObject.AddPrefabAsChild<UGCNameTag>(ugcNameTagPrefab);
				string username = (userInfo.shouldBeAnonymous ? LocalizationManager.GetTranslation("UndergroundComputer/Stats/Anonymous") : userInfo.username);
				uGCNameTag.Initialize(username, userInfo.platformID, userInfo.GSID, userInfo.platform, userInfo.shouldBeAnonymous);
				uGCNameTag.isClickable = false;
				uGCNameTag.SetColor(textColor);
				string text = "";
				if (dictionary[userInfo.GSID] > 1)
				{
					text = text + " (" + dictionary[userInfo.GSID] + ")";
				}
				if (list.Count > 1 && i < list.Count - 1)
				{
					text += ", ";
				}
				if (!text.NullOrEmpty())
				{
					InsertTextInNameBox(text, textColor);
				}
			}
		}
		else
		{
			InsertTextInNameBox("---", textColor);
		}
		refreshColliderSizeIn = 3;
	}

	private void InsertTextInNameBox(string str, Color textColor)
	{
		Text text = Object.Instantiate((ugcNameTagPrefab as GameObject).GetComponent<UGCNameTag>().usernameText);
		text.transform.SetParent(nameBox.transform, worldPositionStays: false);
		text.text = str;
		text.color = textColor;
	}

	public void OnClick()
	{
		if (users != null)
		{
			UndergroundComputer componentInParent = GetComponentInParent<UndergroundComputer>();
			if (componentInParent != null)
			{
				componentInParent.PopupNameOptions(users);
				return;
			}
			ChallengeScoreboard componentInParent2 = GetComponentInParent<ChallengeScoreboard>();
			if (componentInParent2 != null)
			{
				componentInParent2.PopupNameOptions(users);
			}
			else
			{
				Debug.LogError("ERROR: No underground computer or challenge scoreboard found!");
			}
		}
		else
		{
			Debug.LogError("HighScoreDisplayEntry.OnClick -> users is null");
		}
	}

	private void LateUpdate()
	{
		if (refreshColliderSizeIn <= 0)
		{
			return;
		}
		refreshColliderSizeIn--;
		if (refreshColliderSizeIn != 0)
		{
			return;
		}
		if (users != null && (shownInComputer || hasUsersFromPlatform))
		{
			GenericButton component = GetComponent<GenericButton>();
			if (component != null)
			{
				component.enabled = true;
				component.Enable();
				GetComponent<BoxCollider2D>().size = GetComponent<RectTransform>().sizeDelta;
			}
			else
			{
				Debug.LogWarning("List of users was supplied to HighScoreDisplayEntry.Initialize but no button was found on this high score entry!!");
			}
		}
		else
		{
			GenericButton component2 = GetComponent<GenericButton>();
			if (component2 != null)
			{
				component2.enabled = false;
				GetComponent<BoxCollider2D>().enabled = false;
			}
		}
		GetComponent<BoxCollider2D>().size = GetComponent<RectTransform>().sizeDelta;
	}
}
