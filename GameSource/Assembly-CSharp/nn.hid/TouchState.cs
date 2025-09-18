using System;

namespace nn.hid;

public struct TouchState : IEquatable<TouchState>
{
	public long deltaTimeNanoSeconds;

	public TouchAttribute attributes;

	public int fingerId;

	public int x;

	public int y;

	public int diameterX;

	public int diameterY;

	public int rotationAngle;

	private int _reserved;

	public override string ToString()
	{
		return $"fId:{fingerId} pos:({x} {y}) dia:({diameterX} {diameterY}) rotA:{rotationAngle} attr:{attributes} delta:{deltaTimeNanoSeconds}";
	}

	public static bool operator ==(TouchState lhs, TouchState rhs)
	{
		if (lhs.deltaTimeNanoSeconds == rhs.deltaTimeNanoSeconds && lhs.attributes == rhs.attributes && lhs.fingerId == rhs.fingerId && lhs.x == rhs.x && lhs.y == rhs.y && lhs.diameterX == rhs.diameterX && lhs.diameterY == rhs.diameterY)
		{
			return lhs.rotationAngle == rhs.rotationAngle;
		}
		return false;
	}

	public static bool operator !=(TouchState lhs, TouchState rhs)
	{
		return !(lhs == rhs);
	}

	public override bool Equals(object right)
	{
		if (!(right is TouchState))
		{
			return false;
		}
		return Equals((TouchState)right);
	}

	public bool Equals(TouchState other)
	{
		return this == other;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
