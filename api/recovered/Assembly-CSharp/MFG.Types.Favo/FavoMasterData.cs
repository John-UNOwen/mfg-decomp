using MFG.MasterGameData;

namespace MFG.Types.Favo;

public class FavoMasterData
{
	public string ObjectID;

	public FavoItemID ID;

	public int FavoNum;

	public FavoItemType type;

	public string unlockVoiceID;

	public string _unlockVoiceName;

	public string GetName(CharaType charaType)
	{
		return SaniTize.SanitizedString(type switch
		{
			FavoItemType.Voice => _unlockVoiceName, 
			FavoItemType.Cutin => _unlockVoiceName, 
			FavoItemType.Bgm => _unlockVoiceName.Replace("[曲名]", MasterGameDataManager.Character.Properties(charaType).MusicName), 
			FavoItemType.Story => _unlockVoiceName.Replace("[キャラの下の名前]", MasterGameDataManager.Character.Properties(charaType).Profile.ShortName), 
			_ => "", 
		}, "#000000");
	}
}
