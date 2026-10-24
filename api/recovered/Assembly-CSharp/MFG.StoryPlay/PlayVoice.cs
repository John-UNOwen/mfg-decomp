using Cysharp.Threading.Tasks;

namespace MFG.StoryPlay;

public class PlayVoice : Command
{
	public override async UniTask Parse(string[] values)
	{
		await base.Parse(values);
		type = CommandType.PlayVoice;
		targetName = values[3];
	}

	public override async UniTask Exec()
	{
		SoundManager.PlayVoice(targetName).Forget();
	}
}
