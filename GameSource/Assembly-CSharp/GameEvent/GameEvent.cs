namespace GameEvent;

public abstract class GameEvent
{
	public bool Sent { get; internal set; }

	public bool Resolved { get; internal set; }

	public override string ToString()
	{
		return GetType().ToString();
	}

	public void Resolve()
	{
		Resolved = true;
	}
}
