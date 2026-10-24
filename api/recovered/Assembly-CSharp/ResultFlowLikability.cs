using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MFG.GameData;
using MFG.MasterGameData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultFlowLikability : MonoBehaviour
{
	[SerializeField]
	private GameObject m_resultAll;

	[SerializeField]
	private GameObject m_infoAll;

	[SerializeField]
	private GameObject m_popset;

	[SerializeField]
	private GameObject m_fanInfo;

	[SerializeField]
	private GameObject m_favoBonus;

	private readonly int _getExp = 100;

	private ResultFlow.LevelCalc m_levelCalc = new ResultFlow.LevelCalc(MasterGameDataManager.Character.CharacterLevel.GetExpRequiredForNextLevel);

	private int m_totalGetExpOriginal;

	private int m_totalGetExp;

	private int m_totalGetExpDisp;

	private void Update()
	{
		ResultFlow.SetGauge(m_infoAll.transform.Find("Info2").Find("LvGauge"), m_levelCalc);
		m_infoAll.transform.Find("Info2").Find("Add").Find("Num")
			.GetComponent<Text>()
			.text = "+" + m_totalGetExpDisp;
	}

	public async UniTask StartScene(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		Init();
		m_infoAll.transform.Find("Info2").Find("Num").GetComponent<TextMeshProUGUI>()
			.text = m_levelCalc.m_level.ToString("N0");
		m_infoAll.transform.Find("Info2").Find("LimitMessage").gameObject.SetActive(!GameDataManager.Player.GetCurrentChara(IsGetMyCharaData: true).IsDebut);
		m_infoAll.transform.Find("Info1").Find("Fv").Find("RankIndex")
			.Find("Text")
			.GetComponent<TextMeshProUGUI>()
			.text = "対局順位";
		m_infoAll.transform.Find("Info1").Find("Fv").Find("RankText")
			.GetComponent<Text>()
			.text = GameDataManager.Mahjong.Rank + 1 + "位";
		m_infoAll.transform.Find("Info1").Find("Fv").Find("GetLikabilityText")
			.GetComponent<Text>()
			.text = m_totalGetExpOriginal.ToString("N0");
		m_resultAll.GetComponent<Animator>().SetTrigger("In");
		m_infoAll.GetComponent<Animator>().SetTrigger("In1");
		await UniTask.Delay(TimeSpan.FromSeconds(0.20000000298023224), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(5.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResultFlow.Skip(), PlayerLoopTiming.Update, cancellationToken));
		await FavoBonus(cancellationToken);
		await CountUpToNextLevel(m_totalGetExp, first: true, cancellationToken);
		await UniTask.Delay(TimeSpan.FromSeconds(0.20000000298023224), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
		GameDataManager.Player.GetCurrentChara(IsGetMyCharaData: true).Exp += m_totalGetExp;
	}

	public void Init()
	{
		int getExp = _getExp;
		CharaGameData currentChara = GameDataManager.Player.GetCurrentChara(IsGetMyCharaData: true);
		m_levelCalc.m_maxLevel = currentChara.MaxLevel;
		m_levelCalc.m_level = currentChara.Level;
		m_levelCalc.m_exp = currentChara.Exp - currentChara.Master.GetTotalExpRequired(currentChara.Level);
		m_levelCalc.m_dispExpBefore = m_levelCalc.m_exp;
		m_levelCalc.m_dispExp = m_levelCalc.m_exp;
		GetExpClamp(getExp);
	}

	public void GetExpClamp(int totalGetExp)
	{
		CharaGameData currentChara = GameDataManager.Player.GetCurrentChara(IsGetMyCharaData: true);
		m_totalGetExp = totalGetExp;
		m_totalGetExpDisp = totalGetExp;
		m_totalGetExpOriginal = totalGetExp;
		if (currentChara.Exp + m_totalGetExpDisp >= currentChara.Master.GetTotalExpRequired(currentChara.MaxLevel))
		{
			m_totalGetExp = currentChara.Master.GetTotalExpRequired(currentChara.MaxLevel) - currentChara.Exp;
			m_totalGetExpDisp = m_totalGetExp;
		}
	}

	private async UniTask FavoBonus(CancellationToken cancellationToken)
	{
		if (GameDataManager.FavoBonusEventEnable())
		{
			m_favoBonus.SetActive(value: true);
			float favoBonusRatio = GameDataManager.GetFavoBonusRatio();
			int bonusExp = (int)Math.Round(favoBonusRatio * (float)_getExp) - _getExp;
			m_favoBonus.transform.Find("Banner").Find("Num").GetComponent<TextMeshProUGUI>()
				.text = favoBonusRatio.ToString();
			m_favoBonus.transform.Find("Bonus").Find("Num").GetComponent<TextMeshProUGUI>()
				.text = bonusExp.ToString();
			m_favoBonus.GetComponent<Animator>().SetTrigger("In1");
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResultFlow.Skip(), PlayerLoopTiming.Update, cancellationToken));
			bool isExit = false;
			Tweener tw1 = DOVirtual.Float(bonusExp, 0f, 1f, delegate(float value)
			{
				m_favoBonus.transform.Find("Bonus").Find("Num").GetComponent<TextMeshProUGUI>()
					.text = ((int)value).ToString();
			}).SetEase(Ease.InQuad).OnComplete(delegate
			{
				isExit = true;
			});
			Tweener tw2 = DOVirtual.Float(m_totalGetExp, m_totalGetExp + bonusExp, 1f, delegate(float value)
			{
				GetExpClamp((int)value);
			}).SetEase(Ease.InQuad);
			if (bonusExp > 0)
			{
				await SoundManager.PlaySe("SFX_SYS_METER1");
			}
			await UniTask.WhenAny(UniTask.WaitUntil(() => isExit, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResultFlow.Skip(), PlayerLoopTiming.Update, cancellationToken));
			tw1.Complete();
			tw2.Complete();
			SoundManager.StopSe("SFX_SYS_METER1").Forget();
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResultFlow.Skip(), PlayerLoopTiming.Update, cancellationToken));
			m_favoBonus.GetComponent<Animator>().SetTrigger("In2");
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResultFlow.Skip(), PlayerLoopTiming.Update, cancellationToken));
		}
	}

	private async UniTask CountUpToNextLevel(int totalGetExp, bool first = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		bool isExit = false;
		bool isLevelUp = false;
		int getExpInThisLevel = totalGetExp;
		m_infoAll.transform.Find("Info2").Find("Num").GetComponent<TextMeshProUGUI>()
			.text = m_levelCalc.m_level.ToString("N0");
		if (m_levelCalc.IsLevelUp(totalGetExp))
		{
			getExpInThisLevel = m_levelCalc.GetNextExp() - m_levelCalc.m_exp;
			isLevelUp = true;
		}
		Tweener tw1 = DOVirtual.Float(m_levelCalc.m_exp, m_levelCalc.m_exp + getExpInThisLevel, 2f, delegate(float value)
		{
			m_levelCalc.m_dispExp = (int)value;
		}).SetEase(Ease.InQuad).OnComplete(delegate
		{
			isExit = true;
		});
		Tweener tw2 = DOVirtual.Float(m_totalGetExpDisp, m_totalGetExpDisp - getExpInThisLevel, 2f, delegate(float value)
		{
			m_totalGetExpDisp = (int)value;
		}).SetEase(Ease.InQuad);
		if (getExpInThisLevel > 0)
		{
			await SoundManager.PlaySe("SFX_SYS_METER1");
		}
		await UniTask.WhenAny(UniTask.WaitUntil(() => isExit, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResultFlow.Skip(), PlayerLoopTiming.Update, cancellationToken));
		tw1.Complete();
		tw2.Complete();
		SoundManager.StopSe("SFX_SYS_METER1").Forget();
		if (isLevelUp)
		{
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(0.5), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResultFlow.Skip(), PlayerLoopTiming.Update, cancellationToken));
			m_popset.SetActive(value: true);
			m_popset.GetComponent<Animator>().SetTrigger("TriggerLikability");
			SoundManager.PlaySe("SFX_SYS_RANKUP1").Forget();
			SoundManager.PlayMyCharaVoiceRandom(new List<string> { "VOX_GXX_FAVOR_1", "VOX_GXX_FAVOR_2", "VOX_GXX_FAVOR_3" }).Forget();
			m_popset.transform.Find("PopLikability").Find("Num1").GetComponent<TextMeshProUGUI>()
				.text = m_levelCalc.m_level.ToString("N0");
			m_popset.transform.Find("PopLikability").Find("Num2").GetComponent<TextMeshProUGUI>()
				.text = (m_levelCalc.m_level + 1).ToString("N0");
			await UniTask.Delay(TimeSpan.FromSeconds(0.5), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(5.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResultFlow.Skip(), PlayerLoopTiming.Update, cancellationToken));
			if (ResultFlow.Skip())
			{
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			}
			m_levelCalc.AddExp(getExpInThisLevel);
			m_levelCalc.m_dispExp = 0;
			m_levelCalc.m_dispExpBefore = 0;
			m_popset.GetComponent<Animator>().SetTrigger("TriggerOut");
			if (m_levelCalc.m_level == m_levelCalc.m_maxLevel && !GameDataManager.PlayerConfig.IsDispHelpResultFavoLimit && !GameDataManager.Player.GetCurrentChara(IsGetMyCharaData: true).IsDebut)
			{
				GameDataManager.PlayerConfig.IsDispHelpResultFavoLimit = true;
				await FanInfoDisp(cancellationToken);
			}
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(0.5), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResultFlow.Skip(), PlayerLoopTiming.Update, cancellationToken));
			await CountUpToNextLevel(totalGetExp - getExpInThisLevel, first: false, cancellationToken);
		}
		else
		{
			m_levelCalc.AddExp(getExpInThisLevel);
		}
	}

	private async UniTask FanInfoDisp(CancellationToken cancellationToken = default(CancellationToken))
	{
		float timeLimit = ((GameUtility.IsAppTerminatDecided || Cloudstrap.IsDisconnect) ? 5 : 20);
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
		m_fanInfo.SetActive(value: true);
		m_fanInfo.GetComponent<Animator>().SetTrigger("Start");
		await UniTask.Delay(TimeSpan.FromSeconds(2.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(timeLimit), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResidentFlow.CustomInput.GetMouseButtonDown(0), PlayerLoopTiming.Update, cancellationToken));
		m_fanInfo.GetComponent<Animator>().SetTrigger("Skip");
		m_fanInfo.SetActive(value: false);
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
	}
}
