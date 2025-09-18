using System;
using UnityEngine;

[Serializable]
public class PS2DColliderPoint
{
	public Vector2 position = Vector2.zero;

	public Vector2 wPosition = Vector2.zero;

	public Vector2 normal = Vector2.zero;

	public float signedAngle;

	public PS2DDirection direction;

	public PS2DColliderPoint(Vector2 position)
	{
		this.position = position;
	}
}
