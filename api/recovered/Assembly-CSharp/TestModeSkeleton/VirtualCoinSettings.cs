using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using KAMUNITY;
using MFG.Sytem.Utilities;
using TestModeSkeleton.CabinetDef;
using TestModeSkeleton.Price;
using UnityEngine;

namespace TestModeSkeleton;

public class VirtualCoinSettings : CabinetSettingsBase<VirtualCoinSettings.SaveData>
{
	public class SaveData : CabinetSaveDataBase
	{
		public class Pattern
		{
			public int BasicRate;

			public VirtualCoinDef.BalanceOfCredit BalanceOfCredit;

			public VirtualCoinDef.TimeService TimeService;

			public int ServiceValue;

			public int ServiceLimit;

			public int ServiceTimeStart;

			public int ServiceTimeEnd;

			public void Default()
			{
				BasicRate = VirtualCoinDef.BasicRateSetting.Default;
				BalanceOfCredit = (VirtualCoinDef.BalanceOfCredit)VirtualCoinDef.BalanceOfCreditSetting.Default;
				TimeService = (VirtualCoinDef.TimeService)VirtualCoinDef.TimeServiceSetting.Default;
				ServiceValue = VirtualCoinDef.ServiceValueSetting.Default;
				ServiceLimit = VirtualCoinDef.ServiceLimitSetting.Default;
				ServiceTimeStart = VirtualCoinDef.ServiceTimeStartSetting.Default;
				ServiceTimeEnd = VirtualCoinDef.ServiceTimeEndSetting.Default;
			}

			public bool IsPayable()
			{
				List<int> list = new List<int>();
				CoinSettings.SaveData coinSettings = new CoinSettings.SaveData
				{
					CoinSlot = CabinetManager.Coin.Data.CoinSlot,
					Start = CabinetManager.Coin.Data.Start,
					PremiumStart = CabinetManager.Coin.Data.PremiumStart,
					Gacha = CabinetManager.Coin.Data.Gacha,
					Gacha5 = CabinetManager.Coin.Data.Gacha5
				};
				foreach (GameContentsInfo.GameContents value in Enum.GetValues(typeof(GameContentsInfo.GameContents)))
				{
					switch (value)
					{
					case GameContentsInfo.GameContents.PremiumStart:
						list.Add(CalcPaseliPrice(GameContentsInfo.GameContents.PremiumStart, coinSettings, isTimeServiceActive: false).price + CalcPaseliPrice(GameContentsInfo.GameContents.Start, coinSettings, isTimeServiceActive: false).price);
						break;
					default:
						list.Add(CalcPaseliPrice(value, coinSettings, isTimeServiceActive: false).price);
						break;
					case GameContentsInfo.GameContents.Continue:
					case GameContentsInfo.GameContents.PremiumContinue:
					case GameContentsInfo.GameContents.MusicGacha:
						break;
					}
				}
				return list.All((int p) => PriceUtility.IsPayablePrice(p));
			}

			public bool IsTimeServicePeriod(DateTime time)
			{
				if (TimeService != VirtualCoinDef.TimeService.On)
				{
					return false;
				}
				int min = (int)TimeSpan.FromHours(PatternUpdateTime).TotalMinutes;
				int max = (int)TimeSpan.FromHours(24 + PatternUpdateTime).TotalMinutes;
				int num = CabinetUtility.Loop(ServiceTimeStart, min, max);
				int num2 = CabinetUtility.Loop(ServiceTimeEnd, min, max);
				int num3 = CabinetUtility.Loop((int)(time - new DateTime(time.Year, time.Month, time.Day)).TotalMinutes, min, max);
				if (num >= num2)
				{
					num2 += (int)TimeSpan.FromHours(24.0).TotalMinutes;
				}
				if (num <= num3 && num3 < num2)
				{
					return true;
				}
				return false;
			}

			public (int price, bool isLimitOrReferCredit) CalcPaseliPrice(GameContentsInfo.GameContents contents, CoinSettings.SaveData coinSettings, bool isTimeServiceActive)
			{
				int coinPrice = coinSettings.CalcCoinPrice(contents);
				return PriceUtility.CalcPaseliPrice(contents, coinPrice, BasicRate, BalanceOfCredit, isTimeServiceActive, ServiceValue);
			}
		}

		public Pattern[] patterns = new Pattern[3]
		{
			new Pattern(),
			new Pattern(),
			new Pattern()
		};

		public VirtualCoinDef.Patterns[] schedules = new VirtualCoinDef.Patterns[CabinetUtility.EnumLength<DayOfWeek>() + 1];

		[XmlIgnore]
		public int ServiceRemain { get; set; }

		public override void Default()
		{
			patterns[0].Default();
			patterns[1].Default();
			patterns[2].Default();
			Array.Clear(schedules, VirtualCoinDef.ScheduleSetting.Default, schedules.Length);
			ServiceRemain = VirtualCoinDef.ServiceLimitSetting.Default;
		}
	}

	private static readonly int PatternUpdateTime = 5;

	private DateTime timeServiceCheckTime;

	public PriceInfo CurrentPrice = new PriceInfo();

	public override string Label => "vcoin";

