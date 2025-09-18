using System.Collections.Generic;

public static class Util_ListT
{
	public static void SwapRemove<T>(this List<T> list, int idx)
	{
		int num = list.Count - 1;
		if (idx == 0 && num == -1)
		{
			list.Clear();
			return;
		}
		if (num != idx)
		{
			list[idx] = list[num];
		}
		list.RemoveAt(num);
	}
}
