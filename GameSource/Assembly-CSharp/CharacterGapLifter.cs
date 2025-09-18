using UnityEngine;

public class CharacterGapLifter : MonoBehaviour
{
	public Transform[] rayOrigins;

	public LayerMask layerMask;

	public float raycastLength = 0.2f;

	public float lastCollisionDistance;

	public float margin = 0.2f;

	public bool speedCheck = true;

	public bool insideWallCheck = true;

	public bool obstacleStraightDown;

	public bool bothCollidersHitting;

	private RaycastHit2D[] hit = new RaycastHit2D[4];

	public bool CanFloat()
	{
		Physics2D.queriesHitTriggers = false;
		lastCollisionDistance = 9999f;
		obstacleStraightDown = false;
		bothCollidersHitting = true;
		for (int i = 0; i < rayOrigins.Length; i++)
		{
			if (Physics2D.RaycastNonAlloc(rayOrigins[i].position, Vector3.down, hit, raycastLength, layerMask.value) == 0)
			{
				bothCollidersHitting = false;
			}
			else
			{
				lastCollisionDistance = Mathf.Min(hit[0].distance, lastCollisionDistance);
			}
		}
		if (Physics2D.RaycastNonAlloc(base.transform.position, Vector3.down, hit, raycastLength * 2f, layerMask.value) > 0 && hit[0].collider != null)
		{
			obstacleStraightDown = true;
		}
		if (bothCollidersHitting)
		{
			return !obstacleStraightDown;
		}
		return false;
	}
}
