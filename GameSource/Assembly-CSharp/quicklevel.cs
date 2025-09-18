using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class quicklevel : MonoBehaviour
{
	public bool startNow;

	public GameState.LevelName targetLevel;

	public Character.Animals[] animalsToSelect;

	private IEnumerator performQuickStart;

	private void Start()
	{
	}

	private void Update()
	{
		if (performQuickStart != null)
		{
			if (!performQuickStart.MoveNext())
			{
				performQuickStart = null;
			}
		}
		else if (startNow)
		{
			startNow = false;
			performQuickStart = PerformQuickStart();
		}
	}

	private IEnumerator PerformQuickStart()
	{
		LevelSelectController levelSelectController = Object.FindObjectOfType<LevelSelectController>();
		if (!levelSelectController.HostIsLoaded)
		{
			Debug.Log("Waiting for treehouse to be loaded...");
		}
		while (!levelSelectController.HostIsLoaded)
		{
			yield return null;
		}
		if (!HaveLobbyPlayer())
		{
			Debug.Log("Waiting for player to be added...");
		}
		while (!HaveLobbyPlayer())
		{
			yield return null;
		}
		while (LobbyManager.instance.PlayerTracker.WaitingForIDs)
		{
			yield return null;
		}
		while (GameSettings.GetInstance().GameMode != GameState.GameMode.PARTY)
		{
			yield return null;
		}
		HotSeat hotSeat = Object.FindObjectOfType<HotSeat>();
		if (hotSeat == null)
		{
			Debug.LogError("Could not find hotseat, aborting.");
			yield break;
		}
		Dictionary<Character.Animals, Character> allChars = new Dictionary<Character.Animals, Character>();
		Character[] array = Object.FindObjectsOfType<Character>();
		foreach (Character character in array)
		{
			allChars.Add(character.CharacterSprite, character);
		}
		HashSet<LobbyCursor> usedCursors = new HashSet<LobbyCursor>();
		int i2 = 0;
		Character.Animals[] array2 = animalsToSelect;
		for (int j = 0; j < array2.Length; j++)
		{
			Character.Animals animal = array2[j];
			Debug.Log("Picking character " + animal);
			if (allChars.TryGetValue(animal, out var charToSelect) && !charToSelect.Picked)
			{
				LobbyCursor lobbyCur = GetRightmostFreeCursor(usedCursors);
				if (lobbyCur == null)
				{
					Debug.Log("Waiting for free cursor...");
				}
				while (lobbyCur == null)
				{
					yield return null;
					lobbyCur = GetRightmostFreeCursor(usedCursors);
				}
				usedCursors.Add(lobbyCur);
				if (!lobbyCur.Enabled)
				{
					Debug.Log("Waiting for lobby cursor to be enabled...");
				}
				while (!lobbyCur.Enabled)
				{
					yield return null;
				}
				Debug.Log("Sending character pick request...");
				lobbyCur.LocalPlayer.AssociatedLobbyPlayer.RequestPickCharacter(charToSelect);
				if (!charToSelect.Picked)
				{
					Debug.Log("Waiting for character to be picked...");
				}
				while (!charToSelect.Picked)
				{
					yield return null;
				}
				if (i2 != animalsToSelect.Length - 1)
				{
					Debug.Log("Sitting " + animal.ToString() + " on the couch.");
					SimulateSitPlayer(levelSelectController, hotSeat, lobbyCur.LocalPlayer.UseController, i2);
					while (lobbyCur.AssociatedLobbyPlayer.PlayerStatus != LobbyPlayer.Status.COUCH)
					{
						yield return null;
					}
				}
			}
			else
			{
				Debug.LogError("Could not select character " + animal.ToString() + " -- already picked?");
			}
			int i = i2 + 1;
			i2 = i;
			charToSelect = null;
		}
		Debug.Log("Launching level " + targetLevel);
		LevelPortal[] array3 = Object.FindObjectsOfType<LevelPortal>();
		foreach (LevelPortal levelPortal in array3)
		{
			if (levelPortal.TargetLevel == targetLevel)
			{
				levelSelectController.LaunchLevel(levelPortal);
				break;
			}
		}
	}

	private bool HaveLobbyPlayer()
	{
		NetworkLobbyPlayer[] lobbySlots = LobbyManager.instance.lobbySlots;
		foreach (NetworkLobbyPlayer networkLobbyPlayer in lobbySlots)
		{
			if (!(networkLobbyPlayer == null) && !networkLobbyPlayer.netId.IsEmpty())
			{
				return true;
			}
		}
		return false;
	}

	private LobbyCursor GetRightmostFreeCursor(HashSet<LobbyCursor> usedCursors)
	{
		LobbyPlayer lobbyPlayer = null;
		NetworkLobbyPlayer[] lobbySlots = LobbyManager.instance.lobbySlots;
		foreach (NetworkLobbyPlayer networkLobbyPlayer in lobbySlots)
		{
			if (!(networkLobbyPlayer == null))
			{
				LobbyPlayer component = networkLobbyPlayer.GetComponent<LobbyPlayer>();
				if (component.CursorInstance != null && component.CursorInstance.Enabled && !usedCursors.Contains((LobbyCursor)component.CursorInstance))
				{
					lobbyPlayer = component;
				}
			}
		}
		if (lobbyPlayer != null)
		{
			return (LobbyCursor)lobbyPlayer.CursorInstance;
		}
		return null;
	}

	private void SimulateSitPlayer(LevelSelectController levelSelectController, HotSeat HotSeatCouch, Controller playerController, int i)
	{
		Player player = PlayerManager.GetInstance().GetPlayer(i + 1);
		HotSeatCouch.SitPlayer(player);
		if (GameSettings.GetInstance().GameMode == GameState.GameMode.PARTY)
		{
			levelSelectController.PartyModeButton.SimulatePress();
		}
		levelSelectController.PartyModeButton.Lock();
		Debug.Log("Simulate Sit Player Locked the party Button");
		GameState.GetInstance().UsingHotSeat = true;
		levelSelectController.PlayerJoinIndicators[i].ReadyEnabled();
		player.AssociatedLobbyPlayer.PlayerStatus = LobbyPlayer.Status.COUCH;
		for (int j = 0; j != levelSelectController.JoinedPlayers.Length; j++)
		{
			if (levelSelectController.JoinedPlayers[j].PlayerStatus == LobbyPlayer.Status.INACTIVE)
			{
				PlayerManager.GetInstance().AddPlayer(playerController);
				playerController.AddPlayer(j + 1);
				break;
			}
		}
	}
}
