using System;

namespace nn.irsensor;

public struct MomentProcessorConfig : IEquatable<MomentProcessorConfig>
{
	public IrCameraConfig irCameraConfig;

	public Rect windowOfInterest;

	public MomentProcessorPreprocess preprocess;

	public int preprocessIntensityThreshold;

	public override string ToString()
	{
		return $"({irCameraConfig} {windowOfInterest} {preprocess} {preprocessIntensityThreshold}";
	}

	public static bool operator ==(MomentProcessorConfig lhs, MomentProcessorConfig rhs)
	{
		if (lhs.irCameraConfig == rhs.irCameraConfig && lhs.windowOfInterest == rhs.windowOfInterest && lhs.preprocess == rhs.preprocess)
		{
			return lhs.preprocessIntensityThreshold == rhs.preprocessIntensityThreshold;
		}
		return false;
	}

	public static bool operator !=(MomentProcessorConfig lhs, MomentProcessorConfig rhs)
	{
		return !(lhs == rhs);
	}

	public override bool Equals(object right)
	{
		if (!(right is MomentProcessorConfig))
		{
			return false;
		}
		return Equals((MomentProcessorConfig)right);
	}

	public bool Equals(MomentProcessorConfig other)
	{
		return this == other;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
