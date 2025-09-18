using System;
using System.Runtime.InteropServices;
using nn.util;

namespace nn.irsensor;

public struct Arm : IEquatable<Arm>
{
	[MarshalAs(UnmanagedType.U1)]
	public bool isValid;

	public Float2 wristPosition;

	public Float2 armDirection;

	public int protrusionIndex;

	public override string ToString()
	{
		return $"({isValid} {wristPosition} {armDirection} {protrusionIndex})";
	}

	public static bool operator ==(Arm lhs, Arm rhs)
	{
		if (lhs.isValid == rhs.isValid && lhs.wristPosition == rhs.wristPosition && lhs.armDirection == rhs.armDirection)
		{
			return lhs.protrusionIndex == rhs.protrusionIndex;
		}
		return false;
	}

	public static bool operator !=(Arm lhs, Arm rhs)
	{
		return !(lhs == rhs);
	}

	public override bool Equals(object right)
	{
		if (!(right is Arm))
		{
			return false;
		}
		return Equals((Arm)right);
	}

	public bool Equals(Arm other)
	{
		return this == other;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
