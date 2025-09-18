using System;
using nn.util;

namespace nn.irsensor;

public struct Palm : IEquatable<Palm>
{
	public Float2 center;

	public float area;

	public float depthFactor;

	public override string ToString()
	{
		return $"({center} {area} {depthFactor})";
	}

	public static bool operator ==(Palm lhs, Palm rhs)
	{
		if (lhs.center == rhs.center && lhs.area == rhs.area)
		{
			return lhs.depthFactor == rhs.depthFactor;
		}
		return false;
	}

	public static bool operator !=(Palm lhs, Palm rhs)
	{
		return !(lhs == rhs);
	}

	public override bool Equals(object right)
	{
		if (!(right is Palm))
		{
			return false;
		}
		return Equals((Palm)right);
	}

	public bool Equals(Palm other)
	{
		return this == other;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
