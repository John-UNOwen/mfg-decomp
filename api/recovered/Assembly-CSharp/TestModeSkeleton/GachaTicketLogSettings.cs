using System;
using System.Collections.Generic;

namespace TestModeSkeleton;

public class GachaTicketLogSettings : CabinetSettingsBase<GachaTicketLogSettings.SaveData>
{
	public class SaveData : CabinetSaveDataBase
	{
		public class LogData
		{
			public DateTime Date;

			public int PurchaseCount;

			public int ConsumeCount;

			public string RefID;

			public string CardNumber;
		}

		public List<LogData> Logs = new List<LogData>();

		public override void Default()
		{
			Logs.Clear();
		}
	}

	public override string Label => "gachalog";

	public void RecordPurchase(int purchaseTicketCount)
	{
		AddLog(purchaseTicketCount, 0);
	}

	public void RecordConsume(int consumeTicketCount)
	{
		AddLog(0, consumeTicketCount);
	}

	private void AddLog(int purchaseCount, int consumeCount)
	{
		if (GameUtility.IsKamunityBootEnable() && !GameDataManager.Account.IsGuestUser)
		{
			base.Data.Logs.Add(new SaveData.LogData
			{
				Date = CabinetManager.Clock.GetTime(),
				PurchaseCount = purchaseCount,
				ConsumeCount = consumeCount,
				RefID = GameDataManager.Account.RefID,
				CardNumber = GameDataManager.Account.CardNumber
			});
			Save();
		}
	}

	public override void InitAfterLoad(DateTime time, ClockSettings.SaveData clock)
	{
		DateTime thresholdDatetime = time - TimeSpan.FromDays(30.0);
		base.Data.Logs.RemoveAll((SaveData.LogData log) => log.Date <= thresholdDatetime);
		Save();
	}

	public override void Factory()
	{
	}
}
