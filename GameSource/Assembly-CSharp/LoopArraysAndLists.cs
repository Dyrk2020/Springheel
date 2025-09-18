using System.Collections.Generic;
using UnityEngine;

public static class LoopArraysAndLists
{
	public static int LoopID<T>(this List<T> list, int i)
	{
		if (i >= list.Count)
		{
			return i % list.Count;
		}
		if (i < 0)
		{
			return list.Count * (Mathf.Abs(i + 1) / list.Count + 1) + i;
		}
		return i;
	}

	public static T Loop<T>(this List<T> list, int i)
	{
		if (i >= list.Count)
		{
			return list[i % list.Count];
		}
		if (i < 0)
		{
			return list[list.Count * (Mathf.Abs(i + 1) / list.Count + 1) + i];
		}
		return list[i];
	}

	public static int LoopID<T>(this T[] array, int i)
	{
		if (i >= array.Length)
		{
			return i % array.Length;
		}
		if (i < 0)
		{
			return array.Length * (Mathf.Abs(i + 1) / array.Length + 1) + i;
		}
		return i;
	}

	public static T Loop<T>(this T[] array, int i)
	{
		if (i >= array.Length)
		{
			return array[i % array.Length];
		}
		if (i < 0)
		{
			return array[array.Length * (Mathf.Abs(i + 1) / array.Length + 1) + i];
		}
		return array[i];
	}
}
