using System.Collections;

public class CoinIdol : Coin
{
	public Boulder BoulderTrap;

	private bool triggered;

	public override void Reset()
	{
		base.Reset();
		triggered = false;
		awarded = false;
	}

	protected override void Pickup(Character chr)
	{
		base.Pickup(chr);
		if (!triggered)
		{
			BoulderTrap.TriggerTrap();
			triggered = true;
		}
	}

	protected override IEnumerator returnCoinToSpawn()
	{
		yield return base.returnCoinToSpawn();
		Reset();
		BoulderTrap.Reset();
	}
}
