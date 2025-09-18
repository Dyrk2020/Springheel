using System;
using Moserware.Numerics;

namespace Moserware.Skills.TrueSkill;

internal static class TruncatedGaussianCorrectionFunctions
{
	public static double VExceedsMargin(double teamPerformanceDifference, double drawMargin, double c)
	{
		return VExceedsMargin(teamPerformanceDifference / c, drawMargin / c);
	}

	public static double VExceedsMargin(double teamPerformanceDifference, double drawMargin)
	{
		double num = GaussianDistribution.CumulativeTo(teamPerformanceDifference - drawMargin);
		if (num < 2.222758749E-162)
		{
			return 0.0 - teamPerformanceDifference + drawMargin;
		}
		return GaussianDistribution.At(teamPerformanceDifference - drawMargin) / num;
	}

	public static double WExceedsMargin(double teamPerformanceDifference, double drawMargin, double c)
	{
		return WExceedsMargin(teamPerformanceDifference / c, drawMargin / c);
	}

	public static double WExceedsMargin(double teamPerformanceDifference, double drawMargin)
	{
		if (GaussianDistribution.CumulativeTo(teamPerformanceDifference - drawMargin) < 2.222758749E-162)
		{
			if (teamPerformanceDifference < 0.0)
			{
				return 1.0;
			}
			return 0.0;
		}
		double num = VExceedsMargin(teamPerformanceDifference, drawMargin);
		return num * (num + teamPerformanceDifference - drawMargin);
	}

	public static double VWithinMargin(double teamPerformanceDifference, double drawMargin, double c)
	{
		return VWithinMargin(teamPerformanceDifference / c, drawMargin / c);
	}

	public static double VWithinMargin(double teamPerformanceDifference, double drawMargin)
	{
		double num = Math.Abs(teamPerformanceDifference);
		double num2 = GaussianDistribution.CumulativeTo(drawMargin - num) - GaussianDistribution.CumulativeTo(0.0 - drawMargin - num);
		if (num2 < 2.222758749E-162)
		{
			if (teamPerformanceDifference < 0.0)
			{
				return 0.0 - teamPerformanceDifference - drawMargin;
			}
			return 0.0 - teamPerformanceDifference + drawMargin;
		}
		double num3 = GaussianDistribution.At(0.0 - drawMargin - num) - GaussianDistribution.At(drawMargin - num);
		if (teamPerformanceDifference < 0.0)
		{
			return (0.0 - num3) / num2;
		}
		return num3 / num2;
	}

	public static double WWithinMargin(double teamPerformanceDifference, double drawMargin, double c)
	{
		return WWithinMargin(teamPerformanceDifference / c, drawMargin / c);
	}

	public static double WWithinMargin(double teamPerformanceDifference, double drawMargin)
	{
		double num = Math.Abs(teamPerformanceDifference);
		double num2 = GaussianDistribution.CumulativeTo(drawMargin - num) - GaussianDistribution.CumulativeTo(0.0 - drawMargin - num);
		if (num2 < 2.222758749E-162)
		{
			return 1.0;
		}
		double num3 = VWithinMargin(num, drawMargin);
		return num3 * num3 + ((drawMargin - num) * GaussianDistribution.At(drawMargin - num) - (0.0 - drawMargin - num) * GaussianDistribution.At(0.0 - drawMargin - num)) / num2;
	}
}
