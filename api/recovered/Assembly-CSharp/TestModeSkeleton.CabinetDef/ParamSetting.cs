namespace TestModeSkeleton.CabinetDef;

public class ParamSetting
{
	public int Min { get; set; }

	public int Max { get; set; }

	public int Default { get; set; }

	public int Delta { get; private set; }

	public ParamSetting(int min, int max, int @default, int delta = 1)
	{
		Min = min;
		Max = max;
		Default = @default;
		Delta = delta;
	}

	public static ParamSetting Create<T>(T @default) where T : struct
	{
		return new ParamSetting(0, CabinetUtility.EnumLength<T>() - 1, (int)(object)@default);
	}
}
