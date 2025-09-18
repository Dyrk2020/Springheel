using System;
using UnityEngine;

namespace Dreamteck.Splines;

[Serializable]
public class SampleCollection
{
	[HideInInspector]
	public SplineSample[] samples = new SplineSample[0];

	public int[] optimizedIndices = new int[0];

	public SplineComputer.SampleMode sampleMode;

	private SplineSample _workSample = new SplineSample();

	[HideInInspector]
	public double clipFrom;

	[HideInInspector]
	public double clipTo = 1.0;

	[HideInInspector]
	public bool loopSamples;

	public int Count => samples.Length;

	private bool hasSamples => Count > 0;

	public SampleCollection()
	{
	}

	public SampleCollection(SampleCollection input)
	{
		samples = input.samples;
		optimizedIndices = input.optimizedIndices;
		sampleMode = input.sampleMode;
	}

	public int GetClippedSampleCount(double clipFrom, double clipTo, out int startIndex, out int endIndex)
	{
		startIndex = (endIndex = 0);
		if (sampleMode == SplineComputer.SampleMode.Default)
		{
			startIndex = DMath.FloorInt((double)(Count - 1) * clipFrom);
			endIndex = DMath.CeilInt((double)(Count - 1) * clipTo);
		}
		else
		{
			double lerp = 0.0;
			double lerp2 = 0.0;
			GetSamplingValues(clipFrom, out startIndex, out lerp);
			GetSamplingValues(clipTo, out endIndex, out lerp2);
			if (lerp2 > 0.0 && endIndex < Count - 1)
			{
				endIndex++;
			}
		}
		if (clipTo < clipFrom)
		{
			int num = endIndex + 1;
			int num2 = Count - startIndex;
			return num + num2;
		}
		return endIndex - startIndex + 1;
	}

	public void GetSamplingValues(double percent, out int sampleIndex, out double lerp)
	{
		lerp = 0.0;
		if (sampleMode == SplineComputer.SampleMode.Optimized)
		{
			double num = percent * (double)(optimizedIndices.Length - 1);
			int num2 = DMath.FloorInt(num);
			sampleIndex = optimizedIndices[num2];
			double t = 0.0;
			if (num2 < optimizedIndices.Length - 1)
			{
				double t2 = num - (double)num2;
				double a = (double)num2 / (double)(optimizedIndices.Length - 1);
				double b = (double)(num2 + 1) / (double)(optimizedIndices.Length - 1);
				t = DMath.Lerp(a, b, t2);
			}
			if (sampleIndex < Count - 1)
			{
				lerp = DMath.InverseLerp(samples[sampleIndex].percent, samples[sampleIndex + 1].percent, t);
			}
		}
		else
		{
			sampleIndex = DMath.FloorInt(percent * (double)(Count - 1));
			lerp = (double)(Count - 1) * percent - (double)sampleIndex;
		}
	}

	public Vector3 EvaluatePosition(double percent)
	{
		if (!hasSamples)
		{
			return Vector3.zero;
		}
		GetSamplingValues(percent, out var sampleIndex, out var lerp);
		if (lerp > 0.0)
		{
			return Vector3.Lerp(samples[sampleIndex].position, samples[sampleIndex + 1].position, (float)lerp);
		}
		return samples[sampleIndex].position;
	}

	public SplineSample Evaluate(double percent)
	{
		SplineSample result = new SplineSample();
		Evaluate(percent, result);
		return result;
	}

	public void Evaluate(double percent, SplineSample result)
	{
		if (!hasSamples)
		{
			result = new SplineSample();
			return;
		}
		GetSamplingValues(percent, out var sampleIndex, out var lerp);
		if (lerp > 0.0)
		{
			SplineSample.Lerp(samples[sampleIndex], samples[sampleIndex + 1], lerp, result);
		}
		else
		{
			result.CopyFrom(samples[sampleIndex]);
		}
	}

	public void Evaluate(ref SplineSample[] results, double from = 0.0, double to = 1.0)
	{
		if (!hasSamples)
		{
			results = new SplineSample[0];
			return;
		}
		Spline.FormatFromTo(ref from, ref to);
		GetSamplingValues(from, out var sampleIndex, out var lerp);
		GetSamplingValues(to, out var sampleIndex2, out lerp);
		if (lerp > 0.0 && sampleIndex2 < Count - 1)
		{
			sampleIndex2++;
		}
		int num = sampleIndex2 - sampleIndex + 1;
		if (results == null)
		{
			results = new SplineSample[num];
		}
		else if (results.Length != num)
		{
			results = new SplineSample[num];
		}
		results[0] = Evaluate(from);
		results[results.Length - 1] = Evaluate(to);
		for (int i = 1; i < results.Length - 1; i++)
		{
			results[i] = samples[i + sampleIndex];
		}
	}

