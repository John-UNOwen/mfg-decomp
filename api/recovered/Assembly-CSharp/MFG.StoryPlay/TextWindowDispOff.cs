using Cysharp.Threading.Tasks;

namespace MFG.StoryPlay;

public class TextWindowDispOff : Command
{
	public override async UniTask Parse(string[] values)
	{
		await base.Parse(values);
		type = CommandType.TextWindowDispOff;
	}

	public override async UniTask Exec()
	{
		StoryPlayFlow.GetInstance().textWindow.SetActive(value: false);
	}
}
