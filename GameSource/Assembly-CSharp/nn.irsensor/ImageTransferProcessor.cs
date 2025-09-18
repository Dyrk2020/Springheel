using System;
using System.Runtime.InteropServices;

namespace nn.irsensor;

public static class ImageTransferProcessor
{
	public const int QvgaImageSize = 76800;

	public const int QqvgaImageSize = 19200;

	public const int QqqvgaImageSize = 4800;

	public const int ImageSize320x240 = 76800;

	public const int ImageSize160x120 = 19200;

	public const int ImageSize80x60 = 4800;

	public const int ImageSize40x30 = 1200;

	public const int ImageSize20x15 = 300;

	public const int QvgaWorkBufferSize = 155648;

	public const int QqvgaWorkBufferSize = 40960;

	public const int QqqvgaWorkBufferSize = 12288;

	public const int WorkBufferSize320x240 = 155648;

	public const int WorkBufferSize160x120 = 40960;

	public const int WorkBufferSize80x60 = 12288;

	public const int WorkBufferSize40x30 = 4096;

	public const int WorkBufferSize20x15 = 4096;

	public const long ExposureTimeMinNanoSeconds = 7000L;

	public const long ExposureTimeMaxNanoSeconds = 600000L;

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_irsensor_GetImageTransferProcessorDefaultConfig")]
	public static extern void GetDefaultConfig(ref ImageTransferProcessorConfig pOutValue);

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_irsensor_GetImageTransferProcessorDefaultConfigEx")]
	public static extern void GetDefaultConfig(ref ImageTransferProcessorExConfig pOutValue);

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_irsensor_RunImageTransferProcessor")]
	public static extern void Run(IrCameraHandle handle, ImageTransferProcessorConfig config, IntPtr workBuffer, long workBufferSize);

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_irsensor_RunImageTransferProcessorEx")]
	public static extern void Run(IrCameraHandle handle, ImageTransferProcessorExConfig config, IntPtr workBuffer, long workBufferSize);

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_irsensor_GetImageTransferProcessorState")]
	public static extern Result GetState(ref ImageTransferProcessorState pOutState, IntPtr pOutImage, long size, IrCameraHandle handle);

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_irsensor_InitializeImageTransferWorkBuffer")]
	public static extern void InitializeWorkBuffer(ref IntPtr pOutWorkBuffer, ref long pOutWorkBufferSize, ImageTransferProcessorConfig config);

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_irsensor_InitializeImageTransferWorkBufferEx")]
	public static extern void InitializeWorkBuffer(ref IntPtr pOutWorkBuffer, ref long pOutWorkBufferSize, ImageTransferProcessorExConfig config);

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_irsensor_DestroyImageTransferWorkBuffer")]
	public static extern void DestroyWorkBuffer(IntPtr workBuffer);

	public static int GetWorkBufferSize(ImageTransferProcessorFormat format)
	{
		return format switch
		{
			ImageTransferProcessorFormat.Format320x240 => 155648, 
			ImageTransferProcessorFormat.Format160x120 => 40960, 
			ImageTransferProcessorFormat.Format80x60 => 12288, 
			ImageTransferProcessorFormat.Format40x30 => 4096, 
			ImageTransferProcessorFormat.Format20x15 => 4096, 
			_ => 155648, 
		};
	}

	public static int GetImageSize(ImageTransferProcessorFormat format)
	{
		return format switch
		{
			ImageTransferProcessorFormat.Format320x240 => 76800, 
			ImageTransferProcessorFormat.Format160x120 => 19200, 
			ImageTransferProcessorFormat.Format80x60 => 4800, 
			ImageTransferProcessorFormat.Format40x30 => 1200, 
			ImageTransferProcessorFormat.Format20x15 => 300, 
			_ => 76800, 
		};
	}

	public static int GetImageWidth(ImageTransferProcessorFormat format)
	{
		return format switch
		{
			ImageTransferProcessorFormat.Format320x240 => 320, 
			ImageTransferProcessorFormat.Format160x120 => 160, 
			ImageTransferProcessorFormat.Format80x60 => 80, 
			ImageTransferProcessorFormat.Format40x30 => 40, 
			ImageTransferProcessorFormat.Format20x15 => 20, 
			_ => 320, 
		};
	}

	public static int GetImageHeight(ImageTransferProcessorFormat format)
	{
		return format switch
		{
			ImageTransferProcessorFormat.Format320x240 => 240, 
			ImageTransferProcessorFormat.Format160x120 => 120, 
			ImageTransferProcessorFormat.Format80x60 => 60, 
			ImageTransferProcessorFormat.Format40x30 => 30, 
			ImageTransferProcessorFormat.Format20x15 => 15, 
			_ => 240, 
		};
	}
}
