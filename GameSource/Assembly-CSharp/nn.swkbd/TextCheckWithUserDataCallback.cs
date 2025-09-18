using System;
using System.Runtime.InteropServices;

namespace nn.swkbd;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate TextCheckResult TextCheckWithUserDataCallback(IntPtr pOutDialogTextBuf, ref long pOutDialogTextLengthSize, ref String pStr, IntPtr pUserData);
