using TMPro;
using UnityEngine;

public class CompetitionRankRateDisp : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _rankRateText;

	public void Reflesh(int rank, int rankRate)
	{
		_rankRateText.text = RankRateToString(rank, rankRate);
	}

	public static string RankRateToString(int rank, int rankRate)
	{
		if (rank <= 0)
		{
			return "<size=64>--.--";
		}
		if (1 <= rank && rank <= 10)
		{
			return $"<size=64>{rank}<size=36>位";
		}
		return $"<size=36>上位 <size=64>{rankRate / 10}.{rankRate % 10}<size=36>%";
	}
}
