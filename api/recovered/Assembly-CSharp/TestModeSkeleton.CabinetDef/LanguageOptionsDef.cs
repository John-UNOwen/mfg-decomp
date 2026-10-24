namespace TestModeSkeleton.CabinetDef;

public class LanguageOptionsDef
{
	public enum GameMode
	{
		Japanese,
		English,
		Korean,
		ChineseTraditional,
		ChineseSimplified
	}

	public enum TestMode
	{
		Japanese,
		English
	}

	public static ParamSetting GameModeSetting = ParamSetting.Create(GameMode.Japanese);

	public static ParamSetting TestModeSetting = ParamSetting.Create(TestMode.Japanese);

	public static void ChangeDefault(GameMode gameMode, TestMode testMode)
	{
		GameModeSetting.Default = (int)gameMode;
		TestModeSetting.Default = (int)testMode;
	}
}
