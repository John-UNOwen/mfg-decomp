namespace MFG.Types.Competition;

public static class CompetitionIDExtentions
{
	public static bool IsKAC2023(this CompetitionID competitionID)
	{
		if (competitionID != CompetitionID.Competition003)
		{
			return competitionID == CompetitionID.Competition004;
		}
		return true;
	}
}
