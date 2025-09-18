using System;
using System.Runtime.InteropServices;
using nn.util;

namespace nn.irsensor;

public struct Finger : IEquatable<Finger>
{
	[MarshalAs(UnmanagedType.U1)]
	public bool isValid;

	public Float2 tip;

	public float tipDepthFactor;

	public Float2 root;

	public int protrusionIndex;

	public override string ToString()
	{
		return $"({isValid} {tip} {tipDepthFactor} {root} {protrusionIndex}";
	}

	public static bool operator ==(Finger lhs, Finger rhs)
	{
		if (lhs.isValid == rhs.isValid && lhs.tip == rhs.tip && lhs.tipDepthFactor == rhs.tipDepthFactor && lhs.root == rhs.root)
		{
			return lhs.protrusionIndex == rhs.protrusionIndex;
		}
		return false;
	}

	public static bool operator !=(Finger lhs, Finger rhs)
	{
		return !(lhs == rhs);
	}

	public override bool Equals(object right)
	{
		if (!(right is Finger))
		{
			return false;
		}
		return Equals((Finger)right);
	}

	public bool Equals(Finger other)
	{
		return this == other;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
