using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using Konami.System.Net;
using MFG.GameRequest;
using MFG.MasterGameData;
using MFG.Sytem.Utilities.Xml;
using MFG.Types;
using MFG.Types.Competition;
using MFG.Types.Customize.Costume;
using MFG.Types.Customize.CustomizeItem;
using MFG.Types.YellCampaign;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class PlayerGameData : GameDataBase
{
	public int MatchingVersion = 70;

	public const int CurrentMatchingVersion = 70;

	public int TMakeVersion = 1;

	public const int CurrentDataVersion = 14;

	public int DataVersion = 14;

	public static int LoadDataVersion = 14;

	public const string ClientStateName = "player_game";

	public bool _IsPro;

	public string PlayerName;

	public bool IsAnonymous;

	public bool IsAnonymousShopCompetition = true;

	[NonSerialized]
	public int PlayerID;

	public CharaType SelectChara;

	public CompetitionID EntryConfirmedCompetitionID;

	public CompetitionID RecentFinishedCompetitionID;

	public TrophyType RecentFinishedCompetitionTrophyType;

	public YellRank YellRank;

	[SerializeField]
	[HideInInspector]
	private string _lastestFreePlayCampaignDateTime;

	[SerializeField]
	private bool m_isNetCafe;

	public List<MFG.Types.Customize.CustomizeItem.ItemID> StampHistory = new List<MFG.Types.Customize.CustomizeItem.ItemID>();

	public List<CharaType> PurchaseCharaList = new List<CharaType>();

	public List<CharaType> GachaCharaUsableList = new List<CharaType>();

	public List<CharaGameData> CharaData = new List<CharaGameData>();

	public List<MahjongActorMatchRecord> MahjongActorMatchRecords = new List<MahjongActorMatchRecord>();

	public List<bool> m_dojoSlotUnlocked = new List<bool>(DojoFlow.DojoSlotNum) { true, false, false, false };

	[SerializeField]
	private int m_fightStar = 10;

	[SerializeField]
	private DanLevelType m_danLevel = DanLevelType.Newbie;

	public CharaType OdekakeChara = (CharaType)(-1);

	public MFG.Types.Customize.Costume.ItemID OdekakeCharaCostume;

	public CharaType BeforeChara = (CharaType)(-1);

	public CharaGameData OdekakeCharaGameData;

	public CustomizeGameData OdekakeCustomizeData;

	private Dictionary<int, CharaType> m_dataVersionCharaNum = new Dictionary<int, CharaType>
	{
		{
			1,
			CharaType.Chara09
		},
		{
			2,
			CharaType.Chara10
		},
		{
			3,
			CharaType.Chara11
		},
		{
			4,
			CharaType.Chara12
		},
		{
			5,
			CharaType.Chara13
		},
		{
			6,
			CharaType.Chara14
		},
		{
			7,
			CharaType.Chara15
		},
		{
			8,
			CharaType.Chara16
		},
		{
			9,
			CharaType.Chara17
		},
		{
			10,
			CharaType.Chara18
		},
		{
			11,
			CharaType.Chara19
		},
		{
			12,
			CharaType.Chara20
		},
		{
			13,
			CharaType.Chara21
		},
		{
			14,
			(CharaType)21
		}
	};

	public DateTimeOffset LastestFreePlayCampaignDateTime
	{
		get
		{
			return DateTimeOffsetManager.GetJSTFromString(_lastestFreePlayCampaignDateTime, isContainOffset: true);
		}
		set
		{
			_lastestFreePlayCampaignDateTime = value.ToString();
		}
	}

	public bool IsNetCafePlayer
	{
		get
		{
			return m_isNetCafe;
		}
		private set
		{
			m_isNetCafe = value;
		}
	}

	public int FightStar
	{
		get
		{
			return m_fightStar;
		}
		set
		{
			m_fightStar = value;
			DanLevel = DanLevelMaster.GetCurrentDanLevelAndFightStarBorder(m_fightStar).currentDanLevel;
			if (DanLevel >= MaxDanLevel)
			{
				DanLevel = MaxDanLevel;
				m_fightStar = DanLevelMaster.GetTotalFightStarRequired(MaxDanLevel);
			}
			if (m_fightStar < 0)
			{
				m_fightStar = 0;
			}
		}
	}

	public DanLevelType DanLevel
	{
		get
		{
			return m_danLevel;
		}
		private set
		{
			m_danLevel = value;
		}
	}

	public DanLevelType MaxDanLevel => DanLevelType.Max;

	public bool IsDanLevelMax => DanLevel == MaxDanLevel;

	public int EnableStickerCount => 4;

	public bool IsOdekake => OdekakeChara >= CharaType.Chara01;

	public void RefleshCompetitionTrophy(CompetitionInfo latestCompetitionInfo, List<CompetitionInfo> kacInfos)
	{
		CompetitionID competitionID;
		TrophyType trophyType;
		if (latestCompetitionInfo == null)
		{
			competitionID = CompetitionID.None;
			trophyType = TrophyType.None;
		}
		else if (latestCompetitionInfo.CompetitionID.IsKAC2023())
		{
			CompetitionInfo infoA = kacInfos.Find((CompetitionInfo x) => x.IsFinalResultDisplayed && x.CompetitionID <= latestCompetitionInfo.CompetitionID && x.CompetitionID == CompetitionID.Competition003);
			CompetitionInfo infoB = kacInfos.Find((CompetitionInfo x) => x.IsFinalResultDisplayed && x.CompetitionID <= latestCompetitionInfo.CompetitionID && x.CompetitionID == CompetitionID.Competition004);
			(CompetitionID competitionID, TrophyType trophyType) higherGradeTrophy = GetHigherGradeTrophy(infoA, infoB);
			CompetitionID item = higherGradeTrophy.competitionID;
			TrophyType item2 = higherGradeTrophy.trophyType;
			competitionID = item;
			trophyType = item2;
		}
		else
		{
			competitionID = latestCompetitionInfo.CompetitionID;
			trophyType = (latestCompetitionInfo.IsFinalResultDisplayed ? latestCompetitionInfo.TrophyType : TrophyType.None);
		}
		RecentFinishedCompetitionID = competitionID;
		RecentFinishedCompetitionTrophyType = trophyType;
		Debug.Log($"{GetType().Name}.RefleshCompetitionTrophy: CompetitionID={competitionID}, TrophyType={trophyType}");
	}

	private (CompetitionID competitionID, TrophyType trophyType) GetHigherGradeTrophy(CompetitionInfo infoA, CompetitionInfo infoB)
	{
		TrophyType trophyType = infoA?.TrophyType ?? TrophyType.None;
		TrophyType trophyType2 = infoB?.TrophyType ?? TrophyType.None;
		CompetitionID item = infoA?.CompetitionID ?? CompetitionID.None;
		CompetitionID item2 = infoB?.CompetitionID ?? CompetitionID.None;
		if (trophyType == TrophyType.None && trophyType2 == TrophyType.None)
		{
			return (competitionID: CompetitionID.None, trophyType: TrophyType.None);
		}
		if (IsHigherGradeThan(trophyType, trophyType2))
		{
			return (competitionID: item, trophyType: trophyType);
		}
		return (competitionID: item2, trophyType: trophyType2);
		static bool IsHigherGradeThan(TrophyType a, TrophyType b)
		{
			if (a == TrophyType.None && b == TrophyType.None)
			{
				return false;
			}
			if (a == TrophyType.None && b != 0)
			{
				return false;
			}
			if (a != 0 && b == TrophyType.None)
			{
				return true;
			}
			return a < b;
		}
	}

	public void MahjongActorMatchRecord_AddCount(MahjongActorID mahjongActorID, bool isWin)
	{
		if (mahjongActorID != 0)
		{
			MahjongActorMatchRecord mahjongActorMatchRecord = MahjongActorMatchRecords.Find((MahjongActorMatchRecord x) => x.MahjongActorID == mahjongActorID);
			if (mahjongActorMatchRecord == null)
			{
				mahjongActorMatchRecord = new MahjongActorMatchRecord
				{
					MahjongActorID = mahjongActorID
				};
				MahjongActorMatchRecords.Add(mahjongActorMatchRecord);
			}
			mahjongActorMatchRecord.TotalCount++;
			if (isWin)
			{
				mahjongActorMatchRecord.TotalWinCount++;
			}
			Debug.Log($"MahjongActorMatchRecord_AddCount() mahjongActorID:{mahjongActorID}, isWin:{isWin}, totalWinCount:{mahjongActorMatchRecord.TotalWinCount}, totalCount:{mahjongActorMatchRecord.TotalCount}");
		}
	}

	public CharaGameData GetCurrentChara(bool IsGetMyCharaData = false)
	{
		if (IsOdekake && !IsGetMyCharaData)
		{
			return OdekakeCharaGameData;
		}
		return CharaData[(int)SelectChara];
	}

	public void DojoUnlock()
	{
		m_dojoSlotUnlocked[0] = true;
		if (DanLevel >= DanLevelType.KYU5)
		{
			m_dojoSlotUnlocked[1] = true;
		}
		if (DanLevel >= DanLevelType.DAN1)
		{
			m_dojoSlotUnlocked[2] = true;
		}
		if (DanLevel >= DanLevelType.JANSEI1)
		{
			m_dojoSlotUnlocked[3] = true;
		}
	}

	public bool IsPro()
	{
		if (!MasterGameDataManager.Character.Properties(SelectChara).ProProfile.Enable)
		{
			return false;
		}
		return _IsPro;
	}

	public (DanLevelType expectedDanLevel, int expectedTotalFightStar) ExpectedDanLevelAndFightStar(int acquisitionFightStar)
	{
		(DanLevelType, int, int) currentDanLevelAndFightStarBorder = DanLevelMaster.GetCurrentDanLevelAndFightStarBorder(FightStar + acquisitionFightStar);
		(DanLevelType, int) result = default((DanLevelType, int));
		if (currentDanLevelAndFightStarBorder.Item1 < MaxDanLevel)
		{
			result.Item1 = currentDanLevelAndFightStarBorder.Item1;
			result.Item2 = FightStar + acquisitionFightStar;
		}
		else
		{
			result.Item1 = MaxDanLevel;
			result.Item2 = DanLevelMaster.GetTotalFightStarRequired(MaxDanLevel);
		}
		return result;
	}

	public void UpdateCostume()
	{
		OdekakeInit();
		GetCurrentChara().CostumeItemEquip = GameUtility.CostumeUtility.CostumeConversion(GetCurrentChara().CostumeItemEquip);
	}

	public void OdekakeInit()
	{
		if (IsOdekake)
		{
			SelectChara = BeforeChara;
			OdekakeChara = (CharaType)(-1);
			BeforeChara = (CharaType)(-1);
			OdekakeCustomizeData = new CustomizeGameData();
		}
	}

	public PlayerGameData()
	{
		int num = (int)m_dataVersionCharaNum[LoadDataVersion];
		for (int i = 0; i < num; i++)
		{
			CharaData.Add(new CharaGameData((CharaType)i));
		}
	}

	public PlayerGameData Clone()
	{
		PlayerGameData playerGameData = (PlayerGameData)MemberwiseClone();
		List<CharaGameData> list = new List<CharaGameData>();
		foreach (CharaGameData charaDatum in CharaData)
		{
			list.Add(charaDatum.Clone());
		}
		playerGameData.CharaData = list;
		return playerGameData;
	}

	public void Parse(XElement mpDataElem)
	{
		try
		{
			XElement xElement = mpDataElem.Element("mid");
			if (xElement != null)
			{
				PlayerID = xElement.ValueInt();
			}
			XElement xElement2 = mpDataElem.Element("name");
			if (xElement2 != null)
			{
				PlayerName = xElement2.Value.Replace("\r", "").Replace("\n", "");
			}
		}
		catch (MustNeedNodeNotFoundException)
		{
		}
	}

	public void Apply(GetMenuData.ResultData resultData)
	{
		PlayerName = resultData.PlayerData.PlayerName;
	}

	public async UniTask RefleshTMakeVersionAndSave(CancellationToken cancellationToken = default(CancellationToken))
	{
		try
		{
			GameDataManager.Player.TMakeVersion = 25;
			if (GameDataManager.IsReachSongToytoyNoticeEnable())
			{
				GameDataManager.Player.TMakeVersion = 26;
			}
			if (GameDataManager.IsToukaCpuForceEnable())
			{
				GameDataManager.Player.TMakeVersion = 27;
			}
			else if (GameDataManager.IsToukaCharaEnable())
			{
				GameDataManager.Player.TMakeVersion = 28;
			}
			await GameDataManager.Player.Save(cancellationToken);
		}
		catch (OperationCanceledException ex)
		{
			Debug.Log("OperationCanceledException Catched. " + ex.Message);
		}
		catch (NetworkResponseException ex2)
		{
			GameDataManager.PlayerConfig.IsDataSaveFailed = true;
			Debug.Log("NetworkResponseException Catched. " + ex2.Message);
		}
	}

	public async UniTask Save(CancellationToken cancellationToken = default(CancellationToken))
	{
		MatchingVersion = 70;
		string text = JsonUtility.ToJson(this);
		Debug.Log("PlayerGameData Save : " + text);
		await RequestApiBase<ClientStateWrite, ClientStateWrite.ResultData, (string, string)>.Send(("player_game", text), enableErrorScreenTransition: false, enableSendLog: true, cancellationToken);
	}

	public static PlayerGameData LoadFromJson(string jsonString)
	{
		Debug.Log("PlayerGameData Load : " + jsonString);
		LoadDataVersion = JsonUtility.FromJson<PlayerGameData>(jsonString).DataVersion;
		PlayerGameData playerGameData = JsonUtility.FromJson<PlayerGameData>(jsonString);
		LoadDataVersion = 14;
		if (playerGameData == null)
		{
			playerGameData = new PlayerGameData();
		}
		playerGameData.Validate();
		playerGameData.DataVersion = 14;
		if (!GameDataManager.IsCharaEnable(playerGameData.SelectChara))
		{
			playerGameData.SelectChara = CharaType.Chara01;
		}
		playerGameData.IsNetCafePlayer = GameUtility.IsNetcafeBehavior;
		return playerGameData;
	}

	private void Validate()
	{
		DanLevel = (DanLevelType)Math.Clamp((int)DanLevel, 1, (int)MaxDanLevel);
		for (int i = CharaData.Count; i < Enum.GetNames(typeof(CharaType)).Length; i++)
		{
			CharaGameData charaGameData = new CharaGameData((CharaType)i);
			CharaType charaType = (CharaType)i;
			charaGameData.charaType = charaType;
			charaGameData.ResetAllEqipToDefault();
			CharaData.Add(charaGameData);
		}
	}
}
