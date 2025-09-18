using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreLine : MonoBehaviour
{
	public Vector3 NextAttachPosition;

	public GraphScoreBoard scoreBoardParent;

	protected bool drawing;

	protected ScorePiece lastNewScorepiece;

	public bool LockedAfterScoring;

	public bool Disconnected;

	public Character.Animals Animal;

	public Image animalImage;

	public Text animalName;

	public UGCNameTag nameTag;

	public Text comeBack;

	public Text disconnected;

	public Text handicapText;

	public int handicap = 100;

	public Image Textbacking;

	private List<ScorePiece> scorePieces = new List<ScorePiece>();

	public Color ImageConnectedColor;

	public Color LongNameImageConnectedColor;

	public Color ImageDisconnectedColor;

	public int LongNameThreshold = 8;

	private void Start()
	{
		DeActivateComeback();
		SetDisconnected(disconnect: false);
	}

	public void AddScorePointBlock(PointBlock pb)
	{
		if (pb.type == PointBlock.pointBlockType.suicide)
		{
			if (!(lastNewScorepiece != null) || lastNewScorepiece.pointBlockType != PointBlock.pointBlockType.trap)
			{
				return;
			}
			NextAttachPosition -= new Vector3(lastNewScorepiece.width * scoreBoardParent.SizeFactor, 0f, 0f);
		}
		ScorePiece preinstantiatedPointBlock = scoreBoardParent.GetPreinstantiatedPointBlock(pb);
		preinstantiatedPointBlock.transform.SetParent(base.transform);
		preinstantiatedPointBlock.transform.localScale = Vector3.one;
		preinstantiatedPointBlock.transform.localPosition = NextAttachPosition;
		if ((NextAttachPosition + new Vector3(preinstantiatedPointBlock.width * scoreBoardParent.SizeFactor, 0f, 0f)).x > scoreBoardParent.ExtendPaperDistance.transform.localPosition.x)
		{
			scoreBoardParent.ExtendScoreboard();
		}
		preinstantiatedPointBlock.setImageWidth(scoreBoardParent.SizeFactor * (float)handicap / 100f);
		if (pb.type != PointBlock.pointBlockType.suicide)
		{
			NextAttachPosition += new Vector3(preinstantiatedPointBlock.width * scoreBoardParent.SizeFactor * (float)handicap / 100f, 0f, 0f);
		}
		else
		{
			preinstantiatedPointBlock.suicideblock = lastNewScorepiece;
		}
		preinstantiatedPointBlock.animate();
		preinstantiatedPointBlock.pointBlockType = pb.type;
		lastNewScorepiece = preinstantiatedPointBlock;
		if (scoreBoardParent.Skip)
		{
			preinstantiatedPointBlock.text.enabled = false;
		}
		scorePieces.Add(preinstantiatedPointBlock);
	}

	private IEnumerator DrawScorePiece()
	{
		yield return null;
	}

	public void ActivateComeback()
	{
		if (GameSettings.GetInstance().PointTypeEnabled(PointBlock.pointBlockType.comeback))
		{
			comeBack.enabled = true;
		}
	}

	public void DeActivateComeback()
	{
		comeBack.enabled = false;
	}

	public void SetHandicap(int newHandicap)
	{
		handicap = newHandicap;
		if (handicap < 100)
		{
			handicapText.text = "( " + handicap + "% )";
		}
		else
		{
			handicapText.text = "";
		}
	}

	public void SetDisconnected(bool disconnect)
	{
		Disconnected = disconnect;
		disconnected.enabled = disconnect;
		Color color = animalImage.color;
		if (disconnect)
		{
			color.a = 0.5f;
			animalImage.color = ImageDisconnectedColor;
			color = animalName.color;
			color.a = 0.5f;
			animalName.color = color;
			nameTag.SetColor(color);
			color = handicapText.color;
			color.a = 0.5f;
			handicapText.color = color;
			DeActivateComeback();
			if (scorePieces == null)
			{
				return;
			}
			{
				foreach (ScorePiece scorePiece in scorePieces)
				{
					color = scorePiece.pieceImage.color;
					color.a = 0.5f;
					scorePiece.pieceImage.color = color;
				}
				return;
			}
		}
		if (nameTag != null && nameTag.name != null && nameTag.playerNameUncensored.Length > LongNameThreshold)
		{
			animalImage.color = LongNameImageConnectedColor;
		}
		else
		{
			animalImage.color = ImageConnectedColor;
		}
		color = animalName.color;
		color.a = 1f;
		animalName.color = color;
		nameTag.SetColor(color);
		color = handicapText.color;
		color.a = 1f;
		handicapText.color = color;
		if (scorePieces == null)
		{
			return;
		}
		foreach (ScorePiece scorePiece2 in scorePieces)
		{
			color = scorePiece2.pieceImage.color;
			color.a = 1f;
			scorePiece2.pieceImage.color = color;
		}
	}

	public void clearLastNewScorePiece()
	{
		lastNewScorepiece = null;
	}

	public void UseAnimalName(string name)
	{
		animalName.gameObject.SetActive(value: true);
		nameTag.gameObject.SetActive(value: false);
		animalName.text = name;
	}

	public void UsePlayerName(LobbyPlayer lobbyPl)
	{
		animalName.gameObject.SetActive(value: false);
		nameTag.gameObject.SetActive(value: true);
		nameTag.Initialize(lobbyPl.playerName, lobbyPl.platformUniqueID, lobbyPl.GSID, lobbyPl.platform, isAnonymous: false);
		if (lobbyPl.playerName.Length > LongNameThreshold)
		{
			animalImage.color = LongNameImageConnectedColor;
		}
		else
		{
			animalImage.color = ImageConnectedColor;
		}
	}

	public void ShowtextBacking(bool show)
	{
		if (Textbacking != null)
		{
			Textbacking.enabled = show;
		}
	}
}
