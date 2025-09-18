using System.Collections.Generic;
using UnityEngine;

public class WeightedBlockList
{
	public struct WeightedBlock
	{
		public Placeable placeable;

		public int weight;
	}

	private WeightedBlock[] weightedBlocks;

	private Dictionary<Placeable, int> placeable2idx;

	private int totalWeight;

	private int maxWeight;

	public bool isEmpty => totalWeight == 0;

	public WeightedBlockList()
	{
	}

	public WeightedBlockList(int size)
	{
		placeable2idx = new Dictionary<Placeable, int>(size);
		weightedBlocks = new WeightedBlock[size];
		totalWeight = 0;
		maxWeight = 0;
	}

	public void AddWeight(int idx, Placeable placeable, int weight)
	{
		totalWeight += weight;
		weightedBlocks[idx].placeable = placeable;
		weightedBlocks[idx].weight = weight;
		if (!placeable2idx.ContainsKey(placeable))
		{
			placeable2idx.Add(placeable, idx);
		}
	}

	public int GetWeightForPlaceable(Placeable placeable)
	{
		int value = -1;
		if (placeable2idx.TryGetValue(placeable, out value))
		{
			return weightedBlocks[value].weight;
		}
		return -1;
	}

	public int GetMaxWeight()
	{
		return maxWeight;
	}

	private int GetRandomIndex()
	{
		int num = Random.Range(0, totalWeight);
		int num2 = 0;
		for (int i = 0; i < weightedBlocks.Length; i++)
		{
			int num3 = num2 + weightedBlocks[i].weight;
			if (num >= num2 && num < num3)
			{
				return i;
			}
			num2 = num3;
		}
		Debug.LogError("Failed to grab random index");
		return -1;
	}

	public Placeable GetRandomPlaceablePrefab()
	{
		int randomIndex = GetRandomIndex();
		if (randomIndex == -1)
		{
			return null;
		}
		return weightedBlocks[randomIndex].placeable;
	}

	public WeightedBlockList Clone()
	{
		return new WeightedBlockList
		{
			placeable2idx = new Dictionary<Placeable, int>(placeable2idx),
			weightedBlocks = (WeightedBlock[])weightedBlocks.Clone(),
			totalWeight = totalWeight,
			maxWeight = maxWeight
		};
	}

	public void ApplySkew(Placeable.Tag matchTag, float multiplier)
	{
		for (int i = 0; i < weightedBlocks.Length; i++)
		{
			Placeable placeable = weightedBlocks[i].placeable;
			if (placeable != null && (placeable.placeableTag & matchTag) != Placeable.Tag.None)
			{
				weightedBlocks[i].weight = Mathf.CeilToInt((float)weightedBlocks[i].weight * multiplier);
			}
		}
	}

	public void ApplySkewNot(Placeable.Tag notTag, float multiplier)
	{
		for (int i = 0; i < weightedBlocks.Length; i++)
		{
			Placeable placeable = weightedBlocks[i].placeable;
			if (placeable != null && (placeable.placeableTag & notTag) == 0)
			{
				weightedBlocks[i].weight = Mathf.CeilToInt((float)weightedBlocks[i].weight * multiplier);
			}
		}
	}

	public void ApplyAreaSkewAbove(float areaValue, float multiplier)
	{
		for (int i = 0; i < weightedBlocks.Length; i++)
		{
			Placeable placeable = weightedBlocks[i].placeable;
			if (placeable != null && placeable.Area > areaValue)
			{
				weightedBlocks[i].weight = Mathf.CeilToInt((float)weightedBlocks[i].weight * multiplier);
			}
		}
	}

	public void ApplyAreaSkewBelow(float areaValue, float multiplier)
	{
		for (int i = 0; i < weightedBlocks.Length; i++)
		{
			Placeable placeable = weightedBlocks[i].placeable;
			if (placeable != null && placeable.Area < areaValue)
			{
				weightedBlocks[i].weight = Mathf.CeilToInt((float)weightedBlocks[i].weight * multiplier);
			}
		}
	}

	public void RecomputeTotalWeights()
	{
		totalWeight = 0;
		maxWeight = 0;
		for (int i = 0; i < weightedBlocks.Length; i++)
		{
			if (weightedBlocks[i].placeable != null)
			{
				totalWeight += weightedBlocks[i].weight;
				if (weightedBlocks[i].weight > maxWeight)
				{
					maxWeight = weightedBlocks[i].weight;
				}
			}
		}
	}
}
