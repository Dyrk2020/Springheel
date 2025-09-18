using System;
using nn.util;

namespace nn.irsensor;

public struct Shape : IEquatable<Shape>
{
	public int firstPointIndex;

	public int pointCount;

	public float intensityAverage;

	public Float2 intensityCentroid;

	public override string ToString()
	{
		return $"({firstPointIndex} {pointCount} {intensityAverage} {intensityCentroid})";
	}

	public static bool operator ==(Shape lhs, Shape rhs)
	{
		if (lhs.firstPointIndex == rhs.firstPointIndex && lhs.pointCount == rhs.pointCount && lhs.intensityAverage == rhs.intensityAverage)
		{
			return lhs.intensityCentroid == rhs.intensityCentroid;
		}
		return false;
	}

	public static bool operator !=(Shape lhs, Shape rhs)
	{
		return !(lhs == rhs);
	}

	public override bool Equals(object right)
	{
		if (!(right is Shape))
		{
			return false;
		}
		return Equals((Shape)right);
	}

	public bool Equals(Shape other)
	{
		return this == other;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
