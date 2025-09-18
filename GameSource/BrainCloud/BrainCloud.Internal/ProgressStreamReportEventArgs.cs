using System;

namespace BrainCloud.Internal;

public class ProgressStreamReportEventArgs : EventArgs
{
	public int BytesMoved { get; private set; }

	public long StreamLength { get; private set; }

	public long StreamPosition { get; private set; }

	public bool WasRead { get; private set; }

	public ProgressStreamReportEventArgs()
	{
	}

	public ProgressStreamReportEventArgs(int bytesMoved, long streamLength, long streamPosition, bool wasRead)
		: this()
	{
		BytesMoved = bytesMoved;
		StreamLength = streamLength;
		StreamPosition = streamPosition;
		WasRead = WasRead;
	}
}
