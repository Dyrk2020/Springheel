using System;

namespace nn.hid;

public struct GesturePoint : IEquatable<GesturePoint>
{
	public int x;

	public int y;

	public override string ToString()
	{
		return $"({x} {y})";
	}

	public static bool operator ==(GesturePoint lhs, GesturePoint rhs)
	{
		if (lhs.x == rhs.x)
		{
			return lhs.y == rhs.y;
		}
		return false;
	}

	public static bool operator !=(GesturePoint lhs, GesturePoint rhs)
	{
		return !(lhs == rhs);
	}

	public override bool Equals(object right)
	{
		if (!(right is GesturePoint))
		{
			return false;
		}
		return Equals((GesturePoint)right);
	}

	public bool Equals(GesturePoint other)
	{
		return this == other;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
