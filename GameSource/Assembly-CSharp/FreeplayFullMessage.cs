using System;
using GameEvent;
using UnityEngine;
using UnityEngine.UI;

public class FreeplayFullMessage : MonoBehaviour, IGameEventListener
{
	public Text messageText;

	public Text percentText;

	public Text percentShadow;

	public Text fullText;

	public Text fullShadow;

	public GameObject textHolder;

	private int totalFullness;

	private string percentStr;

	private Color textColor;

	private float lastPct;

	private int opsSinceLastCount;

	private void Awake()
	{
		ChangeListener(adding: true);
		percentText.text = "0%";
		percentShadow.text = "0%";
	}

	private void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<PiecePlacedEvent>(this, adding);
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, adding);
		GameEventManager.ChangeListener<DestroyPieceEvent>(this, adding);
		GameEventManager.ChangeListener<StartPhaseEvent>(this, adding);
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(StartPhaseEvent))
		{
			textHolder.SetActive((e as StartPhaseEvent).Phase == GameControl.GamePhase.PLACE);
			return;
		}
		opsSinceLastCount++;
		if (opsSinceLastCount > 20)
		{
			QuickSaver component = LobbyManager.instance.CurrentGameController.GetComponent<QuickSaver>();
			totalFullness = component.CalculateLevelFullness();
			opsSinceLastCount = 0;
		}
		else
		{
			int num = 0;
			if (type == typeof(PiecePlacedEvent))
			{
				Placeable placedBlock = (e as PiecePlacedEvent).PlacedBlock;
				if (placedBlock.IsSaveable)
				{
					num = placedBlock.placementCost;
					MultipieceBlock multipieceBlock = placedBlock as MultipieceBlock;
					if (multipieceBlock != null)
					{
						MultipiecePart[] parts = multipieceBlock.Parts;
						foreach (MultipiecePart multipiecePart in parts)
						{
							num += multipiecePart.placementCost;
						}
					}
					foreach (Placeable childPiece in placedBlock.ChildPieces)
					{
						num += childPiece.placementCost;
					}
				}
			}
			else if (type == typeof(DestroyPieceEvent))
			{
				Placeable piece = (e as DestroyPieceEvent).Piece;
				if (piece.IsSaveable && !piece.PickedUp)
				{
					num = -piece.placementCost;
					foreach (Placeable childPiece2 in piece.ChildPieces)
					{
						num -= childPiece2.placementCost;
					}
				}
			}
			else if (type == typeof(NetworkMessageReceivedEvent))
			{
				NetworkMessageReceivedEvent networkMessageReceivedEvent = e as NetworkMessageReceivedEvent;
				if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.PiecePickedUp)
				{
					MsgPiecePickedUp msgPiecePickedUp = networkMessageReceivedEvent.ReadMessage as MsgPiecePickedUp;
					Placeable placeable = null;
					if (msgPiecePickedUp.PieceID != 0)
					{
						foreach (Placeable allPlaceable in Placeable.AllPlaceables)
						{
							if (allPlaceable != null && allPlaceable.ID == msgPiecePickedUp.PieceID)
							{
								placeable = allPlaceable;
								break;
							}
						}
					}
					if (placeable != null)
					{
						num -= placeable.placementCost;
						foreach (Placeable childPiece3 in placeable.ChildPieces)
						{
							num -= childPiece3.placementCost;
						}
					}
				}
			}
			totalFullness += num;
		}
		int levelFullnessScoreLimit = GameSettings.GetInstance().LevelFullnessScoreLimit;
		int num2 = 100;
		if (levelFullnessScoreLimit > 0)
		{
			num2 = totalFullness * 100 / levelFullnessScoreLimit;
		}
		percentStr = num2 + "%";
		percentText.text = percentStr;
		percentShadow.text = percentStr;
		textColor = Color.white;
		if (num2 >= 100)
		{
			textColor = Color.red;
			fullText.color = textColor;
			fullText.SetAlpha(1f);
			fullShadow.SetAlpha(1f);
		}
		else if (num2 >= 75)
		{
			textColor = GameSettings.GetInstance().SystemAlertColor;
		}
		if (lastPct >= 100f && num2 < 100)
		{
			fullText.SetAlpha(0f);
			fullShadow.SetAlpha(0f);
		}
		messageText.color = textColor;
		percentText.color = textColor;
		lastPct = num2;
	}
}
