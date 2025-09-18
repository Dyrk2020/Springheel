using System;
using nn.util;

namespace nn.irsensor;

public struct ClusteringData : IEquatable<ClusteringData>
{
	public float averageIntensity;

	public Float2 centroid;

	public int pixelCount;

	public Rect bound;

	public override string ToString()
	{
		return $"({averageIntensity} {centroid.ToString()} {pixelCount} {bound.ToString()})";
	}

	public static bool operator ==(ClusteringData lhs, ClusteringData rhs)
	{
		if (lhs.averageIntensity == rhs.averageIntensity && lhs.centroid == rhs.centroid && lhs.pixelCount == rhs.pixelCount)
		{
			return lhs.bound == rhs.bound;
		}
		return false;
	}

	public static bool operator !=(ClusteringData lhs, ClusteringData rhs)
	{
		return !(lhs == rhs);
	}

	public override bool Equals(object right)
	{
		if (!(right is ClusteringData))
		{
			return false;
		}
		return Equals((ClusteringData)right);
	}

	public bool Equals(ClusteringData other)
	{
		return this == other;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
