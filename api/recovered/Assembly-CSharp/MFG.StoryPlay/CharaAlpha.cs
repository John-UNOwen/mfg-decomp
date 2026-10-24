using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine.UI;

namespace MFG.StoryPlay;

public class CharaAlpha : CharaCommand
{
	private float alpha1;

	private float alpha2;

	public override async UniTask Parse(string[] values)
	{
		await base.Parse(values);
		type = CommandType.CharaAlpha;
		charaName = values[2];
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
			if (enable1)
			{
				obj2D.transform.Find("Base").Find("RawImage").GetComponent<RawImage>()
					.DOFade(alpha1, 0f)
					.ToUniTask()
					.Forget();
			}
			if (enable2)
			{
				obj2D.transform.Find("Base").Find("RawImage").GetComponent<RawImage>()
					.DOFade(alpha2, sec)
					.ToUniTask()
					.Forget();
			}
		}
	}
}
