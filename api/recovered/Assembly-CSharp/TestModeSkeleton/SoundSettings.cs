using TestModeSkeleton.CabinetDef;

namespace TestModeSkeleton;

public class SoundSettings : CabinetSettingsBase<SoundSettings.SaveData>
{
	public class SaveData : CabinetSaveDataBase
	{
		public int SoundVolume { get; set; }

		public int SoundHeadVolume { get; set; }

		public int SoundVolumeInAttractMode { get; set; }

		public override void Default()
		{
			SoundVolume = SoundOptionsDef.SoundVolumeSetting.Default;
			SoundHeadVolume = SoundOptionsDef.SoundHeadVolumeSetting.Default;
			SoundVolumeInAttractMode = SoundOptionsDef.SoundVolumeInAttractModeSetting.Default;
		}
	}

	public override string Label => "sound";
}
