using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.MahjonPlay.Hand;
using MFG.Types;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class HaiNavi : IDisposable
{
	public List<ShantenInfo> ShantenInfos = new List<ShantenInfo>(14);

	public int PrevShanten = 13;

	public int CurShanten = 13;

	public YakuInfo m_yakuInfo = new YakuInfo();

	public Dictionary<int, List<YakuPossibilityInfo>> m_oneByOneDiscardedYakuPossibilityInfos = new Dictionary<int, List<YakuPossibilityInfo>>();

	private List<ScoreInfo> m_minScoreInfos = new List<ScoreInfo>(14);

	private ScoreInfo m_minScoreInfoWithIgnoreTsumoHai = new ScoreInfo();

	public bool IsAnalyzing;

	public TaikyokuState m_taikyokuState = new TaikyokuState();

	private bool m_enableAnalyze = true;

	private List<StructuredHand> m_handsWithIgnoreTsumoHai = new List<StructuredHand>();

	private List<List<StructuredHand>> m_oneByOneDiscardedHands = new List<List<StructuredHand>>();

	private BlockDivider m_blockDivider = new BlockDivider();

	private YakuEvaluator m_yakuEvaluator = new YakuEvaluator();

	private List<(int handIndex, StructuredHand hand, YakuPossibilityInfo possibilityInfo)> m_yakuPossilibityInfos = new List<(int, StructuredHand, YakuPossibilityInfo)>(64);

	private int[] m_normalHandCounts = new int[14];

	private int m_doraCount;

	[SerializeField]
	private GameObject m_debugInfoPanel;

	private int m_dispIndex;

	private Dictionary<Yaku, int> m_supressPossibilityInfos = new Dictionary<Yaku, int>();

	private CompositeDisposable _disposables = new CompositeDisposable();

	private Dictionary<MachiType, int> m_machiTypeDispOrders = new Dictionary<MachiType, int>
	{
		{
			MachiType.None,
			0
		},
		{
			MachiType.Other,
			7
		},
		{
			MachiType.Ryanmen,
			6
		},
		{
			MachiType.Shabo,
			5
		},
		{
			MachiType.Kanchan,
			4
		},
		{
			MachiType.Penchan,
			3
		},
		{
			MachiType.Tanki,
			2
		},
		{
			MachiType.ShantenUp,
			1
		}
	};

	private bool IsOneByOneDiscardedHandSearch
	{
		get
		{
			if (m_taikyokuState.IsMyTurn())
			{
				return !m_taikyokuState.IsReached();
			}
			return false;
		}
	}

	public Han GetHan()
	{
		return GetMinScore()?.Han ?? Han.Nothing;
	}

	private ScoreInfo GetMinScore()
	{
		if (IsOneByOneDiscardedHandSearch)
		{
			return m_minScoreInfos.Where((ScoreInfo scoreInfo) => scoreInfo.TotalPayment > 0).MinElement((ScoreInfo scoreInfo) => scoreInfo?.Han);
		}
		return m_minScoreInfoWithIgnoreTsumoHai;
	}

	private List<ScoreInfo> GetScoreInfo()
	{
		if (IsOneByOneDiscardedHandSearch)
		{
			try
			{
				return m_minScoreInfos.Where((ScoreInfo x) => x.Han == m_minScoreInfos.Where((ScoreInfo scoreInfo) => scoreInfo.TotalPayment > 0).Min((ScoreInfo y) => y.Han)).ToList();
			}
			catch
			{
				return new List<ScoreInfo>();
			}
		}
		List<ScoreInfo> list = new List<ScoreInfo>();
		if (m_minScoreInfoWithIgnoreTsumoHai.TotalPayment > 0)
		{
			list.Add(m_minScoreInfoWithIgnoreTsumoHai);
		}
		return list;
	}

	public int ToitsuCount()
	{
		if (IsOneByOneDiscardedHandSearch)
		{
			return m_oneByOneDiscardedHands.Max((List<StructuredHand> hands) => hands.Where((StructuredHand x) => x.Blocks.DivideType == DivideType.Normal).Max((StructuredHand x) => x.ToitsuCount()));
		}
		return m_handsWithIgnoreTsumoHai.Where((StructuredHand x) => x.Blocks.DivideType == DivideType.Normal).Max((StructuredHand x) => x.ToitsuCount());
	}

	public bool IsYakuhai(HaiID haiID)
	{
		return m_taikyokuState.IsYakuhai(haiID);
	}

	public void SetAnalyzeData(int playerIndex, bool isMainPlayer, List<Player> players, Wanpai wanpai, int ba, TakuType gameMode)
	{
		m_taikyokuState.Set(playerIndex, isMainPlayer, players, ba);
	}

	public async UniTask DebugAnalyze(int index)
	{
	}

	public void Reset()
	{
		m_supressPossibilityInfos.Clear();
	}

	public async UniTask Analyze(bool isTsumoAgariSelectable = false, bool isDebugAnalyze = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (IsAnalyzing)
		{
			UnityEngine.Debug.LogError("Hainavi: Analyze実行中に呼ばれた");
			return;
		}
		Stopwatch sw = new Stopwatch();
		sw.Start();
		m_taikyokuState.IsTsumoagariSelectable = isTsumoAgariSelectable;
		m_taikyokuState.NotifyAnalyze(isDebugAnalyze);
		if (m_enableAnalyze)
		{
			IsAnalyzing = true;
			try
			{
				await UniTask.Run(async delegate
				{
					await ExcecuteBlockDivide();
					EvaluateShanten();
					EvaluateYakuPossibility();
					EvaluateScore();
					EvaluateShantenUpHai();
				}, configureAwait: true, cancellationToken);
			}
			catch (OperationCanceledException ex)
			{
				UnityEngine.Debug.Log("OperationCanceledException Catched. " + ex.Message);
			}
			IsAnalyzing = false;
			GetHan();
			_ = 0 + 1;
		}
		sw.Stop();
	}

	private async UniTask ExcecuteBlockDivide()
	{
		if (IsOneByOneDiscardedHandSearch)
		{
			m_oneByOneDiscardedHands = await m_blockDivider.DivideAsOneByOneDiscarded(m_taikyokuState.HandHais(), m_taikyokuState.GetMeldBlock(), flushCache: false);
		}
		else
		{
			m_handsWithIgnoreTsumoHai = m_blockDivider.Divide(m_taikyokuState.HandHais(), m_taikyokuState.GetMeldBlock(), isIgnoreTsumoHai: true);
		}
	}

	private void EvaluateShanten()
	{
		m_taikyokuState.GetMeldBlock();
		ShantenInfos.Clear();
		PrevShanten = 13;
		CurShanten = 13;
		if (IsOneByOneDiscardedHandSearch)
		{
			foreach (List<StructuredHand> oneByOneDiscardedHand in m_oneByOneDiscardedHands)
			{
				ShantenInfo shantenInfo = new ShantenInfo();
				shantenInfo.Shanten = oneByOneDiscardedHand.Min((StructuredHand hand) => hand.ShantenCount());
				shantenInfo.NormalHandShanten = oneByOneDiscardedHand.Min((StructuredHand hand) => (hand.Blocks.DivideType != DivideType.Normal) ? 13 : hand.ShantenCount());
				shantenInfo.ChitoiHandShanten = oneByOneDiscardedHand.Min((StructuredHand hand) => (hand.Blocks.DivideType != DivideType.Chitoitsu) ? 13 : hand.ShantenCount());
				shantenInfo.KokushiHandShanten = oneByOneDiscardedHand.Min((StructuredHand hand) => (hand.Blocks.DivideType != DivideType.Kokushi) ? 13 : hand.ShantenCount());
				ShantenInfos.Add(shantenInfo);
			}
			PrevShanten = ShantenInfos[m_taikyokuState.GetTehaiCount() - 1].Shanten;
		}
		else
		{
			int num = m_taikyokuState.GetTehaiCount();
			if (m_taikyokuState.IsMyTurn())
			{
				num--;
			}
			for (int i = 0; i < num; i++)
			{
				ShantenInfos.Add(new ShantenInfo());
			}
			ShantenInfo shantenInfo2 = new ShantenInfo();
			shantenInfo2.Shanten = m_handsWithIgnoreTsumoHai.Min((StructuredHand hand) => hand.ShantenCount());
			shantenInfo2.NormalHandShanten = m_handsWithIgnoreTsumoHai.Min((StructuredHand hand) => (hand.Blocks.DivideType != DivideType.Normal) ? 13 : hand.ShantenCount());
			shantenInfo2.ChitoiHandShanten = m_handsWithIgnoreTsumoHai.Min((StructuredHand hand) => (hand.Blocks.DivideType != DivideType.Chitoitsu) ? 13 : hand.ShantenCount());
			shantenInfo2.KokushiHandShanten = m_handsWithIgnoreTsumoHai.Min((StructuredHand hand) => (hand.Blocks.DivideType != DivideType.Kokushi) ? 13 : hand.ShantenCount());
			ShantenInfos.Add(shantenInfo2);
			PrevShanten = shantenInfo2.Shanten;
		}
		foreach (ShantenInfo shantenInfo3 in ShantenInfos)
		{
			shantenInfo3.ShantenDifference = ShantenDifference.None;
			if (shantenInfo3.Shanten > PrevShanten)
			{
				shantenInfo3.ShantenDifference = ShantenDifference.Down;
			}
			else if (shantenInfo3.Shanten < PrevShanten)
			{
				shantenInfo3.ShantenDifference = ShantenDifference.Up;
			}
			if (shantenInfo3.Shanten < CurShanten)
			{
				CurShanten = shantenInfo3.Shanten;
			}
		}
	}

	public List<HaiID> MachiHais()
	{
		return m_handsWithIgnoreTsumoHai.Where((StructuredHand hand) => hand.ShantenCount() == 0).SelectMany((StructuredHand hand) => hand.AgariHaiIDs).Distinct()
			.ToList();
	}

	private List<StructuredHand> DiscardedHands(int discardedIndex)
	{
		if (IsOneByOneDiscardedHandSearch)
		{
			return m_oneByOneDiscardedHands[discardedIndex];
		}
		return m_handsWithIgnoreTsumoHai;
	}

	private void EvaluateYakuPossibility()
	{
		m_yakuPossilibityInfos.Clear();
		for (int i = 0; i < 14; i++)
		{
			m_normalHandCounts[i] = 0;
		}
		int num = 0;
		if (!IsOneByOneDiscardedHandSearch)
		{
			num = ShantenInfos.Count - 1;
		}
		for (int j = num; j < ShantenInfos.Count; j++)
		{
			ShantenInfo shantenInfo = ShantenInfos[j];
			int shantenCondition = ((shantenInfo.Shanten > 0) ? (shantenInfo.Shanten + 3) : ((!m_taikyokuState.IsReached()) ? 1 : 0));
			List<StructuredHand> list = DiscardedHands(j);
			IEnumerable<StructuredHand> enumerable = list.Where((StructuredHand hand) => hand.ShantenCount() <= shantenCondition);
			foreach (StructuredHand item in enumerable)
			{
				item.YakuInfo = m_yakuEvaluator.Evaluate(item.Blocks, item.GetMachiTypes(), m_taikyokuState, item.ShantenCount(), item.AgariHaiIDs);
			}
			if (shantenInfo.Shanten <= 0)
			{
				Dictionary<HaiID, List<StructuredHand>> dictionary = (from a in enumerable
					where a.ShantenCount() == 0
					select a into hand
					from agariHaiID in hand.AgariHaiIDs
					select new { hand, agariHaiID } into x
					group x by x.agariHaiID into x
					select new
					{
						agariHaiID = x.Key,
						hands = x.Select(y => y.hand).ToList()
					}).ToDictionary(x => x.agariHaiID, x => x.hands);
				foreach (HaiID agariHaiID2 in dictionary.Keys)
				{
					List<StructuredHand> list2 = dictionary[agariHaiID2];
					long num2 = 0L;
					StructuredHand structuredHand = null;
					foreach (StructuredHand item2 in list2)
					{
						int ignoreIndex = (IsOneByOneDiscardedHandSearch ? j : (m_taikyokuState.IsMyTurn() ? (m_taikyokuState.GetTehaiCount() - 1) : (-1)));
						item2.EvaluateScore(m_taikyokuState, m_taikyokuState.GetDoraCount(ignoreIndex));
						long num3 = 0L;
						if (item2.m_scoreInfo.Count > 0)
						{
							num3 = item2.m_scoreInfo.Where((ScoreInfo x) => x.AgariHaiID == agariHaiID2).DefaultIfEmpty().Max((ScoreInfo x) => (((x != null) ? new long?(x.TotalPayment * 100000) : null) + (long?)x?.HanCount * 1000L + x?.Fu).GetValueOrDefault());
						}
						if (num3 > num2)
						{
							num2 = num3;
							structuredHand = item2;
						}
					}
					foreach (StructuredHand item3 in list2)
					{
						long num4 = 0L;
						if (item3.m_scoreInfo.Count > 0)
						{
							num4 = item3.m_scoreInfo.Where((ScoreInfo x) => x.AgariHaiID == agariHaiID2).DefaultIfEmpty().Max((ScoreInfo x) => (((x != null) ? new long?(x.TotalPayment * 100000) : null) + (long?)x?.HanCount * 1000L + x?.Fu).GetValueOrDefault());
						}
						if (num4 < num2)
						{
							if ((GameDataManager.Mahjong.IsAtomicEnableMode ? item3.CalculatedAgariHaiIDCount : item3.AgariHaiIDs.Count) >= 2)
							{
								item3.YakuInfo.DemoteToTakame(structuredHand.YakuInfo);
								item3.AgariHaiIDs = item3.AgariHaiIDs.Except(structuredHand.AgariHaiIDs).ToList();
							}
							else
							{
								list.Remove(item3);
							}
						}
					}
				}
			}
			foreach (StructuredHand item4 in enumerable)
			{
				if (item4.Blocks.DivideType == DivideType.Normal)
				{
					m_normalHandCounts[j]++;
				}
				foreach (YakuPossibilityInfo yakuPossibilityInfo in item4.YakuInfo.YakuPossibilityInfos)
				{
					m_yakuPossilibityInfos.Add((j, item4, yakuPossibilityInfo));
				}
			}
		}
		List<Yaku> greaterEstablishedInTakameYakus = (from x in m_yakuPossilibityInfos
			where x.possibilityInfo.Possibility == YakuPossibility.Established || x.possibilityInfo.Possibility == YakuPossibility.EstablishedInTakame
			group x by x.possibilityInfo.Yaku into x
			select x.Key).ToList();
		m_supressPossibilityInfos = m_supressPossibilityInfos.Where((KeyValuePair<Yaku, int> x) => x.Value == CurShanten && !greaterEstablishedInTakameYakus.Contains(x.Key)).ToDictionary((KeyValuePair<Yaku, int> x) => x.Key, (KeyValuePair<Yaku, int> x) => x.Value);
		m_oneByOneDiscardedYakuPossibilityInfos = (from x in (from x in m_yakuPossilibityInfos
				where !m_supressPossibilityInfos.ContainsKey(x.possibilityInfo.Yaku)
				group x by new
				{
					HandIndex = x.handIndex,
					Yaku = x.possibilityInfo.Yaku
				}).Select(x =>
			{
				(int, StructuredHand, YakuPossibilityInfo) tuple = x.MaxElement(((int handIndex, StructuredHand hand, YakuPossibilityInfo possibilityInfo) y) => (y.hand.Blocks.DivideType == DivideType.Normal) ? y.possibilityInfo.Possibility : YakuPossibility.None);
				if (tuple.Item2.Blocks.DivideType == DivideType.Normal && tuple.Item3.Possibility == YakuPossibility.Established)
				{
					foreach (var item5 in x.Where(((int handIndex, StructuredHand hand, YakuPossibilityInfo possibilityInfo) _) => x.Key.Yaku == Yaku.Sananko))
					{
						UnityEngine.Debug.Log($"Sananko Possibilities : {item5.possibilityInfo.Possibility}");
					}
					if ((m_normalHandCounts[x.Key.HandIndex] != x.Count(((int handIndex, StructuredHand hand, YakuPossibilityInfo possibilityInfo) z) => z.hand.Blocks.DivideType == DivideType.Normal) || tuple.Item3.Possibility != YakuPossibility.Established || x.Any(((int handIndex, StructuredHand hand, YakuPossibilityInfo possibilityInfo) y) => y.hand.Blocks.DivideType == DivideType.Normal && y.possibilityInfo.Possibility <= YakuPossibility.EstablishedInTakame)) && x.Any(((int handIndex, StructuredHand hand, YakuPossibilityInfo possibilityInfo) y) => (y.hand.ShantenCount() == 0 && y.hand.Blocks.DivideType == DivideType.Normal && y.possibilityInfo.Possibility <= YakuPossibility.EstablishedInTakame) || !x.Any(((int handIndex, StructuredHand hand, YakuPossibilityInfo possibilityInfo) y) => y.hand.ShantenCount() == 0)))
					{
						tuple.Item3.Possibility = YakuPossibility.EstablishedInTakame;
					}
				}
				return new
				{
					HandIndex = x.Key.HandIndex,
					YakuPossibilityInfo = tuple.Item3
				};
			})
			group x.YakuPossibilityInfo by x.HandIndex into x
			select new
			{
				HandIndex = x.Key,
				Possibility = x.ToList()
			}).ToDictionary(x => x.HandIndex, x => x.Possibility);
		List<Yaku> list3 = new List<Yaku>();
		foreach (int key in m_oneByOneDiscardedYakuPossibilityInfos.Keys)
		{
			List<Yaku> collection = (from x in m_oneByOneDiscardedYakuPossibilityInfos[key]
				where x.Karaten == KaratenInfo.None
				select x.Yaku).ToList();
			list3.AddRange(collection);
		}
		list3 = list3.Distinct().ToList();
		foreach (int key2 in m_oneByOneDiscardedYakuPossibilityInfos.Keys)
		{
			List<YakuPossibilityInfo> infos = m_oneByOneDiscardedYakuPossibilityInfos[key2];
			if (list3.Count > 0)
			{
				foreach (Yaku item6 in list3)
				{
					YakuInfo.DemoteOnebyoneKaraten(ref infos, item6);
				}
			}
			YakuInfo.DemoteSubordinateYaku(ref infos);
		}
		List<YakuPossibilityInfo> yakuPossibilityInfos = m_yakuInfo.YakuPossibilityInfos;
		m_yakuInfo.YakuPossibilityInfos = (from x in m_oneByOneDiscardedYakuPossibilityInfos.SelectMany((KeyValuePair<int, List<YakuPossibilityInfo>> x) => x.Value)
			group x by x.Yaku into x
			select x.MaxElement((YakuPossibilityInfo y) => y.Possibility)).ToList();
		if (m_taikyokuState.IsReached() || m_taikyokuState.IsTsumoagariSelectable)
		{
			List<YakuPossibilityInfo> list4 = new List<YakuPossibilityInfo>();
			if (!m_taikyokuState.IsReached() && m_taikyokuState.IsTsumoagariSelectable && IsOneByOneDiscardedHandSearch)
			{
				try
				{
					list4 = m_oneByOneDiscardedYakuPossibilityInfos[m_oneByOneDiscardedYakuPossibilityInfos.Keys.Max()].Where((YakuPossibilityInfo x) => x.Possibility != YakuPossibility.Established && x.Possibility != YakuPossibility.EstablishedInTakame).ToList();
					YakuInfo.TsumoMinogashiDemote(ref m_yakuInfo.YakuPossibilityInfos, list4.Select((YakuPossibilityInfo x) => x.Yaku).ToList());
				}
				catch (InvalidOperationException ex)
				{
					UnityEngine.Debug.Log("InvalidOperationException -> " + ex.ToString());
				}
			}
			foreach (int key3 in m_oneByOneDiscardedYakuPossibilityInfos.Keys)
			{
				m_oneByOneDiscardedYakuPossibilityInfos[key3].RemoveAll((YakuPossibilityInfo x) => x.Possibility != YakuPossibility.Established && x.Possibility != YakuPossibility.EstablishedInTakame);
			}
			m_yakuInfo.YakuPossibilityInfos.RemoveAll((YakuPossibilityInfo x) => x.Possibility != YakuPossibility.Established && x.Possibility != YakuPossibility.EstablishedInTakame);
			YakuInfo.DemoteSubordinateYaku(ref m_yakuInfo.YakuPossibilityInfos);
			if (m_taikyokuState.IsTsumoagariSelectable)
			{
				YakuInfo.DemoteKaratenInfo(ref m_yakuInfo.YakuPossibilityInfos);
			}
		}
		List<Yaku> list5 = new List<Yaku>();
		foreach (YakuPossibilityInfo info in yakuPossibilityInfos)
		{
			if (m_yakuInfo.YakuPossibilityInfos.FindIndex((YakuPossibilityInfo x) => x.Yaku == info.Yaku && x.Possibility >= info.Possibility) < 0)
			{
				list5.Add(info.Yaku);
			}
		}
		foreach (Yaku item7 in list5)
		{
			if (!m_supressPossibilityInfos.ContainsKey(item7))
			{
				m_supressPossibilityInfos.Add(item7, CurShanten);
			}
		}
	}

	public List<Block> GetCompletedMentsu(int discardedPos)
	{
		int tehaiCount = m_taikyokuState.GetTehaiCount();
		List<Block> list = null;
		if (discardedPos >= 0)
		{
			List<StructuredHand> list2 = m_oneByOneDiscardedHands[tehaiCount - 1];
			List<StructuredHand> list3 = m_oneByOneDiscardedHands[discardedPos];
			List<Block> list4 = new List<Block>();
			List<Block> list5 = new List<Block>();
			foreach (StructuredHand item in list2)
			{
				list4.AddRange(item.Blocks.FindAll((Block x) => x.BlockType == BlockType.AnKoutsu || x.BlockType == BlockType.AnShuntsu || x.BlockType == BlockType.MinKoutsu || x.BlockType == BlockType.MinShuntsu));
			}
			foreach (StructuredHand item2 in list3)
			{
				list5.AddRange(item2.Blocks.FindAll((Block x) => x.BlockType == BlockType.AnKoutsu || x.BlockType == BlockType.AnShuntsu || x.BlockType == BlockType.MinKoutsu || x.BlockType == BlockType.MinShuntsu));
			}
			list = list5.Except(list4, new BlockComparer()).ToList();
		}
		else
		{
			List<Block> list6 = new List<Block>();
			foreach (StructuredHand item3 in m_handsWithIgnoreTsumoHai)
			{
				list6.AddRange(item3.Blocks.FindAll((Block x) => x.BlockType == BlockType.AnKoutsu || x.BlockType == BlockType.AnShuntsu || x.BlockType == BlockType.MinKoutsu || x.BlockType == BlockType.MinShuntsu));
			}
			list = list6.Distinct().ToList();
		}
		list.Sort();
		return list;
	}

	public bool IsBackfired(HaiID tsumoHaiID)
	{
		Tile lastTile = m_taikyokuState.Dump().GetLastTile();
		if (lastTile == null)
		{
			return false;
		}
		return IsBackfired(lastTile.pai.ToHaiID(), tsumoHaiID);
	}

	public bool IsBackfired(HaiID haiA, HaiID haiB)
	{
		HaiMarkType mark = haiA.GetMark();
		HaiMarkType mark2 = haiB.GetMark();
		if (mark != mark2)
		{
			return false;
		}
		if (mark == HaiMarkType.Jihai)
		{
			if (haiA == haiB)
			{
				return true;
			}
		}
		else
		{
			SuID su = haiA.GetSu();
			SuID su2 = haiB.GetSu();
			if (su == su2)
			{
				return true;
			}
			if (su >= SuID.SU3 && su - 2 == su2)
			{
				return true;
			}
			if (su >= SuID.SU2 && su - 1 == su2)
			{
				return true;
			}
			if (su <= SuID.SU8 && su + 1 == su2)
			{
				return true;
			}
			if (su <= SuID.SU7 && su + 2 == su2)
			{
				return true;
			}
		}
		return false;
	}

	private int GetMachiTypeDispOrder(MachiType machiType)
	{
		int value = -1;
		m_machiTypeDispOrders.TryGetValue(machiType, out value);
		return value;
	}

	private void EvaluateShantenUpHai()
	{
		int num = m_oneByOneDiscardedHands.Count;
		if (!IsOneByOneDiscardedHandSearch)
		{
			num = 1;
		}
		for (int i = 0; i < num; i++)
		{
			ShantenInfo shantenInfo = ShantenInfos[i];
			if (!IsOneByOneDiscardedHandSearch)
			{
				shantenInfo = ShantenInfos[ShantenInfos.Count - 1];
			}
			shantenInfo.MachiType = MachiType.None;
			List<StructuredHand> list = DiscardedHands(i);
			MachiType[] array = new MachiType[34];
			int count = list.Count;
			for (int j = 0; j < count; j++)
			{
				StructuredHand structuredHand = list[j];
				MachiType[] machiTypes = structuredHand.GetMachiTypes();
				if (structuredHand.ShantenCount() == 0 && GetMachiTypeDispOrder(structuredHand.AgariMachiType) > GetMachiTypeDispOrder(shantenInfo.MachiType))
				{
					shantenInfo.MachiType = structuredHand.AgariMachiType;
				}
				if (structuredHand.ShantenCount() <= shantenInfo.Shanten && machiTypes != null)
				{
					for (int k = 0; k < 34; k++)
					{
						array[k] |= machiTypes[k];
					}
				}
			}
			for (int l = 0; l < 34; l++)
			{
				HaiID item = (HaiID)l;
				if (array[l] > MachiType.None)
				{
					shantenInfo.ShantenUpHais.Add(item);
				}
			}
			if (shantenInfo.Shanten == 0)
			{
				if (shantenInfo.ShantenUpHais.Count >= 3)
				{
					shantenInfo.MachiType = MachiType.Other;
				}
				else if (shantenInfo.MachiType != MachiType.Ryanmen && shantenInfo.MachiType != MachiType.Shabo && shantenInfo.MachiType != MachiType.Kanchan && shantenInfo.MachiType != MachiType.Penchan && shantenInfo.MachiType != MachiType.Tanki)
				{
					UnityEngine.Debug.LogWarning($"牌ナビ/想定外の待ちフラグ {shantenInfo.MachiType}");
				}
			}
		}
	}

	private void EvaluateScore()
	{
		m_minScoreInfos.Clear();
		m_minScoreInfoWithIgnoreTsumoHai = new ScoreInfo();
		if (IsOneByOneDiscardedHandSearch)
		{
			int count = m_oneByOneDiscardedHands.Count;
			for (int i = 0; i < count; i++)
			{
				_ = ShantenInfos[i];
				List<StructuredHand> hands2 = m_oneByOneDiscardedHands[i];
				(ScoreInfo, ScoreInfo) tuple = EvaluateScore(hands2, i);
				m_minScoreInfos.Add(tuple.Item1);
				if (i == count - 1)
				{
					(m_minScoreInfoWithIgnoreTsumoHai, _) = tuple;
				}
			}
		}
		else
		{
			int num = m_taikyokuState.GetTehaiCount();
			(ScoreInfo, ScoreInfo) tuple3 = EvaluateScore(m_handsWithIgnoreTsumoHai, m_taikyokuState.IsMyTurn() ? (num - 1) : (-1));
			if (m_taikyokuState.IsMyTurn())
			{
				num--;
			}
			for (int j = 0; j < num; j++)
			{
				m_minScoreInfos.Add(new ScoreInfo());
			}
			m_minScoreInfos.Add(tuple3.Item1);
			(m_minScoreInfoWithIgnoreTsumoHai, _) = tuple3;
		}
		(ScoreInfo min, ScoreInfo max) EvaluateScore(List<StructuredHand> hands, int discardedIndex = -1)
		{
			int count2 = hands.Count;
			ScoreInfo scoreInfo2 = null;
			ScoreInfo scoreInfo3 = null;
			for (int k = 0; k < count2; k++)
			{
				List<ScoreInfo> list = hands[k].EvaluateScore(m_taikyokuState, m_taikyokuState.GetDoraCount(discardedIndex));
				if (list.Count != 0)
				{
					ScoreInfo scoreInfo4 = list.MaxElement((ScoreInfo scoreInfo) => scoreInfo.TotalPayment);
					if (scoreInfo2 == null || (scoreInfo4 != null && scoreInfo4.TotalPayment > scoreInfo2.TotalPayment))
					{
						scoreInfo2 = scoreInfo4;
					}
					scoreInfo4 = list.MinElement((ScoreInfo scoreInfo) => scoreInfo.TotalPayment);
					if (scoreInfo3 == null || (scoreInfo4 != null && scoreInfo4.TotalPayment < scoreInfo3.TotalPayment))
					{
						scoreInfo3 = scoreInfo4;
					}
				}
			}
			if (scoreInfo3 == null)
			{
				scoreInfo3 = new ScoreInfo();
			}
			if (scoreInfo2 == null)
			{
				scoreInfo2 = new ScoreInfo();
			}
			return (min: scoreInfo3, max: scoreInfo2);
		}
	}

	private void DebugHanDisp()
	{
		if (m_debugInfoPanel == null)
		{
			return;
		}
		bool flag = DebugFlag.IsActive(DebugFlag.Flag.HanDisp);
		m_debugInfoPanel.SetActive(flag);
		if (!flag || !m_taikyokuState.IsMainPlayer)
		{
			return;
		}
		Text component = m_debugInfoPanel.transform.Find("HanInfoText").GetComponent<Text>();
		component.text = string.Empty;
		List<ScoreInfo> scoreInfo = GetScoreInfo();
		component.text += ((scoreInfo.Count <= 0) ? "候補なし\n" : $"{m_dispIndex + 1}/{scoreInfo.Count}\n");
		if (scoreInfo.Count <= 0)
		{
			return;
		}
		if (m_dispIndex >= scoreInfo.Count)
		{
			m_dispIndex = scoreInfo.Count - 1;
		}
		ScoreInfo scoreInfo2 = scoreInfo[m_dispIndex];
		if (scoreInfo2 == null || scoreInfo2.Han == Han.Nothing)
		{
			return;
		}
		foreach (Yaku yaku in scoreInfo2.Yakus)
		{
			component.text = component.text + MahjongUtility.GetYakuName(yaku) + "\n";
		}
		if (scoreInfo2.DoraCount > 0)
		{
			component.text += $"ドラ:{scoreInfo2.DoraCount}\n";
		}
		component.text += ((scoreInfo2.Han >= Han.Mangan) ? (" " + MahjongUtility.GetHanName(scoreInfo2.Han) + "\n") : $"{scoreInfo2.Fu}符 {MahjongUtility.GetHanName(scoreInfo2.Han)}\n");
		component.text += $"和了牌:{scoreInfo2.AgariHaiID}\n";
		component.text += (scoreInfo2.IsTsumo ? "ツモ和了\n" : "ロン和了\n");
		component.text += "得点:";
		component.text += ((!scoreInfo2.IsTsumo) ? $"{scoreInfo2.TotalPayment}" : (scoreInfo2.IsOya ? $"{scoreInfo2.KoPaymentByTsumo} ALL" : $"{scoreInfo2.OyaPaymentByTsumo}/{scoreInfo2.KoPaymentByTsumo}"));
		component.text += "\n";
	}

	public void Dispose()
	{
		_disposables.Dispose();
	}
}
