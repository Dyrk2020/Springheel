using System;
using nn.account;

namespace nn.friends;

public sealed class NotificationQueue : IDisposable
{
	private IntPtr queue = IntPtr.Zero;

	~NotificationQueue()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
	}

	public Result Initialize(Uid uid)
	{
		return default(Result);
	}

	public Result Initialize()
	{
		return default(Result);
	}

	public void Terminate()
	{
	}

	public Uid GetUid()
	{
		return default(Uid);
	}

	public Result Clear()
	{
		return default(Result);
	}

	public Result Pop(ref NotificationInfo outInfo)
	{
		return default(Result);
	}

	public bool Exists()
	{
		return false;
	}
}
