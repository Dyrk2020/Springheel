namespace Moserware.Skills;

internal static class PartialPlay
{
	public static double GetPartialPlayPercentage(object player)
	{
		if (!(player is ISupportPartialPlay { PartialPlayPercentage: var num }))
		{
			return 1.0;
		}
		if (num < 0.0001)
		{
			num = 0.0001;
		}
		return num;
	}
}
