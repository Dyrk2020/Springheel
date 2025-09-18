using System.Runtime.InteropServices;

namespace nn.fs;

public static class FileSystem
{
	public const int MountNameLengthMax = 15;

	public static ErrorRange ResultPathNotFound => new ErrorRange(2, 1, 2);

	public static ErrorRange ResultPathAlreadyExists => new ErrorRange(2, 2, 3);

	public static ErrorRange ResultTargetLocked => new ErrorRange(2, 7, 8);

	public static ErrorRange ResultDirectoryNotEmpty => new ErrorRange(2, 8, 9);

	public static ErrorRange ResultDirectoryStatusChanged => new ErrorRange(2, 13, 14);

	public static ErrorRange ResultUsableSpaceNotEnough => new ErrorRange(2, 30, 46);

	public static ErrorRange ResultUnsupportedSdkVersion => new ErrorRange(2, 50, 51);

	public static ErrorRange ResultMountNameAlreadyExists => new ErrorRange(2, 60, 61);

	public static ErrorRange ResultTargetNotFound => new ErrorRange(2, 1002, 1003);

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_fs_GetEntryType")]
	public static extern Result GetEntryType(ref EntryType outValue, string path);

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_fs_GetFreeSpaceSize")]
	public static extern Result GetFreeSpaceSize(ref long outValue, string path);

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_fs_Unmount")]
	public static extern void Unmount(string name);

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_fs_Commit")]
	public static extern Result Commit(string name);

	[DllImport("NintendoSDKPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nn_fs_Commit1")]
	private static extern Result Commit(string[] name, int nameCount);

	public static Result Commit(string[] name)
	{
		return Commit(name, name.Length);
	}
}
