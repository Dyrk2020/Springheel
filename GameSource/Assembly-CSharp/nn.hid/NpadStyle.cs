using System;

namespace nn.hid;

[Flags]
public enum NpadStyle
{
	None = 0,
	FullKey = 1,
	Handheld = 2,
	JoyDual = 4,
	JoyLeft = 8,
	JoyRight = 0x10,
	Invalid = 0x20
}
