using System;
using Moserware.Numerics;

namespace Moserware.Skills.TrueSkill;

internal static class DrawMargin
{
	public static double GetDrawMarginFromDrawProbability(double drawProbability, double beta)
	{
		return GaussianDistribution.InverseCumulativeTo(0.5 * (drawProbability + 1.0), 0.0, 1.0) * Math.Sqrt(2.0) * beta;
	}
}
