using System;
using System.Collections.Generic;
using MFG.Types.ScrambleFestival;

[Serializable]
public class ScrambleFestivalInfo
{
	public GameEventType EventType;

	public List<TeamType> WinnerTeam = new List<TeamType>();

	public List<ScrambleFestivalScore> ScoreInfos;

	public string RefleshDate = "";
}
