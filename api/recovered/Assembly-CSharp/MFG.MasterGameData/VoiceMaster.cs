using System;
using System.Collections.Generic;
using MFG.Types;
using MFG.Types.Customize.Voice;

namespace MFG.MasterGameData;

public class VoiceMaster : MasterGameDataBase
{
	private static List<Item> m_items = new List<Item>
	{
		new Item
		{
			ItemID = ItemID.Talk_1,
			Label = "VOX_GXX_MJG_TALK_1",
			Name = "雑談",
			Category = Category.Talk,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.Talk_2,
			Label = "VOX_GXX_MJG_TALK_2",
			Name = "雑談",
			Category = Category.Talk,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.Talk_3,
			Label = "VOX_GXX_MJG_TALK_3",
			Name = "雑談",
			Category = Category.Talk,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 30
		},
		new Item
		{
			ItemID = ItemID.Talk_4,
			Label = "VOX_GXX_MJG_TALK_4",
			Name = "雑談",
			Category = Category.Talk,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 50
		},
		new Item
		{
			ItemID = ItemID.Talk_5,
			Label = "VOX_GXX_MJG_TALK_5",
			Name = "雑談",
			Category = Category.Talk,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 70
		},
		new Item
		{
			ItemID = ItemID.Talk_6,
			Label = "VOX_GXX_MJG_TALK_6",
			Name = "雑談",
			Category = Category.Talk,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 90
		},
		new Item
		{
			ItemID = ItemID.Talk_7,
			Label = "VOX_GXX_MJG_TALK_7",
			Name = "雑談",
			Category = Category.Talk,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 110
		},
		new Item
		{
			ItemID = ItemID.Talk_8,
			Label = "VOX_GXX_MJG_TALK_8",
			Name = "雑談",
			Category = Category.Talk,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 120
		},
		new Item
		{
			ItemID = ItemID.Talk_9,
			Label = "VOX_GXX_MJG_TALK_9",
			Name = "雑談",
			Category = Category.Talk,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 130
		},
		new Item
		{
			ItemID = ItemID.Talk_10,
			Label = "VOX_GXX_MJG_TALK_10",
			Name = "雑談",
			Category = Category.Talk,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 138
		},
		new Item
		{
			ItemID = ItemID.Talk_11,
			Label = "VOX_GXX_MJG_TALK_11",
			Name = "雑談",
			Category = Category.Talk,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 139
		},
		new Item
		{
			ItemID = ItemID.Talk_12,
			Label = "VOX_GXX_MJG_TALK_12",
			Name = "雑談",
			Category = Category.Talk,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 140
		},
		new Item
		{
			ItemID = ItemID.Talk_13,
			Label = "VOX_GXX_MJG_TALK_13",
			Name = "雑談",
			Category = Category.Talk,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 145
		},
		new Item
		{
			ItemID = ItemID.Talk_14,
			Label = "VOX_GXX_MJG_TALK_14",
			Name = "雑談",
			Category = Category.Talk,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 155
		},
		new Item
		{
			ItemID = ItemID.Talk_15,
			Label = "VOX_GXX_MJG_TALK_15",
			Name = "雑談",
			Category = Category.Talk,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 165
		},
		new Item
		{
			ItemID = ItemID.RyukyokuPlus_1,
			Label = "VOX_GXX_MJG_SCORE1_1",
			Name = "流局",
			Category = Category.RyukyokuPlus,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.RyukyokuPlus_2,
			Label = "VOX_GXX_MJG_SCORE1_2",
			Name = "流局",
			Category = Category.RyukyokuPlus,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 56
		},
		new Item
		{
			ItemID = ItemID.RyukyokuPlus_3,
			Label = "VOX_GXX_MJG_SCORE1_3",
			Name = "流局",
			Category = Category.RyukyokuPlus,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 125
		},
		new Item
		{
			ItemID = ItemID.ScorePlus1_1,
			Label = "VOX_GXX_MJG_SCORE2_1",
			Name = "アガリ",
			Category = Category.ScorePlus1,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.ScorePlus1_2,
			Label = "VOX_GXX_MJG_SCORE2_2",
			Name = "アガリ",
			Category = Category.ScorePlus1,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 57
		},
		new Item
		{
			ItemID = ItemID.ScorePlus1_3,
			Label = "VOX_GXX_MJG_SCORE2_3",
			Name = "アガリ",
			Category = Category.ScorePlus1,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 126
		},
		new Item
		{
			ItemID = ItemID.ScorePlus2_1,
			Label = "VOX_GXX_MJG_SCORE3_1",
			Name = "アガリ",
			Category = Category.ScorePlus2,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.ScorePlus2_2,
			Label = "VOX_GXX_MJG_SCORE3_2",
			Name = "アガリ",
			Category = Category.ScorePlus2,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 58
		},
		new Item
		{
			ItemID = ItemID.ScorePlus2_3,
			Label = "VOX_GXX_MJG_SCORE3_3",
			Name = "アガリ",
			Category = Category.ScorePlus2,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 127
		},
		new Item
		{
			ItemID = ItemID.ScorePlus3_1,
			Label = "VOX_GXX_MJG_SCORE4_1",
			Name = "アガリ",
			Category = Category.ScorePlus3,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.ScorePlus3_2,
			Label = "VOX_GXX_MJG_SCORE4_2",
			Name = "アガリ",
			Category = Category.ScorePlus3,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 59
		},
		new Item
		{
			ItemID = ItemID.ScorePlus3_3,
			Label = "VOX_GXX_MJG_SCORE4_3",
			Name = "アガリ",
			Category = Category.ScorePlus3,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 128
		},
		new Item
		{
			ItemID = ItemID.ScorePlus4_1,
			Label = "VOX_GXX_MJG_SCORE5_1",
			Name = "アガリ",
			Category = Category.ScorePlus4,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.ScorePlus4_2,
			Label = "VOX_GXX_MJG_SCORE5_2",
			Name = "アガリ",
			Category = Category.ScorePlus4,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 61
		},
		new Item
		{
			ItemID = ItemID.ScorePlus4_3,
			Label = "VOX_GXX_MJG_SCORE5_3",
			Name = "アガリ",
			Category = Category.ScorePlus4,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 129
		},
		new Item
		{
			ItemID = ItemID.RyukyokuMinus_1,
			Label = "VOX_GXX_MJG_SCORE6_1",
			Name = "流局",
			Category = Category.RyukyokuMinus,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.RyukyokuMinus_2,
			Label = "VOX_GXX_MJG_SCORE6_2",
			Name = "流局",
			Category = Category.RyukyokuMinus,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 62
		},
		new Item
		{
			ItemID = ItemID.RyukyokuMinus_3,
			Label = "VOX_GXX_MJG_SCORE6_3",
			Name = "流局",
			Category = Category.RyukyokuMinus,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 131
		},
		new Item
		{
			ItemID = ItemID.ScoreMinus1_1,
			Label = "VOX_GXX_MJG_SCORE7_1",
			Name = "支払い",
			Category = Category.ScoreMinus1,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.ScoreMinus1_2,
			Label = "VOX_GXX_MJG_SCORE7_2",
			Name = "支払い",
			Category = Category.ScoreMinus1,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 63
		},
		new Item
		{
			ItemID = ItemID.ScoreMinus1_3,
			Label = "VOX_GXX_MJG_SCORE7_3",
			Name = "支払い",
			Category = Category.ScoreMinus1,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 132
		},
		new Item
		{
			ItemID = ItemID.ScoreMinus2_1,
			Label = "VOX_GXX_MJG_SCORE8_1",
			Name = "支払い",
			Category = Category.ScoreMinus2,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.ScoreMinus2_2,
			Label = "VOX_GXX_MJG_SCORE8_2",
			Name = "支払い",
			Category = Category.ScoreMinus2,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 64
		},
		new Item
		{
			ItemID = ItemID.ScoreMinus2_3,
			Label = "VOX_GXX_MJG_SCORE8_3",
			Name = "支払い",
			Category = Category.ScoreMinus2,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 133
		},
		new Item
		{
			ItemID = ItemID.ScoreMinus3_1,
			Label = "VOX_GXX_MJG_SCORE9_1",
			Name = "支払い",
			Category = Category.ScoreMinus3,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.ScoreMinus3_2,
			Label = "VOX_GXX_MJG_SCORE9_2",
			Name = "支払い",
			Category = Category.ScoreMinus3,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 65
		},
		new Item
		{
			ItemID = ItemID.ScoreMinus3_3,
			Label = "VOX_GXX_MJG_SCORE9_3",
			Name = "支払い",
			Category = Category.ScoreMinus3,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 134
		},
		new Item
		{
			ItemID = ItemID.ScoreMinus4_1,
			Label = "VOX_GXX_MJG_SCORE10_1",
			Name = "支払い",
			Category = Category.ScoreMinus4,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.ScoreMinus4_2,
			Label = "VOX_GXX_MJG_SCORE10_2",
			Name = "支払い",
			Category = Category.ScoreMinus4,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 66
		},
		new Item
		{
			ItemID = ItemID.ScoreMinus4_3,
			Label = "VOX_GXX_MJG_SCORE10_3",
			Name = "支払い",
			Category = Category.ScoreMinus4,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 135
		},
		new Item
		{
			ItemID = ItemID.ScoreTobi_1,
			Label = "VOX_GXX_MJG_SCORE11_1",
			Name = "支払い",
			Category = Category.ScoreTobi,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.ScoreTobi_2,
			Label = "VOX_GXX_MJG_SCORE11_2",
			Name = "支払い",
			Category = Category.ScoreTobi,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 67
		},
		new Item
		{
			ItemID = ItemID.ScoreTobi_3,
			Label = "VOX_GXX_MJG_SCORE11_3",
			Name = "支払い",
			Category = Category.ScoreTobi,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 136
		},
		new Item
		{
			ItemID = ItemID.ScoreNothing_1,
			Label = "VOX_GXX_MJG_SCORE12_1",
			Name = "流局",
			Category = Category.ScoreNothing,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.ScoreNothing_2,
			Label = "VOX_GXX_MJG_SCORE12_2",
			Name = "流局",
			Category = Category.ScoreNothing,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 68
		},
		new Item
		{
			ItemID = ItemID.ScoreNothing_3,
			Label = "VOX_GXX_MJG_SCORE12_3",
			Name = "流局",
			Category = Category.ScoreNothing,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 137
		},
		new Item
		{
			ItemID = ItemID.ScoreIndigenousYaku,
			Label = "VOX_GXX_MJG_SCORE13",
			Name = "固有役和了",
			Category = Category.ScoreIndigenousYaku,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.AgariN_1,
			Label = "VOX_GXX_MJG_AGARI_N_1",
			Name = "ガールズカットイン",
			Category = Category.AgariN,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.AgariN_2,
			Label = "VOX_GXX_MJG_AGARI_N_2",
			Name = "ガールズカットイン",
			Category = Category.AgariN,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 55
		},
		new Item
		{
			ItemID = ItemID.AgariN_3,
			Label = "VOX_GXX_MJG_AGARI_N_3",
			Name = "ガールズカットイン",
			Category = Category.AgariN,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 124
		},
		new Item
		{
			ItemID = ItemID.AgariSR_1,
			Label = "VOX_GXX_MJG_AGARI_SR_1",
			Name = "ガールズカットイン",
			Category = Category.AgariSR,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.AgariSR_2,
			Label = "VOX_GXX_MJG_AGARI_SR_2",
			Name = "ガールズカットイン",
			Category = Category.AgariSR,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.AgariSR_3,
			Label = "VOX_GXX_MJG_AGARI_SR_3",
			Name = "ガールズカットイン",
			Category = Category.AgariSR,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 15
		},
		new Item
		{
			ItemID = ItemID.AgariSR_4,
			Label = "VOX_GXX_MJG_AGARI_SR_4",
			Name = "ガールズカットイン",
			Category = Category.AgariSR,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 51
		},
		new Item
		{
			ItemID = ItemID.AgariSR_5,
			Label = "VOX_GXX_MJG_AGARI_SR_5",
			Name = "ガールズカットイン",
			Category = Category.AgariSR,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 84
		},
		new Item
		{
			ItemID = ItemID.AgariSR_6,
			Label = "VOX_GXX_MJG_AGARI_SR_6",
			Name = "ガールズカットイン",
			Category = Category.AgariSR,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 119
		},
		new Item
		{
			ItemID = ItemID.ReachCutin_1,
			Label = "VOX_GXX_MJG_REACH_N",
			Name = "リーチ",
			Category = Category.ReachCutin,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.ReachCutin_2,
			Label = "VOX_GXX_MJG_REACH_R",
			Name = "リーチ",
			Category = Category.ReachCutin,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.ReachCutin_3,
			Label = "VOX_GXX_MJG_REACH_G1",
			Name = "リーチ",
			Category = Category.ReachCutin,
			ObtainType = ObtainType.Gacha,
			GachaItemNo = 35
		},
		new Item
		{
			ItemID = ItemID.ReachCutin_4,
			Label = "VOX_GXX_MJG_REACH_G2",
			Name = "リーチ",
			Category = Category.ReachCutin,
			ObtainType = ObtainType.Gacha,
			GachaItemNo = 36
		},
		new Item
		{
			ItemID = ItemID.DoubleReachCutin_1,
			Label = "VOX_GXX_MJG_DREACH_N",
			Name = "リーチ",
			Category = Category.DoubleReachCutin,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.DoubleReachCutin_2,
			Label = "VOX_GXX_MJG_DREACH_R",
			Name = "リーチ",
			Category = Category.DoubleReachCutin,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.OwnMeld_1,
			Label = "VOX_GXX_MJG_COMMENT1_1",
			Name = "鳴き",
			Category = Category.OwnMeld,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.OwnMeld_2,
			Label = "VOX_GXX_MJG_COMMENT1_2",
			Name = "鳴き",
			Category = Category.OwnMeld,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.OwnMeld_3,
			Label = "VOX_GXX_MJG_COMMENT1_3",
			Name = "鳴き",
			Category = Category.OwnMeld,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 8
		},
		new Item
		{
			ItemID = ItemID.OwnMeld_4,
			Label = "VOX_GXX_MJG_COMMENT1_4",
			Name = "鳴き",
			Category = Category.OwnMeld,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 44
		},
		new Item
		{
			ItemID = ItemID.OwnMeld_5,
			Label = "VOX_GXX_MJG_COMMENT1_5",
			Name = "鳴き",
			Category = Category.OwnMeld,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 77
		},
		new Item
		{
			ItemID = ItemID.OwnMeld_6,
			Label = "VOX_GXX_MJG_COMMENT1_6",
			Name = "鳴き",
			Category = Category.OwnMeld,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 113
		},
		new Item
		{
			ItemID = ItemID.Melded_1,
			Label = "VOX_GXX_MJG_COMMENT2_1",
			Name = "鳴き(他家)",
			Category = Category.Melded,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.Melded_2,
			Label = "VOX_GXX_MJG_COMMENT2_2",
			Name = "鳴き(他家)",
			Category = Category.Melded,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.Melded_3,
			Label = "VOX_GXX_MJG_COMMENT2_3",
			Name = "鳴き(他家)",
			Category = Category.Melded,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 9
		},
		new Item
		{
			ItemID = ItemID.Melded_4,
			Label = "VOX_GXX_MJG_COMMENT2_4",
			Name = "鳴き(他家)",
			Category = Category.Melded,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 45
		},
		new Item
		{
			ItemID = ItemID.Melded_5,
			Label = "VOX_GXX_MJG_COMMENT2_5",
			Name = "鳴き(他家)",
			Category = Category.Melded,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 78
		},
		new Item
		{
			ItemID = ItemID.Melded_6,
			Label = "VOX_GXX_MJG_COMMENT2_6",
			Name = "鳴き(他家)",
			Category = Category.Melded,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 114
		},
		new Item
		{
			ItemID = ItemID.MyTurnSkipped_1,
			Label = "VOX_GXX_MJG_COMMENT3_1",
			Name = "鳴き(他家)",
			Category = Category.MyTurnSkipped,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.MyTurnSkipped_2,
			Label = "VOX_GXX_MJG_COMMENT3_2",
			Name = "鳴き(他家)",
			Category = Category.MyTurnSkipped,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.MyTurnSkipped_3,
			Label = "VOX_GXX_MJG_COMMENT3_3",
			Name = "鳴き(他家)",
			Category = Category.MyTurnSkipped,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 11
		},
		new Item
		{
			ItemID = ItemID.MyTurnSkipped_4,
			Label = "VOX_GXX_MJG_COMMENT3_4",
			Name = "鳴き(他家)",
			Category = Category.MyTurnSkipped,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 46
		},
		new Item
		{
			ItemID = ItemID.MyTurnSkipped_5,
			Label = "VOX_GXX_MJG_COMMENT3_5",
			Name = "鳴き(他家)",
			Category = Category.MyTurnSkipped,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 79
		},
		new Item
		{
			ItemID = ItemID.MyTurnSkipped_6,
			Label = "VOX_GXX_MJG_COMMENT3_6",
			Name = "鳴き(他家)",
			Category = Category.MyTurnSkipped,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 115
		},
		new Item
		{
			ItemID = ItemID.DoraMelded_1,
			Label = "VOX_GXX_MJG_COMMENT4_1",
			Name = "鳴き(他家)",
			Category = Category.DoraMelded,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.DoraMelded_2,
			Label = "VOX_GXX_MJG_COMMENT4_2",
			Name = "鳴き(他家)",
			Category = Category.DoraMelded,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 54
		},
		new Item
		{
			ItemID = ItemID.DoraMelded_3,
			Label = "VOX_GXX_MJG_COMMENT4_3",
			Name = "鳴き(他家)",
			Category = Category.DoraMelded,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 123
		},
		new Item
		{
			ItemID = ItemID.Reach_1,
			Label = "VOX_GXX_MJG_COMMENT5_1",
			Name = "リーチ後",
			Category = Category.Reach,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.Reach_2,
			Label = "VOX_GXX_MJG_COMMENT5_2",
			Name = "リーチ後",
			Category = Category.Reach,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.Reach_1,
			Label = "VOX_GXX_MJG_COMMENT5_1",
			Name = "リーチ後",
			Category = Category.Reach,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.Reach_2,
			Label = "VOX_GXX_MJG_COMMENT5_2",
			Name = "リーチ後",
			Category = Category.Reach,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.Reach_3,
			Label = "VOX_GXX_MJG_COMMENT5_3",
			Name = "リーチ後",
			Category = Category.Reach,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 12
		},
		new Item
		{
			ItemID = ItemID.Reach_4,
			Label = "VOX_GXX_MJG_COMMENT5_4",
			Name = "リーチ後",
			Category = Category.Reach,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 47
		},
		new Item
		{
			ItemID = ItemID.Reach_5,
			Label = "VOX_GXX_MJG_COMMENT5_5",
			Name = "リーチ後",
			Category = Category.Reach,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 81
		},
		new Item
		{
			ItemID = ItemID.Reach_6,
			Label = "VOX_GXX_MJG_COMMENT5_6",
			Name = "リーチ後",
			Category = Category.Reach,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 116
		},
		new Item
		{
			ItemID = ItemID.OkkakeReach_1,
			Label = "VOX_GXX_MJG_COMMENT6_1",
			Name = "リーチ後",
			Category = Category.OkkakeReach,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.OkkakeReach_2,
			Label = "VOX_GXX_MJG_COMMENT6_2",
			Name = "リーチ後",
			Category = Category.OkkakeReach,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.OkkakeReach_3,
			Label = "VOX_GXX_MJG_COMMENT6_3",
			Name = "リーチ後",
			Category = Category.OkkakeReach,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 13
		},
		new Item
		{
			ItemID = ItemID.OkkakeReach_4,
			Label = "VOX_GXX_MJG_COMMENT6_4",
			Name = "リーチ後",
			Category = Category.OkkakeReach,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 48
		},
		new Item
		{
			ItemID = ItemID.OkkakeReach_5,
			Label = "VOX_GXX_MJG_COMMENT6_5",
			Name = "リーチ後",
			Category = Category.OkkakeReach,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 82
		},
		new Item
		{
			ItemID = ItemID.OkkakeReach_6,
			Label = "VOX_GXX_MJG_COMMENT6_6",
			Name = "リーチ後",
			Category = Category.OkkakeReach,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 117
		},
		new Item
		{
			ItemID = ItemID.OkkakeReached_1,
			Label = "VOX_GXX_MJG_COMMENT7_1",
			Name = "リーチ後(他家)",
			Category = Category.OkkakeReached,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.OkkakeReached_2,
			Label = "VOX_GXX_MJG_COMMENT7_2",
			Name = "リーチ後(他家)",
			Category = Category.OkkakeReached,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.OkkakeReached_3,
			Label = "VOX_GXX_MJG_COMMENT7_3",
			Name = "リーチ後(他家)",
			Category = Category.OkkakeReached,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 14
		},
		new Item
		{
			ItemID = ItemID.OkkakeReached_4,
			Label = "VOX_GXX_MJG_COMMENT7_4",
			Name = "リーチ後(他家)",
			Category = Category.OkkakeReached,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 49
		},
		new Item
		{
			ItemID = ItemID.OkkakeReached_5,
			Label = "VOX_GXX_MJG_COMMENT7_5",
			Name = "リーチ後(他家)",
			Category = Category.OkkakeReached,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 83
		},
		new Item
		{
			ItemID = ItemID.OkkakeReached_6,
			Label = "VOX_GXX_MJG_COMMENT7_6",
			Name = "リーチ後(他家)",
			Category = Category.OkkakeReached,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 118
		},
		new Item
		{
			ItemID = ItemID.LongThinking_1,
			Label = "VOX_GXX_MJG_THINK_1",
			Name = "長考",
			Category = Category.LongThinking,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.LongThinking_2,
			Label = "VOX_GXX_MJG_THINK_2",
			Name = "長考",
			Category = Category.LongThinking,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.LongThinking_3,
			Label = "VOX_GXX_MJG_THINK_3",
			Name = "長考",
			Category = Category.LongThinking,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 7
		},
		new Item
		{
			ItemID = ItemID.LongThinking_4,
			Label = "VOX_GXX_MJG_THINK_4",
			Name = "長考",
			Category = Category.LongThinking,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 43
		},
		new Item
		{
			ItemID = ItemID.LongThinking_5,
			Label = "VOX_GXX_MJG_THINK_5",
			Name = "長考",
			Category = Category.LongThinking,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 76
		},
		new Item
		{
			ItemID = ItemID.LongThinking_6,
			Label = "VOX_GXX_MJG_THINK_6",
			Name = "長考",
			Category = Category.LongThinking,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 112
		},
		new Item
		{
			ItemID = ItemID.Dahai_1,
			Label = "VOX_GXX_MJG_DAHAI1_1",
			Name = "打牌",
			Category = Category.Dahai,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.Dahai_2,
			Label = "VOX_GXX_MJG_DAHAI1_2",
			Name = "打牌",
			Category = Category.Dahai,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.Dahai_3,
			Label = "VOX_GXX_MJG_DAHAI1_3",
			Name = "打牌",
			Category = Category.Dahai,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 3
		},
		new Item
		{
			ItemID = ItemID.Dahai_4,
			Label = "VOX_GXX_MJG_DAHAI1_4",
			Name = "打牌",
			Category = Category.Dahai,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 38
		},
		new Item
		{
			ItemID = ItemID.Dahai_5,
			Label = "VOX_GXX_MJG_DAHAI1_5",
			Name = "打牌",
			Category = Category.Dahai,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 72
		},
		new Item
		{
			ItemID = ItemID.Dahai_6,
			Label = "VOX_GXX_MJG_DAHAI1_6",
			Name = "打牌",
			Category = Category.Dahai,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 107
		},
		new Item
		{
			ItemID = ItemID.Dahai_7,
			Label = "VOX_GXX_MJG_DAHAI1_G1",
			Name = "打牌",
			Category = Category.Dahai,
			ObtainType = ObtainType.Gacha,
			GachaItemNo = 14
		},
		new Item
		{
			ItemID = ItemID.Dahai_8,
			Label = "VOX_GXX_MJG_DAHAI1_G2",
			Name = "打牌",
			Category = Category.Dahai,
			ObtainType = ObtainType.Gacha,
			GachaItemNo = 15
		},
		new Item
		{
			ItemID = ItemID.Dahai_9,
			Label = "VOX_GXX_MJG_DAHAI1_G3",
			Name = "打牌",
			Category = Category.Dahai,
			ObtainType = ObtainType.Gacha,
			GachaItemNo = 16
		},
		new Item
		{
			ItemID = ItemID.Dahai_10,
			Label = "VOX_GXX_MJG_DAHAI1_G4",
			Name = "打牌",
			Category = Category.Dahai,
			ObtainType = ObtainType.Gacha,
			GachaItemNo = 17
		},
		new Item
		{
			ItemID = ItemID.DahaiAnpai_1,
			Label = "VOX_GXX_MJG_DAHAI2_1",
			Name = "安全牌",
			Category = Category.DahaiAnpai,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.DahaiAnpai_2,
			Label = "VOX_GXX_MJG_DAHAI2_2",
			Name = "安全牌",
			Category = Category.DahaiAnpai,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.DahaiAnpai_3,
			Label = "VOX_GXX_MJG_DAHAI2_3",
			Name = "安全牌",
			Category = Category.DahaiAnpai,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 4
		},
		new Item
		{
			ItemID = ItemID.DahaiAnpai_4,
			Label = "VOX_GXX_MJG_DAHAI2_4",
			Name = "安全牌",
			Category = Category.DahaiAnpai,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 39
		},
		new Item
		{
			ItemID = ItemID.DahaiAnpai_5,
			Label = "VOX_GXX_MJG_DAHAI2_5",
			Name = "安全牌",
			Category = Category.DahaiAnpai,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 73
		},
		new Item
		{
			ItemID = ItemID.DahaiAnpai_6,
			Label = "VOX_GXX_MJG_DAHAI2_6",
			Name = "安全牌",
			Category = Category.DahaiAnpai,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 108
		},
		new Item
		{
			ItemID = ItemID.DahaiAnpai_7,
			Label = "VOX_GXX_MJG_DAHAI2_G1",
			Name = "安全牌",
			Category = Category.DahaiAnpai,
			ObtainType = ObtainType.Gacha,
			GachaItemNo = 18
		},
		new Item
		{
			ItemID = ItemID.DahaiAnpai_8,
			Label = "VOX_GXX_MJG_DAHAI2_G2",
			Name = "安全牌",
			Category = Category.DahaiAnpai,
			ObtainType = ObtainType.Gacha,
			GachaItemNo = 19
		},
		new Item
		{
			ItemID = ItemID.DahaiDanger_1,
			Label = "VOX_GXX_MJG_DAHAI3_1",
			Name = "危険牌",
			Category = Category.DahaiDanger,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.DahaiDanger_2,
			Label = "VOX_GXX_MJG_DAHAI3_2",
			Name = "危険牌",
			Category = Category.DahaiDanger,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.DahaiDanger_3,
			Label = "VOX_GXX_MJG_DAHAI3_3",
			Name = "危険牌",
			Category = Category.DahaiDanger,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 5
		},
		new Item
		{
			ItemID = ItemID.DahaiDanger_4,
			Label = "VOX_GXX_MJG_DAHAI3_4",
			Name = "危険牌",
			Category = Category.DahaiDanger,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 41
		},
		new Item
		{
			ItemID = ItemID.DahaiDanger_5,
			Label = "VOX_GXX_MJG_DAHAI3_5",
			Name = "危険牌",
			Category = Category.DahaiDanger,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 74
		},
		new Item
		{
			ItemID = ItemID.DahaiDanger_6,
			Label = "VOX_GXX_MJG_DAHAI3_6",
			Name = "危険牌",
			Category = Category.DahaiDanger,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 109
		},
		new Item
		{
			ItemID = ItemID.DahaiDanger_7,
			Label = "VOX_GXX_MJG_DAHAI3_G1",
			Name = "危険牌",
			Category = Category.DahaiDanger,
			ObtainType = ObtainType.Gacha,
			GachaItemNo = 20
		},
		new Item
		{
			ItemID = ItemID.DahaiDanger_8,
			Label = "VOX_GXX_MJG_DAHAI3_G2",
			Name = "危険牌",
			Category = Category.DahaiDanger,
			ObtainType = ObtainType.Gacha,
			GachaItemNo = 21
		},
		new Item
		{
			ItemID = ItemID.DahaiTsumoGiri1_1,
			Label = "VOX_GXX_MJG_DAHAI4_1",
			Name = "ツモ切り",
			Category = Category.DahaiTsumoGiri1,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.DahaiTsumoGiri1_2,
			Label = "VOX_GXX_MJG_DAHAI4_2",
			Name = "ツモ切り",
			Category = Category.DahaiTsumoGiri1,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.DahaiTsumoGiri1_3,
			Label = "VOX_GXX_MJG_DAHAI4_3",
			Name = "ツモ切り",
			Category = Category.DahaiTsumoGiri1,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 6
		},
		new Item
		{
			ItemID = ItemID.DahaiTsumoGiri1_4,
			Label = "VOX_GXX_MJG_DAHAI4_4",
			Name = "ツモ切り",
			Category = Category.DahaiTsumoGiri1,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 42
		},
		new Item
		{
			ItemID = ItemID.DahaiTsumoGiri1_5,
			Label = "VOX_GXX_MJG_DAHAI4_5",
			Name = "ツモ切り",
			Category = Category.DahaiTsumoGiri1,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 75
		},
		new Item
		{
			ItemID = ItemID.DahaiTsumoGiri1_6,
			Label = "VOX_GXX_MJG_DAHAI4_6",
			Name = "ツモ切り",
			Category = Category.DahaiTsumoGiri1,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 111
		},
		new Item
		{
			ItemID = ItemID.DahaiTsumoGiri1_7,
			Label = "VOX_GXX_MJG_DAHAI4_G1",
			Name = "ツモ切り",
			Category = Category.DahaiTsumoGiri1,
			ObtainType = ObtainType.Gacha,
			GachaItemNo = 24
		},
		new Item
		{
			ItemID = ItemID.DahaiTsumoGiri1_8,
			Label = "VOX_GXX_MJG_DAHAI4_G2",
			Name = "ツモ切り",
			Category = Category.DahaiTsumoGiri1,
			ObtainType = ObtainType.Gacha,
			GachaItemNo = 25
		},
		new Item
		{
			ItemID = ItemID.DahaiTsumoGiri1_9,
			Label = "VOX_GXX_MJG_DAHAI4_G3",
			Name = "ツモ切り",
			Category = Category.DahaiTsumoGiri1,
			ObtainType = ObtainType.Gacha,
			GachaItemNo = 26
		},
		new Item
		{
			ItemID = ItemID.DahaiTsumoGiri1_10,
			Label = "VOX_GXX_MJG_DAHAI4_G4",
			Name = "ツモ切り",
			Category = Category.DahaiTsumoGiri1,
			ObtainType = ObtainType.Gacha,
			GachaItemNo = 27
		},
		new Item
		{
			ItemID = ItemID.DahaiTsumoGiri1_11,
			Label = "VOX_GXX_MJG_DAHAI4_G5",
			Name = "ツモ切り",
			Category = Category.DahaiTsumoGiri1,
			ObtainType = ObtainType.Gacha,
			GachaItemNo = 28
		},
		new Item
		{
			ItemID = ItemID.DahaiTsumoGiri2_1,
			Label = "VOX_GXX_MJG_DAHAI5_1",
			Name = "ツモ切り",
			Category = Category.DahaiTsumoGiri2,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.DahaiTsumoGiri2_2,
			Label = "VOX_GXX_MJG_DAHAI5_2",
			Name = "ツモ切り",
			Category = Category.DahaiTsumoGiri2,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 35
		},
		new Item
		{
			ItemID = ItemID.DahaiTsumoGiri2_3,
			Label = "VOX_GXX_MJG_DAHAI5_3",
			Name = "ツモ切り",
			Category = Category.DahaiTsumoGiri2,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 104
		},
		new Item
		{
			ItemID = ItemID.DahaiTsumoGiri2_4,
			Label = "VOX_GXX_MJG_DAHAI5_G1",
			Name = "ツモ切り",
			Category = Category.DahaiTsumoGiri2,
			ObtainType = ObtainType.Gacha,
			GachaItemNo = 29
		},
		new Item
		{
			ItemID = ItemID.DahaiTsumoGiri2_5,
			Label = "VOX_GXX_MJG_DAHAI5_G2",
			Name = "ツモ切り",
			Category = Category.DahaiTsumoGiri2,
			ObtainType = ObtainType.Gacha,
			GachaItemNo = 30
		},
		new Item
		{
			ItemID = ItemID.DahaiTsumoGiri3_1,
			Label = "VOX_GXX_MJG_DAHAI6_1",
			Name = "ツモ切り",
			Category = Category.DahaiTsumoGiri3,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.DahaiTsumoGiri3_2,
			Label = "VOX_GXX_MJG_DAHAI6_2",
			Name = "ツモ切り",
			Category = Category.DahaiTsumoGiri3,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 52
		},
		new Item
		{
			ItemID = ItemID.DahaiTsumoGiri3_3,
			Label = "VOX_GXX_MJG_DAHAI6_3",
			Name = "ツモ切り",
			Category = Category.DahaiTsumoGiri3,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 121
		},
		new Item
		{
			ItemID = ItemID.DahaiTsumoGiri3_4,
			Label = "VOX_GXX_MJG_DAHAI6_G1",
			Name = "ツモ切り",
			Category = Category.DahaiTsumoGiri3,
			ObtainType = ObtainType.Gacha,
			GachaItemNo = 31
		},
		new Item
		{
			ItemID = ItemID.DahaiTsumoGiri3_5,
			Label = "VOX_GXX_MJG_DAHAI6_G2",
			Name = "ツモ切り",
			Category = Category.DahaiTsumoGiri3,
			ObtainType = ObtainType.Gacha,
			GachaItemNo = 32
		},
		new Item
		{
			ItemID = ItemID.DahaiDora_1,
			Label = "VOX_GXX_MJG_DAHAI7_1",
			Name = "ドラ切り",
			Category = Category.DahaiDora,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.DahaiDora_2,
			Label = "VOX_GXX_MJG_DAHAI7_2",
			Name = "ドラ切り",
			Category = Category.DahaiDora,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 34
		},
		new Item
		{
			ItemID = ItemID.DahaiDora_3,
			Label = "VOX_GXX_MJG_DAHAI7_3",
			Name = "ドラ切り",
			Category = Category.DahaiDora,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 103
		},
		new Item
		{
			ItemID = ItemID.DahaiDora_4,
			Label = "VOX_GXX_MJG_DAHAI7_G1",
			Name = "ドラ切り",
			Category = Category.DahaiDora,
			ObtainType = ObtainType.Gacha,
			GachaItemNo = 33
		},
		new Item
		{
			ItemID = ItemID.DahaiDora_5,
			Label = "VOX_GXX_MJG_DAHAI7_G2",
			Name = "ドラ切り",
			Category = Category.DahaiDora,
			ObtainType = ObtainType.Gacha,
			GachaItemNo = 34
		},
		new Item
		{
			ItemID = ItemID.DahaiReachDanger_1,
			Label = "VOX_GXX_MJG_DAHAI8_1",
			Name = "危険牌",
			Category = Category.DahaiReachDanger,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.DahaiReachDanger_2,
			Label = "VOX_GXX_MJG_DAHAI8_2",
			Name = "危険牌",
			Category = Category.DahaiReachDanger,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 53
		},
		new Item
		{
			ItemID = ItemID.DahaiReachDanger_3,
			Label = "VOX_GXX_MJG_DAHAI8_3",
			Name = "危険牌",
			Category = Category.DahaiReachDanger,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 122
		},
		new Item
		{
			ItemID = ItemID.DahaiReachDanger_4,
			Label = "VOX_GXX_MJG_DAHAI8_G1",
			Name = "危険牌",
			Category = Category.DahaiReachDanger,
			ObtainType = ObtainType.Gacha,
			GachaItemNo = 22
		},
		new Item
		{
			ItemID = ItemID.DahaiReachDanger_5,
			Label = "VOX_GXX_MJG_DAHAI8_G2",
			Name = "危険牌",
			Category = Category.DahaiReachDanger,
			ObtainType = ObtainType.Gacha,
			GachaItemNo = 23
		},
		new Item
		{
			ItemID = ItemID.ChunBeam1,
			Label = "VOX_GXX_MJG_CHUN1",
			Name = "中ビーム",
			Category = Category.ChunBeam1,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.ChunBeam2,
			Label = "VOX_GXX_MJG_CHUN2",
			Name = "中ビーム",
			Category = Category.ChunBeam2,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.ChunBeam3,
			Label = "VOX_GXX_MJG_CHUN3",
			Name = "中ビーム",
			Category = Category.ChunBeam3,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.ChunBeam4,
			Label = "VOX_GXX_MJG_CHUN4",
			Name = "中ビーム",
			Category = Category.ChunBeam4,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.Tsumo_1,
			Label = "VOX_GXX_MJG_PICK1_1",
			Name = "ツモ",
			Category = Category.Tsumo,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.Tsumo_2,
			Label = "VOX_GXX_MJG_PICK1_2",
			Name = "ツモ",
			Category = Category.Tsumo,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.Tsumo_3,
			Label = "VOX_GXX_MJG_PICK1_3",
			Name = "ツモ",
			Category = Category.Tsumo,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.Tsumo_4,
			Label = "VOX_GXX_MJG_PICK1_4",
			Name = "ツモ",
			Category = Category.Tsumo,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 36
		},
		new Item
		{
			ItemID = ItemID.Tsumo_5,
			Label = "VOX_GXX_MJG_PICK1_5",
			Name = "ツモ",
			Category = Category.Tsumo,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 69
		},
		new Item
		{
			ItemID = ItemID.Tsumo_6,
			Label = "VOX_GXX_MJG_PICK1_6",
			Name = "ツモ",
			Category = Category.Tsumo,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 105
		},
		new Item
		{
			ItemID = ItemID.Tsumo_7,
			Label = "VOX_GXX_MJG_PICK1_G1",
			Name = "ツモ",
			Category = Category.Tsumo,
			ObtainType = ObtainType.Gacha,
			GachaItemNo = 5
		},
		new Item
		{
			ItemID = ItemID.Tsumo_8,
			Label = "VOX_GXX_MJG_PICK1_G2",
			Name = "ツモ",
			Category = Category.Tsumo,
			ObtainType = ObtainType.Gacha,
			GachaItemNo = 6
		},
		new Item
		{
			ItemID = ItemID.Tsumo2Shanten_1,
			Label = "VOX_GXX_MJG_NOKORI2_1",
			Name = "ツモ",
			Category = Category.Tsumo2Shanten,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.Tsumo2Shanten_2,
			Label = "VOX_GXX_MJG_NOKORI2_2",
			Name = "ツモ",
			Category = Category.Tsumo2Shanten,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.Tsumo2Shanten_3,
			Label = "VOX_GXX_MJG_NOKORI2_3",
			Name = "ツモ",
			Category = Category.Tsumo2Shanten,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.Tsumo2Shanten_4,
			Label = "VOX_GXX_MJG_NOKORI2_4",
			Name = "ツモ",
			Category = Category.Tsumo2Shanten,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.Tsumo2Shanten_5,
			Label = "VOX_GXX_MJG_NOKORI2_5",
			Name = "ツモ",
			Category = Category.Tsumo2Shanten,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.Tsumo2Shanten_6,
			Label = "VOX_GXX_MJG_NOKORI2_6",
			Name = "ツモ",
			Category = Category.Tsumo2Shanten,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.Tsumo2Shanten_7,
			Label = "VOX_GXX_MJG_NOKORI2_G1",
			Name = "ツモ",
			Category = Category.Tsumo2Shanten,
			ObtainType = ObtainType.Gacha,
			GachaItemNo = 7
		},
		new Item
		{
			ItemID = ItemID.Tsumo2Shanten_8,
			Label = "VOX_GXX_MJG_NOKORI2_G2",
			Name = "ツモ",
			Category = Category.Tsumo2Shanten,
			ObtainType = ObtainType.Gacha,
			GachaItemNo = 8
		},
		new Item
		{
			ItemID = ItemID.Tsumo1Shanten_1,
			Label = "VOX_GXX_MJG_NOKORI1_1",
			Name = "ツモ",
			Category = Category.Tsumo1Shanten,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.Tsumo1Shanten_2,
			Label = "VOX_GXX_MJG_NOKORI1_2",
			Name = "ツモ",
			Category = Category.Tsumo1Shanten,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.Tsumo1Shanten_3,
			Label = "VOX_GXX_MJG_NOKORI1_3",
			Name = "ツモ",
			Category = Category.Tsumo1Shanten,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.Tsumo1Shanten_4,
			Label = "VOX_GXX_MJG_NOKORI1_4",
			Name = "ツモ",
			Category = Category.Tsumo1Shanten,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.Tsumo1Shanten_5,
			Label = "VOX_GXX_MJG_NOKORI1_5",
			Name = "ツモ",
			Category = Category.Tsumo1Shanten,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.Tsumo1Shanten_6,
			Label = "VOX_GXX_MJG_NOKORI1_6",
			Name = "ツモ",
			Category = Category.Tsumo1Shanten,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.Tsumo1Shanten_7,
			Label = "VOX_GXX_MJG_NOKORI1_G1",
			Name = "ツモ",
			Category = Category.Tsumo1Shanten,
			ObtainType = ObtainType.Gacha,
			GachaItemNo = 9
		},
		new Item
		{
			ItemID = ItemID.Tsumo1Shanten_8,
			Label = "VOX_GXX_MJG_NOKORI1_G2",
			Name = "ツモ",
			Category = Category.Tsumo1Shanten,
			ObtainType = ObtainType.Gacha,
			GachaItemNo = 10
		},
		new Item
		{
			ItemID = ItemID.Tsumo1Shanten_9,
			Label = "VOX_GXX_MJG_NOKORI1_G3",
			Name = "ツモ",
			Category = Category.Tsumo1Shanten,
			ObtainType = ObtainType.Gacha,
			GachaItemNo = 11
		},
		new Item
		{
			ItemID = ItemID.Tsumo1Shanten_10,
			Label = "VOX_GXX_MJG_NOKORI1_G4",
			Name = "ツモ",
			Category = Category.Tsumo1Shanten,
			ObtainType = ObtainType.Gacha,
			GachaItemNo = 12
		},
		new Item
		{
			ItemID = ItemID.Tsumo1Shanten_11,
			Label = "VOX_GXX_MJG_NOKORI1_G5",
			Name = "ツモ",
			Category = Category.Tsumo1Shanten,
			ObtainType = ObtainType.Gacha,
			GachaItemNo = 13
		},
		new Item
		{
			ItemID = ItemID.TsumoUrame_1,
			Label = "VOX_GXX_MJG_PICK2_1",
			Name = "裏目",
			Category = Category.TsumoUrame,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.TsumoUrame_2,
			Label = "VOX_GXX_MJG_PICK2_2",
			Name = "裏目",
			Category = Category.TsumoUrame,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.TsumoUrame_3,
			Label = "VOX_GXX_MJG_PICK2_3",
			Name = "裏目",
			Category = Category.TsumoUrame,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 2
		},
		new Item
		{
			ItemID = ItemID.TsumoUrame_4,
			Label = "VOX_GXX_MJG_PICK2_4",
			Name = "裏目",
			Category = Category.TsumoUrame,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 37
		},
		new Item
		{
			ItemID = ItemID.TsumoUrame_5,
			Label = "VOX_GXX_MJG_PICK2_5",
			Name = "裏目",
			Category = Category.TsumoUrame,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 71
		},
		new Item
		{
			ItemID = ItemID.TsumoUrame_6,
			Label = "VOX_GXX_MJG_PICK2_6",
			Name = "裏目",
			Category = Category.TsumoUrame,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 106
		},
		new Item
		{
			ItemID = ItemID.KyokuStartFirst_1,
			Label = "VOX_GXX_MJG_START_TONG_1",
			Name = "意気込み",
			Category = Category.KyokuStartFirst,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.KyokuStartFirst_2,
			Label = "VOX_GXX_MJG_START_TONG_2",
			Name = "意気込み",
			Category = Category.KyokuStartFirst,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 16
		},
		new Item
		{
			ItemID = ItemID.KyokuStartFirst_3,
			Label = "VOX_GXX_MJG_START_TONG_3",
			Name = "意気込み",
			Category = Category.KyokuStartFirst,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 85
		},
		new Item
		{
			ItemID = ItemID.KyokuStartRenchan_1,
			Label = "VOX_GXX_MJG_START_RENCHAN1_1",
			Name = "局開始時",
			Category = Category.KyokuStartRenchan,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.KyokuStartRenchan_2,
			Label = "VOX_GXX_MJG_START_RENCHAN1_2",
			Name = "局開始時",
			Category = Category.KyokuStartRenchan,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 18
		},
		new Item
		{
			ItemID = ItemID.KyokuStartRenchan_3,
			Label = "VOX_GXX_MJG_START_RENCHAN1_3",
			Name = "局開始時",
			Category = Category.KyokuStartRenchan,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 87
		},
		new Item
		{
			ItemID = ItemID.KyokuStartRenchanOthers_1,
			Label = "VOX_GXX_MJG_START_RENCHAN2_1",
			Name = "局開始時",
			Category = Category.KyokuStartRenchanOthers,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.KyokuStartRenchanOthers_2,
			Label = "VOX_GXX_MJG_START_RENCHAN2_2",
			Name = "局開始時",
			Category = Category.KyokuStartRenchanOthers,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 19
		},
		new Item
		{
			ItemID = ItemID.KyokuStartRenchanOthers_3,
			Label = "VOX_GXX_MJG_START_RENCHAN2_3",
			Name = "局開始時",
			Category = Category.KyokuStartRenchanOthers,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 88
		},
		new Item
		{
			ItemID = ItemID.KyokuStartAgari_1,
			Label = "VOX_GXX_MJG_START_AGARI1_1",
			Name = "局開始時",
			Category = Category.KyokuStartAgari,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.KyokuStartAgari_2,
			Label = "VOX_GXX_MJG_START_AGARI1_2",
			Name = "局開始時",
			Category = Category.KyokuStartAgari,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 21
		},
		new Item
		{
			ItemID = ItemID.KyokuStartAgari_3,
			Label = "VOX_GXX_MJG_START_AGARI1_3",
			Name = "局開始時",
			Category = Category.KyokuStartAgari,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 89
		},
		new Item
		{
			ItemID = ItemID.KyokuStartTsumo_1,
			Label = "VOX_GXX_MJG_START_AGARI2_1",
			Name = "局開始時",
			Category = Category.KyokuStartTsumo,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.KyokuStartTsumo_2,
			Label = "VOX_GXX_MJG_START_AGARI2_2",
			Name = "局開始時",
			Category = Category.KyokuStartTsumo,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 28
		},
		new Item
		{
			ItemID = ItemID.KyokuStartTsumo_3,
			Label = "VOX_GXX_MJG_START_AGARI2_3",
			Name = "局開始時",
			Category = Category.KyokuStartTsumo,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 97
		},
		new Item
		{
			ItemID = ItemID.KyokuStartHoju1_1,
			Label = "VOX_GXX_MJG_START_FURI1_1",
			Name = "局開始時",
			Category = Category.KyokuStartHoju1,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.KyokuStartHoju1_2,
			Label = "VOX_GXX_MJG_START_FURI1_2",
			Name = "局開始時",
			Category = Category.KyokuStartHoju1,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 22
		},
		new Item
		{
			ItemID = ItemID.KyokuStartHoju1_3,
			Label = "VOX_GXX_MJG_START_FURI1_3",
			Name = "局開始時",
			Category = Category.KyokuStartHoju1,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 91
		},
		new Item
		{
			ItemID = ItemID.KyokuStartHoju2_1,
			Label = "VOX_GXX_MJG_START_FURI2_1",
			Name = "局開始時",
			Category = Category.KyokuStartHoju2,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.KyokuStartHoju2_2,
			Label = "VOX_GXX_MJG_START_FURI2_2",
			Name = "局開始時",
			Category = Category.KyokuStartHoju2,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 23
		},
		new Item
		{
			ItemID = ItemID.KyokuStartHoju2_3,
			Label = "VOX_GXX_MJG_START_FURI2_3",
			Name = "局開始時",
			Category = Category.KyokuStartHoju2,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 92
		},
		new Item
		{
			ItemID = ItemID.KyokuStartHoju3_1,
			Label = "VOX_GXX_MJG_START_FURI3_1",
			Name = "局開始時",
			Category = Category.KyokuStartHoju3,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.KyokuStartHoju3_2,
			Label = "VOX_GXX_MJG_START_FURI3_2",
			Name = "局開始時",
			Category = Category.KyokuStartHoju3,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 27
		},
		new Item
		{
			ItemID = ItemID.KyokuStartHoju3_3,
			Label = "VOX_GXX_MJG_START_FURI3_3",
			Name = "局開始時",
			Category = Category.KyokuStartHoju3,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 96
		},
		new Item
		{
			ItemID = ItemID.KyokuStartTenpai_1,
			Label = "VOX_GXX_MJG_START_TENPAI_1",
			Name = "局開始時",
			Category = Category.KyokuStartTenpai,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.KyokuStartTenpai_2,
			Label = "VOX_GXX_MJG_START_TENPAI_2",
			Name = "局開始時",
			Category = Category.KyokuStartTenpai,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 24
		},
		new Item
		{
			ItemID = ItemID.KyokuStartTenpai_3,
			Label = "VOX_GXX_MJG_START_TENPAI_3",
			Name = "局開始時",
			Category = Category.KyokuStartTenpai,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 93
		},
		new Item
		{
			ItemID = ItemID.KyokuStartNoten_1,
			Label = "VOX_GXX_MJG_START_NOTEN_1",
			Name = "局開始時",
			Category = Category.KyokuStartNoten,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.KyokuStartNoten_2,
			Label = "VOX_GXX_MJG_START_NOTEN_2",
			Name = "局開始時",
			Category = Category.KyokuStartNoten,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 25
		},
		new Item
		{
			ItemID = ItemID.KyokuStartNoten_3,
			Label = "VOX_GXX_MJG_START_NOTEN_3",
			Name = "局開始時",
			Category = Category.KyokuStartNoten,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 94
		},
		new Item
		{
			ItemID = ItemID.KyokuStartNoScore_1,
			Label = "VOX_GXX_MJG_START_NOSCORE_1",
			Name = "局開始時",
			Category = Category.KyokuStartNoScore,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.KyokuStartNoScore_2,
			Label = "VOX_GXX_MJG_START_NOSCORE_2",
			Name = "局開始時",
			Category = Category.KyokuStartNoScore,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 26
		},
		new Item
		{
			ItemID = ItemID.KyokuStartNoScore_3,
			Label = "VOX_GXX_MJG_START_NOSCORE_3",
			Name = "局開始時",
			Category = Category.KyokuStartNoScore,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 95
		},
		new Item
		{
			ItemID = ItemID.KyokuStartAllLast1_1,
			Label = "VOX_GXX_MJG_START_RANK1_1",
			Name = "オーラス",
			Category = Category.KyokuStartAllLast1,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.KyokuStartAllLast1_2,
			Label = "VOX_GXX_MJG_START_RANK1_2",
			Name = "オーラス",
			Category = Category.KyokuStartAllLast1,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 29
		},
		new Item
		{
			ItemID = ItemID.KyokuStartAllLast1_3,
			Label = "VOX_GXX_MJG_START_RANK1_3",
			Name = "オーラス",
			Category = Category.KyokuStartAllLast1,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 98
		},
		new Item
		{
			ItemID = ItemID.KyokuStartAllLast2_1,
			Label = "VOX_GXX_MJG_START_RANK2_1",
			Name = "オーラス",
			Category = Category.KyokuStartAllLast2,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.KyokuStartAllLast2_2,
			Label = "VOX_GXX_MJG_START_RANK2_2",
			Name = "オーラス",
			Category = Category.KyokuStartAllLast2,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 31
		},
		new Item
		{
			ItemID = ItemID.KyokuStartAllLast2_3,
			Label = "VOX_GXX_MJG_START_RANK2_3",
			Name = "オーラス",
			Category = Category.KyokuStartAllLast2,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 99
		},
		new Item
		{
			ItemID = ItemID.KyokuStartAllLast3_1,
			Label = "VOX_GXX_MJG_START_RANK3_1",
			Name = "オーラス",
			Category = Category.KyokuStartAllLast3,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.KyokuStartAllLast3_2,
			Label = "VOX_GXX_MJG_START_RANK3_2",
			Name = "オーラス",
			Category = Category.KyokuStartAllLast3,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 32
		},
		new Item
		{
			ItemID = ItemID.KyokuStartAllLast3_3,
			Label = "VOX_GXX_MJG_START_RANK3_3",
			Name = "オーラス",
			Category = Category.KyokuStartAllLast3,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 101
		},
		new Item
		{
			ItemID = ItemID.KyokuStartAllLast4_1,
			Label = "VOX_GXX_MJG_START_RANK4_1",
			Name = "オーラス",
			Category = Category.KyokuStartAllLast4,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.KyokuStartAllLast4_2,
			Label = "VOX_GXX_MJG_START_RANK4_2",
			Name = "オーラス",
			Category = Category.KyokuStartAllLast4,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 33
		},
		new Item
		{
			ItemID = ItemID.KyokuStartAllLast4_3,
			Label = "VOX_GXX_MJG_START_RANK4_3",
			Name = "オーラス",
			Category = Category.KyokuStartAllLast4,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 102
		},
		new Item
		{
			ItemID = ItemID.Matching_1,
			Label = "VOX_GXX_APPEAL2_1",
			Name = "オーラス",
			Category = Category.Matching,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.Matching_2,
			Label = "VOX_GXX_APPEAL2_2",
			Name = "オーラス",
			Category = Category.Matching,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 17
		},
		new Item
		{
			ItemID = ItemID.Matching_3,
			Label = "VOX_GXX_APPEAL2_3",
			Name = "オーラス",
			Category = Category.Matching,
			ObtainType = ObtainType.Favo,
			NeedUnlockFavo = 86
		},
		new Item
		{
			ItemID = ItemID.ResultScorePanel1_1,
			Label = "VOX_GXX_RNK_RESULT1",
			Name = "結果発表1位",
			Category = Category.ResultScorePanel1,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.ResultScorePanel1_2,
			Label = "VOX_GXX_RNK_RESULT1_G1",
			Name = "結果発表1位",
			Category = Category.ResultScorePanel1,
			ObtainType = ObtainType.Gacha,
			GachaItemNo = 37
		},
		new Item
		{
			ItemID = ItemID.ResultScorePanel2_1,
			Label = "VOX_GXX_RNK_RESULT2",
			Name = "結果発表2位",
			Category = Category.ResultScorePanel2,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.ResultScorePanel2_2,
			Label = "VOX_GXX_RNK_RESULT2_G1",
			Name = "結果発表2位",
			Category = Category.ResultScorePanel2,
			ObtainType = ObtainType.Gacha,
			GachaItemNo = 38
		},
		new Item
		{
			ItemID = ItemID.ResultScorePanel3_1,
			Label = "VOX_GXX_RNK_RESULT3",
			Name = "結果発表3位",
			Category = Category.ResultScorePanel3,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.ResultScorePanel3_2,
			Label = "VOX_GXX_RNK_RESULT3_G1",
			Name = "結果発表3位",
			Category = Category.ResultScorePanel3,
			ObtainType = ObtainType.Gacha,
			GachaItemNo = 39
		},
		new Item
		{
			ItemID = ItemID.ResultScorePanel4_1,
			Label = "VOX_GXX_RNK_RESULT4",
			Name = "結果発表最下位",
			Category = Category.ResultScorePanel4,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.ResultScorePanel4_2,
			Label = "VOX_GXX_RNK_RESULT4_G1",
			Name = "結果発表最下位",
			Category = Category.ResultScorePanel4,
			ObtainType = ObtainType.Gacha,
			GachaItemNo = 40
		},
		new Item
		{
			ItemID = ItemID.ResultScorePanelWin_1,
			Label = "VOX_GXX_RNK_WIN1",
			Name = "結果発表2人打ち勝利",
			Category = Category.ResultScorePanelWin,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.ResultScorePanelLose_1,
			Label = "VOX_GXX_RNK_LOSE1",
			Name = "結果発表2人打ち敗北",
			Category = Category.ResultScorePanelLose,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.DahaiIppatsuMissed_1,
			Label = "VOX_GXX_MJG_DAHAI10_1",
			Name = "一発ならず",
			Category = Category.DahaiIppatsuMissed,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.DahaiIppatsuMissed_2,
			Label = "VOX_GXX_MJG_DAHAI10_2",
			Name = "一発ならず",
			Category = Category.DahaiIppatsuMissed,
			ObtainType = ObtainType.Default
		},
		new Item
		{
			ItemID = ItemID.DahaiIppatsuMissed_3,
			Label = "VOX_GXX_MJG_DAHAI10_3",
			Name = "一発ならず",
			Category = Category.DahaiIppatsuMissed,
			ObtainType = ObtainType.Default
		}
	};

	public static Item GetItem(ItemID itemID)
	{
		return m_items.Find((Item item) => item.ItemID == itemID);
	}

	public static Item GetItem(string oid)
	{
		return m_items.Find((Item a) => a.ObjectID == ToMasterOID(oid));
	}

	public static string ToMasterOID(string oid)
	{
		if (oid.Contains("OID_Voice_Chara"))
		{
			oid = oid.Remove(9, 8);
		}
		return oid;
	}

	public static ItemID ToItemID(string oid)
	{
		try
		{
			return Enum.Parse<ItemID>(ToMasterOID(oid).Remove(0, 10));
		}
		catch
		{
			return ItemID.None;
		}
	}

	public static CharaType GetCharaType(string oid)
	{
		if (oid.Contains("_Chara"))
		{
			return Enum.Parse<CharaType>(oid.Substring(10, 7));
		}
		return (CharaType)(-1);
	}

	public static string ToObjectID(ItemID itemID)
	{
		return $"OID_Voice_{itemID}";
	}

	public static string ToObjectID(CharaType chara, ItemID itemID)
	{
		return $"OID_Voice_{chara}_{itemID}";
	}

	public static List<Item> GetItems(Predicate<Item> match)
	{
		return m_items.FindAll(match);
	}
}
