using System;
using TestModeSkeleton;
using UnityEngine;

public static class DateTimeOffsetManager
{
	private static TimeSpan s_jstTimeSpan = new TimeSpan(9, 0, 0);

	public static DateTimeOffset GetJSTFromString(string timeStr, bool isContainOffset = false)
	{
		if (string.IsNullOrEmpty(timeStr))
		{
			return default(DateTimeOffset);
		}
		if (isContainOffset)
		{
			return DateTimeOffset.Parse(timeStr);
		}
		if (DateTime.TryParse(timeStr, out var result))
		{
			return new DateTimeOffset(result, s_jstTimeSpan);
		}
		return default(DateTimeOffset);
	}

	public static DateTimeOffset GetJSTTime()
	{
		return new DateTimeOffset(TimeZoneInfo.ConvertTimeFromUtc(CabinetManager.Clock.GetRawTime(), TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time")), s_jstTimeSpan);
	}

	public static DateTimeOffset GetNextBusinessJSTTime()
	{
		DateTimeOffset dateTimeOffset = new DateTimeOffset(TimeZoneInfo.ConvertTimeFromUtc(CabinetManager.Clock.GetRawTime(), TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time")), s_jstTimeSpan);
		if (dateTimeOffset.Hour >= 7)
		{
			dateTimeOffset = dateTimeOffset.AddDays(1.0);
		}
		DateTimeOffset dateTimeOffset2 = new DateTimeOffset(dateTimeOffset.Year, dateTimeOffset.Month, dateTimeOffset.Day, 7, 0, 0, s_jstTimeSpan);
		Debug.Log($"GetNextBusinessJSTTime() : {dateTimeOffset2}");
		return dateTimeOffset2;
	}
}
