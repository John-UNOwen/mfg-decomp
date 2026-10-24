using Cysharp.Threading.Tasks;

namespace MFG.StoryPlay;

public class StopBgm : Command
{
	public override async UniTask Parse(string[] values)
	{
		await base.Parse(values);
		type = CommandType.StopBgm;
		targetName = values[3];
	}

	public override async UniTask Exec()
	{
		SoundManager.StopBgmWithFadeout().Forget();
	}
}
