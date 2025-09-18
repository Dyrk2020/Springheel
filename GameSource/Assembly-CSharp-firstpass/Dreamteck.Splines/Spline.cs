using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dreamteck.Splines;

[Serializable]
public class Spline
{
	public enum Direction
	{
		Forward = 1,
		Backward = -1
	}

	public enum Type
	{
		CatmullRom,
		BSpline,
		Bezier,
		Linear
	}

	public SplinePoint[] points = new SplinePoint[0];

	[SerializeField]
	private bool closed;

	public Type type = Type.Bezier;

	public bool linearAverageDirection = true;

	public AnimationCurve customValueInterpolation;

	public AnimationCurve customNormalInterpolation;

	public int sampleRate = 10;

	private static Vector3[] catPoints = new Vector3[4];

	public bool isClosed
	{
		get
		{
			if (closed)
			{
				return points.Length >= 4;
			}
			return false;
		}
		set
		{
		}
	}

	public double moveStep
	{
		get
		{
			if (type == Type.Linear)
			{
				return 1f / (float)(points.Length - 1);
			}
			return 1f / (float)(iterations - 1);
		}
		set
		{
		}
	}

	public int iterations
	{
		get
		{
			if (type == Type.Linear)
			{
				return points.Length;
			}
			return sampleRate * (points.Length - 1) - (points.Length - 1) + 1;
		}
	}

	public Spline(Type type)
	{
		this.type = type;
		points = new SplinePoint[0];
	}

	public Spline(Type type, int sampleRate)
	{
		this.type = type;
		this.sampleRate = sampleRate;
		points = new SplinePoint[0];
	}

	public float CalculateLength(double from = 0.0, double to = 1.0, double resolution = 1.0)
	{
		if (points.Length == 0)
		{
			return 0f;
		}
		resolution = DMath.Clamp01(resolution);
		if (resolution == 0.0)
		{
			return 0f;
		}
		from = DMath.Clamp01(from);
		to = DMath.Clamp01(to);
		if (to < from)
		{
			to = from;
		}
		double num = from;
		Vector3 vector = EvaluatePosition(num);
		float num2 = 0f;
		do
		{
			num = DMath.Move(num, to, moveStep / resolution);
			Vector3 vector2 = EvaluatePosition(num);
			num2 += (vector2 - vector).magnitude;
			vector = vector2;
		}
		while (num != to);
		return num2;
	}

	public double Project(Vector3 position, int subdivide = 4, double from = 0.0, double to = 1.0)
	{
		if (points.Length == 0)
		{
			return 0.0;
		}
		if (closed && from == 0.0 && to == 1.0)
		{
			double closestPoint = GetClosestPoint(subdivide, position, from, to, Mathf.RoundToInt(Mathf.Max(iterations / points.Length, 10)) * 5);
			if (closestPoint < moveStep)
			{
				double closestPoint2 = GetClosestPoint(subdivide, position, 0.5, to, Mathf.RoundToInt(Mathf.Max(iterations / points.Length, 10)) * 5);
				if (Vector3.Distance(position, EvaluatePosition(closestPoint2)) < Vector3.Distance(position, EvaluatePosition(closestPoint)))
				{
					return closestPoint2;
				}
			}
			return closestPoint;
		}
		return GetClosestPoint(subdivide, position, from, to, Mathf.RoundToInt(Mathf.Max(iterations / points.Length, 10)) * 5);
	}

	public bool Raycast(out RaycastHit hit, out double hitPercent, LayerMask layerMask, double resolution = 1.0, double from = 0.0, double to = 1.0, QueryTriggerInteraction hitTriggers = QueryTriggerInteraction.UseGlobal)
	{
		resolution = DMath.Clamp01(resolution);
		from = DMath.Clamp01(from);
		to = DMath.Clamp01(to);
		double num = from;
		Vector3 vector = EvaluatePosition(num);
		hitPercent = 0.0;
		if (resolution == 0.0)
		{
			hit = default(RaycastHit);
			hitPercent = 0.0;
			return false;
		}
		do
		{
			double a = num;
			num = DMath.Move(num, to, moveStep / resolution);
			Vector3 vector2 = EvaluatePosition(num);
			if (Physics.Linecast(vector, vector2, out hit, layerMask, hitTriggers))
			{
				double t = (hit.point - vector).sqrMagnitude / (vector2 - vector).sqrMagnitude;
				hitPercent = DMath.Lerp(a, num, t);
				return true;
			}
			vector = vector2;
		}
		while (num != to);
		return false;
	}

