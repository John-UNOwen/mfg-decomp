using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameContents;
using KAMUNITY;
using MFG.Sytem.Utilities;
using MFG.Types;
using MFG.Types.Customize.Costume;
using TCSNET;
using TestModeSkeleton;
using TestModeSkeleton.CabinetDef;
using TestModeSkeleton.Price;
using UnityEngine;

public class GameUtility
{
	public static class ServerLog
	{
		private static List<(string key, string value)> _logs = new List<(string, string)>();

		public static async UniTask Send(string key, string value)
		{
			await Send(new List<(string, string)> { (key, value) });
		}

		public static async UniTask<bool> Send(List<(string key, string value)> logs)
		{
			if (!IsKamunityBootEnable())
			{
				return true;
			}
			if (!IsNetworkActive())
			{
				Debug.Log("SendLog: IsNetworkActive=false");
				return false;
			}
			XrpcResponseHelper<TcsXrpcModuleTypes.PutMsgResponse> xrpcHelper = new XrpcResponseHelper<TcsXrpcModuleTypes.PutMsgResponse>();
			TcsXrpcModule.XrpcMethodPutMsg.Request(logs, xrpcHelper.Callback, null);
			await xrpcHelper.CoWaitForXrpcFinish();
			if (xrpcHelper.isError)
			{
				Debug.Log($"SendLog: isError={xrpcHelper.isError}");
				return false;
			}
			foreach (var (text, text2) in logs)
			{
				Debug.Log("SendLog: key=" + text + ", value=" + text2);
			}
			return true;
		}

		public static void Reserve(string key, string value)
		{
			_logs.Add((key, value));
		}

		public static void Reserve(List<(string key, string value)> logs)
		{
			_logs.AddRange(logs);
		}

		public static async UniTask SendReservedLog()
		{
			if (_logs.Count > 0 && await Send(_logs))
			{
				_logs.Clear();
			}
		}
	}

	public static class ContinuePriceUtility
	{
		private static Dictionary<TakuType, (int lowerScore, int upperScore)> s_scoreMaster = new Dictionary<TakuType, (int, int)>
		{
			{
				TakuType.Tonpu,
				(30000, 40000)
			},
			{
				TakuType.Sanma,
				(40000, 50000)
			},
			{
				TakuType.Nima,
				(1000000, 1000001)
			}
		};

		private static Dictionary<TakuType, (int lowerScore, int upperScore)> s_kirisameScoreMaster = new Dictionary<TakuType, (int, int)>
		{
			{
				TakuType.Tonpu,
				(35000, 45000)
			},
			{
				TakuType.Sanma,
				(45000, 55000)
			},
			{
				TakuType.Nima,
				(1000000, 1000001)
			}
		};

		private static Dictionary<TakuType, (int lowerScore, int upperScore)> s_buttobiGalaxyScoreMaster = new Dictionary<TakuType, (int, int)>
		{
			{
				TakuType.Tonpu,
				(25000, 35000)
			},
			{
				TakuType.Sanma,
				(30000, 40000)
			},
			{
				TakuType.Nima,
				(1000000, 1000001)
			}
		};

		private static Dictionary<TakuType, (int lowerScore, int upperScore)> s_aotenjoScoreMaster = new Dictionary<TakuType, (int, int)>
		{
			{
				TakuType.Tonpu,
				(250000000, 350000000)
			},
			{
				TakuType.Sanma,
				(1000000000, 1200000000)
			},
			{
				TakuType.Nima,
				(30000000, 30000001)
			}
		};

		private static Dictionary<TakuType, (int lowerScore, int upperScore)> s_reversalScoreMaster = new Dictionary<TakuType, (int, int)>
		{
			{
				TakuType.Tonpu,
				(45000, 55000)
			},
			{
				TakuType.Sanma,
				(50000, 60000)
			},
			{
				TakuType.Nima,
				(1000000, 1000001)
			}
		};

