using Cysharp.Threading.Tasks;
using MFG.GameData;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class CompetitionPrizeWindow : MonoBehaviour
{
	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private GameObject cellPrefab;

	[SerializeField]
	private Transform contents;

	[SerializeField]
	private Transform winPrizeList;

	[SerializeField]
	private Transform joinPrizeList;

	[SerializeField]
	private TextMeshProUGUI prizeText;

	public void Open()
	{
		SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
		base.gameObject.SetActive(value: true);
		winPrizeList.GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical();
		winPrizeList.GetComponent<VerticalLayoutGroup>().SetLayoutVertical();
		LayoutRebuilder.MarkLayoutForRebuild(winPrizeList.GetComponent<RectTransform>());
		LayoutRebuilder.ForceRebuildLayoutImmediate(winPrizeList.GetComponent<RectTransform>());
		LayoutRebuilder.MarkLayoutForRebuild(contents.GetComponent<RectTransform>());
		LayoutRebuilder.ForceRebuildLayoutImmediate(contents.GetComponent<RectTransform>());
	}

	public void Init()
	{
		CompetitionInfo inProgressCompetitionInfo = GameDataManager.Competition.GetInProgressCompetitionInfo();
		if (inProgressCompetitionInfo == null)
		{
			return;
		}
		int count = inProgressCompetitionInfo.WinRanks.Count;
		for (int i = 0; i < count; i++)
		{
			GameObject gameObject = Object.Instantiate(cellPrefab, winPrizeList);
			if (inProgressCompetitionInfo.WinRanks.Count > i)
			{
				gameObject.transform.Find("Rank/RankText").GetComponent<TextMeshProUGUI>().text = inProgressCompetitionInfo.WinRanks[i];
			}
			if (inProgressCompetitionInfo.WinPrizes.Count > i)
			{
				gameObject.transform.Find("Prize/PrizeText").GetComponent<TextMeshProUGUI>().text = inProgressCompetitionInfo.WinPrizes[i];
			}
		}
		string joinPrize = inProgressCompetitionInfo.JoinPrize;
		if (joinPrize == null || joinPrize == "")
		{
			joinPrizeList.gameObject.SetActive(value: false);
		}
		else
		{
			joinPrizeList.gameObject.SetActive(value: true);
			prizeText.text = joinPrize;
		}
		closeButton.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			base.gameObject.SetActive(value: false);
		}).AddTo(this);
	}
}
