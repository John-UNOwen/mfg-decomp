using System;
using System.Runtime.InteropServices;
using System.Text;

namespace KAMUNITY;

[KamunityDllApiClass]
public static class AvsEa3Coin
{
	[KamunityDllApiClass]
	internal static class InternalMethod
	{
		public delegate AvsEa3CoinTypes.EA3_EACOIN_ERROR FUNC_Ea3EacoinCheckin(string slotpath, string ectype, string cardtype, string cardid, string passwd);

		public delegate AvsEa3CoinTypes.EA3_EACOIN_ERROR FUNC_Ea3EacoinReport(string reportpath);

		public delegate AvsEa3CoinTypes.EA3_EACOIN_ERROR FUNC_Ea3EacoinPlayerlog(string cardtype, string cardid, string passwd);

		public static FUNC_Ea3EacoinCheckin Ea3EacoinCheckin { get; private set; }

		public static FUNC_Ea3EacoinReport Ea3EacoinReport { get; private set; }

		public static FUNC_Ea3EacoinPlayerlog Ea3EacoinPlayerlog { get; private set; }
	}

	public delegate AvsTypes.EA3_EACOIN_STATUS FUNC_Ea3EacoinGetStatus();

	[Obsolete("use FUNC_Ea3EacoinGetStatus")]
	public delegate AvsTypes.EA3_EACOIN_STATUS FUNC_Ea3EaCoinGetStatus();

	public delegate AvsTypes.EA3_EACOIN_SESSION FUNC_Ea3EacoinQuerySessionState(string slotpath);

	public delegate AvsEa3CoinTypes.EA3_EACOIN_ERROR FUNC_Ea3EacoinCheckout(string slotpath);

	public delegate int FUNC_Ea3EacoinConsume(string articlepath);

	public delegate int FUNC_Ea3EacoinIconsume(string articlepath);

	public delegate int FUNC_Ea3EacoinConsumeWithPrice(string articlepath, int price);

	public delegate AvsEa3CoinTypes.EA3_EACOIN_ERROR FUNC_Ea3EacoinQueryCheckinStatus(string slotpath);

	public delegate AvsEa3CoinTypes.EA3_EACOIN_ERROR FUNC_Ea3EacoinQueryConsumeStatus(string articlepath);

	public delegate AvsEa3CoinTypes.EA3_EACOIN_ERROR FUNC_Ea3EacoinQueryBalance(string slotpath);

	public delegate AvsEa3CoinTypes.EA3_EACOIN_ERROR FUNC_Ea3EacoinQueryPoint(string slotpath);

	public delegate AvsEa3CoinTypes.EA3_EACOIN_ERROR FUNC_Ea3EacoinQueryAutochargeStatus(string slotpath);

	public delegate int FUNC_Ea3EacoinQueryAutochargeIsTriggerd(string slotpath);

	public delegate int FUNC_Ea3EacoinQueryInshopchargeStatus(string slotpath);

	public delegate int FUNC_Ea3EacoinQueryInshopchargeIsDetected(string slotpath);

	public delegate int FUNC_Ea3EacoinQueryPrice(string articlepath);

	public delegate AvsEa3CoinTypes.EA3_EACOIN_ERROR FUNC_Ea3EacoinSupplyExtra(string slotpath, int extracoin);

	public delegate int FUNC_Ea3EacoinQueryExtra(string slotpath);

	public delegate int FUNC_Ea3EacoinQueryNotchAmount();

	public delegate int FUNC_Ea3EacoinQueryNotchCount();

	public delegate int FUNC_Ea3EacoinQuerySupplyLimit();

	public delegate AvsEa3CoinTypes.EA3_EACOIN_ERROR FUNC_Ea3EacoinQueryTotalConsume(string slotpath);

	public delegate AvsEa3CoinTypes.EA3_EACOIN_ERROR FUNC_Ea3EacoinQueryLastNotch(string slotpath);

	public delegate AvsEa3CoinTypes.EA3_EACOIN_ERROR FUNC_Ea3EacoinQueryConsumeExcess(string slotpath);

	public delegate AvsEa3CoinTypes.EA3_EACOIN_ERROR FUNC_Ea3EacoinQuertReauthNeed(string slotpath);

	public delegate AvsEa3CoinTypes.EA3_EACOIN_ERROR FUNC_Ea3EacoinReauth(string slotpath, string passwd);

