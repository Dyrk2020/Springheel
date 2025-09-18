using System;

namespace nn.util;

public struct Float3 : IEquatable<Float3>
{
	public float x;

	public float y;

	public float z;

	public Float3(float x, float y, float z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public void Set(float x, float y, float z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public override string ToString()
	{
		return $"({x} {y} {z})";
	}

	public static bool operator ==(Float3 lhs, Float3 rhs)
	{
		if (lhs.x == rhs.x && lhs.y == rhs.y)
		{
			return lhs.z == rhs.z;
		}
		return false;
	}

	public static bool operator !=(Float3 lhs, Float3 rhs)
	{
		return !(lhs == rhs);
	}

	public override bool Equals(object right)
	{
		if (!(right is Float3))
		{
			return false;
		}
		return Equals((Float3)right);
	}

	public bool Equals(Float3 other)
	{
		return this == other;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
