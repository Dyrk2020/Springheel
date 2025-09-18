using System.Runtime.InteropServices;

namespace nn;

public static class Nifm
{
	public static readonly ErrorRange ResultErrorHandlingCompleted = new ErrorRange(110, 190, 191);

	[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_nifm_Initialize")]
	public static extern Result Initialize();

	[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_nifm_SubmitNetworkRequest")]
	public static extern void SubmitNetworkRequest();

	[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_nifm_SubmitNetworkRequestAndWait")]
	public static extern void SubmitNetworkRequestAndWait();

	[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_nifm_CancelNetworkRequest")]
	public static extern void CancelNetworkRequest();

	[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_nifm_IsNetworkRequestOnHold")]
	public static extern bool IsNetworkRequestOnHold();

	[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_nifm_IsNetworkAvailable")]
	public static extern bool IsNetworkAvailable();

	[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_nifm_SetLocalNetworkMode")]
	public static extern void SetLocalNetworkMode(bool isLocalNetworkMode);

	[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_nifm_HandleNetworkRequestErrorResult")]
	public static extern bool HandleNetworkRequestErrorResult();
}
