using System;
using UnityEngine;

[Serializable]
public class CharacterSpriteManager : ScriptableObject
{
	public Sprite[] OKCursors;

	public Sprite[] BadCursors;

	public Sprite[] NotebookCursors;

	public Sprite[] Portraits;

	public Sprite[] LiveOutlines;

	public Sprite[] DeadOutlines;

	public AnimatorOverrideController[] CharacterSpriteOverrides;

	public AnimatorOverrideController[] SpectatorSpriteOverrides;

	public Color[] ZombieColors;

	public Sprite DefaultBlankSprite;

	private static CharacterSpriteManager instance;

	public CharacterSpriteLibrary GetCharacterSprites(Character.Animals animal)
	{
		if (animal == Character.Animals.NONE)
		{
			CharacterSpriteLibrary characterSpriteLibrary = new CharacterSpriteLibrary();
			characterSpriteLibrary.OKCursor = DefaultBlankSprite;
			characterSpriteLibrary.BadCursor = DefaultBlankSprite;
			characterSpriteLibrary.NotebookCursor = DefaultBlankSprite;
			characterSpriteLibrary.Portrait = DefaultBlankSprite;
			characterSpriteLibrary.DeadOutline = DefaultBlankSprite;
			characterSpriteLibrary.DeadOutline = DefaultBlankSprite;
			characterSpriteLibrary.CharacterSpriteOverride = null;
			characterSpriteLibrary.SpectatorSpriteOverride = null;
			return characterSpriteLibrary;
		}
		int num = (int)(animal - 1);
		if (num < 0 || num >= OKCursors.Length)
		{
			Debug.LogError("Error getting sprite library for character " + animal);
			return null;
		}
		return new CharacterSpriteLibrary
		{
			OKCursor = OKCursors[num],
			BadCursor = BadCursors[num],
			NotebookCursor = NotebookCursors[num],
			Portrait = Portraits[num],
			LiveOutline = LiveOutlines[num],
			DeadOutline = DeadOutlines[num],
			CharacterSpriteOverride = CharacterSpriteOverrides[num],
			SpectatorSpriteOverride = SpectatorSpriteOverrides[num]
		};
	}

	public static CharacterSpriteManager GetInstance()
	{
		if (instance == null)
		{
			Debug.LogError("Character sprite manager instance is null.");
		}
		return instance;
	}

	public static void SetInstance(CharacterSpriteManager instance)
	{
		CharacterSpriteManager.instance = instance;
	}

	public Sprite GetCharaterPortrait(Character.Animals animal)
	{
		if (animal == Character.Animals.NONE)
		{
			return DefaultBlankSprite;
		}
		int num = (int)(animal - 1);
		if (num < 0 || num >= OKCursors.Length)
		{
			Debug.LogError("Error getting portrait for character " + animal);
			return null;
		}
		return Portraits[num];
	}

	public Sprite GetCharaterAliveIcon(Character.Animals animal)
	{
		if (animal == Character.Animals.NONE)
		{
			return DefaultBlankSprite;
		}
		int num = (int)(animal - 1);
		if (num < 0 || num >= OKCursors.Length)
		{
			Debug.LogError("Error getting portrait for character " + animal);
			return null;
		}
		return LiveOutlines[num];
	}

	public Sprite GetCharaterDeadIcon(Character.Animals animal)
	{
		if (animal == Character.Animals.NONE)
		{
			return DefaultBlankSprite;
		}
		int num = (int)(animal - 1);
		if (num < 0 || num >= OKCursors.Length)
		{
			Debug.LogError("Error getting portrait for character " + animal);
			return null;
		}
		return DeadOutlines[num];
	}

	public Color GetZombieColour(Character.Animals animal)
	{
		if (animal == Character.Animals.NONE)
		{
			return ZombieColors[0];
		}
		int num = (int)(animal - 1);
		if (num < 0 || num >= OKCursors.Length)
		{
			Debug.LogError("Error getting portrait for character " + animal);
			return ZombieColors[0];
		}
		return ZombieColors[num];
	}
}
