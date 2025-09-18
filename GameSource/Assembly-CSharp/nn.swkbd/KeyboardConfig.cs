using System;
using System.Runtime.InteropServices;

namespace nn.swkbd;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct KeyboardConfig
{
	public KeyboardMode keyboardMode;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
	public string okText;

	[MarshalAs(UnmanagedType.U2)]
	public char leftOptionalSymbolKey;

	[MarshalAs(UnmanagedType.U2)]
	public char rightOptionalSymbolKey;

	[MarshalAs(UnmanagedType.U1)]
	public bool isPredictionEnabled;

	public InvalidChar invalidCharFlag;

	public InitialCursorPos initialCursorPos;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 65)]
	public string headerText;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 129)]
	public string subText;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 257)]
	public string guideText;

	public int textMaxLength;

	public int textMinLength;

	public PasswordMode passwordMode;

	public InputFormMode inputFormMode;

	[MarshalAs(UnmanagedType.U1)]
	public bool isUseNewLine;

	[MarshalAs(UnmanagedType.U1)]
	public bool isUseUtf8;

	[MarshalAs(UnmanagedType.U1)]
	public bool isUseBlurBackground;

	private int _initialStringOffset;

	private int _initialStringLength;

	private int _userDictionaryOffset;

	private int _userDictionaryNum;

	[MarshalAs(UnmanagedType.U1)]
	private bool _isUseTextCheck;

	private IntPtr _textCheckCallback;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
	public int[] separateTextPos;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
	private DictionaryInfo[] _customizedDicInfoList;

	private byte _customizedDicCount;

	[MarshalAs(UnmanagedType.U1)]
	public bool isCancelButtonDisabled;

	[MarshalAs(UnmanagedType.U1)]
	public bool isGb180302022Lv1Enabled;

	private IntPtr _textCheckWithUserDataCallback;

	private IntPtr _textCheckCallbackUserData;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
	private byte[] _reserved;
}
