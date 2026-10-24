using Cysharp.Threading.Tasks;

namespace MFG.StoryPlay;

public class FadeOutAnime : Command
{
	public override async UniTask Parse(string[] values)
	{
		await base.Parse(values);
		type = CommandType.FadeOutAnime;
		sec = Utility.ParseSec(values[6]);
	}

	public override async UniTask Exec()
	{
		await ResidentFlow.GetInstance().TransitionEffectOpen("StoryPlay");
	}
}
