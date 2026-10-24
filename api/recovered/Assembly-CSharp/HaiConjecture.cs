using System;
using System.Collections.Generic;
using System.Linq;
using MFG.Types;
using MFG.Types.HaiConjecture;
using UniRx;
using UnityEngine;

public class HaiConjecture : MonoBehaviour
{
	[Header("Cha の順に格納してください")]
	[SerializeField]
	private List<Hand> hands = new List<Hand>();

	private List<(Cha cha, Pai pai)> dumps = new List<(Cha, Pai)>();

	private Dictionary<Cha, int> reachIndex = new Dictionary<Cha, int>();

	private List<HaiConjectureList> conjectureList = new List<HaiConjectureList>();

	[SerializeField]
	private Subject<HaiConjectureList> onMyConjectureChanged = new Subject<HaiConjectureList>();

	public IObservable<HaiConjectureList> OnMyConjectureChanged => onMyConjectureChanged.AsObservable();

	private void Start()
	{
		if (hands.Count <= 0)
		{
			return;
		}
		for (int i = 0; i <= 3; i++)
		{
			conjectureList.Add(new HaiConjectureList());
		}
		foreach (Hand hand in hands)
		{
			hand.OnAddTsumoHai.Subscribe(delegate((Pai pai, Cha cha) e)
			{
				Debug.Log($"Add TsumoHai : {e.cha} {e.pai}\n");
				conjectSafeHaiAtTsumohai(e.cha);
				conjectDangerHaiAtTsumohai(e.cha);
				onMyConjectureChanged.OnNext(conjectureList[0]);
			}).AddTo(this);
			hand.OnDiscardTsumoHai.Subscribe(delegate((Pai pai, Cha cha) e)
			{
				(Cha, Pai) item = (e.cha, MahjongUtility.AkaToNormal(e.pai));
				dumps.Add(item);
				Debug.Log($"Discard TsumoHai : {item.Item1} {item.Item2}\n");
				conjectSafeHaiAtSutehai(e.cha);
				conjectDangerHaiAtSutehai(e.cha);
			}).AddTo(this);
			hand.OnStartReach.Subscribe(delegate(Cha cha)
			{
				Debug.Log($"Reach Start : {cha}, {dumps?.Count ?? (-1)}\n");
				if (!reachIndex.ContainsKey(cha))
				{
					reachIndex[cha] = dumps.Count;
				}
			}).AddTo(this);
			hand.OnTurnComing.Subscribe(delegate(Cha cha)
			{
				Debug.Log($"Turn Coming : {cha}\n");
				conjectSafeHaiAtTsumohai(cha);
				conjectDangerHaiAtTsumohai(cha);
				onMyConjectureChanged.OnNext(conjectureList[0]);
			}).AddTo(this);
		}
	}

	public void Reset()
	{
		dumps.Clear();
		reachIndex.Clear();
		foreach (HaiConjectureList conjecture in conjectureList)
		{
			conjecture.Safe.Clear();
			conjecture.SafeOnlyReach.Clear();
			conjecture.Danger.Clear();
		}
		onMyConjectureChanged.OnNext(conjectureList[0]);
	}

	public bool IsTargetForConject(Cha _cha, Cha _myCha)
	{
		return !isIgnoreConject(_cha, _myCha, isOnlyReach: true);
	}

	public List<Cha> IsSafeChaList(Pai _pai, Cha _myCha)
	{
		Pai pai = _pai.ToNormal();
		return (from pair in conjectureList[(int)_myCha].Safe
			where pair.Value.Contains(pai)
			select pair.Key).ToList();
	}

	public List<Cha> IsSafeOnlyReachChaList(Pai _pai, Cha _myCha)
	{
		Pai pai = _pai.ToNormal();
		return (from pair in conjectureList[(int)_myCha].SafeOnlyReach
			where pair.Value.Contains(pai)
			select pair.Key).ToList();
	}

	public List<Cha> IsDangerChaList(Pai _pai, Cha _myCha)
	{
		Pai pai = _pai.ToNormal();
		return (from pair in conjectureList[(int)_myCha].Danger
			where pair.Value.Contains(pai)
			select pair.Key).ToList();
	}

	public void conjectSafeAtOption()
	{
		if (hands[0].IsNowTsumoChoice())
		{
			conjectSafeHaiAtTsumohai(Cha.Jicha);
			conjectDangerHaiAtTsumohai(Cha.Jicha);
			onMyConjectureChanged.OnNext(conjectureList[0]);
		}
	}

	private void conjectSafeHaiAtTsumohai(Cha turn)
	{
		conjectSafeHaiAtSutehai(turn);
	}

