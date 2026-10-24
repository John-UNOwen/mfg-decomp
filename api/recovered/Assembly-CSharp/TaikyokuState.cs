using System.Collections.Generic;
using MFG.MahjonPlay.Hand;
using MFG.Types;

public class TaikyokuState
{
	public class SnapShot
	{
		private int m_handItemCount;

		private HaiID[] m_hand = new HaiID[14];

		private Counter m_handHaiCounter = new Counter();

		private Counter m_handHaiCounterWithIgnoreIndex = new Counter();

		private BlockSet m_meldBlock = new BlockSet();

		private bool m_isReached;

		private bool m_isDoubleReached;

		private bool m_isTsumoagariSelectable;

		private int m_ba;

		private Kaze m_jikaze;

		private int m_doraCount;

		public void Create(Player myPlayer, int ba, Kaze jikaze)
		{
			m_isReached = myPlayer.m_isReach;
			m_isDoubleReached = myPlayer.m_isDoubleReach;
			m_ba = ba;
			m_jikaze = jikaze;
			m_handItemCount = myPlayer.hand.GetItemCount();
			m_hand = new HaiID[m_handItemCount];
			for (int i = 0; i < m_handItemCount; i++)
			{
				m_hand[i] = myPlayer.hand.GetTile(i).pai.ToHaiID();
			}
			m_handHaiCounter = myPlayer.hand.GetHaiCounter();
			m_handHaiCounterWithIgnoreIndex.Copy(m_handHaiCounter);
			m_meldBlock = myPlayer.meld.GetBlockSet().Clone();
			m_doraCount = myPlayer.GetDoraCount();
		}

		public void Create(HaiID[] hand, BlockSet meld, bool isReached, bool isTsumoAgariSelectable, int ba, Kaze jikaze, int doraCount)
		{
			m_handItemCount = hand.Length;
			m_hand = new HaiID[m_handItemCount];
			for (int i = 0; i < m_handItemCount; i++)
			{
				m_hand[i] = hand[i];
			}
			m_handHaiCounter = Counter.Create(hand);
			m_handHaiCounterWithIgnoreIndex.Copy(m_handHaiCounter);
			m_meldBlock = meld.Clone();
			m_isReached = isReached;
			m_isTsumoagariSelectable = isTsumoAgariSelectable;
			m_ba = ba;
			m_jikaze = jikaze;
			m_doraCount = doraCount;
		}

		public int Ba()
		{
			return m_ba;
		}

		public Kaze Jikaze()
		{
			return m_jikaze;
		}

		public BlockSet GetMeldBlock()
		{
			return m_meldBlock;
		}

		public HaiID[] HandHais()
		{
			return m_hand;
		}

		public int GetDoraCount()
		{
			return m_doraCount;
		}

		public int GetItemCount()
		{
			return m_handItemCount;
		}

		public bool IsReached()
		{
			return m_isReached;
		}

		public bool IsDoubleReached()
		{
			return m_isDoubleReached;
		}
	}

	private class SnapShotSource
	{
		public HaiID[] HandHaiIDs;

		public List<Block> Melds = new List<Block>();

		public bool IsReached;

		public bool IsTsumoagariSelectable;

		public Ba Ba;

		public Kaze Jikaze;

		public int DoraCount;
	}

	public bool IsMainPlayer;

	public bool EnableSnapShot;

	public bool CanReach;

	private List<Player> Players;

	private Player MyPlayer;

	private int PlayerIndex;

	private int Ba;

	private int DoraCount;

	private SnapShot m_snapShot = new SnapShot();

	private bool m_isDebugAnalyze;

	public bool IsTsumoagariSelectable;

	public bool IsFireReach => MyPlayer.m_isFireReach;

	public bool IsOpenReach => MyPlayer.m_isOpenReach;

	public bool IsAtomicReach => MyPlayer.m_isAtomicReach;

	private bool IsOneByOneDiscardedHandSearch
	{
		get
		{
			if (IsMyTurn())
			{
				return !IsReached();
			}
			return false;
		}
	}

