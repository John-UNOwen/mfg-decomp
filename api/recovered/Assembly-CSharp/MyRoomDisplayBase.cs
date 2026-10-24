using System;
using System.Threading;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MyRoomDisplayBase : MonoBehaviour
{
	[SerializeField]
	protected Button btDetail;

	protected MyRoomFlow myRoomFlow;

	protected bool detailInteractable;

	public MyRoomFlow.WindowType windowType;

	public virtual void SetStatus(CancellationToken cancellationToken)
	{
	}

	public virtual void Init(MyRoomFlow mrf, MyRoomDetailWindow pageAll, CancellationToken cancellationToken, bool isProEnable = true)
	{
		myRoomFlow = mrf;
		SetStatus(cancellationToken);
		btDetail.interactable = !GameDataManager.Account.IsGuestUser && (isProEnable || !GameDataManager.Player.IsPro());
		btDetail.OnClickAsObservable().Subscribe(async delegate
		{
			try
			{
				await pageAll.WindowOpen(windowType, cancellationToken, this);
				SetStatus(cancellationToken);
			}
			catch (OperationCanceledException)
			{
				Debug.Log($"{windowType}.Onclick Canceled");
			}
		}).AddTo(this);
	}
}
