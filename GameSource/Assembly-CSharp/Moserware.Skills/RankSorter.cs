using System.Collections.Generic;
using System.Linq;

namespace Moserware.Skills;

internal static class RankSorter
{
	public static void Sort<T>(ref IEnumerable<T> teams, ref int[] teamRanks)
	{
		Guard.ArgumentNotNull(teams, "teams");
		Guard.ArgumentNotNull(teamRanks, "teamRanks");
		int num = 0;
		bool flag = false;
		int[] array = teamRanks;
		foreach (int num2 in array)
		{
			if (num2 < num)
			{
				flag = true;
				break;
			}
			num = num2;
		}
		if (!flag)
		{
			return;
		}
		List<T> list = teams.ToList();
		Dictionary<T, int> dictionary = new Dictionary<T, int>();
		for (int j = 0; j < list.Count; j++)
		{
			T key = list[j];
			int value = teamRanks[j];
			dictionary[key] = value;
		}
		T[] array2 = new T[teamRanks.Length];
		int[] array3 = new int[teamRanks.Length];
		int num3 = 0;
		foreach (KeyValuePair<T, int> item in dictionary.OrderBy((KeyValuePair<T, int> pair) => pair.Value))
		{
			array2[num3] = item.Key;
			array3[num3++] = item.Value;
		}
		teams = array2;
		teamRanks = array3;
	}
}
