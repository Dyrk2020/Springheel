using System;

namespace nn.irsensor;

public struct Protrusion : IEquatable<Protrusion>
{
	public int firstPointIndex;

	public int pointCount;

	public override string ToString()
	{
		return $"({firstPointIndex} {pointCount})";
	}

	public static bool operator ==(Protrusion lhs, Protrusion rhs)
	{
		if (lhs.firstPointIndex == rhs.firstPointIndex)
		{
			return lhs.pointCount == rhs.pointCount;
		}
		return false;
	}

	public static bool operator !=(Protrusion lhs, Protrusion rhs)
	{
		return !(lhs == rhs);
	}

	public override bool Equals(object right)
	{
		if (!(right is Protrusion))
		{
			return false;
		}
		return Equals((Protrusion)right);
	}

	public bool Equals(Protrusion other)
	{
		return this == other;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