	public void Set(int playerIndex, bool isMainPlayer, List<Player> players, int ba)
	{
		PlayerIndex = playerIndex;
		IsMainPlayer = isMainPlayer;
		Players = players;
		MyPlayer = Players[PlayerIndex];
		Ba = ba;
	}

	public void NotifyAnalyze(bool isDebugAnalyze = false)
	{
		m_isDebugAnalyze = isDebugAnalyze;
		if (!isDebugAnalyze)
		{
			m_snapShot.Create(MyPlayer, Ba, MyPlayer.m_jikaze);
		}
		EnableSnapShot = true;
	}

	public HaiID[] HandHais()
	{
		if (EnableSnapShot)
		{
			return m_snapShot.HandHais();
		}
		return MyPlayer.hand.GetHais();
	}

	public int GetTehaiCount()
	{
		if (EnableSnapShot)
		{
			return m_snapShot.GetItemCount();
		}
		return MyPlayer.hand.GetItemCount();
	}

	public BlockSet GetMeldBlock()
	{
		if (EnableSnapShot)
		{
			return m_snapShot.GetMeldBlock();
		}
		return MyPlayer.meld.GetBlockSet();
	}

	public int GetDoraCount(int ignoreIndex = -1)
	{
		if (m_isDebugAnalyze)
		{
			return m_snapShot.GetDoraCount();
		}
		if (ignoreIndex >= 0)
		{
			return MyPlayer.GetDoraCount(ignoreIndex);
		}
		if (EnableSnapShot)
		{
			return m_snapShot.GetDoraCount();
		}
		return MyPlayer.GetDoraCount();
	}

	public int GetBonusCount(HaiID machiHai = HaiID.None)
	{
		if (GameDataManager.Mahjong.IsAotenjoMode)
		{
			return MyPlayer.m_comebackTakuBonusDisp.bonusHan + MahjongUtility.MeldBonus.MeldCount(Players) + MahjongUtility.AllGreen.BonusCount(Players.Find((Player x) => x.IsJicha()), machiHai);
		}
		if (GameDataManager.Mahjong.IsComebackMode)
		{
			return MyPlayer.m_comebackTakuBonusDisp.bonusHan;
		}
		if (GameDataManager.Mahjong.IsMeldBonusMode)
		{
			return MahjongUtility.MeldBonus.MeldCount(Players);
		}
		if (GameDataManager.Mahjong.IsAllGreenBonusMode)
		{
			return MahjongUtility.AllGreen.BonusCount(Players.Find((Player x) => x.IsJicha()), machiHai);
		}
		return 0;
	}

	public Dump Dump()
	{
		return MyPlayer.dump;
	}

	public bool IsMyTurn()
	{
		return GetTehaiCount() % 3 == 2;
	}

	public bool IsReached()
	{
		if (EnableSnapShot)
		{
			return m_snapShot.IsReached();
		}
		return MyPlayer.m_isReach;
	}

	public bool IsDoubleReached()
	{
		if (EnableSnapShot)
		{
			return m_snapShot.IsDoubleReached();
		}
		return MyPlayer.m_isDoubleReach;
	}

	public bool IsYakuhai(HaiID haiID)
	{
		int num = 0;
		int num2 = 0;
		if (EnableSnapShot)
		{
			num = m_snapShot.Ba();
			num2 = (int)m_snapShot.Jikaze();
		}
		else
		{
			num = Ba;
			num2 = (int)MyPlayer.m_jikaze;
		}
		HaiID haiID2 = (HaiID)(27 + num);
		HaiID haiID3 = (HaiID)(27 + num2);
		if (haiID != HaiID.J5 && haiID != HaiID.J6 && haiID != HaiID.J7 && haiID != haiID2)
		{
			return haiID == haiID3;
		}
		return true;
	}

	public HaiID GetBakazeHaiID()
	{
		if (EnableSnapShot)
		{
			return (HaiID)(27 + m_snapShot.Ba());
		}
		return (HaiID)(27 + Ba);
	}

	public HaiID GetJikazeHaiID()
	{
		if (EnableSnapShot)
		{
			return (HaiID)(27 + m_snapShot.Jikaze());
		}
		return (HaiID)(27 + MyPlayer.m_jikaze);
	}

	public void CreateDebugSnapShot(int index)
	{
	}
}
