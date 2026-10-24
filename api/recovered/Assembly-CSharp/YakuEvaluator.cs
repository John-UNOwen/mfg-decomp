using System;
using System.Collections.Generic;
using System.Linq;
using MFG.MahjonPlay.Hand;
using MFG.Types;
using UnityEngine;

public class YakuEvaluator
{
	private class SansyokuDoujyunCounter
	{
		public ushort MentsuBitFlag;

		public ushort KanpenBitFlag;

		public ushort RyanmenBitFlag;

		public ushort ZureTatsuBitFlag;
	}

	private class IkkitsukanCounter
	{
		public ushort MentsuBitFlag;

		public ushort KanpenBitFlag;

		public ushort RyanmenBitFlag;

		public ushort ZureTatsuBitFlag;
	}

	private class BlockCounter
	{
		public bool IsAvail = true;

		public int MentsuCount;

		public int ToitsuCount;

		public int TatsuCount;

		public int IsolatedCount;

		public ushort BitFlag;

		public void Clear()
		{
			IsAvail = true;
			MentsuCount = 0;
			ToitsuCount = 0;
			TatsuCount = 0;
			IsolatedCount = 0;
			BitFlag = 0;
		}
	}

	private BlockSet m_blocks;

	private Counter m_counter;

	private int m_shantenCount;

	private bool m_isTenpai;

	private TaikyokuState m_state;

	private MachiType[] m_machiTypes;

	private KaratenEvauator _karatenEvauator;

	private List<HaiID> _machiHaiIDs;

	private readonly int[] m_tyurenRequiredSuhaiCounts = new int[9] { 3, 1, 1, 1, 1, 1, 1, 1, 3 };

	private BlockCounter[] m_blockCounts = new BlockCounter[4];

	public YakuEvaluator()
	{
		for (int i = 0; i < 4; i++)
		{
			m_blockCounts[i] = new BlockCounter();
		}
	}

	private MachiType[] GetAllNoneMachiType()
	{
		MachiType[] array = new MachiType[34];
		for (HaiID haiID = HaiID.M1; haiID < HaiID.Max; haiID++)
		{
			array[(int)haiID] = MachiType.None;
		}
		return array;
	}

	public YakuInfo Evaluate(BlockSet blocks, MachiType[] machiType, TaikyokuState state, int shantenCount, List<HaiID> machiHais)
	{
		m_blocks = blocks;
		m_machiTypes = ((machiType != null) ? machiType : GetAllNoneMachiType());
		m_state = state;
		m_shantenCount = shantenCount;
		m_counter = m_blocks.CreateCounter();
		m_isTenpai = m_shantenCount <= 0;
		_machiHaiIDs = machiHais;
		for (int i = 0; i < 4; i++)
		{
			m_blockCounts[i].Clear();
		}
		YakuInfo yakuInfo = new YakuInfo();
		_karatenEvauator = new KaratenEvauator(m_counter);
		EvaluateYakuPossibility(yakuInfo);
		return yakuInfo;
	}

	private void EvaluateYakuPossibility(YakuInfo ret)
	{
		if (m_blocks.DivideType == DivideType.Kokushi && GameDataManager.Mahjong.TakuType != TakuType.Nima && !GameDataManager.Mahjong.IsJihaiUnusedSanma)
		{
			KaratenInfo karaten = _karatenEvauator.EvalKokushiKaraten();
			switch (m_shantenCount)
			{
			case 0:
				Add(ret, Yaku.KokushiTanki, YakuPossibility.Established, m_blocks.FindAllIndex((Block x) => x.HaiID.IsYaochu()), HaiID.None, HaiID.None, karaten);
				return;
			case 1:
			case 2:
				Add(ret, Yaku.KokushiTanki, YakuPossibility.High, m_blocks.FindAllIndex((Block x) => x.HaiID.IsYaochu()), HaiID.None, HaiID.None, karaten);
				return;
			case 3:
				Add(ret, Yaku.KokushiTanki, YakuPossibility.Middle, m_blocks.FindAllIndex((Block x) => x.HaiID.IsYaochu()), HaiID.None, HaiID.None, karaten);
				break;
			}
		}
		EvalReachi(in ret);
		EvalTanyao(in ret);
		EvalTsuisoAndChinrotoAndHonroto(in ret);
		EvalHonitsuChinitsu(in ret);
		EvalYakuhai(in ret);
		EvalSansyokuDoukou(in ret);
		EvalSankantsuAndSuKantsu(in ret);
		EvalAnko(in ret);
		EvalSyosushiAndDaisushi(in ret);
		EvalShousangenAndDaisangen(in ret);
		if (m_blocks.DivideType == DivideType.Chitoitsu)
		{
			if (m_blocks.FindAll((Block x) => x.BlockType == BlockType.AnKantsu || x.BlockType == BlockType.AnKoutsu).Count() >= 2)
			{
				return;
			}
			m_blocks.Count();
			switch (m_shantenCount)
			{
			case 0:
			{
				Block? block = m_blocks.Find((Block x) => x.BlockType == BlockType.Isolated);
				Add(ret, Yaku.Chitoitsu, YakuPossibility.Established, m_blocks.FindAllIndex((Block x) => x.BlockType == BlockType.Toitsu), block.Value.HaiID);
				break;
			}
			case 1:
				Add(ret, Yaku.Chitoitsu, YakuPossibility.High, m_blocks.FindAllIndex((Block x) => x.BlockType == BlockType.Toitsu));
				break;
			case 2:
				Add(ret, Yaku.Chitoitsu, YakuPossibility.Middle, m_blocks.FindAllIndex((Block x) => x.BlockType == BlockType.Toitsu));
				break;
			}
		}
		else
		{
			EvalPinfu(in ret);
			EvalIpeikoAndRyanPeiko(in ret);
			EvalIkkitsukan(in ret);
			EvalSansyokuDoujyun(in ret);
			EvalTtyuren(in ret);
			EvalJunchanAndChanta(in ret);
			EvalRyuiso(in ret);
		}
	}

	private int CalcYakuEvaluationPoint(int mentsuCount, int toitsuCount, int tatsuCount, bool enableChitoitsuEvaluation = true)
	{
		if (enableChitoitsuEvaluation && toitsuCount >= 4)
		{
			return 2 * toitsuCount;
		}
		int num = ((toitsuCount > 0) ? 5 : 4) - mentsuCount;
		int num2 = toitsuCount + tatsuCount;
		if (num2 > num)
		{
			num2 = num;
		}
		return 3 * mentsuCount + 2 * num2;
	}

	private void EvalReachi(in YakuInfo ret)
	{
		if (CanReach())
		{
			Add(ret, Yaku.Richi, YakuPossibility.High, 0);
		}
		else if (IsDoubleReached())
		{
			Add(ret, Yaku.DoubleRichi, YakuPossibility.Established, 0);
		}
		else if (IsReached())
		{
			Add(ret, Yaku.Richi, YakuPossibility.Established, 0);
		}
		else if (!m_blocks.IsMenzen())
		{
			Add(ret, Yaku.Richi, YakuPossibility.None, 0);
		}
	}

	private void EvalYakuhai(in YakuInfo ret)
	{
		if (!GameDataManager.Mahjong.IsJihaiUnusedSanma)
		{
			EvalYakuhai(in ret, Yaku.Haku, HaiID.J5);
			EvalYakuhai(in ret, Yaku.Hatsu, HaiID.J6);
			EvalYakuhai(in ret, Yaku.Tyun, HaiID.J7);
			EvalYakuhai(in ret, Yaku.Bakaze, GetBakazeHaiID());
			EvalYakuhai(in ret, Yaku.Jikaze, GetJikazeHaiID());
		}
	}

	private void EvalYakuhai(in YakuInfo ret, Yaku yaku, HaiID yakuhaiID)
	{
		ushort num = m_blocks.FindAllIndex((Block x) => x.HaiID == yakuhaiID && x.IsMentsu());
		if (num > 0)
		{
			Add(ret, yaku, YakuPossibility.Established, num);
			return;
		}
		num = m_blocks.FindAllIndex((Block x) => x.HaiID == yakuhaiID && x.IsToitsu());
		if (num > 0)
		{
			KaratenInfo karaten = _karatenEvauator.EvalYakuhaiKaraten(yakuhaiID);
			if (IsTenpai() && m_machiTypes[(int)yakuhaiID] > MachiType.None)
			{
				Add(ret, yaku, YakuPossibility.EstablishedInTakame, num, yakuhaiID, HaiID.None, karaten);
			}
			else
			{
				Add(ret, yaku, YakuPossibility.High, num, HaiID.None, HaiID.None, karaten);
			}
		}
	}

