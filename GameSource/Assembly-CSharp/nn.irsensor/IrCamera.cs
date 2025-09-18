using System.Runtime.InteropServices;
using nn.hid;

namespace nn.irsensor;

public static class IrCamera
{
	public const int IntensityMax = 255;

	public const int ImageWidth = 320;

	public const int ImageHeight = 240;

	public const int GainMin = 1;

	public const int GainMax = 16;

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_irsensor_GetIrCameraHandle")]
	public static extern IrCameraHandle GetHandle(NpadId npadId);

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_irsensor_Initialize")]
	public static extern void Initialize(IrCameraHandle handle);

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_irsensor_Finalize")]
	public static extern void Finalize(IrCameraHandle handle);

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_irsensor_GetIrCameraStatus")]
	public static extern IrCameraStatus GetStatus(IrCameraHandle handle);

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_irsensor_CheckFirmwareUpdateNecessity")]
	public static extern Result CheckFirmwareUpdateNecessity([MarshalAs(UnmanagedType.U1)] ref bool pOutIsUpdateNeeded, IrCameraHandle handle);
}
