using System;
using Moserware.Numerics;
using Moserware.Skills.FactorGraphs;

namespace Moserware.Skills.TrueSkill.Factors;

public class GaussianLikelihoodFactor : GaussianFactor
{
	private readonly double _Precision;

	public override double LogNormalization => GaussianDistribution.LogRatioNormalization(base.Variables[0].Value, base.Messages[0].Value);

	public GaussianLikelihoodFactor(double betaSquared, Variable<GaussianDistribution> variable1, Variable<GaussianDistribution> variable2)
		: base($"Likelihood of {variable2} going to {variable1}")
	{
		_Precision = 1.0 / betaSquared;
		CreateVariableToMessageBinding(variable1);
		CreateVariableToMessageBinding(variable2);
	}

	private double UpdateHelper(Message<GaussianDistribution> message1, Message<GaussianDistribution> message2, Variable<GaussianDistribution> variable1, Variable<GaussianDistribution> variable2)
	{
		GaussianDistribution gaussianDistribution = message1.Value.Clone();
		GaussianDistribution gaussianDistribution2 = message2.Value.Clone();
		GaussianDistribution gaussianDistribution3 = variable1.Value.Clone();
		GaussianDistribution gaussianDistribution4 = variable2.Value.Clone();
		double num = _Precision / (_Precision + gaussianDistribution4.Precision - gaussianDistribution2.Precision);
		GaussianDistribution gaussianDistribution5 = GaussianDistribution.FromPrecisionMean(num * (gaussianDistribution4.PrecisionMean - gaussianDistribution2.PrecisionMean), num * (gaussianDistribution4.Precision - gaussianDistribution2.Precision));
		GaussianDistribution gaussianDistribution6 = gaussianDistribution3 / gaussianDistribution * gaussianDistribution5;
		message1.Value = gaussianDistribution5;
		variable1.Value = gaussianDistribution6;
		return gaussianDistribution6 - gaussianDistribution3;
	}

	public override double UpdateMessage(int messageIndex)
	{
		return messageIndex switch
		{
			0 => UpdateHelper(base.Messages[0], base.Messages[1], base.Variables[0], base.Variables[1]), 
			1 => UpdateHelper(base.Messages[1], base.Messages[0], base.Variables[1], base.Variables[0]), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
