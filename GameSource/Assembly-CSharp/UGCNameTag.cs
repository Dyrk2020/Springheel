using System.Collections.Generic;
using GameSparks.Core;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class UGCNameTag : MonoBehaviour
{
	public Image UCHNetIcon;

	public Image PSNVerifiedIcon;

	public Text usernameText;

	public BoxCollider2D boxCollider;

	public bool isClickable = true;

	public string playerNameUncensored;

	public LobbyPlayer.SocialPlatform platform;

	public string platformID;

	public string GSID;

	public string GSID_old;

	public bool hasVerifiedAccount;

	public bool waitingForVerifiedAccountResult;

	public bool receivedVerifiedAccountResult;

	public bool isAnonymous;

	public bool neverAnonymous;

	private int refreshColliderSizeIn = 3;

	private void Start()
	{
	}

	private void Update()
	{
		if ((!isClickable || isAnonymous) && boxCollider != null && boxCollider.enabled)
		{
			boxCollider.enabled = false;
			GetComponent<PickableButton>().PickColliders = new Collider2D[0];
		}
	}

	private void LateUpdate()
	{
		if (refreshColliderSizeIn > 0)
		{
			refreshColliderSizeIn--;
			if (refreshColliderSizeIn == 0 && boxCollider != null)
			{
				boxCollider.size = GetComponent<RectTransform>().sizeDelta;
			}
		}
	}

	public void Initialize(UndergroundComputer.FeaturedLevelData featuredLevelData, bool isAnonymous)
	{
		Initialize(featuredLevelData.authorName, GetPlatformIDFromFeaturedLevelData(featuredLevelData), featuredLevelData.authorId, GetPlatformFromFeaturedLevelData(featuredLevelData), isAnonymous);
		GSID_old = featuredLevelData.authorId_old;
	}

	public void Initialize(string username, string platformID, string GSID, LobbyPlayer.SocialPlatform platform, bool isAnonymous)
	{
		receivedVerifiedAccountResult = false;
		hasVerifiedAccount = false;
		waitingForVerifiedAccountResult = false;
		this.isAnonymous = isAnonymous;
		playerNameUncensored = username;
		this.platformID = platformID;
		this.platform = platform;
		this.GSID = GSID;
		if (!neverAnonymous && (GSID.NullOrEmpty() || isAnonymous))
		{
			usernameText.text = LocalizationManager.GetTranslation("UndergroundComputer/Stats/Anonymous");
		}
		else
		{
			usernameText.text = username;
		}
		UpdateIcons();
		PickableButton component = GetComponent<PickableButton>();
		if (component != null)
		{
			component.Enable();
		}
	}

	public void Clear()
	{
		Initialize("", "", "", LobbyPlayer.SocialPlatform.Undefined, isAnonymous: false);
	}

	public void UpdateIcons()
	{
		if (LobbyPlayer.LocalMachinePlatform == LobbyPlayer.SocialPlatform.PSN && !playerNameUncensored.NullOrEmpty())
		{
			if (UCHNetIcon != null && platform != LobbyPlayer.SocialPlatform.PSN && platform != LobbyPlayer.SocialPlatform.Undefined)
			{
				UCHNetIcon.gameObject.SetActive(value: true);
				UCHNetIcon.enabled = true;
			}
			else if (UCHNetIcon != null)
			{
				UCHNetIcon.gameObject.SetActive(value: false);
			}
			if (PSNVerifiedIcon != null && platform == LobbyPlayer.SocialPlatform.PSN)
			{
				if (receivedVerifiedAccountResult)
				{
					PSNVerifiedIcon.gameObject.SetActive(hasVerifiedAccount);
					PSNVerifiedIcon.enabled = hasVerifiedAccount;
				}
			}
			else if (PSNVerifiedIcon != null)
			{
				PSNVerifiedIcon.gameObject.SetActive(value: false);
			}
		}
		else
		{
			waitingForVerifiedAccountResult = false;
			receivedVerifiedAccountResult = true;
			hasVerifiedAccount = false;
			if (UCHNetIcon != null)
			{
				UCHNetIcon.gameObject.SetActive(value: false);
			}
			if (PSNVerifiedIcon != null)
			{
				PSNVerifiedIcon.gameObject.SetActive(value: false);
			}
		}
		refreshColliderSizeIn = 3;
		bool flag = GSID.NullOrEmpty();
		PickableButton component = GetComponent<PickableButton>();
		if (component != null)
		{
			component.enabled = !flag;
		}
		if (boxCollider != null)
		{
			boxCollider.enabled = !flag;
		}
	}

	public void OnClick(UndergroundComputer undergroundComputer)
	{
		undergroundComputer.PopupNameOptions(new List<UserInfoPopup.UserInfo>
		{
			new UserInfoPopup.UserInfo
			{
				username = playerNameUncensored,
				GSID = GSID,
				GSID_old = GSID_old,
				platform = platform,
				platformID = platformID,
				shouldBeAnonymous = isAnonymous
			}
		});
	}

	public static string GetPlatformIDFromFeaturedLevelData(UndergroundComputer.FeaturedLevelData featuredLevelData)
	{
		string result = null;
		if (featuredLevelData.authorPlatformIds == null)
		{
			return null;
		}
		if (featuredLevelData.authorPlatformIds.ContainsKey("ST"))
		{
			result = featuredLevelData.authorPlatformIds["ST"];
		}
		return result;
	}

	public static string GetPlatformIDFromGSData(GSData data)
	{
		string text = null;
		if (data.ContainsKey("ST"))
		{
			return data.GetString("ST");
		}
		if (data.ContainsKey("XB"))
		{
			return data.GetString("XB");
		}
		if (data.ContainsKey("PSN"))
		{
			return data.GetString("PSN");
		}
		if (data.ContainsKey("PSID"))
		{
			return data.GetString("PSID");
		}
		if (data.ContainsKey("SWITCH"))
		{
			return data.GetString("SWITCH");
		}
		if (data.ContainsKey("NX"))
		{
			return data.GetString("NX");
		}
		if (data.ContainsKey("EA"))
		{
			return data.GetString("EA");
		}
		return null;
	}

	public static LobbyPlayer.SocialPlatform GetPlatformFromFeaturedLevelData(UndergroundComputer.FeaturedLevelData featuredLevelData)
	{
		if (featuredLevelData != null && featuredLevelData.authorPlatformIds != null)
		{
			foreach (KeyValuePair<string, string> authorPlatformId in featuredLevelData.authorPlatformIds)
			{
				LobbyPlayer.SocialPlatform platformFromString = GetPlatformFromString(authorPlatformId.Key);
				if (platformFromString != LobbyPlayer.SocialPlatform.Undefined)
				{
					return platformFromString;
				}
				Debug.LogError("Unknown backend platform: " + authorPlatformId.Key);
			}
		}
		return LobbyPlayer.SocialPlatform.Undefined;
	}

	public static LobbyPlayer.SocialPlatform GetPlatformFromGSData(GSData data)
	{
		if (data != null)
		{
			if (data.ContainsKey("ST"))
			{
				return LobbyPlayer.SocialPlatform.Steam;
			}
			if (data.ContainsKey("XB"))
			{
				return LobbyPlayer.SocialPlatform.XboxLive;
			}
			if (data.ContainsKey("PSN") || data.ContainsKey("PSID"))
			{
				return LobbyPlayer.SocialPlatform.PSN;
			}
			if (data.ContainsKey("SWITCH") || data.ContainsKey("NX"))
			{
				return LobbyPlayer.SocialPlatform.Nintendo;
			}
			if (data.ContainsKey("EA"))
			{
				return LobbyPlayer.SocialPlatform.Origin;
			}
		}
		return LobbyPlayer.SocialPlatform.Undefined;
	}

	public static LobbyPlayer.SocialPlatform GetPlatformFromString(string gameSparksPlatformString)
	{
		switch (gameSparksPlatformString)
		{
		case "ST":
			return LobbyPlayer.SocialPlatform.Steam;
		case "XB":
			return LobbyPlayer.SocialPlatform.XboxLive;
		case "PS":
		case "PSID":
		case "PSN":
			return LobbyPlayer.SocialPlatform.PSN;
		case "SWITCH":
		case "NX":
			return LobbyPlayer.SocialPlatform.Nintendo;
		case "EA":
			return LobbyPlayer.SocialPlatform.Origin;
		default:
			return LobbyPlayer.SocialPlatform.Undefined;
		}
	}

	public static string GetGSPlatformStringFromPlatform(LobbyPlayer.SocialPlatform platform)
	{
		return platform switch
		{
			LobbyPlayer.SocialPlatform.PSN => "PSN", 
			LobbyPlayer.SocialPlatform.Steam => "ST", 
			LobbyPlayer.SocialPlatform.XboxLive => "XB", 
			LobbyPlayer.SocialPlatform.Nintendo => "SWITCH", 
			LobbyPlayer.SocialPlatform.Origin => "EA", 
			_ => null, 
		};
	}

	public void SetColor(Color color)
	{
		UCHNetIcon.color = color;
		usernameText.color = color;
	}

	public void InitializeAsync(UndergroundComputer.FeaturedLevelData featuredLevelData)
	{
		Initialize(featuredLevelData, featuredLevelData.authorId.NullOrEmpty());
		GSID_old = featuredLevelData.authorId_old;
	}

	public void InitializeAsync(string username, string platformID, string GSID, LobbyPlayer.SocialPlatform platform)
	{
		Initialize(username, platformID, GSID, platform, GSID.NullOrEmpty());
	}
}
