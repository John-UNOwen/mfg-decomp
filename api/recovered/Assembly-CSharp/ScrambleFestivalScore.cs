using System;
using System.Collections.Generic;
using MFG.Types;
using MFG.Types.ScrambleFestival;

[Serializable]
public class ScrambleFestivalScore
{
	public TeamType Team;

	public int Score;

	public int PrevScore;

	public int ChoiceRate;

	public int PrevChoiceRate;

	public List<MahjongActorID> MaList = new List<MahjongActorID>();

	public List<MahjongActorID> PrevMaList = new List<MahjongActorID>();
}