	public delegate int FUNC_Ea3EacoinOpchpass(string passwd, string newpasswd);

	public delegate AvsEa3CoinTypes.EA3_EACOIN_ERROR FUNC_Ea3EacoinQuertGeneralStatus();

	public delegate AvsEa3CoinTypes.EA3_EACOIN_ERROR FUNC_Ea3EacoinAbort();

	public delegate AvsEa3CoinTypes.EA3_EACOIN_ERROR FUNC_Ea3EacoinQueryAbortStatus();

	public delegate int FUNC_Ea3EacoinQueryEacoinIsContracted();

	public delegate int FUNC_Ea3EacoinQueryConsignIsContracted();

	public delegate int FUNC_Ea3EacoinQueryConsignIsValid();

	public delegate int FUNC_Ea3EacoinIncomelog(string passwd);

	public delegate void FUNC_Ea3EacoinIncomelogGet(out AvsEa3CoinTypes.ea3_eacoin_logs logs);

	public delegate void FUNC_Ea3EacoinPlayerlogGet(out AvsEa3CoinTypes.ea3_eacoin_log_detail logDetail);

	public delegate int FUNC_Ea3EacoinHoldPrice(string slotpath);

	public delegate int FUNC_Ea3EacoinSetPricelistPeriodFilter(string slotpath, int mask);

	public delegate int FUNC_Ea3EacoinSetPricelistShowcaseFilter(string slotpath, string showcaseid);

	public delegate int FUNC_Ea3EacoinQueryArticle([In] string articlepath, out AvsEa3CoinTypes.ea3_eacoin_pricelist_article article);

	public delegate int FUNC_Ea3EacoinOpenPricelist(string slotpath);

	public delegate int FUNC_Ea3EacoinNextPricelistArticle(string slotpath);

	public delegate int FUNC_Ea3EacoinGetPricelistArticle([In] string slotpath, out AvsEa3CoinTypes.ea3_eacoin_pricelist_article article);

	public delegate int FUNC_Ea3EacoinRewindPricelist(string slotpath);

	public delegate int FUNC_Ea3EacoinClosePricelist(string slotpath);

	public delegate int FUNC_Ea3EacoinQueryPricelistInfo([In] string slotpath, out AvsEa3CoinTypes.ea3_eacoin_pricelist_info info);

	public delegate int FUNC_Ea3EacoinQueryPricelistShowcase([In] string slotpath, [In] string showcaseid, out AvsEa3CoinTypes.ea3_eacoin_pricelist_showcase showcase);

	public delegate int FUNC_Ea3EacoinQueryPricelistCampaign([In] string slotpath, [In] int campaignno, out AvsEa3CoinTypes.ea3_eacoin_pricelist_campaign campaign);

	public delegate int FUNC_Ea3EacoinQueryPricelistArticle([In] string slotpath, [In] string articleid, out AvsEa3CoinTypes.ea3_eacoin_pricelist_article article);

	public delegate int FUNC_Ea3EacoinQueryPricelistShowcaseidArray([In] string slotpath, out AvsEa3CoinTypes.ea3_eacoin_pricelist_showcaseid_array scid_ary);

	public delegate int FUNC_Ea3EacoinQueryPricelistCampaignnoArray([In] string slotpath, out AvsEa3CoinTypes.ea3_eacoin_pricelist_campaignno_array cpno_ary);

	public delegate int FUNC_Ea3EacoinQueryPricelistPeriodArray([In] string slotpath, out AvsEa3CoinTypes.ea3_eacoin_pricelist_period_array period_ary);

	public static FUNC_Ea3EacoinGetStatus Ea3EacoinGetStatus { get; private set; }

	public static FUNC_Ea3EacoinQuerySessionState Ea3EacoinQuerySessionState { get; private set; }

	public static FUNC_Ea3EacoinCheckout Ea3EacoinCheckout { get; private set; }

	public static FUNC_Ea3EacoinConsume Ea3EacoinConsume { get; private set; }

	public static FUNC_Ea3EacoinIconsume Ea3EacoinIconsume { get; private set; }

	public static FUNC_Ea3EacoinConsumeWithPrice Ea3EacoinConsumeWithPrice { get; private set; }

	public static FUNC_Ea3EacoinQueryCheckinStatus Ea3EacoinQueryCheckinStatus { get; private set; }

