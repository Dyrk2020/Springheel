using System;

namespace nn.util;

public struct Float2 : IEquatable<Float2>
{
	public float x;

	public float y;

	public Float2(float x, float y)
	{
		this.x = x;
		this.y = y;
	}

	public void Set(float x, float y)
	{
		this.x = x;
		this.y = y;
	}

	public override string ToString()
	{
		return $"({x} {y})";
	}

	public static bool operator ==(Float2 lhs, Float2 rhs)
	{
		if (lhs.x == rhs.x)
		{
			return lhs.y == rhs.y;
		}
		return false;
	}

	public static bool operator !=(Float2 lhs, Float2 rhs)
	{
		return !(lhs == rhs);
	}

	public override bool Equals(object right)
	{
		if (!(right is Float2))
		{
			return false;
		}
		return Equals((Float2)right);
	}

	public bool Equals(Float2 other)
	{
		return this == other;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
