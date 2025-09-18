using SuperSocket.ClientEngine.Protocol;

namespace WebSocket4Net.Command;

public abstract class WebSocketCommandBase : ICommand<WebSocket, WebSocketCommandInfo>, ICommand
{
	public abstract string Name { get; }

	public abstract void ExecuteCommand(WebSocket session, WebSocketCommandInfo commandInfo);
}
