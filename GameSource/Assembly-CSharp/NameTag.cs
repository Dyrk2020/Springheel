using UnityEngine;
using UnityEngine.UI;

public class NameTag : MonoBehaviour
{
	public Text nameBox;

	public CanvasGroup nameCanvasGroup;

	public Canvas canvas;

	public Image UCHNetIcon;

	public Image PSNVerifiedIcon;

	private float lastAlpha = -1f;

	public float currentAlpha = 1f;

	public float maxOpacity = 1f;

	public string Currentname => nameBox.text;

	private void Awake()
	{
		canvas = GetComponent<Canvas>();
		nameBox.text = "";
		UCHNetIcon.gameObject.SetActive(value: false);
		PSNVerifiedIcon.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		currentAlpha = Mathf.Min(currentAlpha, maxOpacity);
		if (lastAlpha != currentAlpha)
		{
			lastAlpha = currentAlpha;
			nameCanvasGroup.alpha = lastAlpha;
		}
	}

	public void UpdateIcons(LobbyPlayer lp)
	{
		UpdateIcons(lp, UCHNetIcon, PSNVerifiedIcon, usePlayerColor: true);
	}

	public static void UpdateIcons(LobbyPlayer lp, Image UCHNetIcon, Image PSNVerifiedIcon, bool usePlayerColor)
	{
		if (LobbyPlayer.LocalMachinePlatform == LobbyPlayer.SocialPlatform.PSN && lp != null)
		{
			if (UCHNetIcon != null && lp.platform != LobbyPlayer.SocialPlatform.PSN && lp.platform != LobbyPlayer.SocialPlatform.Undefined)
			{
				UCHNetIcon.gameObject.SetActive(value: true);
				UCHNetIcon.enabled = true;
				if (usePlayerColor)
				{
					UCHNetIcon.color = lp.PlayerColor;
				}
			}
			else
			{
				UCHNetIcon.gameObject.SetActive(value: false);
			}
			if (PSNVerifiedIcon != null && lp.platform == LobbyPlayer.SocialPlatform.PSN)
			{
				PSNVerifiedIcon.gameObject.SetActive(lp.hasVerifiedSocialAccount);
				PSNVerifiedIcon.enabled = lp.hasVerifiedSocialAccount;
			}
			else
			{
				PSNVerifiedIcon.gameObject.SetActive(value: false);
			}
		}
		else
		{
			UCHNetIcon.gameObject.SetActive(value: false);
			PSNVerifiedIcon.gameObject.SetActive(value: false);
		}
	}

	public void setNameBoxText(string newName, Character chr)
	{
		if (chr.AssociatedLobbyPlayer != null)
		{
			nameBox.color = chr.AssociatedLobbyPlayer.PlayerColor;
		}
		if (chr.AssociatedGamePlayer != null)
		{
			nameBox.color = chr.AssociatedGamePlayer.PlayerColor;
		}
		nameBox.text = newName;
	}

	public void setNameBoxText(string newName, Cursor cursor)
	{
		if (cursor.AssociatedLobbyPlayer != null)
		{
			nameBox.color = cursor.AssociatedLobbyPlayer.PlayerColor;
		}
		if (cursor.AssociatedGamePlayer != null)
		{
			nameBox.color = cursor.AssociatedGamePlayer.PlayerColor;
		}
		nameBox.text = newName;
	}

	public void MatchLayerOrder(SpriteRenderer sr)
	{
		canvas.sortingOrder = sr.sortingOrder + 1;
		canvas.sortingLayerID = sr.sortingLayerID;
	}
}
