using System;
using System.Text;
using SuperSocket.ClientEngine;

namespace WebSocket4Net.Protocol;

internal class HandshakeReader : ReaderBase
{
	private const string m_BadRequestPrefix = "HTTP/1.1 400 ";

	protected static readonly string BadRequestCode;

	protected static readonly byte[] HeaderTerminator;

	private SearchMarkState<byte> m_HeadSeachState;

	protected static WebSocketCommandInfo DefaultHandshakeCommandInfo { get; private set; }

	static HandshakeReader()
	{
		BadRequestCode = 400.ToString();
		HeaderTerminator = Encoding.UTF8.GetBytes("\r\n\r\n");
	}

	public HandshakeReader(WebSocket websocket)
		: base(websocket)
	{
		m_HeadSeachState = new SearchMarkState<byte>(HeaderTerminator);
	}

	public override WebSocketCommandInfo GetCommandInfo(byte[] readBuffer, int offset, int length, out int left)
	{
		left = 0;
		int matched = m_HeadSeachState.Matched;
		int num = readBuffer.SearchMark(offset, length, m_HeadSeachState);
		if (num < 0)
		{
			AddArraySegment(readBuffer, offset, length);
			return null;
		}
		int num2 = num - offset;
		string empty = string.Empty;
		if (base.BufferSegments.Count > 0)
		{
			if (num2 > 0)
			{
				AddArraySegment(readBuffer, offset, num2);
				empty = base.BufferSegments.Decode(Encoding.UTF8);
			}
			else
			{
				empty = base.BufferSegments.Decode(Encoding.UTF8, 0, base.BufferSegments.Count - matched);
			}
		}
		else
		{
			empty = Encoding.UTF8.GetString(readBuffer, offset, num2);
		}
		left = length - num2 - (HeaderTerminator.Length - matched);
		base.BufferSegments.ClearSegements();
		if (!empty.StartsWith("HTTP/1.1 400 ", StringComparison.OrdinalIgnoreCase))
		{
			WebSocketCommandInfo webSocketCommandInfo = new WebSocketCommandInfo();
			webSocketCommandInfo.Key = (-1).ToString();
			webSocketCommandInfo.Text = empty;
			return webSocketCommandInfo;
		}
		WebSocketCommandInfo webSocketCommandInfo2 = new WebSocketCommandInfo();
		webSocketCommandInfo2.Key = 400.ToString();
		webSocketCommandInfo2.Text = empty;
		return webSocketCommandInfo2;
	}
}
