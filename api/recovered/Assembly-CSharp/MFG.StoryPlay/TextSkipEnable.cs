using Cysharp.Threading.Tasks;

namespace MFG.StoryPlay;

public class TextSkipEnable : Command
{
	public override async UniTask Parse(string[] values)
	{
		await base.Parse(values);
		type = CommandType.TextSkipEnable;
	}

	public override async UniTask Exec()
	{
		StoryPlayFlow.GetInstance().m_textSkipEnable = true;
	}
}
