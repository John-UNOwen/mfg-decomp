using System.Collections.Generic;
using System.Globalization;

namespace KAMUNITY.Text;

public static class Tabooword
{
	public enum CheckStatus
	{
		Ok,
		Ng_PartialMatch,
		Ng_PerfectMatch,
		Ng_ForwardMatch,
		Ng_BackwardMatch
	}

	public class CheckResut
	{
		public CheckStatus status;

		public string taboo_word;
	}

	public static CheckResut Check(string check_word, TaboowordListAsset tbw_asset, CompareOptions opt = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth)
	{
		return Check(check_word, tbw_asset.m_PartialMatchList, tbw_asset.m_PerfectMatchList, tbw_asset.m_ForwardMatchList, tbw_asset.m_BackwardMatchList, tbw_asset.m_WhiteList, opt);
	}

	public static CheckResut Check(string check_word, List<string> partialMatchList, List<string> perfectMatchList, List<string> forwardMatchList, List<string> backwardMatchList, List<string> whiteList, CompareOptions opt = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth)
	{
		CheckResut checkResut = new CheckResut();
		checkResut.status = CheckStatus.Ok;
		checkResut.taboo_word = "";
		CompareInfo compareInfo = CultureInfo.CurrentCulture.CompareInfo;
		if (partialMatchList != null)
		{
			foreach (string partialMatch in partialMatchList)
			{
				if (compareInfo.IndexOf(check_word, partialMatch, opt) >= 0)
				{
					checkResut.status = CheckStatus.Ng_PartialMatch;
					checkResut.taboo_word = partialMatch;
					break;
				}
			}
		}
		if (perfectMatchList != null)
		{
			foreach (string perfectMatch in perfectMatchList)
			{
				if (compareInfo.Compare(check_word, perfectMatch, opt) == 0)
				{
					checkResut.status = CheckStatus.Ng_PerfectMatch;
					checkResut.taboo_word = perfectMatch;
					break;
				}
			}
		}
		if (forwardMatchList != null)
		{
			foreach (string forwardMatch in forwardMatchList)
			{
				if (compareInfo.IndexOf(check_word, forwardMatch, opt) == 0)
				{
					checkResut.status = CheckStatus.Ng_ForwardMatch;
					checkResut.taboo_word = forwardMatch;
					break;
				}
			}
		}
		if (backwardMatchList != null)
		{
			foreach (string backwardMatch in backwardMatchList)
			{
				int num = compareInfo.LastIndexOf(check_word, backwardMatch, opt);
				if (num >= 0 && num + backwardMatch.Length == check_word.Length)
				{
					checkResut.status = CheckStatus.Ng_BackwardMatch;
					checkResut.taboo_word = backwardMatch;
					break;
				}
			}
		}
		if (checkResut.status != 0 && whiteList != null)
		{
			foreach (string white in whiteList)
			{
				if (compareInfo.IndexOf(check_word, white, opt) >= 0)
				{
					checkResut.status = CheckStatus.Ok;
					checkResut.taboo_word = "";
				}
			}
		}
		return checkResut;
	}
}
