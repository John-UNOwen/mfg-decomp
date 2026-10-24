using System;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ShopCompetitionHomeWindow : MonoBehaviour
{
	[SerializeField]
	private GameObject windowObj;

	[SerializeField]
	private Button btClose;

	[SerializeField]
	private TextMeshProUGUI dateText;

	[SerializeField]
	private TextMeshProUGUI shopNameText;

	[SerializeField]
	private TextMeshProUGUI shopRank;

	[SerializeField]
	private TextMeshProUGUI shopScore;

	[SerializeField]
	private TextMeshProUGUI playerRank;

	[SerializeField]
	private TextMeshProUGUI playerScore;

	[SerializeField]
	private Button btRanking;

	[SerializeField]
	private Button btRankingClose;

	[SerializeField]
	private Animator rankingWindow;

	[SerializeField]
	private Button btRule;

	[SerializeField]
	private Button btRuleClose;

	[SerializeField]
	private Animator ruleWindow;

	[SerializeField]
	private Button btAnonymous;

	[SerializeField]
	private Button btAnonymousClose;

	[SerializeField]
	private Animator anonymousWindow;

	[SerializeField]
	private Button btAnonymousYes;

	[SerializeField]
	private Button btAnonymousNo;

	private bool isOpen;

	private bool isRankingOpen;

	private bool isRuleOpen;

	private bool isAnonymousOpen;

	private bool isAnonymousForceOpen;

	private bool isFirst;

	private bool isCheckedAnonymous;

	private bool isAnonymousDecided;

	private bool isAnonymous = true;

	public bool IsOpen => isOpen;

	public bool IsAnonymousForceOpen => isAnonymousForceOpen;

	private async void Start()
	{
		btClose.OnClickAsObservable().Subscribe(delegate
		{
			Close().Forget();
		}).AddTo(this);
		btRanking.OnClickAsObservable().Subscribe(delegate
		{
			RankingOpen();
		}).AddTo(this);
		btRankingClose.OnClickAsObservable().Subscribe(delegate
		{
			RankingClose();
		}).AddTo(this);
		btRule.OnClickAsObservable().Subscribe(delegate
		{
			RuleOpen();
		}).AddTo(this);
		btRuleClose.OnClickAsObservable().Subscribe(delegate
		{
			RuleClose();
		}).AddTo(this);
		btAnonymous.OnClickAsObservable().Subscribe(delegate
		{
			AnonymousOpen();
		}).AddTo(this);
		btAnonymousClose.OnClickAsObservable().Subscribe(delegate
		{
			isAnonymousDecided = true;
			GameDataManager.Player.IsAnonymousShopCompetition = isAnonymous;
			AnonymousClose();
		}).AddTo(this);
		btAnonymousYes.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			isAnonymous = true;
			AnonymousButtonSetTrigger();
		}).AddTo(this);
		btAnonymousNo.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			isAnonymous = false;
			AnonymousButtonSetTrigger();
		}).AddTo(this);
	}

	public void AnonymousButtonSetTrigger()
	{
		btAnonymousYes.transform.GetComponent<Animator>().SetTrigger(isAnonymous ? "Selected" : "Normal");
		btAnonymousNo.transform.GetComponent<Animator>().SetTrigger(isAnonymous ? "Normal" : "Selected");
	}

	public void Open(bool isFirstDisplay = false)
	{
		if (!isOpen)
		{
			ShopCompetitionInfo inProgressCompetitionInfo = GameDataManager.ShopCompetition.GetInProgressCompetitionInfo();
			if (inProgressCompetitionInfo != null)
			{
				isFirst = isFirstDisplay;
				isAnonymousDecided = false;
				dateText.text = inProgressCompetitionInfo.End.ToString("yyyy年MM月dd日（ddd）HH：mmまで");
				shopNameText.text = inProgressCompetitionInfo.ShopName;
				shopRank.text = ((inProgressCompetitionInfo.ShopRank <= 0) ? "―" : inProgressCompetitionInfo.ShopRank.ToString("N0"));
				shopScore.text = inProgressCompetitionInfo.ShopScore.ToString("N0");
				playerRank.text = ((inProgressCompetitionInfo.PersonalRank <= 0) ? "―" : inProgressCompetitionInfo.PersonalRank.ToString("N0"));
				playerScore.text = inProgressCompetitionInfo.PersonalScore.ToString("N0");
				isOpen = true;
				base.gameObject.SetActive(value: true);
				SoundManager.PlaySe("SFX_MJG_INFO1").Forget();
				windowObj.GetComponent<Animator>().SetTrigger("In");
			}
		}
	}

	public async UniTask Close()
	{
		if (!isOpen)
		{
			return;
		}
		if (isRankingOpen)
		{
			RankingClose();
		}
		if (isRuleOpen)
		{
			RuleClose();
		}
		if (isFirst && !isCheckedAnonymous)
		{
			isAnonymousForceOpen = true;
			AnonymousOpen();
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(20.0)), UniTask.WaitUntil(() => !isAnonymousOpen));
		}
		if (isFirst && !isAnonymousDecided)
		{
			GameDataManager.Player.IsAnonymousShopCompetition = true;
		}
		if (isAnonymousOpen)
		{
			AnonymousClose(isSEPlay: false);
		}
		SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
		windowObj.GetComponent<Animator>().SetTrigger("Out");
		base.gameObject.SetActive(value: false);
		isOpen = false;
		await UniTask.Delay(TimeSpan.FromSeconds(1.0));
		Debug.Log($"IsAnonymousShopCompetition {GameDataManager.Player.IsAnonymousShopCompetition}");
	}

	private void RankingOpen()
	{
		if (!isRankingOpen)
		{
			isRankingOpen = true;
			rankingWindow.gameObject.SetActive(value: true);
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			rankingWindow.SetTrigger("In");
		}
	}

	private void RankingClose()
	{
		if (isRankingOpen)
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			rankingWindow.SetTrigger("Out");
			rankingWindow.gameObject.SetActive(value: false);
			isRankingOpen = false;
		}
	}

	private void RuleOpen()
	{
		if (!isRuleOpen)
		{
			isRuleOpen = true;
			ruleWindow.gameObject.SetActive(value: true);
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			ruleWindow.SetTrigger("In");
		}
	}

	private void RuleClose()
	{
		if (isRuleOpen)
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			ruleWindow.SetTrigger("Out");
			ruleWindow.gameObject.SetActive(value: false);
			isRuleOpen = false;
		}
	}

	private void AnonymousOpen(bool isSEPlay = true)
	{
		if (!isAnonymousOpen)
		{
			isCheckedAnonymous = true;
			isAnonymousOpen = true;
			anonymousWindow.gameObject.SetActive(value: true);
			if (isSEPlay)
			{
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			}
			anonymousWindow.SetTrigger("In");
			isAnonymous = GameDataManager.Player.IsAnonymousShopCompetition;
			AnonymousButtonSetTrigger();
		}
	}

	private void AnonymousClose(bool isSEPlay = true)
	{
		if (isAnonymousOpen)
		{
			if (isSEPlay)
			{
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			}
			anonymousWindow.SetTrigger("Out");
			anonymousWindow.gameObject.SetActive(value: false);
			isAnonymousOpen = false;
		}
	}
}
