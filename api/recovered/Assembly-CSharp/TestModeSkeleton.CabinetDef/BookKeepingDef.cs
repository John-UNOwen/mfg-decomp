namespace TestModeSkeleton.CabinetDef;

public class BookKeepingDef
{
	public enum BookKeeping
	{
		Off,
		On
	}

	public static ParamSetting BookKeepingSetting = ParamSetting.Create(BookKeeping.Off);
}
