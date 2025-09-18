namespace GameEvent;

public class CoinPickupEvent : GameEvent
{
	public readonly bool PickedUp;

	public CoinPickupEvent(bool pickedUp)
	{
		PickedUp = pickedUp;
	}
}
