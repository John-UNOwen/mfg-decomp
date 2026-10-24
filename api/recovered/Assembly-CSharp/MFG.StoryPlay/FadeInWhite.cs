using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace MFG.StoryPlay;

public class FadeInWhite : Command
{
	public override async UniTask Parse(string[] values)
	{
		await base.Parse(values);
		type = CommandType.FadeInWhite;
		sec = Utility.ParseSec(values[6]);
	}

	public override async UniTask Exec()
	{
		GameObject fadeWhite = StoryPlayFlow.GetInstance().fadeWhite;
		fadeWhite.SetActive(value: true);
		fadeWhite.GetComponent<Image>().DOFade(1f, 0f).ToUniTask()
			.Forget();
		fadeWhite.GetComponent<Image>().DOFade(0f, sec).ToUniTask()
			.Forget();
		StoryPlayFlow.GetInstance().fadeBlack.SetActive(value: false);
	}
}
