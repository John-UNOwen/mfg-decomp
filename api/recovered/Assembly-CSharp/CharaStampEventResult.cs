using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.Types.CharaStamp;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class CharaStampEventResult : MonoBehaviour
{
	[SerializeField]
	private GameObject windowObj;

	[SerializeField]
	private GameObject eventScoreObj;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private GameObject stampParent;

	[SerializeField]
	private CharaStampID charaStampID;

	[SerializeField]
	private GameObject charaGetObj;

	public bool isOpen;

	private CharaStampInfo info;

	private bool isClear;

	private bool isJandri;

	private Animator stampAnim;

	private Animator stampAnim2;

	private async void Start()
	{
		info = GameDataManager.CharaStampEvent.GetInfo(charaStampID);
		if (info != null)
		{
			for (int i = 0; i < info.StampNum; i++)
			{
				stampParent.transform.Find($"Stamp{i + 1}").GetComponent<Animator>().SetTrigger("Loop");
			}
			isJandri = GameDataManager.Mission.IsMissionProgressBoost;
			int num = info.Progress(isJandri);
			isClear = info.IsUsable;
			stampAnim = stampParent.transform.Find($"Stamp{info.StampNum}").GetComponent<Animator>();
			if (num == 2)
			{
				stampAnim2 = stampParent.transform.Find($"Stamp{info.StampNum - 1}").GetComponent<Animator>();
			}
		}
		closeButton.OnClickAsObservable().Subscribe(delegate
		{
			CloseWindow().Forget();
		}).AddTo(this);
	}

	public async UniTask OpenWindow(CancellationToken cancellationToken)
	{
		if (isOpen)
		{
			return;
		}
		isOpen = true;
		base.gameObject.SetActive(value: true);
		windowObj.GetComponent<Animator>().SetTrigger("In");
		await UniTask.WaitUntil(() => windowObj.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("CompeScoreAllLoop"), PlayerLoopTiming.Update, cancellationToken);
		stampAnim.SetTrigger("In");
		if (stampAnim2 != null)
		{
			stampAnim2.SetTrigger("In");
		}
		await UniTask.WaitUntil(() => stampAnim.GetCurrentAnimatorStateInfo(0).IsName("CharaStampLoop"), PlayerLoopTiming.Update, cancellationToken);
		if (isClear)
		{
			charaGetObj.SetActive(value: true);
			charaGetObj.GetComponent<Animator>().SetTrigger("In");
			SoundManager.PlaySe("SFX_SYS_BONUS5").Forget();
			await UniTask.WaitUntil(() => charaGetObj.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("CharaGetAnimeLoop"), PlayerLoopTiming.Update, cancellationToken);
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(2.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResultFlow.Skip(), PlayerLoopTiming.Update, cancellationToken));
			charaGetObj.GetComponent<Animator>().SetTrigger("End");
		}
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(8.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResultFlow.Skip(), PlayerLoopTiming.Update, cancellationToken));
		closeButton.gameObject.SetActive(value: true);
		await CloseWindow(cancellationToken);
	}

	public async UniTask CloseWindow(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (isOpen)
		{
			SoundManager.StopSeAll().Forget();
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			windowObj.GetComponent<Animator>().SetTrigger("Out");
			isOpen = false;
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResultFlow.Skip(), PlayerLoopTiming.Update, cancellationToken));
		}
	}
}
