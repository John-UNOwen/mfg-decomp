using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using UniRx;
using UnityEngine;

public class MyRoomFriendWindow : MonoBehaviour
{
	[SerializeField]
	private Transform friendList;

	[SerializeField]
	private GameObject friendListPrefab;

	[SerializeField]
	private GameObject unableWindow;

	public async UniTask Init(MyRoomDetailWindow pageAll, CancellationToken cancellationToken)
	{
		foreach (Transform friend2 in friendList)
		{
			UnityEngine.Object.Destroy(friend2.gameObject);
		}
		IEnumerable<FriendInfo> enumerable = GameDataManager.Friend.FriendInfos?.Where((FriendInfo x) => x.IsUsable);
		if (!GameDataManager.Player.IsPro() && enumerable != null && enumerable.Count() > 0)
		{
			unableWindow.SetActive(value: false);
			List<UniTask> list = new List<UniTask>();
			foreach (FriendInfo friend in enumerable)
			{
				FriendListItem component = UnityEngine.Object.Instantiate(friendListPrefab, friendList).transform.GetComponent<FriendListItem>();
				list.Add(component.Init(friend));
				component.decideButton.OnClickAsObservable().Subscribe(async delegate
				{
					try
					{
						await pageAll.WindowOpen(MyRoomFlow.WindowType.FriendConfirmWindow, cancellationToken, friend);
					}
					catch (OperationCanceledException)
					{
						Debug.Log("MyRoomFriendWindow: OperationCanceledException Catched.");
					}
				}).AddTo(this);
			}
			await list;
		}
		else
		{
			unableWindow.SetActive(value: true);
		}
	}
}
