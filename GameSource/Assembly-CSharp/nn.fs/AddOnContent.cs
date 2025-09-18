namespace nn.fs;

public static class AddOnContent
{
	public static Result QueryMountCacheSize(ref long pOutValue, int targetIndex)
	{
		pOutValue = 0L;
		return default(Result);
	}

	public static Result Mount(string name, int targetIndex, byte[] pFileSystemCacheBuffer, long fileSystemCacheBufferSize)
	{
		return default(Result);
	}
}
