using System;

namespace nn.irsensor;

public struct Rect : IEquatable<Rect>
{
	public short x;

	public short y;

	public short width;

	public short height;

	public Rect(short x, short y, short width, short height)
	{
		this.x = x;
		this.y = y;
		this.width = width;
		this.height = height;
	}

	public override string ToString()
	{
		return $"(x:{x} y:{y} w:{width} h:{height})";
	}

	public static bool operator ==(Rect lhs, Rect rhs)
	{
		if (lhs.x == rhs.x && lhs.y == rhs.y && lhs.width == rhs.width)
		{
			return lhs.height == rhs.height;
		}
		return false;
	}

	public static bool operator !=(Rect lhs, Rect rhs)
	{
		return !(lhs == rhs);
	}

	public override bool Equals(object right)
	{
		if (!(right is Rect))
		{
			return false;
		}
		return Equals((Rect)right);
	}

	public bool Equals(Rect other)
	{
		return this == other;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
