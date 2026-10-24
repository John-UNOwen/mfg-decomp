using System.Collections.Generic;
using MFG.Scenes.Home;
using MFG.Types;
using MFG.Types.PlayRuleMaster;

namespace MFG.MasterGameData;

public class PlayRuleMaster : MasterGameDataBase
{
	public static List<ChangeRule> DefaultRules = new List<ChangeRule>
	{
		new ChangeRule
		{
			Unit = RuleUnit.TaikyokuUnit,
			ChangeText = "東風戦"
		},
		new ChangeRule
		{
			Unit = RuleUnit.Renchan,
			ChangeText = "親のアガり"
		},
		new ChangeRule
		{
			Unit = RuleUnit.FirstScore,
			ChangeText = "20,000点"
		},
		new ChangeRule
		{
			Unit = RuleUnit.Buttobi,
			ChangeText = "あり(持ち点が0点以下で終了)"
		},
		new ChangeRule
		{
			Unit = RuleUnit.RankScore,
			ChangeText = "1位 +5,000点\n4位 －5,000点"
		},
		new ChangeRule
		{
			Unit = RuleUnit.Tsumibo,
			ChangeText = "一本場につき300点"
		},
		new ChangeRule
		{
			Unit = RuleUnit.Meld,
			ChangeText = "ポン：あり/チー：あり/カン：あり"
		},
		new ChangeRule
		{
			Unit = RuleUnit.RedDora,
			ChangeText = "(各1枚)",
			ImageIndex = 0
		},
		new ChangeRule
		{
			Unit = RuleUnit.DoubRon,
			ChangeText = "あり"
		},
		new ChangeRule
		{
			Unit = RuleUnit.TriRon,
			ChangeText = "あり"
		},
		new ChangeRule
		{
			Unit = RuleUnit.Pao,
			ChangeText = "あり\n(ダイサンゲン、ダイスーシー、スーカンツ)"
		}
	};

