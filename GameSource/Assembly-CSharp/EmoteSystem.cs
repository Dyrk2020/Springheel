using System;
using System.Collections;
using GameEvent;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class EmoteSystem : MonoBehaviour, InputReceiver, IGameEventListener
{
	public Text Up;

	public Text Left;

	public Text Right;

	public Text Down;

	public Image characterPortrait;

	public RectTransform centerHolder;

	public Camera SceneCamera;

	public Camera UICamera;

	private Camera cameraToUse;

	protected EmoteMeanings[] contentTitleEnum;

	protected EmoteMeanings[] UpContentEnums;

	protected EmoteMeanings[] LeftContentEnums;

	protected EmoteMeanings[] RightContentEnums;

	protected EmoteMeanings[] DownContentEnums;

	public EmoteMeanings[] contentTitleEnumRun;

	public EmoteMeanings[] UpContentEnumsRun;

	public EmoteMeanings[] LeftContentEnumsRun;

	public EmoteMeanings[] RightContentEnumsRun;

	public EmoteMeanings[] DownContentEnumsRun;

	public EmoteMeanings[] contentTitleEnumBuild;

	public EmoteMeanings[] UpContentEnumsBuild;

	public EmoteMeanings[] LeftContentEnumsBuild;

	public EmoteMeanings[] RightContentEnumsBuild;

	public EmoteMeanings[] DownContentEnumsBuild;

	public EmoteMeanings[] contentTitleEnumLobby;

	public EmoteMeanings[] UpContentEnumsLobby;

	public EmoteMeanings[] LeftContentEnumsLobby;

	public EmoteMeanings[] RightContentEnumsLobby;

	public EmoteMeanings[] DownContentEnumsLobby;

	public CanvasGroup EmoteCanvasGroup;

	public LobbyPlayer LobbyPlayer;

	public GamePlayer GamePlayer;

	public Animator animator;

	protected emoteState emoteState;

	protected float TargetVisibility;

	protected bool TriggerNeedsReleasing;

	protected float KeyBoardVisibilityTimer;

	protected EmoteMeanings[] possibleContent;

	protected bool lockEmotePosition;

	protected Vector3 TargetPosition;

	protected int EmoteLimitRemaining;

	private void Start()
	{
		ZoomCamera currentZoomCamera = LobbyManager.instance.GetCurrentZoomCamera();
		if ((bool)currentZoomCamera)
		{
			SceneCamera = currentZoomCamera.GetComponent<Camera>();
		}
		UICamera = LobbyManager.instance.GetCurrentUICamera();
		ChangeListener(adding: true);
		SetEmoteContext(EmoteContext.CONTEXT_LOBBY);
		HideEmoteUI();
		EmoteLimitRemaining = 4;
		StartCoroutine(EmoteRefill());
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<StartPhaseEvent>(this, adding);
		GameEventManager.ChangeListener<GameStartEvent>(this, adding);
	}

	private void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	private IEnumerator EmoteRefill()
	{
		while (true)
		{
			if (EmoteLimitRemaining < 4)
			{
				EmoteLimitRemaining++;
			}
			yield return new WaitForSeconds(60f / (float)GameSettings.GetInstance().MaxEmotesPerMinute);
		}
	}

	public void Update()
	{
		if (LobbyPlayer == null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		EmoteCanvasGroup.alpha = Mathf.MoveTowards(EmoteCanvasGroup.alpha, TargetVisibility, GameSettings.GetInstance().emoteUIFadeSpeed * Time.unscaledDeltaTime);
		if (TriggerNeedsReleasing || emoteState == emoteState.EmoteSent)
		{
			return;
		}
		if (emoteState == emoteState.Nothing)
		{
			TargetVisibility = 0f;
			if ((double)EmoteCanvasGroup.alpha < 0.01)
			{
				HideEmoteUI();
			}
		}
		else
		{
			if (SceneCamera == null)
			{
				Debug.Log("Scene EmoteSystem Camera is Null");
				if (UICamera == null)
				{
					Debug.Log("UI EmoteSystem Camera is Null, no camera's available. Emote System hidden");
					return;
				}
			}
			cameraToUse = SceneCamera;
			bool flag = false;
			if (GamePlayer != null && GamePlayer.CharacterInstance != null)
			{
				if (GamePlayer.PartyPickCursor != null && !GamePlayer.PartyPickCursor.Frozen && GamePlayer.PartyPickCursor.Enabled && !lockEmotePosition)
				{
					TargetPosition = GamePlayer.PartyPickCursor.transform.position;
					if (UICamera != null)
					{
						cameraToUse = UICamera;
					}
				}
				else if (GameSettings.GetInstance().GameMode != GameState.GameMode.CHALLENGE && GamePlayer.CursorInstance != null && !GamePlayer.CursorInstance.Frozen && GamePlayer.CursorInstance.Enabled && !lockEmotePosition)
				{
					TargetPosition = GamePlayer.CursorInstance.transform.position;
				}
				else if (GamePlayer.CharacterInstance != null && GamePlayer.CharacterInstance.Enabled && !GamePlayer.CharacterInstance.Dying && !GamePlayer.CharacterInstance.Dead && !GamePlayer.CharacterInstance.Frozen && !GamePlayer.CharacterInstance.Success && !lockEmotePosition)
				{
					TargetPosition = GamePlayer.CharacterInstance.transform.position;
				}
				else
				{
					flag = true;
					lockEmotePosition = true;
				}
			}
			else if (LobbyPlayer.CharacterInstance != null && LobbyPlayer.CharacterInstance.Enabled && !LobbyPlayer.CharacterInstance.InMenu)
			{
				TargetPosition = LobbyPlayer.CharacterInstance.transform.position;
			}
			else if (LobbyPlayer.CursorInstance != null && LobbyPlayer.CursorInstance.Enabled)
			{
				TargetPosition = LobbyPlayer.CursorInstance.transform.position;
			}
			else
			{
				flag = true;
				lockEmotePosition = true;
			}
			if (cameraToUse != null)
			{
				Vector2 vector;
				if (flag)
				{
					int num = 1;
					if (GamePlayer != null)
					{
						num = GamePlayer.networkNumber;
					}
					else if (LobbyPlayer != null)
					{
						num = LobbyPlayer.networkNumber;
					}
					vector = Vector2.Lerp(new Vector2(cameraToUse.pixelWidth / 5, cameraToUse.pixelHeight / 4), new Vector2(cameraToUse.pixelWidth * 4 / 5, cameraToUse.pixelHeight / 4), (float)(num - 1) / 3f);
					characterPortrait.enabled = true;
				}
				else
				{
					vector = cameraToUse.WorldToScreenPoint(TargetPosition);
					characterPortrait.enabled = false;
				}
				centerHolder.position = vector;
			}
		}
		if (emoteState != emoteState.Nothing && KeyBoardVisibilityTimer > 0f)
		{
			KeyBoardVisibilityTimer -= Time.unscaledDeltaTime;
			if (KeyBoardVisibilityTimer <= 0f)
			{
				TargetVisibility = 0f;
				emoteState = emoteState.Nothing;
			}
		}
	}

	public void SetEmoteContext(EmoteContext newContext)
	{
		switch (newContext)
		{
		case EmoteContext.CONTEXT_RUN:
			contentTitleEnum = contentTitleEnumRun;
			UpContentEnums = UpContentEnumsRun;
			LeftContentEnums = LeftContentEnumsRun;
			RightContentEnums = RightContentEnumsRun;
			DownContentEnums = DownContentEnumsRun;
			break;
		case EmoteContext.CONTEXT_BUILD:
			contentTitleEnum = contentTitleEnumBuild;
			UpContentEnums = UpContentEnumsBuild;
			LeftContentEnums = LeftContentEnumsBuild;
			RightContentEnums = RightContentEnumsBuild;
			DownContentEnums = DownContentEnumsBuild;
			break;
		case EmoteContext.CONTEXT_LOBBY:
			contentTitleEnum = contentTitleEnumLobby;
			UpContentEnums = UpContentEnumsLobby;
			LeftContentEnums = LeftContentEnumsLobby;
			RightContentEnums = RightContentEnumsLobby;
			DownContentEnums = DownContentEnumsLobby;
			break;
		}
	}

	public void HideEmoteUI()
	{
		EmoteCanvasGroup.alpha = 0f;
		TargetVisibility = 0f;
		emoteState = emoteState.Nothing;
		KeyBoardVisibilityTimer = 0f;
		lockEmotePosition = false;
	}

	public void insertString(string[] newStrings)
	{
		if (LobbyPlayer != null && LobbyPlayer.LocalPlayer != null && LobbyPlayer.LocalPlayer.UseController != null && LobbyPlayer.LocalPlayer.UseController.GetControllerType() == Controller.ControllerType.KEYBOARD)
		{
			Up.text = "1." + newStrings[0];
			Left.text = "4." + newStrings[3];
			Right.text = "2." + newStrings[1];
			Down.text = "3." + newStrings[2];
		}
		else
		{
			Up.text = newStrings[0];
			Left.text = newStrings[3];
			Right.text = newStrings[1];
			Down.text = newStrings[2];
		}
	}

	public string getDirectionString(emoteDirections direction)
	{
		string text = "";
		switch (direction)
		{
		case emoteDirections.UP:
			text = Up.text;
			break;
		case emoteDirections.RIGHT:
			text = Right.text;
			break;
		case emoteDirections.DOWN:
			text = Down.text;
			break;
		case emoteDirections.LEFT:
			text = Left.text;
			break;
		}
		if (LobbyPlayer.LocalPlayer.UseController.GetControllerType() == Controller.ControllerType.KEYBOARD)
		{
			text = text.Remove(0, 2);
		}
		return text;
	}

	public void ReceiveEvent(InputEvent e)
	{
		if (LobbyManager.instance == null || !LobbyManager.instance.IsInOnlineGame)
		{
			return;
		}
		if (e.Key == InputEvent.InputKey.LeftTrigger)
		{
			if (e.Sender.GetControllerType() == Controller.ControllerType.KEYBOARD)
			{
				if (e.Changed && e.Valueb)
				{
					emoteState = emoteState.SelectingCategory;
					animator.SetTrigger("Reset");
					insertString(EmoteArrayConvert(contentTitleEnum));
					TargetVisibility = 1f;
					KeyBoardVisibilityTimer = GameSettings.GetInstance().emoteUIDisplayTime;
				}
			}
			else if (e.Valuef > 0.1f)
			{
				if (!TriggerNeedsReleasing)
				{
					if (emoteState == emoteState.Nothing)
					{
						emoteState = emoteState.SelectingCategory;
						animator.SetTrigger("Reset");
						insertString(EmoteArrayConvert(contentTitleEnum));
					}
					TargetVisibility = e.Valuef;
				}
			}
			else
			{
				if (TriggerNeedsReleasing)
				{
					TriggerNeedsReleasing = false;
				}
				emoteState = emoteState.Nothing;
			}
			return;
		}
		if (e.Sender.GetControllerType() == Controller.ControllerType.KEYBOARD && (e.Key == InputEvent.InputKey.OrthoDown2 || e.Key == InputEvent.InputKey.OrthoLeft2 || e.Key == InputEvent.InputKey.OrthoUp2 || e.Key == InputEvent.InputKey.OrthoRight2))
		{
			if (emoteState == emoteState.Nothing)
			{
				emoteState = emoteState.SelectingCategory;
				TargetVisibility = 1f;
				KeyBoardVisibilityTimer = GameSettings.GetInstance().emoteUIDisplayTime;
			}
			else if (emoteState == emoteState.SelectingMessage)
			{
				TargetVisibility = 1f;
				KeyBoardVisibilityTimer = GameSettings.GetInstance().emoteUIDisplayTime;
			}
		}
		if (!e.Changed || !e.Valueb)
		{
			return;
		}
		if (emoteState == emoteState.SelectingCategory)
		{
			switch (e.Key)
			{
			case InputEvent.InputKey.OrthoUp2:
				emoteState = emoteState.SelectingMessage;
				insertString(EmoteArrayConvert(UpContentEnums));
				animator.SetInteger("Direction", 0);
				animator.SetTrigger("Select");
				possibleContent = UpContentEnums;
				break;
			case InputEvent.InputKey.OrthoDown2:
				emoteState = emoteState.SelectingMessage;
				insertString(EmoteArrayConvert(DownContentEnums));
				animator.SetInteger("Direction", 2);
				animator.SetTrigger("Select");
				possibleContent = DownContentEnums;
				break;
			case InputEvent.InputKey.OrthoLeft2:
				emoteState = emoteState.SelectingMessage;
				insertString(EmoteArrayConvert(LeftContentEnums));
				animator.SetInteger("Direction", 3);
				animator.SetTrigger("Select");
				possibleContent = LeftContentEnums;
				break;
			case InputEvent.InputKey.OrthoRight2:
				emoteState = emoteState.SelectingMessage;
				insertString(EmoteArrayConvert(RightContentEnums));
				animator.SetInteger("Direction", 1);
				animator.SetTrigger("Select");
				possibleContent = RightContentEnums;
				break;
			}
		}
		else
		{
			if (emoteState != emoteState.SelectingMessage)
			{
				return;
			}
			bool flag = false;
			emoteDirections emoteDirections2 = emoteDirections.UP;
			switch (e.Key)
			{
			case InputEvent.InputKey.OrthoUp2:
				flag = true;
				emoteDirections2 = emoteDirections.UP;
				break;
			case InputEvent.InputKey.OrthoDown2:
				flag = true;
				emoteDirections2 = emoteDirections.DOWN;
				break;
			case InputEvent.InputKey.OrthoLeft2:
				flag = true;
				emoteDirections2 = emoteDirections.LEFT;
				break;
			case InputEvent.InputKey.OrthoRight2:
				flag = true;
				emoteDirections2 = emoteDirections.RIGHT;
				break;
			}
			if (!flag)
			{
				return;
			}
			if (EmoteLimitRemaining > 0)
			{
				EmoteLimitRemaining--;
				animator.SetInteger("Direction", (int)emoteDirections2);
				animator.SetTrigger("Confirm");
				if (possibleContent[(int)emoteDirections2] == EmoteMeanings.EMOTE_Explitive)
				{
					GameState.ChatSystem.NewChatMessage(getDirectionString(emoteDirections2), possibleContent[(int)emoteDirections2], LobbyPlayer.networkNumber);
				}
				else
				{
					GameState.ChatSystem.NewChatMessage(EmoteConverter(possibleContent[(int)emoteDirections2]), possibleContent[(int)emoteDirections2], LobbyPlayer.networkNumber);
				}
				emoteState = emoteState.EmoteSent;
				if (e.Sender.GetControllerType() != Controller.ControllerType.KEYBOARD)
				{
					TriggerNeedsReleasing = true;
				}
			}
			else
			{
				UserMessageManager.Instance.UserMessage(ScriptLocalization.Network.EmoteLimitReached, 1.5f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: false);
			}
		}
	}

	public static string EmoteConverter(EmoteMeanings emote)
	{
		string result = "error: emote failed";
		switch (emote)
		{
		case EmoteMeanings.CHAT_Text:
			Debug.Log("no emote Selected, was a text message");
			break;
		case EmoteMeanings.TITLE_Information:
			result = ScriptLocalization.Emote_Title.Information;
			break;
		case EmoteMeanings.TITLE_Reactions:
			result = ScriptLocalization.Emote_Title.Reactions;
			break;
		case EmoteMeanings.TITLE_Apology:
			result = ScriptLocalization.Emote_Title.Apologies;
			break;
		case EmoteMeanings.TITLE_Compliment:
			result = ScriptLocalization.Emote_Title.Compliments;
			break;
		case EmoteMeanings.EMOTE_OverHere:
			result = ScriptLocalization.Emote.OverHere;
			break;
		case EmoteMeanings.EMOTE_HurryUp:
			result = ScriptLocalization.Emote.HurryUp;
			break;
		case EmoteMeanings.EMOTE_No:
			result = ScriptLocalization.Emote.No;
			break;
		case EmoteMeanings.EMOTE_Yes:
			result = ScriptLocalization.Emote.Yes;
			break;
		case EmoteMeanings.EMOTE_UhOh:
			result = ScriptLocalization.Emote.UhOh;
			break;
		case EmoteMeanings.EMOTE_Hahaha:
			result = ScriptLocalization.Emote.Hahaha;
			break;
		case EmoteMeanings.EMOTE_Ouch:
			result = ScriptLocalization.Emote.Ouch;
			break;
		case EmoteMeanings.EMOTE_Nooo:
			result = ScriptLocalization.Emote.Noooo;
			break;
		case EmoteMeanings.EMOTE_Amazing:
			result = ScriptLocalization.Emote.Amazing;
			break;
		case EmoteMeanings.EMOTE_Wow:
			result = ScriptLocalization.Emote.Wow;
			break;
		case EmoteMeanings.EMOTE_GreatRun:
			result = ScriptLocalization.Emote.GreatRun;
			break;
		case EmoteMeanings.EMOTE_Thanks:
			result = ScriptLocalization.Emote.Thanks;
			break;
		case EmoteMeanings.EMOTE_Whoops:
			result = ScriptLocalization.Emote.Whoops;
			break;
		case EmoteMeanings.EMOTE_Explitive:
			result = GetExplitive(4, 6);
			break;
		case EmoteMeanings.EMOTE_Sorry:
			result = ScriptLocalization.Emote.Sorry;
			break;
		case EmoteMeanings.EMOTE_NoProblem:
			result = ScriptLocalization.Emote.NoProblem;
			break;
		case EmoteMeanings.TITLE_Design:
			result = ScriptLocalization.Emote_Title.Design;
			break;
		case EmoteMeanings.TITLE_Greetings:
			result = ScriptLocalization.Emote_Title.Greetings;
			break;
		case EmoteMeanings.EMOTE_TooEasy:
			result = ScriptLocalization.Emote.TooEasy;
			break;
		case EmoteMeanings.EMOTE_Thinking:
			result = ScriptLocalization.Emote.Thinking;
			break;
		case EmoteMeanings.EMOTE_OMG:
			result = ScriptLocalization.Emote.OMG;
			break;
		case EmoteMeanings.EMOTE_Yeah:
			result = ScriptLocalization.Emote.Yeah;
			break;
		case EmoteMeanings.EMOTE_GoodIdea:
			result = ScriptLocalization.Emote.GoodIdea;
			break;
		case EmoteMeanings.EMOTE_WellDone:
			result = ScriptLocalization.Emote.WellDone;
			break;
		case EmoteMeanings.EMOTE_SoClose:
			result = ScriptLocalization.Emote.SoClose;
			break;
		case EmoteMeanings.EMOTE_NotThatOne:
			result = ScriptLocalization.Emote.NotThatone;
			break;
		case EmoteMeanings.EMOTE_Impossible:
			result = ScriptLocalization.Emote.Impossible;
			break;
		case EmoteMeanings.EMOTE_Higher:
			result = ScriptLocalization.Emote.Higher;
			break;
		case EmoteMeanings.EMOTE_Lower:
			result = ScriptLocalization.Emote.Lower;
			break;
		case EmoteMeanings.EMOTE_GlueHere:
			result = ScriptLocalization.Emote.GlueHere;
			break;
		case EmoteMeanings.EMOTE_Bomb:
			result = ScriptLocalization.Emote.Bomb;
			break;
		case EmoteMeanings.EMOTE_Rematch:
			result = ScriptLocalization.Emote.Rematch;
			break;
		case EmoteMeanings.EMOTE_WaitingForAFriend:
			result = ScriptLocalization.Emote.WaitingForAFriend;
			break;
		case EmoteMeanings.EMOTE_BeRightBack:
			result = ScriptLocalization.Emote.BeRightBack;
			break;
		case EmoteMeanings.EMOTE_Okay:
			result = ScriptLocalization.Emote.Okay;
			break;
		case EmoteMeanings.EMOTE_Hello:
			result = ScriptLocalization.Emote.Hello;
			break;
		case EmoteMeanings.EMOTE_Goodbye:
			result = ScriptLocalization.Emote.Goodbye;
			break;
		case EmoteMeanings.EMOTE_GoodGame:
			result = ScriptLocalization.Emote.Goodgame;
			break;
		case EmoteMeanings.EMOTE_WellPlayed:
			result = ScriptLocalization.Emote.WellPlayed;
			break;
		case EmoteMeanings.EMOTE_NotThere:
			result = ScriptLocalization.Emote.NotThere;
			break;
		case EmoteMeanings.EMOTE_MoreTraps:
			result = ScriptLocalization.Emote.MoreTraps;
			break;
		case EmoteMeanings.EMOTE_NiceOutfit:
			result = ScriptLocalization.Emote.NiceOutfit;
			break;
		}
		return result;
	}

	public string[] EmoteArrayConvert(EmoteMeanings[] emotes)
	{
		if (emotes == null)
		{
			return null;
		}
		string[] array = new string[emotes.Length];
		for (int i = 0; i < emotes.Length; i++)
		{
			array[i] = EmoteConverter(emotes[i]);
		}
		return array;
	}

	public static string GetExplitive(int minLength, int maxLength)
	{
		string text = "";
		string text2 = "&&&@@%%##**$©";
		int num = UnityEngine.Random.Range(minLength, maxLength);
		char c = ' ';
		for (int i = 0; i < num; i++)
		{
			char c2;
			for (c2 = text2[UnityEngine.Random.Range(0, text2.Length)]; c2 == c; c2 = text2[UnityEngine.Random.Range(0, text2.Length)])
			{
			}
			text += c2;
			c = c2;
		}
		return text + "!";
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(GameStartEvent))
		{
			SetEmoteContext(EmoteContext.CONTEXT_BUILD);
		}
		if (type == typeof(StartPhaseEvent))
		{
			StartPhaseEvent startPhaseEvent = e as StartPhaseEvent;
			if (startPhaseEvent.Phase == GameControl.GamePhase.PLACE)
			{
				SetEmoteContext(EmoteContext.CONTEXT_BUILD);
			}
			else if (startPhaseEvent.Phase == GameControl.GamePhase.PLAY || startPhaseEvent.Phase == GameControl.GamePhase.SUDDENDEATH)
			{
				SetEmoteContext(EmoteContext.CONTEXT_RUN);
			}
		}
	}
}
