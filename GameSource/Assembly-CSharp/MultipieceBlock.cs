using System.Collections.Generic;
using UnityEngine;

public class MultipieceBlock : ActiveBlock
{
	public bool UseSpriteParts = true;

	public Sprite Left;

	public Sprite Center;

	public Sprite Right;

	public Sprite Single;

	public MultipiecePart[] Parts;

	public bool Separable;

	protected override void Awake()
	{
		base.Awake();
		MultipiecePart[] parts = Parts;
		foreach (MultipiecePart obj in parts)
		{
			obj.MainBlock = this;
			obj.relativeAttachPosition = obj.transform.localPosition;
		}
	}

	protected virtual void Update()
	{
		bool flag = false;
		MultipiecePart[] parts = Parts;
		foreach (MultipiecePart multipiecePart in parts)
		{
			if (!(multipiecePart == null) && !multipiecePart.MarkedForDestruction)
			{
				if (!Separable)
				{
					multipiecePart.HoveredCursors = HoveredCursors;
				}
				flag = true;
			}
		}
		if (!flag && !base.MarkedForDestruction)
		{
			DestroySelf();
		}
	}

	protected override void Activate()
	{
		base.Activate();
	}

	public override void Disable()
	{
		base.Disable();
		MultipiecePart[] parts = Parts;
		for (int i = 0; i < parts.Length; i++)
		{
			parts[i].Disable();
		}
	}