	private bool IsTimeServicing { get; set; }

	public override void InitAfterLoad(DateTime time, ClockSettings.SaveData clock)
	{
		timeServiceCheckTime = time;
		base.Data.ServiceRemain = GetPatternDataInTime(time).ServiceLimit;
	}

	public override void Update(DateTime time)
	{
		if (timeServiceCheckTime.Hour < PatternUpdateTime && time.Hour >= PatternUpdateTime)
		{
			base.Data.ServiceRemain = GetPatternDataInTime(time).ServiceLimit;
		}
		timeServiceCheckTime = time;
	}

	public override bool Save(DateTime time, CoinSettings.SaveData coin)
	{
		if (base.Save(time, coin))
		{
			return RefleshPrice(time, coin, Fraction.One);
		}
		return false;
	}

	public bool RefleshPrice(DateTime time, CoinSettings.SaveData coinSettings, Fraction continuePriceRate)
	{
		SaveData.Pattern patternDataInTime = GetPatternDataInTime(time);
		bool isTimeServiceActive = base.Data.ServiceRemain > 0 && patternDataInTime.IsTimeServicePeriod(time);
		PriceInfo priceInfo = PriceUtility.CalcPaseliPrice(coinSettings, patternDataInTime.BasicRate, patternDataInTime.BalanceOfCredit, continuePriceRate, isTimeServiceActive, patternDataInTime.ServiceValue);
		return Apply(priceInfo, isTimeServiceActive);
	}

	public bool RefleshPrice(PriceInfo priceInfo, bool isTimeServiceActive)
	{
		return Apply(priceInfo, isTimeServiceActive);
	}

	private bool Apply(PriceInfo priceInfo, bool isTimeServiceActive)
	{
		IsTimeServicing = isTimeServiceActive;
		CurrentPrice = priceInfo;
		ApplyToAvs(CurrentPrice);
		return true;
	}

	private bool ApplyToAvs(PriceInfo priceInfo)
	{
		Debug.Log("ApplyToAvs called.");
		priceInfo.DebugLog();
		if (!GameUtility.IsKamunityBootEnable())
		{
			return true;
		}
		int num = Avs.FsOpenW("/dev/nvram/eacoin.xml");
		if (!Avs.IsFsOpenSuccess(num))
		{
			KAMUNITY.Logger.LogWarning("ファイルオープンエラー:eacoin.xml");
			return false;
		}
		byte[] array = MakeXml(priceInfo);
		int num2 = Avs.FsWrite(num, array, array.Length);
		Avs.FsClose(num);
		if (num2 < 0)
		{
			KAMUNITY.Logger.LogWarning("ファイル書き込みエラー:eacoin.xml");
			return false;
		}
		AvsEa3Coin.Ea3EacoinReport(AvsEa3CoinTypes.REPORT_TYPE.RELOAD_OPTION);
		return true;
	}

	private byte[] MakeXml(PriceInfo priceInfo)
	{
		int num = (GameUtility.IsKonastaOrNetcafeBehavior ? 1 : 3);
		XDocument xDocument = new XDocument(new XElement("opt", new XElement("eacoin", new XElement("enable", new XAttribute("__type", "u8"), num), new XElement("game", new XElement("start", new XAttribute("__type", "u16"), priceInfo.StartPrice), new XElement("continue", new XAttribute("__type", "u16"), priceInfo.ContinuePrice), new XElement("gacha", new XAttribute("__type", "u16"), priceInfo.GachaPrice), new XElement("gacha2", new XAttribute("__type", "u16"), priceInfo.Gacha2Price), new XElement("premium_start", new XAttribute("__type", "u16"), priceInfo.PremiumStartPrice), new XElement("premium_continue", new XAttribute("__type", "u16"), priceInfo.PremiumContinuePrice), new XElement("music_gacha", new XAttribute("__type", "u16"), priceInfo.MusicGachaPrice)), new XElement("slot1", new XElement("player", new XAttribute("__type", "u8"), 1)))));
		using CabinetSerializer.EncodingStringWriter encodingStringWriter = new CabinetSerializer.EncodingStringWriter(new StringBuilder(), Encoding.UTF8);
		xDocument.Save(encodingStringWriter);
		return Encoding.UTF8.GetBytes(encodingStringWriter.ToString());
	}

	public VirtualCoinDef.Patterns GetPatternInTime(DateTime time)
	{
		if (time.Hour < PatternUpdateTime)
		{
			time = time.AddDays(-1.0);
		}
		DayOfWeek dayOfWeek = time.DayOfWeek;
		if (CabinetManager.Clock.IsHoliday(time))
		{
			dayOfWeek = (DayOfWeek)CabinetUtility.EnumLength<DayOfWeek>();
		}
		return base.Data.schedules[(int)dayOfWeek];
	}

	public SaveData.Pattern GetPatternDataInTime(DateTime time)
	{
		return base.Data.patterns[(int)GetPatternInTime(time)];
	}

	public void ConsumeServiceRemain()
	{
		if (IsTimeServicing)
		{
			base.Data.ServiceRemain = Mathf.Max(base.Data.ServiceRemain - 1, 0);
		}
	}
}
