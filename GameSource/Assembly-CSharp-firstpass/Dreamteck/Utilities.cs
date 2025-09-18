using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dreamteck;

public static class Utilities
{
	public static T SerializableClone<T>(this T obj)
	{
		return JsonUtility.FromJson<T>(JsonUtility.ToJson(obj));
	}

	public static void Shuffle<T>(this IList<T> list)
	{
		int num = list.Count;
		while (num > 1)
		{
			num--;
			int left = UnityEngine.Random.Range(0, num + 1);
			list.Swap(left, num);
		}
	}

	public static void RemoveAtUnsorted<T>(this List<T> list, int i)
	{
		int index = list.Count - 1;
		list[i--] = list[index];
		list.RemoveAt(index);
	}

	public static T PopLast<T>(this IList<T> list)
	{
		T result = list[list.Count - 1];
		list.RemoveAt(list.Count - 1);
		return result;
	}

	public static void Swap<T>(this IList<T> list, int left, int right)
	{
		T value = list[left];
		list[left] = list[right];
		list[right] = value;
	}

	public static void SafeInvoke(this Delegate del, params object[] parameters)
	{
		Delegate[] invocationList = del.GetInvocationList();
		foreach (Delegate obj in invocationList)
		{
			try
			{
				obj.Method.Invoke(obj.Target, parameters);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	public static T PopRandom<T>(this List<T> list)
	{
		if (list.Count > 0)
		{
			int index = UnityEngine.Random.Range(0, list.Count);
			T result = list[index];
			list.RemoveAt(index);
			return result;
		}
		throw new ArgumentException("Attempting to remove an element from an empty list");
	}
}
