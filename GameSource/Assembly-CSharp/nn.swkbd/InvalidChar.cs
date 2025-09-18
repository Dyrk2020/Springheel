using System;

namespace nn.swkbd;

[Flags]
public enum InvalidChar
{
	Space = 2,
	AtMark = 4,
	Percent = 8,
	Slash = 0x10,
	BackSlash = 0x20,
	Numeric = 0x40,
	OutsideOfDownloadCode = 0x80,
	OutsideOfMiiNickName = 0x100,
	Force32 = -1
}
