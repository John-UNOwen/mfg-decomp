using Cysharp.Threading.Tasks;

namespace MFG.StoryPlay;

public class PlayFullVoice : Command
{
	public override async UniTask Parse(string[] values)
	{
		await base.Parse(values);
		type = CommandType.PlayFullVoice;
		targetName = values[3];
	}

	public override async UniTask Exec()
	{
		StoryPlayFlow.GetInstance().m_playingVoiceLabel = targetName;
		await SoundManager.PlayVoice(targetName);
	}
}
