using System.Collections.Generic;
using MFG.Types;

public class ConjectureInfo
{
	public Pai pai;

	public List<Cha> safeList = new List<Cha>();

	public List<Cha> dangerList = new List<Cha>();

	public ConjectureInfo(Pai p, Cha s, Cha d)
	{
		pai = p;
		if (s != 0)
		{
			safeList.Add(s);
		}
		if (d != 0)
		{
			dangerList.Add(d);
		}
	}
}
