using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MFG.GameData;
using MFG.Types;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultFlowFightStar : MonoBehaviour
{
	[SerializeField]
	private GameObject m_fightStarAll;

	[SerializeField]
	private GameObject m_fightStarInfoAll;

	[SerializeField]
	private GameObject m_popset;

	[SerializeField]
	private List<GameObject> m_starUpPrefabs = new List<GameObject>();

	[SerializeField]
	private GameObject m_starUpParent;

	[SerializeField]
	private List<GameObject> m_starDownPrefabs = new List<GameObject>();

	[SerializeField]
	private GameObject m_starDownParent;

	[SerializeField]
	private DanBanner m_danBanner;

	[SerializeField]
	private GameObject m_revengeSuccess;

	[SerializeField]
	private GameObject m_hoten;

	[SerializeField]
	private GameObject m_topInfo;

	[SerializeField]
	private float m_debugStarAnimeFrame = 10f;

	private ResultFlow.LevelCalc m_danCalc = new ResultFlow.LevelCalc(GetFightStarRequiredForNextDanLevel);

	private int m_totalGetFightStarOriginal;

	private int m_totalGetFightStar;

	private int m_totalGetFightStarDisp;

	private int m_revengeSuccessStar;

	private int m_hotenStar;

	private bool m_isGaugeDown;

	private bool m_isExit;

	public CancellationTokenSource m_cancellationTokenSource;

	public CancellationToken m_cancellationToken;

	private static int GetFightStarRequiredForNextDanLevel(int DanLevel)
	{
		return DanLevelMaster.GetFightStarRequiredForNextDanLevel((DanLevelType)DanLevel);
	}

	private async void Start()
	{
	}

	private void Update()
	{
		ResultFlow.SetGauge(m_fightStarInfoAll.transform.Find("Info2").Find("LvGauge"), m_danCalc, m_isGaugeDown);
		if (m_revengeSuccessStar <= 0 && m_hotenStar <= 0)
		{
			SetFightStarText(m_totalGetFightStarDisp);
		}
	}

	public async UniTask StartScene()
	{
		Init();
		m_danBanner.SetDanBanner(m_fightStarInfoAll.transform.Find("Info2").Find("Dani"), (DanLevelType)m_danCalc.m_level);
		m_fightStarInfoAll.transform.Find("Info1").Find("Fv").Find("RankText")
			.GetComponent<Text>()
			.text = GameDataManager.Mahjong.Rank + 1 + "位";
		SetFightStarText(m_totalGetFightStar - m_revengeSuccessStar - m_hotenStar);
		SetRevenge(m_revengeSuccessStar);
		SetHoten(m_hotenStar);
		m_fightStarInfoAll.transform.Find("Info1").Find("Fv").Find("Rate")
			.Find("Rate1")
			.gameObject.SetActive(GameDataManager.Mahjong.GainLossType == GainLossType.Risky);
		m_fightStarInfoAll.transform.Find("Info1").Find("Fv").Find("Rate")
			.Find("Rate2")
			.gameObject.SetActive(GameDataManager.Mahjong.GainLossType == GainLossType.Steadily);
		m_fightStarInfoAll.transform.Find("Info1").Find("Fv").Find("Rate")
			.Find("Rate3")
			.gameObject.SetActive(GameDataManager.Mahjong.GainLossType == GainLossType.SuperRisky);
		m_fightStarAll.GetComponent<Animator>().SetTrigger("In");
		m_fightStarInfoAll.GetComponent<Animator>().SetTrigger("In1");
		if (!GameDataManager.PlayerConfig.IsDispHelpFirstPlayResultFightStar && !DebugFlag.IsActive(DebugFlag.Flag.SkipGuideDisp))
		{
			await GuideDisp();
			GameDataManager.PlayerConfig.IsDispHelpFirstPlayResultFightStar = true;
		}
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(3.0)), UniTask.WaitUntil(() => ResultFlow.Skip()));
		if (m_totalGetFightStar > 0)
		{
			SoundManager.PlayMyCharaVoice("VOX_GXX_STAR1").Forget();
		}
		else if (m_totalGetFightStar < 0)
		{
			SoundManager.PlayMyCharaVoice("VOX_GXX_STAR2").Forget();
		}
		await CountUpRevengeStar();
		await CountUpHotenStar();
		await CountUpToNextLevel(m_totalGetFightStar, first: true);
		await UniTask.Delay(TimeSpan.FromSeconds(0.20000000298023224));
		ResidentFlow.GetInstance().GetTimer().SetTimer((GameUtility.IsAppTerminatDecided || Cloudstrap.IsDisconnect) ? 5 : 20, disp: true);
		ResidentFlow.GetInstance().GetTimer().StartTimer();
		await UniTask.WhenAny(UniTask.WaitUntil(() => ResidentFlow.GetInstance().GetTimer().IsEnd()), UniTask.WaitUntil(() => ResultFlow.Skip()));
		ResidentFlow.GetInstance().GetTimer().ResetTimer(disp: false);
		m_fightStarAll.GetComponent<Animator>().SetTrigger("Out");
		PlayerGameData player = GameDataManager.Player;
		player.FightStar += m_totalGetFightStar;
		player.DojoUnlock();
	}

	private void Init()
	{
		PlayerGameData player = GameDataManager.Player;
		m_totalGetFightStar = DanLevelMaster.GetGetAcquiredFightStar(GameDataManager.Mahjong.TakuType, player.DanLevel, GameDataManager.Mahjong.GainLossType, (Rank)GameDataManager.Mahjong.Rank, GameDataManager.Mahjong.IsKiken);
		if (m_totalGetFightStar < 0)
		{
			m_isGaugeDown = true;
		}
		if (GameDataManager.IsRevengeContinueEnable())
		{
			if (GameDataManager.Mahjong.TakuType == TakuType.Tonpu && GameDataManager.Mahjong.Rank == 3)
			{
				GameDataManager.PlayerConfig.RevengeContinueValue = Math.Abs(m_totalGetFightStar);
			}
			else if (GameDataManager.Mahjong.TakuType == TakuType.Sanma && GameDataManager.Mahjong.Rank == 2)
			{
				int revengeContinueValue = ((Math.Abs(m_totalGetFightStar) % 2 == 0) ? (Math.Abs(m_totalGetFightStar) / 2) : ((Math.Abs(m_totalGetFightStar) + 1) / 2));
				GameDataManager.PlayerConfig.RevengeContinueValue = revengeContinueValue;
			}
			else if ((GameDataManager.Mahjong.TakuType == TakuType.Tonpu && GameDataManager.Mahjong.Rank == 0) || (GameDataManager.Mahjong.TakuType == TakuType.Sanma && GameDataManager.Mahjong.Rank == 0))
			{
				m_revengeSuccessStar = GameDataManager.PlayerConfig.RevengeContinueValue;
				m_totalGetFightStar += m_revengeSuccessStar;
				GameDataManager.PlayerConfig.RevengeContinueValue = 0;
			}
			else
			{
				GameDataManager.PlayerConfig.RevengeContinueValue = 0;
			}
		}
		else
		{
			GameDataManager.PlayerConfig.RevengeContinueValue = 0;
		}
		if (GameDataManager.PlayerConfig.HotenFightStar > 0)
		{
			m_hotenStar = GameDataManager.PlayerConfig.HotenFightStar;
			m_totalGetFightStar += m_hotenStar;
			GameDataManager.PlayerConfig.HotenFightStar = 0;
		}
		m_danCalc.m_maxLevel = (int)GameDataManager.Player.MaxDanLevel;
		m_danCalc.m_level = (int)player.DanLevel;
		m_danCalc.m_exp = player.FightStar - DanLevelMaster.GetTotalFightStarRequired(player.DanLevel);
		m_danCalc.m_dispExpBefore = m_danCalc.m_exp;
		m_danCalc.m_dispExp = m_danCalc.m_exp;
		m_totalGetFightStarDisp = m_totalGetFightStar;
		m_totalGetFightStarOriginal = m_totalGetFightStar;
		if (player.FightStar + m_totalGetFightStar >= DanLevelMaster.GetTotalFightStarRequired(player.MaxDanLevel))
		{
			m_totalGetFightStar = DanLevelMaster.GetTotalFightStarRequired(player.MaxDanLevel) - player.FightStar;
			m_totalGetFightStarDisp = m_totalGetFightStar;
		}
		if (player.FightStar + m_totalGetFightStar <= 0)
		{
			m_totalGetFightStar = -player.FightStar;
			m_totalGetFightStarDisp = m_totalGetFightStar;
		}
	}

	private async UniTask CountUpRevengeStar()
	{
		if (m_revengeSuccessStar <= 0)
		{
			return;
		}
		Animator anim = m_revengeSuccess.transform.GetComponent<Animator>();
		anim.SetTrigger("In");
		await UniTask.WhenAny(UniTask.WaitWhile(() => anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f), UniTask.WaitUntil(() => ResultFlow.Skip()));
		if (anim.GetCurrentAnimatorStateInfo(0).IsName("RevengeIn"))
		{
			anim.SetTrigger("Skip");
		}
		List<Tweener> tweenerList = new List<Tweener>();
		float tweenTime = 1f;
		tweenerList.Add(DOVirtual.Float(m_revengeSuccessStar, 0f, tweenTime, delegate(float value)
		{
			SetRevenge((int)value);
		}).OnComplete(delegate
		{
			SetRevenge(0);
		}));
		tweenerList.Add(DOVirtual.Float(m_totalGetFightStar - m_revengeSuccessStar - m_hotenStar, m_totalGetFightStar - m_hotenStar, tweenTime, delegate(float value)
		{
			SetFightStarText((int)value);
		}).OnComplete(delegate
		{
			SetFightStarText(m_totalGetFightStar - m_hotenStar);
		}));
		await SoundManager.PlaySe("SFX_MJG_POINT1");
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(tweenTime)), UniTask.WaitUntil(() => ResultFlow.Skip()));
		foreach (Tweener item in tweenerList)
		{
			item.Complete();
		}
		SoundManager.StopSe("SFX_MJG_POINT1").Forget();
		SoundManager.PlaySe("SFX_MJG_POINT2").Forget();
		m_revengeSuccessStar = 0;
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(0.5)), UniTask.WaitUntil(() => ResultFlow.Skip()));
		anim.SetTrigger("Out");
	}

	private async UniTask CountUpHotenStar()
	{
		if (m_hotenStar <= 0)
		{
			return;
		}
		Animator anim = m_hoten.transform.GetComponent<Animator>();
		anim.SetTrigger("In");
		await UniTask.WhenAny(UniTask.WaitWhile(() => anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f), UniTask.WaitUntil(() => ResultFlow.Skip()));
		if (anim.GetCurrentAnimatorStateInfo(0).IsName("RevengeIn"))
		{
			anim.SetTrigger("Skip");
		}
		List<Tweener> tweenerList = new List<Tweener>();
		float tweenTime = 1f;
		tweenerList.Add(DOVirtual.Float(m_hotenStar, 0f, tweenTime, delegate(float value)
		{
			SetHoten((int)value);
		}).OnComplete(delegate
		{
			SetHoten(0);
		}));
		tweenerList.Add(DOVirtual.Float(m_totalGetFightStar - m_hotenStar, m_totalGetFightStar, tweenTime, delegate(float value)
		{
			SetFightStarText((int)value);
		}).OnComplete(delegate
		{
			SetFightStarText(m_totalGetFightStar);
		}));
		await SoundManager.PlaySe("SFX_MJG_POINT1");
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(tweenTime)), UniTask.WaitUntil(() => ResultFlow.Skip()));
		foreach (Tweener item in tweenerList)
		{
			item.Complete();
		}
		SoundManager.StopSe("SFX_MJG_POINT1").Forget();
		SoundManager.PlaySe("SFX_MJG_POINT2").Forget();
		m_hotenStar = 0;
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(0.5)), UniTask.WaitUntil(() => ResultFlow.Skip()));
		anim.SetTrigger("Out");
	}

	private async UniTask CountUpToNextLevel(int totalGetExp, bool first = false)
	{
		m_cancellationTokenSource = new CancellationTokenSource();
		m_cancellationToken = m_cancellationTokenSource.Token;
		bool isLevelUp = false;
		bool isLevelDown = false;
		int getExpInThisLevel = totalGetExp;
		m_danBanner.SetDanBanner(m_fightStarInfoAll.transform.Find("Info2").Find("Dani"), (DanLevelType)m_danCalc.m_level);
		if (m_danCalc.IsLevelUp(totalGetExp))
		{
			getExpInThisLevel = m_danCalc.GetNextExp() - m_danCalc.m_exp;
			isLevelUp = true;
			GameDataManager.Mission.isLevelUp = true;
		}
		else if (m_danCalc.IsLevelDown(totalGetExp))
		{
			getExpInThisLevel = -m_danCalc.m_exp - 1;
			isLevelDown = true;
		}
		int danexp = m_danCalc.m_exp + getExpInThisLevel;
		int totalGetFightStarDisp = m_totalGetFightStarDisp - getExpInThisLevel;
		m_isExit = false;
		if (getExpInThisLevel > 0)
		{
			await SoundManager.PlaySe("SFX_SYS_METER1");
			for (int i = 0; i < getExpInThisLevel; i++)
			{
				InstantiateStarUp(i, i == getExpInThisLevel - 1, m_cancellationToken).Forget();
			}
		}
		else if (getExpInThisLevel < 0)
		{
			await SoundManager.PlaySe("SFX_SYS_METER2");
			for (int j = 0; j < Math.Abs(getExpInThisLevel); j++)
			{
				InstantiateStarDown(j, j == Math.Abs(getExpInThisLevel) - 1, m_cancellationToken).Forget();
			}
		}
		else
		{
			m_isExit = true;
		}
		await UniTask.WhenAny(UniTask.WaitUntil(() => m_isExit, PlayerLoopTiming.Update, m_cancellationToken), UniTask.WaitUntil(() => ResultFlow.Skip(), PlayerLoopTiming.Update, m_cancellationToken));
		m_cancellationTokenSource.Cancel();
		m_danCalc.m_dispExp = danexp;
		m_totalGetFightStarDisp = totalGetFightStarDisp;
		SoundManager.StopSe("SFX_SYS_METER1").Forget();
		SoundManager.StopSe("SFX_SYS_METER2").Forget();
		if (isLevelUp)
		{
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(0.5)), UniTask.WaitUntil(() => ResultFlow.Skip()));
			m_popset.SetActive(value: true);
			m_popset.GetComponent<Animator>().SetTrigger("TriggerDaniUp");
			SoundManager.PlaySe("SFX_SYS_RANKUP1").Forget();
			if (m_danCalc.m_level + 1 == 11 && !GameDataManager.PlayerConfig.IsPlayVoiceRankUpDan)
			{
				GameDataManager.PlayerConfig.IsPlayVoiceRankUpDan = true;
				SoundManager.PlayMyCharaVoice("VOX_GXX_RANKUP2").Forget();
			}
			else if (m_danCalc.m_level + 1 == 21 && !GameDataManager.PlayerConfig.IsPlayVoiceRankUpJansei)
			{
				GameDataManager.PlayerConfig.IsPlayVoiceRankUpJansei = true;
				SoundManager.PlayMyCharaVoice("VOX_GXX_RANKUP3").Forget();
			}
			else
			{
				SoundManager.PlayMyCharaVoice("VOX_GXX_RANKUP1").Forget();
			}
			m_danBanner.SetDanBanner(m_popset.transform.Find("PoDaniUp").Find("Dani1"), (DanLevelType)m_danCalc.m_level);
			m_danBanner.SetDanBanner(m_popset.transform.Find("PoDaniUp").Find("Dani2"), (DanLevelType)(m_danCalc.m_level + 1));
			await UniTask.Delay(TimeSpan.FromSeconds(0.5));
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(5.0)), UniTask.WaitUntil(() => ResultFlow.Skip()));
			m_danCalc.AddExp(getExpInThisLevel);
			m_danCalc.m_dispExp = 0;
			m_danCalc.m_dispExpBefore = 0;
			ResidentFlow.GetInstance().GetHeader().SetDanLevel((DanLevelType)m_danCalc.m_level);
			m_popset.GetComponent<Animator>().SetTrigger("TriggerOut");
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(0.5)), UniTask.WaitUntil(() => ResultFlow.Skip()));
			await CountUpToNextLevel(totalGetExp - getExpInThisLevel);
		}
		else if (isLevelDown)
		{
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(0.5)), UniTask.WaitUntil(() => ResultFlow.Skip()));
			m_popset.SetActive(value: true);
			m_popset.GetComponent<Animator>().SetTrigger("TriggerDaniDown");
			SoundManager.PlaySe("SFX_SYS_RANKDOWN1").Forget();
			SoundManager.PlayMyCharaVoice("VOX_GXX_RANKDOWN1").Forget();
			m_danBanner.SetDanBanner(m_popset.transform.Find("PoDaniDown").Find("Dani1"), (DanLevelType)m_danCalc.m_level);
			m_danBanner.SetDanBanner(m_popset.transform.Find("PoDaniDown").Find("Dani2"), (DanLevelType)(m_danCalc.m_level - 1));
			await UniTask.Delay(TimeSpan.FromSeconds(0.5));
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(5.0)), UniTask.WaitUntil(() => ResultFlow.Skip()));
			m_danCalc.AddExp(getExpInThisLevel);
			m_danCalc.m_dispExp = m_danCalc.m_exp;
			m_danCalc.m_dispExpBefore = m_danCalc.m_exp + 1;
			ResidentFlow.GetInstance().GetHeader().SetDanLevel((DanLevelType)m_danCalc.m_level);
			m_popset.GetComponent<Animator>().SetTrigger("TriggerOut");
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(0.5)), UniTask.WaitUntil(() => ResultFlow.Skip()));
			await CountUpToNextLevel(totalGetExp - getExpInThisLevel);
		}
		else
		{
			m_danCalc.AddExp(getExpInThisLevel);
		}
	}

	private async UniTask InstantiateStarUp(int index, bool isLast, CancellationToken token = default(CancellationToken))
	{
		GameObject obj = UnityEngine.Object.Instantiate(m_starUpPrefabs[UnityEngine.Random.Range(0, 3)], m_starUpParent.transform);
		await UniTask.Delay(TimeSpan.FromSeconds(m_debugStarAnimeFrame / 60f * (float)index), ignoreTimeScale: false, PlayerLoopTiming.Update, token);
		if (token.IsCancellationRequested)
		{
			return;
		}
		obj.GetComponent<Animator>().SetTrigger("In");
		await UniTask.Delay(TimeSpan.FromSeconds(0.44999998807907104), ignoreTimeScale: false, PlayerLoopTiming.Update, token);
		if (!token.IsCancellationRequested)
		{
			m_danCalc.m_dispExp++;
			m_totalGetFightStarDisp--;
			if (isLast)
			{
				m_isExit = true;
			}
		}
	}

	private async UniTask InstantiateStarDown(int index, bool isLast, CancellationToken token = default(CancellationToken))
	{
		GameObject obj = UnityEngine.Object.Instantiate(m_starDownPrefabs[UnityEngine.Random.Range(0, 3)], m_starDownParent.transform);
		await UniTask.Delay(TimeSpan.FromSeconds(m_debugStarAnimeFrame / 60f * (float)index), ignoreTimeScale: false, PlayerLoopTiming.Update, token);
		if (token.IsCancellationRequested)
		{
			return;
		}
		obj.GetComponent<Animator>().SetTrigger("In");
		await UniTask.Delay(TimeSpan.FromSeconds(0.44999998807907104), ignoreTimeScale: false, PlayerLoopTiming.Update, token);
		if (!token.IsCancellationRequested)
		{
			m_danCalc.m_dispExp--;
			m_totalGetFightStarDisp++;
			if (isLast)
			{
				m_isExit = true;
			}
		}
	}

	private void SetFightStarText(int fightStar)
	{
		m_fightStarInfoAll.transform.Find("Info1").Find("Fv").Find("GetFightstarText")
			.GetComponent<Text>()
			.color = CustomColor.GetColor(fightStar);
		m_fightStarInfoAll.transform.Find("Info1").Find("Fv").Find("GetFightstarText")
			.GetComponent<Text>()
			.text = Utility.GetSign(fightStar) + Math.Abs(fightStar);
	}

	private void SetRevenge(int fightStar)
	{
		m_revengeSuccess.transform.Find("RevengeNum").GetComponent<TextMeshProUGUI>().text = fightStar.ToString();
	}

	private void SetHoten(int fightStar)
	{
		m_hoten.transform.Find("RevengeNum").GetComponent<TextMeshProUGUI>().text = fightStar.ToString();
	}

	private async UniTask GuideDisp(CancellationToken cancellationToken = default(CancellationToken))
	{
		float timeLimit = ((GameUtility.IsAppTerminatDecided || Cloudstrap.IsDisconnect) ? 5 : 20);
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
		m_topInfo.SetActive(value: true);
		m_topInfo.GetComponent<Animator>().SetTrigger("Start");
		await UniTask.Delay(TimeSpan.FromSeconds(2.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(timeLimit), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResidentFlow.CustomInput.GetMouseButtonDown(0), PlayerLoopTiming.Update, cancellationToken));
		m_topInfo.GetComponent<Animator>().SetTrigger("Skip");
		SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
		m_topInfo.SetActive(value: false);
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
	}
}
