using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.GameRequest;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.CutinItem;
using MFG.Types.Customize.MatchingBackground;
using MFG.Types.PlayerItem;

public class GachaResultInfo
{
	public ulong RequestID;

	public List<GachaItemInfo> ItemInfos = new List<GachaItemInfo>();

	public List<GachaFightSpiritInfo> FightSpiritInfos = new List<GachaFightSpiritInfo>();

	public int PrevGiftCount;

	public int AfterGiftCount;

	public int AcquiredGiftCount;

	public int AcquiredExchangePoint;

	private GachaPlayMode mode;

	private int gachaCount;

	private bool isTicket;

	private GachaSeriesServerInfo seriesInfo;

	public bool IsUR1stConfirmed => ItemInfos.Any((GachaItemInfo x) => x.ThumbnailChangeRaritys[ThumbnailChangePhase.Change1st] == MFG.Types.Customize.CutinItem.Rarity.UR);

	public MFG.Types.Customize.CutinItem.ItemID GetItemID(int index)
	{
		return ItemInfos?[index].ItemID ?? MFG.Types.Customize.CutinItem.ItemID.None;
	}

	public void DebugLog()
	{
		ItemInfos.ForEach(delegate(GachaItemInfo x)
		{
			x.DebugLog();
		});
	}

	public GachaResultInfo()
	{
	}

	public GachaResultInfo(CutinGachaPlayDraw.ResultData resultData)
	{
		RequestID = resultData.GachaResult.RequestID;
		List<(MFG.Types.Customize.CutinItem.ItemID, GachaGiftType)> list = resultData.GachaResult.GainItems.Select(((string, GachaGiftType) x) => (CutinItemMaster.GetItemID(x.Item1), x.Item2)).ToList();
		foreach (var item in list)
		{
			GachaItemInfo gachaItemInfo = GachaPlayUtility.CreateItem(item.Item1);
			gachaItemInfo.GiftType = item.Item2;
			ItemInfos.Add(gachaItemInfo);
		}
		foreach (CharaType fightSpirit in resultData.GachaResult.FightSpirits)
		{
			FightSpiritInfos.Add(new GachaFightSpiritInfo
			{
				CharaType = fightSpirit
			});
		}
		AcquiredGiftCount = resultData.GachaResult.AddGiftCount;
		PrevGiftCount = GameDataManager.PlayerItem.Count(MFG.Types.PlayerItem.ItemID.Present01);
		AfterGiftCount = PrevGiftCount + AcquiredGiftCount;
		AcquiredExchangePoint = Math.Min(list.Count, 9999);
	}

	public async UniTask GachaApply(GachaPlayMode playMode, GachaSeriesServerInfo gInfo)
	{
		mode = playMode;
		gachaCount = GachaPlayUtility.Count(mode);
		isTicket = GachaPlayUtility.IsTicketGacha(mode);
		seriesInfo = gInfo;
		CharaType enabledChara = AffectToGameData();
		await GameDataManager.Customize.Save();
		await GameDataManager.Gacha.Save();
		await GameDataManager.Player.Save();
		await GameDataManager.PlayerItem.Save();
		await GameDataManager.PlayerPaidItem.Save();
		await SendCardConnect(gachaCount == 5);
		await RequestApiBase<CutinGachaPlayApply, CutinGachaPlayApply.ResultData, ulong>.Send(RequestID);
		if (enabledChara >= CharaType.Chara12)
		{
			CharaEnabledSend.ResultData resultData = await RequestApiBase<CharaEnabledSend, CharaEnabledSend.ResultData, CharaType>.Send(enabledChara);
			if (resultData.IsGachaEnable)
			{
				GameDataManager.Notice.PickupGachaNoticeCharaType = enabledChara;
				GameDataManager.Notice.PickupGachaNoticeDate = resultData.EndDateString;
			}
		}
	}

