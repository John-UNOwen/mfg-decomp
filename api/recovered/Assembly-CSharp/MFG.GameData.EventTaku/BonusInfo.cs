namespace MFG.GameData.EventTaku;

public class BonusInfo
{
	public string BonusTypeKey;

	public int BonusCount;

	public decimal PointRatioByCount;

	public int TotalBonusPoint => BonusCount * decimal.ToInt32(PointRatioByCount);
}
