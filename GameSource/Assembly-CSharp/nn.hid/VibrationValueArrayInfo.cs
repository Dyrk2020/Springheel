using System.Runtime.InteropServices;

namespace nn.hid;

public struct VibrationValueArrayInfo
{
	public int sampleLength;

	[MarshalAs(UnmanagedType.U1)]
	public bool isLoop;

	public uint loopStartPosition;

	public uint loopEndPosition;

	public uint loopInterval;

	public override string ToString()
	{
		return $"SampleLength:{sampleLength} Loop:{isLoop}({loopStartPosition} - {loopEndPosition}, {loopInterval})";
	}
}
