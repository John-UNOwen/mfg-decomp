namespace MFG.Types;

public static class KazeExtentions
{
	public static HaiID ToHaiID(this Kaze kaze)
	{
		return (HaiID)(27 + (kaze - 0));
	}
}