		public static (int lowerScore, int upperScore) GetBoundaryScore(TakuType takuType, MFG.Types.EventType eventType)
		{
			switch (eventType)
			{
			case MFG.Types.EventType.ToitoiButtobiGalaxy:
				return s_buttobiGalaxyScoreMaster[takuType];
			case MFG.Types.EventType.Aotenjo:
				return s_aotenjoScoreMaster[takuType];
			case MFG.Types.EventType.Comeback:
			case MFG.Types.EventType.Kirisame:
			case MFG.Types.EventType.Bomb:
				return s_kirisameScoreMaster[takuType];
			case MFG.Types.EventType.Reversal:
				return s_reversalScoreMaster[takuType];
			default:
				return s_scoreMaster[takuType];
			}
		}

		public static (Fraction priceRate, int lowerScore, int upperScore) GetPriceRate(TakuType takuType, MFG.Types.EventType eventType, int score)
		{
			(int lowerScore, int upperScore) boundaryScore = GetBoundaryScore(takuType, eventType);
			int item = boundaryScore.lowerScore;
			int item2 = boundaryScore.upperScore;
			Fraction fraction = new Fraction();
			fraction = ((!GameDataManager.Mahjong.IsAotenjoMode) ? new Fraction
			{
				Numerator = item2 - score,
				Denominator = item2 - item
			} : new Fraction
			{
				Numerator = item2 / 100 - score / 100,
				Denominator = item2 / 100 - item / 100
			});
			if (takuType == TakuType.Nima)
			{
				fraction.Numerator = 1;
				fraction.Denominator = 1;
			}
			return (priceRate: fraction, lowerScore: item, upperScore: item2);
		}
	}

	public static class CostumeUtility
	{
		public static ItemID CostumeConversion(ItemID itemID, bool getDefault = false)
		{
			if (itemID == ItemID.Costume13_002 || itemID == ItemID.Costume13_004)
			{
				if (!IsNetcafeBehavior || getDefault)
				{
					return ItemID.Costume13_002;
				}
				return ItemID.Costume13_004;
			}
			return itemID;
		}
	}

	public static class FreePlayCampaignUtility
	{
		public static async UniTask<(bool isEnable, DateTimeOffset currentCampaignDateTime)> IsEnableFreePlay()
		{
			DateTimeOffset currentCampaignDateTime = GetCurrentDateTime();
			if (GameDataManager.Account.IsGuestUser)
			{
				return (false, currentCampaignDateTime);
			}
			bool num = CabinetManager.Game.Data.FreePlayCampaign == GameOptionsDef.FreePlayCampaign.On;
			bool flag = GameDataManager.System.GameControlInfo.EventStatus.IsActive(GameEventType.CollaborationFreePlay);
			if (!num || !flag || !IsKamunityBootEnable())
			{
				return (false, currentCampaignDateTime);
			}
			XrpcResponseHelper<TcsXrpcModuleTypes.GetExtCampaignResponse> xrpcHelper = new XrpcResponseHelper<TcsXrpcModuleTypes.GetExtCampaignResponse>();
			TcsXrpcModule.XrpcMethodGetExtCampaign.Request(xrpcHelper.Callback, null);
			await xrpcHelper.CoWaitForXrpcFinish();
			int eventID = GameDataManager.System.GameControlInfo.EventStatus.GetGameEventData(GameEventType.CollaborationFreePlay)?.ParamValue("event_id", 0) ?? 0;
			if (eventID <= 0)
			{
				return (false, currentCampaignDateTime);
			}
			TcsXrpcModuleTypes.GetExtCampaignResponse.ExtCampaignInfo extCampaignInfo = xrpcHelper.result?.ExtCampaignInfos?.Find((TcsXrpcModuleTypes.GetExtCampaignResponse.ExtCampaignInfo x) => x.EventID == eventID);
			if (extCampaignInfo == null)
			{
				return (false, currentCampaignDateTime);
			}
			return (extCampaignInfo.EventValue > 0 && currentCampaignDateTime > GameDataManager.Player.LastestFreePlayCampaignDateTime, currentCampaignDateTime);
		}

		private static DateTimeOffset GetCurrentDateTime()
		{
			return (DateTimeOffsetManager.GetJSTTime() - new TimeSpan(7, 0, 0)).Trunc(TimeSpan.FromDays(1.0));
		}
	}

	private static bool s_isEamDevConnectionCalled = false;

	private static bool s_isEamDevConnection = false;

	private static bool s_isInhouseCabinetCalled = false;

	private static bool s_isInhouseCabinet = false;

