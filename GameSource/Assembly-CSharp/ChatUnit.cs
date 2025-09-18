using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class ChatUnit : MonoBehaviour
{
	public Image chatPortrait;

	public UGCNameTag nameTag;

	public Text colonText;

	public Text chatText;

	public void SetChatUnitMessage(ChatMessageDetails chatMessage)
	{
		if (chatMessage.UserName.NullOrEmpty() || !chatMessage.isChatMessage)
		{
			GetComponent<HorizontalLayoutGroup>().padding.left = 0;
			chatPortrait.gameObject.SetActive(value: false);
		}
		else
		{
			chatPortrait.sprite = CharacterSpriteManager.GetInstance().GetCharaterPortrait(chatMessage.Animal);
			if (chatMessage.Animal == Character.Animals.NONE)
			{
				chatPortrait.color = chatMessage.UserNameColor;
			}
			chatPortrait.rectTransform.sizeDelta = new Vector2(GameSettings.GetInstance().ChatMessageFontSize * 2, GameSettings.GetInstance().ChatMessageFontSize * 2);
		}
		if (chatMessage.Message != null && chatMessage.Message.Length > GameSettings.GetInstance().maxCharactersPerMessage)
		{
			chatMessage.Message = chatMessage.Message.Substring(0, GameSettings.GetInstance().maxCharactersPerMessage);
		}
		string text;
		if ((chatMessage.Message != null && chatMessage.EmoteType == EmoteMeanings.CHAT_Text) || chatMessage.EmoteType == EmoteMeanings.EMOTE_Explitive)
		{
			text = chatMessage.Message;
		}
		else
		{
			if (chatMessage.Message == null)
			{
				return;
			}
			text = EmoteSystem.EmoteConverter(chatMessage.EmoteType);
		}
		if (chatMessage.UserName.NullOrEmpty())
		{
			nameTag.gameObject.SetActive(value: false);
			colonText.gameObject.SetActive(value: false);
			chatText.text = text;
			chatText.color = chatMessage.UserNameColor;
		}
		else
		{
			LobbyPlayer lobbyPlayer = LobbyManager.instance.GetLobbyPlayer(chatMessage.NetworkNumber);
			if (lobbyPlayer != null)
			{
				nameTag.Initialize(chatMessage.UserName, lobbyPlayer.platformUniqueID, lobbyPlayer.GSID, lobbyPlayer.platform, isAnonymous: false);
			}
			else
			{
				nameTag.Initialize(chatMessage.UserName, chatMessage.platformID, chatMessage.GSID, chatMessage.platform, isAnonymous: false);
			}
			nameTag.SetColor(chatMessage.UserNameColor);
			colonText.color = chatMessage.UserNameColor;
			colonText.gameObject.SetActive(chatMessage.isChatMessage);
			chatText.text = text;
			chatText.color = chatMessage.MessageColor;
		}
		chatText.fontSize = GameSettings.GetInstance().ChatMessageFontSize;
	}

	private string ColorToHex(Color32 color)
	{
		return "#" + color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2") + color.a.ToString("X2");
	}

	private Color HexToColor(string hex)
	{
		byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
		byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
		byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
		return new Color32(r, g, b, byte.MaxValue);
	}
}
