using System;
using System.Collections.Generic;
using MFG.Types;
using MFG.Types.Story;

namespace MFG.MasterGameData;

public class StoryMaster : MasterGameDataBase
{
	private static List<StoryID> _defaultStories = new List<StoryID> { StoryID.StoryEvent0000 };

	private static List<Story> _stories = new List<Story>
	{
		new Story
		{
			ObjectID = "OID_StoryEvent0000",
			StoryID = StoryID.StoryEvent0000,
			StoryType = StoryType.EventStory,
			Part = 0,
			Chapter = 0,
			Name = "プロローグ",
			UnlockingType = UnlockingType.Default,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryEvent0101",
			StoryID = StoryID.StoryEvent0101,
			StoryType = StoryType.EventStory,
			Part = 1,
			Chapter = 1,
			Name = "第1回 ワクワク！役ビンゴ大会",
			UnlockingType = UnlockingType.Event
		},
		new Story
		{
			ObjectID = "OID_StoryEvent0201",
			StoryID = StoryID.StoryEvent0201,
			StoryType = StoryType.EventStory,
			Part = 2,
			Chapter = 1,
			Name = "水着をたのもう！（壱高）",
			UnlockingType = UnlockingType.Event
		},
		new Story
		{
			ObjectID = "OID_StoryEvent0301",
			StoryID = StoryID.StoryEvent0301,
			StoryType = StoryType.EventStory,
			Part = 3,
			Chapter = 1,
			Name = "水着をたのもう！（弐女）",
			UnlockingType = UnlockingType.Event
		},
		new Story
		{
			ObjectID = "OID_StoryEvent0401",
			StoryID = StoryID.StoryEvent0401,
			StoryType = StoryType.EventStory,
			Part = 4,
			Chapter = 1,
			Name = "水着をたのもう！（帝役）",
			UnlockingType = UnlockingType.Event
		},
		new Story
		{
			ObjectID = "OID_StoryEvent0501",
			StoryID = StoryID.StoryEvent0501,
			StoryType = StoryType.EventStory,
			Part = 5,
			Chapter = 1,
			Name = "つよつよテンシの押せ押せ豪腕麻雀",
			UnlockingType = UnlockingType.Event
		},
		new Story
		{
			ObjectID = "OID_StoryEvent0601",
			StoryID = StoryID.StoryEvent0601,
			StoryType = StoryType.EventStory,
			Part = 6,
			Chapter = 1,
			Name = "トイトイのぶっとびギャラクシー",
			UnlockingType = UnlockingType.Event
		},
		new Story
		{
			ObjectID = "OID_StoryEvent0701",
			StoryID = StoryID.StoryEvent0701,
			StoryType = StoryType.EventStory,
			Part = 7,
			Chapter = 1,
			Name = "第2回 ワクワク！役ビンゴ大会",
			UnlockingType = UnlockingType.Event
		},
		new Story
		{
			ObjectID = "OID_StoryEvent0801",
			StoryID = StoryID.StoryEvent0801,
			StoryType = StoryType.EventStory,
			Part = 8,
			Chapter = 1,
			Name = "ハーフアニバありがとう",
			UnlockingType = UnlockingType.Event
		},
		new Story
		{
			ObjectID = "OID_StoryEvent0901",
			StoryID = StoryID.StoryEvent0901,
			StoryType = StoryType.EventStory,
			Part = 9,
			Chapter = 1,
			Name = "気炎万丈 炎のリーチ道場",
			UnlockingType = UnlockingType.Event
		},
		new Story
		{
			ObjectID = "OID_StoryEvent1001",
			StoryID = StoryID.StoryEvent1001,
			StoryType = StoryType.EventStory,
			Part = 10,
			Chapter = 1,
			Name = "カオス登場",
			UnlockingType = UnlockingType.Event
		},
		new Story
		{
			ObjectID = "OID_StoryEvent1101",
			StoryID = StoryID.StoryEvent1101,
			StoryType = StoryType.EventStory,
			Part = 11,
			Chapter = 1,
			Name = "トイトイのぶっとびギャラクシーDX",
			UnlockingType = UnlockingType.Event
		},
		new Story
		{
			ObjectID = "OID_StoryEvent1201",
			StoryID = StoryID.StoryEvent1201,
			StoryType = StoryType.EventStory,
			Part = 12,
			Chapter = 1,
			Name = "イッシキ・クリアイベントストーリー1",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent1202",
			StoryID = StoryID.StoryEvent1202,
			StoryType = StoryType.EventStory,
			Part = 12,
			Chapter = 2,
			Name = "イッシキ・クリアイベントストーリー2",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent1301",
			StoryID = StoryID.StoryEvent1301,
			StoryType = StoryType.EventStory,
			Part = 13,
			Chapter = 1,
			Name = "冥き終焉の暗黒ドラ卓",
			UnlockingType = UnlockingType.Event
		},
		new Story
		{
			ObjectID = "OID_StoryEvent1401",
			StoryID = StoryID.StoryEvent1401,
			StoryType = StoryType.EventStory,
			Part = 14,
			Chapter = 1,
			Name = "イッシキ・クリアイベントストーリー3",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent1402",
			StoryID = StoryID.StoryEvent1402,
			StoryType = StoryType.EventStory,
			Part = 14,
			Chapter = 2,
			Name = "イッシキ・クリアイベントストーリー4",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent1501",
			StoryID = StoryID.StoryEvent1501,
			StoryType = StoryType.EventStory,
			Part = 15,
			Chapter = 1,
			Name = "イッシキ・クリアイベントストーリー5",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent1502",
			StoryID = StoryID.StoryEvent1502,
			StoryType = StoryType.EventStory,
			Part = 15,
			Chapter = 2,
			Name = "イッシキ・クリアイベントストーリー6",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent1601",
			StoryID = StoryID.StoryEvent1601,
			StoryType = StoryType.EventStory,
			Part = 16,
			Chapter = 1,
			Name = "壱高トリオのメンタンピン教室",
			UnlockingType = UnlockingType.Event
		},
		new Story
		{
			ObjectID = "OID_StoryEvent1701",
			StoryID = StoryID.StoryEvent1701,
			StoryType = StoryType.EventStory,
			Part = 17,
			Chapter = 1,
			Name = "1stアニバありがとう",
			UnlockingType = UnlockingType.Event
		},
		new Story
		{
			ObjectID = "OID_StoryEvent1801",
			StoryID = StoryID.StoryEvent1801,
			StoryType = StoryType.EventStory,
			Part = 18,
			Chapter = 1,
			Name = "究極青天井！炎の暗黒メンタンピンギャラクシー",
			UnlockingType = UnlockingType.Event
		},
		new Story
		{
			ObjectID = "OID_StoryEvent1901",
			StoryID = StoryID.StoryEvent1901,
			StoryType = StoryType.EventStory,
			Part = 19,
			Chapter = 1,
			Name = "BEMANIファイトガール！",
			UnlockingType = UnlockingType.Event
		},
		new Story
		{
			ObjectID = "OID_StoryEvent2001",
			StoryID = StoryID.StoryEvent2001,
			StoryType = StoryType.EventStory,
			Part = 20,
			Chapter = 1,
			Name = "六魂清浄 革命のカタルシス",
			UnlockingType = UnlockingType.Event
		},
		new Story
		{
			ObjectID = "OID_StoryEvent2101",
			StoryID = StoryID.StoryEvent2101,
			StoryType = StoryType.EventStory,
			Part = 21,
			Chapter = 1,
			Name = "小麦色ファイトガール投票選抜戦",
			UnlockingType = UnlockingType.Event
		},
		new Story
		{
			ObjectID = "OID_StoryEvent2201",
			StoryID = StoryID.StoryEvent2201,
			StoryType = StoryType.EventStory,
			Part = 22,
			Chapter = 1,
			Name = "その名はG-14 イヨ！ 前編",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent2202",
			StoryID = StoryID.StoryEvent2202,
			StoryType = StoryType.EventStory,
			Part = 22,
			Chapter = 2,
			Name = "その名はG-14 イヨ！ 後編",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent2203",
			StoryID = StoryID.StoryEvent2203,
			StoryType = StoryType.EventStory,
			Part = 22,
			Chapter = 3,
			Name = "イヨと壱高",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent2204",
			StoryID = StoryID.StoryEvent2204,
			StoryType = StoryType.EventStory,
			Part = 22,
			Chapter = 4,
			Name = "イヨと弐女",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent2205",
			StoryID = StoryID.StoryEvent2205,
			StoryType = StoryType.EventStory,
			Part = 22,
			Chapter = 5,
			Name = "イヨと帝役",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent2206",
			StoryID = StoryID.StoryEvent2206,
			StoryType = StoryType.EventStory,
			Part = 22,
			Chapter = 6,
			Name = "イヨとイッシキ",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent2301",
			StoryID = StoryID.StoryEvent2301,
			StoryType = StoryType.EventStory,
			Part = 23,
			Chapter = 1,
			Name = "霧雨魔法店",
			UnlockingType = UnlockingType.Event
		},
		new Story
		{
			ObjectID = "OID_StoryEvent2401",
			StoryID = StoryID.StoryEvent2401,
			StoryType = StoryType.EventStory,
			Part = 24,
			Chapter = 1,
			Name = "1.5thアニバありがとう",
			UnlockingType = UnlockingType.Event
		},
		new Story
		{
			ObjectID = "OID_StoryEvent2501",
			StoryID = StoryID.StoryEvent2501,
			StoryType = StoryType.EventStory,
			Part = 25,
			Chapter = 1,
			Name = "オペレーションPCK",
			UnlockingType = UnlockingType.Event
		},
		new Story
		{
			ObjectID = "OID_StoryEvent2601",
			StoryID = StoryID.StoryEvent2601,
			StoryType = StoryType.EventStory,
			Part = 26,
			Chapter = 1,
			Name = "魔法少女 らぶはむ♥ココア\u3000その1",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent2602",
			StoryID = StoryID.StoryEvent2602,
			StoryType = StoryType.EventStory,
			Part = 26,
			Chapter = 2,
			Name = "魔法少女 らぶはむ♥ココア\u3000その2",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent2603",
			StoryID = StoryID.StoryEvent2603,
			StoryType = StoryType.EventStory,
			Part = 26,
			Chapter = 3,
			Name = "ココア\u3000よんじょ",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent2604",
			StoryID = StoryID.StoryEvent2604,
			StoryType = StoryType.EventStory,
			Part = 26,
			Chapter = 4,
			Name = "ココア\u3000どうぶつ",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent2605",
			StoryID = StoryID.StoryEvent2605,
			StoryType = StoryType.EventStory,
			Part = 26,
			Chapter = 5,
			Name = "ココア\u3000おともだち",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent2701",
			StoryID = StoryID.StoryEvent2701,
			StoryType = StoryType.EventStory,
			Part = 27,
			Chapter = 1,
			Name = "恋もツモれば！らぶらぶ突撃麻雀",
			UnlockingType = UnlockingType.Event
		},
		new Story
		{
			ObjectID = "OID_StoryEvent2801",
			StoryID = StoryID.StoryEvent2801,
			StoryType = StoryType.EventStory,
			Part = 28,
			Chapter = 1,
			Name = "魔法少女 らぶはむ♥ダイア\u3000その1",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent2802",
			StoryID = StoryID.StoryEvent2802,
			StoryType = StoryType.EventStory,
			Part = 28,
			Chapter = 2,
			Name = "魔法少女 らぶはむ♥ダイア\u3000その2",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent2803",
			StoryID = StoryID.StoryEvent2803,
			StoryType = StoryType.EventStory,
			Part = 28,
			Chapter = 3,
			Name = "ミツモト顧問",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent2804",
			StoryID = StoryID.StoryEvent2804,
			StoryType = StoryType.EventStory,
			Part = 28,
			Chapter = 4,
			Name = "帝役の先生",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent2805",
			StoryID = StoryID.StoryEvent2805,
			StoryType = StoryType.EventStory,
			Part = 28,
			Chapter = 5,
			Name = "会議",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent2901",
			StoryID = StoryID.StoryEvent2901,
			StoryType = StoryType.EventStory,
			Part = 29,
			Chapter = 1,
			Name = "2ndアニバありがとう",
			UnlockingType = UnlockingType.Event
		},
		new Story
		{
			ObjectID = "OID_StoryEvent3001",
			StoryID = StoryID.StoryEvent3001,
			StoryType = StoryType.EventStory,
			Part = 30,
			Chapter = 1,
			Name = "究極青天井！炎の暗黒メンタンPCKギャラクシー霧雨突撃カタルシス",
			UnlockingType = UnlockingType.Event
		},
		new Story
		{
			ObjectID = "OID_StoryEvent3101",
			StoryID = StoryID.StoryEvent3101,
			StoryType = StoryType.EventStory,
			Part = 31,
			Chapter = 1,
			Name = "堕天使ダブリエル\u3000その1",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent3102",
			StoryID = StoryID.StoryEvent3102,
			StoryType = StoryType.EventStory,
			Part = 31,
			Chapter = 2,
			Name = "堕天使ダブリエル\u3000その2",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent3103",
			StoryID = StoryID.StoryEvent3103,
			StoryType = StoryType.EventStory,
			Part = 31,
			Chapter = 3,
			Name = "堕天使ダブリエル\u3000その3",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent3104",
			StoryID = StoryID.StoryEvent3104,
			StoryType = StoryType.EventStory,
			Part = 31,
			Chapter = 4,
			Name = "ダブと弐女",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent3105",
			StoryID = StoryID.StoryEvent3105,
			StoryType = StoryType.EventStory,
			Part = 31,
			Chapter = 5,
			Name = "ダブとアマト氏",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent3106",
			StoryID = StoryID.StoryEvent3106,
			StoryType = StoryType.EventStory,
			Part = 31,
			Chapter = 6,
			Name = "シスターダブリエル",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent3201",
			StoryID = StoryID.StoryEvent3201,
			StoryType = StoryType.EventStory,
			Part = 32,
			Chapter = 1,
			Name = "天才！爆砕！ボンバー卓",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent3301",
			StoryID = StoryID.StoryEvent3301,
			StoryType = StoryType.EventStory,
			Part = 33,
			Chapter = 1,
			Name = "勝負！イッパツ！\u3000その1",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent3302",
			StoryID = StoryID.StoryEvent3302,
			StoryType = StoryType.EventStory,
			Part = 33,
			Chapter = 2,
			Name = "勝負！イッパツ！\u3000その2",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent3303",
			StoryID = StoryID.StoryEvent3303,
			StoryType = StoryType.EventStory,
			Part = 33,
			Chapter = 3,
			Name = "壱高の新入生",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent3304",
			StoryID = StoryID.StoryEvent3304,
			StoryType = StoryType.EventStory,
			Part = 33,
			Chapter = 4,
			Name = "わんか先生サイン会",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent3305",
			StoryID = StoryID.StoryEvent3305,
			StoryType = StoryType.EventStory,
			Part = 33,
			Chapter = 5,
			Name = "インタビュー・ウィズ・クニノカミ",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent3306",
			StoryID = StoryID.StoryEvent3306,
			StoryType = StoryType.EventStory,
			Part = 33,
			Chapter = 6,
			Name = "タテナオリ・イッパツ\u3000ローシャンテへ行く",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent3401",
			StoryID = StoryID.StoryEvent3401,
			StoryType = StoryType.EventStory,
			Part = 34,
			Chapter = 1,
			Name = "GET READYですヨ！\u3000全緑卓",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent3501",
			StoryID = StoryID.StoryEvent3501,
			StoryType = StoryType.EventStory,
			Part = 35,
			Chapter = 1,
			Name = "奥義継承！らぶはむアトミックリーチ卓",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent3601",
			StoryID = StoryID.StoryEvent3601,
			StoryType = StoryType.EventStory,
			Part = 36,
			Chapter = 1,
			Name = "カワサキの鬼ジュン\u3000その1",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent3602",
			StoryID = StoryID.StoryEvent3602,
			StoryType = StoryType.EventStory,
			Part = 36,
			Chapter = 2,
			Name = "カワサキの鬼ジュン\u3000その2",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent3603",
			StoryID = StoryID.StoryEvent3603,
			StoryType = StoryType.EventStory,
			Part = 36,
			Chapter = 3,
			Name = "カワサキの鬼ジュン\u3000その3",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent3604",
			StoryID = StoryID.StoryEvent3604,
			StoryType = StoryType.EventStory,
			Part = 36,
			Chapter = 4,
			Name = "地元最高♪",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent3605",
			StoryID = StoryID.StoryEvent3605,
			StoryType = StoryType.EventStory,
			Part = 36,
			Chapter = 5,
			Name = "“帝役”潰すゾ～！！！",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent3606",
			StoryID = StoryID.StoryEvent3606,
			StoryType = StoryType.EventStory,
			Part = 36,
			Chapter = 6,
			Name = "3年生同盟",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent3701",
			StoryID = StoryID.StoryEvent3701,
			StoryType = StoryType.EventStory,
			Part = 37,
			Chapter = 1,
			Name = "究極青天井！炎のらぶはむ暗黒メンタンPCKギャラクシー霧雨突撃爆砕カタルシスですヨ！",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent3801",
			StoryID = StoryID.StoryEvent3801,
			StoryType = StoryType.EventStory,
			Part = 38,
			Chapter = 1,
			Name = "クレンホウ動く！？",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent3802",
			StoryID = StoryID.StoryEvent3802,
			StoryType = StoryType.EventStory,
			Part = 38,
			Chapter = 2,
			Name = "クレンホウの正体！！",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent3803",
			StoryID = StoryID.StoryEvent3803,
			StoryType = StoryType.EventStory,
			Part = 38,
			Chapter = 3,
			Name = "クレンホウ\u3000死す",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent3804",
			StoryID = StoryID.StoryEvent3804,
			StoryType = StoryType.EventStory,
			Part = 38,
			Chapter = 4,
			Name = "ライバル宣言！？",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent3805",
			StoryID = StoryID.StoryEvent3805,
			StoryType = StoryType.EventStory,
			Part = 38,
			Chapter = 5,
			Name = "その女、ヒットマンにつき",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent3806",
			StoryID = StoryID.StoryEvent3806,
			StoryType = StoryType.EventStory,
			Part = 38,
			Chapter = 6,
			Name = "部長さん集まれー♪",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryEvent3807",
			StoryID = StoryID.StoryEvent3807,
			StoryType = StoryType.EventStory,
			Part = 38,
			Chapter = 7,
			Name = "会議\u30002",
			UnlockingType = UnlockingType.Event,
			Priority = 100
		},
		new Story
		{
			ObjectID = "OID_StoryChara001Main0101",
			StoryID = StoryID.StoryChara001Main0101,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara01,
			Part = 1,
			Chapter = 1,
			Name = "ヒヨリ キャラクターシナリオ １",
			UnlockingType = UnlockingType.Default,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara001Main0102",
			StoryID = StoryID.StoryChara001Main0102,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara01,
			Part = 1,
			Chapter = 2,
			Name = "ヒヨリ キャラクターシナリオ ２",
			UnlockingType = UnlockingType.Debut,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara001Love0101",
			StoryID = StoryID.StoryChara001Love0101,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara01,
			Part = 1,
			Name = "ヒヨリ コミュニケーションシナリオ １",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 10,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara001Love0201",
			StoryID = StoryID.StoryChara001Love0201,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara01,
			Part = 2,
			Name = "ヒヨリ コミュニケーションシナリオ ２",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 40,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara001Love0301",
			StoryID = StoryID.StoryChara001Love0301,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara01,
			Part = 3,
			Name = "ヒヨリ コミュニケーションシナリオ ３",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 80,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara002Main0101",
			StoryID = StoryID.StoryChara002Main0101,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara02,
			Part = 1,
			Chapter = 1,
			Name = "セン キャラクターシナリオ １",
			UnlockingType = UnlockingType.Default,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara002Main0102",
			StoryID = StoryID.StoryChara002Main0102,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara02,
			Part = 1,
			Chapter = 2,
			Name = "セン キャラクターシナリオ ２",
			UnlockingType = UnlockingType.Debut,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara002Love0101",
			StoryID = StoryID.StoryChara002Love0101,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara02,
			Part = 1,
			Name = "セン コミュニケーションシナリオ １",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 10,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara002Love0201",
			StoryID = StoryID.StoryChara002Love0201,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara02,
			Part = 2,
			Name = "セン コミュニケーションシナリオ ２",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 40,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara002Love0301",
			StoryID = StoryID.StoryChara002Love0301,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara02,
			Part = 3,
			Name = "セン コミュニケーションシナリオ ３",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 80,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara003Main0101",
			StoryID = StoryID.StoryChara003Main0101,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara03,
			Part = 1,
			Chapter = 1,
			Name = "ツミレ キャラクターシナリオ １",
			UnlockingType = UnlockingType.Default,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara003Main0102",
			StoryID = StoryID.StoryChara003Main0102,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara03,
			Part = 1,
			Chapter = 2,
			Name = "ツミレ キャラクターシナリオ ２",
			UnlockingType = UnlockingType.Debut,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara003Love0101",
			StoryID = StoryID.StoryChara003Love0101,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara03,
			Part = 1,
			Name = "ツミレ コミュニケーションシナリオ １",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 10,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara003Love0201",
			StoryID = StoryID.StoryChara003Love0201,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara03,
			Part = 2,
			Name = "ツミレ コミュニケーションシナリオ ２",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 40,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara003Love0301",
			StoryID = StoryID.StoryChara003Love0301,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara03,
			Part = 3,
			Name = "ツミレ コミュニケーションシナリオ ３",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 80,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara004Main0101",
			StoryID = StoryID.StoryChara004Main0101,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara04,
			Part = 1,
			Chapter = 1,
			Name = "テンシ キャラクターシナリオ １",
			UnlockingType = UnlockingType.Default,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara004Main0102",
			StoryID = StoryID.StoryChara004Main0102,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara04,
			Part = 1,
			Chapter = 2,
			Name = "テンシ キャラクターシナリオ ２",
			UnlockingType = UnlockingType.Debut,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara004Love0101",
			StoryID = StoryID.StoryChara004Love0101,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara04,
			Part = 1,
			Name = "テンシ コミュニケーションシナリオ １",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 10,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara004Love0201",
			StoryID = StoryID.StoryChara004Love0201,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara04,
			Part = 2,
			Name = "テンシ コミュニケーションシナリオ ２",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 40,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara004Love0301",
			StoryID = StoryID.StoryChara004Love0301,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara04,
			Part = 3,
			Name = "テンシ コミュニケーションシナリオ ３",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 80,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara005Main0101",
			StoryID = StoryID.StoryChara005Main0101,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara05,
			Part = 1,
			Chapter = 1,
			Name = "ヤオ キャラクターシナリオ １",
			UnlockingType = UnlockingType.Default,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara005Main0102",
			StoryID = StoryID.StoryChara005Main0102,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara05,
			Part = 1,
			Chapter = 2,
			Name = "ヤオ キャラクターシナリオ ２",
			UnlockingType = UnlockingType.Debut,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara005Love0101",
			StoryID = StoryID.StoryChara005Love0101,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara05,
			Part = 1,
			Name = "ヤオ コミュニケーションシナリオ １",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 10,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara005Love0201",
			StoryID = StoryID.StoryChara005Love0201,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara05,
			Part = 2,
			Name = "ヤオ コミュニケーションシナリオ ２",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 40,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara005Love0301",
			StoryID = StoryID.StoryChara005Love0301,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara05,
			Part = 3,
			Name = "ヤオ コミュニケーションシナリオ ３",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 80,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara006Main0101",
			StoryID = StoryID.StoryChara006Main0101,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara06,
			Part = 1,
			Chapter = 1,
			Name = "ミツバ キャラクターシナリオ １",
			UnlockingType = UnlockingType.Default,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara006Main0102",
			StoryID = StoryID.StoryChara006Main0102,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara06,
			Part = 1,
			Chapter = 2,
			Name = "ミツバ キャラクターシナリオ ２",
			UnlockingType = UnlockingType.Debut,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara006Love0101",
			StoryID = StoryID.StoryChara006Love0101,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara06,
			Part = 1,
			Name = "ミツバ コミュニケーションシナリオ １",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 10,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara006Love0201",
			StoryID = StoryID.StoryChara006Love0201,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara06,
			Part = 2,
			Name = "ミツバ コミュニケーションシナリオ ２",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 40,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara006Love0301",
			StoryID = StoryID.StoryChara006Love0301,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara06,
			Part = 3,
			Name = "ミツバ コミュニケーションシナリオ ３",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 80,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara007Main0101",
			StoryID = StoryID.StoryChara007Main0101,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara07,
			Part = 1,
			Chapter = 1,
			Name = "トイトイ キャラクターシナリオ １",
			UnlockingType = UnlockingType.Default,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara007Main0102",
			StoryID = StoryID.StoryChara007Main0102,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara07,
			Part = 1,
			Chapter = 2,
			Name = "トイトイ キャラクターシナリオ ２",
			UnlockingType = UnlockingType.Debut,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara007Love0101",
			StoryID = StoryID.StoryChara007Love0101,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara07,
			Part = 1,
			Name = "トイトイ コミュニケーションシナリオ １",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 10,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara007Love0201",
			StoryID = StoryID.StoryChara007Love0201,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara07,
			Part = 2,
			Name = "トイトイ コミュニケーションシナリオ ２",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 40,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara007Love0301",
			StoryID = StoryID.StoryChara007Love0301,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara07,
			Part = 3,
			Name = "トイトイ コミュニケーションシナリオ ３",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 80,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara008Main0101",
			StoryID = StoryID.StoryChara008Main0101,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara08,
			Part = 1,
			Chapter = 1,
			Name = "ムサシ キャラクターシナリオ １",
			UnlockingType = UnlockingType.Default,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara008Main0102",
			StoryID = StoryID.StoryChara008Main0102,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara08,
			Part = 1,
			Chapter = 2,
			Name = "ムサシ キャラクターシナリオ ２",
			UnlockingType = UnlockingType.Debut,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara008Love0101",
			StoryID = StoryID.StoryChara008Love0101,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara08,
			Part = 1,
			Name = "ムサシ コミュニケーションシナリオ １",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 10,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara008Love0201",
			StoryID = StoryID.StoryChara008Love0201,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara08,
			Part = 2,
			Name = "ムサシ コミュニケーションシナリオ ２",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 40,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara008Love0301",
			StoryID = StoryID.StoryChara008Love0301,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara08,
			Part = 3,
			Name = "ムサシ コミュニケーションシナリオ ３",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 80,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara009Main0101",
			StoryID = StoryID.StoryChara009Main0101,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara09,
			Part = 1,
			Chapter = 1,
			Name = "パイン キャラクターシナリオ １",
			UnlockingType = UnlockingType.Default,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara009Main0102",
			StoryID = StoryID.StoryChara009Main0102,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara09,
			Part = 1,
			Chapter = 2,
			Name = "パイン キャラクターシナリオ ２",
			UnlockingType = UnlockingType.Debut,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara009Love0101",
			StoryID = StoryID.StoryChara009Love0101,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara09,
			Part = 1,
			Name = "パイン コミュニケーションシナリオ １",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 10,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara009Love0201",
			StoryID = StoryID.StoryChara009Love0201,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara09,
			Part = 2,
			Name = "パイン コミュニケーションシナリオ ２",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 40,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara009Love0301",
			StoryID = StoryID.StoryChara009Love0301,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara09,
			Part = 3,
			Name = "パイン コミュニケーションシナリオ ３",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 80,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara010Main0101",
			StoryID = StoryID.StoryChara010Main0101,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara10,
			Part = 1,
			Chapter = 1,
			Name = "詩織 キャラクターシナリオ １",
			UnlockingType = UnlockingType.Default,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara010Main0102",
			StoryID = StoryID.StoryChara010Main0102,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara10,
			Part = 1,
			Chapter = 2,
			Name = "詩織 キャラクターシナリオ ２",
			UnlockingType = UnlockingType.Debut,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara010Love0101",
			StoryID = StoryID.StoryChara010Love0101,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara10,
			Part = 1,
			Name = "詩織 コミュニケーションシナリオ １",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 10,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara010Love0201",
			StoryID = StoryID.StoryChara010Love0201,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara10,
			Part = 2,
			Name = "詩織 コミュニケーションシナリオ ２",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 40,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara010Love0301",
			StoryID = StoryID.StoryChara010Love0301,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara10,
			Part = 3,
			Name = "詩織 コミュニケーションシナリオ ３",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 80,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara011Main0101",
			StoryID = StoryID.StoryChara011Main0101,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara11,
			Part = 1,
			Chapter = 1,
			Name = "カオス キャラクターシナリオ １",
			UnlockingType = UnlockingType.Default,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara011Main0102",
			StoryID = StoryID.StoryChara011Main0102,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara11,
			Part = 1,
			Chapter = 2,
			Name = "カオス キャラクターシナリオ ２",
			UnlockingType = UnlockingType.Debut,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara011Love0101",
			StoryID = StoryID.StoryChara011Love0101,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara11,
			Part = 1,
			Name = "カオス コミュニケーションシナリオ １",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 10,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara011Love0201",
			StoryID = StoryID.StoryChara011Love0201,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara11,
			Part = 2,
			Name = "カオス コミュニケーションシナリオ ２",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 40,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara011Love0301",
			StoryID = StoryID.StoryChara011Love0301,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara11,
			Part = 3,
			Name = "カオス コミュニケーションシナリオ ３",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 80,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara012Main0101",
			StoryID = StoryID.StoryChara012Main0101,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara12,
			Part = 1,
			Chapter = 1,
			Name = "クリア キャラクターシナリオ １",
			UnlockingType = UnlockingType.Default,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara012Main0102",
			StoryID = StoryID.StoryChara012Main0102,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara12,
			Part = 1,
			Chapter = 2,
			Name = "クリア キャラクターシナリオ ２",
			UnlockingType = UnlockingType.Debut,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara012Love0101",
			StoryID = StoryID.StoryChara012Love0101,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara12,
			Part = 1,
			Name = "クリア コミュニケーションシナリオ １",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 10,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara012Love0201",
			StoryID = StoryID.StoryChara012Love0201,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara12,
			Part = 2,
			Name = "クリア コミュニケーションシナリオ ２",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 40,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara012Love0301",
			StoryID = StoryID.StoryChara012Love0301,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara12,
			Part = 3,
			Name = "クリア コミュニケーションシナリオ ３",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 80,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara013Main0101",
			StoryID = StoryID.StoryChara013Main0101,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara13,
			Part = 1,
			Chapter = 1,
			Name = "イヨ キャラクターシナリオ １",
			UnlockingType = UnlockingType.Default,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara013Main0102",
			StoryID = StoryID.StoryChara013Main0102,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara13,
			Part = 1,
			Chapter = 2,
			Name = "イヨ キャラクターシナリオ ２",
			UnlockingType = UnlockingType.Debut,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara013Love0101",
			StoryID = StoryID.StoryChara013Love0101,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara13,
			Part = 1,
			Name = "イヨ コミュニケーションシナリオ １",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 10,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara013Love0201",
			StoryID = StoryID.StoryChara013Love0201,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara13,
			Part = 2,
			Name = "イヨ コミュニケーションシナリオ ２",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 40,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara013Love0301",
			StoryID = StoryID.StoryChara013Love0301,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara13,
			Part = 3,
			Name = "イヨ コミュニケーションシナリオ ３",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 80,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara014Main0101",
			StoryID = StoryID.StoryChara014Main0101,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara14,
			Part = 1,
			Chapter = 1,
			Name = "グリム・アロエ キャラクターシナリオ １",
			UnlockingType = UnlockingType.Default,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara014Main0102",
			StoryID = StoryID.StoryChara014Main0102,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara14,
			Part = 1,
			Chapter = 2,
			Name = "グリム・アロエ キャラクターシナリオ ２",
			UnlockingType = UnlockingType.Debut,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara014Love0101",
			StoryID = StoryID.StoryChara014Love0101,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara14,
			Part = 1,
			Name = "グリム・アロエ コミュニケーションシナリオ １",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 10,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara014Love0201",
			StoryID = StoryID.StoryChara014Love0201,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara14,
			Part = 2,
			Name = "グリム・アロエ コミュニケーションシナリオ ２",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 40,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara014Love0301",
			StoryID = StoryID.StoryChara014Love0301,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara14,
			Part = 3,
			Name = "グリム・アロエ コミュニケーションシナリオ ３",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 80,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara015Main0101",
			StoryID = StoryID.StoryChara015Main0101,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara15,
			Part = 1,
			Chapter = 1,
			Name = "ココア キャラクターシナリオ １",
			UnlockingType = UnlockingType.Default,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara015Main0102",
			StoryID = StoryID.StoryChara015Main0102,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara15,
			Part = 1,
			Chapter = 2,
			Name = "ココア キャラクターシナリオ ２",
			UnlockingType = UnlockingType.Debut,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara015Love0101",
			StoryID = StoryID.StoryChara015Love0101,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara15,
			Part = 1,
			Name = "ココア コミュニケーションシナリオ １",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 10,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara015Love0201",
			StoryID = StoryID.StoryChara015Love0201,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara15,
			Part = 2,
			Name = "ココア コミュニケーションシナリオ ２",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 40,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara015Love0301",
			StoryID = StoryID.StoryChara015Love0301,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara15,
			Part = 3,
			Name = "ココア コミュニケーションシナリオ ３",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 80,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara016Main0101",
			StoryID = StoryID.StoryChara016Main0101,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara16,
			Part = 1,
			Chapter = 1,
			Name = "ダイア キャラクターシナリオ １",
			UnlockingType = UnlockingType.Default,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara016Main0102",
			StoryID = StoryID.StoryChara016Main0102,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara16,
			Part = 1,
			Chapter = 2,
			Name = "ダイア キャラクターシナリオ ２",
			UnlockingType = UnlockingType.Debut,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara016Love0101",
			StoryID = StoryID.StoryChara016Love0101,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara16,
			Part = 1,
			Name = "ダイア コミュニケーションシナリオ １",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 10,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara016Love0201",
			StoryID = StoryID.StoryChara016Love0201,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara16,
			Part = 2,
			Name = "ダイア コミュニケーションシナリオ ２",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 40,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara016Love0301",
			StoryID = StoryID.StoryChara016Love0301,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara16,
			Part = 3,
			Name = "ダイア コミュニケーションシナリオ ３",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 80,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara017Main0101",
			StoryID = StoryID.StoryChara017Main0101,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara17,
			Part = 1,
			Chapter = 1,
			Name = "ダブリエル キャラクターシナリオ １",
			UnlockingType = UnlockingType.Default,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara017Main0102",
			StoryID = StoryID.StoryChara017Main0102,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara17,
			Part = 1,
			Chapter = 2,
			Name = "ダブリエル キャラクターシナリオ ２",
			UnlockingType = UnlockingType.Debut,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara017Love0101",
			StoryID = StoryID.StoryChara017Love0101,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara17,
			Part = 1,
			Name = "ダブリエル コミュニケーションシナリオ １",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 10,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara017Love0201",
			StoryID = StoryID.StoryChara017Love0201,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara17,
			Part = 2,
			Name = "ダブリエル コミュニケーションシナリオ ２",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 40,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara017Love0301",
			StoryID = StoryID.StoryChara017Love0301,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara17,
			Part = 3,
			Name = "ダブリエル コミュニケーションシナリオ ３",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 80,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara018Main0101",
			StoryID = StoryID.StoryChara018Main0101,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara18,
			Part = 1,
			Chapter = 1,
			Name = "イッパツ キャラクターシナリオ １",
			UnlockingType = UnlockingType.Default,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara018Main0102",
			StoryID = StoryID.StoryChara018Main0102,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara18,
			Part = 1,
			Chapter = 2,
			Name = "イッパツ キャラクターシナリオ ２",
			UnlockingType = UnlockingType.Debut,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara018Love0101",
			StoryID = StoryID.StoryChara018Love0101,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara18,
			Part = 1,
			Name = "イッパツ コミュニケーションシナリオ １",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 10,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara018Love0201",
			StoryID = StoryID.StoryChara018Love0201,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara18,
			Part = 2,
			Name = "イッパツ コミュニケーションシナリオ ２",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 40,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara018Love0301",
			StoryID = StoryID.StoryChara018Love0301,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara18,
			Part = 3,
			Name = "イッパツ コミュニケーションシナリオ ３",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 80,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara019Main0101",
			StoryID = StoryID.StoryChara019Main0101,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara19,
			Part = 1,
			Chapter = 1,
			Name = "シロヱ キャラクターシナリオ １",
			UnlockingType = UnlockingType.Default,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara019Main0102",
			StoryID = StoryID.StoryChara019Main0102,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara19,
			Part = 1,
			Chapter = 2,
			Name = "シロヱ キャラクターシナリオ ２",
			UnlockingType = UnlockingType.Debut,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara019Love0101",
			StoryID = StoryID.StoryChara019Love0101,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara19,
			Part = 1,
			Name = "シロヱ コミュニケーションシナリオ １",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 10,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara019Love0201",
			StoryID = StoryID.StoryChara019Love0201,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara19,
			Part = 2,
			Name = "シロヱ コミュニケーションシナリオ ２",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 40,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara019Love0301",
			StoryID = StoryID.StoryChara019Love0301,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara19,
			Part = 3,
			Name = "シロヱ コミュニケーションシナリオ ３",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 80,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara020Main0101",
			StoryID = StoryID.StoryChara020Main0101,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara20,
			Part = 1,
			Chapter = 1,
			Name = "ジュン キャラクターシナリオ １",
			UnlockingType = UnlockingType.Default,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara020Main0102",
			StoryID = StoryID.StoryChara020Main0102,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara20,
			Part = 1,
			Chapter = 2,
			Name = "ジュン キャラクターシナリオ ２",
			UnlockingType = UnlockingType.Debut,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara020Love0101",
			StoryID = StoryID.StoryChara020Love0101,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara20,
			Part = 1,
			Name = "ジュン コミュニケーションシナリオ １",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 10,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara020Love0201",
			StoryID = StoryID.StoryChara020Love0201,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara20,
			Part = 2,
			Name = "ジュン コミュニケーションシナリオ ２",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 40,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara020Love0301",
			StoryID = StoryID.StoryChara020Love0301,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara20,
			Part = 3,
			Name = "ジュン コミュニケーションシナリオ ３",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 80,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara021Main0101",
			StoryID = StoryID.StoryChara021Main0101,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara21,
			Part = 1,
			Chapter = 1,
			Name = "トウカ キャラクターシナリオ １",
			UnlockingType = UnlockingType.Default,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara021Main0102",
			StoryID = StoryID.StoryChara021Main0102,
			StoryType = StoryType.GirlsMainStory,
			CharaType = CharaType.Chara21,
			Part = 1,
			Chapter = 2,
			Name = "トウカ キャラクターシナリオ ２",
			UnlockingType = UnlockingType.Debut,
			Display = true
		},
		new Story
		{
			ObjectID = "OID_StoryChara021Love0101",
			StoryID = StoryID.StoryChara021Love0101,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara21,
			Part = 1,
			Name = "トウカ コミュニケーションシナリオ １",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 10,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara021Love0201",
			StoryID = StoryID.StoryChara021Love0201,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara21,
			Part = 2,
			Name = "トウカ コミュニケーションシナリオ ２",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 40,
			Display = false
		},
		new Story
		{
			ObjectID = "OID_StoryChara021Love0301",
			StoryID = StoryID.StoryChara021Love0301,
			StoryType = StoryType.GirlsLoveStory,
			CharaType = CharaType.Chara21,
			Part = 3,
			Name = "トウカ コミュニケーションシナリオ ３",
			UnlockingType = UnlockingType.Favo,
			UnlockingCondition = 80,
			Display = false
		}
	};

