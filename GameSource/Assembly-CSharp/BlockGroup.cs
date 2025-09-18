using System.Collections.Generic;
using UnityEngine;

public class BlockGroup : MonoBehaviour
{
	public Placeable[] RequiredBlocks;

	public Placeable[] RandomPool;

	public float Challenge;

	public float ChallengeRange;

	public float Area;

	public float AreaRange;

	private int NumberUniqueBlocks;

	private int NumberUniqueBlocksFiltered;

	private WeightedBlockList weightedList;

	public bool LacksVariety
	{
		get
		{
			if (weightedList == null)
			{
				filterPool();
			}
			return (float)NumberUniqueBlocksFiltered < (float)NumberUniqueBlocks * 0.5f;
		}
	}

	public bool HasBlocksAfterFiltering
	{
		get
		{
			if (weightedList == null)
			{
				filterPool();
			}
			return NumberUniqueBlocksFiltered > 0;
		}
	}

	public int filterPool()
	{
		GameSettings instance = GameSettings.GetInstance();
		int num = 0;
		HashSet<Placeable> hashSet = new HashSet<Placeable>();
		Dictionary<Placeable, int> dictionary = new Dictionary<Placeable, int>();
		for (int i = 0; i < RandomPool.Length; i++)
		{
			Placeable placeable = RandomPool[i];
			bool flag = true;
			if (placeable.FilterOverride.Length == 0)
			{
				flag = instance.itemFilter[placeable].Enabled;
			}
			else
			{
				for (int j = 0; j != placeable.FilterOverride.Length; j++)
				{
					if (!instance.itemFilter[placeable.FilterOverride[j]].Enabled)
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				num++;
				if (dictionary.ContainsKey(placeable))
				{
					dictionary[placeable]++;
				}
				else
				{
					dictionary[placeable] = 1;
				}
			}
			hashSet.Add(placeable);
		}
		NumberUniqueBlocks = hashSet.Count;
		NumberUniqueBlocksFiltered = dictionary.Count;
		weightedList = new WeightedBlockList(dictionary.Count);
		int num2 = 0;
		foreach (KeyValuePair<Placeable, int> item in dictionary)
		{
			weightedList.AddWeight(num2++, item.Key, GetItemFilterWeight(item.Key, item.Value));
		}
		return num;
	}

	public static int GetItemFilterWeight(Placeable placeablePrefab, int weight)
	{
		if (weight == 0)
		{
			return 0;
		}
		Placeable key = ((placeablePrefab.FilterOverride.Length != 0) ? placeablePrefab.FilterOverride[0] : placeablePrefab);
		return TabletBlockList.GetWeightFromStepValue(GameSettings.GetInstance().itemFilter[key].Frequency);
	}

	public PickableBlock GetRandomPickableBlockPrefab()
	{
		Placeable randomPlaceablePrefab = weightedList.GetRandomPlaceablePrefab();
		if (randomPlaceablePrefab != null)
		{
			return randomPlaceablePrefab.PickableBlock;
		}
		return null;
	}

	public PickableBlock GetRandomPickableBlockPrefabUnfiltered()
	{
		return RandomPool[Random.Range(0, RandomPool.Length)].PickableBlock;
	}
}