	public void EvaluatePositions(ref Vector3[] positions, double from = 0.0, double to = 1.0)
	{
		if (!hasSamples)
		{
			positions = new Vector3[0];
			return;
		}
		Spline.FormatFromTo(ref from, ref to);
		GetSamplingValues(from, out var sampleIndex, out var lerp);
		GetSamplingValues(to, out var sampleIndex2, out lerp);
		if (lerp > 0.0 && sampleIndex2 < Count - 1)
		{
			sampleIndex2++;
		}
		int num = sampleIndex2 - sampleIndex + 1;
		if (positions == null)
		{
			positions = new Vector3[num];
		}
		else if (positions.Length != num)
		{
			positions = new Vector3[num];
		}
		positions[0] = EvaluatePosition(from);
		positions[positions.Length - 1] = EvaluatePosition(to);
		for (int i = 1; i < positions.Length - 1; i++)
		{
			positions[i] = samples[i + sampleIndex].position;
		}
	}

	public double Travel(double start, float distance, Spline.Direction direction, out float moved, double clipFrom = 0.0, double clipTo = 1.0)
	{
		moved = 0f;
		if (!hasSamples)
		{
			return 0.0;
		}
		if (direction == Spline.Direction.Forward && start >= 1.0)
		{
			return clipTo;
		}
		if (direction == Spline.Direction.Backward && start <= 0.0)
		{
			return clipFrom;
		}
		double num = start;
		if (distance == 0f)
		{
			return num;
		}
		Vector3 b = EvaluatePosition(start);
		GetSamplingValues(num, out var sampleIndex, out var lerp);
		if (direction == Spline.Direction.Forward && lerp > 0.0)
		{
			sampleIndex++;
		}
		float num2 = 0f;
		int sampleIndex2 = 0;
		int sampleIndex3 = Count - 1;
		bool flag = clipTo < clipFrom;
		if (flag)
		{
			GetSamplingValues(clipFrom, out sampleIndex2, out lerp);
			GetSamplingValues(clipTo, out sampleIndex3, out lerp);
			if (lerp > 0.0)
			{
				sampleIndex3++;
			}
		}
		while (moved < distance)
		{
			num2 = Vector3.Distance(samples[sampleIndex].position, b);
			moved += num2;
			if (moved >= distance)
			{
				break;
			}
			b = samples[sampleIndex].position;
			num = samples[sampleIndex].percent;
			if (direction == Spline.Direction.Forward)
			{
				if (sampleIndex == Count - 1)
				{
					if (!flag)
					{
						break;
					}
					b = samples[0].position;
					num = samples[0].percent;
					sampleIndex = 1;
				}
				if (flag && sampleIndex == sampleIndex3)
				{
					break;
				}
				sampleIndex++;
				continue;
			}
			if (sampleIndex == 0)
			{
				if (!flag)
				{
					break;
				}
				b = samples[Count - 1].position;
				num = samples[Count - 1].percent;
				sampleIndex = Count - 2;
			}
			if (flag && sampleIndex == sampleIndex2)
			{
				break;
			}
			sampleIndex--;
		}
		float num3 = 0f;
		if (moved > distance)
		{
			num3 = moved - distance;
		}
		double result = DMath.Lerp(num, samples[sampleIndex].percent, 1f - num3 / num2);
		moved -= num3;
		return result;
	}

