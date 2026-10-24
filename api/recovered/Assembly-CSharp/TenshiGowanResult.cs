using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class TenshiGowanResult : MonoBehaviour
{
	[SerializeField]
	private GameObject windowObj;

	[SerializeField]
	private GameObject scoreObj;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private List<Image> confirmScores;

	[SerializeField]
	private List<Sprite> gowanScoreSpr;

	[SerializeField]
	private TextMeshProUGUI nowGet;

	[SerializeField]
	private TextMeshProUGUI totalGet;

	private float countupFlame = 60f;

	public bool isOpen;

	private long score;

	private int getWingNum;

	private int beforeWingNum;

	private int afterWingNum;

	private async void Start()
	{
		score = GameDataManager.TenshiGowanEvent.ThisGameSumScore;
		for (int i = 0; i < confirmScores.Count; i++)
		{
			if (score <= 0 && i == 0)
			{
				confirmScores[i].sprite = gowanScoreSpr[0];
				continue;
			}
			int num = (int)(score / (int)Math.Pow(10.0, i));
			if (num > 0)
			{
				int num2 = num % 10;
				confirmScores[i].sprite = gowanScoreSpr[num2];
				Debug.Log($"{i}桁目:{num2}");
			}
			else
			{
				confirmScores[i].gameObject.SetActive(value: false);
			}
		}
		getWingNum = GameDataManager.TenshiGowanEvent.GetResultTenshiWing();
		beforeWingNum = GameDataManager.TenshiGowanEvent.TenshiWingNum;
		GameDataManager.TenshiGowanEvent.AddWing();
		afterWingNum = GameDataManager.TenshiGowanEvent.TenshiWingNum;
		nowGet.text = getWingNum.ToString();
		totalGet.text = beforeWingNum.ToString();
		Debug.Log($"テンシ豪腕リザルト\u3000→\u3000今回の合計スコア：{score} / 今回のテンシの羽 : {getWingNum} / テンシの羽所持数 : {beforeWingNum} -> {afterWingNum}");
		closeButton.OnClickAsObservable().Subscribe(delegate
		{
			CloseWindow().Forget();
		}).AddTo(this);
	}

	private void Update()
	{
		RefleshText();
	}

	public async UniTask OpenWindow(CancellationToken cancellationToken)
	{
		if (!isOpen)
		{
			isOpen = true;
			base.gameObject.SetActive(value: true);
			windowObj.GetComponent<Animator>().SetTrigger("In");
			await UniTask.WaitUntil(() => windowObj.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("CompeScoreAllLoop"), PlayerLoopTiming.Update, cancellationToken);
			scoreObj.GetComponent<Animator>().SetTrigger("In");
			await UniTask.WaitUntil(() => scoreObj.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("GowanWingGetLoop"), PlayerLoopTiming.Update, cancellationToken);
			await CountUp();
			await CloseWindow();
		}
	}

	public async UniTask CloseWindow()
	{
		if (isOpen)
		{
			GameDataManager.TenshiGowanEvent.Init();
			SoundManager.StopSeAll().Forget();
			SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			windowObj.GetComponent<Animator>().SetTrigger("Out");
			isOpen = false;
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(1.0)), UniTask.WaitUntil(() => ResultFlow.Skip()));
		}
	}

	private async UniTask CountUp()
	{
		await WingNumCountUp();
		getWingNum = 0;
		beforeWingNum = afterWingNum;
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(8.0)), UniTask.WaitUntil(() => ResultFlow.Skip()));
	}

	private async UniTask WingNumCountUp()
	{
		float addRate = (float)getWingNum / countupFlame;
		float getWingFloat = getWingNum;
		float beforeFloat = beforeWingNum;
		SoundManager.PlaySe("SFX_SYS_METER1").Forget();
		for (float i = getWingNum; i >= 1f; i -= addRate)
		{
			await UniTask.Delay(TimeSpan.FromSeconds(countupFlame / 60f / 60f));
			getWingFloat -= addRate;
			beforeFloat += addRate;
			getWingNum = (int)getWingFloat;
			beforeWingNum = (int)beforeFloat;
		}
		SoundManager.StopSe("SFX_SYS_METER1").Forget();
	}

	private void RefleshText()
	{
		nowGet.text = getWingNum.ToString("F0");
		totalGet.text = beforeWingNum.ToString("F0");
	}
}
