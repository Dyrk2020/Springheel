using System;

namespace BrainCloud;

public static class TimeUtil
{
	public static long UTCDateTimeToUTCMillis(this DateTime utcDateTime)
	{
		return (utcDateTime.Ticks - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).Ticks) / 10000;
	}

	public static DateTime UTCMillisToUTCDateTime(this long utcMillis)
	{
		long value = utcMillis * 10000;
		return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddTicks(value);
	}

	public static DateTime LocalTimeToUTCTime(this DateTime localDate)
	{
		return localDate.ToUniversalTime();
	}

	public static DateTime UTCTimeToLocalTime(this DateTime utcDate)
	{
		return utcDate.ToLocalTime();
	}

	public static long DateTimeOffsetToUTCMillis(this DateTimeOffset utcDateTimeOffset)
	{
		return (utcDateTimeOffset.Ticks - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).Ticks) / 10000;
	}

	public static DateTimeOffset UTCMillisToDateTimeOffset(this long utcMillis)
	{
		long ticks = utcMillis * 10000;
		return new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).AddTicks(ticks);
	}

	public static DateTimeOffset LocalTimeToUTCTime(this DateTimeOffset localDate)
	{
		return localDate.ToUniversalTime();
	}

	public static DateTimeOffset UTCTimeToLocalTime(this DateTimeOffset utcDate)
	{
		return utcDate.ToLocalTime();
	}
}
