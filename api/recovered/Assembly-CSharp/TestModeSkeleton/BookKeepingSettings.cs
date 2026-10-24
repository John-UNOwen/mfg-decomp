using System;
using System.Xml.Serialization;
using KAMUNITY;
using TestModeSkeleton.CabinetDef;

namespace TestModeSkeleton;

public class BookKeepingSettings : CabinetSettingsBase<BookKeepingSettings.SaveData>
{
	public enum Variable
	{
		GameStarting = 0,
		Continuation = 1,
		Gacha = 2,
		Gacha1 = 3,
		Gacha5 = 4,
		MusicGacha = 5,
		SumBits = 3
	}

	public class SaveData : CabinetSaveDataBase
	{
		[XmlIgnore]
		public BookKeepingDef.BookKeeping BookKeeping
		{
			get
			{
				if (AvsBookkeeping.BookkeepingIsRecordEnable() != 0)
				{
					return BookKeepingDef.BookKeeping.On;
				}
				return BookKeepingDef.BookKeeping.Off;
			}
			set
			{
				AvsBookkeeping.BookkeepingSetRecordEnable((value == BookKeepingDef.BookKeeping.On) ? 1 : 0);
			}
		}

		public override void Default()
		{
		}
	}

	private ulong? gameStartTime;

	public override string Label => "book";

	public override void InitAfterLoad(DateTime time, ClockSettings.SaveData clock)
	{
		AvsBookkeeping.BookkeepingClockIsSet(clock.Initialized ? 1 : 0);
		AvsBookkeeping.BookkeepingClockSetDiff((uint)clock.OffsetTime);
	}

	public override void FirstTimeStartup()
	{
		AvsBookkeeping.BookkeepingStatusSetFactory();
		AvsBookkeeping.BookkeepingDataSetFactory();
	}

	public override void Factory()
	{
	}

	public override void SuperFactory()
	{
		AvsBookkeeping.BookkeepingStatusSetFactory();
		AvsBookkeeping.BookkeepingDataSetFactory();
	}

	public override bool Save(DateTime time = default(DateTime), CoinSettings.SaveData coin = null)
	{
		AvsBookkeeping.BookkeepingFlush();
		return base.Save();
	}

	public void StartGamePlayTime()
	{
		if (GameUtility.IsKamunityBootEnable())
		{
			if (gameStartTime.HasValue)
			{
				Logger.Log("EndGamePlayTime()がコールされなかったので、プレー時間の計測をやり直します。");
			}
			gameStartTime = Avs.TimeGet(AvsTypes.AVS_CLOCK.SERVER);
		}
	}

	public void DisposeGamePlayTime()
	{
		gameStartTime = null;
	}

	public void EndGamePlayTime()
	{
		if (GameUtility.IsKamunityBootEnable())
		{
			if (gameStartTime.HasValue)
			{
				AvsBookkeeping.BookkeepingSetPlayTime((uint)(Avs.TimeGet(AvsTypes.AVS_CLOCK.SERVER) - gameStartTime.Value));
				gameStartTime = null;
			}
			else
			{
				Logger.LogError("StartGamePlayTime()をコールする必要があります。");
			}
		}
	}

	public void AddPlayCount(Variable variable, int count = 1)
	{
		if (GameUtility.IsKamunityBootEnable())
		{
			uint playCount = GetPlayCount(variable);
			AvsBookkeeping.BookkeepingSetVariable((uint)variable, playCount + (uint)count);
		}
	}

	public uint GetPlayCount(Variable variable)
	{
		return AvsBookkeeping.BookkeepingGetVariable((uint)variable);
	}
}