	public bool RaycastAll(out RaycastHit[] hits, out double[] hitPercents, LayerMask layerMask, double resolution = 1.0, double from = 0.0, double to = 1.0, QueryTriggerInteraction hitTriggers = QueryTriggerInteraction.UseGlobal)
	{
		resolution = DMath.Clamp01(resolution);
		from = DMath.Clamp01(from);
		to = DMath.Clamp01(to);
		double num = from;
		Vector3 vector = EvaluatePosition(num);
		List<RaycastHit> list = new List<RaycastHit>();
		List<double> list2 = new List<double>();
		if (resolution == 0.0)
		{
			hits = new RaycastHit[0];
			hitPercents = new double[0];
			return false;
		}
		bool result = false;
		do
		{
			double a = num;
			num = DMath.Move(num, to, moveStep / resolution);
			Vector3 vector2 = EvaluatePosition(num);
			RaycastHit[] array = Physics.RaycastAll(vector, vector2 - vector, Vector3.Distance(vector, vector2), layerMask, hitTriggers);
			for (int i = 0; i < array.Length; i++)
			{
				result = true;
				double t = (array[i].point - vector).sqrMagnitude / (vector2 - vector).sqrMagnitude;
				list2.Add(DMath.Lerp(a, num, t));
				list.Add(array[i]);
			}
			vector = vector2;
		}
		while (num != to);
		hits = list.ToArray();
		hitPercents = list2.ToArray();
		return result;
	}

	public double GetPointPercent(int pointIndex)
	{
		return DMath.Clamp01((double)pointIndex / (double)(points.Length - 1));
	}

	public Vector3 EvaluatePosition(double percent)
	{
		if (points.Length == 0)
		{
			return Vector3.zero;
		}
		Vector3 point = default(Vector3);
		EvaluatePosition(ref point, percent);
		return point;
	}

	public SplineSample Evaluate(double percent)
	{
		SplineSample result = new SplineSample();
		Evaluate(result, percent);
		return result;
	}

	public SplineSample Evaluate(int pointIndex)
	{
		SplineSample result = new SplineSample();
		Evaluate(result, GetPointPercent(pointIndex));
		return result;
	}

	public void Evaluate(SplineSample result, int pointIndex)
	{
		Evaluate(result, GetPointPercent(pointIndex));
	}

	public void Evaluate(SplineSample result, double percent)
	{
		if (points.Length == 0)
		{
			result = new SplineSample();
			return;
		}
		percent = DMath.Clamp01(percent);
		if (closed && points.Length <= 2)
		{
			closed = false;
		}
		if (points.Length == 1)
		{
			result.position = points[0].position;
			result.up = points[0].normal;
			result.forward = Vector3.forward;
			result.size = points[0].size;
			result.color = points[0].color;
			result.percent = percent;
			return;
		}
		double num = (double)(points.Length - 1) * percent;
		int num2 = Mathf.Clamp(DMath.FloorInt(num), 0, points.Length - 2);
		double num3 = num - (double)num2;
		Vector3 position = EvaluatePosition(percent);
		result.position = position;
		result.percent = percent;
		if (num2 <= points.Length - 2)
		{
			SplinePoint splinePoint = points[num2 + 1];
			if (num2 == points.Length - 2 && closed)
			{
				splinePoint = points[0];
			}
			float num4 = (float)num3;
			if (customValueInterpolation != null && customValueInterpolation.length > 0)
			{
				num4 = customValueInterpolation.Evaluate(num4);
			}
			float num5 = (float)num3;
			if (customNormalInterpolation != null && customNormalInterpolation.length > 0)
			{
				num5 = customNormalInterpolation.Evaluate(num5);
			}
			result.size = Mathf.Lerp(points[num2].size, splinePoint.size, num4);
			result.color = Color.Lerp(points[num2].color, splinePoint.color, num4);
			result.up = Vector3.Slerp(points[num2].normal, splinePoint.normal, num5);
		}
		else if (closed)
		{
			result.size = points[0].size;
			result.color = points[0].color;
			result.up = points[0].normal;
		}
		else
		{
			result.size = points[num2].size;
			result.color = points[num2].color;
			result.up = points[num2].normal;
		}
		if (type == Type.BSpline)
		{
			double num6 = 1.0 / (double)(iterations - 1);
			if (percent <= 1.0 - num6 && percent >= num6)
			{
				result.forward = EvaluatePosition(percent + num6) - EvaluatePosition(percent - num6);
			}
			else
			{
				Vector3 zero = Vector3.zero;
				Vector3 zero2 = Vector3.zero;
				if (closed)
				{
					zero = ((!(percent < num6)) ? EvaluatePosition(percent - num6) : EvaluatePosition(1.0 - (num6 - percent)));
					zero2 = ((!(percent > 1.0 - num6)) ? EvaluatePosition(percent + num6) : EvaluatePosition(num6 - (1.0 - percent)));
					result.forward = zero2 - zero;
				}
				else
				{
					zero = result.position - EvaluatePosition(percent - num6);
					zero2 = EvaluatePosition(percent + num6) - result.position;
					result.forward = Vector3.Slerp(zero2, zero, zero.magnitude / zero2.magnitude);
				}
			}
		}
		else
		{
			EvaluateTangent(ref result.forward, percent);
		}
		result.forward.Normalize();
	}