	public static Story GetStory(StoryID storyID)
	{
		return _stories.Find((Story story) => story.StoryID == storyID) ?? null;
	}

	public static StoryID ToStoryID(string oid)
	{
		return _stories.Find((Story _story) => _story.ObjectID == oid)?.StoryID ?? StoryID.None;
	}

	public static List<Story> GetStories(Predicate<Story> match)
	{
		return _stories.FindAll(match);
	}

	public static string UnlockText(Story story)
	{
		string result = "";
		if (story.CharaType == CharaType.Chara10 && !GameDataManager.IsShioriStoryEnable())
		{
			return "後日解禁";
		}
		switch (story.UnlockingType)
		{
		case UnlockingType.Debut:
			result = "デビューすると解禁";
			break;
		case UnlockingType.Favo:
			result = "好感度" + story.UnlockingCondition + "で解禁";
			break;
		case UnlockingType.Mission:
			result = "";
			break;
		case UnlockingType.Event:
			result = "イベント開催で解禁";
			break;
		}
		return result;
	}

	public static List<StoryID> GetDefaultStoryIDs()
	{
		List<StoryID> defaultStories = _defaultStories;
		foreach (Story story2 in GetStories((Story story) => story.UnlockingType == UnlockingType.Default))
		{
			if (defaultStories.FindIndex((StoryID x) => x == story2.StoryID) == -1)
			{
				defaultStories.Add(story2.StoryID);
			}
		}
		return defaultStories;
	}
}
