using System;
using System.Collections.Generic;
using MFG.GameData;
using MFG.Types;
using MFG.Types.Customize.CustomizeItem;
using UnityEngine;

namespace MFG.MahjongPlay.Cutin;

[Serializable]
public class PlayerInteractor : IPlayerInteractor
{
	[SerializeField]
	private Player m_player;

	[SerializeField]
	private GameObject m_cutinParent;

	[SerializeField]
	private GameObject m_nakiCutinParent;

	[SerializeField]
	private GameObject m_knockdownTileParent;

	[SerializeField]
	private GameObject m_thrownReachStickParent;

	[SerializeField]
	private GameObject m_landedReachStickParent;

	private Han m_han;

	private int m_doraCount;

	private List<Yaku> m_yakus = new List<Yaku>();

	private bool m_scorePaymentPlayer;

	private int m_meldBonusHan;

	private int m_comebackBonusHan;

	private int m_akadoraCount;

	private int m_allgreenCount;

	private bool m_isBomb;

	private bool m_isCatharsis;

	private ItemID m_reachStickID;

	private Pai m_dahai;

	public MFG.Types.EventType EventType()
	{
		return GameDataManager.Mahjong.EventType;
	}

	public PlayerInteractor(Player player)
	{
		m_player = player;
	}

	public Cha Cha()
	{
		return m_player.m_cha;
	}

	public Kaze Kaze()
	{
		return m_player.m_jikaze;
	}

	public int FavoLevel()
	{
		return m_player.m_level;
	}

	public ReachType ReachType()
	{
		if (!m_player.m_isDoubleReach)
		{
			if (!m_player.m_isReach)
			{
				return MFG.Types.ReachType.None;
			}
			return MFG.Types.ReachType.Reach;
		}
		return MFG.Types.ReachType.DoubleReach;
	}

	public ReachLevel ReachLevel()
	{
		if (!m_player.m_isFireReach)
		{
			if (!m_player.m_isAtomicReach)
			{
				if (!m_player.m_isOpenReach)
				{
					return MFG.Types.ReachLevel.Normal;
				}
				return MFG.Types.ReachLevel.OpenReach;
			}
			return MFG.Types.ReachLevel.AtomicReach;
		}
		return MFG.Types.ReachLevel.FireReach;
	}

	public CharaType CharaType()
	{
		return m_player.m_charaType;
	}

	public int CharaVariantID()
	{
		return m_player.m_charaVariantID;
	}

	public Pai Dahai()
	{
		return m_dahai;
	}

	public List<CutinItemInfo> CutinItems()
	{
		return m_player.CutinItems;
	}

	public Dictionary<Category, List<ItemID>> CustomizeItemEquips()
	{
		return m_player.CustomizeItemEquips;
	}

	public GameObject CutinParent()
	{
		return m_cutinParent;
	}

	public GameObject NakiCutinParent()
	{
		return m_nakiCutinParent;
	}

	public GameObject KnockdownTileParent()
	{
		return m_knockdownTileParent;
	}

	public void CreateKnockdownTiles(GameObject tileParent)
	{
		foreach (Transform item in tileParent.transform)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		Hand hand = UnityEngine.Object.Instantiate(m_player.hand, tileParent.transform);
		Meld meld = UnityEngine.Object.Instantiate(m_player.meld, tileParent.transform);
		hand.name = "Hand";
		hand.transform.localScale = Vector3.one;
		hand.transform.localPosition = new Vector3(-0.372f, 0f, 0f);
		hand.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
		hand.StopTenpaiEffect();
		meld.name = "Meld";
		meld.transform.localScale = Vector3.one;
		meld.transform.localPosition = new Vector3(0.389f, 0f, 0f);
		meld.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
		meld.StopTenpaiEffect();
	}

	public GameObject ThrownReachStickParent()
	{
		return m_thrownReachStickParent;
	}

	public GameObject LandedReachStickParent()
	{
		return m_landedReachStickParent;
	}

	public ItemID ReachStickID()
	{
		return m_reachStickID;
	}

	public bool IsActive()
	{
		return m_player.IsValid;
	}

	public bool IsScorePaymentPlayer()
	{
		return m_scorePaymentPlayer;
	}

	public Han PaymentAmount()
	{
		if (GameDataManager.Mahjong.IsAotenjoMode && m_han == Han.Yakuman && PaymentYakus().Contains(Yaku.Kazoeyakuman))
		{
			return Han.Sanbaiman;
		}
		return m_han;
	}

	public int DoraCount()
	{
		return m_doraCount;
	}

	public int MeldBonusCount()
	{
		return m_meldBonusHan;
	}

	public int ComebackBonusCount()
	{
		return m_comebackBonusHan;
	}

	public int AkadoraCount()
	{
		return m_akadoraCount;
	}

	public int AllGreenCount()
	{
		return m_allgreenCount;
	}

	public List<Yaku> PaymentYakus()
	{
		return m_yakus;
	}

	public List<VoiceInfo> GachaVoices()
	{
		return m_player.GachaVoiceItemEquip;
	}

	public bool IsBombBonusPlay()
	{
		return m_isBomb;
	}

	public bool IsCatharsisPlay()
	{
		return m_isCatharsis;
	}

	public void SetPaymentInfo(Han han, int doraCount, int meldBonusHan, int comebackBonusHan, int allgreenBonusHan, List<Yaku> yakus, bool isScorePaymentPlayer, int akadoraCount, bool isContainBombCha, bool isCatharsisEffectPlay)
	{
		m_han = han;
		m_doraCount = doraCount;
		m_meldBonusHan = meldBonusHan;
		m_comebackBonusHan = comebackBonusHan;
		m_akadoraCount = akadoraCount;
		m_allgreenCount = allgreenBonusHan;
		if (yakus != null)
		{
			m_yakus = yakus;
		}
		m_scorePaymentPlayer = isScorePaymentPlayer;
		m_isBomb = isContainBombCha;
		m_isCatharsis = isCatharsisEffectPlay;
	}

	public void SetDahai(Pai pai)
	{
		m_dahai = pai;
	}

	public void SetReachStick(ItemID reachStickID)
	{
		m_reachStickID = reachStickID;
	}
}
