using System;
using System.Collections.Generic;
using System.Linq;
using TestModeSkeleton.CabinetDef;
using UnityEngine;

namespace TestModeSkeleton;

public class GameSettings : CabinetSettingsBase<GameSettings.SaveData>
{
	public enum ShopClosePhase
	{
		Normal,
		Notice,
		Enforce,
		Final
	}

	public class SaveData : CabinetSaveDataBase
	{
		public GameOptionsDef.BreakTime BreakTime;

		public int BreakTimeLength;

		public int SaveDataVersion = 2;

		public ClosingTimeOptionsDef.ShopClose ShopClose { get; set; }

		public int ShopCloseTime { get; set; }

		public List<int> ShopCloseDayOfWeek { get; set; }

		public List<int> ShopCloseTimeDayOfWeek { get; set; }

		public GameOptionsDef.ContinueOnOff ContinueMenu { get; set; }

		public int GachaBuyNum { get; set; }

		public int CabinetId { get; set; }

		public GameOptionsDef.LEDWingOnOff LEDWingOnOff { get; set; }

		public GameOptionsDef.LEDCtrlOnOff LEDCtrlOnOff { get; set; }

		public GameOptionsDef.LEDBttnOnOff LEDBttnOnOff { get; set; }

		public GameOptionsDef.LEDHeadOnOff LEDHeadOnOff { get; set; }

		public GameOptionsDef.LEDSideOnOff LEDSideOnOff { get; set; }

		public GameOptionsDef.FreePlayCampaign FreePlayCampaign { get; set; }

		public override void Default()
		{
			ShopClose = (ClosingTimeOptionsDef.ShopClose)GameOptionsDef.ShopCloseSetting.Default;
			ShopCloseTime = GameOptionsDef.ShopCloseTimeSetting.Default;
			ShopCloseDayOfWeek = Enumerable.Repeat(ClosingTimeOptionsDef.ShopCloseSetting.Default, 7).ToList();
			ShopCloseTimeDayOfWeek = Enumerable.Repeat(ClosingTimeOptionsDef.ShopCloseTimeSetting.Default, 7).ToList();
			ContinueMenu = (GameOptionsDef.ContinueOnOff)GameOptionsDef.ContinueSetting.Default;
			GachaBuyNum = GameOptionsDef.GachaBuyNumSetting.Default;
			CabinetId = GameOptionsDef.CabinetIdSetting.Default;
			BreakTime = (GameOptionsDef.BreakTime)GameOptionsDef.BreakTimeSetting.Default;
			BreakTimeLength = GameOptionsDef.BreakTimeLengthSetting.Default;
			LEDWingOnOff = (GameOptionsDef.LEDWingOnOff)GameOptionsDef.LEDWingSetting.Default;
			LEDCtrlOnOff = (GameOptionsDef.LEDCtrlOnOff)GameOptionsDef.LEDCtrlSetting.Default;
			LEDBttnOnOff = (GameOptionsDef.LEDBttnOnOff)GameOptionsDef.LEDBttnSetting.Default;
			LEDHeadOnOff = (GameOptionsDef.LEDHeadOnOff)GameOptionsDef.LEDHeadSetting.Default;
			LEDSideOnOff = (GameOptionsDef.LEDSideOnOff)GameOptionsDef.LEDSideSetting.Default;
			FreePlayCampaign = (GameOptionsDef.FreePlayCampaign)GameOptionsDef.FreePlayCampaignSetting.Default;
			SaveDataVersion = 2;
		}
	}

	private ulong shopCloseEpochTime;

	private ShopClosePhase last_phase_;

	private bool license_passed_;

	private const int CurrentSaveDataVersion = 2;

	public override string Label => "game";

	public override void InitAfterLoad(DateTime time, ClockSettings.SaveData clock)
	{
		if (base.Data.SaveDataVersion == 0)
		{
			base.Data.BreakTime = (GameOptionsDef.BreakTime)GameOptionsDef.BreakTimeSetting.Default;
			base.Data.BreakTimeLength = GameOptionsDef.BreakTimeLengthSetting.Default;
			Debug.Log("GameSettings.InitAfterLoad : データバージョン更新 0→1");
			base.Data.SaveDataVersion = 1;
		}
		if (base.Data.SaveDataVersion == 1)
		{
			base.Data.ShopCloseDayOfWeek = Enumerable.Repeat((int)base.Data.ShopClose, 7).ToList();
			base.Data.ShopCloseTimeDayOfWeek = Enumerable.Repeat((base.Data.ShopClose == ClosingTimeOptionsDef.ShopClose.On) ? base.Data.ShopCloseTime : ClosingTimeOptionsDef.ShopCloseTimeSetting.Default, 7).ToList();
			Debug.Log("GameSettings.InitAfterLoad : データバージョン更新 1→2");
			base.Data.SaveDataVersion = 2;
		}
		Debug.Log("GameSettings.InitAfterLoad ShopCloseDayOfWeek Before " + string.Join(",", base.Data.ShopCloseDayOfWeek));
		if (base.Data.ShopCloseDayOfWeek.Count > 7)
		{
			base.Data.ShopCloseDayOfWeek = base.Data.ShopCloseDayOfWeek.TakeLast(7).ToList();
		}
		if (base.Data.ShopCloseTimeDayOfWeek.Count > 7)
		{
			base.Data.ShopCloseTimeDayOfWeek = base.Data.ShopCloseTimeDayOfWeek.TakeLast(7).ToList();
		}
		Debug.Log("GameSettings.InitAfterLoad ShopCloseDayOfWeek After " + string.Join(",", base.Data.ShopCloseDayOfWeek));
		time = DateTimeDebug(time);
		ModifyShopCloseTime(time);
	}

