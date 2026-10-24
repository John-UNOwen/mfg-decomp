namespace TestModeSkeleton.CabinetDef;

public class GameOptionsDef
{
	public enum ContinueOnOff
	{
		Off,
		On
	}

	public enum BreakTime
	{
		Off,
		On
	}

	public enum LEDWingOnOff
	{
		Off,
		On
	}

	public enum LEDCtrlOnOff
	{
		Off,
		On
	}

	public enum LEDBttnOnOff
	{
		Off,
		On
	}

	public enum LEDHeadOnOff
	{
		Off,
		On
	}

	public enum LEDSideOnOff
	{
		Off,
		On
	}

	public enum FreePlayCampaign
	{
		Off,
		On
	}

	public static ParamSetting ShopCloseSetting = ParamSetting.Create(ClosingTimeOptionsDef.ShopClose.Off);

	public static ParamSetting ShopCloseTimeSetting = new ParamSetting(0, 1435, 1380, 5);

	public static ParamSetting ContinueSetting = ParamSetting.Create(ContinueOnOff.On);

	public static ParamSetting GachaBuyNumSetting = new ParamSetting(3, 5, 5);

	public static ParamSetting CabinetIdSetting = new ParamSetting(1, 4, 4);

	public static ParamSetting BreakTimeSetting = ParamSetting.Create(BreakTime.On);

	public static ParamSetting BreakTimeLengthSetting = new ParamSetting(1, 15, 5);

	public static ParamSetting LEDWingSetting = ParamSetting.Create(LEDWingOnOff.On);

	public static ParamSetting LEDCtrlSetting = ParamSetting.Create(LEDCtrlOnOff.On);

	public static ParamSetting LEDBttnSetting = ParamSetting.Create(LEDBttnOnOff.On);

	public static ParamSetting LEDHeadSetting = ParamSetting.Create(LEDHeadOnOff.On);

	public static ParamSetting LEDSideSetting = ParamSetting.Create(LEDSideOnOff.On);

	public static ParamSetting FreePlayCampaignSetting = ParamSetting.Create(FreePlayCampaign.On);
}
