using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Sytem.Utilities.Xml;
using MFG.Types;
using MFG.Types.Customize.CustomizeItem;
using MFG.Types.Customize.CutinItem;
using UnityEngine;

namespace MFG.Taikyoku;

[Serializable]
public class PlayerInfo
{
	public int PlayerIndex;

	public string PlayerName;

	public DanLevelType DanLevel;

	public Kaze Zaseki;

	public int CPULevel;

	public CustomizeGameData Customize = new CustomizeGameData();

	public PlayerGameData PlayerGameData = new PlayerGameData();

	public bool IsValidCompetition;

	public bool IsValidShopCompetition;

	public PlayerType PlayerType;

	public bool IsJicha => GameDataManager.CommandClient.PlayerIndex == PlayerIndex;

	public bool IsActive => PlayerType != PlayerType.None;

	public bool IsCpu => PlayerType == PlayerType.Cpu;

	public bool IsHuman => PlayerType == PlayerType.Human;

	public string GetPlayerName()
	{
		if (PlayerGameData.IsAnonymous && !IsJicha)
		{
			return "ファイトガール";
		}
		return PlayerName;
	}

	public void ParseFromEntryPlayerNode(XElement playerDataElem)
	{
		try
		{
			PlayerType = PlayerType.Human;
		}
		catch (MustNeedNodeNotFoundException)
		{
		}
	}

	public void ParseFromMatchingEndNode(XElement playerElem)
	{
		try
		{
			switch (playerElem.AttributeValueInt("ptype"))
			{
			case 0:
			case 1:
			case 2:
			{
				PlayerType = PlayerType.Human;
				XElement xElement2 = playerElem.Element("cpu_level");
				if (xElement2 != null)
				{
					CPULevel = xElement2.ValueInt();
				}
				if (playerElem.Element("zaseki") != null)
				{
					Zaseki = (Kaze)playerElem.NeedNotElement("zaseki").ValueInt();
				}
				XElement xElement3 = playerElem.Element("client_states");
				if (xElement3 != null)
				{
					ParseClientStateElem(xElement3);
					PlayerName = PlayerGameData.PlayerName;
					DanLevel = PlayerGameData.DanLevel;
				}
				XElement xElement4 = playerElem.Element("competition");
				if (xElement4 != null)
				{
					IsValidCompetition = xElement4.NeedNotElement("avail_id").ValueInt() > 0;
				}
				XElement xElement5 = playerElem.Element("shop_competition");
				if (xElement5 != null)
				{
					IsValidShopCompetition = xElement5.NeedNotElement("avail_id").ValueInt() > 0;
				}
				break;
			}
			case 3:
			{
				PlayerType = PlayerType.Cpu;
				CPULevel = playerElem.NeedNotElement("cpu_level").ValueInt();
				DanLevel = (DanLevelType)playerElem.NeedNotElement("cpu_level").ValueInt();
				Zaseki = (Kaze)playerElem.NeedNotElement("zaseki").ValueInt();
				string oid = playerElem.NeedNotElement("cpu_name").Value.Replace("\r", "").Replace("\n", "");
				PlayerGameData.SelectChara = CharaTypeExtentions.ToCharaType(oid);
				int charaVariantID = 0;
				if (GameDataManager.IsClearProEventEnable() && PlayerGameData.SelectChara == CharaType.Chara12)
				{
					XElement xElement = playerElem.Element("cpu_variant");
					Debug.Log($"cpu_variant elem: {xElement}");
					if (xElement != null)
					{
						charaVariantID = xElement.ValueInt();
					}
					if (DebugFlag.IsActive(DebugFlag.Flag.ClearForceFake01Active))
					{
						charaVariantID = 1;
					}
					else if (DebugFlag.IsActive(DebugFlag.Flag.ClearForceFake02Active))
					{
						charaVariantID = 2;
					}
					else if (DebugFlag.IsActive(DebugFlag.Flag.ClearForceFake03Active))
					{
						charaVariantID = 3;
					}
					else if (DebugFlag.IsActive(DebugFlag.Flag.ClearForceFake04Active))
					{
						charaVariantID = 4;
					}
				}
				PlayerGameData.GetCurrentChara().CharaVariantID = charaVariantID;
				PlayerName = CreatePlayerNameForCpuChara(PlayerGameData.SelectChara, IsHuman, charaVariantID);
				GenerateCpuEquip();
				break;
			}
			}
		}
		catch
		{
			Debug.LogError("ParseFromMatchingEndNode Error");
		}
	}

