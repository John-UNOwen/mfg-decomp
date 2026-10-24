using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameContents;
using KAMUNITY;
using MFG.Sytem.Utilities;
using TestModeSkeleton;
using TestModeSkeleton.CabinetDef;
using TestModeSkeleton.Price;
using UnityEngine;

public class PaseliUtility
{
	public static class PPEX
	{
		public static bool IsEnablePPEX
		{
			get
			{
				if (GameUtility.IsKamunityBootEnable())
				{
					return AvsEa3Coin.Ea3EacoinQueryConsignIsValid() == 1;
				}
				return false;
			}
		}

		public static bool IsEnablePurchaseItem
		{
			get
			{
				if (IsEnablePaseli)
				{
					return IsEnablePPEX;
				}
				return false;
			}
		}

		public static List<AvsEa3CoinTypes.ea3_eacoin_pricelist_article> RefleshPrice()
		{
			string sLOT_PATH = EaCoin.SLOT_PATH;
			List<AvsEa3CoinTypes.ea3_eacoin_pricelist_article> list = new List<AvsEa3CoinTypes.ea3_eacoin_pricelist_article>();
			if (AvsEa3Coin.Ea3EacoinHoldPrice(sLOT_PATH) != 0)
			{
				return list;
			}
			if (AvsEa3Coin.Ea3EacoinOpenPricelist(sLOT_PATH) != 0)
			{
				return list;
			}
			AvsEa3CoinTypes.ea3_eacoin_pricelist_article article;
			while (AvsEa3Coin.Ea3EacoinNextPricelistArticle(sLOT_PATH) == 1 && AvsEa3Coin.Ea3EacoinGetPricelistArticle(sLOT_PATH, out article) == 0)
			{
				list.Add(article);
			}
			AvsEa3Coin.Ea3EacoinClosePricelist(sLOT_PATH);
			return list;
		}
	}

	private const int RETRY_COUNT_MAX = 2;

	private static int m_shortBalanceRetryCount;

	public static bool IsCheckinProcessFinished { get; private set; }

	public static int LastConsumedPrice { get; private set; }

	public static int LastNotch { get; private set; }

	public static bool IsDispPaseliBalance { get; set; }

	public static bool IsEnablePaseli
	{
		get
		{
			if (IsCheckinProcessFinished)
			{
				return IsSessionOpened();
			}
			return false;
		}
	}

	public static void Clear()
	{
		IsCheckinProcessFinished = false;
		m_shortBalanceRetryCount = 0;
		LastNotch = 0;
		LastConsumedPrice = 0;
	}

	public static bool IsEnable()
	{
		if (!GameUtility.IsKamunityBootEnable())
		{
			return true;
		}
		if (SystemUtility.Instance.RegionType != 0)
		{
			return false;
		}
		if (GameUtility.IsEventMode())
		{
			return false;
		}
		return AvsEa3Coin.Ea3EacoinGetStatus() == AvsTypes.EA3_EACOIN_STATUS.AVAILABLE;
	}

	public static int GetBalance()
	{
		if (!GameUtility.IsKamunityBootEnable())
		{
			return 1000;
		}
		if (IsSessionOpened())
		{
			return (int)AvsEa3Coin.Ea3EacoinQueryBalance(EaCoin.SLOT_PATH);
		}
		return 0;
	}

	public static int GetExCoin()
	{
		if (!GameUtility.IsKamunityBootEnable())
		{
			return 1000;
		}
		return AvsEa3Coin.Ea3EacoinQueryExtra(EaCoin.SLOT_PATH);
	}

	public static AvsTypes.EA3_EACOIN_STATUS GetPcbStatus()
	{
		if (!GameUtility.IsKamunityBootEnable())
		{
			return AvsTypes.EA3_EACOIN_STATUS.AVAILABLE;
		}
		AvsTypes.EA3_EACOIN_STATUS eA3_EACOIN_STATUS = AvsEa3Coin.Ea3EacoinGetStatus();
		if (eA3_EACOIN_STATUS == AvsTypes.EA3_EACOIN_STATUS.AVAILABLE && (GameUtility.IsFreePlay() || GameUtility.IsEventMode()))
		{
			return AvsTypes.EA3_EACOIN_STATUS.NOT_AVAILABLE;
		}
		return eA3_EACOIN_STATUS;
	}

	public static AvsTypes.EA3_EACOIN_SESSION GetSessionStatus()
	{
		if (!GameUtility.IsKamunityBootEnable())
		{
			return AvsTypes.EA3_EACOIN_SESSION.OPEN;
		}
		return AvsEa3Coin.Ea3EacoinQuerySessionState(EaCoin.SLOT_PATH);
	}