	private static List<string> m_inhouseLocationIDs = new List<string> { "EARX0001", "EARX0002", "EARX0003", "EARX0004", "EARX0005", "EA004106", "EA009737" };

	private static bool s_isProCardIdCalled = false;

	private static bool s_isProCardId = false;

	private static List<string> m_proCardIds = new List<string>
	{
		"012E5524F15574A9", "E0040100177AADEF", "012E3D178B4BC56F", "012E4CE15C554B33", "012E5521B3015898", "012E5521B3015761", "012E5521B3015775", "012E5521B3015789", "4000000023845573", "4000000023846605",
		"4000000023307723", "4000000023369756"
	};

	private static bool m_isReservedGameOver = false;

	public static bool IsAppTerminatDecided;

	private static bool m_isIOEnable = true;

	private static bool m_isKonamiNetConnection = false;

	public static bool OnBeginSkipBootSceneFinished = true;

	private static bool s_isBuildVersionInfoLoadTried;

	private static GameBuildVersionInfo s_buildVersionInfo;

	public static bool IsReservedGameOver
	{
		get
		{
			return m_isReservedGameOver;
		}
		set
		{
			m_isReservedGameOver = value;
			if (!value)
			{
				ReasonGameOver = "";
			}
		}
	}

	public static string ReasonGameOver { get; set; } = "";


	public static bool IsNetcafeBehavior
	{
		get
		{
			if (SystemConfigGameData.AppBehaviorType != AppBehaviorType.Netcafe)
			{
				return DebugFlag.IsActive(DebugFlag.Flag.ForceNetCafePlayer);
			}
			return true;
		}
	}

	public static bool IsKonastaBehavior => SystemConfigGameData.AppBehaviorType == AppBehaviorType.Konasta;

	public static bool IsKonastaOrNetcafeBehavior
	{
		get
		{
			if (!IsNetcafeBehavior)
			{
				return IsKonastaBehavior;
			}
			return true;
		}
	}

	public static bool IsCloudStreaming => SystemConfigGameData.IsCloudStreaming;

	public static bool IsIOEnable
	{
		get
		{
			return m_isIOEnable;
		}
		set
		{
			m_isIOEnable = value;
		}
	}

	public static bool IsKonamiNetConnection
	{
		get
		{
			return m_isKonamiNetConnection;
		}
		set
		{
			m_isKonamiNetConnection = value;
		}
	}

	public static bool IsEAmuseActive()
	{
		return CabinetManager.NetWork.Data.EAmusement == NetworkOptionsDef.EAmusement.On;
	}

	public static bool IsEamDevConnection()
	{
		if (!IsKamunityBootEnable())
		{
			return true;
		}
		if (!s_isEamDevConnectionCalled && AvsEa3.Ea3QueryServicesProductDomain(out var product_domain) >= 0)
		{
			s_isEamDevConnection = product_domain == 0;
			s_isEamDevConnectionCalled = true;
		}
		Debug.Log($"IsEamDevConnection: {s_isEamDevConnection}");
		return s_isEamDevConnection;
	}

	public static bool IsInhouseLocation()
	{
		if (!IsKamunityBootEnable())
		{
			return true;
		}
		if (!s_isInhouseCabinetCalled)
		{
			StringBuilder stringBuilder = new StringBuilder(32);
			AvsEa3.Ea3QueryFacilityPathString(AvsEa3.FACILITY_PATH_STRING.LOCATION_ID, stringBuilder, stringBuilder.Capacity);
			string text = stringBuilder.ToString();
			s_isInhouseCabinet = m_inhouseLocationIDs.Contains(text);
			Debug.Log($"IsInhouseCabinet: {s_isInhouseCabinet} locationID:{text}");
		}
		return s_isInhouseCabinet;
	}

	public static bool IsNetcafeForceFreeGuestPlayUser(string cardID)
	{
		if (!IsNetcafeBehavior || !IsKamunityBootEnable())
		{
			return false;
		}
		bool flag = cardID == "0000000000000000";
		Debug.Log($"IsFreePlayCardNumber: {flag} CardNumber:{cardID}");
		return flag;
	}

	public static bool IsProCardId(string cardId)
	{
		if (!IsKamunityBootEnable())
		{
			return true;
		}
		if (!s_isProCardIdCalled)
		{
			s_isProCardIdCalled = true;
			s_isProCardId = m_proCardIds.Contains(cardId);
			Debug.Log($"IsProCardId: {s_isProCardId} CardId:{cardId}");
		}
		return s_isProCardId;
	}

