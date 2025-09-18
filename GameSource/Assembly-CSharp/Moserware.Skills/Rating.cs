using Moserware.Numerics;

namespace Moserware.Skills;

public class Rating
{
	private const int ConservativeStandardDeviationMultiplier = 3;

	private readonly double _ConservativeStandardDeviationMultiplier;

	private readonly double _Mean;

	private readonly double _StandardDeviation;

	public double Mean => _Mean;

	public double StandardDeviation => _StandardDeviation;

	public double ConservativeRating => _Mean - _ConservativeStandardDeviationMultiplier * _StandardDeviation;

	public Rating(double mean, double standardDeviation)
		: this(mean, standardDeviation, 3.0)
	{
	}

	public Rating(double mean, double standardDeviation, double conservativeStandardDeviationMultiplier)
	{
		_Mean = mean;
		_StandardDeviation = standardDeviation;
		_ConservativeStandardDeviationMultiplier = conservativeStandardDeviationMultiplier;
	}

	public static Rating GetPartialUpdate(Rating prior, Rating fullPosterior, double updatePercentage)
	{
		GaussianDistribution gaussianDistribution = new GaussianDistribution(prior.Mean, prior.StandardDeviation);
		GaussianDistribution gaussianDistribution2 = new GaussianDistribution(fullPosterior.Mean, fullPosterior.StandardDeviation);
		double num = gaussianDistribution2.Precision - gaussianDistribution.Precision;
		double num2 = updatePercentage * num;
		double num3 = gaussianDistribution2.PrecisionMean - gaussianDistribution.PrecisionMean;
		double num4 = updatePercentage * num3;
		GaussianDistribution gaussianDistribution3 = GaussianDistribution.FromPrecisionMean(gaussianDistribution.PrecisionMean + num4, gaussianDistribution.Precision + num2);
		return new Rating(gaussianDistribution3.Mean, gaussianDistribution3.StandardDeviation, prior._ConservativeStandardDeviationMultiplier);
	}

	public override string ToString()
	{
		return $"μ={Mean:0.0000}, σ={StandardDeviation:0.0000}";
	}
}
