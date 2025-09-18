using UnityEngine;

namespace Dreamteck.Splines.Primitives;

public class SplinePrimitive
{
	protected bool closed;

	protected SplinePoint[] points = new SplinePoint[0];

	public Vector3 offset = Vector3.zero;

	public Vector3 rotation = Vector3.zero;

	public bool is2D;

	public virtual void Calculate()
	{
		Generate();
		ApplyOffset();
	}

	protected virtual void Generate()
	{
	}

	public Spline CreateSpline()
	{
		Generate();
		ApplyOffset();
		Spline spline = new Spline(GetSplineType());
		spline.points = points;
		if (closed)
		{
			spline.Close();
		}
		return spline;
	}

	public void UpdateSpline(Spline spline)
	{
		Generate();
		ApplyOffset();
		spline.type = GetSplineType();
		spline.points = points;
		if (closed)
		{
			spline.Close();
		}
		else if (spline.isClosed)
		{
			spline.Break();
		}
	}

	public SplineComputer CreateSplineComputer(string name, Vector3 position, Quaternion rotation)
	{
		Generate();
		ApplyOffset();
		SplineComputer splineComputer = new GameObject(name).AddComponent<SplineComputer>();
		splineComputer.SetPoints(points, SplineComputer.Space.Local);
		if (closed)
		{
			splineComputer.Close();
		}
		splineComputer.transform.position = position;
		splineComputer.transform.rotation = rotation;
		return splineComputer;
	}

	public void UpdateSplineComputer(SplineComputer comp)
	{
		Generate();
		ApplyOffset();
		comp.type = GetSplineType();
		comp.SetPoints(points, SplineComputer.Space.Local);
		if (closed)
		{
			comp.Close();
		}
		else if (comp.isClosed)
		{
			comp.Break();
		}
	}

	public SplinePoint[] GetPoints()
	{
		return points;
	}

	public virtual Spline.Type GetSplineType()
	{
		return Spline.Type.CatmullRom;
	}

	public bool GetIsClosed()
	{
		return closed;
	}

	private void ApplyOffset()
	{
		Quaternion quaternion = Quaternion.Euler(rotation);
		if (is2D)
		{
			quaternion = Quaternion.AngleAxis(0f - rotation.z, Vector3.forward) * Quaternion.AngleAxis(90f, Vector3.right);
		}
		for (int i = 0; i < points.Length; i++)
		{
			points[i].position = quaternion * points[i].position;
			points[i].tangent = quaternion * points[i].tangent;
			points[i].tangent2 = quaternion * points[i].tangent2;
			points[i].normal = quaternion * points[i].normal;
		}
		for (int j = 0; j < points.Length; j++)
		{
			points[j].SetPosition(points[j].position + offset);
		}
	}

	protected void CreatePoints(int count, SplinePoint.Type type)
	{
		if (points.Length != count)
		{
			points = new SplinePoint[count];
		}
		for (int i = 0; i < points.Length; i++)
		{
			points[i].type = type;
			points[i].normal = Vector3.up;
			points[i].color = Color.white;
			points[i].size = 1f;
		}
	}
}
