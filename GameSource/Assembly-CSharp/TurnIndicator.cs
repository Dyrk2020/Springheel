using System;
using UnityEngine;

public class TurnIndicator : MonoBehaviour
{
	public CharacterPortrait PortraitPrefab;

	public Sprite[] BuildSprites = new Sprite[Enum.GetValues(typeof(Character.Animals)).Length];

	public Sprite[] RunSprites = new Sprite[Enum.GetValues(typeof(Character.Animals)).Length];

	public Sprite[] NameSprites = new Sprite[Enum.GetValues(typeof(Character.Animals)).Length];

	public CharacterPortrait[] Portraits;

	public Transform[] PortraitPositions = new Transform[4];

	private int positionOffset;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void SetPlayerCount(int players)
	{
		if (players == 0 || players > 4)
		{
			Debug.LogError("TurnIndicator.SetPlayerCount: invalid number of players: " + players);
			return;
		}
		Portraits = new CharacterPortrait[players];
		for (int i = 0; i != players; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(PortraitPrefab.gameObject, PortraitPositions[i].position, Quaternion.identity);
			gameObject.transform.parent = base.transform;
			if (i > 0)
			{
				gameObject.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
			}
			Portraits[i] = gameObject.GetComponent<CharacterPortrait>();
		}
	}

	public void SetPlayerCharacter(int player, Character.Animals character)
	{
		if (player >= 0 && player <= 3)
		{
			int num = (int)(character - 1);
			CharacterPortrait characterPortrait = Portraits[player];
			characterPortrait.Icons = new Sprite[2];
			characterPortrait.Icons[0] = BuildSprites[num];
			characterPortrait.Icons[1] = RunSprites[num];
			characterPortrait.Icon.sprite = characterPortrait.Icons[characterPortrait.CurrentPhase];
			characterPortrait.Name.sprite = NameSprites[num];
		}
	}

	public void NextTurn()
	{
		Debug.Log("Going to next turn");
		positionOffset = (positionOffset + Portraits.Length - 1) % Portraits.Length;
		for (int i = 0; i != Portraits.Length; i++)
		{
			int num = (i + positionOffset) % Portraits.Length;
			CharacterPortrait characterPortrait = Portraits[i];
			characterPortrait.transform.position = PortraitPositions[num].position;
			if (num == 0)
			{
				characterPortrait.transform.localScale = new Vector3(1f, 1f, 1f);
			}
			else
			{
				characterPortrait.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
			}
		}
	}

	public void SwapPhase(int player)
	{
		Debug.Log("Swapping phase for " + player);
		if (player >= 0 && player <= 3)
		{
			Portraits[player].SwapPhase();
		}
	}

	public void SetWinning(int player, bool winning)
	{
		Portraits[player].Winning = winning;
	}
}
