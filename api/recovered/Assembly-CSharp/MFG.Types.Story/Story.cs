using MFG.Assets;

namespace MFG.Types.Story;

public class Story
{
	public string ObjectID;

	public StoryID StoryID;

	public StoryType StoryType;

	public CharaType CharaType;

	public int Part = 1;

	public int Chapter = 1;

	public int Section = 1;

	public string Name;

	public UnlockingType UnlockingType;

	public int UnlockingCondition;

	public bool Display;

	public int Priority;

	public string DispName => SaniTize.SanitizedString(Name, "#000000");

	public string ScriptPath => AssetPathResolver.ResolveStoryScript(StoryType, CharaType, Part, Chapter);

	public string StoryImagePath => AssetPathResolver.ResolveStoryThumbnail(StoryType, CharaType, Part, Chapter);
}
