using System;
using System.Runtime.InteropServices;
using System.Text;

namespace nn;

public static class NGC
{
	[Flags]
	public enum ProfanityFilterPatternList
	{
		ProfanityFilterPatternList_Japanese = 1,
		ProfanityFilterPatternList_AmericanEnglish = 2,
		ProfanityFilterPatternList_CanadianFrench = 4,
		ProfanityFilterPatternList_LatinAmericanSpanish = 8,
		ProfanityFilterPatternList_BritishEnglish = 0x10,
		ProfanityFilterPatternList_French = 0x20,
		ProfanityFilterPatternList_German = 0x40,
		ProfanityFilterPatternList_Italian = 0x80,
		ProfanityFilterPatternList_Spanish = 0x100,
		ProfanityFilterPatternList_Dutch = 0x200,
		ProfanityFilterPatternList_Korean = 0x400,
		ProfanityFilterPatternList_Chinese = 0x800,
		ProfanityFilterPatternList_Portuguese = 0x1000,
		ProfanityFilterPatternList_Russian = 0x2000,
		ProfanityFilterPatternList_SouthAmericanPortuguese = 0x4000,
		ProfanityFilterPatternList_Taiwanese = 0x8000
	}

	public enum MaskMode
	{
		OverWrite,
		ReplaceByOneCharacter
	}

	public enum SkipMode
	{
		SkipMode_NotSkip,
		SkipMode_SkipAtSign
	}

	public static readonly ErrorRange ResultNotInitialized = new ErrorRange(146, 1, 2);

	public static readonly ErrorRange ResultAlreadyInitialized = new ErrorRange(146, 2, 3);

	public static readonly ErrorRange ResultInvalidPointer = new ErrorRange(146, 3, 4);

	public static readonly ErrorRange ResultInvalidSize = new ErrorRange(146, 4, 5);

	public const int ProfanityFilterPatternList_Max = 16;

	public const int WordLengthMax = 64;

	public const int WordCountMax = 16;

	public const int TextLengthMax = 512;

	[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_ngc_ceg_Initialize")]
	public static extern Result Initialize(bool checkDesiredLanguage);

	[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_ngc_ceg_CountNumbers")]
	public static extern int CountNumbers(string text);

	[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_ngc_ceg_GetContentVersion")]
	public static extern uint GetContentVersion();

	[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_ngc_ceg_CheckProfanityWords")]
	private static extern Result CheckProfanityWords(uint[] checkResults, ProfanityFilterPatternList patterns, byte[] words, int wordCount);

	public static Result CheckProfanityWords(uint[] checkResults, ProfanityFilterPatternList patterns, string[] words, int wordCount)
	{
		if (words.Length > 16)
		{
			return new Result(ResultInvalidSize.Module, ResultInvalidSize.DescriptionBegin);
		}
		byte[] array = new byte[words.Length * 64];
		for (int i = 0; i != words.Length; i++)
		{
			string text = words[i];
			if (text.Length >= 63)
			{
				return new Result(ResultInvalidSize.Module, ResultInvalidSize.DescriptionBegin);
			}
			byte[] bytes = Encoding.UTF8.GetBytes(text);
			for (int j = 0; j != bytes.Length; j++)
			{
				array[i * 64 + j] = bytes[j];
			}
		}
		return CheckProfanityWords(checkResults, patterns, array, wordCount);
	}

	[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_ngc_ceg_MaskProfanityWordsInText")]
	private static extern Result MaskProfanityWordsInText(ref int profanityWordCount, byte[] text, ProfanityFilterPatternList patterns);

	public static Result MaskProfanityWordsInText(ref int profanityWordCount, ref string text, ProfanityFilterPatternList patterns)
	{
		if (text.Length > 511)
		{
			return new Result(ResultInvalidSize.Module, ResultInvalidSize.DescriptionBegin);
		}
		byte[] bytes = Encoding.UTF8.GetBytes(text);
		byte[] array = new byte[bytes.Length + 1];
		Array.Copy(bytes, array, bytes.Length);
		Result result = MaskProfanityWordsInText(ref profanityWordCount, array, patterns);
		int num = 0;
		for (int i = 0; i != array.Length && array[i] > 0; i++)
		{
			num++;
		}
		byte[] array2 = new byte[num];
		Array.Copy(array, array2, num);
		text = Encoding.UTF8.GetString(array2);
		return result;
	}

	[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_ngc_ceg_SetMaskMode")]
	public static extern void SetMaskMode(MaskMode mode);

	[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_ngc_ceg_SkipAtSignCheck")]
	public static extern void SkipAtSignCheck(SkipMode skipMode);

	[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
	public static extern void CleanupAndShutdown();
}
