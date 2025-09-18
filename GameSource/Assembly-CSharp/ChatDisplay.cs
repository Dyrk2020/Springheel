using System;
using System.Collections.Generic;
using System.Text;
using GameEvent;
using I2.Loc;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;

public class ChatDisplay : MonoBehaviour, InputReceiver, IGameEventListener
{
	public ChatUnit ChatUnitPrefab;

	public ChatInputField ChatInputFieldPrefab;

	public RectTransform ChatHolder;

	public CanvasGroup ChatCanvasGroup;

	public KeyboardInput keyboard;

	public LobbyPlayer keyBoardLobbyPlayer;

	public List<ChatUnit> currentMessages = new List<ChatUnit>();

	public ChatInputField currentChatInputField;

	public int currentmessage;

	protected float VisibilityTimer;

	public bool ChatMode;

	public const int MaxChatLogItems = 30;

	public static Queue<string> chatLog = new Queue<string>();

	private float holderXpos;

	public void UnshiftPosition()
	{
		ChatHolder.anchorMin = Vector2.up;
		ChatHolder.anchorMax = Vector2.up;
		ChatHolder.pivot = Vector2.up;
		Vector3 vector = ChatHolder.anchoredPosition;
		vector.x = holderXpos;
		ChatHolder.anchoredPosition = vector;
	}

	public void ShiftPosition()
	{
		ChatHolder.anchorMin = Vector2.one;
		ChatHolder.anchorMax = Vector2.one;
		ChatHolder.pivot = Vector2.one;
		Vector3 vector = ChatHolder.anchoredPosition;
		vector.x = 0f - holderXpos;
		ChatHolder.anchoredPosition = vector;
	}

