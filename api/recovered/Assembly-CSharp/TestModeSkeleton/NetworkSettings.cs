using TestModeSkeleton.CabinetDef;

namespace TestModeSkeleton;

public class NetworkSettings : CabinetSettingsBase<NetworkSettings.SaveData>
{
	public class SaveData : CabinetSaveDataBase
	{
		public NetworkOptionsDef.EAmusement EAmusement { get; set; }

		public NetworkOptionsDef.ShopArea ShopArea { get; set; }

		public string ShopName { get; set; }

		public override void Default()
		{
			EAmusement = (NetworkOptionsDef.EAmusement)NetworkOptionsDef.EAmusementSetting.Default;
			ShopArea = (NetworkOptionsDef.ShopArea)NetworkOptionsDef.ShopAreaSetting.Default;
			ShopName = NetworkOptionsDef.ShopNameDefault;
		}
	}

	public override string Label => "network";
}
