namespace nn.err;

public struct ErrorCode
{
	public uint category;

	public uint number;

	public override string ToString()
	{
		return $"(0x{category:X8} 0x{number:X8})";
	}

	public bool IsValid()
	{
		return true;
	}

	public static ErrorCode GetInvalidErrorCode()
	{
		return default(ErrorCode);
	}
}
