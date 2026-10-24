using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace MFG.StoryPlay;

public class CharaJump : CharaCommand
{
	private int jumpX;

	private int jumpY;

	public override async UniTask Parse(string[] values)
	{
		await base.Parse(values);
		type = CommandType.CharaJump;
		charaName = values[2];
		sec = Utility.ParseSec(values[6]);
		if (values[7] == "")
		{
			jumpX = 0;
		}
		else
		{
			jumpX = int.Parse(values[7]);
		}
		if (values[9] == "")
		{
			jumpY = 0;
		}
		else
		{
			jumpY = int.Parse(values[9]);
		}
	}

	public override async UniTask Exec()
	{
		await base.Exec();
		obj2D.transform.DOLocalMove(new Vector3(jumpX, jumpY, 0f), sec / 2f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutQuart)
			.SetRelative(isRelative: true)
			.ToUniTask()
			.Forget();
	}
}
