using UnityEngine;

namespace TestModeSkeleton;

public class CalibrationSettings : CabinetSettingsBase<CalibrationSettings.SaveData>
{
	public enum CalibrationPoint
	{
		UpperLeft,
		LowerRight,
		Max
	}

	public class SaveData : CabinetSaveDataBase
	{
		public bool IsCalibrated { get; set; }

		public Vector2 UpperLeft { get; set; }

		public Vector2 LowerRight { get; set; }

		public override void Default()
		{
			IsCalibrated = false;
			UpperLeft.Set(0f, 0f);
			LowerRight.Set(0f, 0f);
		}
	}

	public override string Label => "calibration";

	public override void Factory()
	{
	}
}
