using System.Collections.Generic;
using UnityEngine;

public class LivesDisplayController : MonoBehaviour
{
	public GameControl gameControl;

	public List<LivesDisplayBox> livesDisplayBoxes = new List<LivesDisplayBox>();

	public void Initialize()
	{
		foreach (LivesDisplayBox livesDisplayBox in livesDisplayBoxes)
		{
			livesDisplayBox.SetNumLives(0, forceUpdate: true);
		}
	}

	public void SetPlayerLives(int networkNumber, int numLives)
	{
		livesDisplayBoxes[networkNumber - 1].SetNumLives(numLives);
	}

	public void SetPlayerAnimal(int networkNumber, Character.Animals animal)
	{
		livesDisplayBoxes[networkNumber - 1].Initialize(animal);
	}

	public void SetCanRespawn(int networkNumber, bool canRespawn)
	{
		livesDisplayBoxes[networkNumber - 1].CanRespawn = canRespawn;
	}

	public void SetRespawnButtonFill(int networkNumber, float fillAmount)
	{
		livesDisplayBoxes[networkNumber - 1].FillRespawnButton(fillAmount);
	}

	public void SetLocalController(int networkNumber, Controller controller)
	{
		livesDisplayBoxes[networkNumber - 1].SetLocalController(controller);
	}

	public void OnStartNewMatch()
	{
		GameSettings instance = GameSettings.GetInstance();
		int num = 0;
		switch (instance.respawnMode)
		{
		case RespawnMode.Off:
			return;
		case RespawnMode.LivesPerRound:
		case RespawnMode.RespawnsPerRound:
		case RespawnMode.RespawnsPerMatch:
			num = instance.numRespawns;
			break;
		}
		foreach (GamePlayer item in gameControl.CurrentPlayerQueue)
		{
			item.lives = num;
			SetPlayerAnimal(item.networkNumber, item.PickedAnimal);
			SetPlayerLives(item.networkNumber, num);
		}
	}

	public void OnStartNewRound()
	{
		foreach (LivesDisplayBox livesDisplayBox in livesDisplayBoxes)
		{
			livesDisplayBox.CanRespawn = false;
		}
		GameSettings instance = GameSettings.GetInstance();
		int num = 0;
		switch (instance.respawnMode)
		{
		case RespawnMode.Off:
			return;
		case RespawnMode.LivesPerRound:
		case RespawnMode.RespawnsPerRound:
			num = instance.numRespawns;
			break;
		case RespawnMode.RespawnsPerMatch:
			return;
		}
		foreach (GamePlayer item in gameControl.CurrentPlayerQueue)
		{
			item.lives = num;
			SetPlayerLives(item.networkNumber, num);
		}
	}
}
