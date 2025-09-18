using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Dreamteck.Splines.IO;

public class SplineParser
{
	internal class Transformation
	{
		protected static Matrix4x4 matrix;

		internal static void ResetMatrix()
		{
			matrix.SetTRS(Vector3.zero, Quaternion.identity, Vector3.one);
		}

		internal virtual void Push()
		{
		}

		internal static void Apply(SplinePoint[] points)
		{
			for (int i = 0; i < points.Length; i++)
			{
				SplinePoint splinePoint = points[i];
				splinePoint.position = matrix.MultiplyPoint(splinePoint.position);
				splinePoint.tangent = matrix.MultiplyPoint(splinePoint.tangent);
				splinePoint.tangent2 = matrix.MultiplyPoint(splinePoint.tangent2);
				points[i] = splinePoint;
			}
		}
	}

	internal class Translate : Transformation
	{
		private Vector2 offset = Vector2.zero;

		public Translate(Vector2 o)
		{
			offset = o;
		}

		internal override void Push()
		{
			Matrix4x4 matrix4x = default(Matrix4x4);
			matrix4x.SetTRS(new Vector2(offset.x, 0f - offset.y), Quaternion.identity, Vector3.one);
			Transformation.matrix *= matrix4x;
		}
	}

	internal class Rotate : Transformation
	{
		private float angle;

		public Rotate(float a)
		{
			angle = a;
		}

		internal override void Push()
		{
			Matrix4x4 matrix4x = default(Matrix4x4);
			matrix4x.SetTRS(Vector3.zero, Quaternion.AngleAxis(angle, Vector3.back), Vector3.one);
			Transformation.matrix *= matrix4x;
		}
	}

	internal class Scale : Transformation
	{
		private Vector2 multiplier = Vector2.one;

		public Scale(Vector2 s)
		{
			multiplier = s;
		}

		internal override void Push()
		{
			Matrix4x4 matrix4x = default(Matrix4x4);
			matrix4x.SetTRS(Vector3.zero, Quaternion.identity, multiplier);
			Transformation.matrix *= matrix4x;
		}
	}

	internal class SkewX : Transformation
	{
		private float amount;

		public SkewX(float a)
		{
			amount = a;
		}

		internal override void Push()
		{
			Transformation.matrix *= new Matrix4x4
			{
				[0, 0] = 1f,
				[1, 1] = 1f,
				[2, 2] = 1f,
				[3, 3] = 1f,
				[0, 1] = Mathf.Tan((0f - amount) * (MathF.PI / 180f))
			};
		}
	}

	internal class SkewY : Transformation
	{
		private float amount;

		public SkewY(float a)
		{
			amount = a;
		}

		internal override void Push()
		{
			Transformation.matrix *= new Matrix4x4
			{
				[0, 0] = 1f,
				[1, 1] = 1f,
				[2, 2] = 1f,
				[3, 3] = 1f,
				[1, 0] = Mathf.Tan((0f - amount) * (MathF.PI / 180f))
			};
		}
	}

	internal class MatrixTransform : Transformation
	{
		private Matrix4x4 transformMatrix;

		public MatrixTransform(float a, float b, float c, float d, float e, float f)
		{
			transformMatrix.SetRow(0, new Vector4(a, c, 0f, e));
			transformMatrix.SetRow(1, new Vector4(b, d, 0f, 0f - f));
			transformMatrix.SetRow(2, new Vector4(0f, 0f, 1f, 0f));
			transformMatrix.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
		}

		internal override void Push()
		{
			Transformation.matrix *= transformMatrix;
		}
	}

	internal class SplineDefinition
	{
		internal string name = "";

		internal Spline.Type type = Spline.Type.Linear;

		internal List<SplinePoint> points = new List<SplinePoint>();

		internal bool closed;

		internal Vector3 position = Vector3.zero;

		internal Vector3 tangent = Vector3.zero;

		internal Vector3 tangent2 = Vector3.zero;

		internal Vector3 normal = Vector3.back;

		internal float size = 1f;

		internal Color color = Color.white;

		internal int pointCount => points.Count;

		internal SplineDefinition(string n, Spline.Type t)
		{
			name = n;
			type = t;
		}

		internal SplineDefinition(string n, Spline spline)
		{
			name = n;
			type = spline.type;
			closed = spline.isClosed;
			points = new List<SplinePoint>(spline.points);
		}

