namespace MFG.Types;

public static class YakuExtention
{
	public static bool IsNakiYaku(Yaku yaku)
	{
		if (Yaku.ChinisoNaki <= yaku)
		{
			return yaku <= Yaku.IkkitsukanNaki;
		}
		return false;
	}
}