	private void conjectSafeHaiAtSutehai(Cha turn)
	{
		if (dumps.Count <= 0)
		{
			return;
		}
		foreach (HaiConjectureList conjecture in conjectureList)
		{
			conjecture.Safe.Clear();
		}
		Cha cha;
		for (cha = Cha.Jicha; cha <= Cha.Kamicha; cha++)
		{
			List<Pai> list = (from p in dumps.Where(((Cha cha, Pai pai) e) => e.cha == cha && e.pai != Pai.NONE).Distinct()
				select p.pai).ToList();
			if (list == null)
			{
				continue;
			}
			int myCha3;
			for (myCha3 = 0; myCha3 <= 3; myCha3++)
			{
				if (!isIgnoreConject(cha, (Cha)myCha3))
				{
					List<Pai> list2 = list.Where((Pai p) => !hands[myCha3].GetPaiPosAll(p).Contains(-1)).ToList();
					if (list2 == null)
					{
						continue;
					}
					if (!conjectureList[myCha3].Safe.ContainsKey(cha))
					{
						HashSet<Pai> value = new HashSet<Pai>(list2);
						conjectureList[myCha3].Safe.Add(cha, value);
					}
					else
					{
						conjectureList[myCha3].Safe[cha].UnionWith(list2);
					}
				}
				if (isIgnoreConject(cha, (Cha)myCha3, isOnlyReach: true))
				{
					continue;
				}
				List<Pai> list3 = list.Where((Pai p) => !hands[myCha3].GetPaiPosAll(p).Contains(-1)).ToList();
				if (list3 != null)
				{
					if (!conjectureList[myCha3].SafeOnlyReach.ContainsKey(cha))
					{
						HashSet<Pai> value2 = new HashSet<Pai>(list3);
						conjectureList[myCha3].SafeOnlyReach.Add(cha, value2);
					}
					else
					{
						conjectureList[myCha3].SafeOnlyReach[cha].UnionWith(list3);
					}
				}
			}
		}
		if (reachIndex.Count > 0)
		{
			foreach (KeyValuePair<Cha, int> item in reachIndex)
			{
				List<Pai> list4 = (from p in dumps.Skip(item.Value).Distinct()
					select p.pai).ToList();
				if (list4 == null)
				{
					continue;
				}
				int myCha2;
				for (myCha2 = 0; myCha2 <= 3; myCha2++)
				{
					if (!isIgnoreConject(item.Key, (Cha)myCha2))
					{
						List<Pai> list5 = list4.Where((Pai p) => !hands[myCha2].GetPaiPosAll(p).Contains(-1)).ToList();
						if (list5 == null)
						{
							continue;
						}
						if (!conjectureList[myCha2].Safe.ContainsKey(item.Key))
						{
							HashSet<Pai> value3 = new HashSet<Pai>(list5);
							conjectureList[myCha2].Safe.Add(item.Key, value3);
						}
						else
						{
							conjectureList[myCha2].Safe[item.Key].UnionWith(list5);
						}
					}
					if (isIgnoreConject(item.Key, (Cha)myCha2, isOnlyReach: true))
					{
						continue;
					}
					List<Pai> list6 = list4.Where((Pai p) => !hands[myCha2].GetPaiPosAll(p).Contains(-1)).ToList();
					if (list6 != null)
					{
						if (!conjectureList[myCha2].SafeOnlyReach.ContainsKey(item.Key))
						{
							HashSet<Pai> value4 = new HashSet<Pai>(list6);
							conjectureList[myCha2].SafeOnlyReach.Add(item.Key, value4);
						}
						else
						{
							conjectureList[myCha2].SafeOnlyReach[item.Key].UnionWith(list6);
						}
					}
				}
			}
		}
		for (Cha cha2 = Cha.Jicha; cha2 <= Cha.Kamicha; cha2++)
		{
			List<Pai> list7 = new List<Pai>();
			int num = dumps.Count - 1;
			while (num >= 0 && dumps[num].cha != cha2)
			{
				list7.Add(dumps[num].pai);
				num--;
			}
			if (list7.Count == 0)
			{
				continue;
			}
			int myCha;
			for (myCha = 0; myCha <= 3; myCha++)
			{
				if (!isIgnoreConject(cha2, (Cha)myCha))
				{
					List<Pai> list8 = list7.Where((Pai p) => !hands[myCha].GetPaiPosAll(p).Contains(-1)).ToList();
					if (list8 == null)
					{
						continue;
					}
					if (!conjectureList[myCha].Safe.ContainsKey(cha2))
					{
						HashSet<Pai> value5 = new HashSet<Pai>(list8);
						conjectureList[myCha].Safe.Add(cha2, value5);
					}
					else
					{
						conjectureList[myCha].Safe[cha2].UnionWith(list8);
					}
				}
				if (isIgnoreConject(cha2, (Cha)myCha, isOnlyReach: true))
				{
					continue;
				}
				List<Pai> list9 = list7.Where((Pai p) => !hands[myCha].GetPaiPosAll(p).Contains(-1)).ToList();
				if (list9 != null)
				{
					if (!conjectureList[myCha].SafeOnlyReach.ContainsKey(cha2))
					{
						HashSet<Pai> value6 = new HashSet<Pai>(list9);
						conjectureList[myCha].SafeOnlyReach.Add(cha2, value6);
					}
					else
					{
						conjectureList[myCha].SafeOnlyReach[cha2].UnionWith(list9);
					}
				}
			}
		}
		for (int i = 0; i <= 3; i++)
		{
			for (Cha cha3 = Cha.Jicha; cha3 <= Cha.Kamicha; cha3++)
			{
				if (conjectureList[i].Safe.ContainsKey(cha3))
				{
					List<Pai> list10 = conjectureList[i].Safe[cha3].ToList();
					list10.Sort(Hand.ComparePai);
					conjectureList[i].Safe[cha3] = new HashSet<Pai>(list10);
				}
				if (conjectureList[i].SafeOnlyReach.ContainsKey(cha3))
				{
					List<Pai> list11 = conjectureList[i].SafeOnlyReach[cha3].ToList();
					list11.Sort(Hand.ComparePai);
					conjectureList[i].SafeOnlyReach[cha3] = new HashSet<Pai>(list11);
				}
			}
		}
	}

