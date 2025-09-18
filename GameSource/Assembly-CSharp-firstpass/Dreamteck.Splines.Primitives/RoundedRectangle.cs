using UnityEngine;

namespace Dreamteck.Splines.Primitives;

public class RoundedRectangle : SplinePrimitive
{
	public Vector2 size = Vector2.one;

	public float xRadius = 0.25f;

	public float yRadius = 0.25f;

	public override Spline.Type GetSplineType()
	{
		return Spline.Type.Bezier;
	}

	protected override void Generate()
	{
		base.Generate();
		closed = true;
		CreatePoints(9, SplinePoint.Type.Broken);
		Vector2 vector = size - new Vector2(xRadius, yRadius) * 2f;
		points[0].SetPosition(Vector3.up / 2f * vector.y + Vector3.left / 2f * size.x);
		points[1].SetPosition(Vector3.up / 2f * size.y + Vector3.left / 2f * vector.x);
		points[2].SetPosition(Vector3.up / 2f * size.y + Vector3.right / 2f * vector.x);
		points[3].SetPosition(Vector3.up / 2f * vector.y + Vector3.right / 2f * size.x);
		points[4].SetPosition(Vector3.down / 2f * vector.y + Vector3.right / 2f * size.x);
		points[5].SetPosition(Vector3.down / 2f * size.y + Vector3.right / 2f * vector.x);
		points[6].SetPosition(Vector3.down / 2f * size.y + Vector3.left / 2f * vector.x);
		points[7].SetPosition(Vector3.down / 2f * vector.y + Vector3.left / 2f * size.x);
		float num = 2f * (Mathf.Sqrt(2f) - 1f) / 3f * xRadius * 2f;
		float num2 = 2f * (Mathf.Sqrt(2f) - 1f) / 3f * yRadius * 2f;
		points[0].SetTangent2Position(points[0].position + Vector3.up * num2);
		points[1].SetTangentPosition(points[1].position + Vector3.left * num);
		points[2].SetTangent2Position(points[2].position + Vector3.right * num);
		points[3].SetTangentPosition(points[3].position + Vector3.up * num2);
		points[4].SetTangent2Position(points[4].position + Vector3.down * num2);
		points[5].SetTangentPosition(points[5].position + Vector3.right * num);
		points[6].SetTangent2Position(points[6].position + Vector3.left * num);
		points[7].SetTangentPosition(points[7].position + Vector3.down * num2);
		points[8] = points[0];
	}
}
