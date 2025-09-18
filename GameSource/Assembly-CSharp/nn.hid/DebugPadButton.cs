using System;

namespace nn.hid;

[Flags]
public enum DebugPadButton
{
	A = 1,
	B = 2,
	X = 4,
	Y = 8,
	L = 0x10,
	R = 0x20,
	ZL = 0x40,
	ZR = 0x80,
	Start = 0x100,
	Select = 0x200,
	Left = 0x400,
	Up = 0x800,
	Right = 0x1000,
	Down = 0x2000
}
