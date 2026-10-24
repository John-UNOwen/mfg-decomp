using Cysharp.Threading.Tasks;

namespace MFG.StoryPlay;

public class Goto : Command
{
	private string nextLabel;

	public override async UniTask Parse(string[] values)
	{
		await base.Parse(values);
		type = CommandType.Goto;
		nextLabel = values[3];
	}

	public override async UniTask Exec()
	{
		StoryPlayFlow.GetInstance().SetNextCommandLabel(nextLabel);
	}
}