		internal SplinePoint GetLastPoint()
		{
			if (points.Count == 0)
			{
				return default(SplinePoint);
			}
			return points[points.Count - 1];
		}

		internal void SetLastPoint(SplinePoint point)
		{
			if (points.Count != 0)
			{
				points[points.Count - 1] = point;
			}
		}

		internal void CreateClosingPoint()
		{
			SplinePoint item = new SplinePoint(points[0]);
			points.Add(item);
		}

		internal void CreateSmooth()
		{
			points.Add(new SplinePoint(position, tangent, normal, size, color));
		}

		internal void CreateBroken()
		{
			SplinePoint item = new SplinePoint(new SplinePoint(position, tangent, normal, size, color));
			item.type = SplinePoint.Type.Broken;
			item.SetTangent2Position(item.position);
			item.normal = normal;
			item.color = color;
			item.size = size;
			points.Add(item);
		}

		internal void CreateLinear()
		{
			tangent = position;
			CreateSmooth();
		}

		internal SplineComputer CreateSplineComputer(Vector3 position, Quaternion rotation)
		{
			GameObject gameObject = new GameObject(name);
			gameObject.transform.position = position;
			gameObject.transform.rotation = rotation;
			SplineComputer splineComputer = gameObject.AddComponent<SplineComputer>();
			splineComputer.type = type;
			if (closed && points[0].type == SplinePoint.Type.Broken)
			{
				points[0].SetTangentPosition(GetLastPoint().tangent2);
			}
			splineComputer.SetPoints(points.ToArray(), SplineComputer.Space.Local);
			if (closed)
			{
				splineComputer.Close();
			}
			return splineComputer;
		}

		internal Spline CreateSpline()
		{
			Spline spline = new Spline(type);
			spline.points = points.ToArray();
			if (closed)
			{
				spline.Close();
			}
			return spline;
		}

		internal void Transform(List<Transformation> trs)
		{
			SplinePoint[] array = points.ToArray();
			Transformation.ResetMatrix();
			foreach (Transformation tr in trs)
			{
				tr.Push();
			}
			Transformation.Apply(array);
			for (int i = 0; i < array.Length; i++)
			{
				points[i] = array[i];
			}
			Transformation.Apply(new SplinePoint[1] { default(SplinePoint) });
		}
	}

	protected string fileName = "";

	private CultureInfo culture = new CultureInfo("en-US");

	private NumberStyles style = NumberStyles.Any;

	internal SplineDefinition buffer;

	public string name => fileName;

	internal Vector2[] ParseVector2(string coord)
	{
		List<float> list = ParseFloatArray(coord.Substring(1));
		int num = list.Count / 2;
		if (num == 0)
		{
			Debug.Log("Error in " + coord);
			return new Vector2[1] { Vector2.zero };
		}
		Vector2[] array = new Vector2[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = new Vector2(list[i * 2], 0f - list[1 + i * 2]);
		}
		return array;
	}

	internal float[] ParseFloat(string coord)
	{
		List<float> list = ParseFloatArray(coord.Substring(1));
		if (list.Count < 1)
		{
			Debug.Log("Error in " + coord);
			return new float[1];
		}
		return list.ToArray();
	}

	internal List<float> ParseFloatArray(string content)
	{
		string text = "";
		List<float> list = new List<float>();
		for (int i = 0; i < content.Length; i++)
		{
			char c = content[i];
			if ((c == ',' || c == '-' || char.IsWhiteSpace(c) || (text.Contains(".") && c == '.')) && !IsWHiteSpace(text))
			{
				float result = 0f;
				float.TryParse(text, style, culture, out result);
				list.Add(result);
				text = "";
				if (c == '-')
				{
					text = "-";
				}
				if (c == '.')
				{
					text = "0.";
				}
			}
			else if (!char.IsWhiteSpace(c))
			{
				text += c;
			}
		}
		if (!IsWHiteSpace(text))
		{
			float result2 = 0f;
			float.TryParse(text, style, culture, out result2);
			list.Add(result2);
		}
		return list;
	}

	public bool IsWHiteSpace(string s)
	{
		for (int i = 0; i < s.Length; i++)
		{
			if (!char.IsWhiteSpace(s[i]))
			{
				return false;
			}
		}
		return true;
	}
}
