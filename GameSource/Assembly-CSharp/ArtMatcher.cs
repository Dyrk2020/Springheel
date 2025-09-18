using System.Collections.Generic;
using UnityEngine;

public class ArtMatcher : MonoBehaviour
{
	public Character character;

	public Sprite[] inputSprites;

	public Outfit[] outfits;

	[HideInInspector]
	public List<Outfit> defaultOutfits = new List<Outfit> { null, null, null, null };

	public Character.Animals CharacterSprite;

	public Material defaultOutfitMaterial;

	public Material defaultCursorOutfitMaterial;

	protected Sprite lastSprite;

	protected Sprite currentSprite;

	private float lastOpacity = 1f;

	public Dictionary<Sprite, int> spriteInt = new Dictionary<Sprite, int>();

	protected SpriteRenderer followTarget;

	protected bool SpectatorMode;

	private void LateUpdate()
	{
		if (followTarget == null || character == null || character.SpriteRenderer == null)
		{
			return;
		}
		currentSprite = followTarget.sprite;
		float currentOpacity = character.opacityController.currentOpacity;
		if ((currentSprite != lastSprite && outfits.Length != 0) || !followTarget.enabled || currentOpacity != lastOpacity)
		{
			if (!spriteInt.TryGetValue(currentSprite, out var value))
			{
				return;
			}
			for (int i = 0; i < outfits.Length; i++)
			{
				Outfit outfit = ((!(outfits[i].followThisOutfit != null)) ? outfits[i] : outfits[i].followThisOutfit);
				bool flag = GetDefaultForcedOutfit(outfits[i].outfitType) != null;
				bool flag2 = true;
				for (int j = 0; j < outfits.Length; j++)
				{
					if (outfits[j].on && outfits[j].outfitType == outfits[i].outfitType)
					{
						flag2 = false;
						break;
					}
				}
				if (flag && flag2)
				{
					outfits[i].on = true;
				}
				if (outfits[i].outputSpriteGameObject == null && outfit.on)
				{
					outfits[i].outputSpriteGameObject = new GameObject(outfits[i].outfitString);
					outfits[i].outputSpriteGameObject.transform.parent = followTarget.gameObject.transform;
					outfits[i].outputSpriteGameObject.transform.localPosition = Vector3.zero;
					outfits[i].outputSpriteGameObject.transform.localRotation = Quaternion.identity;
					outfits[i].outputSpriteGameObject.transform.localScale = Vector3.one;
					outfits[i].outputSpriteRenderer = outfits[i].outputSpriteGameObject.AddComponent<SpriteRenderer>();
					outfits[i].outputSpriteRenderer.sortingLayerID = followTarget.sortingLayerID;
					if (!outfits[i].BehindLayer)
					{
						outfits[i].outputSpriteRenderer.sortingOrder = followTarget.sortingOrder + outfits[i].LayerNumber;
					}
					else
					{
						outfits[i].outputSpriteRenderer.sortingOrder = followTarget.sortingOrder - outfits[i].LayerNumber;
					}
					if (outfits[i].outfitType != Outfit.OutfitType.Zombie)
					{
						outfits[i].outputSpriteRenderer.color = followTarget.color;
					}
					outfits[i].outputSpriteGameObject.GetComponent<Renderer>().material = defaultOutfitMaterial;
					outfits[i].ApplyHSVMaterialProperties(outfits[i].outputSpriteRenderer);
				}
				if (!(outfits[i].outputSpriteGameObject != null))
				{
					continue;
				}
				if (((character.Visible || SpectatorMode) && outfit.on && (outfit.Unlocked || outfit.TempUnlocked) && character.hasAuthority) || (character.Visible && outfit.on && !character.hasAuthority))
				{
					outfits[i].outputSpriteRenderer.enabled = true;
					outfits[i].outputSpriteRenderer.sprite = outfits[i].outputSprites[value];
					if (character.Sitting)
					{
						if (character.CharacterSprite == Character.Animals.SQUIRREL || character.CharacterSprite == Character.Animals.CHAMELEON || character.CharacterSprite == Character.Animals.ROBOT || character.CharacterSprite == Character.Animals.SHEEP || character.CharacterSprite == Character.Animals.TURTLE || character.CharacterSprite == Character.Animals.PANDA || character.CharacterSprite == Character.Animals.FOX)
						{
							outfits[i].outputSpriteRenderer.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
						}
						else if (character.CharacterSprite == Character.Animals.HORSE || character.CharacterSprite == Character.Animals.CHICKEN || character.CharacterSprite == Character.Animals.ELEPHANT || character.CharacterSprite == Character.Animals.MONKEY || character.CharacterSprite == Character.Animals.PLATYPUS || character.CharacterSprite == Character.Animals.RACCOON || character.CharacterSprite == Character.Animals.SNAKE || character.CharacterSprite == Character.Animals.HIPPO)
						{
							outfits[i].outputSpriteRenderer.gameObject.transform.localScale = new Vector3(-1f, 1f, 1f);
						}
						else
						{
							outfits[i].outputSpriteRenderer.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
						}
					}
					else
					{
						outfits[i].outputSpriteRenderer.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
					}
					if (outfits[i].SpecialLayering)
					{
						if (!outfits[i].BehindLayer ^ outfits[i].InvertFrontBack[value])
						{
							outfits[i].outputSpriteRenderer.sortingOrder = followTarget.sortingOrder + outfits[i].LayerNumber;
						}
						else
						{
							outfits[i].outputSpriteRenderer.sortingOrder = followTarget.sortingOrder - outfits[i].LayerNumber;
						}
					}
					else if (!outfits[i].BehindLayer)
					{
						outfits[i].outputSpriteRenderer.sortingOrder = followTarget.sortingOrder + outfits[i].LayerNumber;
					}
					else
					{
						outfits[i].outputSpriteRenderer.sortingOrder = followTarget.sortingOrder - outfits[i].LayerNumber;
					}
					if (outfits[i].outfitType != Outfit.OutfitType.Zombie)
					{
						outfits[i].outputSpriteRenderer.color = followTarget.color;
					}
					else
					{
						outfits[i].outputSpriteRenderer.color = Color.white;
					}
					outfits[i].outputSpriteRenderer.SetAlpha(character.opacityController.currentOpacity);
				}
				else
				{
					outfits[i].outputSpriteRenderer.enabled = false;
				}
			}
		}
		lastSprite = currentSprite;
		lastOpacity = currentOpacity;
	}

