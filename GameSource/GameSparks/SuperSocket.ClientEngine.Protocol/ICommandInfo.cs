namespace SuperSocket.ClientEngine.Protocol;

public interface ICommandInfo
{
	string Key { get; }
}
public interface ICommandInfo<TCommandData> : ICommandInfo
{
	TCommandData Data { get; }
}