	public void Evaluate(ref SplineSample[] samples, double from = 0.0, double to = 1.0)
	{
		if (points.Length == 0)
		{
			samples = new SplineSample[0];
			return;
		}
		from = DMath.Clamp01(from);
		to = DMath.Clamp(to, from, 1.0);
		double a = from * (double)(iterations - 1);
		int num = DMath.CeilInt(to * (double)(iterations - 1)) - DMath.FloorInt(a) + 1;
		if (samples == null)
		{
			samples = new SplineSample[num];
		}
		else if (samples.Length != num)
		{
			samples = new SplineSample[num];
		}
		double num2 = from;
		double amount = moveStep;
		int num3 = 0;
		while (true)
		{
			samples[num3] = Evaluate(num2);
			num3++;
			if (num3 < samples.Length)
			{
				num2 = DMath.Move(num2, to, amount);
				continue;
			}
			break;
		}
	}

	public void EvaluateUniform(ref SplineSample[] samples, ref double[] originalSamplePercents, double from = 0.0, double to = 1.0)
	{
		if (points.Length == 0)
		{
			samples = new SplineSample[0];
			return;
		}
		from = DMath.Clamp01(from);
		to = DMath.Clamp(to, from, 1.0);
		double a = from * (double)(iterations - 1);
		int num = DMath.CeilInt(to * (double)(iterations - 1)) - DMath.FloorInt(a) + 1;
		if (samples == null || samples.Length != num)
		{
			samples = new SplineSample[num];
		}
		if (originalSamplePercents == null || originalSamplePercents.Length != num)
		{
			originalSamplePercents = new double[num];
		}
		for (int i = 0; i < samples.Length; i++)
		{
			if (samples[i] == null)
			{
				samples[i] = new SplineSample();
			}
		}
		float distance = CalculateLength(from, to) / (float)(iterations - 1);
		Evaluate(samples[0], from);
		samples[0].percent = (originalSamplePercents[0] = from);
		double num2 = from;
		float moved = 0f;
		for (int j = 1; j < samples.Length - 1; j++)
		{
			Evaluate(samples[j], Travel(num2, distance, out moved, Direction.Forward));
			num2 = samples[j].percent;
			originalSamplePercents[j] = num2;
			samples[j].percent = DMath.Lerp(from, to, (double)j / (double)(samples.Length - 1));
		}
		Evaluate(samples[samples.Length - 1], to);
		samples[samples.Length - 1].percent = (originalSamplePercents[originalSamplePercents.Length - 1] = to);
	}

	public void EvaluatePositions(ref Vector3[] positions, double from = 0.0, double to = 1.0)
	{
		if (points.Length == 0)
		{
			positions = new Vector3[0];
			return;
		}
		from = DMath.Clamp01(from);
		to = DMath.Clamp(to, from, 1.0);
		double a = from * (double)(iterations - 1);
		int num = DMath.CeilInt(to * (double)(iterations - 1)) - DMath.FloorInt(a) + 1;
		if (positions.Length != num)
		{
			positions = new Vector3[num];
		}
		double num2 = from;
		double amount = moveStep;
		int num3 = 0;
		while (true)
		{
			positions[num3] = EvaluatePosition(num2);
			num3++;
			if (num3 < positions.Length)
			{
				num2 = DMath.Move(num2, to, amount);
				continue;
			}
			break;
		}
	}

