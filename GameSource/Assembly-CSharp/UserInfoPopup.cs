using System.Collections.Generic;
using GameEvent;
using GameSparks.Core;
using Steamworks;
using UnityEngine;

public class UserInfoPopup : MonoBehaviour, IGameEventListener
{
	public class UserInfo
	{
		public string username;

		public string GSID;

		public string GSID_old;

		public string platformID;

		public LobbyPlayer.SocialPlatform platform;

		public bool shouldBeAnonymous;
	}

	public Object infoPopupEntryPrefab;

	private List<PickableButton> addedButtons = new List<PickableButton>();

	private bool entryClicked;

	private UndergroundComputer undergroundComputer;

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<PickCursorClickedBackgroundEvent>(this, adding);
		GameEventManager.ChangeListener<NoteBookDisplayEvent>(this, adding);
		GameEventManager.ChangeListener<ScoreboardEvent>(this, adding);
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		Object.Destroy(base.gameObject);
	}

	private void Start()
	{
		ChangeListener(adding: true);
	}

	public void Show(List<UserInfo> users, UndergroundComputer undergroundComputer)
	{
		this.undergroundComputer = undergroundComputer;
		int num = 0;
		HashSet<string> hashSet = new HashSet<string>();
		foreach (UserInfo user in users)
		{
			if (!hashSet.Contains(user.GSID))
			{
				num += AddEntriesForUser(user);
				hashSet.Add(user.GSID);
			}
		}
		if (num > 0)
		{
			PickableButton.AllowOnlyButtons(addedButtons.ToArray());
			return;
		}
		entryClicked = true;
		Object.Destroy(base.gameObject);
		Debug.Log("No entries inserted");
	}

	private void OnDestroy()
	{
		ChangeListener(adding: false);
		if (!entryClicked)
		{
			PickableButton.ResetMasks();
		}
	}

	private int AddEntriesForUser(UserInfo user)
	{
		int num = 0;
		if (user.platform == LobbyPlayer.LocalMachinePlatform && user.platform != LobbyPlayer.SocialPlatform.Nintendo && !user.shouldBeAnonymous)
		{
			UserInfoPopupEntry userInfoPopupEntry = base.gameObject.AddPrefabAsChild<UserInfoPopupEntry>(infoPopupEntryPrefab);
			userInfoPopupEntry.Initialize(this, UserInfoPopupEntry.EntryType.ShowProfileFor, user);
			addedButtons.Add(userInfoPopupEntry.GetComponent<PickableButton>());
			num++;
		}
		if (undergroundComputer != null)
		{
			UserInfoPopupEntry userInfoPopupEntry2 = base.gameObject.AddPrefabAsChild<UserInfoPopupEntry>(infoPopupEntryPrefab);
			userInfoPopupEntry2.Initialize(this, UserInfoPopupEntry.EntryType.ShowLevelsBy, user);
			addedButtons.Add(userInfoPopupEntry2.GetComponent<PickableButton>());
			num++;
		}
		return num;
	}

	public void OnClickEntry(int playerLocalNumber, UserInfoPopupEntry entry)
	{
		PickableButton.ResetMasks();
		switch (entry.entryType)
		{
		case UserInfoPopupEntry.EntryType.ShowLevelsBy:
			if (undergroundComputer != null)
			{
				ShowLevelsForUser(undergroundComputer, entry.userInfo);
			}
			break;
		case UserInfoPopupEntry.EntryType.ShowProfileFor:
			OpenProfileForUser(playerLocalNumber, entry.userInfo);
			break;
		}
		entryClicked = true;
		Object.Destroy(base.gameObject);
	}

	public static void OpenProfileForUser(int playerLocalNumber, UserInfo userInfo)
	{
		if (SteamManager.Initialized && userInfo.platform == LobbyPlayer.SocialPlatform.Steam && ulong.TryParse(userInfo.platformID, out var result))
		{
			SteamFriends.ActivateGameOverlayToUser("steamid", new CSteamID(result));
		}
	}

	public static void ShowLevelsForUser(UndergroundComputer undergroundComputer, UserInfo userInfo, int startPage = 0)
	{
		FeaturedQuickFilter.SortingFilter filter = new FeaturedQuickFilter.SortingFilter
		{
			filterType = FeaturedQuickFilter.FilterTypes.Sorted,
			sortBy = "date",
			restrictToUserId = userInfo.GSID,
			restrictToGSID = userInfo.GSID_old
		};
		undergroundComputer.ShowLevelsByPlayer(userInfo.username, userInfo.platformID, filter, userInfo.platform, userInfo.shouldBeAnonymous, isMe: false, startPage);
	}

	public static List<UserInfo> GetUserListFromChallengeTimeData(ChallengeScoreboard.ChallengeTimeData data)
	{
		List<UserInfo> list = new List<UserInfo>(data.playerIds.Count);
		if (data.platformIds.Count < data.playerIds.Count)
		{
			Debug.LogError("Fewer platforms returned than players: " + data.platformIds.Count + " < " + data.playerIds.Count);
		}
		for (int i = 0; i < data.playerIds.Count; i++)
		{
			UserInfo userInfo = null;
			userInfo = ((i >= data.platformIds.Count) ? new UserInfo
			{
				username = data.playerNames[i],
				GSID = data.playerIds[i],
				platform = LobbyPlayer.SocialPlatform.Undefined,
				platformID = null
			} : new UserInfo
			{
				username = data.playerNames[i],
				GSID = data.playerIds[i],
				platform = UGCNameTag.GetPlatformFromGSData(data.platformIds[i]),
				platformID = UGCNameTag.GetPlatformIDFromGSData(data.platformIds[i])
			});
			list.Add(userInfo);
		}
		return list;
	}

	public static List<UserInfo> GetUserList(List<string> playerNames, List<string> playerIds, List<GSData> platformIds)
	{
		if (playerNames.Count != playerIds.Count || playerNames.Count != platformIds.Count)
		{
			Debug.LogError("Lists have different lengths!");
			return null;
		}
		List<UserInfo> list = new List<UserInfo>(playerIds.Count);
		for (int i = 0; i < playerIds.Count; i++)
		{
			list.Add(new UserInfo
			{
				username = playerNames[i],
				GSID = playerIds[i],
				platform = UGCNameTag.GetPlatformFromGSData(platformIds[i]),
				platformID = UGCNameTag.GetPlatformIDFromGSData(platformIds[i])
			});
		}
		return list;
	}
}
