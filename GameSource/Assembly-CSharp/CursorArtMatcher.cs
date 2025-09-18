using System.Collections.Generic;
using UnityEngine;

public class CursorArtMatcher : MonoBehaviour
{
	public Character character;

	public ArtMatcher artMatcher;

	public Cursor cursor;

	protected Sprite lastSprite;

	protected Sprite currentSprite;

	protected Dictionary<int, SpriteRenderer> outputSpriteHolders = new Dictionary<int, SpriteRenderer>();

	protected int[] characterOutfitsArray = new int[3] { -1, -1, -1 };

	public GameObject targetSpriteGameObject;

	protected SpriteRenderer followTarget;

	protected bool active;

	private Character.Animals animal;

	public void Setup()
	{
		cursor = GetComponent<Cursor>();
		followTarget = targetSpriteGameObject.GetComponent<SpriteRenderer>();
		if (character != null)
		{
			int[] outfitsAsArray = character.GetOutfitsAsArray();
			bool flag = outfitsAsArray[0] == characterOutfitsArray[0] && outfitsAsArray[1] == characterOutfitsArray[1] && outfitsAsArray[2] == characterOutfitsArray[2];
			if (character.CharacterSprite == animal && character.networkNumber == cursor.networkNumber && flag)
			{
				return;
			}
			foreach (KeyValuePair<int, SpriteRenderer> outputSpriteHolder in outputSpriteHolders)
			{
				Object.Destroy(outputSpriteHolder.Value.gameObject);
			}
			outputSpriteHolders.Clear();
			lastSprite = null;
		}
		foreach (Character allCharacter in Character.AllCharacters)
		{
			if (allCharacter != null && allCharacter.networkNumber == cursor.networkNumber)
			{
				character = allCharacter;
				animal = allCharacter.CharacterSprite;
				characterOutfitsArray = allCharacter.GetOutfitsAsArray();
				break;
			}
		}
		if (character != null)
		{
			artMatcher = character.GetComponentInChildren<ArtMatcher>();
		}
	}

	private void LateUpdate()
	{
		if (!active || followTarget == null)
		{
			return;
		}
		if (artMatcher == null)
		{
			if (character != null)
			{
				artMatcher = character.GetComponentInChildren<ArtMatcher>();
			}
			if (artMatcher == null)
			{
				return;
			}
		}
		currentSprite = followTarget.sprite;
		if (currentSprite != lastSprite && artMatcher.outfits.Length != 0)
		{
			if (!artMatcher.spriteInt.TryGetValue(currentSprite, out var value))
			{
				return;
			}
			for (int i = 0; i < artMatcher.outfits.Length; i++)
			{
				Outfit outfit = ((!(artMatcher.outfits[i].followThisOutfit != null)) ? artMatcher.outfits[i] : artMatcher.outfits[i].followThisOutfit);
				if (!outputSpriteHolders.ContainsKey(i) && outfit.on)
				{
					GameObject gameObject = new GameObject(artMatcher.outfits[i].outfitString);
					gameObject.transform.parent = targetSpriteGameObject.transform;
					gameObject.transform.localPosition = Vector3.zero;
					gameObject.transform.localRotation = Quaternion.identity;
					gameObject.transform.localScale = Vector3.one;
					gameObject.layer = targetSpriteGameObject.layer;
					outputSpriteHolders.Add(i, gameObject.AddComponent<SpriteRenderer>());
					outputSpriteHolders[i].sortingLayerID = followTarget.sortingLayerID;
					if (!artMatcher.outfits[i].BehindLayer)
					{
						outputSpriteHolders[i].sortingOrder = followTarget.sortingOrder + artMatcher.outfits[i].LayerNumber;
					}
					else
					{
						outputSpriteHolders[i].sortingOrder = followTarget.sortingOrder - artMatcher.outfits[i].LayerNumber;
					}
					outputSpriteHolders[i].color = followTarget.color;
					outputSpriteHolders[i].material = artMatcher.defaultCursorOutfitMaterial;
					artMatcher.outfits[i].ApplyHSVMaterialProperties(outputSpriteHolders[i]);
				}
				if (!outputSpriteHolders.ContainsKey(i))
				{
					continue;
				}
				if (outfit.on && (outfit.Unlocked || outfit.TempUnlocked))
				{
					outputSpriteHolders[i].enabled = true;
					outputSpriteHolders[i].sprite = artMatcher.outfits[i].outputSprites[value];
					if (artMatcher.outfits[i].hidesAnimalBody)
					{
						followTarget.enabled = false;
					}
					if (artMatcher.outfits[i].SpecialLayering)
					{
						if (!artMatcher.outfits[i].BehindLayer ^ artMatcher.outfits[i].InvertFrontBack[value])
						{
							outputSpriteHolders[i].sortingOrder = followTarget.sortingOrder + artMatcher.outfits[i].LayerNumber;
						}
						else
						{
							outputSpriteHolders[i].sortingOrder = followTarget.sortingOrder - artMatcher.outfits[i].LayerNumber;
						}
					}
					if (outputSpriteHolders[i].color != followTarget.color)
					{
						outputSpriteHolders[i].color = followTarget.color;
					}
				}
				else
				{
					outputSpriteHolders[i].enabled = false;
				}
			}
		}
		lastSprite = currentSprite;
	}

	public void Enable(bool show = true)
	{
		lastSprite = null;
		foreach (SpriteRenderer value in outputSpriteHolders.Values)
		{
			value.enabled = show;
		}
		active = show;
		if (show)
		{
			Setup();
		}
	}

	public void Disable()
	{
		Enable(show: false);
	}

	public void SetArtAlpha(float alpha)
	{
		if (!(followTarget != null))
		{
			return;
		}
		for (int i = 0; i < artMatcher.outfits.Length; i++)
		{
			if (outputSpriteHolders.ContainsKey(i) && outputSpriteHolders[i] != null)
			{
				outputSpriteHolders[i].color = new Color(outputSpriteHolders[i].color.r, outputSpriteHolders[i].color.g, outputSpriteHolders[i].color.b, alpha);
			}
		}
	}
}
