using System;

namespace nn.account;

public struct Uid : IEquatable<Uid>
{
	public ulong _data0;

	public ulong _data1;

	public static Uid Invalid => default(Uid);

	public bool IsValid()
	{
		if (_data0 == 0L)
		{
			return _data1 != 0;
		}
		return true;
	}

	public override string ToString()
	{
		return $"{_data0:X16}{_data1:X16}";
	}

	public override bool Equals(object obj)
	{
		if (!(obj is Uid))
		{
			return false;
		}
		return Equals((Uid)obj);
	}

	public bool Equals(Uid other)
	{
		return this == other;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public static bool operator ==(Uid lhs, Uid rhs)
	{
		if (lhs._data0 == rhs._data0)
		{
			return lhs._data1 == rhs._data1;
		}
		return false;
	}

	public static bool operator !=(Uid lhs, Uid rhs)
	{
		return !(lhs == rhs);
	}
}
