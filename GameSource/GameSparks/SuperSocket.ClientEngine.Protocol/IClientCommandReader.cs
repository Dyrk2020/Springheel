namespace SuperSocket.ClientEngine.Protocol;

public interface IClientCommandReader<TCommandInfo> where TCommandInfo : ICommandInfo
{
	IClientCommandReader<TCommandInfo> NextCommandReader { get; }

	TCommandInfo GetCommandInfo(byte[] readBuffer, int offset, int length, out int left);
}
