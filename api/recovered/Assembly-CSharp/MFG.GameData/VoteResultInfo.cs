using MFG.Types;
using MFG.Types.Vote;

namespace MFG.GameData;

public class VoteResultInfo
{
	private Item Master => GameDataManager.Vote.CurrentInfo.Master;

	public bool IsJandriEffective { get; set; }

	public bool IsKiken { get; set; }

	public Rank MyRank { get; set; }

	public bool IsProMatching { get; set; }

	public bool IsProMatchingWinner { get; set; }

	public int RankBonus
	{
		get
		{
			if (!IsKiken)
			{
				return Master.GetRankBonus(GameDataManager.Mahjong.TakuType, MyRank);
			}
			return 0;
		}
	}

	public int ProBonus
	{
		get
		{
			if (!IsProMatching || IsKiken)
			{
				return 0;
			}
			return Master.GetProBonus(IsProMatchingWinner);
		}
	}

	public int AcquiredPoint => (RankBonus + ProBonus) * ((!IsJandriEffective) ? 1 : 2);

	public int BeforeTotalAcquiredPoint { get; set; }
}
