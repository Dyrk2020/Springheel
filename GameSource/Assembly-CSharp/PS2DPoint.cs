using System;
using UnityEngine;

[Serializable]
public class PS2DPoint
{
	public Vector2 position = Vector2.zero;

	public float curve;

	public string name;

	public bool selected;

	public bool clockwise = true;

	public Vector2 median = Vector2.zero;

	public Vector2 handleP = Vector2.zero;

	public Vector2 handleN = Vector2.zero;

	public int controlID;

	public int controlPID;

	public int controlNID;

	public PS2DPointType pointType;

	public PS2DPoint(Vector2 position, string name = "")
	{
		this.position = position;
		this.name = name;
		handleP = position;
		handleN = position;
	}

	public void Move(Vector2 diff, bool moveHandles)
	{
		position += diff;
		if (moveHandles)
		{
			handleN += diff;
			handleP += diff;
		}
	}
}
