using System.Runtime.InteropServices;

namespace nn.swkbd;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct UserWord
{
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 25)]
	public string reading;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 25)]
	public string word;
}
