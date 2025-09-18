using System;
using System.Runtime.InteropServices;

namespace nn.swkbd;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate TextCheckResult TextCheckCallback(IntPtr pOutDialogTextBuf, ref long pOutDialogTextLengthSize, ref String pStr);