	private void EvalJunchanAndChanta(in YakuInfo ret)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		ushort num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		ushort num8 = 0;
		int num9 = 0;
		int num10 = 0;
		int num11 = 0;
		HaiID haiID = HaiID.None;
		bool flag = true;
		int num12 = m_blocks.Count();
		for (int i = 0; i < num12; i++)
		{
			Block block = m_blocks[i];
			HaiID haiID2 = block.HaiID;
			SuID su = haiID2.GetSu();
			switch (block.BlockType)
			{
			case BlockType.MinKantsu:
			case BlockType.MinKoutsu:
			case BlockType.AnKantsu:
				if (haiID2.IsRoutou())
				{
					num++;
					num4 |= (ushort)(1 << i);
					num5++;
					num8 |= (ushort)(1 << i);
					break;
				}
				if (haiID2.IsJihai())
				{
					flag = false;
					num5++;
					num8 |= (ushort)(1 << i);
					break;
				}
				return;
			case BlockType.AnKoutsu:
				if (haiID2.IsRoutou())
				{
					num++;
					num4 |= (ushort)(1 << i);
					num5++;
					num8 |= (ushort)(1 << i);
				}
				else if (haiID2.IsJihai())
				{
					num5++;
					num8 |= (ushort)(1 << i);
				}
				break;
			case BlockType.MinShuntsu:
				if (su == SuID.SU1 || su == SuID.SU7)
				{
					num++;
					num4 |= (ushort)(1 << i);
					num5++;
					num8 |= (ushort)(1 << i);
					break;
				}
				return;
			case BlockType.AnShuntsu:
				switch (su)
				{
				case SuID.SU1:
				case SuID.SU7:
					num++;
					num4 |= (ushort)(1 << i);
					num5++;
					num8 |= (ushort)(1 << i);
					break;
				case SuID.SU2:
				case SuID.SU6:
					num9++;
					break;
				}
				break;
			case BlockType.Toitsu:
				if (haiID2.IsRoutou())
				{
					num2++;
					num4 |= (ushort)(1 << i);
					num6++;
					num8 |= (ushort)(1 << i);
				}
				else if (haiID2.IsJihai())
				{
					num6++;
					num8 |= (ushort)(1 << i);
				}
				break;
			case BlockType.RyanmenTatsu:
				if (su == SuID.SU2 || su == SuID.SU7)
				{
					num10++;
					num4 |= (ushort)(1 << i);
					num8 |= (ushort)(1 << i);
					haiID = ((su == SuID.SU2) ? (haiID2 - 1) : (haiID2 + 2));
				}
				break;
			case BlockType.PenchanTatsu:
				num11++;
				num4 |= (ushort)(1 << i);
				num8 |= (ushort)(1 << i);
				break;
			case BlockType.KanchanTatsu:
				if (su == SuID.SU1 || su == SuID.SU7)
				{
					num11++;
					num4 |= (ushort)(1 << i);
					num8 |= (ushort)(1 << i);
				}
				break;
			case BlockType.Isolated:
				if (haiID2.IsRoutou())
				{
					num3++;
					num4 |= (ushort)(1 << i);
					num7++;
					num8 |= (ushort)(1 << i);
				}
				else if (haiID2.IsJihai())
				{
					num7++;
					num8 |= (ushort)(1 << i);
				}
				break;
			}
		}
		bool flag2 = false;
		if (flag)
		{
			KaratenInfo karaten = _karatenEvauator.EvalJunchanKaraten(IsReached(), haiID);
			if ((num == 4 && num3 == 1) || (num == 3 && num2 == 2) || (num == 3 && num2 == 1 && num11 == 1))
			{
				Add(ret, Yaku.Junchan, YakuPossibility.Established, num4, HaiID.None, HaiID.None, karaten);
				flag2 = true;
			}
			else if (num == 3 && num2 == 1 && num10 == 1)
			{
				Add(ret, Yaku.Junchan, YakuPossibility.EstablishedInTakame, num4, haiID, HaiID.None, karaten);
				flag2 = true;
			}
			else
			{
				int num13 = num + num2 + num11;
				if (m_shantenCount <= 0 && (num >= 4 || (num >= 3 && num13 >= 5)))
				{
					Add(ret, Yaku.Junchan, YakuPossibility.High, num4, HaiID.None, HaiID.None, karaten);
				}
				else if (m_shantenCount <= 1 && ((num >= 3 && num13 >= 4) || (num >= 2 && num13 >= 5)))
				{
					Add(ret, Yaku.Junchan, YakuPossibility.Middle, num4, HaiID.None, HaiID.None, karaten);
				}
			}
		}
		if (flag2 || GameDataManager.Mahjong.IsJihaiUnusedSanma)
		{
			return;
		}
		KaratenInfo karaten2 = _karatenEvauator.EvalChantaKaraten(IsReached(), haiID);
		if ((num5 == 4 && num7 == 1) || (num5 == 3 && num6 == 2) || (num5 == 3 && num6 == 1 && num11 == 1))
		{
			Add(ret, Yaku.Chanta, YakuPossibility.Established, num8, HaiID.None, HaiID.None, karaten2);
			return;
		}
		if (num5 == 3 && num6 == 1 && num10 == 1)
		{
			Add(ret, Yaku.Chanta, YakuPossibility.EstablishedInTakame, num8, haiID, HaiID.None, karaten2);
			return;
		}
		int num14 = num5 + num6 + num11;
		if (m_shantenCount <= 0 && (num5 >= 4 || (num5 >= 3 && num14 >= 5)))
		{
			Add(ret, Yaku.Chanta, YakuPossibility.High, num8, HaiID.None, HaiID.None, karaten2);
		}
		else if (m_shantenCount <= 1 && ((num5 >= 3 && num14 >= 4) || (num5 >= 2 && num14 >= 5)))
		{
			Add(ret, Yaku.Chanta, YakuPossibility.Middle, num8, HaiID.None, HaiID.None, karaten2);
		}
	}

	private void EvalPinfu(in YakuInfo ret)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		ushort num6 = 0;
		HaiID ryanmenID = HaiID.None;
		int num7 = m_blocks.Count();
		HaiID haiID = HaiID.None;
		HaiID haiID2 = HaiID.None;
		for (int i = 0; i < num7; i++)
		{
			Block block = m_blocks[i];
			HaiID haiID3 = block.HaiID;
			switch (block.BlockType)
			{
			case BlockType.AnKoutsu:
				if (!IsYakuhai(haiID3))
				{
					num++;
				}
				if (!haiID3.IsJihai())
				{
					haiID = haiID3;
				}
				break;
			case BlockType.AnShuntsu:
				num2++;
				num6 |= (ushort)(1 << i);
				break;
			case BlockType.Toitsu:
				if (!IsYakuhai(haiID3))
				{
					num3++;
					num6 |= (ushort)(1 << i);
				}
				break;
			case BlockType.RyanmenTatsu:
				num5++;
				num4++;
				ryanmenID = haiID3;
				num6 |= (ushort)(1 << i);
				break;
			case BlockType.PenchanTatsu:
				num4++;
				num6 |= (ushort)(1 << i);
				break;
			case BlockType.KanchanTatsu:
				num4++;
				num6 |= (ushort)(1 << i);
				break;
			case BlockType.Isolated:
				if (!haiID3.IsJihai())
				{
					haiID2 = haiID3;
				}
				break;
			case BlockType.MinKantsu:
			case BlockType.MinKoutsu:
			case BlockType.MinShuntsu:
			case BlockType.AnKantsu:
				return;
			}
		}
		if (IsTenpai() && num3 == 1 && num2 == 3 && num5 == 1)
		{
			Add(ret, Yaku.Pinfu, YakuPossibility.Established, num6, HaiID.None, HaiID.None, _karatenEvauator.EvalPinfuKaraten(IsReached(), ryanmenID));
		}
		else if (IsTenpai() && num2 == 3)
		{
			if (haiID2 == haiID - 1 && haiID.GetSu() >= SuID.SU3 && haiID.GetSu() <= SuID.SU8)
			{
				Add(ret, Yaku.Pinfu, YakuPossibility.EstablishedInTakame, num6, haiID2 - 1, haiID + 1, _karatenEvauator.EvalPinfuKaraten(IsReached(), ryanmenID));
			}
			else if (haiID2 == haiID + 1 && haiID.GetSu() >= SuID.SU2 && haiID.GetSu() <= SuID.SU7)
			{
				Add(ret, Yaku.Pinfu, YakuPossibility.EstablishedInTakame, num6, haiID - 1, haiID2 + 1, _karatenEvauator.EvalPinfuKaraten(IsReached(), ryanmenID));
			}
		}
		else if (num2 >= 2)
		{
			int num8 = CalcYakuEvaluationPoint(num2, (num3 + num > 0) ? 1 : 0, num5, enableChitoitsuEvaluation: false);
			if (num8 >= 11)
			{
				Add(ret, Yaku.Pinfu, YakuPossibility.High, num6);
			}
			else if (num8 >= 10)
			{
				Add(ret, Yaku.Pinfu, YakuPossibility.Middle, num6);
			}
		}
	}

	private void EvalTanyao(in YakuInfo ret)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		ushort num6 = 0;
		HaiID haiID = HaiID.None;
		int num7 = m_blocks.Count();
		for (int i = 0; i < num7; i++)
		{
			Block block = m_blocks[i];
			HaiID haiID2 = block.HaiID;
			switch (block.BlockType)
			{
			case BlockType.MinShuntsu:
				if (haiID2.GetSu() == SuID.SU1 || haiID2.GetSu() == SuID.SU7 || !MahjongUtility.IsKuitanEnable(GameDataManager.Mahjong.EventType))
				{
					return;
				}
				num++;
				num6 |= (ushort)(1 << i);
				break;
			case BlockType.MinKantsu:
				if (haiID2.IsYaochu() || !MahjongUtility.IsKuitanEnable(GameDataManager.Mahjong.EventType))
				{
					return;
				}
				num++;
				num6 |= (ushort)(1 << i);
				break;
			case BlockType.AnKantsu:
				if (haiID2.IsYaochu())
				{
					return;
				}
				num++;
				num6 |= (ushort)(1 << i);
				break;
			case BlockType.MinKoutsu:
				if (haiID2.IsYaochu() || !MahjongUtility.IsKuitanEnable(GameDataManager.Mahjong.EventType))
				{
					return;
				}
				num++;
				num6 |= (ushort)(1 << i);
				break;
			case BlockType.AnKoutsu:
				if (haiID2.IsYaochu())
				{
					return;
				}
				num++;
				num6 |= (ushort)(1 << i);
				break;
			case BlockType.AnShuntsu:
				if (block.HaiID.GetSu() == SuID.SU1 || block.HaiID.GetSu() == SuID.SU7)
				{
					num4++;
					num3++;
					num5++;
				}
				else
				{
					num++;
					num6 |= (ushort)(1 << i);
				}
				break;
			case BlockType.Toitsu:
				if (haiID2.IsYaochu())
				{
					return;
				}
				num2++;
				num6 |= (ushort)(1 << i);
				break;
			case BlockType.PenchanTatsu:
				num4++;
				break;
			case BlockType.KanchanTatsu:
				if (haiID2.GetSu() == SuID.SU1 || haiID2.GetSu() == SuID.SU7)
				{
					num4++;
					num5++;
				}
				else
				{
					num3++;
					num6 |= (ushort)(1 << i);
				}
				break;
			case BlockType.Isolated:
				if (block.HaiID.IsYaochu())
				{
					num4++;
				}
				break;
			case BlockType.RyanmenTatsu:
				num3++;
				if (haiID2.GetSu() == SuID.SU2)
				{
					haiID = HaiUtility.ToHaiID(haiID2.GetMark(), SuID.SU4);
					num5++;
				}
				else if (haiID2.GetSu() == SuID.SU7)
				{
					haiID = HaiUtility.ToHaiID(haiID2.GetMark(), SuID.SU6);
					num5++;
				}
				num6 |= (ushort)(1 << i);
				break;
			}
		}
		if (IsTenpai() && num4 == 0)
		{
			int num8 = 0;
			foreach (HaiID machiHaiID in _machiHaiIDs)
			{
				if (machiHaiID.IsYaochu())
				{
					num8++;
				}
			}
			if (num8 > 0)
			{
				Add(ret, Yaku.Tanyao, YakuPossibility.EstablishedInTakame, num6, haiID, HaiID.None, _karatenEvauator.EvalTakameKaraten(IsReached(), haiID));
			}
			else
			{
				Add(ret, Yaku.Tanyao, YakuPossibility.Established, num6);
			}
		}
		else if (num >= 2 && num5 < 3)
		{
			int num9 = CalcYakuEvaluationPoint(num, num2, num3);
			if (num9 >= 11)
			{
				Add(ret, Yaku.Tanyao, YakuPossibility.High, num6);
			}
			else if (num9 >= 10)
			{
				Add(ret, Yaku.Tanyao, YakuPossibility.Middle, 0);
			}
		}
	}

	public void EvalIpeikoAndRyanPeiko(in YakuInfo ret)
	{
		if (!m_blocks.IsMenzen())
		{
			return;
		}
		List<int> exclusionElementIndexes = new List<int>();
		List<int> ipeikoElementIndexes;
		HaiID takameMachiHaiID;
		YakuPossibility yakuPossibility = EvalIpeiko(exclusionElementIndexes, out ipeikoElementIndexes, out takameMachiHaiID);
		ushort num = 0;
		foreach (int item in ipeikoElementIndexes)
		{
			if (item >= 0)
			{
				num |= (ushort)(1 << item);
			}
		}
		Add(ret, Yaku.Ipeiko, yakuPossibility, num, takameMachiHaiID, HaiID.None, _karatenEvauator.EvalTakameKaraten(IsReached(), takameMachiHaiID));
		if (yakuPossibility < YakuPossibility.High)
		{
			return;
		}
		List<int> ipeikoElementIndexes2;
		HaiID rakameMachiHaiID;
		YakuPossibility possibility = EvalRyanpeko(ipeikoElementIndexes, yakuPossibility, out ipeikoElementIndexes2, out rakameMachiHaiID);
		foreach (int item2 in ipeikoElementIndexes2)
		{
			if (item2 >= 0)
			{
				num |= (ushort)(1 << item2);
			}
		}
		Add(ret, Yaku.Ryanpeko, possibility, num, rakameMachiHaiID, HaiID.None, _karatenEvauator.EvalTakameKaraten(IsReached(), rakameMachiHaiID));
	}

	private YakuPossibility EvalIpeiko(List<int> _exclusionElementIndexes, out List<int> ipeikoElementIndexes, out HaiID takameMachiHaiID)
	{
		List<int> list = (ipeikoElementIndexes = new List<int>(2));
		takameMachiHaiID = HaiID.None;
		if (!m_blocks.IsMenzen())
		{
			return YakuPossibility.None;
		}
		YakuPossibility yakuPossibility = YakuPossibility.None;
		int item = -1;
		int item2 = -1;
		int num = m_blocks.Count();
		for (int i = 0; i < num; i++)
		{
			if (m_blocks[i].BlockType != BlockType.AnShuntsu || (i > 0 && m_blocks[i - 1].BlockType == m_blocks[i].BlockType && m_blocks[i - 1].HaiID == m_blocks[i].HaiID))
			{
				continue;
			}
			HaiID haiID = m_blocks[i].HaiID;
			haiID.GetMark();
			SuID su = haiID.GetSu();
			int retBlockIndex = -1;
			int retBlockIndex2 = -1;
			if (m_blocks.Exists(haiID, BlockType.AnShuntsu, 1, _exclusionElementIndexes, out retBlockIndex))
			{
				List<int> list2 = new List<int>(_exclusionElementIndexes);
				list2.Add(retBlockIndex);
				YakuPossibility yakuPossibility2 = YakuPossibility.None;
				if (m_blocks.Exists(haiID, BlockType.AnShuntsu, 1, list2, out retBlockIndex2))
				{
					yakuPossibility2 = YakuPossibility.Established;
				}
				else if (m_blocks.Exists(haiID, BlockType.KanchanTatsu, 1, list2, out retBlockIndex2))
				{
					yakuPossibility2 = (IsTenpai() ? YakuPossibility.Established : YakuPossibility.High);
				}
				else if (su == SuID.SU1 && m_blocks.Exists(haiID, BlockType.PenchanTatsu, 1, list2, out retBlockIndex2))
				{
					yakuPossibility2 = (IsTenpai() ? YakuPossibility.Established : YakuPossibility.High);
				}
				else if (su == SuID.SU7 && m_blocks.Exists(haiID + 1, BlockType.PenchanTatsu, 1, list2, out retBlockIndex2))
				{
					yakuPossibility2 = (IsTenpai() ? YakuPossibility.Established : YakuPossibility.High);
				}
				else if (m_blocks.Exists(haiID, BlockType.RyanmenTatsu, 1, list2, out retBlockIndex2))
				{
					yakuPossibility2 = (IsTenpai() ? YakuPossibility.EstablishedInTakame : YakuPossibility.High);
					takameMachiHaiID = haiID + 2;
				}
				else if (m_blocks.Exists(haiID + 1, BlockType.RyanmenTatsu, 1, list2, out retBlockIndex2))
				{
					yakuPossibility2 = (IsTenpai() ? YakuPossibility.EstablishedInTakame : YakuPossibility.High);
					takameMachiHaiID = haiID;
				}
				else if (su >= SuID.SU2 && m_blocks.Exists(haiID - 1, BlockType.AnShuntsu, 1, list2, out retBlockIndex2))
				{
					yakuPossibility2 = YakuPossibility.High;
				}
				else if (su <= SuID.SU6 && m_blocks.Exists(haiID + 1, BlockType.AnShuntsu, 1, list2, out retBlockIndex2))
				{
					yakuPossibility2 = YakuPossibility.High;
				}
				if (yakuPossibility2 == YakuPossibility.Established)
				{
					list.Add(retBlockIndex);
					list.Add(retBlockIndex2);
					return YakuPossibility.Established;
				}
				if (yakuPossibility2 > yakuPossibility)
				{
					item = retBlockIndex;
					item2 = retBlockIndex2;
					yakuPossibility = yakuPossibility2;
				}
			}
		}
		list.Add(item);
		list.Add(item2);
		return yakuPossibility;
	}

	private YakuPossibility EvalRyanpeko(List<int> exclusionElementIndexes, YakuPossibility ipeikoPossibility, out List<int> ipeikoElementIndexes, out HaiID rakameMachiHaiID)
	{
		switch (EvalIpeiko(exclusionElementIndexes, out ipeikoElementIndexes, out rakameMachiHaiID))
		{
		case YakuPossibility.Middle:
			if (ipeikoPossibility == YakuPossibility.Established)
			{
				return YakuPossibility.Middle;
			}
			break;
		case YakuPossibility.High:
			switch (ipeikoPossibility)
			{
			case YakuPossibility.Established:
				return YakuPossibility.High;
			case YakuPossibility.High:
				return YakuPossibility.Middle;
			case YakuPossibility.EstablishedInTakame:
				return YakuPossibility.Middle;
			}
			break;
		case YakuPossibility.Established:
			switch (ipeikoPossibility)
			{
			case YakuPossibility.Established:
			{
				HaiID haiID = m_blocks.Find((Block x) => x.BlockType == BlockType.Isolated)?.HaiID ?? HaiID.None;
				if (haiID != HaiID.None)
				{
					HaiID haiID2 = m_blocks[exclusionElementIndexes[0]].HaiID;
					HaiID haiID3 = m_blocks[ipeikoElementIndexes[0]].HaiID;
					HaiMarkType mark = haiID.GetMark();
					HaiMarkType mark2 = haiID2.GetMark();
					HaiMarkType mark3 = haiID3.GetMark();
					if (mark2 == mark3 && Math.Abs(haiID2 - haiID3) < 2 && ((mark == mark2 && haiID >= haiID2 - 2 && haiID <= haiID2 + 4) || (mark == mark3 && haiID >= haiID3 - 2 && haiID <= haiID3 + 4)))
					{
						rakameMachiHaiID = haiID;
						return YakuPossibility.EstablishedInTakame;
					}
				}
				return YakuPossibility.Established;
			}
			case YakuPossibility.High:
				return YakuPossibility.High;
			case YakuPossibility.EstablishedInTakame:
				return YakuPossibility.EstablishedInTakame;
			}
			break;
		case YakuPossibility.EstablishedInTakame:
			switch (ipeikoPossibility)
			{
			case YakuPossibility.Established:
				return YakuPossibility.EstablishedInTakame;
			case YakuPossibility.High:
				return YakuPossibility.Middle;
			case YakuPossibility.EstablishedInTakame:
				return YakuPossibility.High;
			}
			break;
		}
		return YakuPossibility.Low;
	}

	private void EvalSansyokuDoukou(in YakuInfo ret)
	{
		if (GameDataManager.Mahjong.TakuType == TakuType.Nima)
		{
			return;
		}
		YakuPossibility yakuPossibility = YakuPossibility.Low;
		ushort requiredBlockBitFlag = 0;
		HaiID haiID = HaiID.None;
		for (SuID suID = SuID.SU1; suID <= SuID.SU9; suID++)
		{
			var (yakuPossibility2, num, haiID2) = EvalSansyokuDoukou(suID);
			if (yakuPossibility == YakuPossibility.Established)
			{
				Add(ret, Yaku.Sansyokudoko, YakuPossibility.Established, num);
				return;
			}
			if (yakuPossibility2 > yakuPossibility)
			{
				yakuPossibility = yakuPossibility2;
				requiredBlockBitFlag = num;
				haiID = haiID2;
			}
		}
		KaratenInfo karaten = _karatenEvauator.EvalSanshokudokoKaraten(GameDataManager.Mahjong.TakuType == TakuType.Sanma && !GameDataManager.Mahjong.IsJihaiUnusedSanma, IsReached(), haiID);
		Add(ret, Yaku.Sansyokudoko, yakuPossibility, requiredBlockBitFlag, haiID, HaiID.None, karaten);
	}

	private (YakuPossibility possibility, ushort bitIndex, HaiID takameMachiHaiID) EvalSansyokuDoukou(SuID _su)
	{
		bool[] array = new bool[3];
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = -1;
		ushort num5 = 0;
		ushort num6 = 0;
		HaiID haiID = HaiID.None;
		int num7 = m_blocks.Count();
		for (int i = 0; i < num7; i++)
		{
			Block block = m_blocks[i];
			HaiID haiID2 = block.HaiID;
			HaiMarkType mark = haiID2.GetMark();
			SuID su = haiID2.GetSu();
			switch (block.BlockType)
			{
			case BlockType.AnKantsu:
			case BlockType.AnKoutsu:
				if (mark != HaiMarkType.Jihai && su == _su)
				{
					num++;
					array[(int)mark] = true;
					num5 |= (ushort)(1 << i);
				}
				break;
			case BlockType.MinKantsu:
			case BlockType.MinKoutsu:
				if (mark != HaiMarkType.Jihai && su == _su)
				{
					num++;
					array[(int)mark] = true;
					num5 |= (ushort)(1 << i);
				}
				else
				{
					num3++;
				}
				break;
			case BlockType.Toitsu:
				if (mark != HaiMarkType.Jihai && su == _su)
				{
					num2++;
					haiID = haiID2;
					num6 |= (ushort)(1 << i);
				}
				else
				{
					num4 = (int)haiID2;
				}
				break;
			case BlockType.MinShuntsu:
				num3++;
				break;
			}
		}
		if (num3 >= 2)
		{
			return (possibility: YakuPossibility.Low, bitIndex: 0, takameMachiHaiID: HaiID.None);
		}
		switch (num)
		{
		case 3:
			return (possibility: YakuPossibility.Established, bitIndex: num5, takameMachiHaiID: HaiID.None);
		case 2:
			if (num2 < 1)
			{
				break;
			}
			if (IsTenpai() && m_machiTypes[(int)haiID] == MachiType.Shabo)
			{
				if (num4 >= 0)
				{
					return (possibility: YakuPossibility.EstablishedInTakame, bitIndex: (ushort)(num5 | num6), takameMachiHaiID: haiID);
				}
				return (possibility: YakuPossibility.Established, bitIndex: (ushort)(num5 | num6), takameMachiHaiID: HaiID.None);
			}
			return (possibility: YakuPossibility.High, bitIndex: (ushort)(num5 | num6), takameMachiHaiID: HaiID.None);
		}
		return (possibility: (num + num2 < 3) ? YakuPossibility.Low : YakuPossibility.Middle, bitIndex: (ushort)(num5 | num6), takameMachiHaiID: HaiID.None);
	}

	private void EvalSansyokuDoujyun(in YakuInfo ret)
	{
		if (GameDataManager.Mahjong.TakuType == TakuType.Nima || (GameDataManager.Mahjong.TakuType == TakuType.Sanma && !GameDataManager.Mahjong.IsJihaiUnusedSanma))
		{
			return;
		}
		YakuPossibility yakuPossibility = YakuPossibility.Low;
		ushort requiredBlockBitFlag = 0;
		HaiID haiID = HaiID.None;
		for (SuID suID = SuID.SU1; suID <= SuID.SU7; suID++)
		{
			var (yakuPossibility2, num, haiID2) = EvalSansyokuDoujyun(suID);
			if (yakuPossibility == YakuPossibility.Established)
			{
				Add(ret, Yaku.Sansyokudojun, YakuPossibility.Established, num);
				return;
			}
			if (yakuPossibility2 > yakuPossibility)
			{
				yakuPossibility = yakuPossibility2;
				requiredBlockBitFlag = num;
				haiID = haiID2;
			}
		}
		Add(ret, Yaku.Sansyokudojun, yakuPossibility, requiredBlockBitFlag, haiID, HaiID.None, _karatenEvauator.EvalTakameKaraten(IsReached(), haiID));
	}

	private (YakuPossibility possibility, ushort bitIndex, HaiID takameMachiHaiID) EvalSansyokuDoujyun(SuID _su)
	{
		SansyokuDoujyunCounter[] array = new SansyokuDoujyunCounter[3]
		{
			new SansyokuDoujyunCounter(),
			new SansyokuDoujyunCounter(),
			new SansyokuDoujyunCounter()
		};
		HaiID item = HaiID.None;
		int num = 0;
		int num2 = 0;
		List<HaiMarkType> list = new List<HaiMarkType>();
		int num3 = m_blocks.Count();
		for (int i = 0; i < num3; i++)
		{
			Block block = m_blocks[i];
			HaiID haiID = block.HaiID;
			HaiMarkType mark = haiID.GetMark();
			if (mark == HaiMarkType.Jihai)
			{
				continue;
			}
			SansyokuDoujyunCounter sansyokuDoujyunCounter = array[(int)mark];
			SuID su = haiID.GetSu();
			if (array[(int)mark].MentsuBitFlag != 0)
			{
				if (block.BlockType == BlockType.AnShuntsu)
				{
					if (su == _su)
					{
						num++;
					}
					else if ((_su == SuID.SU1 && su == SuID.SU2) || (_su == SuID.SU7 && su == SuID.SU6) || _su + 1 == su || _su - 1 == su)
					{
						num2++;
					}
				}
				else if (block.BlockType == BlockType.RyanmenTatsu && (_su + 1 == su || _su == su))
				{
					num2++;
				}
				continue;
			}
			ushort num4 = (ushort)(1 << i);
			switch (block.BlockType)
			{
			case BlockType.MinShuntsu:
			case BlockType.AnShuntsu:
				if (su == _su && array[(int)mark].MentsuBitFlag == 0)
				{
					sansyokuDoujyunCounter.MentsuBitFlag |= num4;
					if (block.BlockType == BlockType.MinShuntsu)
					{
						list.Add(mark);
					}
				}
				else if (_su == SuID.SU1 && su == SuID.SU2 && sansyokuDoujyunCounter.ZureTatsuBitFlag == 0)
				{
					sansyokuDoujyunCounter.ZureTatsuBitFlag |= num4;
				}
				else if (_su == SuID.SU7 && su == SuID.SU6 && sansyokuDoujyunCounter.ZureTatsuBitFlag == 0)
				{
					sansyokuDoujyunCounter.ZureTatsuBitFlag |= num4;
				}
				else if ((_su + 1 == su || _su - 1 == su) && sansyokuDoujyunCounter.ZureTatsuBitFlag == 0)
				{
					sansyokuDoujyunCounter.ZureTatsuBitFlag |= num4;
				}
				break;
			case BlockType.RyanmenTatsu:
				if ((_su == su || _su + 1 == su) && sansyokuDoujyunCounter.RyanmenBitFlag == 0)
				{
					sansyokuDoujyunCounter.RyanmenBitFlag |= num4;
					item = ((_su != su) ? (haiID - 1) : (haiID + 2));
				}
				break;
			case BlockType.PenchanTatsu:
				if (((_su == SuID.SU1 && su == SuID.SU1) || (_su == SuID.SU7 && su == SuID.SU8)) && sansyokuDoujyunCounter.KanpenBitFlag == 0)
				{
					sansyokuDoujyunCounter.KanpenBitFlag |= num4;
				}
				break;
			case BlockType.KanchanTatsu:
				if (_su == su && sansyokuDoujyunCounter.KanpenBitFlag == 0)
				{
					sansyokuDoujyunCounter.KanpenBitFlag |= num4;
				}
				break;
			}
		}
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		ushort num9 = 0;
		for (int j = 0; j < 3; j++)
		{
			SansyokuDoujyunCounter sansyokuDoujyunCounter2 = array[j];
			if (sansyokuDoujyunCounter2.MentsuBitFlag != 0)
			{
				num5++;
				num9 |= sansyokuDoujyunCounter2.MentsuBitFlag;
			}
			else if (sansyokuDoujyunCounter2.KanpenBitFlag != 0)
			{
				num6++;
				num9 |= sansyokuDoujyunCounter2.KanpenBitFlag;
			}
			else if (sansyokuDoujyunCounter2.RyanmenBitFlag != 0)
			{
				num7++;
				num9 |= sansyokuDoujyunCounter2.RyanmenBitFlag;
			}
			else if (sansyokuDoujyunCounter2.ZureTatsuBitFlag != 0)
			{
				num8++;
				num9 |= sansyokuDoujyunCounter2.ZureTatsuBitFlag;
			}
		}
		if (IsTenpai() && num5 == 3)
		{
			foreach (HaiID machiHaiID in _machiHaiIDs)
			{
				SuID su2 = machiHaiID.GetSu();
				if (!machiHaiID.IsJihai() && ((su2 == _su - 1 && _su >= SuID.SU2 && m_machiTypes[(int)machiHaiID] == MachiType.Tanki) || (su2 == _su + 3 && _su < SuID.SU7 && m_machiTypes[(int)machiHaiID] == MachiType.Tanki)) && num <= 0 && num2 <= 0 && !list.Contains(machiHaiID.GetMark()))
				{
					return (possibility: YakuPossibility.EstablishedInTakame, bitIndex: num9, takameMachiHaiID: HaiID.None);
				}
			}
			return (possibility: YakuPossibility.Established, bitIndex: num9, takameMachiHaiID: HaiID.None);
		}
		if (num5 == 2 && num6 == 1)
		{
			return (possibility: IsTenpai() ? YakuPossibility.Established : YakuPossibility.High, bitIndex: num9, takameMachiHaiID: HaiID.None);
		}
		if (num5 == 2 && num7 == 1)
		{
			return (possibility: IsTenpai() ? YakuPossibility.EstablishedInTakame : YakuPossibility.High, bitIndex: num9, takameMachiHaiID: item);
		}
		int num10 = num5 * 3 + num7 + num6 + num8;
		if (num10 >= 7)
		{
			return (possibility: YakuPossibility.High, bitIndex: num9, takameMachiHaiID: HaiID.None);
		}
		if (num10 >= 5 && num6 + num7 + num8 > 0)
		{
			return (possibility: YakuPossibility.Middle, bitIndex: num9, takameMachiHaiID: HaiID.None);
		}
		return (possibility: YakuPossibility.Low, bitIndex: 0, takameMachiHaiID: HaiID.None);
	}

	public void EvalTtyuren(in YakuInfo ret)
	{
		if (m_blocks.Count((Block x) => x.IsMelded()) > 0)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		HaiID machiHai = HaiID.None;
		for (int i = 0; i < 3; i++)
		{
			HaiMarkType markType = (HaiMarkType)i;
			if (m_counter.GetCountInMark(markType) < 10)
			{
				continue;
			}
			List<HaiID> list = new List<HaiID>();
			for (SuID suID = SuID.SU1; suID <= SuID.SU9; suID++)
			{
				HaiID haiID = HaiUtility.ToHaiID((HaiMarkType)i, suID);
				int count = m_counter.GetCount(haiID);
				if (count >= m_tyurenRequiredSuhaiCounts[(int)suID])
				{
					num++;
					num2 += m_tyurenRequiredSuhaiCounts[(int)suID];
					num3 += count - m_tyurenRequiredSuhaiCounts[(int)suID];
				}
				else
				{
					machiHai = haiID;
					num2 += count;
					list.Add(haiID);
				}
			}
			if (num3 > 0)
			{
				num2++;
			}
			KaratenInfo karaten = _karatenEvauator.EvalChurenKaraten(markType);
			if (num == 9)
			{
				Add(ret, Yaku.TyurenTanki, YakuPossibility.Established, m_blocks.FindAllIndex((Block x) => x.HaiID.GetMark() == markType), HaiID.None, HaiID.None, karaten);
			}
			else if (num >= 8 && num2 >= 12 && m_counter.GetCountInMark(markType) >= 13)
			{
				bool flag = true;
				HaiID haiID2 = HaiID.None;
				HaiID haiID3 = HaiID.None;
				for (SuID suID2 = SuID.SU1; suID2 <= SuID.SU9; suID2++)
				{
					HaiID haiID4 = HaiUtility.ToHaiID(markType, suID2);
					if (m_tyurenRequiredSuhaiCounts[(int)suID2] > m_counter[haiID4])
					{
						haiID2 = haiID4;
					}
					else if (m_tyurenRequiredSuhaiCounts[(int)suID2] < m_counter[haiID4])
					{
						haiID3 = haiID4;
					}
					if ((suID2 == SuID.SU1 || suID2 == SuID.SU9) && m_counter[haiID4] < 2)
					{
						flag = false;
					}
				}
				if (!flag)
				{
					Add(ret, Yaku.TyurenTanki, YakuPossibility.High, m_blocks.FindAllIndex((Block x) => x.HaiID.GetMark() == markType), HaiID.None, HaiID.None, karaten);
					continue;
				}
				if (haiID2 == HaiID.None)
				{
					Add(ret, Yaku.TyurenTanki, YakuPossibility.Established, m_blocks.FindAllIndex((Block x) => x.HaiID.GetMark() == markType), HaiID.None, HaiID.None, karaten);
					continue;
				}
				SuID su = haiID2.GetSu();
				Dictionary<SuID, List<SuID>> dictionary = new Dictionary<SuID, List<SuID>>
				{
					{
						SuID.SU1,
						null
					},
					{
						SuID.SU2,
						new List<SuID> { SuID.SU1 }
					},
					{
						SuID.SU3,
						new List<SuID>
						{
							SuID.SU1,
							SuID.SU4,
							SuID.SU5,
							SuID.SU7,
							SuID.SU8
						}
					},
					{
						SuID.SU4,
						new List<SuID>
						{
							SuID.SU2,
							SuID.SU3
						}
					},
					{
						SuID.SU5,
						null
					},
					{
						SuID.SU6,
						new List<SuID>
						{
							SuID.SU7,
							SuID.SU8
						}
					},
					{
						SuID.SU7,
						new List<SuID>
						{
							SuID.SU2,
							SuID.SU3,
							SuID.SU5,
							SuID.SU6,
							SuID.SU9
						}
					},
					{
						SuID.SU8,
						new List<SuID> { SuID.SU9 }
					},
					{
						SuID.SU9,
						null
					}
				};
				if (dictionary[su] != null && dictionary[su].Contains(haiID3.GetSu()))
				{
					Add(ret, Yaku.TyurenTanki, YakuPossibility.Established, m_blocks.FindAllIndex((Block x) => x.HaiID.GetMark() == markType), HaiID.None, HaiID.None, karaten);
				}
				else
				{
					Add(ret, Yaku.TyurenTanki, YakuPossibility.EstablishedInTakame, m_blocks.FindAllIndex((Block x) => x.HaiID.GetMark() == markType), machiHai, HaiID.None, karaten);
				}
			}
			else if (num >= 8 && num2 >= 11)
			{
				Add(ret, Yaku.TyurenTanki, YakuPossibility.High, m_blocks.FindAllIndex((Block x) => x.HaiID.GetMark() == markType), HaiID.None, HaiID.None, karaten);
			}
			else if (num >= 7 && num2 >= 10)
			{
				Add(ret, Yaku.TyurenTanki, YakuPossibility.Middle, m_blocks.FindAllIndex((Block x) => x.HaiID.GetMark() == markType), HaiID.None, HaiID.None, karaten);
			}
		}
	}

	private void EvalRyuiso(in YakuInfo ret)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = m_blocks.Count();
		for (int i = 0; i < num5; i++)
		{
			Block block = m_blocks[i];
			HaiID haiID = block.HaiID;
			haiID.GetSu();
			switch (block.BlockType)
			{
			case BlockType.MinKantsu:
			case BlockType.MinKoutsu:
			case BlockType.AnKantsu:
				if (haiID.IsRyuiso())
				{
					num3++;
					break;
				}
				return;
			case BlockType.MinShuntsu:
				if (haiID == HaiID.S2)
				{
					num3++;
					break;
				}
				return;
			case BlockType.AnKoutsu:
				if (haiID.IsRyuiso())
				{
					num3++;
				}
				else
				{
					num4 += 3;
				}
				break;
			case BlockType.AnShuntsu:
				switch (haiID)
				{
				case HaiID.S2:
					num3++;
					break;
				case HaiID.S1:
				case HaiID.S3:
				case HaiID.S4:
				case HaiID.S6:
					num4++;
					break;
				case HaiID.S5:
				case HaiID.S7:
					num4 += 2;
					break;
				default:
					num4 += 3;
					break;
				}
				break;
			case BlockType.Toitsu:
				if (haiID.IsRyuiso())
				{
					num++;
				}
				else
				{
					num4 += 2;
				}
				break;
			case BlockType.RyanmenTatsu:
				switch (haiID)
				{
				case HaiID.S2:
				case HaiID.S3:
					num2++;
					break;
				case HaiID.S4:
				case HaiID.S5:
				case HaiID.S6:
				case HaiID.S7:
					num4++;
					break;
				default:
					num4 += 2;
					break;
				}
				break;
			case BlockType.PenchanTatsu:
				num4 = ((haiID != HaiID.S1 && haiID != HaiID.S8) ? (num4 + 2) : (num4 + 1));
				break;
			case BlockType.KanchanTatsu:
				switch (haiID)
				{
				case HaiID.S2:
					num2++;
					break;
				case HaiID.S1:
				case HaiID.S3:
				case HaiID.S5:
					num4++;
					break;
				default:
					num4 += 2;
					break;
				}
				break;
			case BlockType.Isolated:
				if (!haiID.IsRyuiso())
				{
					num4++;
				}
				break;
			}
		}
		int num6 = CalcNormalShantenCount(num3, num, num2);
		HaiID machiHai = HaiID.None;
		KaratenInfo karaten = _karatenEvauator.EvalRyuisoKaraten();
		if (num6 == 0)
		{
			int num7 = 0;
			int num8 = 0;
			for (int j = 0; j < 34; j++)
			{
				HaiID haiID2 = (HaiID)j;
				bool flag = haiID2.IsRyuiso();
				if (m_machiTypes[j] > MachiType.None)
				{
					num7++;
					if (flag)
					{
						num8++;
						machiHai = haiID2;
					}
				}
			}
			if (num7 == num8)
			{
				Add(ret, Yaku.Ryuiso, YakuPossibility.Established, m_blocks.FindAllIndex((Block x) => x.HaiID.IsRyuiso()), HaiID.None, HaiID.None, karaten);
			}
			else
			{
				Add(ret, Yaku.Ryuiso, YakuPossibility.EstablishedInTakame, m_blocks.FindAllIndex((Block x) => x.HaiID.IsRyuiso()), machiHai, HaiID.None, karaten);
			}
		}
		else if (num4 <= 2)
		{
			Add(ret, Yaku.Ryuiso, YakuPossibility.High, m_blocks.FindAllIndex((Block x) => x.HaiID.IsRyuiso()), HaiID.None, HaiID.None, karaten);
		}
		else if (num4 <= 3)
		{
			Add(ret, Yaku.Ryuiso, YakuPossibility.Middle, m_blocks.FindAllIndex((Block x) => x.HaiID.IsRyuiso()), HaiID.None, HaiID.None, karaten);
		}
	}

	private void EvalIkkitsukan(in YakuInfo ret)
	{
		YakuPossibility yakuPossibility = YakuPossibility.Low;
		ushort requiredBlockBitFlag = 0;
		Counter counter = m_counter;
		HaiID haiID = HaiID.None;
		for (HaiMarkType haiMarkType = HaiMarkType.Manzu; haiMarkType < HaiMarkType.Jihai; haiMarkType++)
		{
			if (counter.GetCountInMark(haiMarkType) >= 7)
			{
				var (yakuPossibility2, num, haiID2) = EvalIkkitsukan(haiMarkType);
				if (yakuPossibility2 == YakuPossibility.Established)
				{
					Add(ret, Yaku.Ikkitsukan, YakuPossibility.Established, num);
					return;
				}
				if (yakuPossibility2 > yakuPossibility)
				{
					yakuPossibility = yakuPossibility2;
					requiredBlockBitFlag = num;
					haiID = haiID2;
				}
			}
		}
		Add(ret, Yaku.Ikkitsukan, yakuPossibility, requiredBlockBitFlag, haiID, HaiID.None, _karatenEvauator.EvalTakameKaraten(IsReached(), haiID));
	}

	private (YakuPossibility, ushort bitIndex, HaiID takameMachiHaiID) EvalIkkitsukan(HaiMarkType _markType)
	{
		IkkitsukanCounter[] array = new IkkitsukanCounter[3]
		{
			new IkkitsukanCounter(),
			new IkkitsukanCounter(),
			new IkkitsukanCounter()
		};
		int num = m_blocks.Count();
		HaiID item = HaiID.None;
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < num; i++)
		{
			Block block = m_blocks[i];
			HaiID haiID = block.HaiID;
			HaiMarkType mark = haiID.GetMark();
			if (mark != _markType)
			{
				BlockType blockType = block.BlockType;
				if ((uint)(blockType - 1) <= 3u)
				{
					num3++;
				}
				continue;
			}
			SuID su = haiID.GetSu();
			int num4 = -1;
			ushort num5 = (ushort)(1 << i);
			switch (block.BlockType)
			{
			case BlockType.MinKantsu:
			case BlockType.MinKoutsu:
			case BlockType.AnKantsu:
				num3++;
				break;
			case BlockType.MinShuntsu:
			case BlockType.AnShuntsu:
				num2++;
				if (su == SuID.SU1 || su == SuID.SU4 || su == SuID.SU7)
				{
					array[(int)(su - 0) / 3].MentsuBitFlag |= num5;
					break;
				}
				if (block.BlockType == BlockType.MinShuntsu)
				{
					num3++;
				}
				switch (su)
				{
				case SuID.SU2:
					num4 = 0;
					break;
				case SuID.SU3:
				case SuID.SU5:
					num4 = 1;
					break;
				case SuID.SU6:
					num4 = 2;
					break;
				}
				if (block.BlockType == BlockType.AnShuntsu && num4 >= 0 && array[num4].ZureTatsuBitFlag == 0)
				{
					array[num4].ZureTatsuBitFlag |= num5;
				}
				break;
			case BlockType.RyanmenTatsu:
				switch (su)
				{
				case SuID.SU2:
					num4 = 0;
					item = HaiUtility.ToHaiID(mark, SuID.SU1);
					break;
				case SuID.SU4:
					num4 = 1;
					item = HaiUtility.ToHaiID(mark, SuID.SU6);
					break;
				case SuID.SU5:
					num4 = 1;
					item = HaiUtility.ToHaiID(mark, SuID.SU4);
					break;
				case SuID.SU7:
					num4 = 2;
					item = HaiUtility.ToHaiID(mark, SuID.SU9);
					break;
				}
				if (num4 >= 0 && array[num4].RyanmenBitFlag == 0)
				{
					array[num4].RyanmenBitFlag |= num5;
				}
				break;
			case BlockType.PenchanTatsu:
				switch (su)
				{
				case SuID.SU1:
					num4 = 0;
					break;
				case SuID.SU8:
					num4 = 2;
					break;
				}
				if (num4 >= 0 && array[num4].KanpenBitFlag == 0)
				{
					array[num4].KanpenBitFlag |= num5;
				}
				break;
			case BlockType.KanchanTatsu:
				switch (su)
				{
				case SuID.SU1:
					num4 = 0;
					break;
				case SuID.SU4:
					num4 = 1;
					break;
				case SuID.SU7:
					num4 = 2;
					break;
				}
				if (num4 >= 0 && array[num4].KanpenBitFlag == 0)
				{
					array[num4].KanpenBitFlag |= num5;
				}
				break;
			}
		}
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		int num9 = 0;
		ushort num10 = 0;
		for (int j = 0; j < 3; j++)
		{
			IkkitsukanCounter ikkitsukanCounter = array[j];
			if (ikkitsukanCounter.MentsuBitFlag != 0)
			{
				num6++;
				num10 |= ikkitsukanCounter.MentsuBitFlag;
			}
			else if (ikkitsukanCounter.KanpenBitFlag != 0)
			{
				num7++;
				num10 |= ikkitsukanCounter.KanpenBitFlag;
			}
			else if (ikkitsukanCounter.RyanmenBitFlag != 0)
			{
				num8++;
				num10 |= ikkitsukanCounter.RyanmenBitFlag;
			}
			else if (ikkitsukanCounter.ZureTatsuBitFlag != 0)
			{
				num9++;
				num10 |= ikkitsukanCounter.ZureTatsuBitFlag;
			}
		}
		if (num6 == 3 || (num6 == 2 && num7 == 1))
		{
			if (IsTenpai())
			{
				foreach (HaiID machiHaiID in _machiHaiIDs)
				{
					if (machiHaiID.IsJihai() || machiHaiID.GetMark() != _markType)
					{
						continue;
					}
					if (machiHaiID != HaiUtility.ToHaiID(_markType, SuID.SU2) && machiHaiID != HaiUtility.ToHaiID(_markType, SuID.SU5) && machiHaiID != HaiUtility.ToHaiID(_markType, SuID.SU8) && (m_machiTypes[(int)machiHaiID] == MachiType.Tanki || m_machiTypes[(int)machiHaiID] == MachiType.Penchan))
					{
						if (num6 == 2 && num7 == 1 && (m_machiTypes[(int)machiHaiID] != MachiType.Penchan || m_counter.GetCountInMark(_markType) != 8))
						{
							return (YakuPossibility.EstablishedInTakame, bitIndex: num10, takameMachiHaiID: HaiID.None);
						}
					}
					else if ((machiHaiID == HaiUtility.ToHaiID(_markType, SuID.SU2) || machiHaiID == HaiUtility.ToHaiID(_markType, SuID.SU5) || machiHaiID == HaiUtility.ToHaiID(_markType, SuID.SU8)) && m_machiTypes[(int)machiHaiID] == MachiType.Kanchan && (m_counter[machiHaiID - 1] >= 3 || m_counter[machiHaiID + 1] >= 3))
					{
						return (YakuPossibility.EstablishedInTakame, bitIndex: num10, takameMachiHaiID: HaiID.None);
					}
				}
			}
			return (IsTenpai() ? YakuPossibility.Established : YakuPossibility.High, bitIndex: num10, takameMachiHaiID: HaiID.None);
		}
		if (num6 == 2 && num8 == 1)
		{
			return (IsTenpai() ? YakuPossibility.EstablishedInTakame : YakuPossibility.High, bitIndex: num10, takameMachiHaiID: item);
		}
		if (num6 == 1 && num7 + num8 + num9 == 2 && num3 <= 1)
		{
			return (YakuPossibility.High, bitIndex: num10, takameMachiHaiID: HaiID.None);
		}
		if (!IsTenpai() && num6 == 2 && num7 + num8 + num9 == 1)
		{
			return (YakuPossibility.Middle, bitIndex: num10, takameMachiHaiID: HaiID.None);
		}
		if (IsTenpai() && m_machiTypes.Contains(MachiType.Tanki))
		{
			HaiID haiID2 = HaiUtility.ToHaiID(_markType, SuID.SU1);
			while (haiID2 <= HaiUtility.ToHaiID(_markType, SuID.SU9) && m_counter[haiID2] > 0)
			{
				if (haiID2 == HaiUtility.ToHaiID(_markType, SuID.SU9))
				{
					return (YakuPossibility.EstablishedInTakame, bitIndex: num10, takameMachiHaiID: HaiID.None);
				}
				haiID2++;
			}
		}
		return (YakuPossibility.Low, bitIndex: 0, takameMachiHaiID: HaiID.None);
	}

	public void EvalSankantsuAndSuKantsu(in YakuInfo ret)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		ushort num5 = 0;
		ushort num6 = 0;
		List<HaiID> list = new List<HaiID>();
		List<HaiID> list2 = new List<HaiID>();
		int num7 = m_blocks.Count();
		for (int i = 0; i < num7; i++)
		{
			Block block = m_blocks[i];
			HaiID haiID = block.HaiID;
			switch (block.BlockType)
			{
			case BlockType.MinKantsu:
			case BlockType.AnKantsu:
				num++;
				num5 |= (ushort)(1 << i);
				break;
			case BlockType.MinKoutsu:
				num2++;
				list.Add(haiID);
				num6 |= (ushort)(1 << i);
				break;
			case BlockType.AnKoutsu:
				num3++;
				list2.Add(haiID);
				num6 |= (ushort)(1 << i);
				break;
			case BlockType.MinShuntsu:
				num4++;
				break;
			}
		}
		switch (num)
		{
		case 4:
			Add(ret, Yaku.Sukantsu, YakuPossibility.Established, num5);
			break;
		case 3:
			Add(ret, Yaku.Sankantsu, YakuPossibility.Established, num5);
			if (num4 == 0 && (num2 > 0 || num3 > 0))
			{
				Add(ret, Yaku.Sukantsu, YakuPossibility.High, (ushort)(num5 | num6), HaiID.None, HaiID.None, _karatenEvauator.EvalSukantsu(list, IsReached() ? list2 : null));
			}
			break;
		case 2:
			if (num4 <= 1 && (num2 > 0 || num3 > 0))
			{
				Add(ret, Yaku.Sankantsu, YakuPossibility.High, (ushort)(num5 | num6), HaiID.None, HaiID.None, _karatenEvauator.EvalSankantsu(list, IsReached() ? list2 : null, num));
			}
			break;
		}
	}

	public void EvalAnko(in YakuInfo ret)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		BlockType blockType = BlockType.None;
		HaiID haiID = HaiID.None;
		HaiID haiID2 = HaiID.None;
		HaiID machiHai = HaiID.None;
		HaiID machiHai2 = HaiID.None;
		ushort num8 = 0;
		ushort num9 = 0;
		ushort num10 = 0;
		List<HaiID> list = new List<HaiID>();
		int num11 = m_blocks.Count();
		for (int i = 0; i < num11; i++)
		{
			Block block = m_blocks[i];
			switch (block.BlockType)
			{
			case BlockType.AnKantsu:
			case BlockType.AnKoutsu:
				num++;
				num8 |= (ushort)(1 << i);
				num9 |= (ushort)(1 << i);
				list.Add(block.HaiID);
				break;
			case BlockType.MinKantsu:
			case BlockType.MinKoutsu:
				num2++;
				num8 |= (ushort)(1 << i);
				break;
			case BlockType.MinShuntsu:
				num5++;
				break;
			case BlockType.Toitsu:
				num3++;
				haiID2 = block.HaiID;
				num10 |= (ushort)(1 << i);
				if (m_machiTypes[(int)block.HaiID].HasFlag(MachiType.Shabo) && m_machiTypes[(int)block.HaiID] != MachiType.Shabo)
				{
					num6++;
				}
				switch (num3)
				{
				case 1:
					machiHai = block.HaiID;
					break;
				case 2:
					machiHai2 = block.HaiID;
					break;
				}
				break;
			case BlockType.AnShuntsu:
				num4++;
				break;
			case BlockType.RyanmenTatsu:
			case BlockType.PenchanTatsu:
			case BlockType.KanchanTatsu:
				num7++;
				haiID = block.HaiID;
				blockType = block.BlockType;
				break;
			}
		}
		bool flag = true;
		bool flag2 = true;
		bool flag3 = true;
		int num12 = num + num2;
		int num13 = num5 + num2;
		if (num5 >= 2)
		{
			return;
		}
		if (num5 >= 1)
		{
			flag = false;
			flag3 = false;
		}
		if (num13 >= 2)
		{
			flag2 = false;
			flag3 = false;
		}
		else if (num13 >= 1)
		{
			flag3 = false;
		}
		if (!flag && !flag2 && !flag3)
		{
			return;
		}
		int num14 = num4 + num13;
		bool flag4 = false;
		bool flag5 = false;
		switch (num)
		{
		case 4:
			if (IsTenpai())
			{
				Add(ret, Yaku.Suanko, YakuPossibility.Established, num9);
				Add(ret, Yaku.Sananko, YakuPossibility.Established, num9);
				flag5 = true;
			}
			else
			{
				Add(ret, Yaku.Suanko, YakuPossibility.High, num9);
				Add(ret, Yaku.Sananko, YakuPossibility.High, num9);
			}
			break;
		case 3:
			if (IsTenpai())
			{
				Add(ret, Yaku.Sananko, YakuPossibility.Established, num9);
				flag4 = true;
			}
			else
			{
				Add(ret, Yaku.Sananko, YakuPossibility.High, num9);
			}
			if (flag3 && num5 + num4 < 1)
			{
				switch (num3)
				{
				case 2:
					Add(ret, Yaku.Suanko, YakuPossibility.EstablishedInTakame, (ushort)(num9 | num10), machiHai, machiHai2);
					break;
				case 1:
					Add(ret, Yaku.Suanko, YakuPossibility.High, (ushort)(num9 | num10));
					break;
				default:
					Add(ret, Yaku.Suanko, YakuPossibility.Middle, (ushort)(num9 | num10));
					break;
				}
			}
			break;
		case 2:
			if (!flag2 || num5 + num4 >= 2)
			{
				break;
			}
			if (num3 >= 2)
			{
				if (num14 > 0)
				{
					if (num6 == 2)
					{
						Add(ret, Yaku.Sananko, YakuPossibility.Established, (ushort)(num9 | num10));
						flag4 = true;
					}
					else
					{
						Add(ret, Yaku.Sananko, YakuPossibility.EstablishedInTakame, (ushort)(num9 | num10), machiHai, machiHai2);
					}
				}
				else
				{
					Add(ret, Yaku.Sananko, YakuPossibility.High, (ushort)(num9 | num10));
				}
			}
			else if (num3 == 1)
			{
				if (IsTenpai())
				{
					Add(ret, Yaku.Sananko, YakuPossibility.Middle, (ushort)(num9 | num10));
				}
				else
				{
					Add(ret, Yaku.Sananko, YakuPossibility.High, (ushort)(num9 | num10));
				}
			}
			break;
		case 1:
			if (flag2 && num5 + num4 < 2 && num3 >= 3)
			{
				Add(ret, Yaku.Sananko, YakuPossibility.High, (ushort)(num9 | num10));
			}
			break;
		}
		if (IsTenpai())
		{
			int num15 = 0;
			List<HaiID> list2 = new List<HaiID>();
			List<HaiID> list3 = new List<HaiID>();
			for (HaiID haiID3 = HaiID.M1; haiID3 < HaiID.Max; haiID3++)
			{
				if (m_counter[haiID3] == 3)
				{
					num15++;
				}
				else if (m_counter[haiID3] == 2)
				{
					list2.Add(haiID3);
				}
				else if (m_counter[haiID3] == 1)
				{
					list3.Add(haiID3);
				}
			}
			if (!flag4 && num15 == 3 && list2.Count == 2)
			{
				if (flag2)
				{
					Add(ret, Yaku.Sananko, YakuPossibility.EstablishedInTakame, 0, list2[0], list2[1]);
				}
			}
			else if (!flag4 && num15 == 3 && list2.Count <= 1)
			{
				if (flag2)
				{
					Add(ret, Yaku.Sananko, YakuPossibility.EstablishedInTakame, 0);
				}
			}
			else if (!flag5 && num15 == 4 && list3.Count == 1 && flag3)
			{
				Debug.Log("高め四暗刻単騎");
				Add(ret, Yaku.Suanko, YakuPossibility.EstablishedInTakame, 0, list3[0]);
			}
		}
		if (!flag || num4 + num5 > 0)
		{
			return;
		}
		if (num12 == 3 && num3 == 2)
		{
			Add(ret, Yaku.Toitoiho, YakuPossibility.Established, (ushort)(num8 | num10));
			return;
		}
		switch (num12)
		{
		case 4:
		{
			YakuPossibility possibility = YakuPossibility.Established;
			HaiID haiID6 = m_blocks.FindAll((Block x) => x.BlockType == BlockType.Isolated)[0].HaiID;
			if (haiID6 != HaiID.None)
			{
				SuID su = haiID6.GetSu();
				foreach (HaiID item in list)
				{
					if (item.GetMark() == haiID6.GetMark() && ((item - 1 == haiID6 && su < SuID.SU9) || (item - 2 == haiID6 && su < SuID.SU8) || (item + 2 == haiID6 && su > SuID.SU2) || (item + 2 == haiID6 && su > SuID.SU1)))
					{
						possibility = YakuPossibility.EstablishedInTakame;
					}
				}
			}
			Add(ret, Yaku.Toitoiho, possibility, (ushort)(num8 | num10));
			return;
		}
		case 3:
			if (num3 == 1 && num7 == 1)
			{
				HaiID haiID4;
				switch (blockType)
				{
				case BlockType.RyanmenTatsu:
				case BlockType.PenchanTatsu:
					haiID4 = ((haiID == haiID2) ? (haiID + 1) : ((haiID + 1 == haiID2) ? haiID : HaiID.None));
					break;
				case BlockType.KanchanTatsu:
					haiID4 = ((haiID == haiID2) ? (haiID + 2) : ((haiID + 2 == haiID2) ? haiID : HaiID.None));
					break;
				default:
					haiID4 = HaiID.None;
					break;
				}
				HaiID haiID5 = haiID4;
				if (haiID5 != HaiID.None)
				{
					Add(ret, Yaku.Toitoiho, YakuPossibility.EstablishedInTakame, (ushort)(num8 | num10), haiID5);
				}
				else
				{
					Add(ret, Yaku.Toitoiho, YakuPossibility.High, (ushort)(num8 | num10));
				}
				return;
			}
			break;
		}
		int num16 = CalcYakuEvaluationPoint(num12, num3, 0, enableChitoitsuEvaluation: false);
		if (num16 >= 11 && num12 >= 1)
		{
			Add(ret, Yaku.Toitoiho, YakuPossibility.High, (ushort)(num8 | num10));
		}
		else if (num16 >= 9 && num12 >= 1)
		{
			Add(ret, Yaku.Toitoiho, YakuPossibility.Middle, (ushort)(num8 | num10));
		}
	}

	private void EvalSyosushiAndDaisushi(in YakuInfo ret)
	{
		if (GameDataManager.Mahjong.TakuType == TakuType.Nima || GameDataManager.Mahjong.IsJihaiUnusedSanma || m_counter.GetCountInMark(HaiMarkType.Jihai) < 7)
		{
			return;
		}
		bool flag = true;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		HaiID machiHai = HaiID.None;
		int num6 = m_blocks.Count();
		for (int i = 0; i < num6; i++)
		{
			Block block = m_blocks[i];
			HaiID haiID = block.HaiID;
			bool flag2 = haiID.IsKazehai();
			int mark = (int)haiID.GetMark();
			_ = m_blockCounts[mark];
			if (flag2 && m_machiTypes[(int)haiID] > MachiType.None)
			{
				num5++;
			}
			switch (block.BlockType)
			{
			case BlockType.MinShuntsu:
				num++;
				break;
			case BlockType.MinKantsu:
			case BlockType.MinKoutsu:
			case BlockType.AnKantsu:
				if (flag2)
				{
					num2++;
				}
				else
				{
					num++;
				}
				break;
			case BlockType.AnKoutsu:
				if (flag2)
				{
					num2++;
				}
				break;
			case BlockType.Toitsu:
				if (flag2)
				{
					num3++;
					machiHai = haiID;
				}
				break;
			case BlockType.Isolated:
				if (flag2 && m_counter[haiID] < 4)
				{
					num4++;
				}
				break;
			}
		}
		if (num >= 2)
		{
			return;
		}
		if (num == 1)
		{
			flag = false;
		}
		Dictionary<Yaku, KaratenInfo> dictionary = _karatenEvauator.EvalSushihoKaraten();
		KaratenInfo karaten = dictionary[Yaku.Daisushi];
		KaratenInfo karaten2 = dictionary[Yaku.Syosushi];
		ushort requiredBlockBitFlag = m_blocks.FindAllIndex((Block x) => x.HaiID.IsKazehai());
		if (num2 == 4)
		{
			Add(ret, Yaku.Daisushi, YakuPossibility.Established, requiredBlockBitFlag);
			return;
		}
		if ((num2 == 3 && num4 == 1) || (num2 == 2 && num3 == 2))
		{
			if (IsTenpai())
			{
				Add(ret, Yaku.Syosushi, YakuPossibility.Established, requiredBlockBitFlag, HaiID.None, HaiID.None, karaten2);
				if (flag)
				{
					Add(ret, Yaku.Daisushi, YakuPossibility.Middle, requiredBlockBitFlag, HaiID.None, HaiID.None, karaten);
				}
			}
			else
			{
				Add(ret, Yaku.Syosushi, YakuPossibility.High, requiredBlockBitFlag, HaiID.None, HaiID.None, karaten2);
				if (flag)
				{
					Add(ret, Yaku.Daisushi, YakuPossibility.High, requiredBlockBitFlag, HaiID.None, HaiID.None, karaten);
				}
			}
			return;
		}
		if (num2 == 3 && num3 == 1)
		{
			Add(ret, Yaku.Syosushi, YakuPossibility.Established, requiredBlockBitFlag);
			if (flag)
			{
				if (IsTenpai() && num5 > 0)
				{
					Add(ret, Yaku.Daisushi, YakuPossibility.EstablishedInTakame, requiredBlockBitFlag, machiHai, HaiID.None, karaten);
				}
				else
				{
					Add(ret, Yaku.Daisushi, YakuPossibility.High, requiredBlockBitFlag, HaiID.None, HaiID.None, karaten);
				}
			}
			return;
		}
		if (num2 == 3)
		{
			Add(ret, Yaku.Syosushi, YakuPossibility.High, requiredBlockBitFlag, HaiID.None, HaiID.None, karaten2);
			return;
		}
		int num7 = num2 * 3 + num3 * 2 + num4;
		if (num2 + num3 + num4 == 4 && num4 <= 1)
		{
			if (num7 >= 10)
			{
				Add(ret, Yaku.Daisushi, YakuPossibility.High, requiredBlockBitFlag, HaiID.None, HaiID.None, karaten);
			}
			else if (num7 >= 8)
			{
				Add(ret, Yaku.Daisushi, YakuPossibility.Middle, requiredBlockBitFlag, HaiID.None, HaiID.None, karaten);
			}
			if (num7 >= 9)
			{
				Add(ret, Yaku.Syosushi, YakuPossibility.High, requiredBlockBitFlag, HaiID.None, HaiID.None, karaten2);
			}
			else if (num7 >= 7)
			{
				Add(ret, Yaku.Syosushi, YakuPossibility.Middle, requiredBlockBitFlag, HaiID.None, HaiID.None, karaten2);
			}
		}
	}

	private void EvalShousangenAndDaisangen(in YakuInfo ret)
	{
		if (GameDataManager.Mahjong.IsJihaiUnusedSanma || m_counter.GetCountInMark(HaiMarkType.Jihai) < 5)
		{
			return;
		}
		bool flag = true;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = m_blocks.Count();
		for (int i = 0; i < num6; i++)
		{
			Block block = m_blocks[i];
			HaiID haiID = block.HaiID;
			bool flag2 = haiID.IsSangen();
			int mark = (int)haiID.GetMark();
			_ = m_blockCounts[mark];
			if (flag2 && m_machiTypes[(int)haiID] > MachiType.None)
			{
				num5++;
			}
			switch (block.BlockType)
			{
			case BlockType.MinShuntsu:
				num++;
				break;
			case BlockType.MinKantsu:
			case BlockType.MinKoutsu:
			case BlockType.AnKantsu:
				if (flag2)
				{
					num2++;
				}
				else
				{
					num++;
				}
				break;
			case BlockType.AnKoutsu:
				if (flag2)
				{
					num2++;
				}
				break;
			case BlockType.Toitsu:
				if (flag2)
				{
					num3++;
				}
				break;
			case BlockType.Isolated:
				if (flag2 && m_counter[haiID] < 4)
				{
					num4++;
				}
				break;
			}
		}
		if (num >= 3)
		{
			return;
		}
		if (num == 2)
		{
			flag = false;
		}
		Dictionary<Yaku, KaratenInfo> dictionary = _karatenEvauator.EvalDaiShoSangenKaraten();
		KaratenInfo karaten = dictionary[Yaku.Daisangen];
		KaratenInfo karaten2 = dictionary[Yaku.Syosangen];
		if (num2 == 3)
		{
			Add(ret, Yaku.Daisangen, YakuPossibility.Established, 0);
			return;
		}
		if ((num2 == 2 && num4 == 1) || (num2 == 1 && num3 == 2))
		{
			if (IsTenpai())
			{
				Add(ret, Yaku.Syosangen, YakuPossibility.Established, m_blocks.FindAllIndex((Block x) => x.HaiID.IsSangen()), HaiID.None, HaiID.None, karaten2);
			}
			else
			{
				Add(ret, Yaku.Syosangen, YakuPossibility.High, m_blocks.FindAllIndex((Block x) => x.HaiID.IsSangen()), HaiID.None, HaiID.None, karaten2);
			}
			if (flag)
			{
				Add(ret, Yaku.Daisangen, YakuPossibility.Middle, m_blocks.FindAllIndex((Block x) => x.HaiID.IsSangen()), HaiID.None, HaiID.None, karaten);
			}
			return;
		}
		if (num2 == 2 && num3 == 1)
		{
			Add(ret, Yaku.Syosangen, YakuPossibility.Established, m_blocks.FindAllIndex((Block x) => x.HaiID.IsSangen()));
			if (!flag)
			{
				return;
			}
			if (IsTenpai() && num5 > 0)
			{
				Add(ret, Yaku.Daisangen, YakuPossibility.EstablishedInTakame, m_blocks.FindAllIndex((Block x) => x.HaiID.IsSangen()), HaiID.None, HaiID.None, karaten);
			}
			else
			{
				Add(ret, Yaku.Daisangen, YakuPossibility.High, m_blocks.FindAllIndex((Block x) => x.HaiID.IsSangen()), HaiID.None, HaiID.None, karaten);
			}
			return;
		}
		if (num2 == 2)
		{
			Add(ret, Yaku.Syosangen, YakuPossibility.Middle, m_blocks.FindAllIndex((Block x) => x.HaiID.IsSangen()), HaiID.None, HaiID.None, karaten2);
			return;
		}
		int num7 = num2 * 3 + num3 * 2 + num4;
		if (num2 + num3 + num4 != 3 || num4 > 1)
		{
			return;
		}
		if (num7 >= 8)
		{
			Add(ret, Yaku.Daisangen, YakuPossibility.High, m_blocks.FindAllIndex((Block x) => x.HaiID.IsSangen()), HaiID.None, HaiID.None, karaten);
		}
		else if (num7 >= 5)
		{
			Add(ret, Yaku.Daisangen, YakuPossibility.Middle, m_blocks.FindAllIndex((Block x) => x.HaiID.IsSangen()), HaiID.None, HaiID.None, karaten);
		}
		if (num7 >= 7)
		{
			Add(ret, Yaku.Syosangen, YakuPossibility.High, m_blocks.FindAllIndex((Block x) => x.HaiID.IsSangen()), HaiID.None, HaiID.None, karaten2);
		}
		else if (num7 >= 5)
		{
			Add(ret, Yaku.Syosangen, YakuPossibility.Middle, m_blocks.FindAllIndex((Block x) => x.HaiID.IsSangen()), HaiID.None, HaiID.None, karaten2);
		}
	}

	private void EvalTsuisoAndChinrotoAndHonroto(in YakuInfo ret)
	{
		bool flag = !GameDataManager.Mahjong.IsJihaiUnusedSanma;
		bool flag2 = GameDataManager.Mahjong.TakuType != TakuType.Nima;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = m_blocks.Count();
		for (int i = 0; i < num7; i++)
		{
			Block block = m_blocks[i];
			HaiID haiID = block.HaiID;
			switch (block.BlockType)
			{
			case BlockType.MinKantsu:
			case BlockType.MinKoutsu:
			case BlockType.AnKantsu:
				if (haiID.IsJihai())
				{
					num4++;
					flag2 = false;
					break;
				}
				if (haiID.IsRoutou())
				{
					num++;
					flag = false;
					break;
				}
				return;
			case BlockType.AnKoutsu:
				if (haiID.IsJihai())
				{
					num4++;
				}
				else if (haiID.IsRoutou())
				{
					num++;
				}
				break;
			case BlockType.MinShuntsu:
				return;
			case BlockType.Toitsu:
				if (haiID.IsJihai())
				{
					num5++;
				}
				else if (haiID.IsRoutou())
				{
					num2++;
				}
				break;
			case BlockType.Isolated:
				if (haiID.IsJihai() && m_counter[haiID] < 4)
				{
					num6++;
				}
				else if (haiID.IsRoutou() && m_counter[haiID] < 4)
				{
					num3++;
				}
				break;
			}
		}
		Dictionary<Yaku, KaratenInfo> dictionary = _karatenEvauator.EvalTsuisoChirotoHonrotoKaraten();
		KaratenInfo karaten = dictionary[Yaku.Tsuiso];
		KaratenInfo karaten2 = dictionary[Yaku.Chinroto];
		KaratenInfo karaten3 = dictionary[Yaku.Honroto];
		if (flag)
		{
			if ((num4 == 4 && num6 == 1) || (num4 == 3 && num5 == 2) || (num5 == 6 && num6 == 1 && IsTenpai()))
			{
				Add(ret, Yaku.Tsuiso, YakuPossibility.Established, m_blocks.FindAllIndex((Block x) => x.HaiID.IsJihai()), HaiID.None, HaiID.None, karaten);
			}
			else
			{
				int num8 = num4 * 3 + num5 * 2;
				if (num8 >= 11)
				{
					Add(ret, Yaku.Tsuiso, YakuPossibility.High, m_blocks.FindAllIndex((Block x) => x.HaiID.IsJihai()), HaiID.None, HaiID.None, karaten);
				}
				else if (num8 >= 9)
				{
					Add(ret, Yaku.Tsuiso, YakuPossibility.Middle, m_blocks.FindAllIndex((Block x) => x.HaiID.IsJihai()), HaiID.None, HaiID.None, karaten);
				}
			}
		}
		if (flag2)
		{
			if ((num == 4 && num3 == 1) || (num == 3 && num2 == 2))
			{
				Add(ret, Yaku.Chinroto, YakuPossibility.Established, m_blocks.FindAllIndex((Block x) => x.HaiID.IsRoutou()), HaiID.None, HaiID.None, karaten2);
			}
			else
			{
				int num9 = num * 3 + num2 * 2;
				if (num9 >= 12)
				{
					Add(ret, Yaku.Chinroto, YakuPossibility.High, m_blocks.FindAllIndex((Block x) => x.HaiID.IsRoutou()), HaiID.None, HaiID.None, karaten2);
				}
				else if (num9 >= 10)
				{
					Add(ret, Yaku.Chinroto, YakuPossibility.Middle, m_blocks.FindAllIndex((Block x) => x.HaiID.IsRoutou()), HaiID.None, HaiID.None, karaten2);
				}
			}
		}
		if (GameDataManager.Mahjong.IsJihaiUnusedSanma)
		{
			return;
		}
		int num10 = num4 + num;
		int num11 = num5 + num2;
		int num12 = num6 + num3;
		if ((num10 == 4 && num12 == 1) || (num10 == 3 && num11 == 2) || (num11 == 6 && num12 == 1 && IsTenpai()))
		{
			Add(ret, Yaku.Honroto, YakuPossibility.Established, m_blocks.FindAllIndex((Block x) => x.HaiID.IsRoutou() || x.HaiID.IsJihai()), HaiID.None, HaiID.None, karaten3);
			return;
		}
		int num13 = num10 * 3 + num11 * 2;
		if (num13 >= 11)
		{
			Add(ret, Yaku.Honroto, YakuPossibility.High, m_blocks.FindAllIndex((Block x) => x.HaiID.IsRoutou() || x.HaiID.IsJihai()), HaiID.None, HaiID.None, karaten3);
		}
		else if (num13 >= 10)
		{
			Add(ret, Yaku.Honroto, YakuPossibility.Middle, m_blocks.FindAllIndex((Block x) => x.HaiID.IsRoutou() || x.HaiID.IsJihai()), HaiID.None, HaiID.None, karaten3);
		}
	}

	private void EvalHonitsuChinitsu(in YakuInfo ret)
	{
		bool flag = true;
		int num = m_blocks.Count();
		for (int i = 0; i < num; i++)
		{
			Block block = m_blocks[i];
			HaiID haiID = block.HaiID;
			int mark = (int)haiID.GetMark();
			BlockCounter blockCounter = m_blockCounts[mark];
			switch (block.BlockType)
			{
			case BlockType.MinKantsu:
			case BlockType.MinKoutsu:
			case BlockType.MinShuntsu:
			{
				blockCounter.MentsuCount++;
				blockCounter.BitFlag |= (ushort)(1 << i);
				if (haiID.IsJihai())
				{
					flag = false;
					break;
				}
				for (int j = 0; j < 3; j++)
				{
					if (mark != j)
					{
						m_blockCounts[j].IsAvail = false;
					}
				}
				break;
			}
			case BlockType.AnKantsu:
			case BlockType.AnKoutsu:
			case BlockType.AnShuntsu:
				blockCounter.MentsuCount++;
				blockCounter.BitFlag |= (ushort)(1 << i);
				break;
			case BlockType.Toitsu:
				blockCounter.ToitsuCount++;
				blockCounter.BitFlag |= (ushort)(1 << i);
				break;
			case BlockType.RyanmenTatsu:
			case BlockType.PenchanTatsu:
			case BlockType.KanchanTatsu:
				blockCounter.TatsuCount++;
				blockCounter.BitFlag |= (ushort)(1 << i);
				break;
			case BlockType.Isolated:
				blockCounter.IsolatedCount++;
				blockCounter.BitFlag |= (ushort)(1 << i);
				break;
			}
		}
		int num2 = -1;
		int num3 = 0;
		for (int k = 0; k < 3; k++)
		{
			BlockCounter blockCounter2 = m_blockCounts[k];
			int num4 = CalcYakuEvaluationPoint(blockCounter2.MentsuCount, blockCounter2.ToitsuCount, blockCounter2.TatsuCount);
			if (blockCounter2.IsAvail && (num2 < 0 || num4 > num3))
			{
				num3 = num4;
				num2 = k;
			}
		}
		if (num2 < 0)
		{
			return;
		}
		bool flag2 = false;
		if (flag)
		{
			if (IsTenpai() && (num3 >= 13 || (num3 == 12 && m_blockCounts[num2].IsolatedCount == 1)))
			{
				flag2 = true;
				Add(ret, Yaku.Chiniso, YakuPossibility.Established, m_blockCounts[num2].BitFlag);
			}
			else if (num3 >= 11)
			{
				Add(ret, Yaku.Chiniso, YakuPossibility.High, m_blockCounts[num2].BitFlag);
			}
			else if (num3 >= 10)
			{
				Add(ret, Yaku.Chiniso, YakuPossibility.Middle, m_blockCounts[num2].BitFlag);
			}
		}
		if (!flag2 && !GameDataManager.Mahjong.IsJihaiUnusedSanma)
		{
			BlockCounter blockCounter3 = m_blockCounts[num2];
			BlockCounter blockCounter4 = m_blockCounts[3];
			int num5 = blockCounter3.IsolatedCount + blockCounter4.IsolatedCount;
			int num6 = CalcYakuEvaluationPoint(blockCounter3.MentsuCount + blockCounter4.MentsuCount, blockCounter3.ToitsuCount + blockCounter4.ToitsuCount, blockCounter3.TatsuCount + blockCounter4.TatsuCount);
			if (IsTenpai() && (num6 >= 13 || (num6 == 12 && num5 == 1)))
			{
				Add(ret, Yaku.Honiso, YakuPossibility.Established, (ushort)(blockCounter3.BitFlag | blockCounter4.BitFlag));
			}
			else if (num6 >= 11)
			{
				Add(ret, Yaku.Honiso, YakuPossibility.High, (ushort)(blockCounter3.BitFlag | blockCounter4.BitFlag));
			}
			else if (num6 >= 10)
			{
				Add(ret, Yaku.Honiso, YakuPossibility.Middle, (ushort)(blockCounter3.BitFlag | blockCounter4.BitFlag));
			}
		}
	}

	private bool CanReach()
	{
		return m_state.CanReach;
	}

	private bool IsReached()
	{
		return m_state.IsReached();
	}

	private bool IsDoubleReached()
	{
		return m_state.IsDoubleReached();
	}

	private bool IsTenpai()
	{
		return m_isTenpai;
	}

	private bool IsYakuhai(HaiID haiID)
	{
		HaiID bakazeHaiID = GetBakazeHaiID();
		HaiID jikazeHaiID = GetJikazeHaiID();
		if (haiID != HaiID.J5 && haiID != HaiID.J6 && haiID != HaiID.J7 && haiID != bakazeHaiID)
		{
			return haiID == jikazeHaiID;
		}
		return true;
	}

	private HaiID GetBakazeHaiID()
	{
		return m_state.GetBakazeHaiID();
	}

	private HaiID GetJikazeHaiID()
	{
		return m_state.GetJikazeHaiID();
	}

	private int CalcNormalShantenCount(int mentsuCount, int toitsuCount, int tatsuCount)
	{
		int num = toitsuCount + tatsuCount;
		int num2 = ((toitsuCount > 0) ? 5 : 4) - mentsuCount;
		if (num > num2)
		{
			num = num2;
		}
		return 8 - num - 2 * mentsuCount;
	}

	private void Add(YakuInfo yakuInfo, Yaku yaku, YakuPossibility possibility, ushort requiredBlockBitFlag = 0, HaiID machiHai0 = HaiID.None, HaiID machiHai1 = HaiID.None, KaratenInfo karaten = KaratenInfo.None)
	{
		if (possibility >= YakuPossibility.Middle)
		{
			yakuInfo.YakuPossibilityInfos.Add(new YakuPossibilityInfo
			{
				Yaku = yaku,
				Possibility = possibility,
				MachiHai0ID = machiHai0,
				MachiHai1ID = machiHai1,
				RequiredBlockBitFlag = requiredBlockBitFlag,
				Blocks = m_blocks,
				Karaten = karaten
			});
		}
	}
}
