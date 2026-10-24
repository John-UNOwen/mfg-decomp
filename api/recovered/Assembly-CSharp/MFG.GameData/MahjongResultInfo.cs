using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using MFG.MasterGameData;
using MFG.Sytem.Utilities.Xml;
using MFG.Types;
using MFG.Types.PlayerItem;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class MahjongResultInfo
{
	public int GameMode;

	public int TakuClass;

	public int ContinueState;

	public int ContinueFee;

	public PlayerGameData BeforePlayerGameData = new PlayerGameData();

	public PlayerGameData AfterPlayerGameData = new PlayerGameData();

	public List<PlayerResultInfo> PlayerResultInfos = new List<PlayerResultInfo>();

	public List<ExpBonusInfo> ExpBonusInfos = new List<ExpBonusInfo>();

	public int BaseExp;

	public int GroupAdjustExp;

	public List<AffinityBonusInfo> AffinityBonusInfos = new List<AffinityBonusInfo>();

	public int AffinityGrowthPoint;

	public int ActualAffinityGrowthPoint;

	public List<ItemID> TaikyokuItems = new List<ItemID>();

	public List<Gread2EventResultInfo> Gread2EventResultInfos = new List<Gread2EventResultInfo>();

	public (bool IsWin, SchoolID SchoolID) IsScrambleMatchWin()
	{
		try
		{
			Gread2EventResultInfo gread2EventResultInfo = GameDataManager.Mahjong.ResultInfo.Gread2EventResultInfos.Where((Gread2EventResultInfo x) => x.EventID == 1).First();
			if (gread2EventResultInfo == null)
			{
				return (IsWin: false, SchoolID: SchoolID.None);
			}
			return (IsWin: gread2EventResultInfo.IsWin, SchoolID: gread2EventResultInfo.SchoolID);
		}
		catch
		{
			return (IsWin: false, SchoolID: SchoolID.None);
		}
	}

	public bool IsMotherWarsWin()
	{
		try
		{
			return GameDataManager.Mahjong.ResultInfo.Gread2EventResultInfos.Where((Gread2EventResultInfo x) => x.EventID == 2).First()?.IsWin ?? false;
		}
		catch
		{
			return false;
		}
	}

	public void Parse(XElement mgResultElem)
	{
		BeforePlayerGameData = GameDataManager.Player.Clone();
		AfterPlayerGameData = GameDataManager.Player.Clone();
		try
		{
			GameMode = mgResultElem.NeedNotElement("gmode").ValueInt();
			TakuClass = mgResultElem.NeedNotElement("taku_class").ValueInt();
			ContinueState = mgResultElem.NeedNotElement("continue_state").ValueInt();
			ContinueFee = mgResultElem.NeedNotElement("continue_fee").ValueInt();
			XElement xElement = mgResultElem.Element("old");
			if (xElement != null)
			{
				BeforePlayerGameData.Parse(xElement);
			}
			XElement xElement2 = mgResultElem.Element("new");
			if (xElement2 != null)
			{
				AfterPlayerGameData.Parse(xElement2);
			}
			XElement xElement3 = mgResultElem.Element("breakdown");
			if (xElement3 != null)
			{
				XElement xElement4 = xElement3.Element("exp");
				if (xElement4 != null)
				{
					BaseExp = xElement4.Element("base_exp").ValueInt();
					GroupAdjustExp = xElement4.Element("group_adjust_exp").ValueInt();
					XElement xElement5 = xElement4.Element("bonus_list");
					if (xElement5 != null)
					{
						foreach (XElement item in xElement5.Elements("bonus"))
						{
							if (item != null)
							{
								ExpBonusInfo expBonusInfo = new ExpBonusInfo();
								expBonusInfo.Parse(item);
								ExpBonusInfos.Add(expBonusInfo);
							}
						}
					}
				}
				XElement xElement6 = xElement3.Element("affinity_point");
				if (xElement6 != null)
				{
					XElement xElement7 = xElement6.Element("bonus_list");
					if (xElement7 != null)
					{
						foreach (XElement item2 in xElement7.Elements("bonus"))
						{
							if (item2 != null)
							{
								AffinityBonusInfo affinityBonusInfo = new AffinityBonusInfo();
								affinityBonusInfo.Parse(item2);
								AffinityBonusInfos.Add(affinityBonusInfo);
							}
						}
					}
					AffinityGrowthPoint = xElement6.Element("affinity_growth_point").ValueInt();
					ActualAffinityGrowthPoint = xElement6.Element("actual_affinity_growth_point").ValueInt();
				}
			}
			XElement xElement8 = mgResultElem.Element("battle_item_use_info");
			if (xElement8 != null)
			{
				foreach (XElement item3 in xElement8.Elements("mfg_item"))
				{
					string value = item3.Element("object_id").Value;
					int num = (ActualAffinityGrowthPoint = item3.Element("count").ValueInt());
					ItemID itemID = PlayerItemMaster.ToItemID(value);
					if (num > 0 && itemID > ItemID.None)
					{
						TaikyokuItems.Add(itemID);
					}
				}
			}
			try
			{
				for (int i = 0; i < 4; i++)
				{
					XElement xElement9 = mgResultElem.Element("player_" + i);
					if (xElement9 != null)
					{
						PlayerResultInfo playerResultInfo = new PlayerResultInfo();
						playerResultInfo.Parse(xElement9);
						Debug.Log($"PlayerResultInfo Parse: index={i}, rank={playerResultInfo.Rank}, score={playerResultInfo.Score}, uma={playerResultInfo.Uma}");
						PlayerResultInfos.Add(playerResultInfo);
					}
				}
			}
			catch
			{
			}
			XElement xElement10 = mgResultElem.Element("g2result");
			if (xElement10 == null)
			{
				return;
			}
			foreach (XElement item4 in xElement10.Elements("r"))
			{
				if (item4 != null)
				{
					Gread2EventResultInfo gread2EventResultInfo = new Gread2EventResultInfo();
					gread2EventResultInfo.Parse(item4);
					Gread2EventResultInfos.Add(gread2EventResultInfo);
				}
			}
		}
		catch (MustNeedNodeNotFoundException)
		{
		}
	}
}
