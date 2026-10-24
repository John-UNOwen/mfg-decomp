using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using KAMUNITY;
using TestModeSkeleton.CabinetDef;
using TestModeSkeleton.TestModeExtensions;
using UnityEngine;
using UnityEngine.UI;

namespace TestModeSkeleton;

public class TestModeLocalization : SingletonBehaviour<TestModeLocalization>
{
	private class LocalizedTexts
	{
		public string English { get; private set; }

		public string Japanese { get; private set; }

		public LocalizedTexts(string english = "", string japanese = "")
		{
			English = english;
			Japanese = japanese;
		}
	}

	private readonly Regex regex = new Regex("<l>(.*?)</l>", RegexOptions.Singleline);

	private readonly Dictionary<string, LocalizedTexts> LocalizedTextsDictionary;

	public string GetTextFromKey(string key)
	{
		if (!LocalizedTextsDictionary.ContainsKey(key))
		{
			return key;
		}
		return CabinetManager.Lang.Data.TestModeLanguage switch
		{
			LanguageOptionsDef.TestMode.Japanese => LocalizedTextsDictionary[key].Japanese, 
			LanguageOptionsDef.TestMode.English => LocalizedTextsDictionary[key].English, 
			_ => key, 
		};
	}

	public string GetTextFromTagText(string text)
	{
		return regex.Replace(text, (Match m) => GetTextFromKey(m.Groups[1].Value));
	}

	public Dictionary<Text, string> SetTagTexts(GameObject gameObject)
	{
		Canvas[] componentsInChildren = gameObject.GetComponentsInChildren<Canvas>();
		if (componentsInChildren == null)
		{
			return null;
		}
		List<Text> list = componentsInChildren[0].gameObject.FindChildComponentsRecursively<Text>();
		if (componentsInChildren.Length > 1)
		{
			list.AddRange(componentsInChildren[1].gameObject.FindChildComponentsRecursively<Text>());
		}
		Dictionary<Text, string> result = list.Where((Text t) => regex.IsMatch(t.text)).ToDictionary((Text t) => t, (Text t) => t.text);
		list.ForEach(delegate(Text t)
		{
			t.text = GetTextFromTagText(t.text);
		});
		return result;
	}

	public void SetTagTexts(Dictionary<Text, string> texts)
	{
		texts?.ToList().ForEach(delegate(KeyValuePair<Text, string> p)
		{
			p.Key.text = GetTextFromTagText(p.Value);
		});
	}

	[Conditional("DEBUG")]
	public void DumpLocalizedTexts(int displayLength = -1)
	{
		Converter<string, string> PartialString = delegate(string str)
		{
			if (string.IsNullOrEmpty(str))
			{
				return "";
			}
			return (displayLength < 0) ? str : str[..Mathf.Min(str.Length - 1, displayLength)];
		};
		KAMUNITY.Logger.Log($"登録数:{LocalizedTextsDictionary.Count}");
		LocalizedTextsDictionary.OrderBy((KeyValuePair<string, LocalizedTexts> p) => p.Key).ToList().ForEach(delegate(KeyValuePair<string, LocalizedTexts> p)
		{
			KAMUNITY.Logger.Log("[" + PartialString(p.Key) + "]=>[" + PartialString(p.Value.English) + "][" + PartialString(p.Value.Japanese) + "]");
		});
	}

