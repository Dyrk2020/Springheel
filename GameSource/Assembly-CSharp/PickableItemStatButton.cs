using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class PickableItemStatButton : PickableButton
{
	public enum ItemStatButtonJobs
	{
		PiecesPlaced,
		PiecesDestroyed,
		TrapsPlaced,
		BombsUsed,
		PiecesGlued,
		LargeContraptionsMade,
		TimesTeleported,
		SpringBounces,
		DeathsBySpikeBall,
		DeathsByBarbedWire,
		DeathsByArrow,
		DeathsByTennisBall,
		DeathsBySpinningSaw,
		DeathsByLinearSaw,
		DeathsByPropeller,
		DeathsByFlippingBlock,
		DeathsByBlackHole,
		DeathsByHockeyPuck,
		DeathsByPunchingPlant,
		coinsCollected,
		TrapPoints,
		DeathsByPressureTriggerSpikes,
		DeathsByWreckingBall
	}

	public ItemStatButtonJobs job;

	public Text labelText;

	protected override void Start()
	{
		base.Start();
	}

	protected override void Update()
	{
		base.Update();
		if (Visible && initialized)
		{
			SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
			switch (job)
			{
			case ItemStatButtonJobs.PiecesPlaced:
				buttonText.text = saveFileDataForMainUser.GetStat<StatCount>("PiecesPlaced").count.ToString();
				break;
			case ItemStatButtonJobs.PiecesDestroyed:
				buttonText.text = saveFileDataForMainUser.GetStat<StatCount>("PiecesDestroyed").count.ToString();
				break;
			case ItemStatButtonJobs.TrapsPlaced:
				buttonText.text = saveFileDataForMainUser.GetStat<StatCount>("TrapsPlaced").count.ToString();
				break;
			case ItemStatButtonJobs.BombsUsed:
				buttonText.text = saveFileDataForMainUser.GetStat<StatCount>("BombsPlaced").count.ToString();
				break;
			case ItemStatButtonJobs.PiecesGlued:
				buttonText.text = saveFileDataForMainUser.GetStat<StatCount>("PiecesGlued").count.ToString();
				break;
			case ItemStatButtonJobs.LargeContraptionsMade:
				buttonText.text = saveFileDataForMainUser.GetStat<StatCount>("LargeContraptionsMade").count.ToString();
				break;
			case ItemStatButtonJobs.TimesTeleported:
				buttonText.text = saveFileDataForMainUser.GetStat<StatCount>("TimesTeleported").count.ToString();
				break;
			case ItemStatButtonJobs.SpringBounces:
				buttonText.text = saveFileDataForMainUser.GetStat<StatCount>("SpringBounces").count.ToString();
				break;
			case ItemStatButtonJobs.DeathsBySpikeBall:
				buttonText.text = saveFileDataForMainUser.GetStat<StatCount>("DeathsBySpikeBall").count.ToString();
				break;
			case ItemStatButtonJobs.DeathsByBarbedWire:
				buttonText.text = saveFileDataForMainUser.GetStat<StatCount>("DeathsByBarbedWire").count.ToString();
				break;
			case ItemStatButtonJobs.DeathsByArrow:
				buttonText.text = saveFileDataForMainUser.GetStat<StatCount>("DeathsByArrow").count.ToString();
				break;
			case ItemStatButtonJobs.DeathsByTennisBall:
				buttonText.text = saveFileDataForMainUser.GetStat<StatCount>("DeathsByTennisBall").count.ToString();
				break;
			case ItemStatButtonJobs.DeathsBySpinningSaw:
				buttonText.text = saveFileDataForMainUser.GetStat<StatCount>("DeathsBySpinningSaw").count.ToString();
				break;
			case ItemStatButtonJobs.DeathsByLinearSaw:
				buttonText.text = saveFileDataForMainUser.GetStat<StatCount>("DeathsByLinearSaw").count.ToString();
				break;
			case ItemStatButtonJobs.DeathsByPropeller:
				buttonText.text = saveFileDataForMainUser.GetStat<StatCount>("DeathsByPropeller").count.ToString();
				break;
			case ItemStatButtonJobs.DeathsByFlippingBlock:
				buttonText.text = saveFileDataForMainUser.GetStat<StatCount>("DeathsByFlippingBlock").count.ToString();
				break;
			case ItemStatButtonJobs.DeathsByBlackHole:
				buttonText.text = saveFileDataForMainUser.GetStat<StatCount>("DeathsByBlackHole").count.ToString();
				break;
			case ItemStatButtonJobs.DeathsByHockeyPuck:
				buttonText.text = saveFileDataForMainUser.GetStat<StatCount>("DeathsByHockeyPuck").count.ToString();
				break;
			case ItemStatButtonJobs.DeathsByPunchingPlant:
				buttonText.text = saveFileDataForMainUser.GetStat<StatCount>("DeathsByPunchingPlant").count.ToString();
				break;
			case ItemStatButtonJobs.coinsCollected:
				buttonText.text = saveFileDataForMainUser.GetStat<StatCount>("CoinsCollected").count.ToString();
				break;
			case ItemStatButtonJobs.TrapPoints:
				buttonText.text = saveFileDataForMainUser.GetStat<StatCount>("TrapPointsEarned").count.ToString();
				break;
			case ItemStatButtonJobs.DeathsByPressureTriggerSpikes:
				buttonText.text = saveFileDataForMainUser.GetStat<StatCount>("DeathsByPressureTriggerSpikes").count.ToString();
				break;
			case ItemStatButtonJobs.DeathsByWreckingBall:
				buttonText.text = saveFileDataForMainUser.GetStat<StatCount>("DeathsByWreckingBall").count.ToString();
				break;
			}
		}
	}

	public override void Enable(bool onOff = true)
	{
		base.Enable(onOff);
		SetName();
	}

	protected void Show(bool show)
	{
		buttonText.enabled = show;
		Collider2D[] pickColliders = PickColliders;
		for (int i = 0; i < pickColliders.Length; i++)
		{
			pickColliders[i].enabled = show;
		}
		if ((bool)labelText)
		{
			labelText.enabled = show;
		}
	}

	public string SetName()
	{
		switch (job)
		{
		case ItemStatButtonJobs.PiecesPlaced:
			labelText.text = ScriptLocalization.Stats.PiecesPlaced;
			break;
		case ItemStatButtonJobs.PiecesDestroyed:
			labelText.text = ScriptLocalization.Stats.Pieces_Destroyed;
			break;
		case ItemStatButtonJobs.TrapsPlaced:
			labelText.text = ScriptLocalization.Stats.Traps_Placed;
			break;
		case ItemStatButtonJobs.BombsUsed:
			labelText.text = ScriptLocalization.Stats.BombsExploded;
			break;
		case ItemStatButtonJobs.PiecesGlued:
			labelText.text = ScriptLocalization.Stats.PiecesGlued;
			break;
		case ItemStatButtonJobs.LargeContraptionsMade:
			labelText.text = ScriptLocalization.Stats.LargeContraptions;
			break;
		case ItemStatButtonJobs.TimesTeleported:
			labelText.text = ScriptLocalization.Stats.Teleports;
			break;
		case ItemStatButtonJobs.SpringBounces:
			labelText.text = ScriptLocalization.Stats.Spring_Bounces;
			break;
		case ItemStatButtonJobs.DeathsBySpikeBall:
			labelText.text = ScriptLocalization.Stats.SpikeBall;
			break;
		case ItemStatButtonJobs.DeathsByBarbedWire:
			labelText.text = ScriptLocalization.Stats.Barbwire;
			break;
		case ItemStatButtonJobs.DeathsByArrow:
			labelText.text = ScriptLocalization.Stats.Arrow;
			break;
		case ItemStatButtonJobs.DeathsByTennisBall:
			labelText.text = ScriptLocalization.Stats.Flaming_Tennisball;
			break;
		case ItemStatButtonJobs.DeathsBySpinningSaw:
			labelText.text = ScriptLocalization.Stats.Spinning_Saw;
			break;
		case ItemStatButtonJobs.DeathsByLinearSaw:
			labelText.text = ScriptLocalization.Stats.Linear_Saw;
			break;
		case ItemStatButtonJobs.DeathsByPropeller:
			labelText.text = ScriptLocalization.Stats.Fan_Propeller;
			break;
		case ItemStatButtonJobs.DeathsByFlippingBlock:
			labelText.text = ScriptLocalization.Stats.Flipping_block;
			break;
		case ItemStatButtonJobs.DeathsByBlackHole:
			labelText.text = ScriptLocalization.Stats.Black_Hole;
			break;
		case ItemStatButtonJobs.DeathsByHockeyPuck:
			labelText.text = ScriptLocalization.Stats.HockeyPuck;
			break;
		case ItemStatButtonJobs.DeathsByPunchingPlant:
			labelText.text = ScriptLocalization.Stats.Punching_Plant;
			break;
		case ItemStatButtonJobs.coinsCollected:
			labelText.text = ScriptLocalization.Stats.CoinsCollect;
			break;
		case ItemStatButtonJobs.TrapPoints:
			labelText.text = ScriptLocalization.Stats.Trap_points;
			break;
		}
		return labelText.text;
	}
}
