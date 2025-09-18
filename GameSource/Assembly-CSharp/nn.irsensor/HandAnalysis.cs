using System.Runtime.InteropServices;
using nn.util;

namespace nn.irsensor;

public static class HandAnalysis
{
	public const int ProcessorStateCountMax = 5;

	public const int ShapePointCountMax = 512;

	public const int ShapeCountMax = 16;

	public const int ProtrusionCountMax = 8;

	public const int HandCountMax = 2;

	public const int ImageWidth = 40;

	public const int ImageHeight = 30;

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_irsensor_RunHandAnalysis")]
	public static extern Result Run(IrCameraHandle handle, HandAnalysisConfig config);

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_irsensor_GetHandAnalysisSilhouetteState1")]
	public static extern Result GetSilhouetteState(ref HandAnalysisSilhouetteState pOutValue, IrCameraHandle handle);

	public static Result GetSilhouetteState(HandAnalysisSilhouetteState[] pOutValueArray, ref int pReturnCount, long infSamplingNumber, IrCameraHandle handle)
	{
		return GetSilhouetteState(pOutValueArray, ref pReturnCount, pOutValueArray.Length, infSamplingNumber, handle);
	}

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_irsensor_GetHandAnalysisSilhouetteState")]
	private static extern Result GetSilhouetteState([In][Out] HandAnalysisSilhouetteState[] pOutValueArray, ref int pReturnCount, int maxCount, long infSamplingNumber, IrCameraHandle handle);

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_irsensor_GetHandAnalysisSilhouetteStateAndPoints1")]
	public static extern Result GetSilhouetteState(ref HandAnalysisSilhouetteState pOutState, [In][Out] Float2[] pOutPointBuffer, IrCameraHandle handle);

	public static Result GetSilhouetteState(HandAnalysisSilhouetteState[] pOutStateArray, Float2[][] pOutPointArray, ref int pReturnCount, long infSamplingNumber, IrCameraHandle handle)
	{
		return GetSilhouetteState(pOutStateArray, pOutPointArray, ref pReturnCount, pOutStateArray.Length, infSamplingNumber, handle);
	}

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_irsensor_GetHandAnalysisSilhouetteStateAndPoints")]
	private static extern Result GetSilhouetteState([In][Out] HandAnalysisSilhouetteState[] pOutStateArray, [In][Out] Float2[][] pOutPointArray, ref int pReturnCount, int maxCount, long infSamplingNumber, IrCameraHandle handle);

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_irsensor_GetHandAnalysisImageState1")]
	public static extern Result GetImageState(ref HandAnalysisImageState pOutState, [In][Out] ushort[] pOutImageBuffer, IrCameraHandle handle);

	public static Result GetImageState(HandAnalysisImageState[] pOutStateArray, ushort[] pOutImageArray, ref int pReturnCount, long infSamplingNumber, IrCameraHandle handle)
	{
		return GetImageState(pOutStateArray, pOutImageArray, ref pReturnCount, pOutStateArray.Length, infSamplingNumber, handle);
	}

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_irsensor_GetHandAnalysisImageState")]
	private static extern Result GetImageState([In][Out] HandAnalysisImageState[] pOutStateArray, [In][Out] ushort[] pOutImageArray, ref int pReturnCount, int maxCount, long infSamplingNumber, IrCameraHandle handle);
}