	public TestModeLocalization()
	{
		Dictionary<string, LocalizedTexts> dictionary = new Dictionary<string, LocalizedTexts>();
		dictionary["CHECKING"] = new LocalizedTexts("CHECKING", "起動チェック");
		dictionary["I/O"] = new LocalizedTexts("I/O CHECK", "本体チェック");
		dictionary["SECURITY"] = new LocalizedTexts("SECURITY CHECK", "セキュリティチェック");
		dictionary["BACKUP"] = new LocalizedTexts("BACKUP CHECK", "保存データチェック");
		dictionary["CALIBRATION"] = new LocalizedTexts("CALIBRATION", "キャリブレーション設定チェック");
		dictionary["CLOCK"] = new LocalizedTexts("CLOCK", "時計設定チェック");
		dictionary["DATA1"] = new LocalizedTexts("DATA1", "データチェック");
		dictionary["e-amusement"] = new LocalizedTexts("e-amusement", "e-amusementチェック");
		dictionary["SERVER MESSAGE"] = new LocalizedTexts("SERVER MESSAGE", "サーバーメッセージチェック");
		dictionary["NON CONSUMED"] = new LocalizedTexts("NON CONSUMED", "プレー状況チェック");
		dictionary["MAIN MENU"] = new LocalizedTexts("MAIN MENU", "メインメニュー");
		dictionary["I/O CHECK"] = new LocalizedTexts("I/O CHECK", "本体チェック");
		dictionary["INPUT CHECK"] = new LocalizedTexts("INPUT CHECK", "入力チェック");
		dictionary["ANALOG STICK CHECK"] = new LocalizedTexts("ANALOG STICK CHECK", "アナログスティックチェック");
		dictionary["TOUCH PANEL CHECK"] = new LocalizedTexts("TOUCH PANEL CHECK", "タッチパネルチェック");
		dictionary["LAMP CHECK"] = new LocalizedTexts("LAMP CHECK", "ランプチェック");
		dictionary["MECHANISM CHECK"] = new LocalizedTexts("MECHANISM CHECK", "制御チェック");
		dictionary["SCREEN CHECK"] = new LocalizedTexts("SCREEN CHECK", "画面チェック");
		dictionary["COLOR CHECK"] = new LocalizedTexts("COLOR CHECK", "カラーチェック");
		dictionary["ROM CHECK"] = new LocalizedTexts("ROM CHECK", "ディスクチェック");
		dictionary["IC CARD CHECK"] = new LocalizedTexts("IC CARD CHECK", "ICカードチェック");
		dictionary["BARCODE READER CHECK"] = new LocalizedTexts("BARCODE READER CHECK", "神姫カードリーダーチェック");
		dictionary["SOUND OPTIONS"] = new LocalizedTexts("SOUND OPTIONS", "サウンド設定");
		dictionary["GAME OPTIONS"] = new LocalizedTexts("GAME OPTIONS", "ゲーム設定");
		dictionary["CLOSING TIME OPTIONS"] = new LocalizedTexts("CLOSING TIME OPTIONS", "閉店時刻設定");
		dictionary["COIN OPTIONS"] = new LocalizedTexts("COIN OPTIONS", "コイン設定");
		dictionary["NETWORK OPTIONS"] = new LocalizedTexts("NETWORK OPTIONS", "ネットワーク設定");
		dictionary["NETWORK CHECK"] = new LocalizedTexts("NETWORK CHECK", "ネットワークチェック");
		dictionary["LANGUAGE OPTIONS"] = new LocalizedTexts("LANGUAGE OPTIONS", "言語設定");
		dictionary["TL/BOOKKEEPING"] = new LocalizedTexts("BOOKKEEPING", "集計データ");
		dictionary["TL/CLOCK"] = new LocalizedTexts("CLOCK", "時刻設定");
		dictionary["VIRTUAL COIN"] = new LocalizedTexts("VIRTUAL COIN", "電子マネー");
		dictionary["OPERATION SETTINGS"] = new LocalizedTexts("OPERATION SETTINGS", "プレー料金設定");
		dictionary["PATTERN"] = new LocalizedTexts("PATTERN", "料金パターン");
		dictionary["SCHEDULE"] = new LocalizedTexts("SCHEDULE", "料金スケジュール");
		dictionary["SUPPLY"] = new LocalizedTexts("SUPPLY", "EXTRA PASELI の付与");
		dictionary["INCOME LOG"] = new LocalizedTexts("INCOME LOG", "利用集計");
		dictionary["PLAYER'S LOG"] = new LocalizedTexts("PLAYER'S LOG", "プレーヤー利用履歴");
		dictionary["GACHA TICKET LOG"] = new LocalizedTexts("GACHA TICKET LOG", "ガチャチケット利用履歴");
		dictionary["ALL FACTORY SETTINGS"] = new LocalizedTexts("ALL FACTORY SETTINGS", "すべて初期化");
		dictionary["SYSTEM INFORMATION"] = new LocalizedTexts("SYSTEM INFORMATION", "システム情報");
		dictionary["GAME MODE"] = new LocalizedTexts("GAME MODE", "ゲームモード");
		dictionary["EVENT MODE"] = new LocalizedTexts("EVENT MODE", "イベントモード");
		dictionary["QC MODE"] = new LocalizedTexts("QC MODE", "QCモード");
		dictionary["MENU"] = new LocalizedTexts("MENU", "メニュー");
		dictionary["COIN DATA OF LAST 7 DAYS"] = new LocalizedTexts("COIN DATA OF LAST 7 DAYS", "過去7日間のデータ");
		dictionary["COIN DATA OF LAST 2 MONTHS"] = new LocalizedTexts("COIN DATA OF LAST 2 MONTHS", "過去2カ月間のデータ");
		dictionary["COIN DATA OF LAST 52 WEEKS"] = new LocalizedTexts("COIN DATA OF LAST 52 WEEKS", "過去52週のデータ");
		dictionary["COIN DATA OF EACH DAY"] = new LocalizedTexts("COIN DATA OF EACH DAY", "曜日毎のデータ");
		dictionary["COIN DATA OF EACH HOUR"] = new LocalizedTexts("COIN DATA OF EACH HOUR", "時間毎のデータ");
		dictionary["PLAY DATA SUMMARY"] = new LocalizedTexts("PLAY DATA SUMMARY", "プレー状況");
		dictionary["DETAIL"] = new LocalizedTexts("DETAIL", "直近20件分の消費履歴");
		dictionary["TOUCH PANEL CALIBRATION"] = new LocalizedTexts("TOUCH PANEL CALIBRATION", "タッチパネルキャリブレーション設定");
		dictionary["YES"] = new LocalizedTexts("YES", "はい");
		dictionary["NO"] = new LocalizedTexts("NO", "いいえ");
		dictionary["YOU HAVE NOT SAVED YET. SAVE CHANGES NOW?"] = new LocalizedTexts("YOU HAVE NOT SAVED YET. SAVE CHANGES NOW?", "まだ保存していません。変更を保存しますか？");
		dictionary["YOU HAVE NOT SET YET. SET CHANGES NOW?"] = new LocalizedTexts("YOU HAVE NOT SET YET. SET CHANGES NOW?", "まだ設定していません。設定を反映しますか？");
		dictionary["DO YOU WANT ALL FACTORY SETTINGS?"] = new LocalizedTexts("DO YOU WANT ALL FACTORY SETTINGS?", "すべて初期化しますか？");
		dictionary["DO YOU WANT TO CLEAR ALL DATA?"] = new LocalizedTexts("DO YOU WANT TO CLEAR ALL DATA?", "すべてデータ消去しますか？");
		dictionary["SURE?"] = new LocalizedTexts("SURE?", "本当によろしいですか？");
		dictionary["NO MODIFICATION"] = new LocalizedTexts("NO MODIFICATION", "変更されませんでした");
		dictionary["NOT CLEARED"] = new LocalizedTexts("NOT CLEARED", "データ消去されませんでした");
		dictionary["NOT SET"] = new LocalizedTexts("NOT SET", "設定されませんでした");
		dictionary["ABORTED"] = new LocalizedTexts("ABORTED", "中止しました");
		dictionary["NOW ERASING"] = new LocalizedTexts("NOW ERASING", "データ消去中");
		dictionary["NOW SAVING"] = new LocalizedTexts("NOW SAVING", "保存中");
		dictionary["CHECK ALL ITEMS BEFORE OPERATION."] = new LocalizedTexts("CHECK ALL ITEMS BEFORE OPERATION.", "運営前に、テストモードの各種設定を確認してください");
		dictionary["FIRST, SET THE CLOCK. \"CLOCK\" ITEM IS IN MAIN MENU."] = new LocalizedTexts("FIRST, SET THE CLOCK. \"CLOCK\" ITEM IS IN MAIN MENU.", "まず、時刻設定してください。\n時刻設定はメインメニューにあります。");
		dictionary["SET EXTRA PASELI 1"] = new LocalizedTexts("SET EXTRA PASELI ", "EXTRA PASELI を ");
		dictionary["SET EXTRA PASELI 2"] = new LocalizedTexts("", " に設定しました");
		dictionary["PLEASE WAIT A MOMENT"] = new LocalizedTexts("PLEASE WAIT A MOMENT", "しばらくお待ちください");
		dictionary["TOUCH THE PANEL TO CHECK"] = new LocalizedTexts("TOUCH THE PANEL TO CHECK", "画面をタッチして確認してください");
		dictionary["CARIBLATION TO CHECK"] = new LocalizedTexts("PLEASE TOUCH THE CURSOR", "カーソルをタッチしてください");
		dictionary["CARIBLATION COMPLETE"] = new LocalizedTexts("CARIBLATION IS COMPLETED", "キャリブレーションが完了しました");
		dictionary["HOLD DOWN TEST + SERVICE BUTTON = CHANGEPASSWORD"] = new LocalizedTexts("HOLD DOWN TEST + SERVICE BUTTON = CHANGEPASSWORD", "TESTボタンを押しながらSERVICEボタン = パスワード変更");
		dictionary["HOLD DOWN TEST + SERVICE BUTTON = EXIT"] = new LocalizedTexts("HOLD DOWN TEST + SERVICE BUTTON = EXIT", "TESTボタンを押しながらSERVICEボタン = 終了");
		dictionary["HOLD DOWN TEST + SERVICE BUTTON = SUBMENU"] = new LocalizedTexts("HOLD DOWN TEST + SERVICE BUTTON = SUBMENU", "TESTボタンを押しながらSERVICEボタン = サブメニュー");
		dictionary["HOLD DOWN TEST + SERVICE BUTTON = CALIBRATION"] = new LocalizedTexts("HOLD DOWN TEST + SERVICE BUTTON = CALIBRATION", "TESTボタンを押しながらSERVICEボタン = キャリブレーション");
		dictionary["HOLD DOWN TEST + SERVICE BUTTON = ABORT"] = new LocalizedTexts("HOLD DOWN TEST + SERVICE BUTTON = ABORT", "TESTボタンを押しながらSERVICEボタン = 中止");
		dictionary["SERVICE BUTTON = ABORT"] = new LocalizedTexts("SERVICE BUTTON = ABORT", "SERVICEボタン = 中止");
		dictionary["SERVICE BUTTON = CHANGE VALUE"] = new LocalizedTexts("SERVICE BUTTON = CHANGE VALUE", "SERVICEボタン = 設定を変える");
		dictionary["SERVICE BUTTON = EXECUTE"] = new LocalizedTexts("SERVICE BUTTON = EXECUTE", "SERVICEボタン = 決定");
		dictionary["SERVICE BUTTON = EXIT"] = new LocalizedTexts("SERVICE BUTTON = EXIT", "SERVICEボタン = 終了");
		dictionary["SERVICE BUTTON = SETTING"] = new LocalizedTexts("SERVICE BUTTON = SETTING", "SERVICEボタン = 設定");
		dictionary["SERVICE BUTTON = DECIDE NEUTRAL"] = new LocalizedTexts("SERVICE BUTTON = DECIDE NEUTRAL", "SERVICEボタン = 次へ");
		dictionary["SERVICE BUTTON = DECIDE VALUE RANGE"] = new LocalizedTexts("SERVICE BUTTON = DECIDE VALUE RANGE", "SERVICEボタン = 次へ");
		dictionary["SERVICE BUTTON = DECIDE IGNORE RANGE"] = new LocalizedTexts("SERVICE BUTTON = DECIDE IGNORE RANGE", "SERVICEボタン = 設定完了");
		dictionary["OK BUTTON = ABORT"] = new LocalizedTexts("OK BUTTON = ABORT", "OKボタン = 中止");
		dictionary["OK BUTTON = CHANGE VALUE"] = new LocalizedTexts("OK BUTTON = CHANGE VALUE", "OKボタン = 設定を変える");
		dictionary["OK BUTTON = EXECUTE"] = new LocalizedTexts("OK BUTTON = EXECUTE", "OKボタン = 決定");
		dictionary["OK BUTTON = EXIT"] = new LocalizedTexts("OK BUTTON = EXIT", "OKボタン = 終了");
		dictionary["OK BUTTON = SETTING"] = new LocalizedTexts("OK BUTTON = SETTING", "OKボタン = 設定");
		dictionary["OK BUTTON = DECIDE NEUTRAL"] = new LocalizedTexts("OK BUTTON = DECIDE NEUTRAL", "OKボタン = 次へ");
		dictionary["OK BUTTON = DECIDE VALUE RANGE"] = new LocalizedTexts("OK BUTTON = DECIDE VALUE RANGE", "OKボタン = 次へ");
		dictionary["OK BUTTON = DECIDE IGNORE RANGE"] = new LocalizedTexts("OK BUTTON = DECIDE IGNORE RANGE", "OKボタン = 設定完了");
		dictionary["TEST BUTTON = CHANGE CLOCK"] = new LocalizedTexts("TEST BUTTON = CHANGE CLOCK", "TESTボタン = 時刻変更");
		dictionary["TEST BUTTON = CHANGE TIME"] = new LocalizedTexts("TEST BUTTON = CHANGE TIME", "TESTボタン = 時間変更");
		dictionary["TEST BUTTON = CHANGE VALUE"] = new LocalizedTexts("TEST BUTTON = CHANGE VALUE", "TESTボタン = 設定を変える");
		dictionary["TEST BUTTON = NEXT PAGE"] = new LocalizedTexts("TEST BUTTON = NEXT PAGE", "TESTボタン = 次のページ");
		dictionary["TEST BUTTON = SELECT ITEM"] = new LocalizedTexts("TEST BUTTON = SELECT ITEM", "TESTボタン = 移動");
		dictionary["TEST BUTTON = TEST MODE"] = new LocalizedTexts("TEST BUTTON = TEST MODE", "TESTボタン = テストモード");
		dictionary["TEST BUTTON = CHANGE SUB MONITOR IMAGE"] = new LocalizedTexts("TEST BUTTON = CHANGE SUB MONITOR IMAGE", "TESTボタン = サブモニター画像変更");
		dictionary["L+R BUTTON = CHANGEPASSWORD"] = new LocalizedTexts("L+R BUTTON = CHANGEPASSWORD", "左ボタンを押しながら右ボタン = パスワード変更");
		dictionary["L+R BUTTON = EXIT"] = new LocalizedTexts("L+R BUTTON = EXIT", "左ボタンを押しながら右ボタン = 終了");
		dictionary["L+R BUTTON = SUBMENU"] = new LocalizedTexts("L+R BUTTON = SUBMENU", "左ボタンを押しながら右ボタン = サブメニュー");
		dictionary["L+R BUTTON = CALIBRATION"] = new LocalizedTexts("L+R BUTTON = CALIBRATION", "左ボタンを押しながら右ボタン = キャリブレーション");
		dictionary["L+R BUTTON = ABORT"] = new LocalizedTexts("L+R BUTTON = ABORT", "左ボタンを押しながら右ボタン = 中止");
		dictionary["L/R BUTTON = CHANGE CLOCK"] = new LocalizedTexts("L/R BUTTON = CHANGE CLOCK", "左/右ボタン = 時刻変更");
		dictionary["L/R BUTTON = CHANGE TIME"] = new LocalizedTexts("L/R BUTTON = CHANGE TIME", "左/右ボタン = 時間変更");
		dictionary["L/R BUTTON = CHANGE VALUE"] = new LocalizedTexts("L/R BUTTON = CHANGE VALUE", "左/右ボタン = 設定を変える");
		dictionary["L/R BUTTON = CHANGE LED"] = new LocalizedTexts("L/R BUTTON = CHANGE LED", "左/右ボタン = LEDを指定");
		dictionary["L/R BUTTON = PREV/NEXT PAGE"] = new LocalizedTexts("L/R BUTTON = PREV/NEXT PAGE", "左/右ボタン = 前/次のページ");
		dictionary["L/R BUTTON = SELECT ITEM"] = new LocalizedTexts("L/R BUTTON = SELECT ITEM", "左/右ボタン = 移動");
		dictionary["L/R BUTTON = CHANGE SUB MONITOR IMAGE"] = new LocalizedTexts("L/R BUTTON = CHANGE SUB MONITOR IMAGE", "左/右ボタン = サブモニター画像変更");
		dictionary["U/D BUTTON = CHANGE VALUE"] = new LocalizedTexts("U/D BUTTON = CHANGE VALUE", "上/下ボタン = 設定を変える");
		dictionary["U/D BUTTON = SELECT ITEM"] = new LocalizedTexts("U/D BUTTON = SELECT ITEM", "上/下ボタン = 移動");
		dictionary["FACTORY SETTINGS"] = new LocalizedTexts("FACTORY SETTINGS", "初期化");
		dictionary["SAVE AND EXIT"] = new LocalizedTexts("SAVE AND EXIT", "保存して終了");
		dictionary["EXIT"] = new LocalizedTexts("EXIT", "終了");
		dictionary["CO/START"] = new LocalizedTexts("START", "ノーマルプレー料金");
		dictionary["CO/PREMIUMSTART"] = new LocalizedTexts("PREMIUM START", "プレミアムプレー追加料金");
		dictionary["CO/EXBOOSTSTART"] = new LocalizedTexts("AFFINITY BOOST", "親密度急上昇モード料金");
		dictionary["CO/CONTINUATION"] = new LocalizedTexts("CONTINUATION", "コンティニュー料金");
		dictionary["CO/TIME EXTENTION"] = new LocalizedTexts("TIME EXTENTION", "時間延長料金");
		dictionary["CO/GACHA"] = new LocalizedTexts("GACHA", "ガールズカットインガチャ(1回)料金");
		dictionary["CO/GACHA2"] = new LocalizedTexts("GACHA2", "ガールズカットインガチャ(5回)料金");
		dictionary["CO/FIGHT"] = new LocalizedTexts("FIGHT MODE", "ファイトモード料金");
		dictionary["CO/STORY"] = new LocalizedTexts("STORY START", "ストーリースタート料金");
		dictionary["CO/FULLSET"] = new LocalizedTexts("FULLSET START", "フルセットスタート料金");
		dictionary["TODAY"] = new LocalizedTexts("TODAY", "本日");
		dictionary["LAST 7 DAYS AVERAGE"] = new LocalizedTexts("LAST 7 DAYS AVERAGE", "過去7日間平均");
		dictionary["LAST 7 DAYS TOTAL"] = new LocalizedTexts("LAST 7 DAYS TOTAL", "過去7日間合計");
		dictionary["YESTERDAY"] = new LocalizedTexts("YESTERDAY", "昨日");
		dictionary["-2 DAYS"] = new LocalizedTexts("-2 DAYS", "2日前");
		dictionary["-3 DAYS"] = new LocalizedTexts("-3 DAYS", "3日前");
		dictionary["-4 DAYS"] = new LocalizedTexts("-4 DAYS", "4日前");
		dictionary["-5 DAYS"] = new LocalizedTexts("-5 DAYS", "5日前");
		dictionary["-6 DAYS"] = new LocalizedTexts("-6 DAYS", "6日前");
		dictionary["-7 DAYS"] = new LocalizedTexts("-7 DAYS", "7日前");
		dictionary["TOTAL"] = new LocalizedTexts("TOTAL", "合計");
		dictionary["MONDAY"] = new LocalizedTexts("MONDAY", "月曜日");
		dictionary["TUESDAY"] = new LocalizedTexts("TUESDAY", "火曜日");
		dictionary["WEDNESDAY"] = new LocalizedTexts("WEDNESDAY", "水曜日");
		dictionary["THURSDAY"] = new LocalizedTexts("THURSDAY", "木曜日");
		dictionary["FRIDAY"] = new LocalizedTexts("FRIDAY", "金曜日");
		dictionary["SATURDAY"] = new LocalizedTexts("SATURDAY", "土曜日");
		dictionary["SUNDAY"] = new LocalizedTexts("SUNDAY", "日曜日");
		dictionary["PP WARNING"] = new LocalizedTexts("* e-amusement participation fee is based on the factory setting.\nEven if you change this setting, e-amusement participation fee does not change.", "※e-amusement participationの料金は、工場出荷時設定の料金に基づきます。\nこの設定を変更して運用しても、e-amusement participationの料金は変わりません。");
		dictionary["PREMIUM WARNING"] = new LocalizedTexts("", "※プレミアムプレーは、ノーマルプレー料金とプレミアムプレー追加料金を合算した料金となります。");
		dictionary["TEST BUTTON"] = new LocalizedTexts("TEST BUTTON", "TESTボタン");
		dictionary["SERVICE BUTTON"] = new LocalizedTexts("SERVICE BUTTON", "SERVICEボタン");
		dictionary["COIN MECH"] = new LocalizedTexts("COIN MECH", "コイン検知");
		dictionary["TRIGGER 1"] = new LocalizedTexts("TRIGGER1", "トリガー1");
		dictionary["TRIGGER 2"] = new LocalizedTexts("TRIGGER2", "トリガー2");
		dictionary["STICK BUTTON"] = new LocalizedTexts("STICK BUTTON", "スティックボタン");
		dictionary["BUTTON 1"] = new LocalizedTexts("BUTTON", "ボタン");
		dictionary["BUTTON 2"] = new LocalizedTexts("SENSOR1", "センサー1");
		dictionary["BUTTON 3"] = new LocalizedTexts("SENSOR2", "センサー2");
		dictionary["BUTTON 4"] = new LocalizedTexts("SENSOR3", "センサー3");
		dictionary["HEADPHONE"] = new LocalizedTexts("HEADPHONE", "ヘッドホン");
		dictionary["10KEY"] = new LocalizedTexts("10KEY", "テンキー");
		dictionary["ALL"] = new LocalizedTexts("ALL", "すべて点灯");
		dictionary["AUTO"] = new LocalizedTexts("AUTO", "自動点灯");
		dictionary["COIN BLOCKER"] = new LocalizedTexts("COIN BLOCKER", "コインブロッカー");
		dictionary["USB CHARGER"] = new LocalizedTexts("USB CHARGER", "USB充電");
		dictionary["CLOSE"] = new LocalizedTexts("CLOSE", "閉じる");
		dictionary["OPEN"] = new LocalizedTexts("OPEN", "開く");
		dictionary["ENABLE"] = new LocalizedTexts("ENABLE", "有効");
		dictionary["DISABLE"] = new LocalizedTexts("DISABLE", "無効");
		dictionary["TOUCH STATUS"] = new LocalizedTexts("TOUCH STATUS:", "タッチ状況：");
		dictionary["RED"] = new LocalizedTexts("RED", "赤");
		dictionary["YELLOW"] = new LocalizedTexts("YELLOW", "黄");
		dictionary["GREEN"] = new LocalizedTexts("GREEN", "緑");
		dictionary["CYAN"] = new LocalizedTexts("CYAN", "シアン");
		dictionary["BLUE"] = new LocalizedTexts("BLUE", "青");
		dictionary["MAGENTA"] = new LocalizedTexts("MAGENTA", "マゼンタ");
		dictionary["WHITE"] = new LocalizedTexts("WHITE", "白");
		dictionary["COLORLESS"] = new LocalizedTexts("COLORLESS", "黒く見える部分");
		dictionary["IC CARD"] = new LocalizedTexts("IC CARD", "ICカード");
		dictionary["TOUCH THE IC CARD TO THE IC CARD READER"] = new LocalizedTexts("TOUCH THE IC CARD TO THE IC CARD READER", "ICカードリーダーにICカードをタッチしてください");
		dictionary["SOUND SCALE CHECK"] = new LocalizedTexts("SOUND SCALE CHECK", "ステレオチェック");
		dictionary["LEFT"] = new LocalizedTexts("LEFT", "左");
		dictionary["RIGHT"] = new LocalizedTexts("RIGHT", "右");
		dictionary["SOUND PHASE CHECK"] = new LocalizedTexts("SOUND PHASE CHECK", "スピーカーチェック");
		dictionary["LOUD"] = new LocalizedTexts("LOUD", "大");
		dictionary["SOFT"] = new LocalizedTexts("SOFT", "小");
		dictionary["SOUND VOLUME"] = new LocalizedTexts("SOUND VOLUME", "音量設定");
		dictionary["HEADPHONE SOUND PHASE CHECK"] = new LocalizedTexts("HEADPHONE SOUND PHASE CHECK", "ヘッドホンチェック");
		dictionary["HEADPHONE SOUND VOLUME"] = new LocalizedTexts("HEADPHONE SOUND VOLUME", "音量設定");
		dictionary["HEADPHONE SOUND VOLUME CHECK"] = new LocalizedTexts("HEADPHONE SOUND VOLUME CHECK", "ヘッドホン音量チェック");
		dictionary["SOUND VOLUME IN ATTRACT MODE"] = new LocalizedTexts("SOUND VOLUME IN ATTRACT MODE", "デモ音量");
		dictionary["ANALOG VOLUME"] = new LocalizedTexts("* The volume can be adjusted by the volume control knob.", "※サウンドボリュームでも調節可能です。");
		dictionary["SHOP CLOSE"] = new LocalizedTexts("SHOP CLOSE", "閉店設定");
		dictionary["SHOP CLOSE TIME"] = new LocalizedTexts("SHOP CLOSE TIME", "営業終了時間");
		dictionary["24-HOUR CLOCK"] = new LocalizedTexts("24-HOUR CLOCK", "24時間表記");
		dictionary["CONTINUE"] = new LocalizedTexts("CONTINUE", "コンティニュー");
		dictionary["NUMBER OF MULTIPLE SHINKI TO BUY"] = new LocalizedTexts("NUMBER OF SHINKI BUY", "神姫複数購入時の数");
		dictionary["NUMBER OF SHINKI TO BUY"] = new LocalizedTexts("NUMBER OF SHINKI TO BUY", "神姫購入数(複数用)");
		dictionary["CABINET ID"] = new LocalizedTexts("MATCHING SEPARATE ID (ForDebug)", "マッチング分けID(デバッグ版のみ)");
		dictionary["BREAK TIME"] = new LocalizedTexts("BREAK TIME", "休憩モード");
		dictionary["BREAK TIME LENGTH"] = new LocalizedTexts("BREAK TIME LENGTH", "休憩モードの長さ");
		dictionary["LED WING"] = new LocalizedTexts("SIDE PANEL LED", "サイドパネルLED");
		dictionary["LED CTRL PANEL"] = new LocalizedTexts("CONTROL PANEL LED", "コントロールパネルLED");
		dictionary["LED QBUTTON"] = new LocalizedTexts("BUTTON LED", "ボタンLED");
		dictionary["LED TITLE"] = new LocalizedTexts("TITLE LED", "タイトルパネルLED");
		dictionary["LED SIDE"] = new LocalizedTexts("SIDE LED", "サイドパネルLED");
		dictionary["FREE PLAY CAMPAIGN"] = new LocalizedTexts("Free Play Campaign", "無料プレーキャンペーン");
		dictionary["MONDAY"] = new LocalizedTexts("MONDAY", "月");
		dictionary["TUESDAY"] = new LocalizedTexts("TUESDAY", "火");
		dictionary["WEDNESDAY"] = new LocalizedTexts("WEDNESDAY", "水");
		dictionary["THURSDAY"] = new LocalizedTexts("THURSDAY", "木");
		dictionary["FRIDAY"] = new LocalizedTexts("FRIDAY", "金");
		dictionary["SATURDAY"] = new LocalizedTexts("SATURDAY", "土");
		dictionary["SUNDAY"] = new LocalizedTexts("SUNDAY", "日");
		dictionary["T1"] = new LocalizedTexts("T1");
		dictionary["T2"] = new LocalizedTexts("T2");
		dictionary["T3"] = new LocalizedTexts("T3");
		dictionary["T4"] = new LocalizedTexts("T4");
		dictionary["T5"] = new LocalizedTexts("T5");
		dictionary["T6"] = new LocalizedTexts("T6");
		dictionary["T0"] = new LocalizedTexts("T0");
		dictionary["CAMERA1"] = new LocalizedTexts("CAMERA 1", "カメラ1");
		dictionary["CAMERA2"] = new LocalizedTexts("CAMERA 2", "カメラ2");
		dictionary["CAMERA3"] = new LocalizedTexts("CAMERA 3", "カメラ3");
		dictionary["BARCODE DETECT OK"] = new LocalizedTexts("DETECT OK", "OK");
		dictionary["BARCODE DETECT NG"] = new LocalizedTexts("DETECT NG", "なし");
		dictionary["READ INTERVAL"] = new LocalizedTexts("CARD READ INTERVAL(ms)", "全スロット読み込み間隔(ms)");
		dictionary["READ INTERVAL UNIT"] = new LocalizedTexts("READ INTERVAL(ms)", "神姫カード読み込み間隔(ms)");
		dictionary["RETRY COUNT"] = new LocalizedTexts("RETRY COUNT", "解析リトライ回数");
		dictionary["BRIGHTNESS"] = new LocalizedTexts("BRIGHTNESS", "輝度");
		dictionary["CHROMA"] = new LocalizedTexts("CHROMA", "彩度");
		dictionary["CONTRAST"] = new LocalizedTexts("CONTRAST", "コントラスト");
		dictionary["CONTRAST OFFSET"] = new LocalizedTexts("CONTRAST OFFSET", "コントラストオフセット");
		dictionary["FREE PLAY"] = new LocalizedTexts("FREE PLAY", "フリープレー");
		dictionary["COIN SLOT"] = new LocalizedTexts("COIN SLOT", "コインスロット");
		dictionary["TO START"] = new LocalizedTexts("TO START");
		dictionary["TO CONTINUE"] = new LocalizedTexts("TO CONTINUE");
		dictionary["TO TIME EXTEND"] = new LocalizedTexts("TO TIME EXTEND");
		dictionary["TO GACHA"] = new LocalizedTexts("TO GACHA");
		dictionary["TO FIGHT"] = new LocalizedTexts("TO START");
		dictionary["TO STORY"] = new LocalizedTexts("TO START");
		dictionary["TO FULLSET"] = new LocalizedTexts("TO START");
		dictionary["GAME"] = new LocalizedTexts("GAME", "本体");
		dictionary["ROUTER"] = new LocalizedTexts("ROUTER", "ルーター");
		dictionary["CENTER"] = new LocalizedTexts("CENTER", "センター");
		dictionary["SERVER"] = new LocalizedTexts("SERVER", "サーバー");
		dictionary["NO/STATUS"] = new LocalizedTexts("STATUS", "接続状態");
		dictionary["CONNECTED"] = new LocalizedTexts("CONNECTED", "接続中");
		dictionary["NOT CONNECTED"] = new LocalizedTexts("NOT CONNECTED", "接続されていません");
		dictionary["GAME TO ROUTER"] = new LocalizedTexts("GAME TO ROUTER", "ルーターとの通信");
		dictionary["GAME TO CENTER"] = new LocalizedTexts("GAME TO CENTER", "センターとの通信");
		dictionary["GAME TO SERVER"] = new LocalizedTexts("GAME TO SERVER", "サーバーとの通信");
		dictionary["IP ADDRESS"] = new LocalizedTexts("IP ADDRESS", "IPアドレス");
		dictionary["SUBNET MASK"] = new LocalizedTexts("SUBNET MASK", "サブネットマスク");
		dictionary["DEFAULT ROUTER"] = new LocalizedTexts("DEFAULT ROUTER", "デフォルトルーター");
		dictionary["DOMAIN NAME"] = new LocalizedTexts("DOMAIN NAME", "ドメイン名");
		dictionary["DNS SERVER"] = new LocalizedTexts("DNS SERVER", "DNSサーバー");
		dictionary["DHCP SERVER"] = new LocalizedTexts("DHCP SERVER", "DHCPサーバー");
		dictionary["NTP SERVER"] = new LocalizedTexts("NTP SERVER", "NTPサーバー");
		dictionary["MAC ADDRESS"] = new LocalizedTexts("MAC ADDRESS", "MACアドレス");
		dictionary["NO/AVAILABLE"] = new LocalizedTexts("AVAILABLE", "利用可");
		dictionary["NO/NOT AVAILABLE"] = new LocalizedTexts("NOT AVAILABLE", "確認中");
		dictionary["NO/DISABLE"] = new LocalizedTexts("DISABLE", "利用不可");
		dictionary["SHOP AREA"] = new LocalizedTexts("SHOP AREA", "店舗地域");
		dictionary["SHOP NAME"] = new LocalizedTexts("SHOP NAME", "店舗名");
		dictionary["GAME MODE(DEFAULT)"] = new LocalizedTexts("GAME MODE(DEFAULT)", "ゲームモード（デフォルト）");
		dictionary["TEST MODE"] = new LocalizedTexts("TEST MODE", "テストモード");
		dictionary["BK/BOOKKEEPING"] = new LocalizedTexts("BOOKKEEPING", "集計");
		dictionary["CLEAR ALL DATA"] = new LocalizedTexts("CLEAR ALL DATA", "データ消去");
		dictionary["TOTAL ACTIVE TIME"] = new LocalizedTexts("TOTAL ACTIVE TIME", "総稼働時間");
		dictionary["TOTAL PLAY TIME"] = new LocalizedTexts("TOTAL PLAY TIME", "総プレー時間");
		dictionary["AVERAGE PLAY TIME"] = new LocalizedTexts("AVERAGE PLAY TIME", "平均プレー時間");
		dictionary["LONGEST PLAY TIME"] = new LocalizedTexts("LONGEST PLAY TIME", "最長プレー時間");
		dictionary["SHORTEST PLAY TIME"] = new LocalizedTexts("LONGEST PLAY TIME", "最短プレー時間");
		dictionary["BK/COUNT01"] = new LocalizedTexts("TOTAL PLAY COUNT", "総プレー回数");
		dictionary["BK/COUNT02"] = new LocalizedTexts("TOTAL GACHA COUNT", "総ガールズカットインガチャ回数");
		dictionary["BK/COUNT03"] = new LocalizedTexts("GACHA1 COUNT", "ガールズカットインガチャ(1回)回数");
		dictionary["BK/COUNT04"] = new LocalizedTexts("GACHA5 COUNT", "ガールズカットインガチャ(5回)回数");
		dictionary["BK/COUNT05"] = new LocalizedTexts("MUSICGACHA COUNT", "リーチソング＆ボイスガチャ回数");
		dictionary["H"] = new LocalizedTexts("H", "時間");
		dictionary["M"] = new LocalizedTexts("M", "分");
		dictionary["S"] = new LocalizedTexts("S", "秒");
		dictionary["SET TIME"] = new LocalizedTexts("SET TIME", "時刻");
		dictionary["CLOCK NOT SET"] = new LocalizedTexts("CLOCK NOT SET", "未設定");
		dictionary["VC/STATUS"] = new LocalizedTexts("STATUS", "利用状態");
		dictionary["VC/AVAILABLE"] = new LocalizedTexts("AVAILABLE", "利用可");
		dictionary["VC/NOT AVAILABLE"] = new LocalizedTexts("NOT AVAILABLE", "利用不可");
		dictionary["TIME SERVICE"] = new LocalizedTexts("TIME SERVICE", "タイムサービス");
		dictionary["SERVICE VALUE"] = new LocalizedTexts("SERVICE VALUE", "サービス価格");
		dictionary["SERVICE LIMIT"] = new LocalizedTexts("SERVICE LIMIT", "サービス制限");
		dictionary["PLAY FOR EACH DAY"] = new LocalizedTexts("PLAY FOR EACH DAY", "プレー / 1日毎");
		dictionary["SERVICE REMAIN"] = new LocalizedTexts("SERVICE REMAIN", "サービス残り");
		dictionary["PLAY"] = new LocalizedTexts("PLAY", "プレー");
		dictionary["SERVICE TIME"] = new LocalizedTexts("SERVICE TIME", "サービス時間");
		dictionary["BASIC"] = new LocalizedTexts("BASIC", "通常");
		dictionary["COPY FROM PATTERN"] = new LocalizedTexts("COPY FROM PATTERN", "他の料金パターンからコピー");
		dictionary["EXTRA PASELI"] = new LocalizedTexts("EXTRA PASELI", "EXTRA PASELI");
		dictionary["CLEAR"] = new LocalizedTexts("CLEAR", "初期化");
		dictionary["SET AND EXIT"] = new LocalizedTexts("SET AND EXIT", "ゲームに反映して終了");
		dictionary["NOT CLEARED:"] = new LocalizedTexts("NOT CLEARED:", "以下は初期化されません");
		dictionary["BOOKKEEPING DATA"] = new LocalizedTexts("BOOKKEEPING DATA", "集計データ");
		dictionary["AF/CLOCK"] = new LocalizedTexts("CLOCK", "設定した時刻");
		dictionary["SUPER FACTORY REBOOT"] = new LocalizedTexts("INITALIZING THE BACKUP DATA COMPLETE. PLEASE TURN OFF THE SWITCH AND REBOOT.", "バックアップデータを初期化しました\n電源を切り、再起動してください");
		LocalizedTextsDictionary = dictionary;
		// decompiler artifact removed: it emitted the base-class constructor call as
		// "base._002Ector();" at the end of the body; C# issues that call implicitly.
	}
}
