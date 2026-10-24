using System;

public static class DateTimeOffsetExtensions
{
	public static DateTimeOffset Trunc(this DateTimeOffset dateTimeOffset, TimeSpan timeSpan)
	{
		if (timeSpan == TimeSpan.Zero)
		{
			return dateTimeOffset;
		}
		return dateTimeOffset.AddTicks(-(dateTimeOffset.Ticks % timeSpan.Ticks));
	}
}