	public static string ConvertCardNumberToUserCode(AvsTypes.CardType cardtype, string cardid)
	{
		if (!IsKamunityBootEnable())
		{
			return "";
		}
		StringBuilder stringBuilder = new StringBuilder(64);
		if (Cardnumber.ConvertCardNumberToUserCode(cardtype, cardid, stringBuilder, stringBuilder.Capacity) == Cardnumber.CARDNUMBER_GENERATE.SUCCEED)
		{
			return stringBuilder.ToString();
		}
		return "";
	}

	public static bool IsNetworkActive()
	{
		AvsTypes.EA3_STATUS eA3_STATUS = AvsTypes.EA3_STATUS.ONLINE;
		if (IsKamunityBootEnable())
		{
			eA3_STATUS = EagCore.Ea3GetStatus(out var _, out var _);
		}
		return eA3_STATUS == AvsTypes.EA3_STATUS.ONLINE;
	}

	public static bool IsEventMode()
	{
		return CabinetManager.Coin.EventMode == CoinOptionsDef.EventMode.On;
	}

	public static bool IsFreePlay()
	{
		if (IsEventMode())
		{
			return true;
		}
		return CabinetManager.Coin.Data.FreePlay == CoinOptionsDef.FreePlay.On;
	}

	public static void ThrowCoin(int coin_num)
	{
		SendPcbEvent(PCB_EVENT.EVENT.COIN_IN, coin_num);
	}

	public static void ThrowCoinService(int coin_num)
	{
		SendPcbEvent(PCB_EVENT.EVENT.SERVICE_BUTTON);
	}

	public static int GetCoinNumFromCurCoinSlotType()
	{
		return CabinetUtility.ExtractCoinNum(CabinetManager.Coin.Data.CoinSlot);
	}

	public static int GetCreditNumFromCurCoinSlotType()
	{
		return CabinetUtility.ExtractCreditNum(CabinetManager.Coin.Data.CoinSlot);
	}

	public static int GetCurrentCredit()
	{
		return DeviceManager.Instance.Coin().Credit;
	}

	public static int GetCurrentFractionCoin()
	{
		return DeviceManager.Instance.Coin().FractionCoin;
	}

	public static int CalcCreditAndFractionCoin(int coin, ref int credit, ref int fraction_coin)
	{
		switch (CabinetManager.Coin.Data.CoinSlot)
		{
		case CoinOptionsDef.CoinSlot.Coin01Credit1:
			credit = coin;
			fraction_coin = 0;
			break;
		case CoinOptionsDef.CoinSlot.Coin01Credit2:
			credit = 2 * coin;
			fraction_coin = 0;
			break;
		case CoinOptionsDef.CoinSlot.Coin01Credit3:
			credit = 3 * coin;
			fraction_coin = 0;
			break;
		case CoinOptionsDef.CoinSlot.Coin02Credit1:
			credit = coin / 2;
			fraction_coin = coin % 2;
			break;
		case CoinOptionsDef.CoinSlot.Coin02Credit3:
			credit = 3 * (coin / 2);
			fraction_coin = coin % 2;
			break;
		case CoinOptionsDef.CoinSlot.Coin03Credit1:
			credit = coin / 3;
			fraction_coin = coin % 3;
			break;
		case CoinOptionsDef.CoinSlot.Coin03Credit2:
			credit = 2 * (coin / 3);
			fraction_coin = coin % 3;
			break;
		case CoinOptionsDef.CoinSlot.Coin04Credit1:
			credit = coin / 4;
			fraction_coin = coin % 4;
			break;
		case CoinOptionsDef.CoinSlot.Coin04Credit3:
			credit = 3 * (coin / 4);
			fraction_coin = coin % 4;
			break;
		case CoinOptionsDef.CoinSlot.Coin05Credit1:
			credit = coin / 5;
			fraction_coin = coin % 5;
			break;
		case CoinOptionsDef.CoinSlot.Coin05Credit2:
			credit = 2 * (coin / 5);
			fraction_coin = coin % 5;
			break;
		case CoinOptionsDef.CoinSlot.Coin06Credit1:
			credit = coin / 6;
			fraction_coin = coin % 6;
			break;
		case CoinOptionsDef.CoinSlot.Coin07Credit1:
			credit = coin / 7;
			fraction_coin = coin % 7;
			break;
		case CoinOptionsDef.CoinSlot.Coin08Credit1:
			credit = coin / 8;
			fraction_coin = coin % 8;
			break;
		case CoinOptionsDef.CoinSlot.Coin09Credit1:
			credit = coin / 9;
			fraction_coin = coin % 9;
			break;
		case CoinOptionsDef.CoinSlot.Coin10Credit1:
			credit = coin / 10;
			fraction_coin = coin % 10;
			break;
		default:
			credit = coin;
			fraction_coin = 0;
			break;
		}
		return 0;
	}

