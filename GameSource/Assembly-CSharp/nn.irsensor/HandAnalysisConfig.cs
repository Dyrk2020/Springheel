using System;

namespace nn.irsensor;

public struct HandAnalysisConfig : IEquatable<HandAnalysisConfig>
{
	public HandAnalysisMode mode;

	public override string ToString()
	{
		return $"({mode})";
	}

	public static bool operator ==(HandAnalysisConfig lhs, HandAnalysisConfig rhs)
	{
		return lhs.mode == rhs.mode;
	}

	public static bool operator !=(HandAnalysisConfig lhs, HandAnalysisConfig rhs)
	{
		return !(lhs == rhs);
	}

	public override bool Equals(object right)
	{
		if (!(right is HandAnalysisConfig))
		{
			return false;
		}
		return Equals((HandAnalysisConfig)right);
	}

	public bool Equals(HandAnalysisConfig other)
	{
		return this == other;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
