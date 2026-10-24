using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace MFG.StoryPlay;

public class CharaShake : CharaCommand
{
	private int shakeX;

	private int shakeY;

	public override async UniTask Parse(string[] values)
	{
		await base.Parse(values);
		type = CommandType.CharaShake;
		charaName = values[2];
		sec = Utility.ParseSec(values[6]);
		if (values[7] == "")
		{
			shakeX = 0;
		}
		else
		{
			shakeX = int.Parse(values[7]);
		}
		if (values[9] == "")
		{
			shakeY = 0;
		}
		else
		{
			shakeY = int.Parse(values[9]);
		}
	}

	public override async UniTask Exec()
	{
		await base.Exec();
		obj2D.transform.DOPunchPosition(new Vector3(shakeX, shakeY, 0f), sec).ToUniTask().Forget();
	}
}
