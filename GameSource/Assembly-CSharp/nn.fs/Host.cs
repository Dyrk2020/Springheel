namespace nn.fs;

public static class Host
{
	public enum MountHostOptionFlag
	{
		PseudoCaseSensitive = 1
	}

	public struct MountHostOption
	{
		public int flags;

		public static MountHostOption MakeValue(int flags)
		{
			return new MountHostOption
			{
				flags = flags
			};
		}
	}

	public static ErrorRange ResultSaveDataHostFileSystemCorrupted => new ErrorRange(2, 4441, 4460);

	public static ErrorRange ResultSaveDataHostEntryCorrupted => new ErrorRange(2, 4442, 4443);

	public static ErrorRange ResultSaveDataHostFileDataCorrupted => new ErrorRange(2, 4443, 4444);

	public static ErrorRange ResultSaveDataHostFileCorrupted => new ErrorRange(2, 4444, 4445);

	public static ErrorRange ResultInvalidSaveDataHostHandle => new ErrorRange(2, 4445, 4446);

	public static ErrorRange ResultHostFileSystemCorrupted => new ErrorRange(2, 4701, 4720);

	public static ErrorRange ResultHostEntryCorrupted => new ErrorRange(2, 4702, 4703);

	public static ErrorRange ResultHostFileDataCorrupted => new ErrorRange(2, 4703, 4704);

	public static ErrorRange ResultHostFileCorrupted => new ErrorRange(2, 4704, 4705);

	public static ErrorRange ResultInvalidHostHandle => new ErrorRange(2, 4705, 4706);

	public static Result MountHost(string name, string rootPath)
	{
		Nn.Abort("To enable nn.fs.Host class in UnityEditor or in a relase build, define NN_FS_HOST_ENABLE symbol. It cannot be used for the master ROM submission.");
		return default(Result);
	}

	public static Result MountHost(string name, string rootPath, MountHostOption option)
	{
		Nn.Abort("To enable nn.fs.Host class in UnityEditor or in a relase build, define NN_FS_HOST_ENABLE symbol. It cannot be used for the master ROM submission.");
		return default(Result);
	}

	public static Result MountHostRoot()
	{
		Nn.Abort("To enable nn.fs.Host class in UnityEditor or in a relase build, define NN_FS_HOST_ENABLE symbol. It cannot be used for the master ROM submission.");
		return default(Result);
	}

	public static Result MountHostRoot(MountHostOption option)
	{
		Nn.Abort("To enable nn.fs.Host class in UnityEditor or in a relase build, define NN_FS_HOST_ENABLE symbol. It cannot be used for the master ROM submission.");
		return default(Result);
	}

	public static void UnMountHostRoot()
	{
		Nn.Abort("To enable nn.fs.Host class in UnityEditor or in a relase build, define NN_FS_HOST_ENABLE symbol. It cannot be used for the master ROM submission.");
	}

	public static void DisableAbortByHostAccessFailed()
	{
		Nn.Abort("To enable nn.fs.Host class in UnityEditor or in a relase build, define NN_FS_HOST_ENABLE symbol. It cannot be used for the master ROM submission.");
	}
}
