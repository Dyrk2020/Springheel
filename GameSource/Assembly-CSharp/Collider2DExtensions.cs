using UnityEngine;

public static class Collider2DExtensions
{
	public static Bounds CalculateScaledBounds(this BoxCollider2D collider)
	{
		Transform transform = collider.transform;
		Vector3 localScale = transform.localScale;
		Vector2 size = collider.size;
		Vector2 offset = collider.offset;
		Vector3 size2 = new Vector3(size.x * localScale.x, size.y * localScale.y, 1f);
		Vector3 vector = new Vector3(offset.x * localScale.x, offset.y * localScale.y, 0f);
		return new Bounds(transform.position + vector, size2);
	}
}
