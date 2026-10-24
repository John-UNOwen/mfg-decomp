using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Sytem.Utilities.Xml;
using MFG.Taikyoku.Command;
using MFG.Taikyoku.Command.Receive;
using MFG.Types;
using MFG.Types.Customize.CustomizeItem;
using UnityEngine;

namespace MFG.Taikyoku;

[Serializable]
public class HaifuData
{
	public TakuType TakuType;

	public MFG.Types.EventType EventType;

	public int PlayerIndex;

	public int NextFetchSeqNo;

	public List<ReceiveCommand> ReceivedCommands = new List<ReceiveCommand>();

	public List<PlayerInfo> PlayerInfos = new List<PlayerInfo>();

	public List<int> KyokuIndexList = new List<int>();

	public void Parse(XElement baseElem, string paifuName = "")
	{
		try
		{
			IEnumerable<XElement> enumerable = baseElem.Elements("core_io");
			int num = 0;
			int num2 = 0;
			int cellIndex = 0;
			List<CharaType> list = (from x in Enum.GetValues(typeof(CharaType)).Cast<CharaType>().ToList()
				where GameDataManager.IsCharaEnable(x)
				select x).ToList();
			foreach (XElement item in enumerable)
			{
				switch (num)
				{
				case 0:
				{
					XElement xElement2 = item.Element("in").Element("tmake");
					var (takuType, eventType) = ((GAME_MODE)xElement2.Element("play_mode").Element("gmode").ValueInt()).GetTakuTypeAndEventType();
					Debug.Log($"TaikyokuData TakuType {takuType} / EventType {eventType}");
					TakuType = takuType;
					EventType = eventType;
					num2 = xElement2.Element("pnum").ValueInt();
					KyokuIndexList = new List<int>();
					PlayerInfos = new List<PlayerInfo>();
					PlayerIndex = 0;
					for (int j = 0; j < num2; j++)
					{
						XElement xElement3 = xElement2.Element($"pinfo_{j}");
						PlayerInfo info2 = new PlayerInfo();
						if (xElement3 != null)
						{
							CharaType charaType2 = (paifuName.Equals("PaifuDemoApril") ? CharaType.Chara04 : list[UnityEngine.Random.Range(0, list.Count)]);
							list.Remove(charaType2);
							info2.PlayerIndex = j;
							SetCharaInfo(charaType2, ref info2);
						}
						PlayerInfos.Add(info2);
					}
					break;
				}
				case 1:
				{
					XElement xElement = item.Element("out").Element("tmake");
					for (int i = 0; i < MahjongUtility.GetPlayerNum(TakuType); i++)
					{
						if (i >= num2)
						{
							PlayerInfo info = new PlayerInfo();
							xElement.Element("assign_cpu").Element($"cpu_{i - num2}");
							CharaType charaType = list[UnityEngine.Random.Range(0, list.Count)];
							list.Remove(charaType);
							info.PlayerIndex = i;
							SetCharaInfo(charaType, ref info);
							PlayerInfos.Add(info);
						}
						PlayerInfos[i].Zaseki = (Kaze)xElement.Element($"zaseki_{i}").ValueInt();
					}
					ParseCellInfoParts(item, ref cellIndex);
					break;
				}
				default:
					ParseCellInfoParts(item, ref cellIndex);
					break;
				}
				num++;
			}
			NextFetchSeqNo = cellIndex;
		}
		catch (MustNeedNodeNotFoundException)
		{
		}
	}

	private void SetCharaInfo(CharaType chara, ref PlayerInfo info)
	{
		string shortName = MasterGameDataManager.Character.Properties(chara).Profile.ShortName;
		info.PlayerName = shortName;
		info.PlayerGameData.SelectChara = chara;
		info.DanLevel = DanLevelType.JANSEI1;
		info.PlayerGameData.GetCurrentChara().CustomizeItemEquips[Category.ReachBgm] = new List<ItemID> { ItemIDExtentions.s_reachBgmFavoIdx[(2, chara)] };
		info.Customize.CutinItems = CutinItemInfo.CreateDefaultItems();
		string oid = "OID_" + ((int)(chara + 1)).ToString("D2") + "AgariSR01";
		info.Customize.CutinItems.Add(new CutinItemInfo
		{
			ItemID = CutinItemMaster.GetItemID(oid),
			IsOn = true
		});
		if (chara >= CharaType.Chara12)
		{
			oid = "OID_" + ((int)(chara + 1)).ToString("D2") + "AgariSR02";
			info.Customize.CutinItems.Add(new CutinItemInfo
			{
				ItemID = CutinItemMaster.GetItemID(oid),
				IsOn = true
			});
		}
	}

	private void ParseCellInfoParts(XElement coreIO, ref int cellIndex)
	{
		XElement xElement = coreIO?.Element("out")?.Element("taikyoku")?.Element("cell_info") ?? null;
		if (xElement != null)
		{
			int num = xElement.Element("cell_sno").AttributeValueInt("count");
			for (int i = cellIndex; i < cellIndex + num; i++)
			{
				XElement cellDataElem = xElement.Element("cell_data_" + i);
				ParseCellData(cellDataElem, i);
			}
			cellIndex += num;
		}
	}

	protected void ParseCellData(XElement cellDataElem, int seqNo)
	{
		int num = cellDataElem.AttributeValueInt("kind");
		if (num == 17)
		{
			KyokuIndexList.Add(seqNo);
		}
		ReceiveCommand receiveCommand = ReceiveCommandFactory.Create((RECEIVE_COMMAND_TYPE)num, seqNo);
		if (receiveCommand != null)
		{
			receiveCommand.Parse(cellDataElem);
			ReceivedCommands.Add(receiveCommand);
		}
	}
}
