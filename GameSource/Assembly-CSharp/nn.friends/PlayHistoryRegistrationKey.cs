using System.Runtime.InteropServices;

namespace nn.friends;

[StructLayout(LayoutKind.Sequential, Size = 64)]
public struct PlayHistoryRegistrationKey
{
	private const int Size = 64;

	public byte[] GetValue()
	{
		byte[] array = new byte[64];
		GCHandle gCHandle = GCHandle.Alloc(this, GCHandleType.Pinned);
		Marshal.Copy(gCHandle.AddrOfPinnedObject(), array, 0, 64);
		gCHandle.Free();
		return array;
	}
}