	public void AttachArtMatcher(Character chr)
	{
		character = chr;
		base.transform.parent = character.SpriteRenderer.transform;
		followTarget = character.SpriteRenderer;
		for (int i = 0; i < inputSprites.Length; i++)
		{
			if (inputSprites[i] != null)
			{
				spriteInt.Add(inputSprites[i], i);
			}
		}
	}

	public void SwitchToSpectator()
	{
		if (!(character != null))
		{
			return;
		}
		character.EnableZombieOutfit(enable: false);
		SpectatorMode = true;
		base.transform.parent = character.SpectatorImage.SpriteRender.transform;
		followTarget = character.SpectatorImage.SpriteRender;
		character.SpectatorImage.SpriteRender.enabled = !character.bodyHidden;
		for (int i = 0; i < outfits.Length; i++)
		{
			if (outfits[i].outputSpriteGameObject != null && outfits[i].on)
			{
				outfits[i].outputSpriteGameObject.transform.parent = followTarget.gameObject.transform;
				outfits[i].outputSpriteGameObject.transform.localPosition = Vector3.zero;
				outfits[i].outputSpriteGameObject.transform.localRotation = Quaternion.identity;
				outfits[i].outputSpriteGameObject.transform.localScale = Vector3.one;
				outfits[i].outputSpriteRenderer.sortingLayerID = followTarget.sortingLayerID;
				if (!outfits[i].BehindLayer)
				{
					outfits[i].outputSpriteRenderer.sortingOrder = followTarget.sortingOrder + outfits[i].LayerNumber;
				}
				else
				{
					outfits[i].outputSpriteRenderer.sortingOrder = followTarget.sortingOrder - outfits[i].LayerNumber;
				}
				outfits[i].outputSpriteRenderer.color = followTarget.color;
				outfits[i].outputSpriteGameObject.GetComponent<Renderer>().material = defaultOutfitMaterial;
				outfits[i].ApplyHSVMaterialProperties(outfits[i].outputSpriteRenderer);
			}
		}
	}

	public void SwitchToCharacter()
	{
		if (!(character != null))
		{
			return;
		}
		SpectatorMode = false;
		base.transform.parent = character.SpriteRenderer.transform;
		followTarget = character.SpriteRenderer;
		for (int i = 0; i < outfits.Length; i++)
		{
			if (outfits[i].outputSpriteGameObject != null && outfits[i].on)
			{
				outfits[i].outputSpriteGameObject.transform.parent = followTarget.gameObject.transform;
				outfits[i].outputSpriteGameObject.transform.localPosition = Vector3.zero;
				outfits[i].outputSpriteGameObject.transform.localRotation = Quaternion.identity;
				outfits[i].outputSpriteGameObject.transform.localScale = Vector3.one;
				outfits[i].outputSpriteRenderer.sortingLayerID = followTarget.sortingLayerID;
				if (!outfits[i].BehindLayer)
				{
					outfits[i].outputSpriteRenderer.sortingOrder = followTarget.sortingOrder + outfits[i].LayerNumber;
				}
				else
				{
					outfits[i].outputSpriteRenderer.sortingOrder = followTarget.sortingOrder - outfits[i].LayerNumber;
				}
				outfits[i].outputSpriteRenderer.color = followTarget.color;
				outfits[i].outputSpriteGameObject.GetComponent<Renderer>().material = defaultOutfitMaterial;
				outfits[i].ApplyHSVMaterialProperties(outfits[i].outputSpriteRenderer);
			}
		}
	}

	public Outfit GetDefaultForcedOutfit(Outfit.OutfitType outfitType)
	{
		return GetDefaultForcedOutfit((int)outfitType);
	}

	public Outfit GetDefaultForcedOutfit(int idx)
	{
		if (idx < defaultOutfits.Count)
		{
			return defaultOutfits[idx];
		}
		return null;
	}
}
