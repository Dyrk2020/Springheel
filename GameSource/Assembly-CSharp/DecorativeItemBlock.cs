using System;
using UnityEngine;

public class DecorativeItemBlock : Placeable
{
	public enum DecorativeLayer
	{
		FOREGROUND,
		MIDGROUND,
		BACKGROUND,
		FAR_BACKGROUND
	}

	public enum DecorationType
	{
		NATURE,
		CONSTRUCTION,
		MOUNTAINS,
		MISCELLANEOUS,
		DEATH_PIT
	}

	[Header("----DecorativeItemBlock----")]
	public DecorativeLayer Layer = DecorativeLayer.MIDGROUND;

	public DecorationType Type = DecorationType.MISCELLANEOUS;

	[Header("Hover Cycling")]
	private int temporaryHoverSortOrder;

	private int savedSortOrder;

	public event Action OnPlaced;

	protected override void Awake()
	{
		base.Awake();
		inDestructible = true;
		dontChangeArtLayers = true;
		isSetPiece = true;
		Category = PieceCategory.PLATFORM;
		DefaultSortingLayer = GetLayerName(Layer);
		ApplySortingLayer();
		AddAttachableTag();
	}

	private void AddAttachableTag()
	{
		CollisionTag componentInChildren = GetComponentInChildren<CollisionTag>();
		if (componentInChildren != null)
		{
			componentInChildren.bitMask |= TagComparer.Tag.AttachedPiece;
		}
	}

	protected override void Start()
	{
		base.Start();
		ApplySortingLayer();
	}

	private void ApplySortingLayer()
	{
		SpriteRenderer[] artSprites = ArtSprites;
		foreach (SpriteRenderer spriteRenderer in artSprites)
		{
			if (spriteRenderer != null)
			{
				spriteRenderer.sortingLayerName = DefaultSortingLayer;
			}
		}
	}

	private string GetLayerName(DecorativeLayer layer)
	{
		return layer switch
		{
			DecorativeLayer.FOREGROUND => "Foreground Background", 
			DecorativeLayer.MIDGROUND => "Default", 
			DecorativeLayer.BACKGROUND => "Background 1", 
			DecorativeLayer.FAR_BACKGROUND => "Background 5", 
			_ => "Default", 
		};
	}

	public override void EnablePlaced()
	{
		base.EnablePlaced();
		ApplySortingLayer();
		this.OnPlaced?.Invoke();
	}

	public void BringToFrontTemporarily()
	{
		if (spriteSortOrder != null)
		{
			savedSortOrder = spriteSortOrder.currentBaseOrder;
			temporaryHoverSortOrder = savedSortOrder + 10000;
			spriteSortOrder.setSortOrder(temporaryHoverSortOrder);
		}
	}

	public void RestoreOriginalOrder()
	{
		if (spriteSortOrder != null && temporaryHoverSortOrder != 0)
		{
			spriteSortOrder.setSortOrder(savedSortOrder);
			temporaryHoverSortOrder = 0;
		}
	}

	public override void Tint()
	{
		if (GameSettings.GetInstance() == null)
		{
			return;
		}
		if (bombTints > 0)
		{
			SpriteRenderer[] artSprites = ArtSprites;
			for (int i = 0; i < artSprites.Length; i++)
			{
				artSprites[i].color = bombTintColor;
			}
		}
		else if (pickedUp || (ParentPiece != null && ParentPiece.PickedUp))
		{
			if (CanPlace())
			{
				for (int j = 0; j < ArtSprites.Length; j++)
				{
					ArtSprites[j].color = initialColors[j];
				}
				return;
			}
			SpriteRenderer[] artSprites = ArtSprites;
			for (int i = 0; i < artSprites.Length; i++)
			{
				artSprites[i].color = GameSettings.GetInstance().negativeColor;
			}
		}
		else if (HoveredCursors.Count > 0)
		{
			for (int k = 0; k < ArtSprites.Length; k++)
			{
				ArtSprites[k].color = initialColors[k] + new Color(0.2f, 0.2f, 0.2f, 0f);
			}
		}
		else
		{
			for (int l = 0; l < ArtSprites.Length; l++)
			{
				ArtSprites[l].color = initialColors[l];
			}
		}
	}
}
