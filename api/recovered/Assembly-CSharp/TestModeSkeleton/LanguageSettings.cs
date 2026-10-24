using TestModeSkeleton.CabinetDef;

namespace TestModeSkeleton;

public class LanguageSettings : CabinetSettingsBase<LanguageSettings.SaveData>
{
	public class SaveData : CabinetSaveDataBase
	{
		public LanguageOptionsDef.GameMode GameModeLanguage { get; set; }

		public LanguageOptionsDef.TestMode TestModeLanguage
		{
			get
			{
				return (LanguageOptionsDef.TestMode)LanguageOptionsDef.TestModeSetting.Default;
			}
			set
			{
			}
		}

		public override void Default()
		{
			GameModeLanguage = (LanguageOptionsDef.GameMode)LanguageOptionsDef.GameModeSetting.Default;
			TestModeLanguage = (LanguageOptionsDef.TestMode)LanguageOptionsDef.TestModeSetting.Default;
		}
	}

	public override string Label => "lang";

	public override void InitBeforeLoad()
	{
		if (CabinetUtility.GetDestCode() == 'J')
		{
			LanguageOptionsDef.ChangeDefault(LanguageOptionsDef.GameMode.Japanese, LanguageOptionsDef.TestMode.Japanese);
		}
		else
		{
			LanguageOptionsDef.ChangeDefault(LanguageOptionsDef.GameMode.English, LanguageOptionsDef.TestMode.English);
		}
		base.Data.Default();
	}
}
