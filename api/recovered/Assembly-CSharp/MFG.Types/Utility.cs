namespace MFG.Types;

public class Utility
{
	public static string GetSign(int value)
	{
		if (value > 0)
		{
			return "＋";
		}
		if (value < 0)
		{
			return "－";
		}
		return "";
	}
}
