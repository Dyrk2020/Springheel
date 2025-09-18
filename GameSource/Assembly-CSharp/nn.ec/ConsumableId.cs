using System.Runtime.InteropServices;

namespace nn.ec;

public struct ConsumableId
{
	public const int MaxStringLength = 16;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 17)]
	public string value;

	public ConsumableId(string _value)
	{
		value = _value;
	}

	public override string ToString()
	{
		return value;
	}
}
