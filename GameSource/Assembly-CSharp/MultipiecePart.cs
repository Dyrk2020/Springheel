using UnityEngine;

public class MultipiecePart : ActiveBlock
{
	public SpriteRenderer PartSprite;

	public MultipieceBlock MainBlock;

	public GameObject CollidingWith
	{
		get
		{
			foreach (CheckColliding item in PlacementCollidersNew)
			{
				if (item.Required && !item.ReverseAttach)
				{
					return item.CollidingObject;
				}
			}
			return null;
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (MainBlock != null)
		{
			MainBlock.RemovePart(this);
		}
	}

	public override float GetTotalMass()
	{
		if (MainBlock != null)
		{
			return MainBlock.GetTotalMass();
		}
		return base.GetTotalMass();
	}
}
