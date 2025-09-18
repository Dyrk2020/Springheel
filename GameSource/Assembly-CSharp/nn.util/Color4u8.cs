using System;

namespace nn.util;

public struct Color4u8 : IEquatable<Color4u8>
{
	public byte r;

	public byte g;

	public byte b;

	public byte a;

	public void Set(byte r, byte g, byte b, byte a)
	{
		this.r = r;
		this.g = g;
		this.b = b;
		this.a = a;
	}

	public override string ToString()
	{
		return $"({r} {g} {b} {a})";
	}

	public static bool operator ==(Color4u8 lhs, Color4u8 rhs)
	{
		if (lhs.r == rhs.r && lhs.g == rhs.g && lhs.b == rhs.b)
		{
			return lhs.a == rhs.a;
		}
		return false;
	}

	public static bool operator !=(Color4u8 lhs, Color4u8 rhs)
	{
		return !(lhs == rhs);
	}

	public override bool Equals(object right)
	{
		if (!(right is Color4u8))
		{
			return false;
		}
		return Equals((Color4u8)right);
	}

	public bool Equals(Color4u8 other)
	{
		return this == other;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
