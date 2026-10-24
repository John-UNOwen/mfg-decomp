using UnityEngine.UI;

namespace TestModeSkeleton.TestModeExtensions;

public static class TestModeUITextExtensions
{
	public static void SetLocalizedText(this Text uiText, string text)
	{
		uiText.text = SingletonBehaviour<TestModeLocalization>.Instance?.GetTextFromTagText(text) ?? text;
	}
}
