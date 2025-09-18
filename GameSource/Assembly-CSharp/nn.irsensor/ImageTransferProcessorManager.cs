using System;
using System.Runtime.InteropServices;

namespace nn.irsensor;

public class ImageTransferProcessorManager
{
	private ImageTransferProcessorState state;

	private IntPtr pWorkBuffer = IntPtr.Zero;

	private long workBufferSize;

	private ImageTransferProcessorExConfig config;

	private IrCameraHandle handle;

	public ImageTransferProcessorState State => state;

	public byte[] ImageBuffer { get; private set; }

	~ImageTransferProcessorManager()
	{
		_Destroy();
	}

	public void Initialize(IrCameraHandle handle, ImageTransferProcessorFormat format)
	{
		ImageTransferProcessorExConfig pOutValue = default(ImageTransferProcessorExConfig);
		ImageTransferProcessor.GetDefaultConfig(ref pOutValue);
		Initialize(handle, pOutValue);
	}

	public void Initialize(IrCameraHandle handle, ImageTransferProcessorConfig config)
	{
		Initialize(handle, new ImageTransferProcessorExConfig
		{
			origFormat = config.format,
			trimmingFormat = config.format,
			irCameraConfig = config.irCameraConfig
		});
	}

	public void Initialize(IrCameraHandle handle, ImageTransferProcessorExConfig config)
	{
		this.handle = handle;
		if (pWorkBuffer != IntPtr.Zero)
		{
			_Destroy();
		}
		this.config = config;
		ImageTransferProcessor.InitializeWorkBuffer(ref pWorkBuffer, ref workBufferSize, config);
		ImageBuffer = new byte[ImageTransferProcessor.GetImageSize(config.trimmingFormat)];
	}

	public void Destroy()
	{
		_Destroy();
		GC.SuppressFinalize(this);
	}

	public bool IsRunning()
	{
		return ImageProcessor.GetStatus(handle) == ImageProcessorStatus.Running;
	}

	public void Run()
	{
		ImageTransferProcessor.Run(handle, config, pWorkBuffer, workBufferSize);
	}

	public Result Update()
	{
		long size = ImageTransferProcessor.GetImageSize(config.trimmingFormat);
		GCHandle gCHandle = GCHandle.Alloc(ImageBuffer, GCHandleType.Pinned);
		Result result = ImageTransferProcessor.GetState(ref state, gCHandle.AddrOfPinnedObject(), size, handle);
		gCHandle.Free();
		return result;
	}

	public void Stop()
	{
		if (ImageProcessor.GetStatus(handle) == ImageProcessorStatus.Running)
		{
			ImageProcessor.Stop(handle);
		}
	}

	private void _Destroy()
	{
		Stop();
		ImageTransferProcessor.DestroyWorkBuffer(pWorkBuffer);
		pWorkBuffer = IntPtr.Zero;
		workBufferSize = 0L;
	}
}
