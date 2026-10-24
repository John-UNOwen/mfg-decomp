namespace MFG.GameData.EventTaku;

public interface IResultInfo
{
	bool IsJandriEffective { get; }

	RankBonusInfo RankBonus { get; }

	OtherBonusInfos OtherBonuses { get; }

	int AcquiredPoint { get; }

	int BeforeTotalAcquiredPoint { get; }
}
