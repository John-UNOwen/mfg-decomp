using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MFG.GameData;
using MFG.Types.Competition;
using TMPro;
using UnityEngine;

public class CompetitionRankGraph : MonoBehaviour
{
	[SerializeField]
	private GameObject graphBase;

	[SerializeField]
	private GameObject arrow;

	[SerializeField]
	private GameObject top70Percent;

	[SerializeField]
	private GameObject top50Percent;

	[SerializeField]
	private TextMeshProUGUI untilText;

	[SerializeField]
	private CompetitionScoreDisp untilScoreDisp;

	private Dictionary<int, int[]> BorderLines = new Dictionary<int, int[]>
	{
		{
			0,
			new int[6] { 0, 0, 100, 300, 700, 1000 }
		},
		{
			1,
			new int[6] { 0, 0, 100, 300, 500, 1000 }
		},
		{
			2,
			new int[6] { 0, 10, 100, 300, 500, 1000 }
		}
	};

	public async UniTask Init(CompetitionInfo info, bool isEnableStatus = true, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (isEnableStatus)
		{
			base.gameObject.SetActive(value: true);
			await GraphReflesh(info, isArrowAnimation: false, cancellationToken);
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
		UntilTextReflesh(info);
	}

	public async UniTask GraphReflesh(CompetitionInfo competitionInfo, bool isArrowAnimation = true, CancellationToken cancellationToken = default(CancellationToken))
	{
		int rankRate = Math.Clamp(competitionInfo.RankRate, -1, 1000);
		await GraphReflesh(rankRate, competitionInfo.RankRateCategoryType, competitionInfo._rankRateCategory, isArrowAnimation, cancellationToken);
	}

	public async UniTask GraphReflesh(int rankRate, int rankCategory, int rankRateCategory, bool isArrowAnimation = true, CancellationToken cancellationToken = default(CancellationToken))
	{
		try
		{
			top70Percent.SetActive(rankCategory == 0);
			top50Percent.SetActive(rankCategory >= 1);
			int num = BorderLines[rankCategory][rankRateCategory];
			int min = ((rankRateCategory > 0) ? (BorderLines[rankCategory][rankRateCategory - 1] + 1) : 0);
			float num2 = (isArrowAnimation ? 1f : 0f);
			RectTransform rectTransform = graphBase.transform as RectTransform;
			RectTransform rectTransform2 = arrow.transform as RectTransform;
			float num3 = rectTransform.offsetMin.x + 11f;
			float num4 = rectTransform.offsetMax.x - 11f;
			_ = rankRate;
			float x;
			switch (rankRateCategory)
			{
			case 1:
				x = num4 - 12f;
				break;
			case 2:
				x = ((!((float)rankRate <= (float)num * 0.7f)) ? (num3 + 0.9f * (num4 - num3) + 16f) : (num3 + 0.95f * (num4 - num3) + 2f));
				break;
			default:
			{
				if (rankRate <= 0)
				{
				}
				int num5 = Math.Clamp(rankRate, min, num);
				float num6 = (float)(10 - (num5 + 99) / 100) * 0.1f + 0.05f;
				x = num3 + num6 * (num4 - num3);
				break;
			}
			}
			if (rankRate < 0 || rankRateCategory == 0)
			{
				arrow.SetActive(value: false);
				arrow.transform.localPosition = new Vector3(num3, rectTransform2.anchoredPosition.y, 0f);
				return;
			}
			arrow.SetActive(value: true);
			if (num2 == 0f)
			{
				arrow.transform.localPosition = new Vector3(x, rectTransform2.anchoredPosition.y, 0f);
			}
			else
			{
				await arrow.transform.DOLocalMove(new Vector3(x, rectTransform2.anchoredPosition.y, 0f), num2).SetEase(Ease.OutQuart).ToUniTask();
			}
		}
		catch (OperationCanceledException)
		{
		}
	}

	public void UntilTextReflesh(CompetitionInfo competitionInfo)
	{
		int nextBorderRankRateCategory = competitionInfo._nextBorderRankRateCategory;
		if (competitionInfo.CompetitionStatus == CompetitionStatus.None || nextBorderRankRateCategory == 0)
		{
			untilText.text = "";
			untilScoreDisp.Reflesh("");
		}
		else
		{
			int untilNextBorderScore = competitionInfo.UntilNextBorderScore;
			untilText.text = competitionInfo.RankRateLabel(isNext: true) + "まであと";
			untilScoreDisp.Reflesh(untilNextBorderScore, nextBorderRankRateCategory == 1);
		}
	}

	public async UniTask UntilTextAnimation(CompetitionInfo competitionInfo, CancellationToken cancellationToken)
	{
		await UniTask.WhenAll(untilScoreDisp.Fade(0f, 0.5f, cancellationToken), untilText.DOFade(0f, 0.5f).ToUniTask(TweenCancelBehaviour.Kill, cancellationToken));
		UntilTextReflesh(competitionInfo);
		await UniTask.WhenAll(untilScoreDisp.Fade(1f, 0.5f, cancellationToken), untilText.DOFade(1f, 0.5f).ToUniTask(TweenCancelBehaviour.Kill, cancellationToken));
	}
}
