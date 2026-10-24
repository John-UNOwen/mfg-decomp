using Cysharp.Threading.Tasks;

namespace MFG.StoryPlay;

public class FadeInAnime : Command
{
	public override async UniTask Parse(string[] values)
	{
		await base.Parse(values);
		type = CommandType.FadeInAnime;
		sec = Utility.ParseSec(values[6]);
	}

	public override async UniTask Exec()
	{
		await ResidentFlow.GetInstance().TransitionEffectClose();
	}
}
