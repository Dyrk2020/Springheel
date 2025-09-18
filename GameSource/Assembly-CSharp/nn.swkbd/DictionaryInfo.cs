namespace nn.swkbd;

public struct DictionaryInfo
{
	public uint offset;

	public ushort size;

	public DictionaryLang lang;

	public static bool operator ==(DictionaryInfo lhs, DictionaryInfo rhs)
	{
		if (lhs.offset == rhs.offset && lhs.size == rhs.size)
		{
			return lhs.lang == rhs.lang;
		}
		return false;
	}

	public static bool operator !=(DictionaryInfo lhs, DictionaryInfo rhs)
	{
		return !(lhs == rhs);
	}

	public override bool Equals(object right)
	{
		if (!(right is DictionaryInfo))
		{
			return false;
		}
		return Equals((DictionaryInfo)right);
	}

	public bool Equals(DictionaryInfo other)
	{
		return this == other;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
