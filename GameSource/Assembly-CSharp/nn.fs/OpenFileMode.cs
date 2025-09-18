using System;

namespace nn.fs;

[Flags]
public enum OpenFileMode
{
	Read = 1,
	Write = 2,
	AllowAppend = 4
}
