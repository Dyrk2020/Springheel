using System.Collections.Generic;
using UnityEngine;

public class PS2DSnap
{
	private float snapDistance = 1f;

	private List<PS2DSnapAxis> axes = new List<PS2DSnapAxis>(4);

	public Vector2 snapLocation;

	public int snapPoint1;

	public int snapPoint2;

	private int snapAxis1;

	private int snapAxis2;

	public PS2DSnap()
	{
		axes.Add(new PS2DSnapAxis(Vector2.right));
		axes.Add(new PS2DSnapAxis(Vector2.up));
		axes.Add(new PS2DSnapAxis(Vector2.right + Vector2.up));
		axes.Add(new PS2DSnapAxis(Vector2.left + Vector2.up));
		snapPoint1 = -1;
		snapPoint2 = -1;
	}

	public void Reset(float size)
	{
		for (int i = 0; i < axes.Count; i++)
		{
			axes[i].Reset(snapDistance, size);
		}
	}

	public void CheckPoint(int pointId, Vector2 pointDragging, Vector2 pointStatic)
	{
		for (int i = 0; i < axes.Count; i++)
		{
			axes[i].CheckPoint(pointId, pointDragging, pointStatic);
		}
	}

	public int GetClosestAxes()
	{
		snapAxis1 = -1;
		snapAxis2 = -1;
		float num = -5f;
		for (int i = 0; i < axes.Count; i++)
		{
			if (axes[i].snapPoint != -1 && (num < -1f || axes[i].snapDist < num))
			{
				snapAxis1 = i;
				num = axes[i].snapDist;
			}
		}
		if (snapAxis1 != -1)
		{
			num = -5f;
			for (int j = 0; j < axes.Count; j++)
			{
				if (j != snapAxis1 && axes[j].snapPoint != -1 && (num < -1f || axes[j].snapDist < num))
				{
					snapAxis2 = j;
					num = axes[j].snapDist;
				}
			}
		}
		if (snapAxis2 > -1)
		{
			snapLocation = axes[snapAxis1].GetIntersection(axes[snapAxis2]);
			snapPoint1 = axes[snapAxis1].snapPoint;
			snapPoint2 = axes[snapAxis2].snapPoint;
			return 2;
		}
		if (snapAxis1 > -1)
		{
			snapLocation = axes[snapAxis1].baseLocation;
			snapPoint1 = axes[snapAxis1].snapPoint;
			snapPoint2 = -1;
			return 1;
		}
		snapPoint1 = -1;
		snapPoint2 = -1;
		return 0;
	}
}
