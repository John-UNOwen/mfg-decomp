namespace TestModeSkeleton.CabinetDef;

public class ClosingTimeOptionsDef
{
	public enum ShopClose
	{
		Off,
		On
	}

	public static ParamSetting ShopCloseSetting = ParamSetting.Create(ShopClose.Off);

	public static ParamSetting ShopCloseTimeSetting = new ParamSetting(0, 1435, 1380, 5);
}
