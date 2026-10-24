using Cysharp.Threading.Tasks;

namespace MFG.StoryPlay;

public class PlayBgm : Command
{
	public override async UniTask Parse(string[] values)
	{
		await base.Parse(values);
		type = CommandType.PlayBgm;
		targetName = values[3];
	}

	public override async UniTask Exec()
	{
		SoundManager.PlayBgm(targetName).Forget();
	}
}
