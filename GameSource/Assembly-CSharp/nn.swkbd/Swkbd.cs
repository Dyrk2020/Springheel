using System;
using System.Text;

namespace nn.swkbd;

public static class Swkbd
{
	public const int TextMaxLength = 500;

	public const int SeparateModeTextMaxLength = 24;

	public const int HeaderTextMaxLength = 64;

	public const int SubTextMaxLength = 128;

	public const int GuideTextMaxLength = 256;

	public const int OkTextMaxLength = 8;

	public const int UnfixedStringLengthMax = 24;

	public const int UserWordMax = 5000;

	public const int DialogTextMaxLength = 500;

	public const int SepareteTextPosMax = 8;

	public const int CustomizedDicionarySetMax = 24;

	public static ErrorRange ResultCanceled => new ErrorRange(159, 1, 2);

	public static Result ShowKeyboard(StringBuilder outResultString, ShowKeyboardArg showKeyboardArg)
	{
		return default(Result);
	}

	public static Result ShowKeyboard(StringBuilder outResultString, ShowKeyboardArg showKeyboardArg, bool suspendUnityThreads)
	{
		return default(Result);
	}

	public static Result ShowKeyboard(byte[] outResultString, ShowKeyboardArg showKeyboardArg)
	{
		return default(Result);
	}

	public static Result ShowKeyboard(byte[] outResultString, ShowKeyboardArg showKeyboardArg, bool suspendUnityThreads)
	{
		return default(Result);
	}

	public static void InitializeKeyboardConfig(ref KeyboardConfig pOutKeyboardConfig)
	{
	}

	public static void MakePreset(ref KeyboardConfig pOutKeyboardConfig, Preset preset)
	{
	}

	public static long GetRequiredStringBufferSize()
	{
		return 0L;
	}

	public static void SetLeftOptionalSymbolKey(ref KeyboardConfig pOutKeyboardConfig, char code)
	{
	}

	public static void SetLeftOptionalSymbolKeyUtf8(ref KeyboardConfig pOutKeyboardConfig, byte[] code)
	{
	}

	public static void SetRightOptionalSymbolKey(ref KeyboardConfig pOutKeyboardConfig, char code)
	{
	}

	public static void SetRightOptionalSymbolKeyUtf8(ref KeyboardConfig pOutKeyboardConfig, byte[] code)
	{
	}

	public static void SetOkText(ref KeyboardConfig pOutKeyboardConfig, string pStr)
	{
	}

	public static void SetOkTextUtf8(ref KeyboardConfig pOutKeyboardConfig, byte[] pStr)
	{
	}

	public static void SetHeaderText(ref KeyboardConfig pOutKeyboardConfig, string pStr)
	{
	}

	public static void SetHeaderTextUtf8(ref KeyboardConfig pOutKeyboardConfig, byte[] pStr)
	{
	}

	public static void SetSubText(ref KeyboardConfig pOutKeyboardConfig, string pStr)
	{
	}

	public static void SetSubTextUtf8(ref KeyboardConfig pOutKeyboardConfig, byte[] pStr)
	{
	}

	public static void SetGuideText(ref KeyboardConfig pOutKeyboardConfig, string pStr)
	{
	}

	public static void SetGuideTextUtf8(ref KeyboardConfig pOutKeyboardConfig, byte[] pStr)
	{
	}

	public static void SetInitialText(ref ShowKeyboardArg pOutShowKeyboardArg, string pStr)
	{
	}

	public static void SetInitialTextUtf8(ref ShowKeyboardArg pOutShowKeyboardArg, byte[] pStr)
	{
	}

	public static void SetUserWordList(ref ShowKeyboardArg pOutShowKeyboardArg, UserWord[] pUserWord, int userWordNum)
	{
	}

	[Obsolete("This method is obsolete. Call SetTextCheckCallback(ref ShowKeyboardArg, TextCheckWithUserDataCallback, IntPtr) instead.", false)]
	public static void SetTextCheckCallback(ref ShowKeyboardArg pOutShowKeyboardArg, TextCheckCallback pCallback)
	{
	}

	public static void SetTextCheckCallback(ref ShowKeyboardArg pOutShowKeyboardArg, TextCheckWithUserDataCallback pCallback, IntPtr pUserData)
	{
	}

	public static void SetCustomizedDictionaries(ref ShowKeyboardArg pOutShowKeyboardArg, CustomizedDictionarySet dicSet)
	{
	}

	public static void Initialize(ref ShowKeyboardArg pOutShowKeyboardArg)
	{
	}

	public static void Initialize(ref ShowKeyboardArg pOutShowKeyboardArg, bool useDirectory)
	{
	}

	public static void Initialize(ref ShowKeyboardArg pOutShowKeyboardArg, bool useDirectory, bool useTextCheck)
	{
	}

	public static void Initialize(ref ShowKeyboardArg pOutShowKeyboardArg, int userWordNum)
	{
	}

	public static void Initialize(ref ShowKeyboardArg pOutShowKeyboardArg, int userWordNum, bool useTextCheck)
	{
	}

	public static void Destroy(ref ShowKeyboardArg pOutShowKeyboardArg)
	{
	}
}
