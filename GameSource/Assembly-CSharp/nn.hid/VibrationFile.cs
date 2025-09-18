using System.Runtime.InteropServices;

namespace nn.hid;

public static class VibrationFile
{
	public static ErrorRange ResultInvalid => new ErrorRange(202, 140, 150);

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_hid_ParseVibrationFile")]
	public static extern Result Parse(ref VibrationFileInfo pOutInfo, ref VibrationFileParserContext pOutContext, byte[] address, long fileSize);

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_hid_RetrieveVibrationValue")]
	public static extern void RetrieveValue(ref VibrationValue pOutValue, int position, ref VibrationFileParserContext pContext);

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_hid_GenerateVibrationFile")]
	private static extern void Generate(ref long pOutSize, byte[] outBuffer, long bufferSize, VibrationValueArrayInfo info, VibrationValue[] pValues);

	public static void Generate(ref long pOutSize, byte[] outBuffer, VibrationValueArrayInfo info, VibrationValue[] pValues)
	{
		Generate(ref pOutSize, outBuffer, outBuffer.LongLength, info, pValues);
	}

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_hid_CalculateVibrationFileSize")]
	public static extern long CalculateSize(VibrationValueArrayInfo info);
}
