using System;
using Moserware.Numerics;
using Moserware.Skills.FactorGraphs;

namespace Moserware.Skills.TrueSkill.Factors;

public class GaussianGreaterThanFactor : GaussianFactor
{
	private readonly double _Epsilon;

	public override double LogNormalization
	{
		get
		{
			GaussianDistribution value = base.Variables[0].Value;
			GaussianDistribution value2 = base.Messages[0].Value;
			GaussianDistribution gaussianDistribution = value / value2;
			return 0.0 - GaussianDistribution.LogProductNormalization(gaussianDistribution, value2) + Math.Log(GaussianDistribution.CumulativeTo((gaussianDistribution.Mean - _Epsilon) / gaussianDistribution.StandardDeviation));
		}
	}

	public GaussianGreaterThanFactor(double epsilon, Variable<GaussianDistribution> variable)
		: base($"{variable} > {epsilon:0.000}")
	{
		_Epsilon = epsilon;
		CreateVariableToMessageBinding(variable);
	}

	protected override double UpdateMessage(Message<GaussianDistribution> message, Variable<GaussianDistribution> variable)
	{
		GaussianDistribution gaussianDistribution = variable.Value.Clone();
		GaussianDistribution gaussianDistribution2 = message.Value.Clone();
		GaussianDistribution gaussianDistribution3 = gaussianDistribution / gaussianDistribution2;
		double precision = gaussianDistribution3.Precision;
		double precisionMean = gaussianDistribution3.PrecisionMean;
		double num = Math.Sqrt(precision);
		double teamPerformanceDifference = precisionMean / num;
		double drawMargin = _Epsilon * num;
		double precisionMean2 = gaussianDistribution3.PrecisionMean;
		double num2 = 1.0 - TruncatedGaussianCorrectionFunctions.WExceedsMargin(teamPerformanceDifference, drawMargin);
		GaussianDistribution gaussianDistribution4 = GaussianDistribution.FromPrecisionMean(precision: precision / num2, precisionMean: (precisionMean2 + num * TruncatedGaussianCorrectionFunctions.VExceedsMargin(teamPerformanceDifference, drawMargin)) / num2);
		GaussianDistribution value = gaussianDistribution2 * gaussianDistribution4 / gaussianDistribution;
		message.Value = value;
		variable.Value = gaussianDistribution4;
		return gaussianDistribution4 - gaussianDistribution;
	}
}