	public double Travel(double start, float distance, out float moved, Direction direction)
	{
		moved = 0f;
		if (points.Length <= 1)
		{
			return 0.0;
		}
		if (direction == Direction.Forward && start >= 1.0)
		{
			return 1.0;
		}
		if (direction == Direction.Backward && start <= 0.0)
		{
			return 0.0;
		}
		if (distance == 0f)
		{
			return DMath.Clamp01(start);
		}
		Vector3 point = Vector3.zero;
		EvaluatePosition(ref point, start);
		Vector3 b = point;
		double a = start;
		int num = iterations - 1;
		int num2 = ((direction == Direction.Forward) ? DMath.CeilInt(start * (double)num) : DMath.FloorInt(start * (double)num));
		float num3 = 0f;
		double num4 = start;
		while (true)
		{
			num4 = (double)num2 / (double)num;
			point = EvaluatePosition(num4);
			num3 = Vector3.Distance(point, b);
			b = point;
			moved += num3;
			if (moved >= distance)
			{
				break;
			}
			a = num4;
			if (direction == Direction.Forward)
			{
				if (num2 == num)
				{
					break;
				}
				num2++;
			}
			else
			{
				if (num2 == 0)
				{
					break;
				}
				num2--;
			}
		}
		return DMath.Lerp(a, num4, 1f - (moved - distance) / num3);
	}

	public double Travel(double start, float distance, Direction direction = Direction.Forward)
	{
		float moved;
		return Travel(start, distance, out moved, direction);
	}

	public void EvaluatePosition(ref Vector3 point, double percent)
	{
		percent = DMath.Clamp01(percent);
		double num = (double)(points.Length - 1) * percent;
		int num2 = DMath.FloorInt(num);
		if (type == Type.Bezier)
		{
			num2 = Mathf.Clamp(num2, 0, Mathf.Max(points.Length - 2, 0));
		}
		GetPoint(ref point, num - (double)num2, num2);
	}

	public void EvaluateTangent(ref Vector3 tangent, double percent)
	{
		percent = DMath.Clamp01(percent);
		double num = (double)(points.Length - 1) * percent;
		int num2 = DMath.FloorInt(num);
		if (type == Type.Bezier)
		{
			num2 = Mathf.Clamp(num2, 0, Mathf.Max(points.Length - 2, 0));
		}
		GetTangent(ref tangent, num - (double)num2, num2);
	}

	private double GetClosestPoint(int iterations, Vector3 point, double start, double end, int slices)
	{
		if (iterations <= 0)
		{
			float sqrMagnitude = (point - EvaluatePosition(start)).sqrMagnitude;
			float sqrMagnitude2 = (point - EvaluatePosition(end)).sqrMagnitude;
			if (sqrMagnitude < sqrMagnitude2)
			{
				return start;
			}
			if (sqrMagnitude2 < sqrMagnitude)
			{
				return end;
			}
			return (start + end) / 2.0;
		}
		double num = 0.0;
		float num2 = float.PositiveInfinity;
		double num3 = (end - start) / (double)slices;
		double num4 = start;
		Vector3 point2 = Vector3.zero;
		while (true)
		{
			EvaluatePosition(ref point2, num4);
			float sqrMagnitude3 = (point - point2).sqrMagnitude;
			if (sqrMagnitude3 < num2)
			{
				num2 = sqrMagnitude3;
				num = num4;
			}
			if (num4 == end)
			{
				break;
			}
			num4 = DMath.Move(num4, end, num3);
		}
		double num5 = num - num3;
		if (num5 < start)
		{
			num5 = start;
		}
		double num6 = num + num3;
		if (num6 > end)
		{
			num6 = end;
		}
		return GetClosestPoint(--iterations, point, num5, num6, slices);
	}

	public void Break()
	{
		Break(0);
	}