	public static FUNC_Ea3EacoinQueryConsumeStatus Ea3EacoinQueryConsumeStatus { get; private set; }

	public static FUNC_Ea3EacoinQueryBalance Ea3EacoinQueryBalance { get; private set; }

	public static FUNC_Ea3EacoinQueryPoint Ea3EacoinQueryPoint { get; private set; }

	public static FUNC_Ea3EacoinQueryAutochargeStatus Ea3EacoinQueryAutochargeStatus { get; private set; }

	public static FUNC_Ea3EacoinQueryAutochargeIsTriggerd Ea3EacoinQueryAutochargeIsTriggerd { get; private set; }

	public static FUNC_Ea3EacoinQueryInshopchargeStatus Ea3EacoinQueryInshopchargeStatus { get; private set; }

	public static FUNC_Ea3EacoinQueryInshopchargeIsDetected Ea3EacoinQueryInshopchargeIsDetected { get; private set; }

	public static FUNC_Ea3EacoinQueryPrice Ea3EacoinQueryPrice { get; private set; }

	public static FUNC_Ea3EacoinSupplyExtra Ea3EacoinSupplyExtra { get; private set; }

	public static FUNC_Ea3EacoinQueryExtra Ea3EacoinQueryExtra { get; private set; }

	public static FUNC_Ea3EacoinQueryNotchAmount Ea3EacoinQueryNotchAmount { get; private set; }

	public static FUNC_Ea3EacoinQueryNotchCount Ea3EacoinQueryNotchCount { get; private set; }

	public static FUNC_Ea3EacoinQuerySupplyLimit Ea3EacoinQuerySupplyLimit { get; private set; }

	public static FUNC_Ea3EacoinQueryTotalConsume Ea3EacoinQueryTotalConsume { get; private set; }

	public static FUNC_Ea3EacoinQueryLastNotch Ea3EacoinQueryLastNotch { get; private set; }

	public static FUNC_Ea3EacoinQueryConsumeExcess Ea3EacoinQueryConsumeExcess { get; private set; }

	public static FUNC_Ea3EacoinQuertReauthNeed Ea3EacoinQuertReauthNeed { get; private set; }

	public static FUNC_Ea3EacoinReauth Ea3EacoinReauth { get; private set; }

	public static FUNC_Ea3EacoinOpchpass Ea3EacoinOpchpass { get; private set; }

	public static FUNC_Ea3EacoinQuertGeneralStatus Ea3EacoinQuertGeneralStatus { get; private set; }

	public static FUNC_Ea3EacoinAbort Ea3EacoinAbort { get; private set; }

	public static FUNC_Ea3EacoinQueryAbortStatus Ea3EacoinQueryAbortStatus { get; private set; }

	public static FUNC_Ea3EacoinQueryEacoinIsContracted Ea3EacoinQueryEacoinIsContracted { get; private set; }

	public static FUNC_Ea3EacoinQueryConsignIsContracted Ea3EacoinQueryConsignIsContracted { get; private set; }

	public static FUNC_Ea3EacoinQueryConsignIsValid Ea3EacoinQueryConsignIsValid { get; private set; }

	public static FUNC_Ea3EacoinIncomelog Ea3EacoinIncomelog { get; private set; }

	public static FUNC_Ea3EacoinIncomelogGet Ea3EacoinIncomelogGet { get; private set; }

	public static FUNC_Ea3EacoinPlayerlogGet Ea3EacoinPlayerlogGet { get; private set; }

	public static FUNC_Ea3EacoinHoldPrice Ea3EacoinHoldPrice { get; private set; }

	public static FUNC_Ea3EacoinSetPricelistPeriodFilter Ea3EacoinSetPricelistPeriodFilter { get; private set; }

	public static FUNC_Ea3EacoinSetPricelistShowcaseFilter Ea3EacoinSetPricelistShowcaseFilter { get; private set; }

	public static FUNC_Ea3EacoinQueryArticle Ea3EacoinQueryArticle { get; private set; }

	public static FUNC_Ea3EacoinOpenPricelist Ea3EacoinOpenPricelist { get; private set; }

	public static FUNC_Ea3EacoinNextPricelistArticle Ea3EacoinNextPricelistArticle { get; private set; }

	public static FUNC_Ea3EacoinGetPricelistArticle Ea3EacoinGetPricelistArticle { get; private set; }