	public static bool IsShortBalanceRetryEnable()
	{
		if (m_shortBalanceRetryCount < 2)
		{
			return true;
		}
		return false;
	}

	public static void CheckOut()
	{
		if (IsSessionOpened())
		{
			AvsEa3Coin.Ea3EacoinCheckout(EaCoin.SLOT_PATH);
		}
		m_shortBalanceRetryCount = 0;
	}

	public static void CheckInRequest(AvsTypes.CardType type, string card_id, string password)
	{
		if (GameUtility.IsKamunityBootEnable())
		{
			AvsEa3Coin.Ea3EacoinCheckin(EaCoin.SLOT_PATH, type, card_id, password);
			LastConsumedPrice = 0;
			LastNotch = 0;
		}
	}

	public static bool CheckInStatus(out bool sucess, out AvsEa3CoinTypes.EA3_EACOIN_ERROR status)
	{
		if (!GameUtility.IsKamunityBootEnable() || ResidentFlow.CustomInput.GetMouseButtonDown(0))
		{
			if (Input.GetKeyDown(KeyCode.A))
			{
				status = (AvsEa3CoinTypes.EA3_EACOIN_ERROR)0;
			}
			else if (Input.GetKeyDown(KeyCode.S))
			{
				status = AvsEa3CoinTypes.EA3_EACOIN_ERROR.ECINVALID;
			}
			else
			{
				status = AvsEa3CoinTypes.EA3_EACOIN_ERROR.PROCESSING;
			}
		}
		else
		{
			status = AvsEa3Coin.Ea3EacoinQueryCheckinStatus(EaCoin.SLOT_PATH);
		}
		sucess = false;
		switch (status)
		{
		case AvsEa3CoinTypes.EA3_EACOIN_ERROR.PROCESSING:
			return false;
		case (AvsEa3CoinTypes.EA3_EACOIN_ERROR)0:
			sucess = true;
			break;
		}
		IsCheckinProcessFinished = true;
		return true;
	}

	public static bool IsReauthEnable()
	{
		if (!GameUtility.IsKamunityBootEnable())
		{
			return true;
		}
		if (IsSessionOpened() && AvsEa3Coin.Ea3EacoinQueryConsumeExcess(EaCoin.SLOT_PATH) == (AvsEa3CoinTypes.EA3_EACOIN_ERROR)0)
		{
			return true;
		}
		return false;
	}

	public static bool IsNeedReauth()
	{
		if (!GameUtility.IsKamunityBootEnable())
		{
			return false;
		}
		AvsEa3CoinTypes.EA3_EACOIN_ERROR eA3_EACOIN_ERROR = AvsEa3Coin.Ea3EacoinQuertReauthNeed(EaCoin.SLOT_PATH);
		Debug.Log($"ea3_eacoin_query_reauth_need: ret={eA3_EACOIN_ERROR}");
		if (eA3_EACOIN_ERROR == AvsEa3CoinTypes.EA3_EACOIN_ERROR.PROCESSING)
		{
			return true;
		}
		return false;
	}

	public static bool Reauth(string password)
	{
		if (!GameUtility.IsKamunityBootEnable())
		{
			return true;
		}
		return AvsEa3Coin.Ea3EacoinReauth(EaCoin.SLOT_PATH, password) == (AvsEa3CoinTypes.EA3_EACOIN_ERROR)0;
	}

	public static int GetLastNotch()
	{
		if (!GameUtility.IsKamunityBootEnable())
		{
			LastNotch = 100;
			return 100;
		}
		if (IsSessionOpened())
		{
			LastNotch = (int)AvsEa3Coin.Ea3EacoinQueryLastNotch(EaCoin.SLOT_PATH);
		}
		return LastNotch;
	}

	public static int GetTotalConsume()
	{
		if (!GameUtility.IsKamunityBootEnable())
		{
			return 10;
		}
		return (int)AvsEa3Coin.Ea3EacoinQueryTotalConsume(EaCoin.SLOT_PATH);
	}

	public static int GetNotchAmount()
	{
		if (!GameUtility.IsKamunityBootEnable())
		{
			return 10;
		}
		return AvsEa3Coin.Ea3EacoinQueryNotchAmount();
	}

	public static int GetNotchCount()
	{
		if (!GameUtility.IsKamunityBootEnable())
		{
			return 3;
		}
		return AvsEa3Coin.Ea3EacoinQueryNotchCount();
	}

