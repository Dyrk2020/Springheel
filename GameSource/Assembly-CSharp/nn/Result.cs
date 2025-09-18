using System;

namespace nn;

public struct Result : IEquatable<Result>
{
	public const int ModuleBitsOffset = 0;

	public const int ModuleBitsCount = 9;

	public const int ModuleBitsMask = 511;

	public const int DescriptionBitsOffset = 9;

	public const int DescriptionBitsCount = 13;

	public const int DescriptionBitsMask = 8191;

	public uint innerValue;

	public Result(int module, int description)
	{
		innerValue = (uint)(module | (description << 9));
	}

	public bool IsSuccess()
	{
		return innerValue == 0;
	}

	public void abortUnlessSuccess()
	{
		if (!IsSuccess())
		{
			Nn.Abort(ToString());
		}
	}

	public int GetModule()
	{
		return (int)(innerValue & 0x1FF);
	}

	public int GetDescription()
	{
		return ((int)innerValue >> 9) & 0x1FFF;
	}

	public override string ToString()
	{
		return $"0x{innerValue:X8} Module:{GetModule()} Description:{GetDescription()}";
	}

	public override bool Equals(object obj)
	{
		if (!(obj is Result))
		{
			return false;
		}
		return Equals((Result)obj);
	}

	public bool Equals(Result other)
	{
		return this == other;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public static bool operator ==(Result lhs, Result rhs)
	{
		return lhs.innerValue == rhs.innerValue;
	}

	public static bool operator !=(Result lhs, Result rhs)
	{
		return !(lhs == rhs);
	}
}