	public static CONSUME_MODE GetConsumeMode(GAME_MODE mode, CONSUME_TYPE type)
	{
		switch (mode)
		{
		case GAME_MODE.START:
			if (type != CONSUME_TYPE.CREDIT)
			{
				return CONSUME_MODE.START_PASELI;
			}
			return CONSUME_MODE.START_CREDIT;
		case GAME_MODE.CONTINUE:
			return type switch
			{
				CONSUME_TYPE.FREE => CONSUME_MODE.CONTINUE_FREE, 
				CONSUME_TYPE.CREDIT => CONSUME_MODE.CONTINUE_CREDIT, 
				_ => CONSUME_MODE.CONTINUE_PASELI, 
			};
		case GAME_MODE.GACHA:
			return type switch
			{
				CONSUME_TYPE.TICKET => CONSUME_MODE.GACHA_TICKET_EX, 
				CONSUME_TYPE.CREDIT => CONSUME_MODE.GACHA_CREDIT, 
				_ => CONSUME_MODE.GACHA_PASELI, 
			};
		case GAME_MODE.GACHA5:
			return type switch
			{
				CONSUME_TYPE.TICKET => CONSUME_MODE.GACHA5_TICKET_EX, 
				CONSUME_TYPE.CREDIT => CONSUME_MODE.GACHA5_CREDIT, 
				_ => CONSUME_MODE.GACHA5_PASELI, 
			};
		case GAME_MODE.JANDRI:
			if (type != CONSUME_TYPE.CREDIT)
			{
				return CONSUME_MODE.JANDRI_PASELI;
			}
			return CONSUME_MODE.JANDRI_CREDIT;
		case GAME_MODE.PREMIUMSTART:
			if (type != CONSUME_TYPE.CREDIT)
			{
				return CONSUME_MODE.PREMIUM_START_PASELI;
			}
			return CONSUME_MODE.PREMIUM_START_CREDIT;
		case GAME_MODE.PREMIUMCONTINUE:
			if (type != CONSUME_TYPE.CREDIT)
			{
				return CONSUME_MODE.PREMIUM_CONTINUE_PASELI;
			}
			return CONSUME_MODE.PREMIUM_CONTINUE_CREDIT;
		case GAME_MODE.MUSICGACHA:
			if (type != CONSUME_TYPE.CREDIT)
			{
				return CONSUME_MODE.MUSIC_GACHA_PASELI;
			}
			return CONSUME_MODE.MUSIC_GACHA_CREDIT;
		default:
			return CONSUME_MODE.NONE;
		}
	}

	public static CONSUME_MODE GetConsumeMode(GAME_TYPE game, PRICE_TYPE price, CONSUME_TYPE type)
	{
		return GetConsumeMode(GetGameMode(game, price), type);
	}

	public static GAME_MODE GetGameMode(GAME_TYPE game, PRICE_TYPE price)
	{
		if (price == PRICE_TYPE.NONE)
		{
			return GAME_MODE.NONE;
		}
		return game switch
		{
			GAME_TYPE.START => GAME_MODE.START, 
			GAME_TYPE.CONTINUE => GAME_MODE.CONTINUE, 
			GAME_TYPE.GACHA => GAME_MODE.GACHA, 
			GAME_TYPE.GACHA5 => GAME_MODE.GACHA5, 
			GAME_TYPE.JANDRI => GAME_MODE.JANDRI, 
			GAME_TYPE.PREMIUMSTART => GAME_MODE.PREMIUMSTART, 
			GAME_TYPE.PREMIUMCONTINUE => GAME_MODE.PREMIUMCONTINUE, 
			GAME_TYPE.MUSICGACHA => GAME_MODE.MUSICGACHA, 
			_ => GAME_MODE.NONE, 
		};
	}

