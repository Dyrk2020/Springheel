using System.Runtime.InteropServices;

namespace nn.ec;

public struct CourseId
{
	public const int MaxStringLength = 16;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 17)]
	public string value;

	public CourseId(string _value)
	{
		value = _value;
	}

	public override string ToString()
	{
		return value;
	}
}
