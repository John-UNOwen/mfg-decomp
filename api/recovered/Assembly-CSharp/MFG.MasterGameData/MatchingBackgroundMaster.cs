using System;
using System.Collections.Generic;
using System.Linq;
using MFG.Types;
using MFG.Types.Customize.CutinItem;
using MFG.Types.Customize.MatchingBackground;

namespace MFG.MasterGameData;

public class MatchingBackgroundMaster : MasterGameDataBase
{
	private static List<MFG.Types.Customize.MatchingBackground.Item> _items = new List<MFG.Types.Customize.MatchingBackground.Item>
	{
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_01_A01,
			CharaType = CharaType.Chara01,
			Name = "アガリ「スタンダード」",
			UnlockType = UnlockType.Default,
			IsDefaultEquip = true
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_01_A02,
			CharaType = CharaType.Chara01,
			Name = "リーチ「プチキャラ」",
			UnlockType = UnlockType.Default
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_01_A03,
			CharaType = CharaType.Chara01,
			Name = "アガリ「スマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item01AgariSR01
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_01_A04,
			CharaType = CharaType.Chara01,
			Name = "アガリ「バニー」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item01AgariSR02
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_01_A05,
			CharaType = CharaType.Chara01,
			Name = "アガリ「コミカルスマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item01AgariSR03
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_01_A06,
			CharaType = CharaType.Chara01,
			Name = "アガリ「水着」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item01AgariSR04
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_01_A07,
			CharaType = CharaType.Chara01,
			Name = "アガリ「水着(NDKカラー)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item01AgariSR05
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_01_A08,
			CharaType = CharaType.Chara01,
			Name = "アガリ「レースクイーン」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item01AgariSR06
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_01_A09,
			CharaType = CharaType.Chara01,
			Name = "アガリ「霊夢の巫女服」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item01AgariSR07
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_01_A10,
			CharaType = CharaType.Chara01,
			Name = "アガリ「ネリマギャル」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item01AgariSR08
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_01_A11,
			CharaType = CharaType.Chara01,
			Name = "アガリ「ネリマギャル(キャラメル)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item01AgariSR09
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_01_A12,
			CharaType = CharaType.Chara01,
			Name = "アガリ「レースクイーン(NDKカラー)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item01AgariSR10
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_01_A13,
			CharaType = CharaType.Chara01,
			Name = "アガリ「雀アイドル」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item01AgariSR11
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_01_A14,
			CharaType = CharaType.Chara01,
			Name = "アガリ「マーチングバンド」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item01AgariSR12
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_01_A15,
			CharaType = CharaType.Chara01,
			Name = "アガリ「バレンタイン(ミルク)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item01AgariSR13
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_01_A16,
			CharaType = CharaType.Chara01,
			Name = "アガリ「バレンタイン(ビター)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item01AgariSR14
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_01_A17,
			CharaType = CharaType.Chara01,
			Name = "アガリ「ネリマギャル(ショコラ)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item01AgariSR15
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_01_A99,
			CharaType = CharaType.Chara01,
			Name = "MA演出",
			UnlockType = UnlockType.Ma,
			Category = MFG.Types.Customize.MatchingBackground.Category.Ma
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_02_A01,
			CharaType = CharaType.Chara02,
			Name = "アガリ「スタンダード」",
			UnlockType = UnlockType.Default,
			IsDefaultEquip = true
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_02_A02,
			CharaType = CharaType.Chara02,
			Name = "リーチ「プチキャラ」",
			UnlockType = UnlockType.Default
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_02_A03,
			CharaType = CharaType.Chara02,
			Name = "アガリ「スマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item02AgariSR01
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_02_A04,
			CharaType = CharaType.Chara02,
			Name = "アガリ「バニー」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item02AgariSR02
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_02_A05,
			CharaType = CharaType.Chara02,
			Name = "アガリ「コミカルスマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item02AgariSR03
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_02_A06,
			CharaType = CharaType.Chara02,
			Name = "アガリ「水着」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item02AgariSR04
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_02_A07,
			CharaType = CharaType.Chara02,
			Name = "アガリ「水着(NDKカラー)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item02AgariSR05
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_02_A08,
			CharaType = CharaType.Chara02,
			Name = "アガリ「雀喫茶メイド」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item02AgariSR06
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_02_A09,
			CharaType = CharaType.Chara02,
			Name = "アガリ「青春チアリーダー」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item02AgariSR07
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_02_A10,
			CharaType = CharaType.Chara02,
			Name = "アガリ「水着(小麦色ver.)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item02AgariSR08
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_02_A11,
			CharaType = CharaType.Chara02,
			Name = "アガリ「プライベートデート水着」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item02AgariSR09
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_02_A12,
			CharaType = CharaType.Chara02,
			Name = "アガリ「プライベートデート水着(小麦色ver.)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item02AgariSR10
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_02_A13,
			CharaType = CharaType.Chara02,
			Name = "アガリ「桃色チアリーダー」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item02AgariSR11
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_02_A14,
			CharaType = CharaType.Chara02,
			Name = "アガリ「雀アイドル」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item02AgariSR12
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_02_A15,
			CharaType = CharaType.Chara02,
			Name = "アガリ「壱高スクール水着」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item02AgariSR13
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_02_A16,
			CharaType = CharaType.Chara02,
			Name = "アガリ「壱高スクール水着(えんじ)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item02AgariSR14
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_02_A17,
			CharaType = CharaType.Chara02,
			Name = "アガリ「マーチングバンド」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item02AgariSR15
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_02_A18,
			CharaType = CharaType.Chara02,
			Name = "アガリ「ネリマギャル」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item02AgariSR16
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_02_A19,
			CharaType = CharaType.Chara02,
			Name = "アガリ「ネリマギャル(カヌレ)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item02AgariSR17
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_02_A99,
			CharaType = CharaType.Chara02,
			Name = "MA演出",
			UnlockType = UnlockType.Ma,
			Category = MFG.Types.Customize.MatchingBackground.Category.Ma
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_03_A01,
			CharaType = CharaType.Chara03,
			Name = "アガリ「スタンダード」",
			UnlockType = UnlockType.Default,
			IsDefaultEquip = true
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_03_A02,
			CharaType = CharaType.Chara03,
			Name = "リーチ「プチキャラ」",
			UnlockType = UnlockType.Default
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_03_A03,
			CharaType = CharaType.Chara03,
			Name = "アガリ「スマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item03AgariSR01
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_03_A04,
			CharaType = CharaType.Chara03,
			Name = "アガリ「バニー」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item03AgariSR02
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_03_A05,
			CharaType = CharaType.Chara03,
			Name = "アガリ「コミカルスマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item03AgariSR03
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_03_A06,
			CharaType = CharaType.Chara03,
			Name = "アガリ「水着」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item03AgariSR04
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_03_A07,
			CharaType = CharaType.Chara03,
			Name = "アガリ「水着(AFDカラー)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item03AgariSR05
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_03_A08,
			CharaType = CharaType.Chara03,
			Name = "アガリ「お正月」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item03AgariSR06
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_03_A09,
			CharaType = CharaType.Chara03,
			Name = "アガリ「パロディウス サンバカーニバルコス」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item03AgariSR07
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_03_A10,
			CharaType = CharaType.Chara03,
			Name = "アガリ「銃王無尽グレイスコス」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item03AgariSR08
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_03_A11,
			CharaType = CharaType.Chara03,
			Name = "アガリ「慈愛シスター」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item03AgariSR09
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_03_A12,
			CharaType = CharaType.Chara03,
			Name = "アガリ「慈愛シスター(パージver.)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item03AgariSR10
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_03_A13,
			CharaType = CharaType.Chara03,
			Name = "アガリ「雀喫茶メイド(ナナツノグリーン)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item03AgariSR11
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_03_A14,
			CharaType = CharaType.Chara03,
			Name = "アガリ「雀喫茶メイド(リッチブラック)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item03AgariSR12
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_03_A15,
			CharaType = CharaType.Chara03,
			Name = "アガリ「弐女スクール水着」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item03AgariSR13
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_03_A16,
			CharaType = CharaType.Chara03,
			Name = "アガリ「弐女スクール水着(金)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item03AgariSR14
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_03_A99,
			CharaType = CharaType.Chara03,
			Name = "MA演出",
			UnlockType = UnlockType.Ma,
			Category = MFG.Types.Customize.MatchingBackground.Category.Ma
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_04_A01,
			CharaType = CharaType.Chara04,
			Name = "アガリ「スタンダード」",
			UnlockType = UnlockType.Default,
			IsDefaultEquip = true
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_04_A02,
			CharaType = CharaType.Chara04,
			Name = "リーチ「プチキャラ」",
			UnlockType = UnlockType.Default
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_04_A03,
			CharaType = CharaType.Chara04,
			Name = "アガリ「スマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item04AgariSR01
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_04_A04,
			CharaType = CharaType.Chara04,
			Name = "アガリ「バニー」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item04AgariSR02
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_04_A05,
			CharaType = CharaType.Chara04,
			Name = "アガリ「コミカルスマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item04AgariSR03
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_04_A06,
			CharaType = CharaType.Chara04,
			Name = "アガリ「水着」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item04AgariSR04
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_04_A07,
			CharaType = CharaType.Chara04,
			Name = "アガリ「水着(NBDカラー)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item04AgariSR05
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_04_A08,
			CharaType = CharaType.Chara04,
			Name = "アガリ「クリスマス」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item04AgariSR06
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_04_A09,
			CharaType = CharaType.Chara04,
			Name = "アガリ「サキュバスチアコス」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item04AgariSR07
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_04_A10,
			CharaType = CharaType.Chara04,
			Name = "アガリ「銃王無尽グレイスコス」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item04AgariSR08
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_04_A11,
			CharaType = CharaType.Chara04,
			Name = "アガリ「フランのドレス」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item04AgariSR09
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_04_A12,
			CharaType = CharaType.Chara04,
			Name = "アガリ「クリスマス(ブラックサンタ)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item04AgariSR10
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_04_A13,
			CharaType = CharaType.Chara04,
			Name = "アガリ「ホーリーエンジェル」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item04AgariSR11
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_04_A14,
			CharaType = CharaType.Chara04,
			Name = "アガリ「ホーリーエンジェル(仮面ver.)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item04AgariSR12
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_04_A15,
			CharaType = CharaType.Chara04,
			Name = "アガリ「ぽんのみち(チョンボ)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item04AgariA03
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_04_A16,
			CharaType = CharaType.Chara04,
			Name = "アガリ「雀アイドル」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item04AgariSR13
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_04_A17,
			CharaType = CharaType.Chara04,
			Name = "アガリ「テニスウェア」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item04AgariSR14
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_04_A18,
			CharaType = CharaType.Chara04,
			Name = "アガリ「テニスウェア(帝役)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item04AgariSR15
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_04_A99,
			CharaType = CharaType.Chara04,
			Name = "MA演出",
			UnlockType = UnlockType.Ma,
			Category = MFG.Types.Customize.MatchingBackground.Category.Ma
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_05_A01,
			CharaType = CharaType.Chara05,
			Name = "アガリ「スタンダード」",
			UnlockType = UnlockType.Default,
			IsDefaultEquip = true
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_05_A02,
			CharaType = CharaType.Chara05,
			Name = "リーチ「プチキャラ」",
			UnlockType = UnlockType.Default
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_05_A03,
			CharaType = CharaType.Chara05,
			Name = "アガリ「スマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item05AgariSR01
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_05_A04,
			CharaType = CharaType.Chara05,
			Name = "アガリ「バニー」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item05AgariSR02
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_05_A05,
			CharaType = CharaType.Chara05,
			Name = "アガリ「コミカルスマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item05AgariSR03
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_05_A06,
			CharaType = CharaType.Chara05,
			Name = "アガリ「水着」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item05AgariSR04
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_05_A07,
			CharaType = CharaType.Chara05,
			Name = "アガリ「水着(クールver.)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item05AgariSR05
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_05_A08,
			CharaType = CharaType.Chara05,
			Name = "アガリ「水着(NDKカラー)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item05AgariSR06
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_05_A09,
			CharaType = CharaType.Chara05,
			Name = "アガリ「水着(NDKクールver.)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item05AgariSR07
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_05_A10,
			CharaType = CharaType.Chara05,
			Name = "アガリ「お正月」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item05AgariSR08
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_05_A11,
			CharaType = CharaType.Chara05,
			Name = "アガリ「壱役高等学校夏制服」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item05AgariSR09
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_05_A12,
			CharaType = CharaType.Chara05,
			Name = "アガリ「壱役高等学校夏制服(！？ver.)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item05AgariSR10
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_05_A13,
			CharaType = CharaType.Chara05,
			Name = "アガリ「水着(小麦色ver.)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item05AgariSR11
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_05_A14,
			CharaType = CharaType.Chara05,
			Name = "アガリ「アラビニャンダンサー」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item05AgariSR12
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_05_A15,
			CharaType = CharaType.Chara05,
			Name = "アガリ「アラビニャンダンサー(黒)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item05AgariSR13
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_05_A16,
			CharaType = CharaType.Chara05,
			Name = "アガリ「雀アイドル」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item05AgariSR14
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_05_A17,
			CharaType = CharaType.Chara05,
			Name = "アガリ「ねこ浴衣(藍)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item05AgariSR15
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_05_A18,
			CharaType = CharaType.Chara05,
			Name = "アガリ「ねこ浴衣(白)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item05AgariSR16
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_05_A19,
			CharaType = CharaType.Chara05,
			Name = "アガリ「マーチングバンド」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item05AgariSR17
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_05_A99,
			CharaType = CharaType.Chara05,
			Name = "MA演出",
			UnlockType = UnlockType.Ma,
			Category = MFG.Types.Customize.MatchingBackground.Category.Ma
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_06_A01,
			CharaType = CharaType.Chara06,
			Name = "アガリ「スタンダード」",
			UnlockType = UnlockType.Default,
			IsDefaultEquip = true
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_06_A02,
			CharaType = CharaType.Chara06,
			Name = "リーチ「プチキャラ」",
			UnlockType = UnlockType.Default
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_06_A03,
			CharaType = CharaType.Chara06,
			Name = "アガリ「スマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item06AgariSR01
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_06_A04,
			CharaType = CharaType.Chara06,
			Name = "アガリ「バニー」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item06AgariSR02
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_06_A05,
			CharaType = CharaType.Chara06,
			Name = "アガリ「コミカルスマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item06AgariSR03
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_06_A06,
			CharaType = CharaType.Chara06,
			Name = "アガリ「水着」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item06AgariSR04
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_06_A07,
			CharaType = CharaType.Chara06,
			Name = "アガリ「水着(AFDカラー)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item06AgariSR05
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_06_A08,
			CharaType = CharaType.Chara06,
			Name = "アガリ「シンデレラ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item06AgariSR06
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_06_A09,
			CharaType = CharaType.Chara06,
			Name = "アガリ「暗黒部屋着」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item06AgariSR07
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_06_A10,
			CharaType = CharaType.Chara06,
			Name = "アガリ「暗黒部屋着(パージver.)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item06AgariSR08
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_06_A11,
			CharaType = CharaType.Chara06,
			Name = "アガリ「水着(小麦色ver.)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item06AgariSR09
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_06_A12,
			CharaType = CharaType.Chara06,
			Name = "アガリ「ダークネスローブ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item06AgariSR10
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_06_A13,
			CharaType = CharaType.Chara06,
			Name = "アガリ「ダークネスローブ(仮面ver.)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item06AgariSR11
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_06_A14,
			CharaType = CharaType.Chara06,
			Name = "アガリ「雀バーガースタッフ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item06AgariSR12
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_06_A15,
			CharaType = CharaType.Chara06,
			Name = "アガリ「雀バーガースタッフ(リーダー)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item06AgariSR13
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_06_A16,
			CharaType = CharaType.Chara06,
			Name = "アガリ「雀アイドル」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item06AgariSR14
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_06_A99,
			CharaType = CharaType.Chara06,
			Name = "MA演出",
			UnlockType = UnlockType.Ma,
			Category = MFG.Types.Customize.MatchingBackground.Category.Ma
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_07_A01,
			CharaType = CharaType.Chara07,
			Name = "アガリ「スタンダード」",
			UnlockType = UnlockType.Default,
			IsDefaultEquip = true
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_07_A02,
			CharaType = CharaType.Chara07,
			Name = "リーチ「プチキャラ」",
			UnlockType = UnlockType.Default
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_07_A03,
			CharaType = CharaType.Chara07,
			Name = "アガリ「スマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item07AgariSR01
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_07_A04,
			CharaType = CharaType.Chara07,
			Name = "アガリ「バニー」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item07AgariSR02
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_07_A05,
			CharaType = CharaType.Chara07,
			Name = "アガリ「コミカルスマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item07AgariSR03
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_07_A06,
			CharaType = CharaType.Chara07,
			Name = "アガリ「水着」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item07AgariSR04
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_07_A07,
			CharaType = CharaType.Chara07,
			Name = "アガリ「水着(パージver.)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item07AgariSR05
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_07_A08,
			CharaType = CharaType.Chara07,
			Name = "アガリ「水着(AFDカラー)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item07AgariSR06
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_07_A09,
			CharaType = CharaType.Chara07,
			Name = "アガリ「水着(AFDパージver.)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item07AgariSR07
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_07_A10,
			CharaType = CharaType.Chara07,
			Name = "アガリ「バレンタイン」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item07AgariSR08
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_07_A11,
			CharaType = CharaType.Chara07,
			Name = "アガリ「ゲーマー浴衣」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item07AgariSR09
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_07_A12,
			CharaType = CharaType.Chara07,
			Name = "アガリ「水着(小麦色ver.)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item07AgariSR10
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_07_A13,
			CharaType = CharaType.Chara07,
			Name = "アガリ「バレンタイン(対ありver.)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item07AgariSR11
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_07_A14,
			CharaType = CharaType.Chara07,
			Name = "アガリ「ロリータドレス」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item07AgariSR12
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_07_A15,
			CharaType = CharaType.Chara07,
			Name = "アガリ「ロリータドレス(チョコミントver.)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item07AgariSR13
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_07_A16,
			CharaType = CharaType.Chara07,
			Name = "アガリ「弐女スクール水着」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item07AgariSR14
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_07_A17,
			CharaType = CharaType.Chara07,
			Name = "アガリ「弐女スクール水着(みどり)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item07AgariSR15
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_07_A18,
			CharaType = CharaType.Chara07,
			Name = "アガリ「雀喫茶メイド」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item07AgariSR16
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_07_A19,
			CharaType = CharaType.Chara07,
			Name = "アガリ「雀喫茶メイド(スペースノワール)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item07AgariSR17
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_07_A20,
			CharaType = CharaType.Chara07,
			Name = "アガリ「雀アイドル」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item07AgariSR18
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_07_A99,
			CharaType = CharaType.Chara07,
			Name = "MA演出",
			UnlockType = UnlockType.Ma,
			Category = MFG.Types.Customize.MatchingBackground.Category.Ma
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_08_A01,
			CharaType = CharaType.Chara08,
			Name = "アガリ「スタンダード」",
			UnlockType = UnlockType.Default,
			IsDefaultEquip = true
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_08_A02,
			CharaType = CharaType.Chara08,
			Name = "リーチ「プチキャラ」",
			UnlockType = UnlockType.Default
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_08_A03,
			CharaType = CharaType.Chara08,
			Name = "アガリ「スマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item08AgariSR01
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_08_A04,
			CharaType = CharaType.Chara08,
			Name = "アガリ「バニー」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item08AgariSR02
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_08_A05,
			CharaType = CharaType.Chara08,
			Name = "アガリ「コミカルスマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item08AgariSR03
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_08_A06,
			CharaType = CharaType.Chara08,
			Name = "アガリ「水着」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item08AgariSR04
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_08_A07,
			CharaType = CharaType.Chara08,
			Name = "アガリ「水着(NBDカラー)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item08AgariSR05
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_08_A08,
			CharaType = CharaType.Chara08,
			Name = "アガリ「ハロウィン」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item08AgariSR06
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_08_A09,
			CharaType = CharaType.Chara08,
			Name = "アガリ「役満高等学校体操服」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item08AgariSR07
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_08_A10,
			CharaType = CharaType.Chara08,
			Name = "アガリ「くみゃはっぴ(帝役)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item08AgariSR08
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_08_A11,
			CharaType = CharaType.Chara08,
			Name = "アガリ「くみゃはっぴ(鍛錬)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item08AgariSR09
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_08_A12,
			CharaType = CharaType.Chara08,
			Name = "アガリ「アニマルぱじゃま(くま)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item08AgariSR10
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_08_A13,
			CharaType = CharaType.Chara08,
			Name = "アガリ「アニマルぱじゃま(くみゃ)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item08AgariSR11
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_08_A14,
			CharaType = CharaType.Chara08,
			Name = "アガリ「雀アイドル」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item08AgariSR12
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_08_A15,
			CharaType = CharaType.Chara08,
			Name = "アガリ「袴メイド」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item08AgariSR13
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_08_A16,
			CharaType = CharaType.Chara08,
			Name = "アガリ「袴メイド(抹茶)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item08AgariSR14
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_08_A99,
			CharaType = CharaType.Chara08,
			Name = "MA演出",
			UnlockType = UnlockType.Ma,
			Category = MFG.Types.Customize.MatchingBackground.Category.Ma
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_09_A01,
			CharaType = CharaType.Chara09,
			Name = "アガリ「スタンダード」",
			UnlockType = UnlockType.Default,
			IsDefaultEquip = true
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_09_A02,
			CharaType = CharaType.Chara09,
			Name = "リーチ「プチキャラ」",
			UnlockType = UnlockType.Default
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_09_A03,
			CharaType = CharaType.Chara09,
			Name = "アガリ「スマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item09AgariSR01
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_09_A04,
			CharaType = CharaType.Chara09,
			Name = "アガリ「バニー」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item09AgariSR02
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_09_A05,
			CharaType = CharaType.Chara09,
			Name = "アガリ「コミカルスマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item09AgariSR03
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_09_A06,
			CharaType = CharaType.Chara09,
			Name = "アガリ「水着」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item09AgariSR04
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_09_A07,
			CharaType = CharaType.Chara09,
			Name = "アガリ「水着(白衣ver.)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item09AgariSR05
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_09_A08,
			CharaType = CharaType.Chara09,
			Name = "アガリ「水着(マッド)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item09AgariSR06
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_09_A09,
			CharaType = CharaType.Chara09,
			Name = "アガリ「水着(マッド白衣ver.)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item09AgariSR07
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_09_A10,
			CharaType = CharaType.Chara09,
			Name = "アガリ「アリスメイド」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item09AgariSR08
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_09_A11,
			CharaType = CharaType.Chara09,
			Name = "アガリ「アリスメイド(ロイヤルカラー)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item09AgariSR09
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_09_A12,
			CharaType = CharaType.Chara09,
			Name = "アガリ「水着(小麦色ver.)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item09AgariSR10
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_09_A13,
			CharaType = CharaType.Chara09,
			Name = "アガリ「プリティーボンバーズ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item09AgariSR11
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_09_A14,
			CharaType = CharaType.Chara09,
			Name = "アガリ「プリティーボンバーズ(キラービーカラー)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item09AgariSR12
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_09_A15,
			CharaType = CharaType.Chara09,
			Name = "アガリ「ベリーダンス(黄色)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item09AgariSR13
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_09_A16,
			CharaType = CharaType.Chara09,
			Name = "アガリ「ベリーダンス(黄色ベールver.)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item09AgariSR14
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_09_A17,
			CharaType = CharaType.Chara09,
			Name = "アガリ「ベリーダンス(水色)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item09AgariSR15
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_09_A18,
			CharaType = CharaType.Chara09,
			Name = "アガリ「ベリーダンス(水色ベールver.)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item09AgariSR16
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_10_A01,
			CharaType = CharaType.Chara10,
			Name = "アガリ「スタンダード」",
			UnlockType = UnlockType.Default,
			IsDefaultEquip = true
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_10_A02,
			CharaType = CharaType.Chara10,
			Name = "リーチ「プチキャラ」",
			UnlockType = UnlockType.Default
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_10_A03,
			CharaType = CharaType.Chara10,
			Name = "アガリ「スマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item10AgariSR01
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_10_A04,
			CharaType = CharaType.Chara10,
			Name = "アガリ「きらめき高校体操服」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item10AgariSR02
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_10_A05,
			CharaType = CharaType.Chara10,
			Name = "アガリ「コミカルスマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item10AgariSR03
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_11_A01,
			CharaType = CharaType.Chara11,
			Name = "アガリ「スタンダード」",
			UnlockType = UnlockType.Default,
			IsDefaultEquip = true
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_11_A02,
			CharaType = CharaType.Chara11,
			Name = "リーチ「プチキャラ」",
			UnlockType = UnlockType.Default
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_11_A03,
			CharaType = CharaType.Chara11,
			Name = "アガリ「スマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item11AgariSR01
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_11_A04,
			CharaType = CharaType.Chara11,
			Name = "アガリ「ホワイトバニー」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item11AgariSR02
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_11_A05,
			CharaType = CharaType.Chara11,
			Name = "アガリ「ブラックバニー」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item11AgariSR03
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_11_A06,
			CharaType = CharaType.Chara11,
			Name = "アガリ「コミカルスマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item11AgariSR04
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_11_A07,
			CharaType = CharaType.Chara11,
			Name = "アガリ「水着」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item11AgariSR05
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_11_A08,
			CharaType = CharaType.Chara11,
			Name = "アガリ「普通のナース」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item11AgariSR06
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_11_A09,
			CharaType = CharaType.Chara11,
			Name = "アガリ「普通の黒ナース」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item11AgariSR07
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_11_A10,
			CharaType = CharaType.Chara11,
			Name = "アガリ「スパイドレス」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item11AgariSR08
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_11_A11,
			CharaType = CharaType.Chara11,
			Name = "アガリ「スパイドレス(スノウ)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item11AgariSR09
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_11_A12,
			CharaType = CharaType.Chara11,
			Name = "アガリ「スパイスーツ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item11AgariSR10
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_11_A13,
			CharaType = CharaType.Chara11,
			Name = "アガリ「スパイスーツ(ピンク)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item11AgariSR11
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_11_A99,
			CharaType = CharaType.Chara11,
			Name = "MA演出",
			UnlockType = UnlockType.Ma,
			Category = MFG.Types.Customize.MatchingBackground.Category.Ma
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_12_A01,
			CharaType = CharaType.Chara12,
			Name = "アガリ「スタンダード」",
			UnlockType = UnlockType.Default,
			IsDefaultEquip = true
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_12_A02,
			CharaType = CharaType.Chara12,
			Name = "リーチ「プチキャラ」",
			UnlockType = UnlockType.Default
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_12_A03,
			CharaType = CharaType.Chara12,
			Name = "アガリ「スマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item12AgariSR02
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_12_A04,
			CharaType = CharaType.Chara12,
			Name = "アガリ「ホワイトバニー」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item12AgariSR03
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_12_A05,
			CharaType = CharaType.Chara12,
			Name = "アガリ「ブラックバニー」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item12AgariSR04
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_12_A06,
			CharaType = CharaType.Chara12,
			Name = "アガリ「コミカルスマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item12AgariSR05
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_12_A07,
			CharaType = CharaType.Chara12,
			Name = "アガリ「水着」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item12AgariSR06
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_12_A08,
			CharaType = CharaType.Chara12,
			Name = "アガリ「八雲紫の道士服」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item12AgariSR07
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_12_A09,
			CharaType = CharaType.Chara12,
			Name = "アガリ「セイジョウギャル」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item12AgariSR08
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_12_A10,
			CharaType = CharaType.Chara12,
			Name = "アガリ「セイジョウギャル(ローズ)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item12AgariSR09
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_12_A11,
			CharaType = CharaType.Chara12,
			Name = "アガリ「初詣(白百合)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item12AgariSR10
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_12_A12,
			CharaType = CharaType.Chara12,
			Name = "アガリ「初詣(黒百合)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item12AgariSR11
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_12_A13,
			CharaType = CharaType.Chara12,
			Name = "アガリ「セイジョウギャル(マーガレット)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item12AgariSR12
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_12_A99,
			CharaType = CharaType.Chara12,
			Name = "MA演出",
			UnlockType = UnlockType.Ma,
			Category = MFG.Types.Customize.MatchingBackground.Category.Ma
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_13_A01,
			CharaType = CharaType.Chara13,
			Name = "アガリ「スタンダード」",
			UnlockType = UnlockType.Default,
			IsDefaultEquip = true
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_13_A02,
			CharaType = CharaType.Chara13,
			Name = "リーチ「プチキャラ」",
			UnlockType = UnlockType.Default
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_13_A03,
			CharaType = CharaType.Chara13,
			Name = "アガリ「スマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item13AgariSR02
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_13_A04,
			CharaType = CharaType.Chara13,
			Name = "アガリ「バニー」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item13AgariSR03
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_13_A05,
			CharaType = CharaType.Chara13,
			Name = "アガリ「コミカルスマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item13AgariSR04
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_13_A06,
			CharaType = CharaType.Chara13,
			Name = "アガリ「デビュー」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item13AgariSR05
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_13_A07,
			CharaType = CharaType.Chara13,
			Name = "アガリ「水着」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item13AgariSR06
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_13_A08,
			CharaType = CharaType.Chara13,
			Name = "アガリ「キョンシー（白蛇）」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item13AgariSR07
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_13_A09,
			CharaType = CharaType.Chara13,
			Name = "アガリ「キョンシー（碧蛇）」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item13AgariSR08
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_13_A10,
			CharaType = CharaType.Chara13,
			Name = "アガリ「帝役スクール水着」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item13AgariSR09
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_13_A11,
			CharaType = CharaType.Chara13,
			Name = "アガリ「帝役スクール水着（くろ）」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item13AgariSR10
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_13_A12,
			CharaType = CharaType.Chara13,
			Name = "アガリ「雀喫茶メイド」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item13AgariSR11
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_13_A13,
			CharaType = CharaType.Chara13,
			Name = "アガリ「雀喫茶メイド(ハニービー)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item13AgariSR12
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_14_A01,
			CharaType = CharaType.Chara14,
			Name = "アガリ「スタンダード」",
			UnlockType = UnlockType.Default,
			IsDefaultEquip = true
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_14_A02,
			CharaType = CharaType.Chara14,
			Name = "リーチ「プチキャラ」",
			UnlockType = UnlockType.Default
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_14_A03,
			CharaType = CharaType.Chara14,
			Name = "アガリ「スマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item14AgariSR02
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_14_A04,
			CharaType = CharaType.Chara14,
			Name = "アガリ「バニー」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item14AgariSR03
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_14_A05,
			CharaType = CharaType.Chara14,
			Name = "アガリ「コミカルスマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item14AgariSR04
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_14_A06,
			CharaType = CharaType.Chara14,
			Name = "アガリ「デビュー」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item14AgariSR05
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_14_A07,
			CharaType = CharaType.Chara14,
			Name = "アガリ「水着」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item14AgariSR06
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_14_A08,
			CharaType = CharaType.Chara14,
			Name = "アガリ「サキュバスギャル」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item14AgariSR07
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_14_A09,
			CharaType = CharaType.Chara14,
			Name = "アガリ「サキュバスギャル(パンサー)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item14AgariSR08
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_14_A10,
			CharaType = CharaType.Chara14,
			Name = "アガリ「クリスマス」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item14AgariSR09
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_14_A11,
			CharaType = CharaType.Chara14,
			Name = "アガリ「クリスマス(ナイトメア)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item14AgariSR10
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_14_A12,
			CharaType = CharaType.Chara14,
			Name = "アガリ「雀喫茶メイド」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item14AgariSR11
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_14_A13,
			CharaType = CharaType.Chara14,
			Name = "アガリ「サキュバスバレエ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item14AgariSR12
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_14_A14,
			CharaType = CharaType.Chara14,
			Name = "アガリ「サキュバスギャル(アロエ)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item14AgariSR13
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_15_A01,
			CharaType = CharaType.Chara15,
			Name = "アガリ「スタンダード」",
			UnlockType = UnlockType.Default,
			IsDefaultEquip = true
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_15_A02,
			CharaType = CharaType.Chara15,
			Name = "リーチ「プチキャラ」",
			UnlockType = UnlockType.Default
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_15_A03,
			CharaType = CharaType.Chara15,
			Name = "アガリ「スマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item15AgariSR02
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_15_A04,
			CharaType = CharaType.Chara15,
			Name = "アガリ「バニー」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item15AgariSR03
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_15_A05,
			CharaType = CharaType.Chara15,
			Name = "アガリ「コミカルスマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item15AgariSR04
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_15_A06,
			CharaType = CharaType.Chara15,
			Name = "アガリ「デビュー」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item15AgariSR05
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_15_A07,
			CharaType = CharaType.Chara15,
			Name = "アガリ「水着」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item15AgariSR06
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_15_A08,
			CharaType = CharaType.Chara15,
			Name = "アガリ「アニマルフォーム(しろ)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item15AgariSR07
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_15_A09,
			CharaType = CharaType.Chara15,
			Name = "アガリ「アニマルフォーム(くろ)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item15AgariSR08
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_15_A10,
			CharaType = CharaType.Chara15,
			Name = "アガリ「ブクロギャル」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item15AgariSR09
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_15_A11,
			CharaType = CharaType.Chara15,
			Name = "アガリ「ブクロギャル(オニキス)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item15AgariSR10
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_16_A01,
			CharaType = CharaType.Chara16,
			Name = "アガリ「スタンダード」",
			UnlockType = UnlockType.Default,
			IsDefaultEquip = true
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_16_A02,
			CharaType = CharaType.Chara16,
			Name = "リーチ「プチキャラ」",
			UnlockType = UnlockType.Default
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_16_A03,
			CharaType = CharaType.Chara16,
			Name = "アガリ「スマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item16AgariSR02
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_16_A04,
			CharaType = CharaType.Chara16,
			Name = "アガリ「バニー」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item16AgariSR03
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_16_A05,
			CharaType = CharaType.Chara16,
			Name = "アガリ「コミカルスマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item16AgariSR04
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_16_A06,
			CharaType = CharaType.Chara16,
			Name = "アガリ「デビュー」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item16AgariSR05
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_16_A07,
			CharaType = CharaType.Chara16,
			Name = "アガリ「水着」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item16AgariSR06
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_16_A08,
			CharaType = CharaType.Chara16,
			Name = "アガリ「アニマルフォーム(しろ)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item16AgariSR07
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_16_A09,
			CharaType = CharaType.Chara16,
			Name = "アガリ「アニマルフォーム(くろ)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item16AgariSR08
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_16_A10,
			CharaType = CharaType.Chara16,
			Name = "アガリ「バレーボールウェア(帝役)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item16AgariSR09
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_16_A11,
			CharaType = CharaType.Chara16,
			Name = "アガリ「バレーボールウェア(ママさん)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item16AgariSR10
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_16_A12,
			CharaType = CharaType.Chara16,
			Name = "アガリ「ナガタチョウギャル」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item16AgariSR11
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_16_A13,
			CharaType = CharaType.Chara16,
			Name = "アガリ「ナガタチョウギャル(ラベンダー)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item16AgariSR12
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_16_A14,
			CharaType = CharaType.Chara16,
			Name = "アガリ「トレイン(黒)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item16AgariSR13
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_16_A15,
			CharaType = CharaType.Chara16,
			Name = "アガリ「トレイン(白)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item16AgariSR14
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_17_A01,
			CharaType = CharaType.Chara17,
			Name = "アガリ「スタンダード」",
			UnlockType = UnlockType.Default,
			IsDefaultEquip = true
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_17_A02,
			CharaType = CharaType.Chara17,
			Name = "リーチ「プチキャラ」",
			UnlockType = UnlockType.Default
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_17_A03,
			CharaType = CharaType.Chara17,
			Name = "アガリ「スマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item17AgariSR02
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_17_A04,
			CharaType = CharaType.Chara17,
			Name = "アガリ「バニー」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item17AgariSR03
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_17_A05,
			CharaType = CharaType.Chara17,
			Name = "アガリ「コミカルスマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item17AgariSR04
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_17_A06,
			CharaType = CharaType.Chara17,
			Name = "アガリ「デビュー」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item17AgariSR05
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_17_A07,
			CharaType = CharaType.Chara17,
			Name = "アガリ「水着」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item17AgariSR06
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_17_A08,
			CharaType = CharaType.Chara17,
			Name = "アガリ「ランニングウェア(サイダーブルー)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item17AgariSR07
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_17_A09,
			CharaType = CharaType.Chara17,
			Name = "アガリ「ランニングウェア(カロリーオレンジ)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item17AgariSR08
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_18_A01,
			CharaType = CharaType.Chara18,
			Name = "アガリ「スタンダード」",
			UnlockType = UnlockType.Default,
			IsDefaultEquip = true
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_18_A02,
			CharaType = CharaType.Chara18,
			Name = "リーチ「プチキャラ」",
			UnlockType = UnlockType.Default
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_18_A03,
			CharaType = CharaType.Chara18,
			Name = "アガリ「スマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item18AgariSR02
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_18_A04,
			CharaType = CharaType.Chara18,
			Name = "アガリ「バニー」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item18AgariSR03
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_18_A05,
			CharaType = CharaType.Chara18,
			Name = "アガリ「コミカルスマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item18AgariSR04
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_18_A06,
			CharaType = CharaType.Chara18,
			Name = "アガリ「デビュー」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item18AgariSR05
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_18_A07,
			CharaType = CharaType.Chara18,
			Name = "アガリ「水着」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item18AgariSR06
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_18_A08,
			CharaType = CharaType.Chara18,
			Name = "アガリ「りりぃコス」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item18AgariSR07
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_18_A09,
			CharaType = CharaType.Chara18,
			Name = "アガリ「りりぃコス（ホワイト）」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item18AgariSR08
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_19_A01,
			CharaType = CharaType.Chara19,
			Name = "アガリ「スタンダード」",
			UnlockType = UnlockType.Default,
			IsDefaultEquip = true
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_19_A02,
			CharaType = CharaType.Chara19,
			Name = "リーチ「プチキャラ」",
			UnlockType = UnlockType.Default
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_19_A03,
			CharaType = CharaType.Chara19,
			Name = "アガリ「スマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item19AgariSR02
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_19_A04,
			CharaType = CharaType.Chara19,
			Name = "アガリ「バニー」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item19AgariSR03
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_19_A05,
			CharaType = CharaType.Chara19,
			Name = "アガリ「コミカルスマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item19AgariSR04
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_19_A06,
			CharaType = CharaType.Chara19,
			Name = "アガリ「デビュー」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item19AgariSR05
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_19_A07,
			CharaType = CharaType.Chara19,
			Name = "アガリ「水着」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item19AgariSR06
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_19_A08,
			CharaType = CharaType.Chara19,
			Name = "アガリ「バレーボールウェア(ママさん)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item19AgariSR07
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_19_A09,
			CharaType = CharaType.Chara19,
			Name = "アガリ「バレーボールウェア(帝役)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item19AgariSR08
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_20_A01,
			CharaType = CharaType.Chara20,
			Name = "アガリ「スタンダード」",
			UnlockType = UnlockType.Default,
			IsDefaultEquip = true
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_20_A02,
			CharaType = CharaType.Chara20,
			Name = "リーチ「プチキャラ」",
			UnlockType = UnlockType.Default
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_20_A03,
			CharaType = CharaType.Chara20,
			Name = "アガリ「スマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item20AgariSR02
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_20_A04,
			CharaType = CharaType.Chara20,
			Name = "アガリ「バニー」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item20AgariSR03
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_20_A05,
			CharaType = CharaType.Chara20,
			Name = "アガリ「コミカルスマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item20AgariSR04
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_20_A06,
			CharaType = CharaType.Chara20,
			Name = "アガリ「デビュー」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item20AgariSR05
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_20_A07,
			CharaType = CharaType.Chara20,
			Name = "アガリ「水着」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item20AgariSR06
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_20_A08,
			CharaType = CharaType.Chara20,
			Name = "アガリ「雀喫茶メイド」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item20AgariSR07
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_20_A09,
			CharaType = CharaType.Chara20,
			Name = "アガリ「雀喫茶メイド(カワサキピンク)」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item20AgariSR08
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_21_A01,
			CharaType = CharaType.Chara21,
			Name = "アガリ「スタンダード」",
			UnlockType = UnlockType.Default,
			IsDefaultEquip = true
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_21_A02,
			CharaType = CharaType.Chara21,
			Name = "リーチ「プチキャラ」",
			UnlockType = UnlockType.Default
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_21_A03,
			CharaType = CharaType.Chara21,
			Name = "アガリ「スマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item21AgariSR02
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_21_A04,
			CharaType = CharaType.Chara21,
			Name = "アガリ「バニー」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item21AgariSR03
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_21_A05,
			CharaType = CharaType.Chara21,
			Name = "アガリ「コミカルスマッシュ」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item21AgariSR04
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_21_A06,
			CharaType = CharaType.Chara21,
			Name = "アガリ「デビュー」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item21AgariSR05
		},
		new MFG.Types.Customize.MatchingBackground.Item
		{
			ItemID = MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_21_A07,
			CharaType = CharaType.Chara21,
			Name = "アガリ「水着」",
			UnlockType = UnlockType.GachaCutin,
			UnlockingCutinID = MFG.Types.Customize.CutinItem.ItemID.Item21AgariSR06
		}
	};

	public static MFG.Types.Customize.MatchingBackground.Item GetItem(MFG.Types.Customize.MatchingBackground.ItemID itemID)
	{
		return _items.Find((MFG.Types.Customize.MatchingBackground.Item item) => item.ItemID == itemID);
	}

	public static List<MFG.Types.Customize.MatchingBackground.Item> GetItems(Predicate<MFG.Types.Customize.MatchingBackground.Item> match)
	{
		return _items.FindAll(match);
	}

	public static List<MFG.Types.Customize.MatchingBackground.Item> GetItemAll()
	{
		return _items;
	}

	public static List<MFG.Types.Customize.MatchingBackground.ItemID> GetDefaultEquipItemIDs()
	{
		return (from x in _items
			where x.IsDefaultEquip
			select x.ItemID).ToList();
	}

	public static List<MFG.Types.Customize.MatchingBackground.ItemID> GetDefaultItemIDs()
	{
		return (from x in _items
			where x.UnlockType == UnlockType.Default
			select x.ItemID).ToList();
	}

	public static bool IsDefaultItemID(MFG.Types.Customize.MatchingBackground.ItemID itemID)
	{
		return GetDefaultItemIDs().Exists((MFG.Types.Customize.MatchingBackground.ItemID x) => x == itemID);
	}

	public static List<MFG.Types.Customize.MatchingBackground.Item> GetGachaCutinItems()
	{
		return _items.Where((MFG.Types.Customize.MatchingBackground.Item x) => x.UnlockType == UnlockType.GachaCutin).ToList();
	}

	public static MFG.Types.Customize.MatchingBackground.ItemID GetMaItemID(CharaType chara)
	{
		return _items.Find((MFG.Types.Customize.MatchingBackground.Item x) => x.Category == MFG.Types.Customize.MatchingBackground.Category.Ma && x.CharaType == chara).ItemID;
	}

	public static MFG.Types.Customize.MatchingBackground.ItemID GetDefaultItemIDFirst(CharaType chara)
	{
		return (from x in _items
			where x.IsDefaultEquip && x.CharaType == chara
			select x.ItemID).First();
	}

	public static string ToObjectID(MFG.Types.Customize.MatchingBackground.ItemID itemID)
	{
		return $"OID_{itemID}";
	}

	public static MFG.Types.Customize.MatchingBackground.ItemID ToItemID(string oid)
	{
		try
		{
			return Enum.Parse<MFG.Types.Customize.MatchingBackground.ItemID>(oid.Remove(0, 4));
		}
		catch
		{
			return MFG.Types.Customize.MatchingBackground.ItemID.None;
		}
	}

	public static bool IsPonnomichi(MFG.Types.Customize.MatchingBackground.ItemID itemID)
	{
		return itemID == MFG.Types.Customize.MatchingBackground.ItemID.MatchingBG_04_A15;
	}
}