	public double TravelWithOffset(double start, float distance, Spline.Direction direction, Vector3 offset, out float moved, double clipFrom = 0.0, double clipTo = 1.0)
	{
		moved = 0f;
		if (!hasSamples)
		{
			return 0.0;
		}
		if (direction == Spline.Direction.Forward && start >= 1.0)
		{
			return clipTo;
		}
		if (direction == Spline.Direction.Backward && start <= 0.0)
		{
			return clipFrom;
		}
		double num = start;
		if (distance == 0f)
		{
			return num;
		}
		Evaluate(start, _workSample);
		Vector3 b = _workSample.position + _workSample.up * (offset.y * _workSample.size) + _workSample.right * (offset.x * _workSample.size) + _workSample.forward * (offset.z * _workSample.size);
		GetSamplingValues(num, out var sampleIndex, out var lerp);
		if (direction == Spline.Direction.Forward && lerp > 0.0)
		{
			sampleIndex++;
		}
		float num2 = 0f;
		int sampleIndex2 = 0;
		int sampleIndex3 = Count - 1;
		bool flag = clipTo < clipFrom;
		if (flag)
		{
			GetSamplingValues(clipFrom, out sampleIndex2, out lerp);
			GetSamplingValues(clipTo, out sampleIndex3, out lerp);
			if (lerp > 0.0)
			{
				sampleIndex3++;
			}
		}
		while (moved < distance)
		{
			Vector3 vector = samples[sampleIndex].position + samples[sampleIndex].up * (offset.y * samples[sampleIndex].size) + samples[sampleIndex].right * (offset.x * samples[sampleIndex].size) + samples[sampleIndex].forward * (offset.z * samples[sampleIndex].size);
			num2 = Vector3.Distance(vector, b);
			moved += num2;
			if (moved >= distance)
			{
				break;
			}
			b = vector;
			num = samples[sampleIndex].percent;
			if (direction == Spline.Direction.Forward)
			{
				if (sampleIndex == Count - 1)
				{
					if (!flag)
					{
						break;
					}
					b = samples[0].position + samples[0].up * (offset.y * samples[0].size) + samples[0].right * (offset.x * samples[0].size) + samples[0].forward * (offset.z * samples[0].size);
					num = samples[0].percent;
					sampleIndex = 1;
				}
				if (flag && sampleIndex == sampleIndex3)
				{
					break;
				}
				sampleIndex++;
				continue;
			}
			if (sampleIndex == 0)
			{
				if (!flag)
				{
					break;
				}
				int num3 = Count - 1;
				b = samples[num3].position + samples[num3].up * (offset.y * samples[num3].size) + samples[num3].right * (offset.x * samples[num3].size) + samples[num3].forward * (offset.z * samples[num3].size);
				num = samples[num3].percent;
				sampleIndex = Count - 2;
			}
			if (flag && sampleIndex == sampleIndex2)
			{
				break;
			}
			sampleIndex--;
		}
		float num4 = 0f;
		if (moved > distance)
		{
			num4 = moved - distance;
		}
		double result = DMath.Lerp(num, samples[sampleIndex].percent, 1f - num4 / num2);
		moved -= num4;
		return result;
	}

	public double Travel(double start, float distance, Spline.Direction direction = Spline.Direction.Forward)
	{
		float moved;
		return Travel(start, distance, direction, out moved);
	}

	public void Project(Vector3 position, int controlPointCount, SplineSample result, double from = 0.0, double to = 1.0)
	{
		if (!hasSamples)
		{
			return;
		}
		if (Count == 1)
		{
			if (result == null)
			{
				result = new SplineSample(samples[0]);
			}
			else
			{
				result.CopyFrom(samples[0]);
			}
			return;
		}
		Spline.FormatFromTo(ref from, ref to);
		int num = (controlPointCount - 1) * 6;
		int num2 = Count / num;
		if (num2 < 1)
		{
			num2 = 1;
		}
		float num3 = (position - samples[0].position).sqrMagnitude;
		int sampleIndex = 0;
		int sampleIndex2 = Count - 1;
		double lerp;
		if (from != 0.0)
		{
			GetSamplingValues(from, out sampleIndex, out lerp);
		}
		if (to != 1.0)
		{
			GetSamplingValues(to, out sampleIndex2, out lerp);
			if (lerp > 0.0 && sampleIndex2 < Count - 1)
			{
				sampleIndex2++;
			}
		}
		int num4 = sampleIndex;
		int num5 = sampleIndex2;
		for (int i = sampleIndex; i <= sampleIndex2; i += num2)
		{
			if (i > sampleIndex2)
			{
				i = sampleIndex2;
			}
			float sqrMagnitude = (position - samples[i].position).sqrMagnitude;
			if (sqrMagnitude < num3)
			{
				num3 = sqrMagnitude;
				num4 = Mathf.Max(i - num2, 0);
				num5 = Mathf.Min(i + num2, Count - 1);
			}
			if (i == sampleIndex2)
			{
				break;
			}
		}
		num3 = (position - samples[num4].position).sqrMagnitude;
		int num6 = num4;
		for (int j = num4 + 1; j <= num5; j++)
		{
			float sqrMagnitude2 = (position - samples[j].position).sqrMagnitude;
			if (sqrMagnitude2 < num3)
			{
				num3 = sqrMagnitude2;
				num6 = j;
			}
		}
		int num7 = num6 - 1;
		if (num7 < 0)
		{
			num7 = 0;
		}
		int num8 = num6 + 1;
		if (num8 > Count - 1)
		{
			num8 = Count - 1;
		}
		Vector3 vector = LinearAlgebraUtility.ProjectOnLine(samples[num7].position, samples[num6].position, position);
		Vector3 vector2 = LinearAlgebraUtility.ProjectOnLine(samples[num6].position, samples[num8].position, position);
		float magnitude = (samples[num6].position - samples[num7].position).magnitude;
		float magnitude2 = (samples[num6].position - samples[num8].position).magnitude;
		float magnitude3 = (vector - samples[num7].position).magnitude;
		float magnitude4 = (vector2 - samples[num8].position).magnitude;
		if (num7 < num6 && num6 < num8)
		{
			if ((position - vector).sqrMagnitude < (position - vector2).sqrMagnitude)
			{
				SplineSample.Lerp(samples[num7], samples[num6], magnitude3 / magnitude, result);
				if (sampleMode == SplineComputer.SampleMode.Uniform)
				{
					result.percent = DMath.Lerp(GetSamplePercent(num7), GetSamplePercent(num6), magnitude3 / magnitude);
				}
			}
			else
			{
				SplineSample.Lerp(samples[num8], samples[num6], magnitude4 / magnitude2, result);
				if (sampleMode == SplineComputer.SampleMode.Uniform)
				{
					result.percent = DMath.Lerp(GetSamplePercent(num8), GetSamplePercent(num6), magnitude4 / magnitude2);
				}
			}
		}
		else if (num7 < num6)
		{
			SplineSample.Lerp(samples[num7], samples[num6], magnitude3 / magnitude, result);
			if (sampleMode == SplineComputer.SampleMode.Uniform)
			{
				result.percent = DMath.Lerp(GetSamplePercent(num7), GetSamplePercent(num6), magnitude3 / magnitude);
			}
		}
		else
		{
			SplineSample.Lerp(samples[num8], samples[num6], magnitude4 / magnitude2, result);
			if (sampleMode == SplineComputer.SampleMode.Uniform)
			{
				result.percent = DMath.Lerp(GetSamplePercent(num8), GetSamplePercent(num6), magnitude4 / magnitude2);
			}
		}
		if (Count <= 1 || from != 0.0 || to != 1.0 || !(result.percent < samples[1].percent))
		{
			return;
		}
		Vector3 vector3 = LinearAlgebraUtility.ProjectOnLine(samples[Count - 1].position, samples[Count - 2].position, position);
		if ((position - vector3).sqrMagnitude < (position - result.position).sqrMagnitude)
		{
			double t = LinearAlgebraUtility.InverseLerp(samples[Count - 1].position, samples[Count - 2].position, vector3);
			SplineSample.Lerp(samples[Count - 1], samples[Count - 2], t, result);
			if (sampleMode == SplineComputer.SampleMode.Uniform)
			{
				result.percent = DMath.Lerp(GetSamplePercent(Count - 1), GetSamplePercent(Count - 2), t);
			}
		}
	}

