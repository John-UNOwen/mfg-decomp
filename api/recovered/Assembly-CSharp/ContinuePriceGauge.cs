using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class ContinuePriceGauge : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI m_acquiredScoreText;

	[SerializeField]
	private TextMeshProUGUI m_upperBorderScoreText;

	[SerializeField]
	private GameObject m_lowerBorderFrame;

	[SerializeField]
	private TextMeshProUGUI m_lowerBorderScoreText;

	[SerializeField]
	private TextMeshProUGUI m_lowerScorePriceText;

	[SerializeField]
	private GameObject m_gaugePaseliFrame;

	[SerializeField]
	private TextMeshProUGUI m_gaugeCreditTitleText;

	[SerializeField]
	private TextMeshProUGUI m_gaugeCreditNumText;

	[SerializeField]
	private TextMeshProUGUI m_gaugePaseliPriceText;

	[SerializeField]
	private Animator m_gaugeBarAnimator;

	private float m_gaugeRate;

	private float m_endGaugeRate;

	private const int LowerBorderGaugeFramePos = 70;

	private const int UpperBorderGaugeFramePos = 100;

	private const int GaugeFrameMaxPos = 120;

	private int m_acquiredScore;

	private int m_continuePaseliPrice;

	private int m_continueCretidNum;

	private int m_lowerBorderScore;

	private int m_upperBorderScore;

	private int m_lowerScorePaseliPrice;

	private int m_upperScorePaseliPrice;

	private int m_lowerScoreCreditNum;

	private bool m_isPaseliAvailable;

	private void Start()
	{
	}

	public void Init(int acquiredScore, int continuePaseliPrice, int continueCretidNum, int lowerBorderScore, int upperBorderScore, int lowerScorePaseliPrice, int upperScorePaseliPrice, int lowerScoreCreditNum, bool isPaseliAvailable)
	{
		m_acquiredScore = acquiredScore;
		m_continuePaseliPrice = continuePaseliPrice;
		m_continueCretidNum = continueCretidNum;
		m_lowerBorderScore = lowerBorderScore;
		m_upperBorderScore = upperBorderScore;
		m_lowerScorePaseliPrice = lowerScorePaseliPrice;
		m_upperScorePaseliPrice = upperScorePaseliPrice;
		m_lowerScoreCreditNum = lowerScoreCreditNum;
		m_isPaseliAvailable = isPaseliAvailable;
		m_acquiredScoreText.text = MakeScoreText(m_acquiredScore) ?? "";
		m_upperBorderScoreText.text = "SCORE " + MakeScoreText(m_upperBorderScore);
		m_lowerBorderFrame.SetActive(m_isPaseliAvailable);
		m_lowerBorderScoreText.text = "SCORE " + MakeScoreText(m_lowerBorderScore);
		m_lowerScorePriceText.text = string.Format("{0} : {1}{2}", (m_lowerScoreCreditNum >= 2) ? "CREDITS" : "CREDIT", m_lowerScoreCreditNum, isPaseliAvailable ? $" / PASELI : {m_lowerScorePaseliPrice}" : "");
		m_gaugePaseliFrame.SetActive(m_isPaseliAvailable);
		m_gaugeRate = 0f;
		Gauge_RefleshText(m_gaugeRate);
		static string MakeScoreText(long score)
		{
			if (Math.Abs(score) >= MahjongUtility.Aotenjo.MaxScore)
			{
				return MahjongUtility.Aotenjo.GetScoreString(GameDataManager.Mahjong.AotenjoScore, isYakuResult: false, "#000000");
			}
			string text = "";
			if (score < 0)
			{
				text = "－";
			}
			return text + Math.Abs(score).ToString("N0");
		}
	}

	private void Gauge_RefleshText(float gaugeRate)
	{
		int lowerScoreCreditNum = m_lowerScoreCreditNum;
		int lowerScorePaseliPrice = m_lowerScorePaseliPrice;
		if (gaugeRate < 7f / 12f)
		{
			lowerScoreCreditNum = m_lowerScoreCreditNum;
			lowerScorePaseliPrice = m_lowerScorePaseliPrice;
		}
		else if (7f / 12f <= gaugeRate && gaugeRate < 5f / 6f)
		{
			lowerScoreCreditNum = m_lowerScoreCreditNum;
			lowerScorePaseliPrice = (int)((float)m_lowerScorePaseliPrice + (gaugeRate - 7f / 12f) * (float)(m_upperScorePaseliPrice - m_lowerScorePaseliPrice) / 0.25f);
		}
		else
		{
			lowerScoreCreditNum = 0;
			lowerScorePaseliPrice = 0;
		}
		Gauge_RefleshText(lowerScoreCreditNum, lowerScorePaseliPrice);
	}

	private void Gauge_RefleshText(int creditNum, int paseliPrice)
	{
		m_gaugeCreditTitleText.text = ((creditNum >= 2) ? "CREDITS" : "CREDIT");
		m_gaugeCreditNumText.text = $"{creditNum}";
		m_gaugePaseliPriceText.text = $"{paseliPrice}";
	}

	public async UniTask Play(CancellationToken cancellationToken = default(CancellationToken))
	{
		m_gaugeRate = 0f;
		float num = (float)(m_upperBorderScore - m_lowerBorderScore) / 30f;
		float num2 = (float)m_upperBorderScore - 100f * num;
		float num3 = num * 120f + num2;
		float num4 = num * 0f + num2;
		float num5 = 120f * ((float)m_acquiredScore - num4) / (num3 - num4);
		m_endGaugeRate = num5 / 120f;
		m_endGaugeRate = Mathf.Clamp(m_endGaugeRate, 0f, 1f);
		bool isGaugePlayEnd = false;
		float value2 = 2.5f * ((num5 > 100f) ? 100f : num5) / 100f;
		value2 = Mathf.Clamp(value2, 0f, 2.5f);
		if (num5 >= 1f)
		{
			SoundManager.PlaySe("SFX_SYS_METER1").Forget();
		}
		Tweener tweener = DOVirtual.Float(0f, m_endGaugeRate, value2, delegate(float value)
		{
			m_gaugeRate = value;
			Gauge_RefleshText(m_gaugeRate);
		}).SetEase(Ease.OutSine).OnComplete(delegate
		{
			isGaugePlayEnd = true;
		});
		try
		{
			await UniTask.WaitUntil(() => isGaugePlayEnd, PlayerLoopTiming.Update, cancellationToken);
		}
		catch (OperationCanceledException)
		{
		}
		tweener.Complete();
		SoundManager.StopSe("SFX_SYS_METER1").Forget();
		Gauge_RefleshText(m_continueCretidNum, m_continuePaseliPrice);
	}

	private void Update()
	{
		m_gaugeBarAnimator.Play("SmartContinueStay", 0, m_gaugeRate);
	}
}
