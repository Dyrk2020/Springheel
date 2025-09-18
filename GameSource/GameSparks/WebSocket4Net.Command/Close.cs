using System;

namespace WebSocket4Net.Command;

public class Close : WebSocketCommandBase
{
	public override string Name => 8.ToString();

	public override void ExecuteCommand(WebSocket session, WebSocketCommandInfo commandInfo)
	{
		if (session.StateCode == 2)
		{
			if (commandInfo.CloseStatusCode != session.ProtocolProcessor.CloseStatusCode.NormalClosure && (commandInfo.CloseStatusCode > 0 || !string.IsNullOrEmpty(commandInfo.Text)))
			{
				session.FireError(new Exception($"{commandInfo.CloseStatusCode}: {commandInfo.Text}"));
			}
			session.CloseWithoutHandshake();
			return;
		}
		short num = commandInfo.CloseStatusCode;
		if (num <= 0)
		{
			num = session.ProtocolProcessor.CloseStatusCode.NoStatusCode;
		}
		session.Close(num, commandInfo.Text);
	}
}
