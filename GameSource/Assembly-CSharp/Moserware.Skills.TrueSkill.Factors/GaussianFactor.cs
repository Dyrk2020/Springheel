using Moserware.Numerics;
using Moserware.Skills.FactorGraphs;

namespace Moserware.Skills.TrueSkill.Factors;

public abstract class GaussianFactor : Factor<GaussianDistribution>
{
	protected GaussianFactor(string name)
		: base(name)
	{
	}

	protected override double SendMessage(Message<GaussianDistribution> message, Variable<GaussianDistribution> variable)
	{
		GaussianDistribution value = variable.Value;
		GaussianDistribution value2 = message.Value;
		double result = GaussianDistribution.LogProductNormalization(value, value2);
		variable.Value = value * value2;
		return result;
	}

	public override Message<GaussianDistribution> CreateVariableToMessageBinding(Variable<GaussianDistribution> variable)
	{
		return CreateVariableToMessageBinding(variable, new Message<GaussianDistribution>(GaussianDistribution.FromPrecisionMean(0.0, 0.0), "message from {0} to {1}", this, variable));
	}
}
