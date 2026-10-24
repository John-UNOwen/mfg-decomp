using System.Collections.Generic;
using System.Linq;
using MFG.Types;
using MFG.Types.Favo;

namespace MFG.MasterGameData;

public class FavoMaster : MasterGameDataBase
{
	private static List<FavoMasterData> m_favoMastarTable = new List<FavoMasterData>
	{
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_002",
			ID = FavoItemID.Favo01_002,
			FavoNum = 2,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_PICK2_3",
			_unlockVoiceName = "ボイス 「裏目」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_003",
			ID = FavoItemID.Favo01_003,
			FavoNum = 3,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_DAHAI1_3",
			_unlockVoiceName = "ボイス 「打牌」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_004",
			ID = FavoItemID.Favo01_004,
			FavoNum = 4,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_DAHAI2_3",
			_unlockVoiceName = "ボイス 「安全牌」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_005",
			ID = FavoItemID.Favo01_005,
			FavoNum = 5,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_DAHAI3_3",
			_unlockVoiceName = "ボイス 「危険牌」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_006",
			ID = FavoItemID.Favo01_006,
			FavoNum = 6,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_DAHAI4_3",
			_unlockVoiceName = "ボイス 「ツモ切り」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_007",
			ID = FavoItemID.Favo01_007,
			FavoNum = 7,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_THINK_3",
			_unlockVoiceName = "ボイス 「長考」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_008",
			ID = FavoItemID.Favo01_008,
			FavoNum = 8,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_COMMENT1_3",
			_unlockVoiceName = "ボイス 「鳴き」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_009",
			ID = FavoItemID.Favo01_009,
			FavoNum = 9,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_COMMENT2_3",
			_unlockVoiceName = "ボイス 「鳴き(他家)」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_010",
			ID = FavoItemID.Favo01_010,
			FavoNum = 10,
			type = FavoItemType.Story,
			unlockVoiceID = "",
			_unlockVoiceName = "[キャラの下の名前] コミュニケーションシナリオ １"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_011",
			ID = FavoItemID.Favo01_011,
			FavoNum = 11,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_COMMENT3_3",
			_unlockVoiceName = "ボイス 「鳴き(他家)」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_012",
			ID = FavoItemID.Favo01_012,
			FavoNum = 12,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_COMMENT5_3",
			_unlockVoiceName = "ボイス 「リーチ後」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_013",
			ID = FavoItemID.Favo01_013,
			FavoNum = 13,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_COMMENT6_3",
			_unlockVoiceName = "ボイス 「リーチ後」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_014",
			ID = FavoItemID.Favo01_014,
			FavoNum = 14,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_COMMENT7_3",
			_unlockVoiceName = "ボイス 「リーチ後(他家)」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_015",
			ID = FavoItemID.Favo01_015,
			FavoNum = 15,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_AGARI_SR_3",
			_unlockVoiceName = "ボイス 「ガールズカットイン」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_016",
			ID = FavoItemID.Favo01_016,
			FavoNum = 16,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_START_TONG_2",
			_unlockVoiceName = "ボイス 「意気込み」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_017",
			ID = FavoItemID.Favo01_017,
			FavoNum = 17,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_APPEAL2_2",
			_unlockVoiceName = "ボイス 「マッチング」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_018",
			ID = FavoItemID.Favo01_018,
			FavoNum = 18,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_START_RENCHAN1_2",
			_unlockVoiceName = "ボイス 「連荘」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_019",
			ID = FavoItemID.Favo01_019,
			FavoNum = 19,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_START_RENCHAN2_2",
			_unlockVoiceName = "ボイス 「連荘(他家)」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_020",
			ID = FavoItemID.Favo01_020,
			FavoNum = 20,
			type = FavoItemType.Bgm,
			unlockVoiceID = "",
			_unlockVoiceName = "[曲名] / オフボーカル （ショート）"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_021",
			ID = FavoItemID.Favo01_021,
			FavoNum = 21,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_START_AGARI1_2",
			_unlockVoiceName = "ボイス 「局開始時」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_022",
			ID = FavoItemID.Favo01_022,
			FavoNum = 22,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_START_FURI1_2",
			_unlockVoiceName = "ボイス 「局開始時」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_023",
			ID = FavoItemID.Favo01_023,
			FavoNum = 23,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_START_FURI2_2",
			_unlockVoiceName = "ボイス 「局開始時」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_024",
			ID = FavoItemID.Favo01_024,
			FavoNum = 24,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_START_TENPAI_2",
			_unlockVoiceName = "ボイス 「局開始時」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_025",
			ID = FavoItemID.Favo01_025,
			FavoNum = 25,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_START_NOTEN_2",
			_unlockVoiceName = "ボイス 「局開始時」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_026",
			ID = FavoItemID.Favo01_026,
			FavoNum = 26,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_START_NOSCORE_2",
			_unlockVoiceName = "ボイス 「局開始時」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_027",
			ID = FavoItemID.Favo01_027,
			FavoNum = 27,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_START_FURI3_2",
			_unlockVoiceName = "ボイス 「局開始時」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_028",
			ID = FavoItemID.Favo01_028,
			FavoNum = 28,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_START_AGARI2_2",
			_unlockVoiceName = "ボイス 「局開始時」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_029",
			ID = FavoItemID.Favo01_029,
			FavoNum = 29,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_START_RANK1_2",
			_unlockVoiceName = "ボイス 「オーラス」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_030",
			ID = FavoItemID.Favo01_030,
			FavoNum = 30,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_TALK_3",
			_unlockVoiceName = "ボイス 「雑談」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_031",
			ID = FavoItemID.Favo01_031,
			FavoNum = 31,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_START_RANK2_2",
			_unlockVoiceName = "ボイス 「オーラス」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_032",
			ID = FavoItemID.Favo01_032,
			FavoNum = 32,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_START_RANK3_2",
			_unlockVoiceName = "ボイス 「オーラス」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_033",
			ID = FavoItemID.Favo01_033,
			FavoNum = 33,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_START_RANK4_2",
			_unlockVoiceName = "ボイス 「オーラス」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_034",
			ID = FavoItemID.Favo01_034,
			FavoNum = 34,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_DAHAI7_2",
			_unlockVoiceName = "ボイス 「ドラ切り」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_035",
			ID = FavoItemID.Favo01_035,
			FavoNum = 35,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_DAHAI5_2",
			_unlockVoiceName = "ボイス 「ツモ切り」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_036",
			ID = FavoItemID.Favo01_036,
			FavoNum = 36,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_PICK1_4",
			_unlockVoiceName = "ボイス 「ツモ」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_037",
			ID = FavoItemID.Favo01_037,
			FavoNum = 37,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_PICK2_4",
			_unlockVoiceName = "ボイス 「裏目」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_038",
			ID = FavoItemID.Favo01_038,
			FavoNum = 38,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_DAHAI1_4",
			_unlockVoiceName = "ボイス 「打牌」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_039",
			ID = FavoItemID.Favo01_039,
			FavoNum = 39,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_DAHAI2_4",
			_unlockVoiceName = "ボイス 「安全牌」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_040",
			ID = FavoItemID.Favo01_040,
			FavoNum = 40,
			type = FavoItemType.Story,
			unlockVoiceID = "",
			_unlockVoiceName = "[キャラの下の名前] コミュニケーションシナリオ ２"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_041",
			ID = FavoItemID.Favo01_041,
			FavoNum = 41,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_DAHAI3_4",
			_unlockVoiceName = "ボイス 「危険牌」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_042",
			ID = FavoItemID.Favo01_042,
			FavoNum = 42,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_DAHAI4_4",
			_unlockVoiceName = "ボイス 「ツモ切り」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_043",
			ID = FavoItemID.Favo01_043,
			FavoNum = 43,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_THINK_4",
			_unlockVoiceName = "ボイス 「長考」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_044",
			ID = FavoItemID.Favo01_044,
			FavoNum = 44,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_COMMENT1_4",
			_unlockVoiceName = "ボイス 「鳴き」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_045",
			ID = FavoItemID.Favo01_045,
			FavoNum = 45,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_COMMENT2_4",
			_unlockVoiceName = "ボイス 「鳴き(他家)」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_046",
			ID = FavoItemID.Favo01_046,
			FavoNum = 46,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_COMMENT3_4",
			_unlockVoiceName = "ボイス 「鳴き(他家)」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_047",
			ID = FavoItemID.Favo01_047,
			FavoNum = 47,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_COMMENT5_4",
			_unlockVoiceName = "ボイス 「リーチ後」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_048",
			ID = FavoItemID.Favo01_048,
			FavoNum = 48,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_COMMENT6_4",
			_unlockVoiceName = "ボイス 「リーチ後」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_049",
			ID = FavoItemID.Favo01_049,
			FavoNum = 49,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_COMMENT7_4",
			_unlockVoiceName = "ボイス 「リーチ後(他家)」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_050",
			ID = FavoItemID.Favo01_050,
			FavoNum = 50,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_TALK_4",
			_unlockVoiceName = "ボイス 「雑談」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_051",
			ID = FavoItemID.Favo01_051,
			FavoNum = 51,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_AGARI_SR_4",
			_unlockVoiceName = "ボイス 「ガールズカットイン」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_052",
			ID = FavoItemID.Favo01_052,
			FavoNum = 52,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_DAHAI6_2",
			_unlockVoiceName = "ボイス 「ツモ切り」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_053",
			ID = FavoItemID.Favo01_053,
			FavoNum = 53,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_DAHAI8_2",
			_unlockVoiceName = "ボイス 「危険牌」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_054",
			ID = FavoItemID.Favo01_054,
			FavoNum = 54,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_COMMENT4_2",
			_unlockVoiceName = "ボイス 「鳴き(他家)」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_055",
			ID = FavoItemID.Favo01_055,
			FavoNum = 55,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_AGARI_N_2",
			_unlockVoiceName = "ボイス 「ガールズカットイン」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_056",
			ID = FavoItemID.Favo01_056,
			FavoNum = 56,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_SCORE1_2",
			_unlockVoiceName = "ボイス 「流局」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_057",
			ID = FavoItemID.Favo01_057,
			FavoNum = 57,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_SCORE2_2",
			_unlockVoiceName = "ボイス 「アガリ」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_058",
			ID = FavoItemID.Favo01_058,
			FavoNum = 58,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_SCORE3_2",
			_unlockVoiceName = "ボイス 「アガリ」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_059",
			ID = FavoItemID.Favo01_059,
			FavoNum = 59,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_SCORE4_2",
			_unlockVoiceName = "ボイス 「アガリ」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_060",
			ID = FavoItemID.Favo01_060,
			FavoNum = 60,
			type = FavoItemType.Bgm,
			unlockVoiceID = "",
			_unlockVoiceName = "[曲名] / オフボーカル （フル）"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_061",
			ID = FavoItemID.Favo01_061,
			FavoNum = 61,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_SCORE5_2",
			_unlockVoiceName = "ボイス 「アガリ」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_062",
			ID = FavoItemID.Favo01_062,
			FavoNum = 62,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_SCORE6_2",
			_unlockVoiceName = "ボイス 「流局」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_063",
			ID = FavoItemID.Favo01_063,
			FavoNum = 63,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_SCORE7_2",
			_unlockVoiceName = "ボイス 「支払い」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_064",
			ID = FavoItemID.Favo01_064,
			FavoNum = 64,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_SCORE8_2",
			_unlockVoiceName = "ボイス 「支払い」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_065",
			ID = FavoItemID.Favo01_065,
			FavoNum = 65,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_SCORE9_2",
			_unlockVoiceName = "ボイス 「支払い」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_066",
			ID = FavoItemID.Favo01_066,
			FavoNum = 66,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_SCORE10_2",
			_unlockVoiceName = "ボイス 「支払い」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_067",
			ID = FavoItemID.Favo01_067,
			FavoNum = 67,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_SCORE11_2",
			_unlockVoiceName = "ボイス 「支払い」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_068",
			ID = FavoItemID.Favo01_068,
			FavoNum = 68,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_SCORE12_2",
			_unlockVoiceName = "ボイス 「流局」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_069",
			ID = FavoItemID.Favo01_069,
			FavoNum = 69,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_PICK1_5",
			_unlockVoiceName = "ボイス 「ツモ」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_070",
			ID = FavoItemID.Favo01_070,
			FavoNum = 70,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_TALK_5",
			_unlockVoiceName = "ボイス 「雑談」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_071",
			ID = FavoItemID.Favo01_071,
			FavoNum = 71,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_PICK2_5",
			_unlockVoiceName = "ボイス 「裏目」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_072",
			ID = FavoItemID.Favo01_072,
			FavoNum = 72,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_DAHAI1_5",
			_unlockVoiceName = "ボイス 「打牌」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_073",
			ID = FavoItemID.Favo01_073,
			FavoNum = 73,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_DAHAI2_5",
			_unlockVoiceName = "ボイス 「安全牌」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_074",
			ID = FavoItemID.Favo01_074,
			FavoNum = 74,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_DAHAI3_5",
			_unlockVoiceName = "ボイス 「危険牌」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_075",
			ID = FavoItemID.Favo01_075,
			FavoNum = 75,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_DAHAI4_5",
			_unlockVoiceName = "ボイス 「ツモ切り」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_076",
			ID = FavoItemID.Favo01_076,
			FavoNum = 76,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_THINK_5",
			_unlockVoiceName = "ボイス 「長考」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_077",
			ID = FavoItemID.Favo01_077,
			FavoNum = 77,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_COMMENT1_5",
			_unlockVoiceName = "ボイス 「鳴き」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_078",
			ID = FavoItemID.Favo01_078,
			FavoNum = 78,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_COMMENT2_5",
			_unlockVoiceName = "ボイス 「鳴き(他家)」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_079",
			ID = FavoItemID.Favo01_079,
			FavoNum = 79,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_COMMENT3_5",
			_unlockVoiceName = "ボイス 「鳴き(他家)」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_080",
			ID = FavoItemID.Favo01_080,
			FavoNum = 80,
			type = FavoItemType.Story,
			unlockVoiceID = "",
			_unlockVoiceName = "[キャラの下の名前] コミュニケーションシナリオ ３"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_081",
			ID = FavoItemID.Favo01_081,
			FavoNum = 81,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_COMMENT5_5",
			_unlockVoiceName = "ボイス 「リーチ後」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_082",
			ID = FavoItemID.Favo01_082,
			FavoNum = 82,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_COMMENT6_5",
			_unlockVoiceName = "ボイス 「リーチ後」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_083",
			ID = FavoItemID.Favo01_083,
			FavoNum = 83,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_COMMENT7_5",
			_unlockVoiceName = "ボイス 「リーチ後(他家)」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_084",
			ID = FavoItemID.Favo01_084,
			FavoNum = 84,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_AGARI_SR_5",
			_unlockVoiceName = "ボイス 「ガールズカットイン」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_085",
			ID = FavoItemID.Favo01_085,
			FavoNum = 85,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_START_TONG_3",
			_unlockVoiceName = "ボイス 「意気込み」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_086",
			ID = FavoItemID.Favo01_086,
			FavoNum = 86,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_APPEAL2_3",
			_unlockVoiceName = "ボイス 「マッチング」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_087",
			ID = FavoItemID.Favo01_087,
			FavoNum = 87,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_START_RENCHAN1_3",
			_unlockVoiceName = "ボイス 「連荘」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_088",
			ID = FavoItemID.Favo01_088,
			FavoNum = 88,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_START_RENCHAN2_3",
			_unlockVoiceName = "ボイス 「連荘(他家)」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_089",
			ID = FavoItemID.Favo01_089,
			FavoNum = 89,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_START_AGARI1_3",
			_unlockVoiceName = "ボイス 「局開始時」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_090",
			ID = FavoItemID.Favo01_090,
			FavoNum = 90,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_TALK_6",
			_unlockVoiceName = "ボイス 「雑談」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_091",
			ID = FavoItemID.Favo01_091,
			FavoNum = 91,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_START_FURI1_3",
			_unlockVoiceName = "ボイス 「局開始時」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_092",
			ID = FavoItemID.Favo01_092,
			FavoNum = 92,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_START_FURI2_3",
			_unlockVoiceName = "ボイス 「局開始時」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_093",
			ID = FavoItemID.Favo01_093,
			FavoNum = 93,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_START_TENPAI_3",
			_unlockVoiceName = "ボイス 「局開始時」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_094",
			ID = FavoItemID.Favo01_094,
			FavoNum = 94,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_START_NOTEN_3",
			_unlockVoiceName = "ボイス 「局開始時」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_095",
			ID = FavoItemID.Favo01_095,
			FavoNum = 95,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_START_NOSCORE_3",
			_unlockVoiceName = "ボイス 「局開始時」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_096",
			ID = FavoItemID.Favo01_096,
			FavoNum = 96,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_START_FURI3_3",
			_unlockVoiceName = "ボイス 「局開始時」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_097",
			ID = FavoItemID.Favo01_097,
			FavoNum = 97,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_START_AGARI2_3",
			_unlockVoiceName = "ボイス 「局開始時」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_098",
			ID = FavoItemID.Favo01_098,
			FavoNum = 98,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_START_RANK1_3",
			_unlockVoiceName = "ボイス 「オーラス」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_099",
			ID = FavoItemID.Favo01_099,
			FavoNum = 99,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_START_RANK2_3",
			_unlockVoiceName = "ボイス 「オーラス」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_100",
			ID = FavoItemID.Favo01_100,
			FavoNum = 100,
			type = FavoItemType.Bgm,
			unlockVoiceID = "",
			_unlockVoiceName = "[曲名] / オンボーカル （ショート）"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_101",
			ID = FavoItemID.Favo01_101,
			FavoNum = 101,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_START_RANK3_3",
			_unlockVoiceName = "ボイス 「オーラス」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_102",
			ID = FavoItemID.Favo01_102,
			FavoNum = 102,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_START_RANK4_3",
			_unlockVoiceName = "ボイス 「オーラス」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_103",
			ID = FavoItemID.Favo01_103,
			FavoNum = 103,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_DAHAI7_3",
			_unlockVoiceName = "ボイス 「ドラ切り」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_104",
			ID = FavoItemID.Favo01_104,
			FavoNum = 104,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_DAHAI5_3",
			_unlockVoiceName = "ボイス 「ツモ切り」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_105",
			ID = FavoItemID.Favo01_105,
			FavoNum = 105,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_PICK1_6",
			_unlockVoiceName = "ボイス 「ツモ」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_106",
			ID = FavoItemID.Favo01_106,
			FavoNum = 106,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_PICK2_6",
			_unlockVoiceName = "ボイス 「裏目」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_107",
			ID = FavoItemID.Favo01_107,
			FavoNum = 107,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_DAHAI1_6",
			_unlockVoiceName = "ボイス 「打牌」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_108",
			ID = FavoItemID.Favo01_108,
			FavoNum = 108,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_DAHAI2_6",
			_unlockVoiceName = "ボイス 「安全牌」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_109",
			ID = FavoItemID.Favo01_109,
			FavoNum = 109,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_DAHAI3_6",
			_unlockVoiceName = "ボイス 「危険牌」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_110",
			ID = FavoItemID.Favo01_110,
			FavoNum = 110,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_TALK_7",
			_unlockVoiceName = "ボイス 「雑談」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_111",
			ID = FavoItemID.Favo01_111,
			FavoNum = 111,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_DAHAI4_6",
			_unlockVoiceName = "ボイス 「ツモ切り」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_112",
			ID = FavoItemID.Favo01_112,
			FavoNum = 112,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_THINK_6",
			_unlockVoiceName = "ボイス 「長考」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_113",
			ID = FavoItemID.Favo01_113,
			FavoNum = 113,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_COMMENT1_6",
			_unlockVoiceName = "ボイス 「鳴き」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_114",
			ID = FavoItemID.Favo01_114,
			FavoNum = 114,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_COMMENT2_6",
			_unlockVoiceName = "ボイス 「鳴き(他家)」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_115",
			ID = FavoItemID.Favo01_115,
			FavoNum = 115,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_COMMENT3_6",
			_unlockVoiceName = "ボイス 「鳴き(他家)」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_116",
			ID = FavoItemID.Favo01_116,
			FavoNum = 116,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_COMMENT5_6",
			_unlockVoiceName = "ボイス 「リーチ後」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_117",
			ID = FavoItemID.Favo01_117,
			FavoNum = 117,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_COMMENT6_6",
			_unlockVoiceName = "ボイス 「リーチ後」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_118",
			ID = FavoItemID.Favo01_118,
			FavoNum = 118,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_COMMENT7_6",
			_unlockVoiceName = "ボイス 「リーチ後(他家)」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_119",
			ID = FavoItemID.Favo01_119,
			FavoNum = 119,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_AGARI_SR_6",
			_unlockVoiceName = "ボイス 「ガールズカットイン」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_120",
			ID = FavoItemID.Favo01_120,
			FavoNum = 120,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_TALK_8",
			_unlockVoiceName = "ボイス 「雑談」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_121",
			ID = FavoItemID.Favo01_121,
			FavoNum = 121,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_DAHAI6_3",
			_unlockVoiceName = "ボイス 「ツモ切り」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_122",
			ID = FavoItemID.Favo01_122,
			FavoNum = 122,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_DAHAI8_3",
			_unlockVoiceName = "ボイス 「危険牌」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_123",
			ID = FavoItemID.Favo01_123,
			FavoNum = 123,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_COMMENT4_3",
			_unlockVoiceName = "ボイス 「鳴き(他家)」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_124",
			ID = FavoItemID.Favo01_124,
			FavoNum = 124,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_AGARI_N_3",
			_unlockVoiceName = "ボイス 「ガールズカットイン」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_125",
			ID = FavoItemID.Favo01_125,
			FavoNum = 125,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_SCORE1_3",
			_unlockVoiceName = "ボイス 「流局」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_126",
			ID = FavoItemID.Favo01_126,
			FavoNum = 126,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_SCORE2_3",
			_unlockVoiceName = "ボイス 「アガリ」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_127",
			ID = FavoItemID.Favo01_127,
			FavoNum = 127,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_SCORE3_3",
			_unlockVoiceName = "ボイス 「アガリ」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_128",
			ID = FavoItemID.Favo01_128,
			FavoNum = 128,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_SCORE4_3",
			_unlockVoiceName = "ボイス 「アガリ」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_129",
			ID = FavoItemID.Favo01_129,
			FavoNum = 129,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_SCORE5_3",
			_unlockVoiceName = "ボイス 「アガリ」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_130",
			ID = FavoItemID.Favo01_130,
			FavoNum = 130,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_TALK_9",
			_unlockVoiceName = "ボイス 「雑談」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_131",
			ID = FavoItemID.Favo01_131,
			FavoNum = 131,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_SCORE6_3",
			_unlockVoiceName = "ボイス 「流局」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_132",
			ID = FavoItemID.Favo01_132,
			FavoNum = 132,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_SCORE7_3",
			_unlockVoiceName = "ボイス 「支払い」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_133",
			ID = FavoItemID.Favo01_133,
			FavoNum = 133,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_SCORE8_3",
			_unlockVoiceName = "ボイス 「支払い」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_134",
			ID = FavoItemID.Favo01_134,
			FavoNum = 134,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_SCORE9_3",
			_unlockVoiceName = "ボイス 「支払い」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_135",
			ID = FavoItemID.Favo01_135,
			FavoNum = 135,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_SCORE10_3",
			_unlockVoiceName = "ボイス 「支払い」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_136",
			ID = FavoItemID.Favo01_136,
			FavoNum = 136,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_SCORE11_3",
			_unlockVoiceName = "ボイス 「支払い」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_137",
			ID = FavoItemID.Favo01_137,
			FavoNum = 137,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_SCORE12_3",
			_unlockVoiceName = "ボイス 「流局」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_138",
			ID = FavoItemID.Favo01_138,
			FavoNum = 138,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_TALK_10",
			_unlockVoiceName = "ボイス 「雑談」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_139",
			ID = FavoItemID.Favo01_139,
			FavoNum = 139,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_TALK_11",
			_unlockVoiceName = "ボイス 「雑談」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_140",
			ID = FavoItemID.Favo01_140,
			FavoNum = 140,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_TALK_12",
			_unlockVoiceName = "ボイス 「雑談」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_141",
			ID = FavoItemID.Favo01_141,
			FavoNum = 145,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_TALK_13",
			_unlockVoiceName = "ボイス 「雑談」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_142",
			ID = FavoItemID.Favo01_142,
			FavoNum = 150,
			type = FavoItemType.Bgm,
			unlockVoiceID = "",
			_unlockVoiceName = "[曲名] / オンボーカル （フル）"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_143",
			ID = FavoItemID.Favo01_143,
			FavoNum = 155,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_TALK_14",
			_unlockVoiceName = "ボイス 「雑談」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_144",
			ID = FavoItemID.Favo01_144,
			FavoNum = 165,
			type = FavoItemType.Voice,
			unlockVoiceID = "VOX_GXX_MJG_TALK_15",
			_unlockVoiceName = "ボイス 「雑談」"
		},
		new FavoMasterData
		{
			ObjectID = "OID_Favo01_145",
			ID = FavoItemID.Favo01_145,
			FavoNum = 200,
			type = FavoItemType.Cutin,
			unlockVoiceID = "",
			_unlockVoiceName = "ガールズカットイン「スペシャルアニメ 2」"
		}
	};

	public static FavoMasterData GetFavoData(FavoItemID itemID)
	{
		return m_favoMastarTable.Find((FavoMasterData a) => a.ID == itemID);
	}

	public static FavoMasterData GetFavoData(string oid)
	{
		return m_favoMastarTable.Find((FavoMasterData a) => a.ObjectID == oid);
	}

	public static FavoItemID ToItemID(string oid)
	{
		return m_favoMastarTable.Find((FavoMasterData _item) => _item.ObjectID == oid)?.ID ?? FavoItemID.Favo01_002;
	}

	public static List<FavoMasterData> GetBgmFavoList()
	{
		return (from a in m_favoMastarTable
			where a.type == FavoItemType.Bgm
			select a into b
			orderby b.FavoNum
			select b).ToList();
	}

	public static Dictionary<int, (string, int)> FavoMissionTable(CharaType chara)
	{
		List<int> list = FavoNumList(chara);
		Dictionary<int, (string, int)> dictionary = new Dictionary<int, (string, int)>();
		foreach (int favoNum in list)
		{
			dictionary.Add(favoNum, (m_favoMastarTable.Find((FavoMasterData x) => x.FavoNum == favoNum).ObjectID, 1));
		}
		return dictionary;
	}

	public static List<FavoMasterData> FavoMasterDatas(CharaType chara)
	{
		if (chara >= CharaType.Chara13)
		{
			return m_favoMastarTable.Where((FavoMasterData x) => x.type != FavoItemType.Cutin).ToList();
		}
		return m_favoMastarTable;
	}

	public static List<int> FavoNumList(CharaType chara)
	{
		return (from x in FavoMasterDatas(chara)
			select x.FavoNum into x
			orderby x
			select x).ToList();
	}
}
