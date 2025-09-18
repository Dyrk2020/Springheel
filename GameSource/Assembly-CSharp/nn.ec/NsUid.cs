using System;

namespace nn.ec;

public struct NsUid : IEquatable<NsUid>
{
	public ulong value;

	public NsUid(ulong _value)
	{
		value = _value;
	}

	public override string ToString()
	{
		return value.ToString();
	}

	public static NsUid GetInvalidId()
	{
		return new NsUid
		{
			value = 0uL
		};
	}

	public override bool Equals(object obj)
	{
		if (!(obj is NsUid))
		{
			return false;
		}
		return Equals((NsUid)obj);
	}

	public bool Equals(NsUid other)
	{
		return this == other;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public static bool operator ==(NsUid lhs, NsUid rhs)
	{
		return lhs.value == rhs.value;
	}

	public static bool operator !=(NsUid lhs, NsUid rhs)
	{
		return !(lhs == rhs);
	}
}
