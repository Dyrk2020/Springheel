using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ChatInputField : MonoBehaviour
{
	public Image inputFieldPortrait;

	public Text inputFieldText;

	public Text PlaceHolderText;

	public InputField inputField;

	public void Setup()
	{
		inputFieldText.fontSize = GameSettings.GetInstance().ChatMessageFontSize;
		PlaceHolderText.fontSize = GameSettings.GetInstance().ChatMessageFontSize;
	}

	public void ActivateInputField(Controller controller)
	{
		base.gameObject.SetActive(value: true);
		Controller.LockInputField(inputField, null, unlockOnEndEdit: false);
	}

	public void DeactivateInputField()
	{
		Controller.UnlockInputField();
		base.gameObject.SetActive(value: false);
	}

	public void SendTextChatMessage(int senderNetworkNumber)
	{
		if (inputField.text != "")
		{
			GameState.ChatSystem.NewChatMessage(inputField.text, EmoteMeanings.CHAT_Text, senderNetworkNumber);
		}
		CancelChatMessage();
	}

	public void CancelChatMessage()
	{
		inputField.text = "";
		DeactivateInputField();
	}

	public void SetPortrait(Character.Animals animal, Color color)
	{
		inputFieldPortrait.sprite = CharacterSpriteManager.GetInstance().GetCharaterPortrait(animal);
		if (animal == Character.Animals.NONE)
		{
			inputFieldPortrait.color = color;
		}
		else
		{
			inputFieldPortrait.color = Color.white;
		}
	}

	private int GetFirstLocalLobbyPlayerNetworkNumber()
	{
		NetworkLobbyPlayer[] lobbySlots = LobbyManager.instance.lobbySlots;
		for (int i = 0; i < lobbySlots.Length; i++)
		{
			LobbyPlayer lobbyPlayer = (LobbyPlayer)lobbySlots[i];
			if (lobbyPlayer != null && lobbyPlayer.LocalPlayer != null)
			{
				return lobbyPlayer.networkNumber;
			}
		}
		return -1;
	}
}
