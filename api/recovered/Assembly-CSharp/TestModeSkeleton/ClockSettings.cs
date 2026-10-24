using System;
using KAMUNITY;
using TestModeSkeleton.CabinetDef;

namespace TestModeSkeleton;

public class ClockSettings : CabinetSettingsBase<ClockSettings.SaveData>
{
	public class SaveData : CabinetSaveDataBase
	{
		public bool Initialized { get; set; }

		public int OffsetTime { get; set; }

		public override void Default()
		{
			Initialized = false;
			OffsetTime = ClockDef.OffsetTimeSetting.Default;
		}
	}

	public override string Label => "clock";

	public override void InitBeforeLoad()
	{
		ClockDef.ChangeDefault((CabinetUtility.GetDestCode() == 'J') ? 32400 : 0);
		base.Data.Default();
	}

	public override void Factory()
	{
	}

	public override bool Save(DateTime time = default(DateTime), CoinSettings.SaveData coin = null)
	{
		AvsBookkeeping.BookkeepingClockIsSet(base.Data.Initialized ? 1 : 0);
		AvsBookkeeping.BookkeepingClockSetDiff((uint)base.Data.OffsetTime);
		return base.Save(time, coin);
	}

	public DateTime GetRawTime()
	{
		if (!GameUtility.IsKamunityBootEnable())
		{
			return DateTime.UtcNow;
		}
		return Utility.Epoch2DateTime((long)Avs.TimeGet(AvsTypes.AVS_CLOCK.SERVER));
	}

	public DateTime GetTime()
	{
		if (!GameUtility.IsKamunityBootEnable())
		{
			return TimeZoneInfo.ConvertTime(DateTimeOffset.Now, TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time")).DateTime;
		}
		return GetRawTime().AddSeconds(base.Data.OffsetTime);
	}

	public bool IsHoliday(DateTime time)
	{
		if (!GameUtility.IsKamunityBootEnable())
		{
			return false;
		}
		return AvsEa3.Ea3TimeIsHoliday(ToEpochTime(time)) != 0;
	}

	public ulong ToEpochTime(DateTime from)
	{
		DateTimeOffset dateTimeOffset = new DateTimeOffset(from);
		dateTimeOffset.AddSeconds(base.Data.OffsetTime);
		return (ulong)dateTimeOffset.ToUnixTimeSeconds();
	}
}
