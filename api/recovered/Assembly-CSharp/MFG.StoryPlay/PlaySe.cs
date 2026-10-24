using Cysharp.Threading.Tasks;

namespace MFG.StoryPlay;

public class PlaySe : Command
{
	public override async UniTask Parse(string[] values)
	{
		await base.Parse(values);
		type = CommandType.PlaySe;
		targetName = values[3];
	}

	public override async UniTask Exec()
	{
		SoundManager.PlaySe(targetName).Forget();
	}
}
