using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ClientEngine.Protocol;
using WebSocket4Net.Protocol;

namespace WebSocket4Net;

public class WebSocketCommandInfo : ICommandInfo
{
	public string Key { get; set; }

	public byte[] Data { get; set; }

	public string Text { get; set; }

	public short CloseStatusCode { get; private set; }

	public WebSocketCommandInfo()
	{
	}

	public WebSocketCommandInfo(string key)
	{
		Key = key;
	}

	public WebSocketCommandInfo(string key, string text)
	{
		Key = key;
		Text = text;
	}

	public WebSocketCommandInfo(IList<WebSocketDataFrame> frames)
	{
		sbyte opCode = frames[0].OpCode;
		Key = opCode.ToString();
		switch (opCode)
		{
		case 8:
		{
			WebSocketDataFrame webSocketDataFrame2 = frames[0];
			int length = (int)webSocketDataFrame2.ActualPayloadLength;
			int num2 = webSocketDataFrame2.InnerData.Count - length;
			StringBuilder stringBuilder = new StringBuilder();
			if (length >= 2)
			{
				num2 = webSocketDataFrame2.InnerData.Count - length;
				byte[] array2 = webSocketDataFrame2.InnerData.ToArrayData(num2, 2);
				CloseStatusCode = (short)(array2[0] * 256 + array2[1]);
				if (length > 2)
				{
					stringBuilder.Append(webSocketDataFrame2.InnerData.Decode(Encoding.UTF8, num2 + 2, length - 2));
				}
			}
			else if (length > 0)
			{
				stringBuilder.Append(webSocketDataFrame2.InnerData.Decode(Encoding.UTF8, num2, length));
			}
			if (frames.Count > 1)
			{
				for (int num3 = 1; num3 < frames.Count; num3++)
				{
					WebSocketDataFrame webSocketDataFrame3 = frames[num3];
					num2 = webSocketDataFrame3.InnerData.Count - (int)webSocketDataFrame3.ActualPayloadLength;
					length = (int)webSocketDataFrame3.ActualPayloadLength;
					if (webSocketDataFrame3.HasMask)
					{
						webSocketDataFrame3.InnerData.DecodeMask(webSocketDataFrame3.MaskKey, num2, length);
					}
					stringBuilder.Append(webSocketDataFrame3.InnerData.Decode(Encoding.UTF8, num2, length));
				}
			}
			Text = stringBuilder.ToString();
			return;
		}
		case 2:
		{
			byte[] array = new byte[frames.Sum((WebSocketDataFrame f) => (int)f.ActualPayloadLength)];
			int toIndex = 0;
			for (int num = 0; num < frames.Count; num++)
			{
				WebSocketDataFrame webSocketDataFrame = frames[num];
				int num2 = webSocketDataFrame.InnerData.Count - (int)webSocketDataFrame.ActualPayloadLength;
				int length = (int)webSocketDataFrame.ActualPayloadLength;
				if (webSocketDataFrame.HasMask)
				{
					webSocketDataFrame.InnerData.DecodeMask(webSocketDataFrame.MaskKey, num2, length);
				}
				webSocketDataFrame.InnerData.CopyTo(array, num2, toIndex, length);
			}
			Data = array;
			return;
		}
		}
		StringBuilder stringBuilder2 = new StringBuilder();
		for (int num4 = 0; num4 < frames.Count; num4++)
		{
			WebSocketDataFrame webSocketDataFrame4 = frames[num4];
			int num2 = webSocketDataFrame4.InnerData.Count - (int)webSocketDataFrame4.ActualPayloadLength;
			int length = (int)webSocketDataFrame4.ActualPayloadLength;
			if (webSocketDataFrame4.HasMask)
			{
				webSocketDataFrame4.InnerData.DecodeMask(webSocketDataFrame4.MaskKey, num2, length);
			}
			stringBuilder2.Append(webSocketDataFrame4.InnerData.Decode(Encoding.UTF8, num2, length));
		}
		Text = stringBuilder2.ToString();
	}

	public WebSocketCommandInfo(WebSocketDataFrame frame)
	{
		Key = frame.OpCode.ToString();
		int num = (int)frame.ActualPayloadLength;
		int num2 = frame.InnerData.Count - (int)frame.ActualPayloadLength;
		if (frame.HasMask && num > 0)
		{
			frame.InnerData.DecodeMask(frame.MaskKey, num2, num);
		}
		if (frame.OpCode == 8 && num >= 2)
		{
			byte[] array = frame.InnerData.ToArrayData(num2, 2);
			CloseStatusCode = (short)(array[0] * 256 + array[1]);
			if (num > 2)
			{
				Text = frame.InnerData.Decode(Encoding.UTF8, num2 + 2, num - 2);
			}
			else
			{
				Text = string.Empty;
			}
		}
		else if (frame.OpCode != 2)
		{
			if (num > 0)
			{
				Text = frame.InnerData.Decode(Encoding.UTF8, num2, num);
			}
			else
			{
				Text = string.Empty;
			}
		}
		else if (num > 0)
		{
			Data = frame.InnerData.ToArrayData(num2, num);
		}
		else
		{
			Data = new byte[0];
		}
	}
}
