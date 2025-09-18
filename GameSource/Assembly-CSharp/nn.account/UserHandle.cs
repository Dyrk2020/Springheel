namespace nn.account;

public struct UserHandle
{
	public ulong _data0;

	public ulong _data1;

	public ulong _context;

	public override string ToString()
	{
		return $"{_data0:X16}{_data1:X16}_{_context:X16}";
	}
}
