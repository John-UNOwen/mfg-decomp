using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.MasterGameData;
using MFG.Sytem.Utilities.Xml;
using MFG.Types;
using MFG.Types.AD;
using MFG.Types.Customize.CutinItem;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class GachaInfoGameData : GameDataBase
{
	private const int CurrentDataVersion = 1;

	public int DataVersion = 1;

	public List<GachaSeriesServerInfo> GachaSeriesInfosFromServer = new List<GachaSeriesServerInfo>();

	public List<GachaSeriesServerInfo> ModeGachaInfo(GachaSeriesType seriesType)
	{
		GachaSeriesType gachaSeriesType = seriesType;
		if ((uint)(gachaSeriesType - 4) <= 1u)
		{
			return GachaSeriesInfosFromServer.Where((GachaSeriesServerInfo x) => x.SeriesType == seriesType).ToList();
		}
		return GachaSeriesInfosFromServer.Where((GachaSeriesServerInfo x) => x.SeriesType != GachaSeriesType.Unlock && x.SeriesType != GachaSeriesType.Music).ToList();
	}

	public bool IsAnyActive()
	{
		return GachaSeriesInfosFromServer?.Exists((GachaSeriesServerInfo x) => x.IsDisplay()) ?? false;
	}

	public List<AdTipsID> GetGachaAdTipsIDs()
	{
		List<AdTipsID> list = new List<AdTipsID>();
		List<GachaSeriesServerInfo> list2 = GachaSeriesInfosFromServer.FindAll((GachaSeriesServerInfo x) => x.IsActive);
		if (list2 != null && list2.Count > 0)
		{
			foreach (GachaSeriesServerInfo item in list2)
			{
				list.AddRange(item.ExtraMaster.AdTipsIDs);
			}
		}
		return list;
	}

	public void Parse(XElement elem)
	{
		foreach (XElement item in elem.Elements("info"))
		{
			try
			{
				int seriesID = item.Element("id").ValueInt();
				string value = item.Element("label").Value;
				int ticketNum = item.Element("ticket_nr").ValueInt();
				bool isActive = item.Element("now_active").ValueBool();
				bool isForceActive = item.Element("force_active").ValueBool();
				GachaSeriesType gachaSeriesType = GachaSeriesType.None;
				if (Enum.TryParse<GachaSeriesType>(item.Element("series_type").Value, out var result))
				{
					gachaSeriesType = result;
				}
				List<ItemID> list = new List<ItemID>();
				List<string> list2 = new List<string>();
				foreach (XElement item2 in item.Element("items").Elements("item"))
				{
					string value2 = item2.Value;
					if (gachaSeriesType == GachaSeriesType.Music)
					{
						list2.Add(value2);
						continue;
					}
					ItemID itemID = CutinItemMaster.GetItemID(value2);
					if (itemID != 0)
					{
						list.Add(itemID);
					}
				}
				List<CharaType> list3 = new List<CharaType>();
				XElement xElement = item.Element("pickup_charas");
				if (xElement.HasElements)
				{
					foreach (XElement item3 in xElement.Elements("chara"))
					{
						if (Enum.TryParse<CharaType>(item3.Value, out var result2))
						{
							list3.Add(result2);
						}
					}
				}
				List<ItemID> list4 = new List<ItemID>();
				XElement xElement2 = item.Element("pickup_items");
				if (xElement2 != null && xElement2.HasElements)
				{
					foreach (XElement item4 in xElement2.Elements("item"))
					{
						ItemID itemID2 = CutinItemMaster.GetItemID(item4.Value);
						if (itemID2 != 0)
						{
							list4.Add(itemID2);
						}
					}
				}
				List<ItemID> list5 = new List<ItemID>();
				XElement xElement3 = item.Element("limited_pickup_items");
				if (xElement3 != null && xElement3.HasElements)
				{
					foreach (XElement item5 in xElement3.Elements("item"))
					{
						ItemID itemID3 = CutinItemMaster.GetItemID(item5.Value);
						if (itemID3 != 0)
						{
							list5.Add(itemID3);
						}
					}
				}
				List<ItemID> list6 = new List<ItemID>();
				XElement xElement4 = item.Element("exchange_items");
				if (xElement4 != null)
				{
					foreach (XElement item6 in xElement4.Elements("item"))
					{
						ItemID itemID4 = CutinItemMaster.GetItemID(item6.Value);
						if (itemID4 != 0)
						{
							list6.Add(itemID4);
						}
					}
				}
				List<Dictionary<Rarity, float>> list7 = new List<Dictionary<Rarity, float>>();
				XElement xElement5 = item.Element("pickup_rate");
				if (xElement5 != null)
				{
					List<string> list8 = new List<string> { "normal", "pickup", "limited" };
					foreach (string item7 in list8)
					{
						XElement xElement6 = xElement5.Element(item7);
						Dictionary<Rarity, float> dictionary = new Dictionary<Rarity, float>();
						dictionary[Rarity.N] = (float)xElement6.Element("n").ValueInt() / 10f;
						dictionary[Rarity.R] = (float)xElement6.Element("r").ValueInt() / 10f;
						dictionary[Rarity.SR] = (float)xElement6.Element("sr").ValueInt() / 10f;
						dictionary[Rarity.UR] = (float)xElement6.Element("ur").ValueInt() / 10f;
						list7.Add(dictionary);
						Debug.Log($"{list8} : {dictionary[Rarity.N]} / {dictionary[Rarity.R]} / {dictionary[Rarity.SR]} / {dictionary[Rarity.UR]} ");
					}
				}
				string startDate = ((item.Element("start_date") != null) ? item.Element("start_date").Value : null);
				string endDate = ((item.Element("end_date") != null) ? item.Element("end_date").Value : null);
				string forceEndDate = ((item.Element("force_end_date") != null) ? item.Element("force_end_date").Value : null);
				GachaSeriesServerInfo gachaSeriesServerInfo = new GachaSeriesServerInfo
				{
					SeriesID = seriesID,
					Label = value,
					TicketNum = ticketNum,
					IsActive = isActive,
					SeriesType = gachaSeriesType,
					Items = list,
					PickupItems = list4,
					LimitedItems = list5,
					MusicItems = list2,
					PickUpCharas = list3,
					ExchangeItems = list6,
					StartDate = startDate,
					EndDate = endDate,
					ProbabilityTable = list7,
					IsForceActive = isForceActive,
					ForceEndDate = forceEndDate
				};
				gachaSeriesServerInfo.SortItems();
				GachaSeriesInfosFromServer.Add(gachaSeriesServerInfo);
			}
			catch
			{
			}
			GachaSeriesInfosFromServer = GachaSeriesInfosFromServer.OrderBy((GachaSeriesServerInfo x) => x.SeriesID).ToList();
		}
	}

	public async UniTask<bool> Validate(GachaSeriesType seriesType = GachaSeriesType.None)
	{
		List<GachaSeriesServerInfo> removeList = new List<GachaSeriesServerInfo>();
		foreach (GachaSeriesServerInfo info in GachaSeriesInfosFromServer)
		{
			bool flag = await AssetLoader.Exists(info.AddressableAssetPath);
			if (flag)
			{
				flag = await AssetLoader.Exists(info.TitleAddressableAssetPath);
			}
			bool flag2 = flag;
			if (flag2)
			{
				bool flag3 = seriesType == GachaSeriesType.Music;
				if (!flag3)
				{
					flag3 = await AssetLoader.Exists(info.BgMonitorAddressableAssetPath());
				}
				flag2 = flag3;
			}
			bool flag4 = flag2;
			if (!info.IsDisplay() || !flag4)
			{
				removeList.Add(info);
			}
		}
		foreach (GachaSeriesServerInfo item in removeList)
		{
			GachaSeriesInfosFromServer.Remove(item);
		}
		if (seriesType != 0)
		{
			return ModeGachaInfo(seriesType)?.Exists((GachaSeriesServerInfo x) => x.IsDisplay()) ?? false;
		}
		return IsAnyActive();
	}

	public GachaSeriesName GetPickupItemContainSeries(List<ItemID> ids)
	{
		List<ItemID> list = ids;
		if (list != null && list.Count > 0)
		{
			try
			{
				GachaSeriesServerInfo gachaSeriesServerInfo = (from x in GachaSeriesInfosFromServer
					where x.IsDisplay() && x.SeriesType == GachaSeriesType.Pickup
					orderby x.SeriesID descending
					select x).ToList().Find((GachaSeriesServerInfo x) => x.IsLimitedCutin(ids));
				if (gachaSeriesServerInfo != null)
				{
					Debug.Log($"Gacha GetPickupItemContainSeries {gachaSeriesServerInfo.GachaSeriesName}");
					return gachaSeriesServerInfo.GachaSeriesName;
				}
			}
			catch
			{
			}
		}
		return (GachaSeriesName)(-1);
	}
}
