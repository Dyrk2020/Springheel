using UnityEngine;

namespace Dreamteck.Splines.Primitives;

public class Star : SplinePrimitive
{
	public float radius = 1f;

	public float depth = 0.5f;

	public int sides = 5;

	public override Spline.Type GetSplineType()
	{
		return Spline.Type.Linear;
	}

	protected override void Generate()
	{
		base.Generate();
		closed = true;
		CreatePoints(sides * 2 + 1, SplinePoint.Type.SmoothMirrored);
		float num = radius * depth;
		for (int i = 0; i < sides * 2; i++)
		{
			float num2 = (float)i / (float)(sides * 2);
			Vector3 position = Quaternion.AngleAxis(180f + 360f * num2, Vector3.forward) * Vector3.up * (((float)i % 2f == 0f) ? radius : num);
			points[i].SetPosition(position);
		}
		points[points.Length - 1] = points[0];
	}
}
