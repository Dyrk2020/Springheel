using System;
using System.IO;
using System.Runtime.Remoting.Messaging;

namespace GameSparks.RT;

internal class DuplexTlsStream : Stream
{
	private delegate int ReadDelegate(byte[] buffer, int offset, int count);

	private delegate void WriteDelegate(byte[] buffer, int offset, int count);

	private Stream wrapped;

	public override bool CanRead => wrapped.CanRead;

	public override bool CanSeek => wrapped.CanSeek;

	public override bool CanWrite => wrapped.CanWrite;

	public override long Length => wrapped.Length;

	public override long Position
	{
		get
		{
			return wrapped.Position;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	internal DuplexTlsStream(Stream wrapped)
	{
		this.wrapped = wrapped;
	}

	public override void Flush()
	{
		wrapped.Flush();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		return wrapped.Read(buffer, offset, count);
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		return wrapped.Seek(offset, origin);
	}

	public override void SetLength(long value)
	{
		wrapped.SetLength(value);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		wrapped.Write(buffer, offset, count);
	}

	public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
	{
		ReadDelegate readDelegate = Read;
		return readDelegate.BeginInvoke(buffer, offset, count, callback, state);
	}

	public override int EndRead(IAsyncResult asyncResult)
	{
		AsyncResult asyncResult2 = (AsyncResult)asyncResult;
		ReadDelegate readDelegate = (ReadDelegate)asyncResult2.AsyncDelegate;
		return readDelegate.EndInvoke(asyncResult);
	}

	public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
	{
		WriteDelegate writeDelegate = Write;
		return writeDelegate.BeginInvoke(buffer, offset, count, callback, state);
	}

	public override void EndWrite(IAsyncResult asyncResult)
	{
		AsyncResult asyncResult2 = (AsyncResult)asyncResult;
		WriteDelegate writeDelegate = (WriteDelegate)asyncResult2.AsyncDelegate;
		writeDelegate.EndInvoke(asyncResult);
	}
}