	public static GAME_TYPE GetGameType(GAME_MODE mode)
	{
		return mode switch
		{
			GAME_MODE.START => GAME_TYPE.START, 
			GAME_MODE.CONTINUE => GAME_TYPE.CONTINUE, 
			GAME_MODE.GACHA => GAME_TYPE.GACHA, 
			GAME_MODE.GACHA5 => GAME_TYPE.GACHA5, 
			GAME_MODE.PREMIUMSTART => GAME_TYPE.PREMIUMSTART, 
			GAME_MODE.PREMIUMCONTINUE => GAME_TYPE.PREMIUMCONTINUE, 
			GAME_MODE.MUSICGACHA => GAME_TYPE.MUSICGACHA, 
			_ => GAME_TYPE.NONE, 
		};
	}

	public static CONSUME_TYPE GetConsumeMode(CONSUME_MODE mode)
	{
		string text = mode.ToString();
		if (text.EndsWith("_CREDIT"))
		{
			return CONSUME_TYPE.CREDIT;
		}
		if (text.EndsWith("_PASELI"))
		{
			return CONSUME_TYPE.PASELI;
		}
		return CONSUME_TYPE.NONE;
	}

	public static int GetPrice(GAME_MODE mode, CONSUME_TYPE type, bool isFreeContinue = false)
	{
		if (IsFreePlay())
		{
			return 0;
		}
		CONSUME_MODE consumeMode = GetConsumeMode(mode, type);
		return consumeMode switch
		{
			CONSUME_MODE.PREMIUM_START_CREDIT => GetPrice(GetConsumeMode(mode, CONSUME_TYPE.CREDIT)) + GetPrice(GetConsumeMode(GAME_MODE.START, CONSUME_TYPE.CREDIT)), 
			CONSUME_MODE.PREMIUM_CONTINUE_CREDIT => ((!isFreeContinue) ? GetPrice(GetConsumeMode(GAME_MODE.CONTINUE, CONSUME_TYPE.CREDIT)) : 0) + GetPrice(GetConsumeMode(mode, CONSUME_TYPE.CREDIT)), 
			_ => GetPrice(consumeMode), 
		};
	}

	public static bool IsEnoughCredit(GAME_MODE mode, bool isFreeContinue = false)
	{
		int num = mode switch
		{
			GAME_MODE.PREMIUMSTART => GetPrice(GetConsumeMode(mode, CONSUME_TYPE.CREDIT)) + GetPrice(GetConsumeMode(GAME_MODE.START, CONSUME_TYPE.CREDIT)), 
			GAME_MODE.PREMIUMCONTINUE => ((!isFreeContinue) ? GetPrice(GetConsumeMode(GAME_MODE.CONTINUE, CONSUME_TYPE.CREDIT)) : 0) + GetPrice(GetConsumeMode(mode, CONSUME_TYPE.CREDIT)), 
			_ => GetPrice(GetConsumeMode(mode, CONSUME_TYPE.CREDIT)), 
		};
		if (!IsFreePlay())
		{
			return GetCurrentCredit() >= num;
		}
		return true;
	}

