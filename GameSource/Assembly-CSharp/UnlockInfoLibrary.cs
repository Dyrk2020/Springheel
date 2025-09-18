using System.Collections.Generic;
using UnityEngine;

public class UnlockInfoLibrary : MonoBehaviour
{
	private static UnlockInfoLibrary instance;

	private Dictionary<Character.Animals, UnLockInfo> characterUnlocks = new Dictionary<Character.Animals, UnLockInfo>();

	private Dictionary<Character.Animals, List<UnLockInfo>> outfitUnlocks = new Dictionary<Character.Animals, List<UnLockInfo>>();

	private Dictionary<GameState.LevelName, UnLockInfo> levelUnlocks = new Dictionary<GameState.LevelName, UnLockInfo>();

	public UnLockInfo[] AllOutfitUnlocks;

	public UnLockInfo[] AllCharacterUnlocks;

	public UnLockInfo[] AllLevelUnlocks;

	public static UnlockInfoLibrary Instance
	{
		get
		{
			if (instance == null)
			{
				Debug.LogError("UnlockInfoLibrary instance is null!");
			}
			return instance;
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

	private void Start()
	{
		for (int i = 0; i != AllCharacterUnlocks.Length; i++)
		{
			characterUnlocks[AllCharacterUnlocks[i].AssociatedCharacter] = AllCharacterUnlocks[i];
		}
		for (int j = 0; j != AllOutfitUnlocks.Length; j++)
		{
			if (!outfitUnlocks.ContainsKey(AllOutfitUnlocks[j].AssociatedCharacter))
			{
				outfitUnlocks[AllOutfitUnlocks[j].AssociatedCharacter] = new List<UnLockInfo>();
			}
			outfitUnlocks[AllOutfitUnlocks[j].AssociatedCharacter].Add(AllOutfitUnlocks[j]);
		}
		for (int k = 0; k != AllLevelUnlocks.Length; k++)
		{
			if (!(AllLevelUnlocks[k] == null))
			{
				levelUnlocks[AllLevelUnlocks[k].AssociatedLevel] = AllLevelUnlocks[k];
			}
		}
	}

	public UnLockInfo GetCharacterUnlock(Character.Animals character)
	{
		return characterUnlocks[character];
	}

	public UnLockInfo GetOutfitUnlock(Character.Animals character, int outfitNumber)
	{
		List<UnLockInfo> list = outfitUnlocks[character];
		if (list == null)
		{
			return null;
		}
		foreach (UnLockInfo item in list)
		{
			if (item.OutfitMaskNumber == outfitNumber)
			{
				return item;
			}
		}
		return null;
	}

	public UnLockInfo GetLevelUnlock(GameState.LevelName level)
	{
		return levelUnlocks[level];
	}

	public int GetNumOutfitsForAnimal(Character.Animals animal)
	{
		return outfitUnlocks[animal].Count;
	}
}
