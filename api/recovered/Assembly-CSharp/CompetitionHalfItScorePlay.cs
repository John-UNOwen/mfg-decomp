using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.GameData;
using MFG.Types.Competition;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class CompetitionHalfItScorePlay : CompetitionScoreBase
{
	[SerializeField]
	protected GameObject _homeScoreAll;

	[SerializeField]
	protected GameObject _resultScoreAll;

	[SerializeField]
	private VerticalLayoutGroup _bgVertical;

	public override async UniTask InitAtHome(CompetitionInfo info)
	{
		_homeScoreAll.SetActive(value: true);
		_resultScoreAll.SetActive(value: false);
		await base.InitAtHome(info);
		await HomeScoreAllRefresh(info);
	}

	public override async UniTask InitAtResult(CompetitionInfo beforeCompetitionInfo, CompetitionInfo afterCompetitionInfo, CancellationToken cancellationToken = default(CancellationToken))
	{
		_homeScoreAll.SetActive(value: false);
		_resultScoreAll.SetActive(value: true);
		await base.InitAtResult(beforeCompetitionInfo, afterCompetitionInfo, cancellationToken);
		await _resultScoreAll.transform.GetComponent<CompetitionHalfItResultScore>().Init(beforeCompetitionInfo, afterCompetitionInfo);
	}

	private async UniTask HomeScoreAllRefresh(CompetitionInfo info)
	{
		if (info.CompetitionStatus == CompetitionStatus.InProgress)
		{
			_homeScoreAll.transform.Find("InProgress/BestTotalScore/ScoreNum").GetComponent<TextMeshProUGUI>().text = ((info.TotalScore < 0) ? "--.-" : ((float)info.TotalScore / 10f).ToString("F1"));
			Transform homeIntroductionParent = _homeScoreAll.transform.Find("InProgress/IntroductionParent");
			foreach (Transform item in homeIntroductionParent)
			{
				Object.Destroy(item.gameObject);
			}
			string path = $"Competition/{info.CompetitionID}/Introduction.prefab";
			if (await AssetLoader.Exists(path))
			{
				IDisposableAsset<GameObject> obj = await AssetLoader.LoadAssetAsync<GameObject>(path);
				obj.AddTo(this);
				Object.Instantiate(obj.Asset, homeIntroductionParent);
			}
			_homeScoreAll.SetActive(value: true);
			_bgVertical.spacing = 12f;
			_bgVertical.padding.top = 50;
		}
		else
		{
			_homeScoreAll.SetActive(value: false);
			_bgVertical.spacing = 65f;
			_bgVertical.padding.top = 77;
		}
	}

	public override async UniTask ResultAnimationPlay(CompetitionInfo afterCompetitionInfo, CancellationToken token = default(CancellationToken))
	{
		await _resultScoreAll.transform.GetComponent<CompetitionHalfItResultScore>().Play();
		if (afterCompetitionInfo.IsHiScore)
		{
			await HiScoreAnimationPlay(afterCompetitionInfo, token);
		}
	}
}