	public static int GetPrice(CONSUME_MODE mode)
	{
		return mode switch
		{
			CONSUME_MODE.START_CREDIT => (int)(CabinetManager.Coin.Data.Start + 1), 
			CONSUME_MODE.START_PASELI => GetPaseliPrice(GAME_MODE.START), 
			CONSUME_MODE.CONTINUE_CREDIT => (int)(CabinetManager.Coin.Data.Start + 1), 
			CONSUME_MODE.CONTINUE_PASELI => GetPaseliPrice(GAME_MODE.CONTINUE), 
			CONSUME_MODE.GACHA_CREDIT => (int)(CabinetManager.Coin.Data.Gacha + 1), 
			CONSUME_MODE.GACHA_PASELI => GetPaseliPrice(GAME_MODE.GACHA), 
			CONSUME_MODE.GACHA5_CREDIT => (int)(CabinetManager.Coin.Data.Gacha5 + 1), 
			CONSUME_MODE.GACHA5_PASELI => GetPaseliPrice(GAME_MODE.GACHA5), 
			CONSUME_MODE.JANDRI_PASELI => GetPaseliPrice(GAME_MODE.JANDRI), 
			CONSUME_MODE.PREMIUM_START_PASELI => GetPaseliPrice(GAME_MODE.PREMIUMSTART), 
			CONSUME_MODE.PREMIUM_CONTINUE_PASELI => GetPaseliPrice(GAME_MODE.PREMIUMCONTINUE), 
			CONSUME_MODE.PREMIUM_START_CREDIT => (int)(CabinetManager.Coin.Data.PremiumStart + 1), 
			CONSUME_MODE.PREMIUM_CONTINUE_CREDIT => (int)(CabinetManager.Coin.Data.PremiumStart + 1), 
			CONSUME_MODE.MUSIC_GACHA_PASELI => GetPaseliPrice(GAME_MODE.MUSICGACHA), 
			_ => 0, 
		};
	}

	private static int GetPaseliPrice(GAME_MODE mode)
	{
		if (IsKamunityBootEnable())
		{
			EaCoin.PathData pathData = EaCoin.ConsumePaseliEventArticles[mode];
			Debug.Log($"GetPaseliPrice: {mode} / articleInfo: {pathData.paseli} / price : {AvsEa3Coin.Ea3EacoinQueryPrice(pathData.paseli)}");
			return AvsEa3Coin.Ea3EacoinQueryPrice(pathData.paseli);
		}
		try
		{
			return CabinetManager.VirtualCoin.CurrentPrice.GetPrice(ToGameContents(mode));
		}
		catch
		{
			Debug.LogWarning($"対応するコンテンツ情報がありません: GAME_MODE:{mode}");
			return 0;
		}
	}

	private static GameContentsInfo.GameContents ToGameContents(GAME_MODE mode)
	{
		return mode switch
		{
			GAME_MODE.START => GameContentsInfo.GameContents.Start, 
			GAME_MODE.CONTINUE => GameContentsInfo.GameContents.Continue, 
			GAME_MODE.GACHA => GameContentsInfo.GameContents.Gacha, 
			GAME_MODE.GACHA5 => GameContentsInfo.GameContents.Gacha5, 
			GAME_MODE.JANDRI => throw new NotImplementedException(), 
			GAME_MODE.PREMIUMSTART => GameContentsInfo.GameContents.PremiumStart, 
			GAME_MODE.PREMIUMCONTINUE => GameContentsInfo.GameContents.PremiumContinue, 
			GAME_MODE.MUSICGACHA => throw new NotImplementedException(), 
			_ => throw new NotImplementedException(), 
		};
	}

	public static void ComsumeCredit(GAME_MODE mode, int num = 1, bool isFreeContinue = false)
	{
		int credit = GetPrice(mode, CONSUME_TYPE.CREDIT, isFreeContinue) * num;
		DeviceManager.Instance.Coin().ConsumeCredit(credit);
		if (!IsKamunityBootEnable())
		{
			return;
		}
		CONSUME_MODE consumeMode = GetConsumeMode(mode, CONSUME_TYPE.CREDIT);
		if (GameDataManager.Account.IsGuestUser)
		{
			SendPcbEvent(PCB_EVENT.EVENT.GUEST_START);
		}
		if (IsFreePlay())
		{
			SendPcbEvent(PCB_EVENT.EVENT.GAME_START_FREE);
			return;
		}
		switch (consumeMode)
		{
		case CONSUME_MODE.PREMIUM_START_CREDIT:
			SendPcbConsumeCreditEvent(CONSUME_MODE.START_CREDIT, num);
			break;
		case CONSUME_MODE.PREMIUM_CONTINUE_CREDIT:
			if (!isFreeContinue)
			{
				SendPcbConsumeCreditEvent(CONSUME_MODE.CONTINUE_CREDIT, num);
			}
			break;
		}
		SendPcbConsumeCreditEvent(consumeMode, num);
	}

	public static void SendPcbEvent(PCB_EVENT.EVENT mode, int num = 1)
	{
		if (IsKamunityBootEnable())
		{
			Debug.Log($"SendPcbEvent {mode}");
			AvsEa3.Ea3PosevReportWithNum(PCB_EVENT.EVENT_PATH[mode], num);
		}
	}

