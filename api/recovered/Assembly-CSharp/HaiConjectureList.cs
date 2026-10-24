using System.Collections.Generic;
using MFG.Types;

public class HaiConjectureList
{
	public Dictionary<Cha, HashSet<Pai>> Safe = new Dictionary<Cha, HashSet<Pai>>();

	public Dictionary<Cha, HashSet<Pai>> SafeOnlyReach = new Dictionary<Cha, HashSet<Pai>>();

	public Dictionary<Cha, HashSet<Pai>> Danger = new Dictionary<Cha, HashSet<Pai>>();
}
