using System;

namespace WebSocket4Net;

public class MessageReceivedEventArgs : EventArgs
{
	public string Message { get; private set; }

	public MessageReceivedEventArgs(string message)
	{
		Message = message;
	}
}
