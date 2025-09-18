using System;
using Moserware.Numerics;
using Moserware.Skills.FactorGraphs;

namespace Moserware.Skills.TrueSkill.Factors;

public class GaussianPriorFactor : GaussianFactor
{
	private readonly GaussianDistribution _NewMessage;

	public GaussianPriorFactor(double mean, double variance, Variable<GaussianDistribution> variable)
		: base($"Prior value going to {variable}")
	{
		_NewMessage = new GaussianDistribution(mean, Math.Sqrt(variance));
		CreateVariableToMessageBinding(variable, new Message<GaussianDistribution>(GaussianDistribution.FromPrecisionMean(0.0, 0.0), "message from {0} to {1}", this, variable));
	}

	protected override double UpdateMessage(Message<GaussianDistribution> message, Variable<GaussianDistribution> variable)
	{
		GaussianDistribution gaussianDistribution = variable.Value.Clone();
		GaussianDistribution gaussianDistribution2 = (variable.Value = GaussianDistribution.FromPrecisionMean(gaussianDistribution.PrecisionMean + _NewMessage.PrecisionMean - message.Value.PrecisionMean, gaussianDistribution.Precision + _NewMessage.Precision - message.Value.Precision));
		message.Value = _NewMessage;
		return gaussianDistribution - gaussianDistribution2;
	}
}
