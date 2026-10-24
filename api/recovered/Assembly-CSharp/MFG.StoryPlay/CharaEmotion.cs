using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MFG.StoryPlay;

public class CharaEmotion : CharaCommand
{
	private string emotion;

	public override async UniTask Parse(string[] values)
	{
		await base.Parse(values);
		type = CommandType.CharaEmotion;
		charaName = values[2];
		emotion = values[3];
	}

	public override async UniTask Exec()
	{
		await base.Exec();
		Animator charaAnimator = obj2D.GetComponent<StandingCharaController>().CharaAnimator;
		if (charaAnimator != null && emotion != "")
		{
			charaAnimator.SetTrigger(emotion);
		}
	}
}