	private void GenerateCpuEquip()
	{
		foreach (MFG.Types.Customize.CutinItem.ItemID gachaCharaCpuAdditionalItemID in CutinItemMaster.GetGachaCharaCpuAdditionalItemIDs())
		{
			Customize.CutinItems.Add(new CutinItemInfo
			{
				ItemID = gachaCharaCpuAdditionalItemID,
				IsOn = true
			});
		}
		CharaType selectChara = PlayerGameData.SelectChara;
		CharaGameData currentChara = PlayerGameData.GetCurrentChara();
		currentChara.MatchingBGEquip = MatchingBackgroundMaster.GetDefaultItemIDFirst(selectChara);
		bool isPro = (GameDataManager.IsClearProEventEnable() && selectChara == CharaType.Chara12) || (GameDataManager.IsIyoProEventEnable() && selectChara == CharaType.Chara13);
		currentChara.CostumeItemEquip = CostumeItemMaster.GetDefaultEquipItemID(selectChara);
		currentChara.CatchphraseItemEquip = CatchphraseItemMaster.GetCPUDefaultEquipItemID(selectChara, isPro);
		if (selectChara == CharaType.Chara13 && GameDataManager.IsIyoProEventEnable())
		{
			Customize.CutinItems.Clear();
			Customize.CutinItems.Add(new CutinItemInfo
			{
				ItemID = MFG.Types.Customize.CutinItem.ItemID.Item13NakiA01,
				IsOn = true
			});
			Customize.CutinItems.Add(new CutinItemInfo
			{
				ItemID = MFG.Types.Customize.CutinItem.ItemID.Item13ReachR01,
				IsOn = true
			});
			Customize.CutinItems.Add(new CutinItemInfo
			{
				ItemID = MFG.Types.Customize.CutinItem.ItemID.Item13AgariSR01,
				IsOn = true
			});
			Customize.CutinItems.Add(new CutinItemInfo
			{
				ItemID = MFG.Types.Customize.CutinItem.ItemID.Item13AgariSR02,
				IsOn = true
			});
		}
		else
		{
			string text = ((selectChara >= CharaType.Chara12) ? "2" : "1");
			string oid = "OID_" + ((int)(selectChara + 1)).ToString("D2") + "AgariSR0" + text;
			Customize.CutinItems.Add(new CutinItemInfo
			{
				ItemID = CutinItemMaster.GetItemID(oid),
				IsOn = true
			});
		}
		currentChara.CustomizeItemEquips[MFG.Types.Customize.CustomizeItem.Category.ReachBgm] = new List<MFG.Types.Customize.CustomizeItem.ItemID> { ItemIDExtentions.s_reachBgmFavoIdx[(0, selectChara)] };
		if (selectChara == CharaType.Chara07 && GameDataManager.IsReachSongToytoyNoticeEnable())
		{
			currentChara.CustomizeItemEquips[MFG.Types.Customize.CustomizeItem.Category.ReachBgm] = new List<MFG.Types.Customize.CustomizeItem.ItemID> { MFG.Types.Customize.CustomizeItem.ItemID.ReachBgm230 };
		}
	}

	private string CreatePlayerNameForCpuChara(CharaType charaType, bool isHuman, int charaVariantID)
	{
		if (isHuman)
		{
			return $"プレーヤー{PlayerIndex + 1}";
		}
		if (GameDataManager.IsClearProEventEnable() && charaType == CharaType.Chara12)
		{
			return MasterGameDataManager.Character.Properties(charaType).Profile.Name + (charaType.IsFakeClear(charaVariantID) ? "(影)" : "");
		}
		if (GameDataManager.IsIyoProEventEnable() && charaType == CharaType.Chara13)
		{
			return MasterGameDataManager.Character.Properties(charaType).Profile.Name ?? "";
		}
		return MasterGameDataManager.Character.Properties(charaType).Profile.ShortName ?? "";
	}

	private void ParseClientStateElem(XElement clientStateElem)
	{
		foreach (XElement item in clientStateElem.Elements("state"))
		{
			string value = item.Attribute("kind").Value;
			byte[] bytes = Convert.FromBase64String(item.Element("data").Value);
			string @string = Encoding.UTF8.GetString(bytes);
			if (!(value == "customize_item"))
			{
				if (value == "player_game")
				{
					PlayerGameData = PlayerGameData.LoadFromJson(@string);
				}
			}
			else
			{
				Customize = CustomizeGameData.LoadFromJson(@string);
			}
		}
		if (PlayerGameData.IsOdekake)
		{
			PlayerGameData.OdekakeCharaGameData.charaType = PlayerGameData.OdekakeChara;
			PlayerGameData.OdekakeCharaGameData.CostumeItemEquip = PlayerGameData.OdekakeCharaCostume;
			Customize = PlayerGameData.OdekakeCustomizeData;
		}
	}
}