	private double GetSamplePercent(int sampleIndex)
	{
		if (sampleMode == SplineComputer.SampleMode.Optimized)
		{
			return samples[optimizedIndices[sampleIndex]].percent;
		}
		return (double)sampleIndex / (double)(Count - 1);
	}

	public float CalculateLength(double from = 0.0, double to = 1.0)
	{
		if (!hasSamples)
		{
			return 0f;
		}
		Spline.FormatFromTo(ref from, ref to);
		float num = 0f;
		Vector3 b = EvaluatePosition(from);
		GetSamplingValues(from, out var sampleIndex, out var lerp);
		GetSamplingValues(to, out var sampleIndex2, out lerp);
		if (lerp > 0.0 && sampleIndex2 < Count - 1)
		{
			sampleIndex2++;
		}
		for (int i = sampleIndex + 1; i < sampleIndex2; i++)
		{
			num += Vector3.Distance(samples[i].position, b);
			b = samples[i].position;
		}
		return num + Vector3.Distance(EvaluatePosition(to), b);
	}

	public float CalculateLengthWithOffset(Vector3 offset, double from = 0.0, double to = 1.0)
	{
		if (!hasSamples)
		{
			return 0f;
		}
		Spline.FormatFromTo(ref from, ref to);
		float num = 0f;
		Evaluate(from, _workSample);
		Vector3 b = _workSample.position + _workSample.up * (offset.y * _workSample.size) + _workSample.right * (offset.x * _workSample.size) + _workSample.forward * (offset.z * _workSample.size);
		GetSamplingValues(from, out var sampleIndex, out var lerp);
		GetSamplingValues(to, out var sampleIndex2, out lerp);
		if (lerp > 0.0 && sampleIndex2 < Count - 1)
		{
			sampleIndex2++;
		}
		for (int i = sampleIndex + 1; i < sampleIndex2; i++)
		{
			Vector3 vector = samples[i].position + samples[i].up * (offset.y * samples[i].size) + samples[i].right * (offset.x * samples[i].size) + samples[i].forward * (offset.z * samples[i].size);
			num += Vector3.Distance(vector, b);
			b = vector;
		}
		Evaluate(to, _workSample);
		_workSample.position += _workSample.up * (offset.y * _workSample.size) + _workSample.right * (offset.x * _workSample.size) + _workSample.forward * (offset.z * _workSample.size);
		return num + Vector3.Distance(_workSample.position, b);
	}
}
