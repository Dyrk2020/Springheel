using UnityEngine;

namespace Dreamteck.Splines.Primitives;

public class Spiral : SplinePrimitive
{
	public float startRadius = 1f;

	public float endRadius = 1f;

	public float stretch = 1f;

	public int iterations = 3;

	public bool clockwise = true;

	public AnimationCurve curve = new AnimationCurve();

	public override Spline.Type GetSplineType()
	{
		return Spline.Type.Bezier;
	}

	protected override void Generate()
	{
		base.Generate();
		closed = false;
		CreatePoints(iterations * 4 + 1, SplinePoint.Type.SmoothMirrored);
		float num = Mathf.Abs(endRadius - startRadius) / Mathf.Max(Mathf.Abs(endRadius), Mathf.Abs(startRadius));
		float num2 = 1f;
		if (endRadius > startRadius)
		{
			num2 = -1f;
		}
		float num3 = 0f;
		float num4 = 0f;
		float num5 = (clockwise ? 1f : (-1f));
		for (int i = 0; i <= iterations * 4; i++)
		{
			float num6 = curve.Evaluate((float)i / (float)(iterations * 4));
			float num7 = Mathf.Lerp(startRadius, endRadius, num6);
			Quaternion quaternion = Quaternion.AngleAxis(num3, Vector3.up);
			points[i].position = quaternion * Vector3.forward / 2f * num7 + Vector3.up * num4;
			Quaternion identity = Quaternion.identity;
			identity = ((!(num2 > 0f)) ? Quaternion.AngleAxis(Mathf.Lerp(0f, -14.4f * num5, (1f - num6) * num), Vector3.up) : Quaternion.AngleAxis(Mathf.Lerp(0f, 14.4f * num5, num * num6), Vector3.up));
			if (clockwise)
			{
				points[i].tangent = points[i].position - (identity * quaternion * Vector3.right * num7 + Vector3.up * stretch / 4f) * 2f * (Mathf.Sqrt(2f) - 1f) / 3f;
			}
			else
			{
				points[i].tangent = points[i].position + (identity * quaternion * Vector3.right * num7 - Vector3.up * stretch / 4f) * 2f * (Mathf.Sqrt(2f) - 1f) / 3f;
			}
			points[i].tangent2 = points[i].position - (points[i].tangent - points[i].position);
			num4 += stretch / 4f;
			num3 += 90f * num5;
		}
	}
}
