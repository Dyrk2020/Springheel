using System;
using System.Collections.Generic;

public class SyncedRandom
{
	private Random _random;

	public SyncedRandom(int seed)
	{
		_random = new Random(seed);
	}

	public float Range(float min, float max)
	{
		return (float)_random.NextDouble() * (max - min) + min;
	}

	public int Range(int min, int max)
	{
		return _random.Next(min, max);
	}

	public void ShuffleList<T>(List<T> list)
	{
		int num = list.Count;
		while (num > 1)
		{
			num--;
			int num2 = _random.Next(num + 1);
			int index = num2;
			int index2 = num;
			T val = list[num];
			T val2 = list[num2];
			T val3 = (list[index] = val);
			val3 = (list[index2] = val2);
		}
	}
}
