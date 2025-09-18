using System.Runtime.InteropServices;

namespace nn.fs;

public static class Rom
{
	public static ErrorRange ResultRomHostFileSystemCorrupted => new ErrorRange(2, 4241, 4260);

	public static ErrorRange ResultRomHostEntryCorrupted => new ErrorRange(2, 4242, 4243);

	public static ErrorRange ResultRomHostFileDataCorrupted => new ErrorRange(2, 4243, 4244);

	public static ErrorRange ResultRomHostFileCorrupted => new ErrorRange(2, 4244, 4245);

	public static ErrorRange ResultInvalidRomHostHandle => new ErrorRange(2, 4245, 4246);

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_fs_QueryMountRomCacheSize")]
	public static extern Result QueryMountRomCacheSize(ref long pOutValue);

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_fs_MountRom")]
	public static extern Result MountRom(string name, byte[] pFileSystemCacheBuffer, long fileSystemCacheBufferSize);

	public static bool CanMountRomForDebug()
	{
		return false;
	}
}