	public void Break(int at)
	{
		if (closed && at < points.Length)
		{
			SplinePoint[] array = new SplinePoint[at];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = points[i];
			}
			for (int j = at; j < points.Length - 1; j++)
			{
				points[j - at] = points[j];
			}
			for (int k = 0; k < array.Length; k++)
			{
				points[points.Length - at + k - 1] = array[k];
			}
			points[points.Length - 1] = points[0];
			closed = false;
		}
	}

	public void Close()
	{
		if (points.Length < 4)
		{
			Debug.LogError("Points need to be at least 4 to close the spline");
		}
		else
		{
			closed = true;
		}
	}

	public void CatToBezierTangents()
	{
		switch (type)
		{
		case Type.Linear:
		{
			for (int j = 0; j < points.Length; j++)
			{
				points[j].type = SplinePoint.Type.Broken;
				points[j].SetTangentPosition(points[j].position);
				points[j].SetTangent2Position(points[j].position);
			}
			break;
		}
		case Type.CatmullRom:
		{
			for (int i = 0; i < points.Length; i++)
			{
				GetCatPoints(i);
				points[i].type = SplinePoint.Type.SmoothMirrored;
				if (i == 0)
				{
					Vector3 vector = catPoints[1] - catPoints[2];
					if (closed)
					{
						vector = points[points.Length - 2].position - points[i + 1].position;
						points[i].SetTangentPosition(points[i].position + vector / 6f);
					}
					else
					{
						points[i].SetTangentPosition(points[i].position + vector / 3f);
					}
				}
				else if (i == points.Length - 1)
				{
					Vector3 vector2 = catPoints[2] - catPoints[3];
					points[i].SetTangentPosition(points[i].position + vector2 / 3f);
				}
				else
				{
					Vector3 vector3 = catPoints[0] - catPoints[2];
					points[i].SetTangentPosition(points[i].position + vector3 / 6f);
				}
			}
			break;
		}
		}
		type = Type.Bezier;
	}

	private void GetPoint(ref Vector3 point, double percent, int pointIndex)
	{
		if (closed && points.Length > 3)
		{
			if (pointIndex == points.Length - 2)
			{
				points[0].SetTangentPosition(points[points.Length - 1].tangent);
				points[points.Length - 1] = points[0];
			}
		}
		else
		{
			closed = false;
		}
		switch (type)
		{
		case Type.CatmullRom:
			CatmullRomGetPoint(ref point, percent, pointIndex);
			break;
		case Type.Bezier:
			BezierGetPoint(ref point, percent, pointIndex);
			break;
		case Type.BSpline:
			BSPGetPoint(ref point, percent, pointIndex);
			break;
		case Type.Linear:
			LinearGetPoint(ref point, percent, pointIndex);
			break;
		}
	}

	private void GetTangent(ref Vector3 tangent, double percent, int pointIndex)
	{
		switch (type)
		{
		case Type.CatmullRom:
			GetCatmullRomTangent(ref tangent, percent, pointIndex);
			break;
		case Type.Bezier:
			BezierGetTangent(ref tangent, percent, pointIndex);
			break;
		case Type.Linear:
			LinearGetTangent(ref tangent, percent, pointIndex);
			break;
		case Type.BSpline:
			break;
		}
	}

	private void LinearGetPoint(ref Vector3 point, double t, int i)
	{
		if (points.Length == 0)
		{
			point = Vector3.zero;
		}
		else if (i < points.Length - 1)
		{
			t = DMath.Clamp01(t);
			i = Mathf.Clamp(i, 0, points.Length - 2);
			point = Vector3.Lerp(points[i].position, points[i + 1].position, (float)t);
		}
		else
		{
			point = points[i].position;
		}
	}

	private void LinearGetTangent(ref Vector3 tangent, double t, int i)
	{
		if (points.Length == 0)
		{
			tangent = Vector3.forward;
			return;
		}
		GetCatPoints(i);
		if (linearAverageDirection)
		{
			tangent = Vector3.Slerp(catPoints[1] - catPoints[0], catPoints[2] - catPoints[1], 0.5f);
		}
		else
		{
			tangent = catPoints[2] - catPoints[1];
		}
	}

	private void BSPGetPoint(ref Vector3 point, double time, int i)
	{
		if (points.Length != 0)
		{
			point = points[0].position;
		}
		if (points.Length > 1)
		{
			float num = (float)DMath.Clamp01(time);
			GetCatPoints(i);
			point = ((-catPoints[0] + catPoints[2]) / 2f + num * ((catPoints[0] - 2f * catPoints[1] + catPoints[2]) / 2f + num * (-catPoints[0] + 3f * catPoints[1] - 3f * catPoints[2] + catPoints[3]) / 6f)) * num + (catPoints[0] + 4f * catPoints[1] + catPoints[2]) / 6f;
		}
	}

	private void BezierGetPoint(ref Vector3 point, double t, int i)
	{
		if (points.Length != 0)
		{
			point = points[0].position;
			if (points.Length != 1 && i < points.Length - 1)
			{
				t = DMath.Clamp01(t);
				i = Mathf.Clamp(i, 0, points.Length - 2);
				float num = (float)t;
				float num2 = 1f - num;
				point = num2 * num2 * num2 * points[i].position + 3f * num2 * num2 * num * points[i].tangent2 + 3f * num2 * num * num * points[i + 1].tangent + num * num * num * points[i + 1].position;
			}
		}
	}

	private void BezierGetTangent(ref Vector3 tangent, double t, int i)
	{
		if (points.Length != 0)
		{
			tangent = points[0].tangent2;
			if (points.Length != 1 && i < points.Length - 1)
			{
				t = DMath.Clamp01(t);
				i = Mathf.Clamp(i, 0, points.Length - 2);
				float num = (float)t;
				float num2 = 1f - num;
				tangent = -3f * num2 * num2 * points[i].position + 3f * num2 * num2 * points[i].tangent2 - 6f * num * num2 * points[i].tangent2 - 3f * num * num * points[i + 1].tangent + 6f * num * num2 * points[i + 1].tangent + 3f * num * num * points[i + 1].position;
			}
		}
	}

	private void CatmullRomGetPoint(ref Vector3 point, double t, int i)
	{
		float num = (float)t;
		float num2 = num * num;
		float num3 = num2 * num;
		if (points.Length != 0)
		{
			point = points[0].position;
		}
		if (i < points.Length && points.Length > 1)
		{
			GetCatPoints(i);
			point = 0.5f * (2f * catPoints[1] + (-catPoints[0] + catPoints[2]) * num + (2f * catPoints[0] - 5f * catPoints[1] + 4f * catPoints[2] - catPoints[3]) * num2 + (-catPoints[0] + 3f * catPoints[1] - 3f * catPoints[2] + catPoints[3]) * num3);
		}
	}

	private void GetCatmullRomTangent(ref Vector3 direction, double t, int i)
	{
		float num = (float)t;
		float num2 = num * num;
		if (points.Length != 0)
		{
			direction = Vector3.forward;
		}
		if (i < points.Length && points.Length > 1)
		{
			GetCatPoints(i);
			direction = (6f * num2 - 6f * num) * catPoints[1] + (3f * num2 - 4f * num + 1f) * (catPoints[2] - catPoints[0]) * 0.5f + (-6f * num2 + 6f * num) * catPoints[2] + (3f * num2 - 2f * num) * (catPoints[3] - catPoints[1]) * 0.5f;
		}
	}

	private void GetCatPoints(int i)
	{
		if (i > 0)
		{
			catPoints[0] = points[i - 1].position;
		}
		else if (closed && points.Length - 2 > i)
		{
			catPoints[0] = points[points.Length - 2].position;
		}
		else if (i + 1 < points.Length)
		{
			catPoints[0] = points[i].position + (points[i].position - points[i + 1].position);
		}
		else
		{
			catPoints[0] = points[i].position;
		}
		catPoints[1] = points[i].position;
		if (i + 1 < points.Length)
		{
			catPoints[2] = points[i + 1].position;
		}
		else if (closed && i + 2 - points.Length != i)
		{
			catPoints[2] = points[i + 2 - points.Length].position;
		}
		else
		{
			catPoints[2] = catPoints[1] + (catPoints[1] - catPoints[0]);
		}
		if (i + 2 < points.Length)
		{
			catPoints[3] = points[i + 2].position;
		}
		else if (closed && i + 3 - points.Length != i)
		{
			catPoints[3] = points[i + 3 - points.Length].position;
		}
		else
		{
			catPoints[3] = catPoints[2] + (catPoints[2] - catPoints[1]);
		}
	}

	public static void FormatFromTo(ref double from, ref double to, bool preventInvert = true)
	{
		from = DMath.Clamp01(from);
		to = DMath.Clamp01(to);
		if (preventInvert && from > to)
		{
			double num = from;
			from = to;
			to = num;
		}
		else
		{
			to = DMath.Clamp(to, 0.0, 1.0);
		}
	}
}