	public static void AddShortBalanceRetryCount()
	{
		m_shortBalanceRetryCount++;
	}

	public static bool IsSessionOpened()
	{
		if (!GameUtility.IsKamunityBootEnable())
		{
			return false;
		}
		return AvsEa3Coin.Ea3EacoinQuerySessionIsOpen(EaCoin.SLOT_PATH);
	}

	public static void ComsumePaseli(GAME_MODE mode)
	{
		if (GameUtility.IsKamunityBootEnable())
		{
			EaCoin.PathData pathData = EaCoin.ConsumePaseliEventArticles[mode];
			if (pathData.IsPPEXItem)
			{
				AvsEa3Coin.Ea3EacoinIconsume(pathData.paseli);
			}
			else
			{
				AvsEa3Coin.Ea3EacoinConsume(EaCoin.ConsumePaseliEventArticles[mode].paseli);
			}
		}
	}

	public static AvsEa3CoinTypes.EA3_EACOIN_ERROR ComsumePaseliWithPrice(GAME_MODE mode, int price)
	{
		if (!GameUtility.IsKamunityBootEnable())
		{
			return AvsEa3CoinTypes.EA3_EACOIN_ERROR.UNEXPECTED;
		}
		EaCoin.PathData pathData = EaCoin.ConsumePaseliEventArticles[mode];
		if (pathData.IsPPEXItem)
		{
			return (AvsEa3CoinTypes.EA3_EACOIN_ERROR)AvsEa3Coin.Ea3EacoinIconsume(pathData.paseli);
		}
		return (AvsEa3CoinTypes.EA3_EACOIN_ERROR)AvsEa3Coin.Ea3EacoinConsumeWithPrice(pathData.paseli, price);
	}

	public static AvsEa3CoinTypes.EA3_EACOIN_ERROR GetComsumePaseliStatus(GAME_MODE mode)
	{
		if (!GameUtility.IsKamunityBootEnable())
		{
			if (Input.GetKeyDown(KeyCode.A) || ResidentFlow.CustomInput.GetMouseButtonDown(0))
			{
				return (AvsEa3CoinTypes.EA3_EACOIN_ERROR)0;
			}
			if (Input.GetKeyDown(KeyCode.S))
			{
				return AvsEa3CoinTypes.EA3_EACOIN_ERROR.NOACCOUNT;
			}
			if (Input.GetKeyDown(KeyCode.D))
			{
				return AvsEa3CoinTypes.EA3_EACOIN_ERROR.SHORTBALANCE;
			}
			if (Input.GetKeyDown(KeyCode.F))
			{
				return AvsEa3CoinTypes.EA3_EACOIN_ERROR.MAINTENANCE;
			}
			return AvsEa3CoinTypes.EA3_EACOIN_ERROR.PROCESSING;
		}
		return AvsEa3Coin.Ea3EacoinQueryConsumeStatus(EaCoin.ConsumePaseliEventArticles[mode].paseli);
	}

	public static async UniTask<bool> RefleshPrice(Fraction continuePriceRate, bool isFetchExternalBasicRateInfo)
	{
		GameEventType externalBasicRateEventType = GameEventType.ExternalBasicRate;
		if (GameUtility.IsNetcafeBehavior && isFetchExternalBasicRateInfo)
		{
			await GameDataManager.System.GameControlInfo.Modify(isForceModify: true, suppressException: false);
		}
		if (GameUtility.IsNetcafeBehavior && GameDataManager.System.GameControlInfo.EventStatus.IsActive(externalBasicRateEventType))
		{
			GameEventData gameEventData = GameDataManager.System.GameControlInfo.EventStatus.GetGameEventData(externalBasicRateEventType);
			int playPriceBasicRate = gameEventData.ParamValue("play_rate", 1000);
			int gachaPriceBasicRate = gameEventData.ParamValue("gacha_rate", 1000);
			int musicGachaPriceBasicRate = gameEventData.ParamValue("music_gacha_rate", 1000);
			PriceInfo priceInfo = PriceUtility.CalcPaseliPrice(CabinetManager.Coin.Data, playPriceBasicRate, gachaPriceBasicRate, musicGachaPriceBasicRate, VirtualCoinDef.BalanceOfCredit.MoreImbalance, continuePriceRate, isTimeServiceActive: false, 0);
			return CabinetManager.VirtualCoin.RefleshPrice(priceInfo, isTimeServiceActive: false);
		}
		return CabinetManager.VirtualCoin.RefleshPrice(CabinetManager.Clock.GetTime(), CabinetManager.Coin.Data, continuePriceRate);
	}
}
