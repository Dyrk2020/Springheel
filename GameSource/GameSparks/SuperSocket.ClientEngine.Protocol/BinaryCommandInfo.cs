namespace SuperSocket.ClientEngine.Protocol;

public class BinaryCommandInfo : CommandInfo<byte[]>
{
	public BinaryCommandInfo(string key, byte[] data)
		: base(key, data)
	{
	}
}
