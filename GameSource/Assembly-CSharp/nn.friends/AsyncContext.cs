using System;

namespace nn.friends;

public sealed class AsyncContext : IDisposable
{
	internal IntPtr _context = IntPtr.Zero;

	~AsyncContext()
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

	public Result Cancel()
	{
		return default(Result);
	}

	public Result HasDone(ref bool outDone)
	{
		outDone = true;
		return default(Result);
	}

	public Result GetResult()
	{
		return default(Result);
	}
}
