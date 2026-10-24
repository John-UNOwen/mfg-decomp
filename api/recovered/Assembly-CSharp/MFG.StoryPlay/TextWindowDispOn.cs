using Cysharp.Threading.Tasks;

namespace MFG.StoryPlay;

public class TextWindowDispOn : Command
{
	public override async UniTask Parse(string[] values)
	{
		await base.Parse(values);
		type = CommandType.TextWindowDispOn;
	}

	public override async UniTask Exec()
	{
		StoryPlayFlow.GetInstance().textWindow.SetActive(value: true);
	}
}
