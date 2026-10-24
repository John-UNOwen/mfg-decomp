using System.Collections.Generic;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.CutinItem;
using UnityEngine;

public class CutinRecordDisp : MonoBehaviour
{
	public ItemID itemID;

	public Item Master => CutinItemMaster.GetItem(itemID);

	public void Play()
	{
		SetEquipItem(Master.Category);
		switch (Master.Category)
		{
		case Category.Naki:
			SetupNakiTsumikomi();
			break;
		case Category.Reach:
			SetupReachTsumikomi();
			break;
		case Category.AgariSmall:
		case Category.AgariBig:
			SetupAgariTsumikomi();
			break;
		}
	}

	private void SetEquipItem(Category category)
	{
		GameDataManager.Customize.CutinItems = new List<CutinItemInfo>();
		GameDataManager.Customize.CutinItems.Add(new CutinItemInfo
		{
			ItemID = itemID
		});
		GameDataManager.Mahjong.CutinRecItemOID = Master.ObjectID;
		foreach (ItemID defaultItemID in CutinItemMaster.GetDefaultItemIDs())
		{
			if (CutinItemMaster.GetItem(defaultItemID).Category != category)
			{
				GameDataManager.Customize.CutinItems.Add(new CutinItemInfo
				{
					ItemID = defaultItemID
				});
			}
		}
		GameDataManager.Player.GetCurrentChara().ResetCustomizeEquipToDefault();
	}

	private void SetupNakiTsumikomi()
	{
		List<Pai> list = new List<Pai>();
		foreach (int item in new List<int>
		{
			11, 11, 11, 11, 12, 12, 12, 12, 13, 13,
			13, 13, 14, 14, 14, 79, 17, 17, 37, 37,
			19, 21, 24, 25, 35, 36, 17, 17, 18, 14,
			15, 15, 79, 18, 18, 18, 19, 16, 16, 16,
			16, 19, 19, 21, 21, 21, 22, 22, 22, 22,
			23, 23, 23, 23, 24, 24, 24, 25, 89, 89,
			26, 26, 26, 26, 27, 27, 27, 27, 28, 28,
			28, 28, 29, 29, 29, 29, 31, 31, 31, 31,
			32, 32, 32, 32, 35, 35, 35, 36, 36, 36,
			37, 37, 1, 1, 1, 1, 2, 2, 2, 2,
			3, 3, 3, 3, 4, 4, 4, 4, 5, 5,
			5, 5, 6, 6, 6, 6, 7, 7, 7, 7,
			8, 8, 8, 8, 9, 9, 9, 9, 33, 33,
			33, 33, 34, 34, 34, 34
		})
		{
			list.Add((Pai)item);
		}
		GameDataManager.Mahjong.TsumikomiID = 1;
		GameDataManager.Mahjong.CustomTsumikomi = list;
		GameDataManager.Mahjong.TsumikomiChicha = Kaze.Nan;
		GameDataManager.Mahjong.IsRecNakiCutIn = true;
		GameDataManager.Mahjong.TakuType = TakuType.Nima;
		GameDataManager.Mahjong.EventType = MFG.Types.EventType.None;
	}

	private void SetupReachTsumikomi()
	{
		List<Pai> list = new List<Pai>();
		foreach (int item in new List<int>
		{
			11, 11, 12, 12, 13, 13, 21, 21, 14, 15,
			16, 31, 29, 14, 14, 79, 17, 17, 37, 37,
			19, 21, 24, 25, 35, 36, 31, 11, 11, 12,
			12, 13, 13, 17, 17, 18, 14, 15, 79, 18,
			18, 18, 19, 16, 16, 16, 19, 19, 21, 21,
			21, 22, 22, 22, 22, 23, 23, 23, 23, 24,
			24, 24, 25, 89, 89, 26, 26, 26, 26, 27,
			27, 27, 27, 28, 28, 28, 28, 29, 29, 29,
			31, 31, 32, 32, 32, 32, 35, 35, 35, 36,
			36, 36, 37, 37, 1, 1, 1, 1, 2, 2,
			2, 2, 3, 3, 3, 3, 4, 4, 4, 4,
			5, 5, 5, 5, 6, 6, 6, 6, 7, 7,
			7, 7, 8, 8, 8, 8, 9, 9, 9, 9,
			33, 33, 33, 33, 34, 34, 34, 34
		})
		{
			list.Add((Pai)item);
		}
		GameDataManager.Mahjong.TsumikomiID = 1;
		GameDataManager.Mahjong.CustomTsumikomi = list;
		GameDataManager.Mahjong.TsumikomiChicha = Kaze.Ton;
		GameDataManager.Mahjong.IsRecReachCutIn = true;
		GameDataManager.Mahjong.TakuType = TakuType.Nima;
		GameDataManager.Mahjong.EventType = MFG.Types.EventType.None;
	}

	private void SetupAgariTsumikomi()
	{
		List<Pai> list = new List<Pai>();
		foreach (int item in new List<int>
		{
			14, 14, 79, 17, 17, 37, 37, 19, 21, 24,
			25, 35, 36, 11, 11, 12, 12, 13, 13, 21,
			21, 14, 15, 16, 31, 31, 31, 11, 11, 12,
			12, 13, 13, 17, 17, 18, 14, 15, 79, 18,
			18, 18, 19, 16, 16, 16, 19, 19, 21, 21,
			21, 22, 22, 22, 22, 23, 23, 23, 23, 24,
			24, 24, 25, 89, 89, 26, 26, 26, 26, 27,
			27, 27, 27, 28, 28, 28, 28, 29, 29, 29,
			29, 31, 32, 32, 32, 32, 35, 35, 35, 36,
			36, 36, 37, 37, 1, 1, 1, 1, 2, 2,
			2, 2, 3, 3, 3, 3, 4, 4, 4, 4,
			5, 5, 5, 5, 6, 6, 6, 6, 7, 7,
			7, 7, 8, 8, 8, 8, 9, 9, 9, 9,
			33, 33, 33, 33, 34, 34, 34, 34
		})
		{
			list.Add((Pai)item);
		}
		GameDataManager.Mahjong.TsumikomiID = 1;
		GameDataManager.Mahjong.CustomTsumikomi = list;
		GameDataManager.Mahjong.TsumikomiChicha = Kaze.Nan;
		GameDataManager.Mahjong.IsRecAgariCutIn = true;
		GameDataManager.Mahjong.TakuType = TakuType.Nima;
		GameDataManager.Mahjong.EventType = MFG.Types.EventType.None;
	}
}
