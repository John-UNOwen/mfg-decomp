using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace MFG.StoryPlay;

public class ImageAlpha : Command
{
	private float alpha1;

	private float alpha2;

	public override async UniTask Parse(string[] values)
	{
		await base.Parse(values);
		type = CommandType.ImageAlpha;
		targetName = values[3];
		sec = Utility.ParseSec(values[6]);
		if (values[11] == "")
		{
			enable1 = false;
		}
		else
		{
			alpha1 = float.Parse(values[11]);
		}
		if (values[12] == "")
		{
			enable2 = false;
		}
		else
		{
			alpha2 = float.Parse(values[12]);
		}
	}

	public override async UniTask Exec()
	{
		if (enable1 || enable2)
		{
			await base.Exec();
			Transform transform = StoryPlayFlow.GetInstance().imageTop.transform.Find(targetName);
			if (enable1)
			{
				transform.GetComponent<Image>().DOFade(alpha1, 0f).ToUniTask()
					.Forget();
			}
			if (enable2)
			{
				transform.GetComponent<Image>().DOFade(alpha2, sec).ToUniTask()
					.Forget();
			}
		}
	}
}