	public CharaType AffectToGameData()
	{
		CharaType result = (CharaType)(-1);
		List<CutinItemInfo> cutinItems = GameDataManager.Customize.CutinItems;
		foreach (GachaItemInfo resutItemInfo in ItemInfos)
		{
			int num = cutinItems.FindIndex((CutinItemInfo x) => x.ItemID == resutItemInfo.ItemID);
			int num2 = ((gachaCount == 5) ? 1 : 0);
			if (num < 0 && ((seriesInfo.SeriesType != GachaSeriesType.Pickup && seriesInfo.SeriesType != GachaSeriesType.Limited) || GameDataManager.IsCharaSelectable(CutinItemMaster.GetItem(resutItemInfo.ItemID).CharaType) || GameDataManager.PlayerConfig.UnableCharaGachaState == UnableCharaGachaSelectWindow.State.Get))
			{
				cutinItems.Add(new CutinItemInfo
				{
					ItemID = resutItemInfo.ItemID,
					IsOn = true,
					IsNew = true,
					AcquiredCountInGacha5 = num2
				});
				MFG.Types.Customize.MatchingBackground.Item matchinBGItem = MatchingBackgroundMaster.GetGachaCutinItems().Find((MFG.Types.Customize.MatchingBackground.Item x) => x.UnlockingCutinID == resutItemInfo.ItemID);
				if (matchinBGItem != null && !GameDataManager.Customize.MatchingBGItems.Exists((MatchingBGInfo x) => x.ItemID == matchinBGItem.ItemID) && !GameDataManager.Player.IsPro())
				{
					GameDataManager.Customize.MatchingBGItems.Add(new MatchingBGInfo
					{
						ItemID = matchinBGItem.ItemID
					});
				}
			}
			else if ((seriesInfo.SeriesType != GachaSeriesType.Pickup && seriesInfo.SeriesType != GachaSeriesType.Limited) || GameDataManager.IsCharaSelectable(CutinItemMaster.GetItem(resutItemInfo.ItemID).CharaType) || GameDataManager.PlayerConfig.UnableCharaGachaState != UnableCharaGachaSelectWindow.State.Gift)
			{
				cutinItems[num].AcquiredCountInGacha5 += num2;
			}
			if (GachaUtility.CharaUnlockItemIDDict.ContainsValue(resutItemInfo.ItemID))
			{
				CharaType chara = GachaUtility.GetGachaUnlockCharaType(resutItemInfo.ItemID);
				if (!GameDataManager.Player.GachaCharaUsableList.Exists((CharaType x) => x == chara))
				{
					GameDataManager.Player.GachaCharaUsableList.Add(chara);
					GameDataManager.Notice.InfoNewGirlNoticeFromGacha.Clear();
					GameDataManager.Notice.InfoNewGirlNoticeFromGacha.Add(chara);
					result = chara;
				}
			}
		}
		if (AcquiredGiftCount > 0)
		{
			GameDataManager.PlayerItem.Set(MFG.Types.PlayerItem.ItemID.Present01, AfterGiftCount);
		}
		foreach (GachaFightSpiritInfo fightSpiritInfo in FightSpiritInfos)
		{
			GameDataManager.PlayerItem.Add(PlayerItemMaster.ToFightSpiritItemID(fightSpiritInfo.CharaType), 1);
		}
		seriesInfo.AddExchangePoint(AcquiredExchangePoint);
		GachaCounterRefresh();
		if (isTicket)
		{
			GameDataManager.PlayerItem.Consume(MFG.Types.PlayerItem.ItemID.GachaTicket, GachaGameData.GetRequiredGachaTicket(gachaCount), ItemConsumeType.GachaTicket).Forget();
		}
		if (!GameDataManager.Player.IsOdekake)
		{
			GameDataManager.PlayerConfig.IsMyRoomSceneVisited = false;
		}
		return result;
	}

	public async UniTask SendCardConnect(bool isEnable)
	{
		if (!isEnable)
		{
			return;
		}
		foreach (GachaItemInfo itemInfo in ItemInfos)
		{
			MFG.Types.Customize.CutinItem.Item item = CutinItemMaster.GetItem(itemInfo.ItemID);
			if (item != null && item.Rarity >= MFG.Types.Customize.CutinItem.Rarity.SR)
			{
				GameDataManager.CardConnect.m_ccInfos.Add(new CardConnectInfo(GameDataManager.CardConnect.now_idx, "MemorialCard", item.ObjectID, item.ShortName));
				GameDataManager.CardConnect.now_idx++;
				GameDataManager.Notice.MemorialPrintNoticeInfos.Add(new MemorialPrintNoticeInfo
				{
					ItemID = itemInfo.ItemID
				});
			}
		}
		await GameDataManager.CardConnect.Save();
	}

	private void GachaCounterRefresh()
	{
		GameDataManager.Customize.TotalGacha1WithoutTicketCount += ((!isTicket && gachaCount == 1) ? 1 : 0);
		GameDataManager.Customize.TotalGacha5WithoutTicketCountVer2 += ((!isTicket && gachaCount == 5) ? 1 : 0);
		GameDataManager.Customize.TotalGacha1Count += ((gachaCount == 1) ? 1 : 0);
		GameDataManager.Customize.TotalGacha5CountVer2 += ((gachaCount == 5) ? 1 : 0);
		GameDataManager.Customize.TotalGacha5Count += ((gachaCount == 5) ? 1 : 0);
		GameDataManager.Customize.TotalGacha5WithoutTicketCount += ((gachaCount == 5) ? 1 : 0);
	}
}
