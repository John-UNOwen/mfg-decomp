using Cysharp.Threading.Tasks;

namespace MFG.StoryPlay;

public class FinishSetUp : Command
{
	public override async UniTask Parse(string[] values)
	{
		await base.Parse(values);
		type = CommandType.FinishSetUp;
	}

	public override async UniTask Exec()
	{
	}
}
