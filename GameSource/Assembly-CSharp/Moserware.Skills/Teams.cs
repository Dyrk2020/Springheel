using System.Collections.Generic;
using System.Linq;

namespace Moserware.Skills;

public static class Teams
{
	public static IEnumerable<IDictionary<TPlayer, Rating>> Concat<TPlayer>(params Team<TPlayer>[] teams)
	{
		return teams.Select((Team<TPlayer> t) => t.AsDictionary());
	}
}
