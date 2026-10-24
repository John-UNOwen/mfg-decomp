using Cysharp.Threading.Tasks;

namespace MFG.StoryPlay;

public class TextSkipDisable : Command
{
	public override async UniTask Parse(string[] values)
	{
		await base.Parse(values);
		type = CommandType.TextSkipDisable;
	}

	public override async UniTask Exec()
	{
		StoryPlayFlow.GetInstance().m_textSkipEnable = false;
	}
}
