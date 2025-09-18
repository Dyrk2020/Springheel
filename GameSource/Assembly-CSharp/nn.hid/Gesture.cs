using System.Runtime.InteropServices;

namespace nn.hid;

public static class Gesture
{
	public const int PointCountMax = 4;

	public const int StateCountMax = 16;

	public const int OutputWidthDefault = 1280;

	public const int OutputHeightDefault = 720;

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_hid_InitializeGesture")]
	public static extern void Initialize();

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_hid_InitializeGestureWithResolution")]
	public static extern void Initialize(int outputWidth, int outputHeight);

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_hid_GetGestureStates")]
	public static extern int GetStates([In][Out] GestureState[] pOutValues, int count);
}
