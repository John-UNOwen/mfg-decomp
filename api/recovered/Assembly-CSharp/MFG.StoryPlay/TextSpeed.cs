using Cysharp.Threading.Tasks;

namespace MFG.StoryPlay;

public class TextSpeed : Command
{
	private float speed;

	public override async UniTask Parse(string[] values)
	{
		await base.Parse(values);
		type = CommandType.TextSpeed;
		speed = float.Parse(values[3]);
	}

	public override async UniTask Exec()
	{
		StoryPlayFlow.GetInstance().m_textSpeedDefault = speed;
	}
}
