using UnityEngine;

namespace Dreamteck.Splines.Primitives;

public class Ngon : SplinePrimitive
{
	public float radius = 1f;

	public int sides = 3;

	public override Spline.Type GetSplineType()
	{
		return Spline.Type.Linear;
	}

	protected override void Generate()
	{
		base.Generate();
		closed = true;
		CreatePoints(sides + 1, SplinePoint.Type.SmoothMirrored);
		for (int i = 0; i < sides; i++)
		{
			float num = (float)i / (float)sides;
			Vector3 position = Quaternion.AngleAxis(360f * num, Vector3.forward) * Vector3.up * radius;
			points[i].SetPosition(position);
		}
		points[points.Length - 1] = points[0];
	}
}
