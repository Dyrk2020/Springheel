using System;

namespace nn.util;

public struct Float4 : IEquatable<Float4>
{
	public float x;

	public float y;

	public float z;

	public float w;

	public Float4(float x, float y, float z, float w)
	{
		this.x = x;
		this.y = y;
		this.z = z;
		this.w = w;
	}

	public void Set(float x, float y, float z, float w)
	{
		this.x = x;
		this.y = y;
		this.z = z;
		this.w = w;
	}

	public override string ToString()
	{
		return $"({x} {y} {z} {w})";
	}

	public static bool operator ==(Float4 lhs, Float4 rhs)
	{
		if (lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z)
		{
			return lhs.w == rhs.w;
		}
		return false;
	}

	public static bool operator !=(Float4 lhs, Float4 rhs)
	{
		return !(lhs == rhs);
	}

	public override bool Equals(object right)
	{
		if (!(right is Float4))
		{
			return false;
		}
		return Equals((Float4)right);
	}

	public bool Equals(Float4 other)
	{
		return this == other;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
