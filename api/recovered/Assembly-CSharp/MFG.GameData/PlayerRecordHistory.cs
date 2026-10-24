using MFG.Types;

namespace MFG.GameData;

public class PlayerRecordHistory
{
	public Rank HistoryRank;

	public long Score;

	public bool IsYakuman;

	public bool IsButtobi;

	public string PlayDate;

	public RecordMark GetMark(RecordKind kind)
	{
		if (IsYakuman)
		{
			return RecordMark.Yakuman;
		}
		if (HistoryRank == Rank.Rank1 && ((kind == RecordKind.Yonma && Score >= 40000) || (kind == RecordKind.Sanma && Score >= 50000)))
		{
			return RecordMark.BigWin;
		}
		if (IsButtobi)
		{
			return RecordMark.Buttobi;
		}
		return RecordMark.None;
	}

	public string GetRecordMarkPath(RecordKind kind)
	{
		return GetMark(kind) switch
		{
			RecordMark.BigWin => "Assets/MFG/Scenes/CommonSystem/Textures/RecordIconTop.png", 
			RecordMark.Yakuman => "Assets/MFG/Scenes/CommonSystem/Textures/RecordIconYakuman.png", 
			RecordMark.Buttobi => "Assets/MFG/Scenes/CommonSystem/Textures/RecordIconZero.png", 
			_ => "", 
		};
	}
}
