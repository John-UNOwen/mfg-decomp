using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class BingoResultWindow : MonoBehaviour
{
	[SerializeField]
	private Animator resultAnimation;

	[SerializeField]
	private GameObject windowObj;

	[SerializeField]
	private GameObject bingoComplete;

	[SerializeField]
	private Transform bingoSheetParent1;

	[SerializeField]
	private Transform bingoSheetParent2;

	[SerializeField]
	private BingoSheet m_bingoSheet;

	[SerializeField]
	private BingoSheet m_bingoSheetNext;

	[SerializeField]
	private List<Sprite> bingoLineSpriteList;

	[SerializeField]
	private Image bingoCompleteLineNum;

	[SerializeField]
	private TextMeshProUGUI nowGetFryPanNum;

	[SerializeField]
	private TextMeshProUGUI havingFryPanNum;

	[SerializeField]
	private GameObject bonusFrame;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private Button changeButton;

	private int getFryPanNum;

	private int beforeFryPanNum;

	private int afterFryPanNum;

	private bool isChange;

	[NonSerialized]
	public bool IsClose;

	private async void Start()
	{
		changeButton.OnClickAsObservable().Subscribe(async delegate
		{
			await CardChange();
		});
		closeButton.OnClickAsObservable().Subscribe(delegate
		{
			Close().Forget();
		});
	}

	private void Update()
	{
		RefleshText();
		if (ResidentFlow.GetInstance().GetTimer().IsEnd() || GameUtility.IsAppTerminatDecided || Cloudstrap.IsDisconnect)
		{
			Close().Forget();
		}
	}

	public async UniTask OpenWindow(CancellationToken cancellationToken = default(CancellationToken))
	{
		m_bingoSheet.CardSetUp(GameDataManager.BingoEventRemake.BingoCardYakuList);
		for (int i = 0; i < 9; i++)
		{
			if (GameDataManager.BingoEventRemake.BingoScoreHoles[i] >= 0 && !GameDataManager.BingoEventRemake.ThisPlayCleardHole[i])
			{
				m_bingoSheet.PlayClearLoopHoleEffect(i, GameDataManager.BingoEventRemake.BingoScoreHoles[i]);
			}
		}
		GameDataManager.BingoEventRemake.AfterBingoLine = GameDataManager.BingoEventRemake.GetBingoLines();
		for (int j = 0; j < 8; j++)
		{
			_ = GameDataManager.BingoEventRemake.BeforeBingoLine[j];
		}
		resultAnimation.SetTrigger("In");
		await UniTask.WaitUntil(() => resultAnimation.GetCurrentAnimatorStateInfo(0).IsName("CompeScoreAllLoop"), PlayerLoopTiming.Update, cancellationToken);
		await SoundManager.PlaySe("SFX_EVT_BINGO1");
		windowObj.GetComponent<Animator>().SetTrigger("In");
		await UniTask.Delay(TimeSpan.FromSeconds(1.0));
		bool flag = false;
		for (int k = 0; k < 9; k++)
		{
			if (GameDataManager.BingoEventRemake.BingoScoreHoles[k] >= 0 && GameDataManager.BingoEventRemake.ThisPlayCleardHole[k] && m_bingoSheet.PlayClearHoleEffect(k, GameDataManager.BingoEventRemake.BingoScoreHoles[k]))
			{
				flag = true;
			}
		}
		if (flag)
		{
			SoundManager.PlaySe("SFX_EVT_BINGO3").Forget();
			await UniTask.Delay(TimeSpan.FromSeconds(2.0));
		}
		int thisPlayBingoLineCount = GameDataManager.BingoEventRemake.AfterBingoLine.Count((bool x) => x) - GameDataManager.BingoEventRemake.BeforeBingoLine.Count((bool x) => x);
		getFryPanNum = GameDataManager.BingoEventRemake.GetAddFryPanNum();
		beforeFryPanNum = GameDataManager.BingoEventRemake.GetFryPanNum;
		nowGetFryPanNum.text = getFryPanNum.ToString("F0");
		havingFryPanNum.text = beforeFryPanNum.ToString("F0");
		bonusFrame.SetActive(value: false);
		if (thisPlayBingoLineCount > 0)
		{
			GameDataManager.BingoEventRemake.AddFryPan();
			await SoundManager.PlaySe("SFX_EVT_BINGO4");
			bingoCompleteLineNum.sprite = bingoLineSpriteList[thisPlayBingoLineCount - 1];
			bingoComplete.GetComponent<Animator>().SetTrigger("In");
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResultFlow.Skip(), PlayerLoopTiming.Update, cancellationToken));
		}
		windowObj.GetComponent<Animator>().SetTrigger("FrypanResult");
		await UniTask.WaitUntil(() => windowObj.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("BingCardFrypanLoop"), PlayerLoopTiming.Update, cancellationToken);
		await CountUp(cancellationToken);
		closeButton.gameObject.SetActive(value: true);
		ResidentFlow.GetInstance().GetTimer().SetTimer((GameUtility.IsAppTerminatDecided || Cloudstrap.IsDisconnect) ? 3 : 20, disp: true);
		ResidentFlow.GetInstance().GetTimer().StartTimer();
		if (GameDataManager.BingoEventRemake.BingoScoreHoles.All((int x) => x >= 0))
		{
			await CardChange();
		}
		else
		{
			changeButton.gameObject.SetActive(value: true);
		}
	}

	private async UniTask CardChange()
	{
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
		changeButton.gameObject.SetActive(value: false);
		isChange = true;
		GameDataManager.BingoEventRemake.Reset();
		GameDataManager.BingoEventRemake.MakeBingoCard();
		m_bingoSheetNext.CardSetUp(GameDataManager.BingoEventRemake.BingoCardYakuList);
		if (GameDataManager.BingoEventRemake.BingoScoreHoles[4] >= 0)
		{
			m_bingoSheetNext.PlayClearLoopHoleEffect(4, 0);
		}
		SoundManager.PlaySe("SFX_EVT_BINGO2").Forget();
		windowObj.GetComponent<Animator>().SetTrigger("Change");
		await UniTask.Delay(TimeSpan.FromSeconds(0.5833333134651184));
		SoundManager.PlaySe("SFX_EVT_BINGO1").Forget();
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
	}

	private async UniTask Close()
	{
		if (!IsClose)
		{
			IsClose = true;
			ResidentFlow.GetInstance().GetTimer().SetDisp(disp: false);
			changeButton.interactable = false;
			SoundManager.PlaySe("SFX_EVT_BINGO2").Forget();
			windowObj.GetComponent<Animator>().SetTrigger(isChange ? "ChangeEnd" : "End");
			resultAnimation.SetTrigger("Out");
			await UniTask.Delay(TimeSpan.FromSeconds(1.0));
		}
	}

	private async UniTask CountUp(CancellationToken cancellationToken = default(CancellationToken))
	{
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(5.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResultFlow.Skip(), PlayerLoopTiming.Update, cancellationToken));
		afterFryPanNum = GameDataManager.BingoEventRemake.GetFryPanNum;
		await FryPanNumCountUp(cancellationToken);
		getFryPanNum = 0;
		beforeFryPanNum = afterFryPanNum;
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(3.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResultFlow.Skip(), PlayerLoopTiming.Update, cancellationToken));
	}

	private async UniTask FryPanNumCountUp(CancellationToken cancellationToken = default(CancellationToken))
	{
		float countupFlame = 60f;
		float addRate = (float)getFryPanNum / countupFlame;
		float getWingFloat = getFryPanNum;
		float beforeFloat = beforeFryPanNum;
		SoundManager.PlaySe("SFX_SYS_METER1").Forget();
		float i = getFryPanNum;
		while (i >= 1f && !ResultFlow.Skip())
		{
			await UniTask.Delay(TimeSpan.FromSeconds(countupFlame / 60f / 60f), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
			getWingFloat -= addRate;
			beforeFloat += addRate;
			getFryPanNum = (int)Math.Floor(getWingFloat);
			beforeFryPanNum = (int)Math.Ceiling(beforeFloat);
			i -= addRate;
		}
		SoundManager.StopSe("SFX_SYS_METER1").Forget();
	}

	private void RefleshText()
	{
		nowGetFryPanNum.text = getFryPanNum.ToString("F0");
		havingFryPanNum.text = beforeFryPanNum.ToString("F0");
	}
}