	private static List<Item> _items = new List<Item>
	{
		new Item
		{
			RuleType = PlayRuleType.Yonma,
			TakuType = TakuType.Tonpu,
			Name = "4人打ちルール"
		},
		new Item
		{
			RuleType = PlayRuleType.Sanma,
			TakuType = TakuType.Sanma,
			Name = "3人打ちルール",
			UnableYaku = new List<Yaku> { Yaku.Sansyokudojun },
			ChangeRules = new List<ChangeRule>
			{
				new ChangeRule
				{
					Unit = RuleUnit.TaikyokuUnit,
					ChangeText = "<color=red>東家、南家、西家の東南戦</color>"
				},
				new ChangeRule
				{
					Unit = RuleUnit.FirstScore,
					ChangeText = "<color=red>25,000点</color>"
				},
				new ChangeRule
				{
					Unit = RuleUnit.RankScore,
					ChangeText = "<color=red>1位 +3,000点\n3位 －3,000点</color>"
				},
				new ChangeRule
				{
					Unit = RuleUnit.PaymentTsumo,
					ChangeText = "<color=red>親・子関係なく2人の折半</color>"
				},
				new ChangeRule
				{
					Unit = RuleUnit.EnablePai,
					ImageIndex = 1
				},
				new ChangeRule
				{
					Unit = RuleUnit.NextPai,
					ImageIndex = 0
				},
				new ChangeRule
				{
					Unit = RuleUnit.Tsumibo,
					ChangeText = "一本場につき<color=red>600</color>点"
				},
				new ChangeRule
				{
					Unit = RuleUnit.Meld,
					ChangeText = "ポン：あり/チー：<color=red>なし</color>/カン：あり"
				},
				new ChangeRule
				{
					Unit = RuleUnit.RedDora,
					ChangeText = "<color=red>(各2枚)</color>",
					ImageIndex = 1
				},
				new ChangeRule
				{
					Unit = RuleUnit.TriRon,
					ChangeText = "<color=red>出来ない</color>"
				}
			}
		},
		new Item
		{
			RuleType = PlayRuleType.Nima,
			TakuType = TakuType.Nima,
			Name = "2人打ちルール",
			UnableYaku = new List<Yaku>
			{
				Yaku.Sansyokudojun,
				Yaku.Sansyokudoko,
				Yaku.KokushiTanki,
				Yaku.Syosushi,
				Yaku.Chinroto,
				Yaku.Kokushi13,
				Yaku.Daisushi
			},
			ChangeRules = new List<ChangeRule>
			{
				new ChangeRule
				{
					Unit = RuleUnit.TaikyokuUnit,
					ChangeText = "<color=red>東家、南家の東南戦</color>"
				},
				new ChangeRule
				{
					Unit = RuleUnit.Renchan,
					ChangeText = "<color=red>連荘なし</color>"
				},
				new ChangeRule
				{
					Unit = RuleUnit.FirstScore,
					ChangeText = "<color=red>25,000点</color>"
				},
				new ChangeRule
				{
					Unit = RuleUnit.Buttobi,
					ChangeText = "<color=red>なし</color>"
				},
				new ChangeRule
				{
					Unit = RuleUnit.RankScore,
					ChangeText = "<color=red>なし</color>",
					Height = 34f
				},
				new ChangeRule
				{
					Unit = RuleUnit.PaymentTsumo,
					ChangeText = "<color=red>全額相手から</color>"
				},
				new ChangeRule
				{
					Unit = RuleUnit.EnablePai,
					ImageIndex = 2
				},
				new ChangeRule
				{
					Unit = RuleUnit.NextPai,
					ImageIndex = 1
				},
				new ChangeRule
				{
					Unit = RuleUnit.Tsumibo,
					ChangeText = "<color=red>連荘なし</color>"
				},
				new ChangeRule
				{
					Unit = RuleUnit.Meld,
					ChangeText = "ポン：あり/チー：<color=red>なし</color>/カン：あり"
				},
				new ChangeRule
				{
					Unit = RuleUnit.RedDora,
					ChangeText = "<color=red>(各2枚)</color>",
					ImageIndex = 1
				},
				new ChangeRule
				{
					Unit = RuleUnit.DoubRon,
					ChangeText = "<color=red>出来ない</color>"
				},
				new ChangeRule
				{
					Unit = RuleUnit.TriRon,
					ChangeText = "<color=red>出来ない</color>"
				},
				new ChangeRule
				{
					Unit = RuleUnit.Pao,
					ChangeText = "あり\n(<color=red>ダイサンゲン、スーカンツ</color>)"
				}
			}
		},
		new Item
		{
			RuleType = PlayRuleType.ButtobiGalaxy,
			TakuType = TakuType.Tonpu,
			Name = "「トイトイのぶっとびギャラクシー」4人打ちルール",
			ChangeRules = new List<ChangeRule>
			{
				new ChangeRule
				{
					Unit = RuleUnit.FirstScore,
					ChangeText = "<color=red>15,000点</color>"
				}
			}
		},
		new Item
		{
			RuleType = PlayRuleType.FireReach,
			TakuType = TakuType.Tonpu,
			Name = "「気炎万丈 炎のリーチ道場」4人打ちルール",
			ChangeRules = new List<ChangeRule>
			{
				new ChangeRule
				{
					Unit = RuleUnit.FireReach
				}
			}
		},
		new Item
		{
			RuleType = PlayRuleType.ButtobiGalaxySanma,
			TakuType = TakuType.Sanma,
			Name = "「トイトイのぶっとびギャラクシー」3人打ちルール",
			ChangeRules = new List<ChangeRule>
			{
				new ChangeRule
				{
					Unit = RuleUnit.FirstScore,
					ChangeText = "<color=red>15,000点</color>"
				}
			}
		},
		new Item
		{
			RuleType = PlayRuleType.AccelDora,
			TakuType = TakuType.Tonpu,
			Name = "「冥き終焉の暗黒ドラ卓」4人打ちルール",
			ChangeRules = new List<ChangeRule>
			{
				new ChangeRule
				{
					Unit = RuleUnit.AccelDora
				}
			}
		},
		new Item
		{
			RuleType = PlayRuleType.Mentanpin,
			TakuType = TakuType.Tonpu,
			Name = "「壱高トリオのメンタンピン教室」4人打ちルール",
			ChangeYaku = new List<(Yaku, Han)>
			{
				(Yaku.Richi, Han.Han2),
				(Yaku.Tanyao, Han.Han2),
				(Yaku.Pinfu, Han.Han2),
				(Yaku.DoubleRichi, Han.Han3)
			},
			ChangeRules = new List<ChangeRule>
			{
				new ChangeRule
				{
					Unit = RuleUnit.Mentanpin
				}
			}
		},
		new Item
		{
			RuleType = PlayRuleType.Aotenjo,
			TakuType = TakuType.Sanma,
			Name = "「究極青天井！炎のらぶはむ暗黒メンタンPCKギャラクシー霧雨突撃爆砕カタルシスですヨ！」3人打ちルール",
			ChangeYaku = new List<(Yaku, Han)>
			{
				(Yaku.Richi, Han.Han2),
				(Yaku.Tanyao, Han.Han2),
				(Yaku.Pinfu, Han.Han2),
				(Yaku.DoubleRichi, Han.Han3),
				(Yaku.Ryuiso, Han.Yakuman2)
			},
			ChangeRules = new List<ChangeRule>
			{
				new ChangeRule
				{
					Unit = RuleUnit.FirstScore,
					ChangeText = "<color=red>500,000,000点</color>"
				},
				new ChangeRule
				{
					Unit = RuleUnit.RankScore,
					ChangeText = "<color=red>なし</color>",
					Height = 34f
				},
				new ChangeRule
				{
					Unit = RuleUnit.RedDora,
					ImageIndex = 3
				},
				new ChangeRule
				{
					Unit = RuleUnit.Pao,
					ChangeText = "<color=red>なし</color>",
					Height = 34f
				}
			}
		},
		new Item
		{
			RuleType = PlayRuleType.Comeback,
			TakuType = TakuType.Tonpu,
			Name = "「六魂清浄 革命のカタルシス」4人打ちルール",
			ChangeRules = new List<ChangeRule>
			{
				new ChangeRule
				{
					Unit = RuleUnit.FirstScore,
					ChangeText = "<color=red>25,000点</color>"
				},
				new ChangeRule
				{
					Unit = RuleUnit.Catharsis
				}
			}
		},
		new Item
		{
			RuleType = PlayRuleType.AccelDoraSanma,
			TakuType = TakuType.Sanma,
			Name = "「冥き終焉の暗黒ドラ卓」3人打ちルール",
			ChangeRules = new List<ChangeRule>
			{
				new ChangeRule
				{
					Unit = RuleUnit.AccelDora
				}
			}
		},
		new Item
		{
			RuleType = PlayRuleType.Kirisame,
			TakuType = TakuType.Tonpu,
			Name = "「霧雨魔法店」4人打ちルール",
			ChangeRules = new List<ChangeRule>
			{
				new ChangeRule
				{
					Unit = RuleUnit.RedDora,
					ImageIndex = 2
				}
			}
		},
		new Item
		{
			RuleType = PlayRuleType.MentanpinSanma,
			TakuType = TakuType.Sanma,
			Name = "「壱高トリオのメンタンピン教室」3人打ちルール",
			EnableYaku = new List<Yaku> { Yaku.Sansyokudojun },
			UnableYaku = new List<Yaku>
			{
				Yaku.Jikaze,
				Yaku.Chanta,
				Yaku.Honroto,
				Yaku.Syosangen,
				Yaku.Honiso,
				Yaku.KokushiTanki,
				Yaku.Daisangen,
				Yaku.Tsuiso,
				Yaku.Syosushi,
				Yaku.Kokushi13,
				Yaku.Daisushi
			},
			ChangeYaku = new List<(Yaku, Han)>
			{
				(Yaku.Richi, Han.Han2),
				(Yaku.Tanyao, Han.Han2),
				(Yaku.Pinfu, Han.Han2),
				(Yaku.DoubleRichi, Han.Han3)
			},
			ChangeRules = new List<ChangeRule>
			{
				new ChangeRule
				{
					Unit = RuleUnit.EnablePai,
					ImageIndex = 3
				},
				new ChangeRule
				{
					Unit = RuleUnit.Pao,
					ChangeText = "あり\n<color=red>スーカンツ</color>"
				},
				new ChangeRule
				{
					Unit = RuleUnit.Mentanpin
				}
			}
		},
		new Item
		{
			RuleType = PlayRuleType.MeldBonusSanma,
			TakuType = TakuType.Tonpu,
			Name = "「オペレーションPCK」4人打ちルール",
			ChangeRules = new List<ChangeRule>
			{
				new ChangeRule
				{
					Unit = RuleUnit.PCK
				}
			}
		},
		new Item
		{
			RuleType = PlayRuleType.FireGalaxy,
			TakuType = TakuType.Tonpu,
			Name = "「炎のぶっとびギャラクシー」4人打ちルール",
			ChangeRules = new List<ChangeRule>
			{
				new ChangeRule
				{
					Unit = RuleUnit.FireReach
				}
			}
		},
		new Item
		{
			RuleType = PlayRuleType.FireGalaxySanma,
			TakuType = TakuType.Sanma,
			Name = "「炎のぶっとびギャラクシー」3人打ちルール",
			ChangeRules = new List<ChangeRule>
			{
				new ChangeRule
				{
					Unit = RuleUnit.FireReach
				}
			}
		},
		new Item
		{
			RuleType = PlayRuleType.ReversalSanma,
			TakuType = TakuType.Sanma,
			Name = "「恋もツモれば！らぶらぶ突撃麻雀」3人打ちルール",
			ChangeRules = new List<ChangeRule>
			{
				new ChangeRule
				{
					Unit = RuleUnit.FirstScore,
					ChangeText = "<color=red>35,000点</color>"
				},
				new ChangeRule
				{
					Unit = RuleUnit.Reversal
				}
			}
		},
		new Item
		{
			RuleType = PlayRuleType.MeldBonusSanma,
			TakuType = TakuType.Sanma,
			Name = "「オペレーションPCK」3人打ちルール",
			ChangeRules = new List<ChangeRule>
			{
				new ChangeRule
				{
					Unit = RuleUnit.PCK
				}
			}
		},
		new Item
		{
			RuleType = PlayRuleType.Bomb,
			TakuType = TakuType.Tonpu,
			Name = "「天才！爆砕！ボンバー卓」4人打ちルール",
			ChangeRules = new List<ChangeRule>
			{
				new ChangeRule
				{
					Unit = RuleUnit.FirstScore,
					ChangeText = "<color=red>25,000点</color>"
				},
				new ChangeRule
				{
					Unit = RuleUnit.Bomber
				}
			}
		},
		new Item
		{
			RuleType = PlayRuleType.BombSanma,
			TakuType = TakuType.Sanma,
			Name = "「天才！爆砕！ボンバー卓」3人打ちルール",
			ChangeRules = new List<ChangeRule>
			{
				new ChangeRule
				{
					Unit = RuleUnit.FirstScore,
					ChangeText = "<color=red>30,000点</color>"
				},
				new ChangeRule
				{
					Unit = RuleUnit.Bomber
				}
			}
		},
		new Item
		{
			RuleType = PlayRuleType.AllGreen,
			TakuType = TakuType.Tonpu,
			Name = "「GET READYですヨ！全緑卓」4人打ちルール",
			ChangeYaku = new List<(Yaku, Han)> { (Yaku.Ryuiso, Han.Yakuman2) },
			ChangeRules = new List<ChangeRule>
			{
				new ChangeRule
				{
					Unit = RuleUnit.RedDora,
					ChangeText = "<color=red>なし</color>",
					ImageIndex = -1,
					Height = 34f
				},
				new ChangeRule
				{
					Unit = RuleUnit.AllGreen
				}
			}
		},
		new Item
		{
			RuleType = PlayRuleType.AllGreenSanma,
			TakuType = TakuType.Sanma,
			Name = "「GET READYですヨ！全緑卓」3人打ちルール",
			ChangeYaku = new List<(Yaku, Han)> { (Yaku.Ryuiso, Han.Yakuman2) },
			ChangeRules = new List<ChangeRule>
			{
				new ChangeRule
				{
					Unit = RuleUnit.RedDora,
					ChangeText = "<color=red>なし</color>",
					ImageIndex = -1,
					Height = 34f
				},
				new ChangeRule
				{
					Unit = RuleUnit.AllGreen
				}
			}
		},
		new Item
		{
			RuleType = PlayRuleType.ComebackSanma,
			TakuType = TakuType.Sanma,
			Name = "「六魂清浄 革命のカタルシス」3人打ちルール",
			ChangeRules = new List<ChangeRule>
			{
				new ChangeRule
				{
					Unit = RuleUnit.FirstScore,
					ChangeText = "<color=red>30,000点</color>"
				},
				new ChangeRule
				{
					Unit = RuleUnit.Catharsis
				}
			}
		},
		new Item
		{
			RuleType = PlayRuleType.Atomic,
			TakuType = TakuType.Tonpu,
			Name = "「アトミックリーチ」4人打ちルール",
			ChangeRules = new List<ChangeRule>
			{
				new ChangeRule
				{
					Unit = RuleUnit.Atomic
				}
			}
		},
		new Item
		{
			RuleType = PlayRuleType.AtomicSanma,
			TakuType = TakuType.Sanma,
			Name = "「アトミックリーチ」3人打ちルール",
			ChangeRules = new List<ChangeRule>
			{
				new ChangeRule
				{
					Unit = RuleUnit.Atomic
				}
			}
		},
		new Item
		{
			RuleType = PlayRuleType.PCKGalaxy,
			TakuType = TakuType.Tonpu,
			Name = "「オペレーションぶっとびギャラクシー」4人打ちルール",
			ChangeRules = new List<ChangeRule>
			{
				new ChangeRule
				{
					Unit = RuleUnit.PCK
				}
			}
		},
		new Item
		{
			RuleType = PlayRuleType.PCKGalaxySanma,
			TakuType = TakuType.Sanma,
			Name = "「オペレーションぶっとびギャラクシー」3人打ちルール",
			ChangeRules = new List<ChangeRule>
			{
				new ChangeRule
				{
					Unit = RuleUnit.PCK
				}
			}
		}
	};

	public static List<Item> Items => _items;

	public static Item GetItem(PlayRuleType ruleType)
	{
		return _items.Find((Item item) => item.RuleType == ruleType);
	}
}
