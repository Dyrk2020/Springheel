using System.Collections.Generic;

namespace GameEvent;

public class GameResultsEvent : GameEvent
{
	public readonly IDictionary<GamePlayer, int> PlayerScores;

	public GameResultsEvent(IDictionary<GamePlayer, int> playerScores)
	{
		PlayerScores = playerScores;
	}
}
