using System;
using System.Text;
using System.Xml.Linq;
using KAMUNITY;
using TestModeSkeleton.CabinetDef;
using TestModeSkeleton.Price;

namespace TestModeSkeleton;

public class CoinSettings : CabinetSettingsBase<CoinSettings.SaveData>
{
	public class SaveData : CabinetSaveDataBase
	{
		public CoinOptionsDef.FreePlay FreePlay { get; set; }

		public CoinOptionsDef.CoinSlot CoinSlot { get; set; }

		public CoinOptionsDef.Start Start { get; set; }

		public CoinOptionsDef.Continue Continue { get; set; }

		public CoinOptionsDef.Gacha Gacha { get; set; }

		public CoinOptionsDef.Gacha5 Gacha5 { get; set; }

		public CoinOptionsDef.PremiumStart PremiumStart { get; set; }

		public CoinOptionsDef.PremiumContinue PremiumContinue { get; set; }

		public override void Default()
		{
			FreePlay = (CoinOptionsDef.FreePlay)CoinOptionsDef.FreePlaySetting.Default;
			CoinSlot = (CoinOptionsDef.CoinSlot)CoinOptionsDef.CoinSlotSetting.Default;
			Start = (CoinOptionsDef.Start)CoinOptionsDef.StartSetting.Default;
			Continue = (CoinOptionsDef.Continue)CoinOptionsDef.ContinueSetting.Default;
			Gacha = (CoinOptionsDef.Gacha)CoinOptionsDef.GachaSetting.Default;
			Gacha5 = (CoinOptionsDef.Gacha5)CoinOptionsDef.Gacha5Setting.Default;
			PremiumStart = (CoinOptionsDef.PremiumStart)CoinOptionsDef.PremiumStartSetting.Default;
			PremiumContinue = (CoinOptionsDef.PremiumContinue)CoinOptionsDef.PremiumContinueSetting.Default;
		}

		public int GetAvsFreePlay()
		{
			if (FreePlay != 0)
			{
				return 1;
			}
			return 0;
		}

		public int GetAvsCoinCredit()
		{
			switch (CoinSlot)
			{
			case CoinOptionsDef.CoinSlot.Coin01Credit1:
				return 1;
			case CoinOptionsDef.CoinSlot.Coin01Credit2:
				return 2;
			case CoinOptionsDef.CoinSlot.Coin01Credit3:
				return 3;
			case CoinOptionsDef.CoinSlot.Coin02Credit1:
				return 8;
			case CoinOptionsDef.CoinSlot.Coin02Credit3:
				return 9;
			case CoinOptionsDef.CoinSlot.Coin03Credit1:
				return 11;
			case CoinOptionsDef.CoinSlot.Coin03Credit2:
				return 12;
			case CoinOptionsDef.CoinSlot.Coin04Credit1:
				return 14;
			case CoinOptionsDef.CoinSlot.Coin04Credit3:
				return 15;
			case CoinOptionsDef.CoinSlot.Coin05Credit1:
				return 17;
			case CoinOptionsDef.CoinSlot.Coin05Credit2:
				return 18;
			case CoinOptionsDef.CoinSlot.Coin06Credit1:
				return 19;
			case CoinOptionsDef.CoinSlot.Coin07Credit1:
				return 21;
			case CoinOptionsDef.CoinSlot.Coin08Credit1:
				return 23;
			case CoinOptionsDef.CoinSlot.Coin09Credit1:
				return 25;
			case CoinOptionsDef.CoinSlot.Coin10Credit1:
				return 26;
			default:
				Logger.LogError("不正な値");
				return 1;
			}
		}

		public int GetAvsStart(bool isDefaultValue = false)
		{
			if (!isDefaultValue)
			{
				return (int)(Start + 1);
			}
			return CoinOptionsDef.StartSetting.Default + 1;
		}

		public int GetAvsContinue(bool isDefaultValue = false)
		{
			return GetAvsStart(isDefaultValue);
		}

		public int GetAvsGacha(bool isDefaultValue = false)
		{
			if (!isDefaultValue)
			{
				return (int)(Gacha + 1);
			}
			return CoinOptionsDef.GachaSetting.Default;
		}

		public int GetAvsGacha2(bool isDefaultValue = false)
		{
			if (!isDefaultValue)
			{
				return (int)(Gacha5 + 1);
			}
			return CoinOptionsDef.Gacha5Setting.Default;
		}

		public int GetAvsPremiumStart(bool isDefaultValue = false)
		{
			if (!isDefaultValue)
			{
				return (int)(PremiumStart + 1);
			}
			return CoinOptionsDef.PremiumStartSetting.Default + 1;
		}

