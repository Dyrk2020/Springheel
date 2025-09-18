using System;
using System.Runtime.InteropServices;

namespace nn.irsensor;

public struct IrCameraConfig : IEquatable<IrCameraConfig>
{
	public long exposureTimeNanoSeconds;

	public IrCameraLightTarget lightTarget;

	public int gain;

	[MarshalAs(UnmanagedType.U1)]
	public bool isNegativeImageUsed;

	public override string ToString()
	{
		return $"({exposureTimeNanoSeconds} {lightTarget} {gain} {isNegativeImageUsed})";
	}

	public static bool operator ==(IrCameraConfig lhs, IrCameraConfig rhs)
	{
		if (lhs.exposureTimeNanoSeconds == rhs.exposureTimeNanoSeconds && lhs.lightTarget == rhs.lightTarget && lhs.gain == rhs.gain)
		{
			return lhs.isNegativeImageUsed == rhs.isNegativeImageUsed;
		}
		return false;
	}

	public static bool operator !=(IrCameraConfig lhs, IrCameraConfig rhs)
	{
		return !(lhs == rhs);
	}

	public override bool Equals(object right)
	{
		if (!(right is IrCameraConfig))
		{
			return false;
		}
		return Equals((IrCameraConfig)right);
	}

	public bool Equals(IrCameraConfig other)
	{
		return this == other;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
