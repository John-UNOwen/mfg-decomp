using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MyRoomFriendConfirmWindow : MonoBehaviour
{
	[SerializeField]
	private Button decideButton;

	[SerializeField]
	private Button cancelButton;

	[SerializeField]
	private FriendListItem friendList;

	private MyRoomFlow myRoomFlow;

	private FriendInfo decideFriendInfo;

	private bool isConfirmOpen;

	public void Init(MyRoomFlow mrf)
	{
		myRoomFlow = mrf;
		decideButton.OnClickAsObservable().Subscribe(async delegate
		{
			cancelButton.interactable = false;
			await myRoomFlow.OdekakeDecide(decideFriendInfo);
		}).AddTo(this);
		cancelButton.OnClickAsObservable().Subscribe(delegate
		{
			base.transform.GetComponent<Animator>().SetTrigger("Out");
			base.gameObject.SetActive(value: false);
			decideFriendInfo = new FriendInfo();
			isConfirmOpen = false;
		}).AddTo(this);
	}

	public async UniTask Play(FriendInfo friend, CancellationToken cancellationToken)
	{
		if (!isConfirmOpen)
		{
			friendList.Init(friend).Forget();
			base.gameObject.SetActive(value: true);
			base.transform.GetComponent<Animator>().SetTrigger("In");
			decideFriendInfo = friend;
			isConfirmOpen = true;
			await UniTask.WaitWhile(() => isConfirmOpen, PlayerLoopTiming.Update, cancellationToken);
		}
	}
}
