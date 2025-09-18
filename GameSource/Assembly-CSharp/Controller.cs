using System;
using System.Collections.Generic;
using GameEvent;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class Controller : MonoBehaviour, InputMethod
{
	public enum ControllerType
	{
		KEYBOARD,
		XBOX360,
		XBOXONE,
		PLAYSTATION3,
		PLAYSTATION4,
		GENERIC,
		SWITCH_LEFT,
		SWITCH_RIGHT,
		SWITCH_DUAL,
		SWITCH_FULL,
		SWITCH_HANDHELD
	}

	protected int Player;

	protected List<InputReceiver> receivers;

	protected Character.Animals[] associatedChars = new Character.Animals[4];

	protected bool assumeUser;

	protected bool usePreciseCursor;

	protected bool connected;

	public static InputField lockedInputField;

	public static bool justUnlocked = false;

	public float TimeSinceLastInput;

	public bool IsAFK;

	public float AFKSignaler;

	public int PossibleNetWorkNumber;

	public static bool FullScreenComputerIsActive = false;

	private static List<InputReceiver> receiverListCache = new List<InputReceiver>(256);

	private static List<int> nullReceiverCache = new List<int>(32);

	private List<InputEvent> deferredInputEvents = new List<InputEvent>(32);

	private List<string> ClearedReceivers;

	private Controller redirect;

	protected static List<InputReceiver> globalReceivers = new List<InputReceiver>();

	protected static List<Controller> allControllers = new List<Controller>();

	public bool Connected => connected;

	public bool IsKeyboard => GetControllerType() == ControllerType.KEYBOARD;

	public virtual int PadIndex => -1;

	public static bool InputFieldIsActive => lockedInputField != null;

	public static bool InputFieldWasActiveRecently
	{
		get
		{
			foreach (Controller allController in allControllers)
			{
				if (allController.IsKeyboard)
				{
					return (allController as KeyboardInput).framesSinceUnlock < 3;
				}
			}
			return false;
		}
	}

	protected virtual void Start()
	{
		foreach (InputReceiver globalReceiver in globalReceivers)
		{
			AddReceiver(globalReceiver);
		}
		if (!allControllers.Contains(this))
		{
			allControllers.Add(this);
		}
	}

	public abstract bool IsUsingPosition();

	public abstract Vector2 GetVector(bool absolute = false);

	public abstract void Reset();

	public abstract ControllerType GetControllerType();

	public virtual void Awake()
	{
		receivers = new List<InputReceiver>();
	}

	public void AssumeUser(bool assume)
	{
		assumeUser = assume;
	}

	public bool IsAssumingUser()
	{
		return assumeUser;
	}

	public void AddPlayer(int player)
	{
		if (player >= 1 && player <= 4)
		{
			Player |= 1 << player - 1;
		}
	}

	public void RemovePlayer(int player)
	{
		int num = 0xF ^ (1 << player - 1);
		Player &= num;
		associatedChars[player - 1] = Character.Animals.NONE;
		if (Player == 0)
		{
			PossibleNetWorkNumber = 0;
		}
	}

	public int GetLastPlayerNumber()
	{
		for (int num = 3; num >= 0; num--)
		{
			if ((Player & (1 << num)) > 0)
			{
				return num + 1;
			}
		}
		return 0;
	}

	public int GetLastPlayerNumberAfter(int lastPlayerNumber)
	{
		bool flag = false;
		for (int num = 3; num >= 0; num--)
		{
			if ((Player & (1 << num)) > 0 && (flag || lastPlayerNumber == num + 1))
			{
				if (flag)
				{
					return num + 1;
				}
				flag = true;
			}
		}
		return 0;
	}

	public void ClearPlayers()
	{
		Player = 0;
		associatedChars = new Character.Animals[4];
	}

	public void AssociateCharacter(Character.Animals character, int player)
	{
		if (player >= 1 && player <= 4 && (Player & (1 << player - 1)) != 0)
		{
			associatedChars[player - 1] = character;
		}
	}

	public Character.Animals[] GetAssociatedCharacters()
	{
		return associatedChars;
	}

	public bool ControlsPlayer(int player)
	{
		return (Player & (1 << player - 1)) > 0;
	}

	public int GetControlMask()
	{
		return Player;
	}

	public virtual void AddReceiver(InputReceiver r)
	{
		if (receivers.Contains(r))
		{
			return;
		}
		receivers.Add(r);
		if (r is Character)
		{
			Character character = r as Character;
			if (character.networkNumber != 0)
			{
				PossibleNetWorkNumber = character.networkNumber;
			}
		}
		else if (r is Cursor)
		{
			Cursor cursor = r as Cursor;
			if (cursor.networkNumber != 0)
			{
				PossibleNetWorkNumber = cursor.networkNumber;
			}
		}
	}

	public virtual void RemoveReceiver(InputReceiver r)
	{
		if (receivers.Contains(r))
		{
			receivers.Remove(r);
		}
	}

	public void ClearReceivers()
	{
		ClearReceivers(keepGlobal: false);
	}

	public List<InputReceiver> GetAllReceivers()
	{
		return new List<InputReceiver>(receivers);
	}

	public void ClearReceivers(bool keepGlobal)
	{
		if (Debug.isDebugBuild)
		{
			Debug.Log("Recording cleared receivers...");
			ClearedReceivers = new List<string>(receivers.Count);
			foreach (InputReceiver receiver in receivers)
			{
				try
				{
					MonoBehaviour monoBehaviour = receiver as MonoBehaviour;
					if (monoBehaviour != null)
					{
						ClearedReceivers.Add(monoBehaviour.name);
					}
					else
					{
						Debug.LogWarning("Found null receiver while clearing receivers...");
					}
				}
				catch (Exception ex)
				{
					Debug.LogError("Error while recording cleared receivers: " + ex.Message + "\n" + ex.StackTrace);
				}
			}
		}
		if (keepGlobal)
		{
			receivers.RemoveAll((InputReceiver r) => !globalReceivers.Contains(r));
		}
		else
		{
			receivers.Clear();
		}
	}

	public void NotifyNextFrame(InputEvent e)
	{
		deferredInputEvents.Add(e);
	}

	public void Notify(InputEvent e)
	{
		if (redirect != null)
		{
			redirect.Notify(e);
			return;
		}
		e.Sender = this;
		receiverListCache.Clear();
		foreach (InputReceiver receiver in receivers)
		{
			receiverListCache.Add(receiver);
		}
		foreach (InputReceiver item in receiverListCache)
		{
			if (e.Consumed)
			{
				break;
			}
			if ((item is UnityEngine.Object && (UnityEngine.Object)item != null) || item != null)
			{
				item.ReceiveEvent(e);
			}
		}
		e.Consume();
		nullReceiverCache.Clear();
		for (int i = 0; i != receivers.Count; i++)
		{
			InputReceiver inputReceiver = receivers[i];
			if (inputReceiver is UnityEngine.Object && (UnityEngine.Object)inputReceiver == null)
			{
				nullReceiverCache.Add(i);
			}
		}
		for (int num = nullReceiverCache.Count - 1; num >= 0; num--)
		{
			receivers.SwapRemove(nullReceiverCache[num]);
		}
		if (Player != 0)
		{
			TimeSinceLastInput = 0f;
		}
	}

	public void RedirectInputTo(Controller target)
	{
		redirect = target;
	}

	public Character.Animals GetFirstAssociatedCharacter()
	{
		Character.Animals[] associatedCharacters = GetAssociatedCharacters();
		foreach (Character.Animals animals in associatedCharacters)
		{
			if (animals != Character.Animals.NONE)
			{
				return animals;
			}
		}
		return Character.Animals.NONE;
	}

	public void SetPreciseCursor(bool precise)
	{
		usePreciseCursor = precise;
	}

	public virtual void Update()
	{
		if (Player != 0 && PossibleNetWorkNumber != 0)
		{
			TimeSinceLastInput += Time.unscaledDeltaTime;
			if (!IsAFK)
			{
				if (TimeSinceLastInput > GameSettings.GetInstance().AFKThreshold)
				{
					IsAFK = true;
					MsgAFKPlayer msgAFKPlayer = new MsgAFKPlayer();
					msgAFKPlayer.isAFK = true;
					msgAFKPlayer.PlayerNetworkNumber = PossibleNetWorkNumber;
					if (LobbyManager.instance != null && LobbyManager.instance.client != null)
					{
						LobbyManager.instance.client.Send(NetMsgTypes.AFKPlayer, msgAFKPlayer);
					}
				}
			}
			else if (TimeSinceLastInput < GameSettings.GetInstance().AFKThreshold)
			{
				IsAFK = false;
				MsgAFKPlayer msgAFKPlayer2 = new MsgAFKPlayer();
				msgAFKPlayer2.isAFK = false;
				msgAFKPlayer2.PlayerNetworkNumber = PossibleNetWorkNumber;
				if (LobbyManager.instance != null)
				{
					LobbyManager.instance.client.Send(NetMsgTypes.AFKPlayer, msgAFKPlayer2);
				}
			}
		}
		foreach (InputEvent deferredInputEvent in deferredInputEvents)
		{
			Notify(deferredInputEvent);
		}
		deferredInputEvents.Clear();
	}

	public void Disconnect(Player player = null)
	{
		GameEventManager.SendEvent(new ControllerConnectionEvent(connected: false, player));
		connected = false;
		ClearPlayers();
		ClearReceivers(keepGlobal: true);
		if (player != null)
		{
			player.UseController = null;
		}
	}

	public void Connect(Player player = null)
	{
		GameEventManager.SendEvent(new ControllerConnectionEvent(connected: true, player));
		connected = true;
		if (player != null)
		{
			AddPlayer(player.Number);
		}
	}

	public void RestoreReceivers(Player p, Controller oldController)
	{
		if (p.UseController != this)
		{
			Debug.LogError("RestoreReceivers: Player is not using this controller.");
		}
		CheatListener componentInChildren = GetComponentInChildren<CheatListener>();
		if (componentInChildren != null)
		{
			AddReceiver(componentInChildren);
		}
		if (p.AssociatedLobbyPlayer != null)
		{
			AddReceiver(p.AssociatedLobbyPlayer);
			if (p.AssociatedLobbyPlayer.CharacterInstance != null)
			{
				AddReceiver(p.AssociatedLobbyPlayer.CharacterInstance);
				p.AssociatedLobbyPlayer.CharacterInstance.SetLocalController(this);
			}
			if (p.AssociatedLobbyPlayer.CursorInstance != null)
			{
				AddReceiver(p.AssociatedLobbyPlayer.CursorInstance);
				p.AssociatedLobbyPlayer.CursorInstance.SetLocalController(this);
			}
			if (p.AssociatedLobbyPlayer.EmoteSystem != null)
			{
				AddReceiver(p.AssociatedLobbyPlayer.EmoteSystem);
			}
		}
		if (p.AssociatedGamePlayer != null)
		{
			if (p.AssociatedGamePlayer.CharacterInstance != null)
			{
				AddReceiver(p.AssociatedGamePlayer.CharacterInstance);
				p.AssociatedGamePlayer.CharacterInstance.SetLocalController(this);
			}
			if (p.AssociatedGamePlayer.CursorInstance != null)
			{
				AddReceiver(p.AssociatedGamePlayer.CursorInstance);
				p.AssociatedGamePlayer.CursorInstance.SetLocalController(this);
			}
		}
		foreach (InputReceiver receiver in oldController.receivers)
		{
			if (!receivers.Contains(receiver))
			{
				receivers.Add(receiver);
			}
		}
		if (!Debug.isDebugBuild || ClearedReceivers == null)
		{
			return;
		}
		HashSet<string> hashSet = new HashSet<string>();
		foreach (InputReceiver receiver2 in receivers)
		{
			hashSet.Add((receiver2 as MonoBehaviour).name);
		}
		foreach (string clearedReceiver in ClearedReceivers)
		{
			if (!hashSet.Contains(clearedReceiver))
			{
				Debug.LogWarning("Warning: Receiver not restored after controller reconnect: " + clearedReceiver);
			}
		}
		ClearedReceivers.Clear();
	}

	public static void AddGlobalReceiver(InputReceiver r)
	{
		if (globalReceivers.Contains(r))
		{
			return;
		}
		globalReceivers.Add(r);
		foreach (Controller allController in allControllers)
		{
			allController.AddReceiver(r);
		}
	}

	public static void RemoveGlobalReceiver(InputReceiver r)
	{
		if (!globalReceivers.Contains(r))
		{
			return;
		}
		globalReceivers.Remove(r);
		foreach (Controller allController in allControllers)
		{
			allController.RemoveReceiver(r);
		}
	}

	public static void UnlockInputField()
	{
		SetLockedInputFieldInternal(null);
	}

	public static void LockInputField(InputField inputField, UnityAction<string> onEndEdit, bool unlockOnEndEdit = true)
	{
		SetLockedInputFieldInternal(inputField);
		inputField.onEndEdit.RemoveAllListeners();
		inputField.onEndEdit.AddListener(delegate(string value)
		{
			if (unlockOnEndEdit)
			{
				UnlockInputField();
			}
			if (onEndEdit != null)
			{
				onEndEdit(value);
			}
		});
	}

	private static void SetLockedInputFieldInternal(InputField inputField)
	{
		bool num = lockedInputField != null;
		if (lockedInputField != inputField)
		{
			if (lockedInputField != null)
			{
				if (EventSystem.current.currentSelectedGameObject == lockedInputField.gameObject && !EventSystem.current.alreadySelecting)
				{
					EventSystem.current.SetSelectedGameObject(null);
				}
				else
				{
					lockedInputField.DeactivateInputField();
				}
			}
			lockedInputField = inputField;
			if (lockedInputField != null)
			{
				inputField.ActivateInputField();
			}
		}
		else if (lockedInputField != null && !lockedInputField.isFocused)
		{
			lockedInputField.ActivateInputField();
		}
		if (num && lockedInputField == null)
		{
			justUnlocked = true;
		}
	}

	public static Controller GetControllerAtPadIndex(int padIndex)
	{
		foreach (Controller allController in allControllers)
		{
			if (allController.PadIndex == padIndex)
			{
				return allController;
			}
		}
		return null;
	}

	public static void ClearPlayersForAllControllers()
	{
		foreach (Controller allController in allControllers)
		{
			allController.ClearPlayers();
		}
	}
}