	public override void Enable()
	{
		base.Enable();
		MultipiecePart[] parts = Parts;
		foreach (MultipiecePart multipiecePart in parts)
		{
			if (!(multipiecePart == null))
			{
				if (multipiecePart.MarkedForDestruction)
				{
					multipiecePart.Disable();
				}
				else if (!multipiecePart.PickedUp)
				{
					multipiecePart.Enable();
				}
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
	}

	public override void EnablePlacement(bool showGuides)
	{
		base.EnablePlacement(showGuides);
		MultipiecePart[] parts = Parts;
		foreach (MultipiecePart multipiecePart in parts)
		{
			if (!(multipiecePart == null))
			{
				if (multipiecePart.MarkedForDestruction)
				{
					multipiecePart.Disable();
				}
				else
				{
					multipiecePart.EnablePlacement(showGuides);
				}
			}
		}
	}

	public override void EnablePlaced()
	{
		base.EnablePlaced();
		MultipiecePart[] parts = Parts;
		foreach (MultipiecePart multipiecePart in parts)
		{
			if (multipiecePart != null)
			{
				multipiecePart.EnablePlaced();
			}
		}
	}

	public override bool CanPlace()
	{
		if (!base.CanPlace())
		{
			return false;
		}
		MultipiecePart[] parts = Parts;
		for (int i = 0; i < parts.Length; i++)
		{
			if (parts[i].CanPlace())
			{
				return true;
			}
		}
		return false;
	}

	protected bool CanPlaceWithParts()
	{
		if (!base.CanPlace())
		{
			return false;
		}
		MultipiecePart[] parts = Parts;
		for (int i = 0; i < parts.Length; i++)
		{
			if (!parts[i].CanPlace())
			{
				return false;
			}
		}
		return true;
	}

	public override void Place(int playerNumber, bool sendEvent, bool force)
	{
		base.Place(playerNumber, sendEvent, force);
		int num = 0;
		GameObject gameObject = null;
		for (int i = 0; i != Parts.Length; i++)
		{
			MultipiecePart multipiecePart = Parts[i];
			if (multipiecePart == null)
			{
				Debug.LogWarning("Multipiece part is null: " + base.name);
				continue;
			}
			if (!force && !multipiecePart.CanPlace() && !multipiecePart.inDestructible)
			{
				multipiecePart.DestroySelfIn(destroyChildren: true);
				Debug.Log("Part " + multipiecePart.name + " can't be placed");
				continue;
			}
			if (UseSpriteParts)
			{
				if (gameObject != null && gameObject != multipiecePart.CollidingWith)
				{
					if (num == 1)
					{
						Parts[i - 1].PartSprite.sprite = Single;
					}
					else
					{
						Parts[i - 1].PartSprite.sprite = Right;
					}
					if (i == Parts.Length - 1)
					{
						multipiecePart.PartSprite.sprite = Single;
					}
					else
					{
						multipiecePart.PartSprite.sprite = Left;
					}
					num = 1;
					gameObject = multipiecePart.CollidingWith;
				}
				else
				{
					if (gameObject == null)
					{
						gameObject = multipiecePart.CollidingWith;
					}
					num++;
					if (num == 1)
					{
						multipiecePart.PartSprite.sprite = Left;
					}
					else if (i == Parts.Length - 1)
					{
						multipiecePart.PartSprite.sprite = Right;
					}
					else
					{
						multipiecePart.PartSprite.sprite = Center;
					}
				}
			}
			multipiecePart.Place(playerNumber, sendEvent: false, force);
		}
	}

	protected override void setPickedUp(bool value)
	{
		base.setPickedUp(value);
		MultipiecePart[] parts = Parts;
		for (int i = 0; i < parts.Length; i++)
		{
			parts[i].PickedUp = value;
		}
	}

	public void RemovePart(MultipiecePart part)
	{
		if (Parts.Length == 1 && Parts[0] == part)
		{
			DestroySelf(destroyChildren: false, useSmoke: false);
			return;
		}
		MultipiecePart[] array = new MultipiecePart[Parts.Length - 1];
		int num = 0;
		for (int i = 0; i != Parts.Length; i++)
		{
			if (Parts[i] == part)
			{
				num--;
				part.MainBlock = null;
			}
			else
			{
				array[num] = Parts[i];
			}
			num++;
		}
		Parts = array;
	}

	public override void DestroySelf(bool destroyChildren = false, bool useSmoke = true, bool sendNetworkSignal = true)
	{
		MultipiecePart[] parts = Parts;
		for (int i = 0; i < parts.Length; i++)
		{
			parts[i].DestroySelf(destroyChildren, useSmoke, sendNetworkSignal);
		}
		base.DestroySelf(destroyChildren, useSmoke, sendNetworkSignal);
	}

	public override float GetTotalMass()
	{
		float num = Mass;
		MultipiecePart[] parts = Parts;
		for (int i = 0; i < parts.Length; i++)
		{
			foreach (Placeable childPiece in parts[i].ChildPieces)
			{
				if (childPiece != null)
				{
					num += childPiece.GetTotalMass();
				}
			}
		}
		return num;
	}

	public override void DetachAllChildren(bool keepAttachments = false)
	{
		base.DetachAllChildren(keepAttachments);
		MultipiecePart[] parts = Parts;
		for (int i = 0; i < parts.Length; i++)
		{
			parts[i].DetachAllChildren(keepAttachments);
		}
	}

	public override void PickUp()
	{
		base.PickUp();
		MultipiecePart[] parts = Parts;
		for (int i = 0; i < parts.Length; i++)
		{
			parts[i].PickUp();
		}
	}

	public override void SetColor(Color newColor)
	{
		base.SetColor(newColor);
		MultipiecePart[] parts = Parts;
		for (int i = 0; i < parts.Length; i++)
		{
			parts[i].SetColor(newColor);
		}
	}

	public override IEnumerable<Placeable> GetChildrenAndParts()
	{
		foreach (Placeable childrenAndPart in base.GetChildrenAndParts())
		{
			yield return childrenAndPart;
		}
		MultipiecePart[] parts = Parts;
		foreach (MultipiecePart multipiecePart in parts)
		{
			if (multipiecePart != null)
			{
				yield return multipiecePart;
			}
		}
	}

	public override void Tint()
	{
		base.Tint();
		MultipiecePart[] parts = Parts;
		foreach (MultipiecePart multipiecePart in parts)
		{
			if (multipiecePart != null)
			{
				multipiecePart.Tint();
			}
		}
	}
}
