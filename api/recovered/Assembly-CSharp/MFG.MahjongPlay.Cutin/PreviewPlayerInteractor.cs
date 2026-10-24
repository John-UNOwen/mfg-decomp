using System;
using System.Collections.Generic;
using System.Linq;
using MFG.GameData;
using MFG.Types;
using MFG.Types.Customize.CustomizeItem;
using UnityEngine;

namespace MFG.MahjongPlay.Cutin;

[Serializable]
public class PreviewPlayerInteractor : IPlayerInteractor
{
	[SerializeField]
	private MFG.Types.EventType m_eventType;

	public Cha m_cha;

	public Kaze m_jikaze;

	public int m_favoLevel = 1;

	public CharaType m_charaType;

	public int m_charaVariantID;

	public ReachType m_reachType;

	public ReachLevel m_reachLevel;

	public List<CutinItemInfo> m_cutinItems = new List<CutinItemInfo>();

	[CustomizeItemIDFilter(Category.ReachStick)]
	public ItemID m_reachStickItemID = ItemID.ReachStick001;

	[CustomizeItemIDFilter(Category.PlayerAccessory)]
	public ItemID m_playerAccessoryItemID = ItemID.PlayerAccessory001;

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

	[SerializeField]
	private Han m_han;

	[SerializeField]
	private int m_doraCount;

	[SerializeField]
	private Pai m_dahai;

	[SerializeField]
	private List<Yaku> m_yakus = new List<Yaku>();

	[SerializeField]
	private bool m_scorePaymentPlayer;

	[SerializeField]
	private int m_meldBonusHan;

	[SerializeField]
	private int m_comebackBonusHan;

	[SerializeField]
	private int m_akadoraCount;

	[SerializeField]
	private int m_allgreenCount;

	[SerializeField]
	private ItemID m_reachStickID;

	[SerializeField]
	private bool m_isBomb;

	[SerializeField]
	private bool m_isCatharsis;

	[SerializeField]
	private List<Pai> m_knockdownHands = new List<Pai>
	{
		Pai.S2,
		Pai.S3,
		Pai.S4,
		Pai.P2,
		Pai.P3,
		Pai.P4,
		Pai.M2,
		Pai.M3,
		Pai.M4,
		Pai.M5,
		Pai.M6,
		Pai.M8,
		Pai.M8,
		Pai.M7
	};

	[SerializeField]
	public GameObject m_previewTilesPrefab;

	public MFG.Types.EventType EventType()
	{
		return m_eventType;
	}

	public Cha Cha()
	{
		return m_cha;
	}

	public Kaze Kaze()
	{
		return m_jikaze;
	}

	public int FavoLevel()
	{
		return m_favoLevel;
	}

	public CharaType CharaType()
	{
		return m_charaType;
	}

	public int CharaVariantID()
	{
		return m_charaVariantID;
	}

	public ReachType ReachType()
	{
		return m_reachType;
	}

	public ReachLevel ReachLevel()
	{
		return m_reachLevel;
	}

	public List<CutinItemInfo> CutinItems()
	{
		return m_cutinItems;
	}

	public Pai Dahai()
	{
		return m_dahai;
	}

	public Dictionary<Category, List<ItemID>> CustomizeItemEquips()
	{
		if (m_reachStickItemID == ItemID.None)
		{
			m_reachStickItemID = ItemID.ReachStick001;
		}
		if (m_playerAccessoryItemID == ItemID.None)
		{
			m_playerAccessoryItemID = ItemID.PlayerAccessory001;
		}
		return new Dictionary<Category, List<ItemID>>
		{
			{
				Category.ReachStick,
				new List<ItemID> { m_reachStickItemID }
			},
			{
				Category.PlayerAccessory,
				new List<ItemID> { m_playerAccessoryItemID }
			},
			{
				Category.TableSticker,
				new List<ItemID> { ItemID.TableSticker001 }
			},
			{
				Category.TileSurface,
				new List<ItemID> { ItemID.TileSurface001 }
			},
			{
				Category.ReachBgm,
				new List<ItemID> { ItemID.ReachBgm001 }
			},
			{
				Category.MyRoomBgm,
				new List<ItemID> { ItemID.MyRoomBgm001 }
			},
			{
				Category.StageBg,
				new List<ItemID> { ItemID.StageBg001 }
			}
		};
	}

	public GameObject CutinParent()
	{
		return m_cutinParent;
	}

	public GameObject NakiCutinParent()
	{
		return m_nakiCutinParent;
	}

	public GameObject ThrownReachStickParent()
	{
		return m_thrownReachStickParent;
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
		GameObject gameObject = UnityEngine.Object.Instantiate(m_previewTilesPrefab, tileParent.transform);
		gameObject.name = "Hand";
		gameObject.transform.localScale = Vector3.one;
		gameObject.transform.localPosition = new Vector3(-0.372f, 0f, 0f);
		gameObject.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
		foreach (var item2 in gameObject.transform.Cast<Transform>().Select((Transform item, int index) => new { item, index }))
		{
			Tile component = item2.item.GetComponent<Tile>();
			component.name = $"TileRoot{item2.index + 1}";
			component.SetHaiTexture(m_knockdownHands[item2.index]);
			component.transform.localPosition = new Vector3(0.05f * (float)item2.index, 0f, 0f);
		}
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
		return true;
	}

	public bool IsScorePaymentPlayer()
	{
		return m_scorePaymentPlayer;
	}

	public Han PaymentAmount()
	{
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

	public bool IsBombBonusPlay()
	{
		return m_isBomb;
	}

	public bool IsCatharsisPlay()
	{
		return m_isCatharsis;
	}

	public List<Yaku> PaymentYakus()
	{
		return m_yakus;
	}

	public List<VoiceInfo> GachaVoices()
	{
		return new List<VoiceInfo>();
	}

	public void SetPaymentInfo(Han han, bool isScorePaymentPlayer)
	{
		m_han = han;
		m_scorePaymentPlayer = isScorePaymentPlayer;
	}

	public void SetPreviewParents(PlayerInteractor interactor)
	{
		m_knockdownTileParent = interactor.KnockdownTileParent();
		m_thrownReachStickParent = interactor.ThrownReachStickParent();
	}
}
