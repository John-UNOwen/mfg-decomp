using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace MFG.StoryPlay;

public class FadeInBlack : Command
{
	public override async UniTask Parse(string[] values)
	{
		await base.Parse(values);
		type = CommandType.FadeInBlack;
		sec = Utility.ParseSec(values[6]);
	}

	public override async UniTask Exec()
	{
		GameObject fadeBlack = StoryPlayFlow.GetInstance().fadeBlack;
		fadeBlack.SetActive(value: true);
		fadeBlack.GetComponent<Image>().DOFade(1f, 0f).ToUniTask()
			.Forget();
		fadeBlack.GetComponent<Image>().DOFade(0f, sec).ToUniTask()
			.Forget();
		StoryPlayFlow.GetInstance().fadeWhite.SetActive(value: false);
	}
}
