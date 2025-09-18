using System;
using System.Collections;
using System.Collections.Generic;
using GameEvent;
using UnityEngine;

public class OutfitManager : MonoBehaviour, IGameEventListener
{
	public ArtMatcher[] characterArtMatchers;

	public Dictionary<Character, List<Outfit>[]> characterOutfitsUnlocked = new Dictionary<Character, List<Outfit>[]>();

	public Dictionary<Character, List<Outfit>[]> characterOutfitsAll = new Dictionary<Character, List<Outfit>[]>();

	protected bool Started;

	public LevelSelectController levelSelectController;

	private void Awake()
	{
	}

	private void Start()
	{
		ChangeListeners(adding: true);
	}

	public static void ProcessForcedOutfits()
	{
		ArtMatcher[] array = UnityEngine.Object.FindObjectsOfType<ArtMatcher>();
		foreach (ArtMatcher artMatcher in array)
		{
			Outfit[] outfits = artMatcher.outfits;
			foreach (Outfit outfit in outfits)
			{
				Outfit.OutfitType outfitType = outfit.outfitType;
				if ((uint)(outfitType - 4) > 1u && !outfit.Unlocked && artMatcher.GetDefaultForcedOutfit(outfit.outfitType) == outfit)
				{
					outfit.TempUnlocked = true;
				}
			}
		}
	}

	public void ResetTempUnlocksForDefaultOutfits()
	{
		ArtMatcher[] array = characterArtMatchers;
		foreach (ArtMatcher artMatcher in array)
		{
			for (int j = 0; j < artMatcher.defaultOutfits.Count; j++)
			{
				if (artMatcher.defaultOutfits[j] != null)
				{
					artMatcher.defaultOutfits[j].TempUnlocked = false;
				}
			}
		}
	}

	public void RebuildDatabase()
	{
		characterOutfitsUnlocked.Clear();
		characterOutfitsAll.Clear();
		characterArtMatchers = UnityEngine.Object.FindObjectsOfType<ArtMatcher>();
		ArtMatcher[] array = characterArtMatchers;
		foreach (ArtMatcher artMatcher in array)
		{
			characterOutfitsUnlocked.Add(artMatcher.character, new List<Outfit>[Outfit.NumOutfitTypes]);
			characterOutfitsAll.Add(artMatcher.character, new List<Outfit>[Outfit.NumOutfitTypes]);
			for (int j = 0; j < Outfit.NumOutfitTypes; j++)
			{
				characterOutfitsUnlocked[artMatcher.character][j] = new List<Outfit>();
				characterOutfitsAll[artMatcher.character][j] = new List<Outfit>();
			}
			Outfit[] outfits = artMatcher.outfits;
			foreach (Outfit outfit in outfits)
			{
				if (outfit.outfitType != Outfit.OutfitType.FollowOutfit && outfit.outfitType != Outfit.OutfitType.Zombie)
				{
					characterOutfitsAll[artMatcher.character][(int)outfit.outfitType].Add(outfit);
					bool flag = outfit.Unlocked;
					if (!flag && artMatcher.GetDefaultForcedOutfit(outfit.outfitType) == outfit)
					{
						outfit.TempUnlocked = true;
						flag = true;
					}
					if (flag)
					{
						characterOutfitsUnlocked[artMatcher.character][(int)outfit.outfitType].Add(outfit);
					}
				}
			}
		}
	}

	public void ChangeListeners(bool adding)
	{
		GameEventManager.ChangeListener<CheatUnlockEvent>(this, adding);
		GameEventManager.ChangeListener<NetworkHostLobbyLoadedEvent>(this, adding);
	}

	public void OnDestroy()
	{
		ChangeListeners(adding: false);
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(NetworkHostLobbyLoadedEvent))
		{
			StartCoroutine(RebuildDatabaseNextFrame());
		}
		if (type == typeof(CheatUnlockEvent))
		{
			ResetTempUnlocksForDefaultOutfits();
			StartCoroutine(RebuildDatabaseNextFrame());
		}
	}

	private IEnumerator RebuildDatabaseNextFrame()
	{
		yield return null;
		RebuildDatabase();
	}
}
