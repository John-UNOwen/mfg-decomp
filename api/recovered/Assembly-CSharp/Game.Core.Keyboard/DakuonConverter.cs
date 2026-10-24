using System.Collections.Generic;
using System.Linq;

namespace Game.Core.Keyboard;

public static class DakuonConverter
{
	private static IDictionary<string, string> dakuonList = new Dictionary<string, string>
	{
		{ "か", "が" },
		{ "き", "ぎ" },
		{ "く", "ぐ" },
		{ "け", "げ" },
		{ "こ", "ご" },
		{ "さ", "ざ" },
		{ "し", "じ" },
		{ "す", "ず" },
		{ "せ", "ぜ" },
		{ "そ", "ぞ" },
		{ "た", "だ" },
		{ "ち", "ぢ" },
		{ "つ", "づ" },
		{ "て", "で" },
		{ "と", "ど" },
		{ "は", "ば" },
		{ "ひ", "び" },
		{ "ふ", "ぶ" },
		{ "へ", "べ" },
		{ "ほ", "ぼ" },
		{ "ウ", "ヴ" },
		{ "カ", "ガ" },
		{ "キ", "ギ" },
		{ "ク", "グ" },
		{ "ケ", "ゲ" },
		{ "コ", "ゴ" },
		{ "サ", "ザ" },
		{ "シ", "ジ" },
		{ "ス", "ズ" },
		{ "セ", "ゼ" },
		{ "ソ", "ゾ" },
		{ "タ", "ダ" },
		{ "チ", "ヂ" },
		{ "ツ", "ヅ" },
		{ "テ", "デ" },
		{ "ト", "ド" },
		{ "ハ", "バ" },
		{ "ヒ", "ビ" },
		{ "フ", "ブ" },
		{ "ヘ", "ベ" },
		{ "ホ", "ボ" }
	};

	private static IDictionary<string, string> halfDakuonList = new Dictionary<string, string>
	{
		{ "は", "ぱ" },
		{ "ひ", "ぴ" },
		{ "ふ", "ぷ" },
		{ "へ", "ぺ" },
		{ "ほ", "ぽ" },
		{ "ハ", "パ" },
		{ "ヒ", "ピ" },
		{ "フ", "プ" },
		{ "ヘ", "ペ" },
		{ "ホ", "ポ" }
	};

	public static bool IsDakuon(string text)
	{
		return dakuonList.Values.Any((string o) => o.Equals(text));
	}

	public static bool IsHalfDakuon(string text)
	{
		return halfDakuonList.Values.Any((string o) => o.Equals(text));
	}

	public static string AddDakuon(string text)
	{
		if (!dakuonList.ContainsKey(text))
		{
			return text;
		}
		return dakuonList[text];
	}

	public static string AddHalfDakuon(string text)
	{
		if (!halfDakuonList.ContainsKey(text))
		{
			return text;
		}
		return halfDakuonList[text];
	}

	public static string RemoveDakuon(string text)
	{
		IEnumerable<KeyValuePair<string, string>> source = dakuonList.Where((KeyValuePair<string, string> o) => o.Value.Equals(text));
		if (!source.Any())
		{
			return text;
		}
		return source.First().Key;
	}

	public static string RemoveHalfDakuon(string text)
	{
		IEnumerable<KeyValuePair<string, string>> source = halfDakuonList.Where((KeyValuePair<string, string> o) => o.Value.Equals(text));
		if (!source.Any())
		{
			return text;
		}
		return source.First().Key;
	}
}