	private void Start()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		currentChatInputField = UnityEngine.Object.Instantiate(ChatInputFieldPrefab, ChatHolder.position, ChatHolder.rotation);
		currentChatInputField.transform.SetParent(ChatHolder);
		currentChatInputField.transform.SetAsLastSibling();
		currentChatInputField.Setup();
		holderXpos = ChatHolder.anchoredPosition.x;
		ChangeListener(adding: true);
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, adding);
		GameEventManager.ChangeListener<ClearChatEvent>(this, adding);
	}

	public void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public void Update()
	{
		if ((VisibilityTimer <= 0f && !ChatMode) || (currentMessages.Count == 0 && !currentChatInputField.gameObject.activeSelf))
		{
			float num = Mathf.MoveTowards(ChatCanvasGroup.alpha, 0f, GameSettings.GetInstance().ChatMessagingFadeSpeed * Time.unscaledDeltaTime);
			if (num != ChatCanvasGroup.alpha)
			{
				ChatCanvasGroup.alpha = num;
			}
		}
		else
		{
			float num2 = Mathf.MoveTowards(ChatCanvasGroup.alpha, 1f, GameSettings.GetInstance().ChatMessagingFadeSpeed * 10f * Time.unscaledDeltaTime);
			if (num2 != ChatCanvasGroup.alpha)
			{
				ChatCanvasGroup.alpha = num2;
			}
		}
		if (ChatCanvasGroup.alpha <= 0f && !ChatMode)
		{
			currentChatInputField.gameObject.SetActive(value: false);
		}
		VisibilityTimer -= Time.unscaledDeltaTime;
		if (ChatMode && MultiControllerUIManager.Instance.PlatformHasKeyboard)
		{
			if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))
			{
				currentChatInputField.SendTextChatMessage(CurrentKeyboardSender().NetworkNumber);
				ChatMode = false;
				currentChatInputField.DeactivateInputField();
				currentChatInputField.gameObject.SetActive(value: false);
			}
			else if (Input.GetKeyUp(KeyCode.Escape))
			{
				currentChatInputField.CancelChatMessage();
				ChatMode = false;
				ChatCanvasGroup.alpha = 0.2f;
			}
			if (!currentChatInputField.inputField.isFocused)
			{
				currentChatInputField.inputField.Select();
			}
		}
	}

	public void NewChatMessage(string message, EmoteMeanings EmoteType, int speakerNetworkNumber)
	{
		switch (GameSettings.GetInstance().OnlineChatEmotes)
		{
		case OnlineChatEmotes.EmotesOnly:
			if (EmoteType == EmoteMeanings.CHAT_Text)
			{
				UserMessageManager.Instance.UserMessage(ScriptLocalization.Network.Text_Chat_Disabled_in_options);
				return;
			}
			break;
		case OnlineChatEmotes.ChatAndEmotesOff:
			UserMessageManager.Instance.UserMessage(ScriptLocalization.Network.Text_Chat_and_Emotes_Disabled_in_options);
			return;
		}
		MsgChat msgChat = new MsgChat();
		msgChat.isChatMessage = true;
		msgChat.EmoteType = EmoteType;
		if (EmoteType == EmoteMeanings.CHAT_Text || EmoteType == EmoteMeanings.EMOTE_Explitive)
		{
			msgChat.MessageText = message;
		}
		else
		{
			msgChat.MessageText = "";
		}
		msgChat.NetworkPlayerNumber = speakerNetworkNumber;
		NetworkManager.singleton.client.Send(NetMsgTypes.ChatSent, msgChat);
	}

	public void DisplayNewMessage(ChatMessageDetails chatMessageDetails)
	{
		if (!(ChatHolder == null))
		{
			ChatUnit chatUnit = UnityEngine.Object.Instantiate(ChatUnitPrefab, ChatHolder.position, ChatHolder.rotation);
			chatUnit.transform.SetParent(ChatHolder);
			currentChatInputField.transform.SetAsLastSibling();
			chatUnit.SetChatUnitMessage(chatMessageDetails);
			currentMessages.Add(chatUnit);
			if (currentMessages.Count > GameSettings.GetInstance().maxVisibleMessages)
			{
				ChatUnit chatUnit2 = currentMessages[0];
				currentMessages.Remove(chatUnit2);
				UnityEngine.Object.Destroy(chatUnit2.gameObject);
			}
			VisibilityTimer = GameSettings.GetInstance().ChatMessagingFadeTime;
		}
	}

	public void DebugChatSystemMessage(string message)
	{
		if (GameState.DebugMode)
		{
			GameState.ChatSystem.DisplayNewMessage(new ChatMessageDetails(Character.Animals.NONE, null, Color.white, message, EmoteMeanings.CHAT_Text, 0));
		}
	}

	public void ReceiveEvent(InputEvent e)
	{
		if (LobbyManager.instance == null || !LobbyManager.instance.IsInOnlineGame)
		{
			return;
		}
		if (e.Key == InputEvent.InputKey.Chat && e.Valueb && e.Changed)
		{
			if (currentChatInputField == null)
			{
				return;
			}
			ChatMode = !ChatMode;
			if (ChatMode)
			{
				if (e.Sender is KeyboardInput)
				{
					ChatMessageDetails chatMessageDetails = CurrentKeyboardSender();
					currentChatInputField.SetPortrait(chatMessageDetails.Animal, chatMessageDetails.UserNameColor);
				}
				else
				{
					bool flag = false;
					foreach (Player item in PlayerManager.GetInstance())
					{
						if (item != null && item.AssociatedLobbyPlayer != null && item.UseController != null && item.UseController == e.Sender)
						{
							if (item.PlayerCharacter != null)
							{
								currentChatInputField.SetPortrait(item.PlayerCharacter.CharacterSprite, item.AssociatedLobbyPlayer.PlayerColor);
							}
							else
							{
								currentChatInputField.SetPortrait(Character.Animals.NONE, item.AssociatedLobbyPlayer.PlayerColor);
							}
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						currentChatInputField.SetPortrait(Character.Animals.NONE, Color.white);
					}
				}
				currentChatInputField.ActivateInputField(e.Sender);
				SteamDeck.OpenVirtualKeyboard(null);
			}
		}
		if (e.Key != InputEvent.InputKey.LeftTrigger)
		{
			return;
		}
		if (e.Valueb)
		{
			if (VisibilityTimer < 1f)
			{
				VisibilityTimer = 1f;
			}
		}
		else if (e.Changed && VisibilityTimer <= 1f)
		{
			VisibilityTimer = 0f;
		}
	}

	protected ChatMessageDetails CurrentKeyboardSender()
	{
		ChatMessageDetails result = default(ChatMessageDetails);
		if (keyBoardLobbyPlayer == null)
		{
			NetworkLobbyPlayer[] lobbySlots = LobbyManager.instance.lobbySlots;
			for (int i = 0; i < lobbySlots.Length; i++)
			{
				LobbyPlayer lobbyPlayer = (LobbyPlayer)lobbySlots[i];
				if (!(lobbyPlayer == null) && lobbyPlayer != null && lobbyPlayer.LocalPlayer != null)
				{
					result.Animal = lobbyPlayer.LocalPlayer.UseController.GetFirstAssociatedCharacter();
					result.UserName = lobbyPlayer.playerName;
					result.UserNameColor = lobbyPlayer.PlayerColor;
					result.EmoteType = EmoteMeanings.CHAT_Text;
					result.NetworkNumber = lobbyPlayer.networkNumber;
					break;
				}
			}
		}
		else
		{
			result.Animal = keyBoardLobbyPlayer.LocalPlayer.UseController.GetFirstAssociatedCharacter();
			result.UserName = keyBoardLobbyPlayer.playerName;
			result.UserNameColor = keyBoardLobbyPlayer.PlayerColor;
			result.NetworkNumber = keyBoardLobbyPlayer.networkNumber;
			result.EmoteType = EmoteMeanings.CHAT_Text;
		}
		return result;
	}

	public async void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(NetworkMessageReceivedEvent))
		{
			NetworkMessageReceivedEvent networkMessageReceivedEvent = e as NetworkMessageReceivedEvent;
			if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.ChatSent)
			{
				MsgChat msgChat = networkMessageReceivedEvent.ReadMessage as MsgChat;
				NetworkLobbyPlayer[] lobbySlots = LobbyManager.instance.lobbySlots;
				for (int i = 0; i < lobbySlots.Length; i++)
				{
					LobbyPlayer lobbyPlayer = (LobbyPlayer)lobbySlots[i];
					if (lobbyPlayer == null || lobbyPlayer.networkNumber != msgChat.NetworkPlayerNumber)
					{
						continue;
					}
					EFriendRelationship eFriendRelationship = EFriendRelationship.k_EFriendRelationshipNone;
					if (SteamManager.Initialized)
					{
						eFriendRelationship = SteamFriends.GetFriendRelationship(new CSteamID(lobbyPlayer.SteamID));
					}
					bool flag = eFriendRelationship == EFriendRelationship.k_EFriendRelationshipBlocked || eFriendRelationship == EFriendRelationship.k_EFriendRelationshipIgnoredFriend;
					if (lobbyPlayer.Muted || flag)
					{
						break;
					}
					ChatMessageDetails chatMessageDetails = new ChatMessageDetails
					{
						Animal = lobbyPlayer.PickedAnimal,
						UserName = lobbyPlayer.playerName,
						UserNameColor = lobbyPlayer.PlayerColor,
						EmoteType = msgChat.EmoteType,
						NetworkNumber = lobbyPlayer.networkNumber,
						Message = msgChat.MessageText,
						isChatMessage = msgChat.isChatMessage,
						MessageColor = Color.white
					};
					if (PlatformFeatureRestrictions.IsChatRestricted && msgChat.EmoteType == EmoteMeanings.CHAT_Text)
					{
						break;
					}
					bool flag2 = false;
					switch (GameSettings.GetInstance().OnlineChatEmotes)
					{
					case OnlineChatEmotes.ChatAndEmotesOn:
						DisplayNewMessage(chatMessageDetails);
						flag2 = true;
						break;
					case OnlineChatEmotes.EmotesOnly:
						if (chatMessageDetails.EmoteType != EmoteMeanings.CHAT_Text)
						{
							DisplayNewMessage(chatMessageDetails);
							flag2 = true;
						}
						break;
					}
					if (flag2)
					{
						LogChat(chatMessageDetails.UserName, chatMessageDetails.Message);
					}
					break;
				}
			}
		}
		if (!(type == typeof(ClearChatEvent)))
		{
			return;
		}
		foreach (ChatUnit currentMessage in currentMessages)
		{
			UnityEngine.Object.Destroy(currentMessage.gameObject);
		}
		currentMessages.Clear();
		ChatMode = false;
		currentChatInputField.DeactivateInputField();
		currentChatInputField.gameObject.SetActive(value: false);
		ClearChatLog();
		UserReports.ClearReportedUserLog();
	}

	public static void LogChat(string playerName, string message)
	{
		chatLog.Enqueue(DateTime.Now.ToString("h:mm:ss tt") + " <" + playerName + "> " + message + "\n");
		while (chatLog.Count > 30)
		{
			chatLog.Dequeue();
		}
	}

	public static string GetChatLogAsString()
	{
		int num = 0;
		foreach (string item in chatLog)
		{
			num += item.Length;
		}
		StringBuilder stringBuilder = new StringBuilder(num);
		foreach (string item2 in chatLog)
		{
			stringBuilder.Append(item2);
		}
		return stringBuilder.ToString();
	}

	public static void ClearChatLog()
	{
		chatLog.Clear();
	}
}
