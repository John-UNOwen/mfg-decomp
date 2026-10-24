namespace MFG.GameData.EventTaku;

public class ResultInfo : IResultInfo
{
	public bool IsJandriEffective { get; set; }

	public RankBonusInfo RankBonus { get; set; } = new RankBonusInfo();


	public OtherBonusInfos OtherBonuses { get; set; } = new OtherBonusInfos();


	public virtual int AcquiredPoint => (RankBonus.TotalBonusPoint + OtherBonuses.TotalBonusPoint) * ((!IsJandriEffective) ? 1 : 2);

	public int BeforeTotalAcquiredPoint { get; set; }

	public BlowAwayChaInfo BlowAwayInfo { get; set; } = new BlowAwayChaInfo();

}
