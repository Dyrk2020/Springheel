using System.Collections.Generic;
using UnityEngine;

public class FloorDetector : MonoBehaviour
{
	private bool floorDetected;

	private List<Collider2D> collidedWith = new List<Collider2D>();

	public bool FloorDetected => floorDetected;

	public List<Collider2D> CollidedWith => collidedWith;

	public void Reset()
	{
		floorDetected = false;
		collidedWith.Clear();
	}

	public void OnTriggerEnter2D(Collider2D collider)
	{
		if (!floorDetected)
		{
			CollisionTag component = collider.GetComponent<CollisionTag>();
			if (!(component == null) && component.ContainsAnyTag(TagComparer.Tag.Solid) && !component.ContainsAnyTag(TagComparer.Tag.Player))
			{
				floorDetected = true;
				collidedWith.Add(collider);
				Debug.Log("Collided with : " + collider.gameObject.name);
			}
		}
	}
}
