using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace MFG.StoryPlay;

public class BgChange : Command
{
	private string bgName;

	public override async UniTask Parse(string[] values)
	{
		await base.Parse(values);
		type = CommandType.BgChange;
		bgName = values[3];
		sec = Utility.ParseSec(values[6]);
	}

	public override async UniTask Exec()
	{
		GameObject nowBgObj = StoryPlayFlow.GetInstance().GetBgObj();
		GameObject nextBgObj = StoryPlayFlow.GetInstance().GetBgObj(next: true);
		await StoryPlayFlow.GetInstance().SetBg(bgName, next: true);
		nowBgObj.SetActive(value: true);
		nowBgObj.GetComponent<Image>().DOFade(1f, 0f).ToUniTask()
			.Forget();
		nowBgObj.GetComponent<Image>().DOFade(0f, sec).ToUniTask()
			.Forget();
		nextBgObj.SetActive(value: true);
		nextBgObj.GetComponent<Image>().DOFade(0f, 0f).ToUniTask()
			.Forget();
		nextBgObj.GetComponent<Image>().DOFade(1f, sec).ToUniTask()
			.Forget();
		StoryPlayFlow.GetInstance().ChangeBgName();
	}
}
