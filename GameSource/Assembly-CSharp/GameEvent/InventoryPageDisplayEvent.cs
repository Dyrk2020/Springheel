namespace GameEvent;

public class InventoryPageDisplayEvent : GameEvent
{
	public int pageNumber;

	public InventoryPageDisplayEvent(int pageNumber)
	{
		this.pageNumber = pageNumber;
	}
}