	public static void SendPcbGameModeStartEvent(TakuType takuType)
	{
		switch (GameDataManager.Mahjong.TakuType)
		{
		case TakuType.Nima:
			SendPcbEvent(PCB_EVENT.EVENT.GAME_MODE_NIMA);
			break;
		case TakuType.Sanma:
			SendPcbEvent(PCB_EVENT.EVENT.GAME_MODE_SANMA);
			break;
		case TakuType.Tonpu:
			SendPcbEvent(PCB_EVENT.EVENT.GAME_MODE_YONMA);
			break;
		default:
			Debug.LogError("想定外のケース");
			break;
		}
	}

	public static void SendPcbConsumeCreditEvent(CONSUME_MODE mode, int num = 1)
	{
		if (IsKamunityBootEnable())
		{
			Debug.Log($"SendPcbEvent {mode},{num}");
			AvsEa3.Ea3PosevReportWithNum(EaCoin.ConsumeCreditEventArticles[mode], num);
		}
	}

	public static void SendPpEvent(CONSUME_MODE mode, int num = 1)
	{
		if (IsKamunityBootEnable() && EaCoin.ConsumeParticipationEventArticles.ContainsKey(mode))
		{
			KAMUNITY.Logger.Log($"send pp event --> mode:{mode} num:{num}");
			AvsEa3.Ea3PosevReportWithNum(EaCoin.ConsumeParticipationEventArticles[mode], num);
		}
	}

	public static async UniTask SendConsumeLog(CONSUME_MODE mode, int num = 1)
	{
		if (IsKamunityBootEnable())
		{
			XrpcResponseHelper<TcsXrpcModuleTypes.GetConsumeLogResponse> xrpcResponseHelper = new XrpcResponseHelper<TcsXrpcModuleTypes.GetConsumeLogResponse>();
			TcsXrpcModule.XrpcMethodGetConsumeLog.Request(mode, num, xrpcResponseHelper.Callback, null);
			await xrpcResponseHelper.CoWaitForXrpcFinish();
		}
	}

	public static string GetGameModeStr(GAME_TYPE type)
	{
		return type switch
		{
			GAME_TYPE.START => "ノーマルプレー料金", 
			GAME_TYPE.CONTINUE => "ノーマルコンティニュー料金", 
			GAME_TYPE.GACHA => "1回ガチャ料金", 
			GAME_TYPE.GACHA5 => "5回ガチャ料金", 
			GAME_TYPE.JANDRI => "雀ドリ料金", 
			GAME_TYPE.PREMIUMSTART => "プレミアムプレー料金", 
			GAME_TYPE.PREMIUMCONTINUE => "プレミアムコンティニュー料金", 
			GAME_TYPE.MUSICGACHA => "リーチソング＆ボイスガチャ料金", 
			_ => "料金", 
		};
	}

	public static string GetGameModeStr(GAME_MODE mode)
	{
		return GetGameModeStr(GetGameType(mode));
	}

	public static bool IsKamunityBootEnable()
	{
		return !IsKonamiNetConnection;
	}

	public static async UniTask OnBeginSkipBootScene(CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
	}

	public static void OnBeginFirstGameFlow()
	{
		PaseliUtility.Clear();
		Bi2xInput.Instance.ICReaderOn();
		Bi2xInput.Instance.SetPillarLED(new Color32(127, 30, 40, byte.MaxValue));
		DeviceManager.Instance.AllActivate(bActive: true);
		ResidentFlow instance = ResidentFlow.GetInstance();
		instance.IsTestModeRequest = false;
		instance.IsTestModeEffective = true;
		instance.IsErrorModeEffective = true;
		IsReservedGameOver = false;
		CreditCoinDisp.IsForceDisp = true;
	}

	public static string GetBuildDateString()
	{
		if (!s_isBuildVersionInfoLoadTried)
		{
			s_buildVersionInfo = GameBuildVersionInfo.LoadBuildVersionInfo();
			s_isBuildVersionInfoLoadTried = true;
		}
		if (s_buildVersionInfo == null)
		{
			return "";
		}
		return s_buildVersionInfo.BuildDateTimeString;
	}
}
