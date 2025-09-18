namespace GameEvent;

public interface IGameEventListener
{
	void handleEvent(GameEvent e);
}
