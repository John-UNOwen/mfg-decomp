namespace MFG.Types;

public static class BaExtentions
{
	public static HaiID ToHaiID(this Ba ba)
	{
		return (HaiID)(27 + (ba - 0));
	}
}