	public static FUNC_Ea3EacoinRewindPricelist Ea3EacoinRewindPricelist { get; private set; }

	public static FUNC_Ea3EacoinClosePricelist Ea3EacoinClosePricelist { get; private set; }

	public static FUNC_Ea3EacoinQueryPricelistInfo Ea3EacoinQueryPricelistInfo { get; private set; }

	public static FUNC_Ea3EacoinQueryPricelistShowcase Ea3EacoinQueryPricelistShowcase { get; private set; }

	public static FUNC_Ea3EacoinQueryPricelistCampaign Ea3EacoinQueryPricelistCampaign { get; private set; }

	public static FUNC_Ea3EacoinQueryPricelistArticle Ea3EacoinQueryPricelistArticle { get; private set; }

	public static FUNC_Ea3EacoinQueryPricelistShowcaseidArray Ea3EacoinQueryPricelistShowcaseidArray { get; private set; }

	public static FUNC_Ea3EacoinQueryPricelistCampaignnoArray Ea3EacoinQueryPricelistCampaignnoArray { get; private set; }

	public static FUNC_Ea3EacoinQueryPricelistPeriodArray Ea3EacoinQueryPricelistPeriodArray { get; private set; }

	[Obsolete("use Ea3EacoinGetStatus")]
	public static AvsTypes.EA3_EACOIN_STATUS Ea3EaCoinGetStatus()
	{
		return Ea3EacoinGetStatus();
	}

	public static bool Ea3EacoinQuerySessionIsOpen(string slotpath)
	{
		if (Ea3EacoinQuerySessionState(slotpath) == AvsTypes.EA3_EACOIN_SESSION.OPEN)
		{
			return true;
		}
		return false;
	}

	public static AvsEa3CoinTypes.EA3_EACOIN_ERROR Ea3EacoinCheckin(string slotpath, AvsTypes.CardType cardtype, string cardid, string passwd)
	{
		string cardtype2;
		switch (cardtype)
		{
		default:
			Logger.LogWarningFormat("invalid card type {0}", cardtype);
			return AvsEa3CoinTypes.EA3_EACOIN_ERROR.UNEXPECTED;
		case AvsTypes.CardType.ICCARD:
			cardtype2 = "1";
			break;
		case AvsTypes.CardType.FELICA:
			cardtype2 = "2";
			break;
		case AvsTypes.CardType.VIRTUAL:
			cardtype2 = "4";
			break;
		case AvsTypes.CardType.CARDLESS:
			cardtype2 = "5";
			break;
		}
		return InternalMethod.Ea3EacoinCheckin(slotpath, "1", cardtype2, cardid, passwd);
	}

	public static AvsEa3CoinTypes.EA3_EACOIN_ERROR Ea3EacoinReport(AvsEa3CoinTypes.REPORT_TYPE report_type)
	{
		switch (report_type)
		{
		case AvsEa3CoinTypes.REPORT_TYPE.RESET:
			return InternalMethod.Ea3EacoinReport("/eacoin/reset");
		case AvsEa3CoinTypes.REPORT_TYPE.RELOAD_OPTION:
			return InternalMethod.Ea3EacoinReport("/eacoin/reload_opt");
		default:
			Logger.LogWarningFormat("invalid report_type : {0}", report_type);
			return AvsEa3CoinTypes.EA3_EACOIN_ERROR.UNEXPECTED;
		}
	}

	public static int Ea3MainteSearch(int scope, out int start, out int end, StringBuilder buf, int bufsize, out int trylen)
	{
		return AvsEa3.Ea3MessageSearch("sys.eacoin.mainte", scope, out start, out end, buf, bufsize, out trylen);
	}

	public static AvsEa3CoinTypes.EA3_EACOIN_ERROR Ea3EacoinPlayerlog(AvsTypes.CardType cardtype, string cardid, string passwd)
	{
		string cardtype2 = "";
		switch (cardtype)
		{
		case AvsTypes.CardType.ICCARD:
			cardtype2 = "1";
			break;
		case AvsTypes.CardType.FELICA:
			cardtype2 = "2";
			break;
		case AvsTypes.CardType.VIRTUAL:
			cardtype2 = "4";
			break;
		case AvsTypes.CardType.CARDLESS:
			cardtype2 = "5";
			break;
		}
		return InternalMethod.Ea3EacoinPlayerlog(cardtype2, cardid, passwd);
	}
}
