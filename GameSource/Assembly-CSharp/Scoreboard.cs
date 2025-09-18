using System;
using UnityEngine;

public class Scoreboard : MonoBehaviour
{
	public PlayerScore PlayerScorePrefab;

	public Transform[] ScorePositions = new Transform[4];

	protected PlayerScore[] Scores;

	public Sprite[] NameSprites = new Sprite[Enum.GetValues(typeof(Character.Animals)).Length];

	public Sprite[] IconSprites = new Sprite[Enum.GetValues(typeof(Character.Animals)).Length];

	private Animator anim;

	protected bool showing;

	private void Awake()
	{
		anim = GetComponent<Animator>();
		anim.SetBool("OnScreen", value: false);
	}

	private void Start()
	{
		SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
		PlayerScore[] scores = Scores;
		for (int i = 0; i < scores.Length; i++)
		{
			scores[i].Hide();
		}
	}

	private void Update()
	{
	}

	public void SetPlayerCount(int players)
	{
		if (players == 0 || players > 4)
		{
			Debug.LogError("ScoreBoard.SetPlayerCount: invalid number of players: " + players);
			return;
		}
		Scores = new PlayerScore[players];
		for (int i = 0; i != players; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(PlayerScorePrefab.gameObject, ScorePositions[i].position, Quaternion.identity);
			gameObject.transform.parent = base.transform;
			Scores[i] = gameObject.GetComponent<PlayerScore>();
		}
	}

	public void SetPlayerCharacter(int player, Character.Animals character)
	{
		if (player >= 0 && player <= 3)
		{
			int num = (int)(character - 1);
			PlayerScore obj = Scores[player];
			obj.Icon.sprite = IconSprites[num];
			obj.Name.sprite = NameSprites[num];
		}
	}

	public void IncrementPlayerScore(int player, int amount = 1)
	{
		if (player >= 0 && player <= 3)
		{
			Scores[player].IncrementScore(amount);
		}
	}

	public void SetPlayerScore(int player, int score)
	{
		if (player >= 0 && player <= 3)
		{
			Scores[player].SetScore(score);
		}
	}

	public int GetPlayerScore(int player)
	{
		if (player < 0 || player > 3)
		{
			return 0;
		}
		return Scores[player].GetScore();
	}

	public void SetMaxScore(int score)
	{
		PlayerScore[] scores = Scores;
		for (int i = 0; i < scores.Length; i++)
		{
			scores[i].SetMaxScore(score);
		}
	}

	public void Show()
	{
		if (!showing)
		{
			SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = true;
			}
			PlayerScore[] scores = Scores;
			for (int i = 0; i < scores.Length; i++)
			{
				scores[i].Show();
			}
			anim.SetBool("OnScreen", value: true);
			AkSoundEngine.PostEvent("UI_InGame_ResultSheet_Open", base.gameObject);
			showing = true;
		}
	}

	public void Hide()
	{
		if (showing)
		{
			anim.SetBool("OnScreen", value: false);
			AkSoundEngine.PostEvent("UI_InGame_ResultSheet_Close", base.gameObject);
			showing = false;
		}
	}
}
