using System.Collections.Generic;
using UnityEngine;

public class PlaceableMetadataList : MonoBehaviour
{
	public GameObject[] allBlockPrefabs;

	public GameObject[] extraBlocks;

	public GameObject[] RandomGroups;

	public GameObject[] SpecialGroups;

	public PlaceableMetadata teleporterPrefabMetadata;

	private static PlaceableMetadataList instance;

	private Dictionary<int, Object> cachedBlockMap;

	private Dictionary<string, int> nameToIndexMap;

	private Dictionary<Placeable, List<Placeable>> placeableVariants;

	public static PlaceableMetadataList Instance => instance;

	public Dictionary<int, Object> CachedBlockMap
	{
		get
		{
			if (cachedBlockMap == null)
			{
				cachedBlockMap = new Dictionary<int, Object>();
				for (int i = 0; i < allBlockPrefabs.Length; i++)
				{
					cachedBlockMap.Add(i, allBlockPrefabs[i]);
				}
			}
			return cachedBlockMap;
		}
	}

	public Dictionary<string, int> NameToIndexMap
	{
		get
		{
			if (nameToIndexMap == null)
			{
				nameToIndexMap = new Dictionary<string, int>();
				for (int i = 0; i < allBlockPrefabs.Length; i++)
				{
					nameToIndexMap.Add(allBlockPrefabs[i].GetComponent<Placeable>().Name, i);
				}
			}
			return nameToIndexMap;
		}
	}

	public Dictionary<Placeable, List<Placeable>> PlaceableVariants
	{
		get
		{
			if (placeableVariants == null)
			{
				placeableVariants = new Dictionary<Placeable, List<Placeable>>();
				for (int i = 0; i < extraBlocks.Length; i++)
				{
					Placeable component = extraBlocks[i].GetComponent<Placeable>();
					if (component.FilterOverride.Length != 0)
					{
						Placeable placeable = component.FilterOverride[0];
						List<Placeable> value = null;
						if (!placeableVariants.TryGetValue(placeable, out value))
						{
							value = new List<Placeable>();
							value.Add(placeable);
							placeableVariants[placeable] = value;
						}
						value.Add(component);
					}
				}
			}
			return placeableVariants;
		}
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			Object.DontDestroyOnLoad(base.gameObject);
		}
		else if (instance != this)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public Object GetPrefabForPlaceableIndex(int idx)
	{
		Object value = null;
		if (CachedBlockMap.TryGetValue(idx, out value))
		{
			return value;
		}
		return null;
	}

	public int GetIndexForPlaceable(string Name)
	{
		if (NameToIndexMap.TryGetValue(Name, out var value))
		{
			return value;
		}
		return -1;
	}

	public Placeable GetPlaceableByIndex(int idx)
	{
		Object value = null;
		if (CachedBlockMap.TryGetValue(idx, out value))
		{
			return (value as GameObject).GetComponent<Placeable>();
		}
		return null;
	}

	public PickableBlock GetPickableByIndex(int idx)
	{
		Object value = null;
		if (CachedBlockMap.TryGetValue(idx, out value))
		{
			return (value as GameObject).GetComponent<Placeable>().PickableBlock;
		}
		return null;
	}

	public int AllBlockListLength()
	{
		return allBlockPrefabs.Length;
	}

	public int FindVariantIndex(Placeable variantPlaceable)
	{
		if (variantPlaceable.FilterOverride.Length != 0)
		{
			Placeable placeable = variantPlaceable.FilterOverride[0];
			if (placeable != null)
			{
				if (PlaceableVariants.TryGetValue(placeable, out var value))
				{
					return value.FindIndex((Placeable p) => p == variantPlaceable);
				}
				Debug.LogError("FindVariantIndex: Could not find placeable " + variantPlaceable.Name + " in variant list for " + placeable.Name);
			}
			else
			{
				Debug.LogError("FindVariantIndex: Base Placeable is null");
			}
		}
		else
		{
			Debug.LogError("FindVariantIndex: Supplied placeable has no filter overrides");
		}
		return -1;
	}

	public PickableBlock GetPickableForVariantIndex(PickableBlock basePickable, int variantIndex)
	{
		if (PlaceableVariants.TryGetValue(basePickable.placeablePrefab, out var value))
		{
			if (value.Count > variantIndex)
			{
				if (value[variantIndex] != null)
				{
					return value[variantIndex].PickableBlock;
				}
				Debug.LogError("GetPickableForVariantIndex: Null placeable found");
			}
			else
			{
				Debug.LogError("GetPickableForVariantIndex: Variant index " + variantIndex + " seems invalid.");
			}
		}
		else
		{
			Debug.LogError("GetPickableForVariantIndex: Could not find placeable " + basePickable.placeablePrefab.Name + " in list of blocks with variants");
		}
		return null;
	}
}
