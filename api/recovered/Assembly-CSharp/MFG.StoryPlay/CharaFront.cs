using Cysharp.Threading.Tasks;

namespace MFG.StoryPlay;

public class CharaFront : CharaCommand
{
	public override async UniTask Parse(string[] values)
	{
		await base.Parse(values);
		type = CommandType.CharaFront;
		charaName = values[2];
	}

	public override async UniTask Exec()
	{
		await base.Exec();
		obj2D.transform.SetAsLastSibling();
	}
}
