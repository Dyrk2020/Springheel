using System.Runtime.InteropServices;

namespace nn.irsensor;

public static class MomentProcessor
{
	public const int StateCountMax = 5;

	public const int BlockColumnCount = 8;

	public const int BlockRowCount = 6;

	public const int BlockCount = 48;

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_irsensor_GetMomentProcessorDefaultConfig")]
	public static extern void GetDefaultConfig(ref MomentProcessorConfig pOutValue);

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_irsensor_RunMomentProcessor")]
	public static extern void Run(IrCameraHandle handle, MomentProcessorConfig config);

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_irsensor_GetMomentProcessorState")]
	public static extern Result GetState(ref MomentProcessorState pOutValue, IrCameraHandle handle);

	public static Result GetStatus(MomentProcessorState[] pOutStates, ref int pOutCount, IrCameraHandle handle)
	{
		return GetStates(pOutStates, ref pOutCount, pOutStates.Length, handle);
	}

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_irsensor_GetMomentProcessorStates")]
	private static extern Result GetStates([In][Out] MomentProcessorState[] pOutStates, ref int pOutCount, int countMax, IrCameraHandle handle);

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_irsensor_CalculateMomentRegionStatistic")]
	public static extern MomentStatistic CalculateMomentRegionStatistic(ref MomentProcessorState pState, Rect windowOfInterest, int startRow, int startColumn, int rowCount, int columnCount);
}
