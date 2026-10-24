using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MFG.StoryPlay;

public class CharaManpu : CharaCommand
{
	private string manpu;

	public override async UniTask Parse(string[] values)
	{
		await base.Parse(values);
		type = CommandType.CharaManpu;
		charaName = values[2];
		manpu = values[3];
	}

	public override async UniTask Exec()
	{
		await base.Exec();
		Animator manpuAnimator = obj2D.GetComponent<StandingCharaController>().ManpuAnimator;
		if (manpuAnimator != null)
		{
			manpuAnimator.SetTrigger(manpu);
		}
		SoundManager.PlaySe("SFX_STR_" + manpu.Replace("Manpu", "MANPU")).Forget();
	}
}
