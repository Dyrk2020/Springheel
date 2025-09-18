using UnityEngine;

public class CollisionPiece : MonoBehaviour
{
	public bool Colliding;

	public bool CollidingWall;

	public bool CollidingHazard;

	public static int optionACheckMask = 128;

	public static int optionAIgnoreMask = 256;

	public static int optionBMask = 10368;

	public static int optionCMask = 256;

	private void OnTriggerStay2D(Collider2D c)
	{
		CollisionTag component = c.GetComponent<CollisionTag>();
		if (component != null)
		{
			if (component.ContainsAnyTag(optionACheckMask) && !component.ContainsAnyTag(optionAIgnoreMask))
			{
				Colliding = true;
				CollidingWall = true;
			}
			if (component.ContainsAnyTag(optionBMask))
			{
				Colliding = true;
				CollidingWall = false;
			}
			if (component.ContainsAnyTag(optionCMask))
			{
				CollidingHazard = true;
			}
		}
	}
}
