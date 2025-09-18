using System;

namespace nn.hid;

[Flags]
public enum NpadAttribute
{
	None = 0,
	IsConnected = 1,
	IsWired = 2,
	IsLeftConnected = 4,
	IsLeftWired = 8,
	IsRightConnected = 0x10,
	IsRightWired = 0x20
}
