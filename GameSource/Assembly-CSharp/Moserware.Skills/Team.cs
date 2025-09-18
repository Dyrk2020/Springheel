using System.Collections.Generic;

namespace Moserware.Skills;

public class Team<TPlayer>
{
	private readonly Dictionary<TPlayer, Rating> _PlayerRatings = new Dictionary<TPlayer, Rating>();

	public Team()
	{
	}

	public Team(TPlayer player, Rating rating)
	{
		AddPlayer(player, rating);
	}

	public Team<TPlayer> AddPlayer(TPlayer player, Rating rating)
	{
		_PlayerRatings[player] = rating;
		return this;
	}

	public IDictionary<TPlayer, Rating> AsDictionary()
	{
		return _PlayerRatings;
	}
}
public class Team : Team<Player>
{
	public Team()
	{
	}

	public Team(Player player, Rating rating)
		: base(player, rating)
	{
	}
}
