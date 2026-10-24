namespace TestModeSkeleton.CabinetDef;

public class ClockDef
{
	public static ParamSetting OffsetTimeSetting = new ParamSetting(-43200, 43200, 32400);

	public static void ChangeDefault(int offsetTime)
	{
		OffsetTimeSetting.Default = offsetTime;
	}
}
