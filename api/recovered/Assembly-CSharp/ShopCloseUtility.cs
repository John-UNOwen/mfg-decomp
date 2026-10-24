using System;
using System.Text;
using KAMUNITY;
using TestModeSkeleton;

public class ShopCloseUtility
{
	public static class Konasta
	{
		public const int SecPerMinute = 60;

		public const int RemainSecToNotice = 3600;

		public const int RemainSecToEnforce = 1800;

		public static GameSettings.ShopClosePhase GetShopClosePhase()
		{
			if (AvsEa3.Ea3GetStatus(out var _, out var _) == AvsTypes.EA3_STATUS.MAINTE)
			{
				return GameSettings.ShopClosePhase.Final;
			}
			var (flag, num) = QueryServerMaintenanceSchedule();
			if (flag)
			{
				if (num < 1800)
				{
					if (num <= 0)
					{
						return GameSettings.ShopClosePhase.Final;
					}
					return GameSettings.ShopClosePhase.Enforce;
				}
				if (num < 3600)
				{
					return GameSettings.ShopClosePhase.Notice;
				}
				return GameSettings.ShopClosePhase.Normal;
			}
			return GameSettings.ShopClosePhase.Normal;
		}
	}

	public static DateTime GetTime
	{
		get
		{
			if (!GameUtility.IsKamunityBootEnable())
			{
				return DateTime.UtcNow;
			}
			return CabinetManager.Clock.GetTime();
		}
	}

	public static DayOfWeek GetDayOfWeek(DateTime now)
	{
		if (now.Hour >= 7)
		{
			return now.DayOfWeek;
		}
		return (DayOfWeek)((int)(now.DayOfWeek + 6) % 7);
	}

	public static DayOfWeek GetDayOfWeek()
	{
		if (GetTime.Hour >= 7)
		{
			return GetTime.DayOfWeek;
		}
		return (DayOfWeek)((int)(GetTime.DayOfWeek + 6) % 7);
	}

	public static GameSettings.ShopClosePhase GetShopClosePhase()
	{
		if (DebugFlag.IsActive(DebugFlag.Flag.ForceShopClose))
		{
			return GameSettings.ShopClosePhase.Notice;
		}
		if (GameUtility.IsKonastaOrNetcafeBehavior)
		{
			return Konasta.GetShopClosePhase();
		}
		return CabinetManager.Game.GetShopClosePhase(GetTime);
	}

	public static (bool isMaintenanceScheduled, int secondsToStart) QueryServerMaintenanceSchedule()
	{
		if (!GameUtility.IsKamunityBootEnable())
		{
			return (isMaintenanceScheduled: false, secondsToStart: int.MaxValue);
		}
		if (DebugFlag.IsActive(DebugFlag.Flag.ForceShopClose))
		{
			return (isMaintenanceScheduled: true, secondsToStart: 3000);
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (AvsEa3.Ea3MainteSearch(0, out var start, out var _, stringBuilder, stringBuilder.Capacity, out var _) == 1)
		{
			return (isMaintenanceScheduled: true, secondsToStart: start);
		}
		return (isMaintenanceScheduled: false, secondsToStart: int.MaxValue);
	}

	public static bool IsEntryAcceptable()
	{
		if (DebugFlag.IsActive(DebugFlag.Flag.ForceShopClose))
		{
			return false;
		}
		return GetShopClosePhase() < GameSettings.ShopClosePhase.Enforce;
	}

	public static bool IsAfterNoticePhase()
	{
		return GetShopClosePhase() >= GameSettings.ShopClosePhase.Notice;
	}
}
