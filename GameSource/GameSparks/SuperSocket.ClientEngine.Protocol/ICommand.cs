namespace SuperSocket.ClientEngine.Protocol;

public interface ICommand
{
	string Name { get; }
}
public interface ICommand<TSession, TCommandInfo> : ICommand where TCommandInfo : ICommandInfo
{
	void ExecuteCommand(TSession session, TCommandInfo commandInfo);
}
