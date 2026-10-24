using System;
using Cysharp.Threading.Tasks;

namespace MFG.StoryPlay;

public class WaitSec : Command
{
	public override async UniTask Parse(string[] values)
	{
		await base.Parse(values);
		type = CommandType.WaitSec;
		sec = Utility.ParseSec(values[6]);
	}

	public override async UniTask Exec()
	{
		await UniTask.Delay(TimeSpan.FromSeconds(sec), ignoreTimeScale: false, PlayerLoopTiming.Update, token);
	}
}
