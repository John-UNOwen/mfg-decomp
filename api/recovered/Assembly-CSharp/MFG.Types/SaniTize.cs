namespace MFG.Types;

public static class SaniTize
{
	public static string SanitizedString(string str, string colorCode = "", int tint = 1)
	{
		string arg = ((!string.IsNullOrEmpty(colorCode)) ? (", color=" + colorCode) : "");
		string arg2 = (str.Contains("キャトられ") ? "hearts" : "heart");
		return str.Replace("♥", $"<sprite name=\"{arg2}\" tint={tint}{arg}>").Replace("♡", $"<sprite name=\"{arg2}\" tint={tint}{arg}>");
	}

	public static bool IsSaniTized(string str)
	{
		if (!str.Contains("sprite name=\"heart\""))
		{
			return str.Contains("sprite name=\"hearts\"");
		}
		return true;
	}

	public static string AddColor(string str, string colorCode, int tint = 1)
	{
		string arg = (str.Contains("キャトられ") ? "hearts" : "heart");
		return str.Replace($"<sprite name=\"{arg}\" tint={tint}>", $"<sprite name=\"{arg}\" tint={tint}, color={colorCode}>");
	}
}
