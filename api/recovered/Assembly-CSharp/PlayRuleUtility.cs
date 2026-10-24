using System.Collections.Generic;
using MFG.Types;
using UnityEngine;
using UnityEngine.UI;

public static class PlayRuleUtility
{
	public static Dictionary<(Han han, Yaku yaku), string> YakuNames = new Dictionary<(Han, Yaku), string>
	{
		{
			(Han.Han1, Yaku.Richi),
			"リーチ"
		},
		{
			(Han.Han1, Yaku.Ippatsu),
			"イッパツ"
		},
		{
			(Han.Han1, Yaku.Menzen),
			"メンゼンツモ"
		},
		{
			(Han.Han1, Yaku.Tanyao),
			"タンヤオ"
		},
		{
			(Han.Han1, Yaku.Pinfu),
			"ピンフ"
		},
		{
			(Han.Han1, Yaku.Ipeiko),
			"イーペーコー"
		},
		{
			(Han.Han1, Yaku.Jikaze),
			"ヤクハイ"
		},
		{
			(Han.Han1, Yaku.Rinsyan),
			"リンシャンカイホー"
		},
		{
			(Han.Han1, Yaku.Chankan),
			"チャンカン"
		},
		{
			(Han.Han1, Yaku.Haitei),
			"ハイテイ"
		},
		{
			(Han.Han1, Yaku.Hotei),
			"ホウテイ"
		},
		{
			(Han.Han2, Yaku.Sansyokudojun),
			"サンショクドウジュン"
		},
		{
			(Han.Han2, Yaku.Ikkitsukan),
			"イッキツーカン"
		},
		{
			(Han.Han2, Yaku.Chanta),
			"チャンタ"
		},
		{
			(Han.Han2, Yaku.Chitoitsu),
			"チートイツ"
		},
		{
			(Han.Han2, Yaku.Toitoiho),
			"トイトイホー"
		},
		{
			(Han.Han2, Yaku.Sananko),
			"サンアンコ"
		},
		{
			(Han.Han2, Yaku.Honroto),
			"ホンロートー"
		},
		{
			(Han.Han2, Yaku.Sansyokudoko),
			"サンショクドウコー"
		},
		{
			(Han.Han2, Yaku.Sankantsu),
			"サンカンツ"
		},
		{
			(Han.Han2, Yaku.Syosangen),
			"ショウサンゲン"
		},
		{
			(Han.Han2, Yaku.DoubleRichi),
			"ダブルリーチ"
		},
		{
			(Han.Han3, Yaku.Honiso),
			"ホンイーソー"
		},
		{
			(Han.Han3, Yaku.Junchan),
			"ジュンチャンタ"
		},
		{
			(Han.Han3, Yaku.Ryanpeko),
			"リャンペーコー"
		},
		{
			(Han.Haneman, Yaku.Chiniso),
			"チンイーソー"
		},
		{
			(Han.Yakuman, Yaku.KokushiTanki),
			"コクシムソウ"
		},
		{
			(Han.Yakuman, Yaku.Suanko),
			"スーアンコ"
		},
		{
			(Han.Yakuman, Yaku.Daisangen),
			"ダイサンゲン"
		},
		{
			(Han.Yakuman, Yaku.Tsuiso),
			"ツーイーソー"
		},
		{
			(Han.Yakuman, Yaku.Syosushi),
			"ショウスーシー"
		},
		{
			(Han.Yakuman, Yaku.Ryuiso),
			"リューイーソー(發なし可)"
		},
		{
			(Han.Yakuman, Yaku.Chinroto),
			"チンロートー"
		},
		{
			(Han.Yakuman, Yaku.Sukantsu),
			"スーカンツ"
		},
		{
			(Han.Yakuman, Yaku.TyurenTanki),
			"チュウレンポウトウ\n<align=\"right\">(マンズ以外もOK)</align>"
		},
		{
			(Han.Yakuman, Yaku.Tenho),
			"テンホー"
		},
		{
			(Han.Yakuman, Yaku.Chiho),
			"チーホー"
		},
		{
			(Han.Yakuman2, Yaku.Kokushi13),
			"コクシムソウ(13面待ち)"
		},
		{
			(Han.Yakuman2, Yaku.SuankoTanki),
			"スーアンコ(単騎待ち)"
		},
		{
			(Han.Yakuman2, Yaku.Daisushi),
			"ダイスーシー"
		},
		{
			(Han.Yakuman2, Yaku.Tyuren9),
			"チュウレンポウトウ(9面待ち)"
		}
	};

	public static void RefreshLayout(GameObject obj)
	{
		try
		{
			obj.transform.GetComponent<ContentSizeFitter>().SetLayoutVertical();
			obj.transform.GetComponent<ContentSizeFitter>().SetLayoutHorizontal();
		}
		catch
		{
		}
		try
		{
			obj.transform.GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical();
			obj.transform.GetComponent<VerticalLayoutGroup>().SetLayoutVertical();
		}
		catch
		{
		}
		try
		{
			obj.transform.GetComponent<HorizontalLayoutGroup>().CalculateLayoutInputHorizontal();
			obj.transform.GetComponent<HorizontalLayoutGroup>().SetLayoutHorizontal();
		}
		catch
		{
		}
		try
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(obj.transform.GetComponent<RectTransform>());
		}
		catch
		{
		}
	}
}
