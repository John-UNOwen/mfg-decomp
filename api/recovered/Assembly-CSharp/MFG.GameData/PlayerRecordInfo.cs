using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using MFG.Sytem.Utilities.Xml;
using MFG.Types;

namespace MFG.GameData;

public class PlayerRecordInfo
{
	public RecordKind RecordKind;

	public List<int> RankBreakDownList;

	public int YakumanCount;

	public List<PlayerRecordHistory> Histories;

	public Dictionary<SchoolID, int> ScrambleMatchTotalWinCount = new Dictionary<SchoolID, int>();

	public void SortHistories()
	{
		Histories = Histories.OrderByDescending((PlayerRecordHistory x) => x.PlayDate).ToList();
	}

	public void Parse(XElement infoElem)
	{
		if (Enum.TryParse<RecordKind>(infoElem.Element("kind").Value, out var result))
		{
			RecordKind = result;
		}
		List<int> list = new List<int>();
		foreach (XElement item2 in infoElem.Element("rank_list").Elements("rank_count"))
		{
			list.Add(item2.ValueInt());
		}
		RankBreakDownList = list;
		YakumanCount = infoElem.Element("yakuman_num").ValueInt();
		XElement xElement = infoElem.Element("g2_scramble");
		if (xElement != null)
		{
			foreach (XElement item3 in xElement.Elements("history"))
			{
				SchoolID key = Enum.Parse<SchoolID>(item3.Element("belong").Value);
				int value = item3.Element("win_nr").ValueInt();
				ScrambleMatchTotalWinCount.TryAdd(key, value);
			}
		}
		XElement xElement2 = infoElem.Element("histories");
		List<PlayerRecordHistory> list2 = new List<PlayerRecordHistory>();
		if (xElement2.HasElements)
		{
			foreach (XElement item4 in xElement2.Elements("history"))
			{
				PlayerRecordHistory item = new PlayerRecordHistory
				{
					HistoryRank = (Rank)item4.Element("rank").ValueInt(),
					Score = item4.Element("score").ValueInt64(),
					IsYakuman = item4.Element("is_yakuman").ValueBool(),
					IsButtobi = item4.Element("is_buttobi").ValueBool(),
					PlayDate = item4.Element("date").Value
				};
				list2.Add(item);
			}
		}
		Histories = list2;
		SortHistories();
	}
}
