using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MFG.GameData;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ShopCompetitionResultWindow : MonoBehaviour
{
	[SerializeField]
	private GameObject windowObj;

	[SerializeField]
	private Button btClose;

	[SerializeField]
	private Animator scoreAnimator;

	[SerializeField]
	private TextMeshProUGUI shopNameText;

	[SerializeField]
	private TextMeshProUGUI taikyokuScoreText;

	[SerializeField]
	private TextMeshProUGUI shopRank;

	[SerializeField]
	private TextMeshProUGUI shopNewRank;

	[SerializeField]
	private TextMeshProUGUI shopScore;

	[SerializeField]
	private TextMeshProUGUI playerRank;

	[SerializeField]
	private TextMeshProUGUI playerNewRank;

	[SerializeField]
	private TextMeshProUGUI playerScore;

	private bool isOpen;

	protected TimeoutController _timeoutController = new TimeoutController();

	public bool IsOpen => isOpen;

	private void Start()
	{
		btClose.OnClickAsObservable().Subscribe(delegate
		{
			Close().Forget();
		}).AddTo(this);
	}

	public async UniTask Open(ShopCompetitionInfo beforeInfo, ShopCompetitionInfo afterInfo, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (isOpen)
		{
			return;
		}
		isOpen = true;
		shopNameText.text = afterInfo.ShopName;
		taikyokuScoreText.text = afterInfo.TaikyokuScore.ToString("N0");
		shopRank.text = ((beforeInfo.ShopRank <= 0) ? "―" : beforeInfo.ShopRank.ToString("N0"));
		shopNewRank.text = ((afterInfo.ShopRank <= 0) ? "―" : afterInfo.ShopRank.ToString("N0"));
		shopScore.text = beforeInfo.ShopScore.ToString("N0");
		playerRank.text = ((beforeInfo.PersonalRank <= 0) ? "―" : beforeInfo.PersonalRank.ToString("N0"));
		playerNewRank.text = ((afterInfo.PersonalRank <= 0) ? "―" : afterInfo.PersonalRank.ToString("N0"));
		playerScore.text = beforeInfo.PersonalScore.ToString("N0");
		CancellationToken token2 = _timeoutController.Timeout(TimeSpan.FromSeconds((GameUtility.IsAppTerminatDecided || Cloudstrap.IsDisconnect) ? 1 : 20));
		using CancellationTokenSource linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, token2);
		CancellationToken token = linkedCancellationTokenSource.Token;
		try
		{
			btClose.gameObject.SetActive(value: false);
			base.gameObject.SetActive(value: true);
			windowObj.GetComponent<Animator>().SetTrigger("In");
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, token), UniTask.WaitUntil(() => ResultFlow.Skip(), PlayerLoopTiming.Update, token));
			scoreAnimator.SetTrigger("TaikyokuScoreIn");
			await UniTask.WhenAny(UniTask.WaitUntil(() => scoreAnimator.GetCurrentAnimatorStateInfo(0).IsName("ScoreWait"), PlayerLoopTiming.Update, token), UniTask.WaitUntil(() => ResultFlow.Skip(), PlayerLoopTiming.Update, token));
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, token), UniTask.WaitUntil(() => ResultFlow.Skip(), PlayerLoopTiming.Update, token));
			List<Tweener> tweenerList = new List<Tweener>();
			float time = Math.Min(1.5f, 0.01f * (float)Math.Abs(afterInfo.TaikyokuScore));
			tweenerList.Add(DOVirtual.Float(afterInfo.TaikyokuScore, 0f, time, delegate(float value)
			{
				taikyokuScoreText.text = ((int)value).ToString("N0");
			}).OnComplete(delegate
			{
				taikyokuScoreText.text = "0";
			}));
			int subScore = afterInfo.TaikyokuScore;
			if (beforeInfo.PersonalScore + afterInfo.TaikyokuScore < 0)
			{
				subScore = -beforeInfo.PersonalScore;
			}
			tweenerList.Add(DOVirtual.Float(0f, subScore, time, delegate(float value)
			{
				shopScore.text = Math.Max(beforeInfo.ShopScore + (int)value, 0).ToString("N0");
			}).OnComplete(delegate
			{
				shopScore.text = Math.Max(beforeInfo.ShopScore + subScore, 0).ToString("N0");
			}));
			tweenerList.Add(DOVirtual.Float(0f, subScore, time, delegate(float value)
			{
				playerScore.text = Math.Max(beforeInfo.PersonalScore + (int)value, 0).ToString("N0");
			}).OnComplete(delegate
			{
				playerScore.text = Math.Max(beforeInfo.PersonalScore + subScore, 0).ToString("N0");
			}));
			await SoundManager.PlaySe("SFX_MJG_POINT1");
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(time), ignoreTimeScale: false, PlayerLoopTiming.Update, token), UniTask.WaitUntil(() => ResultFlow.Skip(), PlayerLoopTiming.Update, token));
			SoundManager.StopSe("SFX_MJG_POINT1").Forget();
			SoundManager.PlaySe("SFX_MJG_POINT2").Forget();
			if (tweenerList.Count > 0)
			{
				foreach (Tweener item in tweenerList)
				{
					item.Complete();
				}
			}
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, token), UniTask.WaitUntil(() => ResultFlow.Skip(), PlayerLoopTiming.Update, token));
			scoreAnimator.SetTrigger("NewRankIn");
			await UniTask.WhenAny(UniTask.WaitUntil(() => scoreAnimator.GetCurrentAnimatorStateInfo(0).IsName("AfterWait"), PlayerLoopTiming.Update, token), UniTask.WaitUntil(() => ResultFlow.Skip(), PlayerLoopTiming.Update, token));
			btClose.gameObject.SetActive(value: true);
			await UniTask.WaitWhile(() => IsOpen, PlayerLoopTiming.Update, token);
		}
		catch (OperationCanceledException)
		{
			if (_timeoutController.IsTimeout())
			{
				Debug.Log("_timeoutController.IsTimeout()");
				Close().Forget();
			}
		}
		finally
		{
			_timeoutController.Reset();
		}
	}

	public async UniTask Close()
	{
		if (isOpen)
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			windowObj.GetComponent<Animator>().SetTrigger("Out");
			base.gameObject.SetActive(value: false);
			isOpen = false;
			await UniTask.Delay(TimeSpan.FromSeconds(1.0));
		}
	}
}
