using System;

namespace nn.swkbd;

public struct ShowKeyboardArg
{
	public KeyboardConfig keyboardConfig;

	public IntPtr workBuf;

	public long workBufSize;

	public IntPtr textCheckWorkBuf;

	public long textCheckWorkBufSize;

	private IntPtr _customizeDicBuf;

	private long _customizeDicBufSize;
}
