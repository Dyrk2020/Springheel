using System;
using nn.util;

namespace nn.irsensor;

public struct MomentStatistic : IEquatable<MomentStatistic>
{
	public float averageIntensity;

	public Float2 centroid;

	public override string ToString()
	{
		return $"({averageIntensity} {centroid}";
	}

	public static bool operator ==(MomentStatistic lhs, MomentStatistic rhs)
	{
		if (lhs.averageIntensity == rhs.averageIntensity)
		{
			return lhs.centroid == rhs.centroid;
		}
		return false;
	}

	public static bool operator !=(MomentStatistic lhs, MomentStatistic rhs)
	{
		return !(lhs == rhs);
	}

	public override bool Equals(object right)
	{
		if (!(right is MomentStatistic))
		{
			return false;
		}
		return Equals((MomentStatistic)right);
	}

	public bool Equals(MomentStatistic other)
	{
		return this == other;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
