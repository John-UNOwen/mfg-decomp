namespace TestModeSkeleton.CabinetDef;

public class VirtualCoinDef
{
	public enum Patterns
	{
		P1,
		P2,
		P3
	}

	public enum BalanceOfCredit
	{
		Normal,
		Imbalance,
		MoreImbalance
	}

	public enum TimeService
	{
		Off,
		On
	}

	public static ParamSetting BasicRateSetting = new ParamSetting(400, 5000, 1000);

	public static ParamSetting BalanceOfCreditSetting = ParamSetting.Create(BalanceOfCredit.Normal);

	public static ParamSetting TimeServiceSetting = ParamSetting.Create(TimeService.Off);

	public static ParamSetting ServiceValueSetting = new ParamSetting(10, 100, 10);

	public static ParamSetting ServiceLimitSetting = new ParamSetting(1, 100, 10);

	public static ParamSetting ServiceTimeStartSetting = new ParamSetting(0, 1425, 420, 15);

	public static ParamSetting ServiceTimeEndSetting = new ParamSetting(0, 1425, 660, 15);

	public static ParamSetting CopyFromPatternSetting = new ParamSetting(0, CabinetUtility.EnumLength<Patterns>() - 2, 0);

	public static ParamSetting ScheduleSetting = ParamSetting.Create(Patterns.P1);
}
