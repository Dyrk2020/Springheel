namespace Moserware.Skills;

public class GameInfo
{
	private const double DefaultBeta = 4.166666666666667;

	private const double DefaultDrawProbability = 0.1;

	private const double DefaultDynamicsFactor = 1.0 / 12.0;

	private const double DefaultInitialMean = 25.0;

	private const double DefaultInitialStandardDeviation = 8.333333333333334;

	public double InitialMean { get; set; }

	public double InitialStandardDeviation { get; set; }

	public double Beta { get; set; }

	public double DynamicsFactor { get; set; }

	public double DrawProbability { get; set; }

	public Rating DefaultRating => new Rating(InitialMean, InitialStandardDeviation);

	public static GameInfo DefaultGameInfo => new GameInfo(25.0, 8.333333333333334, 4.166666666666667, 1.0 / 12.0, 0.1);

	public GameInfo(double initialMean, double initialStandardDeviation, double beta, double dynamicFactor, double drawProbability)
	{
		InitialMean = initialMean;
		InitialStandardDeviation = initialStandardDeviation;
		Beta = beta;
		DynamicsFactor = dynamicFactor;
		DrawProbability = drawProbability;
	}
}