	private void conjectDangerHaiAtTsumohai(Cha turn)
	{
		conjectDangerHaiAtSutehai(turn);
	}

	private void conjectDangerHaiAtSutehai(Cha turn)
	{
		if (dumps.Count <= 0)
		{
			return;
		}
		foreach (HaiConjectureList conjecture in conjectureList)
		{
			conjecture.Danger.Clear();
		}
		Cha cha;
		for (cha = Cha.Jicha; cha <= Cha.Kamicha; cha++)
		{
			List<Pai> list = HaiConjectureUtil.CalcMusujiHaiFromSujiList((from p in (from e in dumps.Where(((Cha cha, Pai pai) e) => e.cha == cha).Distinct()
					select e.pai).SelectMany((Pai p) => HaiConjectureUtil.GetSujiHai(p))
				where p != Pai.NONE
				select p).Distinct().ToList()).ToList();
			if (list == null)
			{
				continue;
			}
			int myCha;
			for (myCha = 0; myCha <= 3; myCha++)
			{
				if (isIgnoreConject(cha, (Cha)myCha))
				{
					continue;
				}
				List<Pai> list2 = (from p in list
					where !hands[myCha].GetPaiPosAll(p).Contains(-1)
					where !conjectureList[myCha].Safe.ContainsKey(cha) || !conjectureList[myCha].Safe[cha].Contains(p)
					select p).ToList();
				if (list2 != null)
				{
					if (!conjectureList[myCha].Danger.ContainsKey(cha))
					{
						HashSet<Pai> value = new HashSet<Pai>(list2);
						conjectureList[myCha].Danger.Add(cha, value);
					}
					else
					{
						conjectureList[myCha].Danger[cha].UnionWith(list2);
					}
				}
			}
		}
		for (int i = 0; i <= 3; i++)
		{
			for (Cha cha2 = Cha.Jicha; cha2 <= Cha.Kamicha; cha2++)
			{
				if (conjectureList[i].Danger.ContainsKey(cha2))
				{
					List<Pai> list3 = conjectureList[i].Danger[cha2].ToList();
					list3.Sort(Hand.ComparePai);
					conjectureList[i].Danger[cha2] = new HashSet<Pai>(list3);
				}
			}
		}
	}

	private bool isIgnoreConject(Cha cha, Cha myCha, bool isOnlyReach = false)
	{
		if (cha == myCha)
		{
			return true;
		}
		switch (isOnlyReach ? ConjectureType.OnlyReach : GameDataManager.PlayerConfig.ConjectureType)
		{
		case ConjectureType.OnlyReach:
			return !reachIndex.ContainsKey(cha);
		case ConjectureType.ReachMeld1:
		case ConjectureType.ReachMeld2:
		case ConjectureType.ReachMeld3:
		case ConjectureType.ReachMeld4:
			return !reachIndex.ContainsKey(cha) && hands[(int)cha].MeldCount() < (int)(GameDataManager.PlayerConfig.ConjectureType - 1);
		default:
			return false;
		}
	}
}
