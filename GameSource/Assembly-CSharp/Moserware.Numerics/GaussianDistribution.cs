using System;

namespace Moserware.Numerics;

public class GaussianDistribution
{
	public double Mean { get; private set; }

	public double StandardDeviation { get; private set; }

	public double Precision { get; private set; }

	public double PrecisionMean { get; private set; }

	private double Variance { get; set; }

	public double NormalizationConstant => 1.0 / (Math.Sqrt(Math.PI * 2.0) * StandardDeviation);

	private GaussianDistribution()
	{
	}

	public GaussianDistribution(double mean, double standardDeviation)
	{
		Mean = mean;
		StandardDeviation = standardDeviation;
		Variance = Square(StandardDeviation);
		Precision = 1.0 / Variance;
		PrecisionMean = Precision * Mean;
	}

	public GaussianDistribution Clone()
	{
		return new GaussianDistribution
		{
			Mean = Mean,
			StandardDeviation = StandardDeviation,
			Variance = Variance,
			Precision = Precision,
			PrecisionMean = PrecisionMean
		};
	}

	public static GaussianDistribution FromPrecisionMean(double precisionMean, double precision)
	{
		GaussianDistribution gaussianDistribution = new GaussianDistribution();
		gaussianDistribution.Precision = precision;
		gaussianDistribution.PrecisionMean = precisionMean;
		gaussianDistribution.Variance = 1.0 / precision;
		gaussianDistribution.StandardDeviation = Math.Sqrt(gaussianDistribution.Variance);
		gaussianDistribution.Mean = gaussianDistribution.PrecisionMean / gaussianDistribution.Precision;
		return gaussianDistribution;
	}

	public static GaussianDistribution operator *(GaussianDistribution left, GaussianDistribution right)
	{
		return FromPrecisionMean(left.PrecisionMean + right.PrecisionMean, left.Precision + right.Precision);
	}

	public static double AbsoluteDifference(GaussianDistribution left, GaussianDistribution right)
	{
		return Math.Max(Math.Abs(left.PrecisionMean - right.PrecisionMean), Math.Sqrt(Math.Abs(left.Precision - right.Precision)));
	}

	public static double operator -(GaussianDistribution left, GaussianDistribution right)
	{
		return AbsoluteDifference(left, right);
	}

	public static double LogProductNormalization(GaussianDistribution left, GaussianDistribution right)
	{
		if (left.Precision == 0.0 || right.Precision == 0.0)
		{
			return 0.0;
		}
		double num = left.Variance + right.Variance;
		double x = left.Mean - right.Mean;
		return 0.0 - Math.Log(Math.Sqrt(Math.PI * 2.0)) - Math.Log(num) / 2.0 - Square(x) / (2.0 * num);
	}

	public static GaussianDistribution operator /(GaussianDistribution numerator, GaussianDistribution denominator)
	{
		return FromPrecisionMean(numerator.PrecisionMean - denominator.PrecisionMean, numerator.Precision - denominator.Precision);
	}

	public static double LogRatioNormalization(GaussianDistribution numerator, GaussianDistribution denominator)
	{
		if (numerator.Precision == 0.0 || denominator.Precision == 0.0)
		{
			return 0.0;
		}
		double num = denominator.Variance - numerator.Variance;
		double x = numerator.Mean - denominator.Mean;
		double num2 = Math.Log(Math.Sqrt(Math.PI * 2.0));
		return Math.Log(denominator.Variance) + num2 - Math.Log(num) / 2.0 + Square(x) / (2.0 * num);
	}

	private static double Square(double x)
	{
		return x * x;
	}

	public static double At(double x)
	{
		return At(x, 0.0, 1.0);
	}

	public static double At(double x, double mean, double standardDeviation)
	{
		double num = 1.0 / (standardDeviation * Math.Sqrt(Math.PI * 2.0));
		double num2 = Math.Exp(-1.0 * Math.Pow(x - mean, 2.0) / (2.0 * (standardDeviation * standardDeviation)));
		return num * num2;
	}

	public static double CumulativeTo(double x, double mean, double standardDeviation)
	{
		double num = ErrorFunctionCumulativeTo(-0.7071067811865476 * x);
		return 0.5 * num;
	}

	public static double CumulativeTo(double x)
	{
		return CumulativeTo(x, 0.0, 1.0);
	}

	private static double ErrorFunctionCumulativeTo(double x)
	{
		double num = Math.Abs(x);
		double num2 = 2.0 / (2.0 + num);
		double num3 = 4.0 * num2 - 2.0;
		double[] array = new double[28]
		{
			-1.3026537197817094, 0.6419697923564902, 0.019476473204185836, -0.00956151478680863, -0.000946595344482036, 0.000366839497852761, 4.2523324806907E-05, -2.0278578112534E-05, -1.624290004647E-06, 1.30365583558E-06,
			1.5626441722E-08, -8.5238095915E-08, 6.529054439E-09, 5.059343495E-09, -9.91364156E-10, -2.27365122E-10, 9.6467911E-11, 2.394038E-12, -6.886027E-12, 8.94487E-13,
			3.13092E-13, -1.12708E-13, 3.81E-16, 7.106E-15, -1.523E-15, -9.4E-17, 1.21E-16, -2.8E-17
		};
		int num4 = array.Length;
		double num5 = 0.0;
		double num6 = 0.0;
		for (int num7 = num4 - 1; num7 > 0; num7--)
		{
			double num8 = num5;
			num5 = num3 * num5 - num6 + array[num7];
			num6 = num8;
		}
		double num9 = num2 * Math.Exp((0.0 - num) * num + 0.5 * (array[0] + num3 * num5) - num6);
		if (!(x >= 0.0))
		{
			return 2.0 - num9;
		}
		return num9;
	}

	private static double InverseErrorFunctionCumulativeTo(double p)
	{
		if (p >= 2.0)
		{
			return -100.0;
		}
		if (p <= 0.0)
		{
			return 100.0;
		}
		double num = ((p < 1.0) ? p : (2.0 - p));
		double num2 = Math.Sqrt(-2.0 * Math.Log(num / 2.0));
		double num3 = -0.70711 * ((2.30753 + num2 * 0.27061) / (1.0 + num2 * (0.99229 + num2 * 0.04481)) - num2);
		for (int i = 0; i < 2; i++)
		{
			double num4 = ErrorFunctionCumulativeTo(num3) - num;
			num3 += num4 / (1.1283791670955126 * Math.Exp(0.0 - num3 * num3) - num3 * num4);
		}
		if (!(p < 1.0))
		{
			return 0.0 - num3;
		}
		return num3;
	}

	public static double InverseCumulativeTo(double x, double mean, double standardDeviation)
	{
		return mean - Math.Sqrt(2.0) * standardDeviation * InverseErrorFunctionCumulativeTo(2.0 * x);
	}

	public static double InverseCumulativeTo(double x)
	{
		return InverseCumulativeTo(x, 0.0, 1.0);
	}

	public override string ToString()
	{
		return $"μ={Mean:0.0000}, σ={StandardDeviation:0.0000}";
	}
}
