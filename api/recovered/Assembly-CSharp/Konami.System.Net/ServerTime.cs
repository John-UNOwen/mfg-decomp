using System;

namespace Konami.System.Net;

public class ServerTime
{
	private static DateTime unixEpochTime = new DateTime(1970, 1, 1, 9, 0, 0);

	private static long serverLatestTime = (long)(new DateTime(1970, 1, 1, 9, 0, 0) - unixEpochTime).TotalSeconds;

	private static DateTime clientLatestTime = DateTime.Now;

	private static long before = 0L;

	public static long Get
	{
		get
		{
			long num = serverLatestTime + (long)(DateTime.Now - clientLatestTime).TotalSeconds;
			if (before > num)
			{
				num = before;
			}
			before = num;
			return num;
		}
	}

	public static DateTime GetDateTime => unixEpochTime.AddSeconds(Get);

	public static void Update(long t)
	{
		clientLatestTime = DateTime.Now;
		serverLatestTime = t;
	}

	public static void Clear()
	{
		before = 0L;
	}
}