	public override bool Save(DateTime time, CoinSettings.SaveData coin)
	{
		time = DateTimeDebug(time);
		ModifyShopCloseTime(time);
		base.Data.SaveDataVersion = 2;
		return base.Save(time, coin);
	}

	public void LicensePassed()
	{
		license_passed_ = true;
	}

	public bool IsLicensePassed()
	{
		return license_passed_;
	}

	public ShopClosePhase GetShopClosePhase(DateTime now)
	{
		now = DateTimeDebug(now);
		if (base.Data.ShopCloseDayOfWeek[(int)ShopCloseUtility.GetDayOfWeek(now)] == 0)
		{
			return ShopClosePhase.Normal;
		}
		long num = (long)TimeSpan.FromMinutes(30.0).TotalSeconds;
		long num2 = (long)TimeSpan.FromMinutes(10.0).TotalSeconds;
		long num3 = (long)(shopCloseEpochTime - CabinetManager.Clock.ToEpochTime(now));
		ShopClosePhase shopClosePhase = ShopClosePhase.Normal;
		shopClosePhase = ((num3 <= num) ? ((num3 > num2) ? ShopClosePhase.Notice : ((num3 <= 0) ? ShopClosePhase.Final : ShopClosePhase.Enforce)) : ShopClosePhase.Normal);
		if (last_phase_ != ShopClosePhase.Final)
		{
			last_phase_ = shopClosePhase;
		}
		return last_phase_;
	}

	public void ModifyShopCloseTime(DateTime now)
	{
		int dayOfWeek = (int)ShopCloseUtility.GetDayOfWeek(now);
		shopCloseEpochTime = 9223372036854775807uL;
		for (int i = 0; i <= 7; i++)
		{
			if (base.Data.ShopCloseDayOfWeek[(dayOfWeek + i) % 7] == 1)
			{
				TimeSpan timeSpan = TimeSpan.FromMinutes(base.Data.ShopCloseTimeDayOfWeek[(dayOfWeek + i) % 7]).Add(TimeSpan.FromDays(i)) - new TimeSpan(now.Hour, now.Minute, now.Second);
				if (timeSpan.TotalSeconds >= 0.0)
				{
					shopCloseEpochTime = (ulong)((double)CabinetManager.Clock.ToEpochTime(now) + timeSpan.TotalSeconds);
					Debug.Log($"todayDayOfWeek {(DayOfWeek)dayOfWeek} / nextCloseDayOfWeek {(DayOfWeek)((dayOfWeek + i) % 7)} / remain {timeSpan}");
					break;
				}
			}
		}
	}

	private DateTime DateTimeDebug(DateTime now)
	{
		if (DebugFlag.IsActive(DebugFlag.Flag.DebugJST))
		{
			now = new DateTime(now.Year, now.Month, now.Day, 23, 45, 0);
		}
		else if (DebugFlag.IsActive(DebugFlag.Flag.DebugJST2))
		{
			now = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
			now = now.AddDays(1.0);
		}
		else if (DebugFlag.IsActive(DebugFlag.Flag.DebugJST3))
		{
			now = new DateTime(now.Year, now.Month, now.Day, 0, 15, 0);
			now = now.AddDays(1.0);
		}
		else if (DebugFlag.IsActive(DebugFlag.Flag.DebugJST4))
		{
			now = new DateTime(now.Year, now.Month, now.Day, 0, 30, 0);
			now = now.AddDays(1.0);
		}
		else if (DebugFlag.IsActive(DebugFlag.Flag.DebugJST5))
		{
			now = new DateTime(now.Year, now.Month, now.Day, 1, 15, 0);
			now = now.AddDays(1.0);
		}
		else if (DebugFlag.IsActive(DebugFlag.Flag.DebugJST6))
		{
			now = new DateTime(now.Year, now.Month, now.Day, 1, 30, 0);
			now = now.AddDays(1.0);
		}
		else if (DebugFlag.IsActive(DebugFlag.Flag.DebugJST7))
		{
			now = new DateTime(now.Year, now.Month, now.Day, 1, 45, 0);
			now = now.AddDays(1.0);
		}
		else if (DebugFlag.IsActive(DebugFlag.Flag.DebugJST8))
		{
			now = new DateTime(now.Year, now.Month, now.Day, 2, 0, 0);
			now = now.AddDays(1.0);
		}
		return now;
	}
}
