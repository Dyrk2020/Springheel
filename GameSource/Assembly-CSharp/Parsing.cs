using System;
using System.Globalization;
using System.Threading;

public class Parsing
{
	public class EnsureInvariantCulture
	{
		private CultureInfo previousCulture;

		public EnsureInvariantCulture()
		{
			previousCulture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = invariantCulture;
		}

		~EnsureInvariantCulture()
		{
			if (previousCulture != null)
			{
				Thread.CurrentThread.CurrentCulture = previousCulture;
			}
		}
	}

	private static CultureInfo _invariantCulture;

	public static bool useCurrentCulture;

	public static CultureInfo invariantCulture => CultureInfo.InvariantCulture;

	public static CultureInfo CurrentCultureInfo => Thread.CurrentThread.CurrentCulture;

	public static float ParseFloat_InvariantCulture(string input)
	{
		return float.Parse(input, invariantCulture);
	}

	public static float ParseFloat(string input)
	{
		if (useCurrentCulture)
		{
			return float.Parse(input);
		}
		return ParseFloat_InvariantCulture(input);
	}

	public static double ParseDouble_InvariantCulture(string input)
	{
		return double.Parse(input, invariantCulture);
	}

	public static double ParseDouble(string input)
	{
		if (useCurrentCulture)
		{
			return double.Parse(input);
		}
		return ParseDouble_InvariantCulture(input);
	}

	public static bool TryParseFloat(string input, out float result)
	{
		try
		{
			float num = ParseFloat(input);
			result = num;
			return true;
		}
		catch (Exception)
		{
			result = 0f;
			return false;
		}
	}

	public static bool TryParseDouble(string input, out double result)
	{
		try
		{
			double num = ParseDouble(input);
			result = num;
			return true;
		}
		catch (Exception)
		{
			result = 0.0;
			return false;
		}
	}
}
