using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameRequest;
using MFG.Types;
using MFG.Types.Notice;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class NoticeGameData
{
	private const int CurrentDataVersion = 1;

	public int DataVersion = 1;

	public const string ClientStateName = "notice_game";

	public List<NoticeInfo> NoticeInfos = new List<NoticeInfo>();

	[NonSerialized]
	public List<MemorialPrintNoticeInfo> MemorialPrintNoticeInfos = new List<MemorialPrintNoticeInfo>();

	[NonSerialized]
	public List<MissionMemorialPrintNoticeInfo> MissionMemorialPrintNoticeInfos = new List<MissionMemorialPrintNoticeInfo>();

	[NonSerialized]
	public List<MusicAcquireInfo> MusicAcquireInfos = new List<MusicAcquireInfo>();

	[NonSerialized]
	public List<CharaType> InfoNewGirlNoticeFromGacha = new List<CharaType>();

	[NonSerialized]
	public CharaType PickupGachaNoticeCharaType = (CharaType)(-1);

	[NonSerialized]
	public string PickupGachaNoticeDate = "";

	private static readonly List<NoticeInfo> _debugNoticeInfos = new List<NoticeInfo>
	{
		new NoticeInfo
		{
			NoticeID = 0,
			Category = Category.GachaBanner
		},
		new NoticeInfo
		{
			NoticeID = 1,
			Category = Category.GachaBanner
		},
		new NoticeInfo
		{
			NoticeID = 2,
			Category = Category.GachaBanner
		},
		new NoticeInfo
		{
			NoticeID = 3,
			Category = Category.Update,
			Title = "アップデート1",
			Text = "テスト\nVer2.1.5のアプリを配信いたしました。\n\nアップデート内容\n\n・「ボイス機能」を拡張し、音声などが切り替わるシーンを追加しました。\n・今後のイベントに向けての事前対応を行いました。\n・その他一部機能の改善を行いました。\n\n\n12/19(月)7:00以降はver2.1.4以下ではプレーできなくなりますので、それまでにアップデートを完了していただくようお願いいたします。\n※電波環境の良い所でアップデートをお願いいたします。通信回線の状態によって、ダウンロードに時間がかかる場合がございます。Wi-Fi環境でのご利用を推奨いたします。\n※iOSにおいて、AppStoreで「開く」と表示されてしまう場合はこちらをご覧ください。\n                ",
			ReleaseDate = "2022/12/01 13:00",
			UpdateDate = null,
			IsNew = true
		},
		new NoticeInfo
		{
			NoticeID = 4,
			Category = Category.Update,
			Title = "アップデート2",
			Text = "アップデートをします",
			ReleaseDate = "2022/12/02 13:00",
			UpdateDate = null,
			IsNew = false
		},
		new NoticeInfo
		{
			NoticeID = 5,
			Category = Category.Failure,
			Title = "不具合1",
			Text = "不具合を修正しました",
			ReleaseDate = "2022/12/03 13:00",
			UpdateDate = "2022/12/27 09:00",
			IsNew = true
		},
		new NoticeInfo
		{
			NoticeID = 6,
			Category = Category.Information,
			Title = "インフォメーション1",
			Text = "インフォメーションがあります",
			ReleaseDate = "2022/12/04 13:00",
			UpdateDate = null,
			IsNew = true
		},
		new NoticeInfo
		{
			NoticeID = 7,
			Category = Category.Gacha,
			Title = "ガチャ1",
			Text = "ガチャがあります",
			ReleaseDate = "2022/12/05 13:00",
			UpdateDate = null,
			IsNew = true
		},
		new NoticeInfo
		{
			NoticeID = 8,
			Category = Category.Campaign,
			Title = "キャンペーン1",
			Text = "キャンペーン1をします",
			ReleaseDate = "2022/12/06 13:00",
			UpdateDate = null,
			IsNew = true
		},
		new NoticeInfo
		{
			NoticeID = 9,
			Category = Category.Campaign,
			Title = "キャンペーン2",
			Text = "キャンペーン2をします",
			ReleaseDate = "2022/12/07 13:00",
			UpdateDate = null,
			IsNew = false
		},
		new NoticeInfo
		{
			NoticeID = 10,
			Category = Category.Campaign,
			Title = "キャンペーン3",
			Text = "キャンペーン3をします",
			ReleaseDate = "2022/12/08 13:00",
			UpdateDate = null,
			IsNew = false
		}
	};

	public async UniTask Save(CancellationToken cancellationToken = default(CancellationToken))
	{
		string text = JsonUtility.ToJson(this);
		Debug.Log("NoticeGameData Save : " + text);
		await RequestApiBase<ClientStateWrite, ClientStateWrite.ResultData, (string, string)>.Send(("notice_game", text), enableErrorScreenTransition: false, enableSendLog: true, cancellationToken);
	}

	public static NoticeGameData LoadFromJson(string jsonString)
	{
		Debug.Log("NoticeGameData Load : " + jsonString);
		NoticeGameData noticeGameData = JsonUtility.FromJson<NoticeGameData>(jsonString);
		if (noticeGameData == null)
		{
			noticeGameData = new NoticeGameData();
		}
		noticeGameData.Validate();
		return noticeGameData;
	}

	private void Validate()
	{
	}

	public static List<NoticeInfo> GenerateDubugNoticeInfos()
	{
		return new List<NoticeInfo>
		{
			GenerateDubugNoticeInfo(0),
			GenerateDubugNoticeInfo(1),
			GenerateDubugNoticeInfo(2)
		};
	}

	private static NoticeInfo GenerateDubugNoticeInfo(int seriesID)
	{
		return new NoticeInfo
		{
			NoticeID = seriesID
		};
	}
}
