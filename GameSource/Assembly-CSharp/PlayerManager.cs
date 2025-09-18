using System.Collections;
using GameEvent;
using UnityEngine;

public class PlayerManager : IEnumerable
{
	private int playerCount;

	public static int maxPlayers = 4;

	private Player[] playerList;

	public bool FirstUserLoggedIn;

	private static PlayerManager instance;

	public Player[] Players => playerList;

	public int NumPlayers
	{
		get
		{
			int num = 0;
			Player[] array = playerList;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != null)
				{
					num++;
				}
			}
			return num;
		}
	}

	public static PlayerManager GetInstance()
	{
		if (instance == null)
		{
			instance = new PlayerManager();
		}
		return instance;
	}

	private PlayerManager()
	{
		maxPlayers = GameSettings.GetInstance().MaxPlayers;
		playerList = new Player[GameSettings.GetInstance().MaxPlayers];
	}

	public Player AddPlayer(Controller control)
	{
		if (playerCount < maxPlayers)
		{
			for (int i = 0; i != maxPlayers; i++)
			{
				if (playerList[i] == null)
				{
					Player player = new Player(i + 1);
					player.UseController = control;
					playerList[i] = player;
					playerCount++;
					GameEventManager.SendEvent(new LocalPlayerAddedEvent(player));
					return player;
				}
			}
		}
		return null;
	}

	public Player GetPlayer(int number)
	{
		if (number < 1 || number > maxPlayers)
		{
			Debug.LogWarning("Chosen player " + number + " is out of range");
			return null;
		}
		return playerList[number - 1];
	}

	public Player RemovePlayer(int number)
	{
		if (number < 1 || number > maxPlayers)
		{
			Debug.LogWarning("Chosen player " + number + " is out of range");
			return null;
		}
		Player player = playerList[number - 1];
		if (player != null)
		{
			RichPresenceManager.Instance.SetDefaultPresenceString(player);
			playerCount--;
			player.Removed = true;
		}
		playerList[number - 1] = null;
		if (player != null)
		{
			GameEventManager.SendEvent(new LocalPlayerRemovedEvent(player));
			ControllerDisconnect.SetPromptForPlayer(player.Number, shown: false);
		}
		return player;
	}

	public void ClearAllPlayers()
	{
		Debug.Log("Clearing all players");
		for (int i = 0; i != playerList.Length; i++)
		{
			Player player = playerList[i];
			if (player != null)
			{
				RichPresenceManager.Instance.SetDefaultPresenceString(player);
				playerCount--;
				player.Removed = true;
				playerList[i] = null;
				GameEventManager.SendEvent(new LocalPlayerRemovedEvent(player));
			}
		}
		playerCount = 0;
		ControllerMonitor.Instance.ClearAllJoinedControllers();
		ControllerDisconnect.ClearAllPrompts();
	}

	public void ClearFirstLogin(bool sendDrivingPlayerRemovedEvent = true)
	{
		Debug.Log("Clearing first logged in player");
		FirstUserLoggedIn = false;
		if (sendDrivingPlayerRemovedEvent)
		{
			GameEventManager.SendEvent(new DrivingPlayerRemovedEvent());
		}
	}

	public void SetFirstLogin()
	{
		FirstUserLoggedIn = true;
	}

	public int GetPlayerLocalNumberForAnimal(Character.Animals animal)
	{
		for (int i = 0; i < playerList.Length; i++)
		{
			Player player = playerList[i];
			if (player != null && player.AssociatedLobbyPlayer != null && player.AssociatedLobbyPlayer.CharacterInstance != null && player.AssociatedLobbyPlayer.CharacterInstance.CharacterSprite == animal)
			{
				return player.Number;
			}
		}
		return -1;
	}

	public IEnumerator GetEnumerator()
	{
		return playerList.GetEnumerator();
	}
}
