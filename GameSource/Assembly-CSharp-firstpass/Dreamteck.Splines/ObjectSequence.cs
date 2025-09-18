using System;
using UnityEngine;

namespace Dreamteck.Splines;

[Serializable]
public class ObjectSequence<T>
{
	public enum Iteration
	{
		Ordered,
		Random
	}

	public T startObject;

	public T endObject;

	public T[] objects;

	public Iteration iteration;

	[HideInInspector]
	[SerializeField]
	private int _randomSeed = 1;

	[HideInInspector]
	[SerializeField]
	private int index;

	[HideInInspector]
	[SerializeField]
	private System.Random randomizer;

	public int randomSeed
	{
		get
		{
			return _randomSeed;
		}
		set
		{
			if (value != _randomSeed)
			{
				_randomSeed = value;
				randomizer = new System.Random(_randomSeed);
			}
		}
	}

	public ObjectSequence()
	{
		randomizer = new System.Random(_randomSeed);
	}

	public T GetFirst()
	{
		if (startObject != null)
		{
			return startObject;
		}
		return Next();
	}

	public T GetLast()
	{
		if (endObject != null)
		{
			return endObject;
		}
		return Next();
	}

	public T Next()
	{
		if (iteration == Iteration.Ordered)
		{
			if (index >= objects.Length)
			{
				index = 0;
			}
			return objects[index++];
		}
		int num = randomizer.Next(objects.Length - 1);
		return objects[num];
	}
}
