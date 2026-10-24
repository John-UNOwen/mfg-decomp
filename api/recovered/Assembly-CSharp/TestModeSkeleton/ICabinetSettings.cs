using System;

namespace TestModeSkeleton;

public interface ICabinetSettings
{
	void InitBeforeLoad();

	void InitAfterLoad(DateTime time, ClockSettings.SaveData clock);

	void FirstTimeStartup();

	void Update(DateTime time);

	void Factory();

	void SuperFactory();

	bool Save(DateTime time, CoinSettings.SaveData coin);

	bool Load();

	string GetLabel();

	int GetErrorCode();
}
