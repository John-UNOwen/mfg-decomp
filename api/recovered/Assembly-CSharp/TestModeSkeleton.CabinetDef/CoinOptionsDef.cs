namespace TestModeSkeleton.CabinetDef;

public class CoinOptionsDef
{
	public enum FreePlay
	{
		Off,
		On
	}

	public enum EventMode
	{
		Off,
		On
	}

	public enum CoinSlot
	{
		Coin01Credit1,
		Coin01Credit2,
		Coin01Credit3,
		Coin02Credit1,
		Coin02Credit3,
		Coin03Credit1,
		Coin03Credit2,
		Coin04Credit1,
		Coin04Credit3,
		Coin05Credit1,
		Coin05Credit2,
		Coin06Credit1,
		Coin07Credit1,
		Coin08Credit1,
		Coin09Credit1,
		Coin10Credit1
	}

	public enum Start
	{
		Credit01,
		Credit02,
		Credit03,
		Credit04,
		Credit05,
		Credit06,
		Credit07,
		Credit08,
		Credit09,
		Credit10
	}

	public enum Continue
	{
		Credit01,
		Credit02,
		Credit03,
		Credit04,
		Credit05,
		Credit06,
		Credit07,
		Credit08,
		Credit09,
		Credit10
	}

	public enum Gacha
	{
		Credit01,
		Credit02,
		Credit03,
		Credit04,
		Credit05,
		Credit06,
		Credit07,
		Credit08,
		Credit09,
		Credit10
	}

	public enum Gacha5
	{
		Credit01,
		Credit02,
		Credit03,
		Credit04,
		Credit05,
		Credit06,
		Credit07,
		Credit08,
		Credit09,
		Credit10
	}

	public enum PremiumStart
	{
		Credit01,
		Credit02,
		Credit03,
		Credit04,
		Credit05,
		Credit06,
		Credit07,
		Credit08,
		Credit09,
		Credit10
	}

	public enum PremiumContinue
	{
		Credit01,
		Credit02,
		Credit03,
		Credit04,
		Credit05,
		Credit06,
		Credit07,
		Credit08,
		Credit09,
		Credit10
	}

	public static ParamSetting FreePlaySetting = ParamSetting.Create(FreePlay.Off);

	public static ParamSetting EventModeSetting = ParamSetting.Create(EventMode.Off);

	public static ParamSetting CoinSlotSetting = ParamSetting.Create(CoinSlot.Coin01Credit1);

	public static ParamSetting StartSetting = ParamSetting.Create(Start.Credit01);

	public static ParamSetting ContinueSetting = ParamSetting.Create(Continue.Credit01);

	public static ParamSetting GachaSetting = ParamSetting.Create(Gacha.Credit01);

	public static ParamSetting Gacha5Setting = ParamSetting.Create(Gacha.Credit05);

	public static ParamSetting PremiumStartSetting = ParamSetting.Create(Start.Credit01);

	public static ParamSetting PremiumContinueSetting = ParamSetting.Create(Continue.Credit01);

	public static void ChangeMax(CoinSlot _coinSlot, Start _start, Continue _continue, Gacha _gacha, Gacha5 _gacha2)
	{
		CoinSlotSetting.Max = (int)_coinSlot;
		StartSetting.Max = (int)_start;
		ContinueSetting.Max = (int)_continue;
		GachaSetting.Max = (int)_gacha;
		Gacha5Setting.Max = (int)_gacha2;
	}
}
