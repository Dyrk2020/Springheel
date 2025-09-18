namespace GameEvent;

public class RoundCompleteEvent : GameEvent
{
	public readonly bool PointsAwarded;

	public RoundCompleteEvent(bool pointsAwarded)
	{
		PointsAwarded = pointsAwarded;
	}
}
