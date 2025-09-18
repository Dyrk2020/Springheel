using System;

namespace nn.fs;

[Flags]
public enum OpenDirectoryMode
{
	Directory = 1,
	File = 2,
	All = 3
}
