using System.Collections.Generic;

public class ConjectureInfos
{
	public List<ConjectureInfo> Info;

	public int safeChaNum;

	public int dangerChaNum;

	public ConjectureInfos()
	{
		Info = new List<ConjectureInfo>();
		safeChaNum = 0;
		dangerChaNum = 0;
	}
}
