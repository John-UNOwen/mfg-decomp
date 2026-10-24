using System.Collections.Generic;

public class YakuPossibilityInfoComparer : EqualityComparer<YakuPossibilityInfo>
{
	public override bool Equals(YakuPossibilityInfo a, YakuPossibilityInfo b)
	{
		if (a.Yaku == b.Yaku)
		{
			return a.Possibility == b.Possibility;
		}
		return false;
	}

	public override int GetHashCode(YakuPossibilityInfo a)
	{
		return (int)a.Yaku ^ (int)a.Possibility;
	}
}
