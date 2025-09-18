namespace GameEvent;

public class ControllerControlsCamera : GameEvent
{
	public readonly int playerMaskNumber;

	public ControllerControlsCamera(int playerMaskNumber)
	{
		this.playerMaskNumber = playerMaskNumber;
	}
}
