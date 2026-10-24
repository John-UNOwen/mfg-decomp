using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace MFG.StoryPlay;

public class ImageScale : Command
{
	private float scale1;

	private float scale2;

	public override async UniTask Parse(string[] values)
	{
		await base.Parse(values);
		type = CommandType.ImageScale;
		targetName = values[3];
		sec = Utility.ParseSec(values[6]);
		if (values[13] == "")
		{
			enable1 = false;
		}
		else
		{
			scale1 = float.Parse(values[13]);
		}
		if (values[14] == "")
		{
			enable2 = false;
		}
		else
		{
			scale2 = float.Parse(values[14]);
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
				transform.transform.localScale = new Vector3(scale1, scale1, 0f);
			}
			if (enable2)
			{
				transform.transform.DOScale(new Vector3(scale2, scale2, 0f), sec).SetEase(Ease.OutQuart).ToUniTask()
					.Forget();
			}
		}
	}
}
