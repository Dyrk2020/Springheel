using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphScoreBoard : BaseScoreboard
{
	private const int maxCachedScorePieces = 30;

	public ScorePiece scorePieceWin;

	public ScorePiece scorePieceWinSolo;

	public ScorePiece scorePieceKill;

	public ScorePiece scorePieceComeback;

	public ScorePiece scorePieceFirst;

	public ScorePiece scorePieceCoin;

	public ScorePiece scorePieceWinDead;

	public ScorePiece scorePieceSecond;

	public ScorePiece scorePieceThird;

	public ScorePiece scorePieceFourth;

	public Dictionary<ScorePiece, List<GameObject>> preinstantiatedScorePieces = new Dictionary<ScorePiece, List<GameObject>>();

	public Transform scorePiecePool;

	public ScoreLine scoreLinePrefab;

	protected ScoreLine[] playerScoreLines;

	public RectTransform mainParent;

	public RectTransform[] ScorePositions = new RectTransform[4];

	public Text CustomLevelText;

	private Dictionary<int, ScoreLine> scorelineRelation = new Dictionary<int, ScoreLine>();

	private float drawTypeTime = 0.5f;

	private float drawIndividualBlockTime = 0.5f;

	[Header("GridStuff")]
	public GameObject SolidLinePrefab;

	public GameObject DashedLinePrefab;

	public GameObject GridHolder;

	public GameObject TargetGoalDistanc;

	public GameObject ExtendPaperDistance;

	public Animator PaperExtendor;

	protected override void Start()
	{
		base.Start();
		SetupGridLines();
		ExtendScoreboard(extend: false);
		PreinstantiateScorePieces();
	}

	protected override void Update()
	{
		base.Update();
		if (timed && actuallyVisible && !DrawingScore)
		{
			scoreTimer -= Time.unscaledDeltaTime;
			if (scoreTimer <= 0f)
			{
				Hide(afterTally: true);
			}
		}
	}

	public void SetupGridLines()
	{
		int num = GameSettings.GetInstance().DefaultRuleset.PointTypeValue(PointBlock.pointBlockType.win);
		int num2 = GameSettings.GetInstance().MaxScore / num;
		float num3 = TargetGoalDistanc.transform.localPosition.x / (float)num2;
		SizeFactor = num3 / (float)num;
		for (int i = 0; i <= num2; i++)
		{
			GameObject gameObject;
			if (i == 0 || i == num2)
			{
				gameObject = Object.Instantiate(SolidLinePrefab, GridHolder.transform.position, GridHolder.transform.rotation);
			}
			else
			{
				gameObject = Object.Instantiate(DashedLinePrefab, GridHolder.transform.position, GridHolder.transform.rotation);
				Text componentInChildren = gameObject.GetComponentInChildren<Text>();
				if (componentInChildren != null)
				{
					componentInChildren.text = i.ToString();
					componentInChildren.transform.localScale = new Vector3(0.5f, 1f, 1f);
				}
			}
			gameObject.transform.SetParent(GridHolder.transform);
			gameObject.transform.localScale = Vector3.one * 2f;
			gameObject.transform.localPosition = new Vector3(num3 * (float)i, 0f, 0f);
			if (i == num2)
			{
				gameObject.transform.localPosition = new Vector3(num3 * (float)i - num3 / (float)(4 * num2), 0f, 0f);
			}
			else
			{
				gameObject.transform.localPosition = new Vector3(num3 * (float)i, 0f, 0f);
			}
		}
	}

	public void SetPlayerCharacter(int order, Character.Animals character, bool altSkin, LobbyPlayer lobbyPl, int handicap)
	{
		if (order >= 0 && order <= 3)
		{
			ScoreLine scoreLine = playerScoreLines[order];
			scoreLine.Animal = character;
			scoreLine.animalImage.sprite = CharacterSpriteManager.GetInstance().GetCharaterAliveIcon(character);
			scoreLine.SetHandicap(handicap);
			if (!LobbyManager.instance.IsInOnlineGame)
			{
				scoreLine.UseAnimalName(Character.GetLocalizedAnimal(character, altSkin));
			}
			else
			{
				scoreLine.UsePlayerName(lobbyPl);
			}
			if (!scorelineRelation.ContainsKey(lobbyPl.networkNumber))
			{
				scorelineRelation.Add(lobbyPl.networkNumber, scoreLine);
			}
			else
			{
				Debug.Log("Player number " + lobbyPl.networkNumber + " already exists in scoreboard");
			}
		}
	}

	public void MarkPlayerDisconnected(Character.Animals animal)
	{
		for (int i = 0; i != playerScoreLines.Length; i++)
		{
			if (playerScoreLines[i].Animal == animal)
			{
				playerScoreLines[i].SetDisconnected(disconnect: true);
			}
		}
	}

	public void SetPlayerCount(int numberPlayers)
	{
		if (numberPlayers == 0 || numberPlayers > 4)
		{
			Debug.LogError("GraphScoreBoard.SetPlayerCount: invalid number of players: " + numberPlayers);
		}
		playerScoreLines = new ScoreLine[numberPlayers];
		for (int i = 0; i != numberPlayers; i++)
		{
			GameObject gameObject = Object.Instantiate(scoreLinePrefab.gameObject, ScorePositions[i].position, Quaternion.identity);
			gameObject.transform.SetParent(mainParent);
			gameObject.transform.localScale = Vector3.one;
			playerScoreLines[i] = gameObject.GetComponent<ScoreLine>();
			playerScoreLines[i].scoreBoardParent = this;
		}
	}

	public void displayNewScore(List<PointBlock> pointBlockList)
	{
		StartCoroutine(distributeAllPoints(pointBlockList));
	}

	public void ShowScoreLineTextbacking(bool show)
	{
		for (int i = 0; i != playerScoreLines.Length; i++)
		{
			if (playerScoreLines[i] != null)
			{
				playerScoreLines[i].ShowtextBacking(show);
			}
		}
	}

	private IEnumerator distributeAllPoints(List<PointBlock> pointBlockList)
	{
		DrawingScore = true;
		List<PointBlock> pointsToDistribute = new List<PointBlock>(pointBlockList);
		pointsToDistribute.Sort((PointBlock x, PointBlock y) => x.type.CompareTo(y.type));
		PointBlock.pointBlockType currenttype = PointBlock.pointBlockType.win;
		do
		{
			yield return null;
		}
		while (!actuallyVisible);
		int totalCoinPoints = 0;
		foreach (PointBlock item in pointsToDistribute)
		{
			if (item.type == PointBlock.pointBlockType.coin)
			{
				totalCoinPoints++;
			}
		}
		int currentCoinPoint = 0;
		foreach (PointBlock pb in pointsToDistribute)
		{
			if (pb.type == currenttype)
			{
				switch (pb.type)
				{
				case PointBlock.pointBlockType.coin:
					if (!Skip)
					{
						float a = drawIndividualBlockTime;
						float num = drawIndividualBlockTime / 10f;
						int num2 = 5;
						if (totalCoinPoints > 100)
						{
							num /= 2f;
							num2 *= 2;
						}
						float seconds = num;
						if (currentCoinPoint < num2)
						{
							float t = (float)currentCoinPoint / (float)num2;
							seconds = Mathf.Lerp(a, num, t);
						}
						yield return new WaitForSeconds(seconds);
					}
					currentCoinPoint++;
					break;
				case PointBlock.pointBlockType.trap:
				case PointBlock.pointBlockType.suicide:
				case PointBlock.pointBlockType.comeback:
					if (!Skip)
					{
						yield return new WaitForSeconds(drawIndividualBlockTime);
					}
					break;
				}
			}
			else if (!Skip)
			{
				yield return new WaitForSeconds(drawTypeTime);
			}
			if (scorelineRelation.ContainsKey(pb.playerNumber))
			{
				if (!scorelineRelation[pb.playerNumber].Disconnected)
				{
					scorelineRelation[pb.playerNumber].AddScorePointBlock(pb);
				}
			}
			else
			{
				Debug.Log("PointBlock Error, player number: " + pb.playerNumber);
			}
			currenttype = pb.type;
		}
		foreach (int key in scorelineRelation.Keys)
		{
			if (ScoreKeeper.Instance.IsPlayerInLoseStreak(key))
			{
				scorelineRelation[key].ActivateComeback();
			}
			else
			{
				scorelineRelation[key].DeActivateComeback();
			}
		}
		yield return new WaitForSeconds(drawTypeTime);
		ScoreLine[] array = playerScoreLines;
		for (int num3 = 0; num3 < array.Length; num3++)
		{
			array[num3].clearLastNewScorePiece();
		}
		Skip = false;
		DrawingScore = false;
		foreach (SaveFileData activeUserSaveFileData in StatTracker.Instance.GetActiveUserSaveFileDatas())
		{
			AchievementChecker.Instance.Point_AchievementChecks(activeUserSaveFileData);
		}
	}

	public ScorePiece getPointBlockPiece(PointBlock pb)
	{
		return pb.type switch
		{
			PointBlock.pointBlockType.win => scorePieceWin, 
			PointBlock.pointBlockType.soloWin => scorePieceWinSolo, 
			PointBlock.pointBlockType.comeback => scorePieceComeback, 
			PointBlock.pointBlockType.trap => scorePieceKill, 
			PointBlock.pointBlockType.first => scorePieceFirst, 
			PointBlock.pointBlockType.coin => scorePieceCoin, 
			PointBlock.pointBlockType.winDead => scorePieceWinDead, 
			PointBlock.pointBlockType.second => scorePieceSecond, 
			PointBlock.pointBlockType.third => scorePieceThird, 
			PointBlock.pointBlockType.fourth => scorePieceFourth, 
			_ => null, 
		};
	}

	public void ExtendScoreboard(bool extend = true)
	{
		PaperExtendor.SetBool("Out", extend);
	}

	public void Show(float time, bool GameShowing = true)
	{
		Show(GameShowing);
		scoreTimer = time;
		timed = time > 0f;
		ShowScoreLineTextbacking(GameShowing);
	}

	private void PreinstantiateScorePieces()
	{
		preinstantiatedScorePieces.Add(scorePieceWin, new List<GameObject>());
		preinstantiatedScorePieces.Add(scorePieceWinSolo, new List<GameObject>());
		preinstantiatedScorePieces.Add(scorePieceKill, new List<GameObject>());
		preinstantiatedScorePieces.Add(scorePieceComeback, new List<GameObject>());
		preinstantiatedScorePieces.Add(scorePieceFirst, new List<GameObject>());
		preinstantiatedScorePieces.Add(scorePieceCoin, new List<GameObject>());
		preinstantiatedScorePieces.Add(scorePieceWinDead, new List<GameObject>());
		preinstantiatedScorePieces.Add(scorePieceSecond, new List<GameObject>());
		preinstantiatedScorePieces.Add(scorePieceThird, new List<GameObject>());
		preinstantiatedScorePieces.Add(scorePieceFourth, new List<GameObject>());
		foreach (KeyValuePair<ScorePiece, List<GameObject>> preinstantiatedScorePiece in preinstantiatedScorePieces)
		{
			PreinstantiateScorePiece(preinstantiatedScorePiece.Key, 30);
		}
	}

	private void PreinstantiateScorePiece(ScorePiece piece, int amount)
	{
		List<GameObject> list = preinstantiatedScorePieces[piece];
		for (int i = 0; i < amount; i++)
		{
			GameObject gameObject = Object.Instantiate(piece.gameObject);
			gameObject.transform.SetParent(scorePiecePool);
			gameObject.SetActive(value: false);
			list.Add(gameObject);
		}
	}

	public ScorePiece GetPreinstantiatedPointBlock(PointBlock pb)
	{
		ScorePiece pointBlockPiece = getPointBlockPiece(pb);
		if (pointBlockPiece != null)
		{
			if (preinstantiatedScorePieces.TryGetValue(pointBlockPiece, out var value))
			{
				if (value.Count == 0)
				{
					PreinstantiateScorePiece(pointBlockPiece, 30);
				}
				if (value.Count > 0)
				{
					GameObject obj = value[value.Count - 1];
					value.RemoveAt(value.Count - 1);
					obj.SetActive(value: true);
					return obj.GetComponent<ScorePiece>();
				}
				return null;
			}
			return null;
		}
		return null;
	}

	public void ShowCustomLevelText(string text)
	{
		CustomLevelText.enabled = true;
		CustomLevelText.text = text;
	}

	public void HideCustomLevelText()
	{
		CustomLevelText.enabled = false;
	}
}
