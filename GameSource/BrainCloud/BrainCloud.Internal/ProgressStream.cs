using System;
using System.IO;

namespace BrainCloud.Internal;

internal class ProgressStream : Stream
{
	private Stream innerStream;

	public override bool CanRead => innerStream.CanRead;

	public override bool CanSeek => innerStream.CanSeek;

	public override bool CanWrite => innerStream.CanWrite;

	public override long Length => innerStream.Length;

	public override long Position
	{
		get
		{
			return innerStream.Position;
		}
		set
		{
			innerStream.Position = value;
		}
	}

	public event ProgressStreamReportDelegate BytesRead;

	public event ProgressStreamReportDelegate BytesWritten;

	public event ProgressStreamReportDelegate BytesMoved;

	public ProgressStream(Stream streamToReportOn)
	{
		if (streamToReportOn != null)
		{
			innerStream = streamToReportOn;
			return;
		}
		throw new ArgumentNullException("streamToReportOn");
	}

	protected virtual void OnBytesRead(int bytesMoved)
	{
		if (this.BytesRead != null)
		{
			ProgressStreamReportEventArgs args = new ProgressStreamReportEventArgs(bytesMoved, innerStream.Length, innerStream.Position, wasRead: true);
			this.BytesRead(this, args);
		}
	}

	protected virtual void OnBytesWritten(int bytesMoved)
	{
		if (this.BytesWritten != null)
		{
			ProgressStreamReportEventArgs args = new ProgressStreamReportEventArgs(bytesMoved, innerStream.Length, innerStream.Position, wasRead: false);
			this.BytesWritten(this, args);
		}
	}

	protected virtual void OnBytesMoved(int bytesMoved, bool isRead)
	{
		if (this.BytesMoved != null)
		{
			ProgressStreamReportEventArgs args = new ProgressStreamReportEventArgs(bytesMoved, innerStream.Length, innerStream.Position, isRead);
			this.BytesMoved(this, args);
		}
	}

	public override void Flush()
	{
		innerStream.Flush();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		int num = innerStream.Read(buffer, offset, count);
		OnBytesRead(num);
		OnBytesMoved(num, isRead: true);
		return num;
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		return innerStream.Seek(offset, origin);
	}

	public override void SetLength(long value)
	{
		innerStream.SetLength(value);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		innerStream.Write(buffer, offset, count);
		OnBytesWritten(count);
		OnBytesMoved(count, isRead: false);
	}

	public override void Close()
	{
		innerStream.Close();
		base.Close();
	}
}
