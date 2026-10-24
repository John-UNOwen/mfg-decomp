using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameRequest;
using MFG.Types;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class PresentBoxGameData
{
	private const int CurrentDataVersion = 1;

	public int DataVersion = 1;

	public const string ClientStateName = "present_box_game";

	public List<PresentInfo> PresentInfos = new List<PresentInfo>();

	private static readonly List<PresentInfo> _debugPresentInfos = new List<PresentInfo>
	{
		new PresentInfo
		{
			PresentID = 0uL,
			Title = "プレゼント0",
			Text = "不具合のお詫びです",
			LimitDate = "2022/12/01 13:00",
			ReceivedDate = null
		},
		new PresentInfo
		{
			PresentID = 1uL,
			Title = "プレゼント1",
			Text = "不具合のお詫びです",
			LimitDate = "2022/12/02 13:00",
			ReceivedDate = null
		},
		new PresentInfo
		{
			PresentID = 2uL,
			Title = "プレゼント2",
			Text = "不具合のお詫びです",
			LimitDate = "2022/12/03 13:00",
			ReceivedDate = null
		},
		new PresentInfo
		{
			PresentID = 3uL,
			Title = "プレゼント3",
			Text = "ログインボーナスです",
			LimitDate = "2022/12/04 13:00",
			ReceivedDate = null
		},
		new PresentInfo
		{
			PresentID = 4uL,
			Title = "プレゼント4",
			Text = "ログインボーナスです",
			LimitDate = "2022/12/05 13:00",
			ReceivedDate = null
		},
		new PresentInfo
		{
			PresentID = 5uL,
			Title = "プレゼント5",
			Text = "ログインボーナスです",
			LimitDate = "2022/12/06 13:00",
			ReceivedDate = null
		},
		new PresentInfo
		{
			PresentID = 6uL,
			Title = "プレゼント6",
			Text = "ログインボーナスです",
			LimitDate = "2022/12/07 13:00",
			ReceivedDate = null
		},
		new PresentInfo
		{
			PresentID = 7uL,
			Title = "プレゼント7",
			Text = "ログインボーナスです",
			LimitDate = "2022/12/08 13:00",
			ReceivedDate = null
		},
		new PresentInfo
		{
			PresentID = 8uL,
			Title = "プレゼント8",
			Text = "ログインボーナスです",
			LimitDate = "2022/12/09 13:00",
			ReceivedDate = null
		},
		new PresentInfo
		{
			PresentID = 9uL,
			Title = "プレゼント9",
			Text = "ログインボーナスです",
			LimitDate = "2022/12/10 13:00",
			ReceivedDate = null
		},
		new PresentInfo
		{
			PresentID = 10uL,
			Title = "プレゼント10",
			Text = "ログインボーナスです",
			LimitDate = "2022/12/11 13:00",
			ReceivedDate = null
		},
		new PresentInfo
		{
			PresentID = 11uL,
			Title = "プレゼント11",
			Text = "ログインボーナスです",
			LimitDate = "2022/12/12 13:00",
			ReceivedDate = null
		},
		new PresentInfo
		{
			PresentID = 12uL,
			Title = "プレゼント12",
			Text = "ログインボーナスです",
			LimitDate = "2022/12/13 13:00",
			ReceivedDate = null
		},
		new PresentInfo
		{
			PresentID = 13uL,
			Title = "プレゼント13",
			Text = "ログインボーナスです",
			LimitDate = "2022/12/14 13:00",
			ReceivedDate = null
		}
	};

	public async UniTask Save(CancellationToken cancellationToken = default(CancellationToken))
	{
		string text = JsonUtility.ToJson(this);
		Debug.Log("PresentBoxGameData Save : " + text);
		await RequestApiBase<ClientStateWrite, ClientStateWrite.ResultData, (string, string)>.Send(("present_box_game", text), enableErrorScreenTransition: false, enableSendLog: true, cancellationToken);
	}

	public static PresentBoxGameData LoadFromJson(string jsonString)
	{
		Debug.Log("PresentBoxGameData Load : " + jsonString);
		PresentBoxGameData presentBoxGameData = JsonUtility.FromJson<PresentBoxGameData>(jsonString);
		if (presentBoxGameData == null)
		{
			presentBoxGameData = new PresentBoxGameData();
		}
		presentBoxGameData.Validate();
		return presentBoxGameData;
	}

	private void Validate()
	{
	}

	public void Apply(List<PresentDone.PresentInfo> donePresentInfos)
	{
		foreach (PresentDone.PresentInfo donePresentInfo in donePresentInfos)
		{
			int num = PresentInfos.FindIndex((PresentInfo x) => x.PresentID == donePresentInfo.PresentID);
			if (num < 0)
			{
				Debug.LogWarning($"PresentBoxGameData.Apply(): 該当するPresentが見つかりませんでした。 PresentID:{donePresentInfo.PresentID}");
				continue;
			}
			if (donePresentInfo.IsSuccess)
			{
				Debug.LogWarning($"PresentBoxGameData.Apply(): サーバーでの獲得処理に失敗しました。 PresentID:{donePresentInfo.PresentID}");
				continue;
			}
			PresentInfo presentInfo = PresentInfos[num];
			if (donePresentInfo.IsSuccess)
			{
				presentInfo.IsReceived = true;
			}
		}
	}

	public async UniTask AutoAssignment()
	{
		List<PresentInfo> list = PresentInfos.Where((PresentInfo a) => !a.IsReceived && a.IsAutoApply).ToList();
		if (list == null || list.Count <= 0)
		{
			return;
		}
		string receivedDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
		List<int> list2 = new List<int>();
		foreach (PresentInfo item in list)
		{
			ItemUtility.AddItem(item.Content, item.Count, CharaType.Chara01, item.LimitDate);
			item.IsReceived = true;
			item.ReceivedDate = receivedDate;
			list2.Add((int)item.PresentID);
		}
		await SaveTask(list2);
	}

	public async UniTask SaveTask(List<int> receivedPresents)
	{
		if (receivedPresents != null && receivedPresents.Count > 0)
		{
			await GameDataManager.PlayerItem.Save();
			await GameDataManager.PlayerConfig.Save();
			await GameDataManager.PlayerPaidItem.Save();
			await GameDataManager.Customize.Save();
			await GameDataManager.EventTaku.Save();
			await GameDataManager.Vote.Save();
			await GameDataManager.BingoEventRemake.Save();
			Apply((await RequestApiBase<PresentDone, PresentDone.ResultData, List<int>>.Send(receivedPresents)).PresentInfos);
		}
	}
}