		public int GetAvsPremiumContinue(bool isDefaultValue = false)
		{
			return GetAvsPremiumStart(isDefaultValue);
		}

		public int CalcCoinPrice(GameContentsInfo.GameContents contents, bool isUseFactorySettingValue = false)
		{
			int num = contents switch
			{
				GameContentsInfo.GameContents.Start => GetAvsStart(isUseFactorySettingValue) * PriceUtility.PricePerCoin, 
				GameContentsInfo.GameContents.Continue => GetAvsStart(isUseFactorySettingValue) * PriceUtility.PricePerCoin, 
				GameContentsInfo.GameContents.Gacha => GetAvsGacha(isUseFactorySettingValue) * PriceUtility.PricePerCoin, 
				GameContentsInfo.GameContents.Gacha5 => GetAvsGacha2(isUseFactorySettingValue) * PriceUtility.PricePerCoin, 
				GameContentsInfo.GameContents.PremiumStart => GetAvsPremiumStart(isUseFactorySettingValue) * PriceUtility.PricePerCoin, 
				GameContentsInfo.GameContents.PremiumContinue => GetAvsPremiumStart(isUseFactorySettingValue) * PriceUtility.PricePerCoin, 
				_ => throw new NotImplementedException("不正な値"), 
			};
			CoinOptionsDef.CoinSlot coinSlot = (isUseFactorySettingValue ? ((CoinOptionsDef.CoinSlot)CoinOptionsDef.CoinSlotSetting.Default) : CoinSlot);
			return num * CabinetUtility.ExtractCoinNum(coinSlot) / CabinetUtility.ExtractCreditNum(coinSlot);
		}
	}

	public override string Label => "coin";

	public CoinOptionsDef.EventMode EventMode { get; set; }

	public override void Factory()
	{
		base.Factory();
		EventMode = (CoinOptionsDef.EventMode)CoinOptionsDef.EventModeSetting.Default;
	}

	public override void SuperFactory()
	{
		base.SuperFactory();
		EventMode = (CoinOptionsDef.EventMode)CoinOptionsDef.EventModeSetting.Default;
	}

	public override void InitBeforeLoad()
	{
		CoinOptionsDef.ChangeMax(CoinOptionsDef.CoinSlot.Coin10Credit1, CoinOptionsDef.Start.Credit10, CoinOptionsDef.Continue.Credit10, CoinOptionsDef.Gacha.Credit10, CoinOptionsDef.Gacha5.Credit10);
		base.Data.Default();
	}

	public override bool Save(DateTime time = default(DateTime), SaveData coin = null)
	{
		if (base.Save(time, coin))
		{
			return NotifyToAvs();
		}
		return false;
	}

	private bool NotifyToAvs()
	{
		int num = Avs.FsOpenW("/dev/nvram/coin.xml");
		if (!Avs.IsFsOpenSuccess(num))
		{
			Logger.LogWarning("ファイルオープンエラー:coin.xml");
			return false;
		}
		byte[] array = MakeXml();
		int num2 = Avs.FsWrite(num, array, array.Length);
		Avs.FsClose(num);
		if (num2 < 0)
		{
			Logger.LogWarning("ファイル書き込みエラー:coin.xml");
			return false;
		}
		AvsEa3.Ea3PosevReport("/coin/reload_opt");
		return true;
	}

	private byte[] MakeXml()
	{
		XDocument xDocument = new XDocument(new XElement("coin", new XElement("free", new XAttribute("__type", "u8"), base.Data.GetAvsFreePlay()), new XElement("start", new XAttribute("__type", "u8"), base.Data.GetAvsStart()), new XElement("continue", new XAttribute("__type", "u8"), base.Data.GetAvsStart()), new XElement("gacha", new XAttribute("__type", "u8"), base.Data.GetAvsGacha()), new XElement("gacha2", new XAttribute("__type", "u8"), base.Data.GetAvsGacha2()), new XElement("premium_start", new XAttribute("__type", "u8"), base.Data.GetAvsPremiumStart()), new XElement("premium_continue", new XAttribute("__type", "u8"), base.Data.GetAvsPremiumStart()), new XElement("event", new XAttribute("__type", "u8"), 0), new XElement("slot1", new XElement("player", new XAttribute("__type", "u8"), 1), new XElement("coin_credit", new XAttribute("__type", "u8"), base.Data.GetAvsCoinCredit())), new XElement("pp_event", new XAttribute("__type", "u8"), 0)));
		using CabinetSerializer.EncodingStringWriter encodingStringWriter = new CabinetSerializer.EncodingStringWriter(new StringBuilder(), Encoding.UTF8);
		xDocument.Save(encodingStringWriter);
		return Encoding.UTF8.GetBytes(encodingStringWriter.ToString());
	}
}
