using System.Collections.Generic;
using GameEvent;
using UnityEngine;

public class ControllerDisconnect : MonoBehaviour, InputReceiver, IGameEventListener
{
	public XboxReconnectPrompt[] ConnectPrompts;

	private static bool[] showingPrompts = new bool[4];

	private List<InputReceiver>[] orphanedReceivers = new List<InputReceiver>[4]
	{
		new List<InputReceiver>(),
		new List<InputReceiver>(),
		new List<InputReceiver>(),
		new List<InputReceiver>()
	};

	private Character.Animals[][] orphanedCharacters = new Character.Animals[4][];

	public static void ClearAllPrompts()
	{
		for (int i = 0; i != showingPrompts.Length; i++)
		{
			showingPrompts[i] = false;
		}
	}

	public static void SetPromptForPlayer(int playerNumber, bool shown)
	{
		if (playerNumber >= 1 && playerNumber <= 4)
		{
			showingPrompts[playerNumber - 1] = shown;
		}
	}

	private void Start()
	{
		Controller.AddGlobalReceiver(this);
		GameEventManager.ChangeListener<ControllerConnectionEvent>(this, adding: true);
		foreach (Player item in PlayerManager.GetInstance())
		{
			if (item != null && item.UseController != null && !item.UseController.Connected)
			{
				showingPrompts[item.Number - 1] = true;
			}
		}
		for (int i = 0; i != showingPrompts.Length; i++)
		{
			if (showingPrompts[i])
			{
				ConnectPrompts[i].Show();
			}
			else
			{
				ConnectPrompts[i].Hide();
			}
		}
	}

	private void OnDestroy()
	{
		GameEventManager.ChangeListener<ControllerConnectionEvent>(this, adding: false);
	}

	public void ReceiveEvent(InputEvent e)
	{
		if (e.Key == InputEvent.InputKey.Accept && e.Valueb && e.Changed && !AnyPlayerUsingController(e.Sender))
		{
			reassignController(e.Sender);
		}
	}

	private void reassignController(Controller controller)
	{
	}

	public static bool AnyPlayerUsingController(Controller controller)
	{
		foreach (Player item in PlayerManager.GetInstance())
		{
			if (item != null && item.UseController != null && item.UseController == controller)
			{
				return true;
			}
		}
		return false;
	}

	public static bool AnyPlayerIsMissingController()
	{
		foreach (Player item in PlayerManager.GetInstance())
		{
			if (item != null && (item.UseController == null || !item.UseController.Connected))
			{
				return true;
			}
		}
		return false;
	}

	private void assignControllerToFirstDisconnectedPlayer(Controller controller)
	{
		foreach (Player item in PlayerManager.GetInstance())
		{
			if (item != null && (item.UseController == null || !item.UseController.Connected))
			{
				Debug.Log("Player " + item?.ToString() + " is missing a controller. Assigning one.");
				assignControllerToPlayer(controller, item);
				break;
			}
		}
	}

	private void assignControllerToPlayer(Controller controller, Player player)
	{
		player.UseController = controller;
		Debug.Log("Assigning controller to player " + player);
		Cursor[] array = Object.FindObjectsOfType<Cursor>();
		foreach (Cursor cursor in array)
		{
			if (cursor.localNumber == player.Number)
			{
				controller.AddReceiver(cursor);
			}
		}
		List<InputReceiver> list = orphanedReceivers[player.Number - 1];
		foreach (InputReceiver item in list)
		{
			if (item != null)
			{
				controller.AddReceiver(item);
			}
		}
		list.Clear();
		Character.Animals[] array2 = orphanedCharacters[player.Number - 1];
		for (int j = 0; j != array2.Length; j++)
		{
			controller.AssociateCharacter(array2[j], j + 1);
		}
		orphanedCharacters[player.Number - 1] = new Character.Animals[4];
		Debug.Log("Hiding controller prompt " + (player.Number - 1));
		ConnectPrompts[player.Number - 1].Hide();
		showingPrompts[player.Number - 1] = false;
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		ControllerConnectionEvent controllerConnectionEvent = e as ControllerConnectionEvent;
		Debug.Log("Controller connection event received: " + controllerConnectionEvent.Connected + " for " + controllerConnectionEvent.Player);
		if (controllerConnectionEvent != null && controllerConnectionEvent.Player != null)
		{
			if (controllerConnectionEvent.Connected)
			{
				Debug.Log("Hiding controller prompt " + (controllerConnectionEvent.Player.Number - 1));
				ConnectPrompts[controllerConnectionEvent.Player.Number - 1].Hide();
				showingPrompts[controllerConnectionEvent.Player.Number - 1] = false;
			}
			else
			{
				Debug.Log("Showing controller prompt " + (controllerConnectionEvent.Player.Number - 1));
				orphanedReceivers[controllerConnectionEvent.Player.Number - 1] = controllerConnectionEvent.Player.UseController.GetAllReceivers();
				orphanedCharacters[controllerConnectionEvent.Player.Number - 1] = controllerConnectionEvent.Player.UseController.GetAssociatedCharacters();
				ConnectPrompts[controllerConnectionEvent.Player.Number - 1].Show();
				showingPrompts[controllerConnectionEvent.Player.Number - 1] = true;
			}
		}
	}
}
