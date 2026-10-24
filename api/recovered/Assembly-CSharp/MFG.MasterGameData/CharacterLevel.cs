using System.Collections.Generic;

namespace MFG.MasterGameData;

public class CharacterLevel
{
	private static List<int> m_expRequiredForNextLevel = new List<int>();

	private List<int> ExpRequiredForNextLevel => m_expRequiredForNextLevel;

	public CharacterLevel()
	{
		m_expRequiredForNextLevel.Clear();
		m_expRequiredForNextLevel.Add(0);
		for (int i = 0; i < 99999; i++)
		{
			m_expRequiredForNextLevel.Add(i * 100);
		}
	}

	public int GetExpRequiredForNextLevel(int level)
	{
		if (level < 0 || level >= ExpRequiredForNextLevel.Count)
		{
			return 0;
		}
		int totalExpRequired = GetTotalExpRequired(level);
		int totalExpRequired2 = GetTotalExpRequired(level + 1);
		if (totalExpRequired2 == 0)
		{
			return 0;
		}
		return totalExpRequired2 - totalExpRequired;
	}

	public int GetTotalExpRequired(int level)
	{
		if (level < 0 || level >= ExpRequiredForNextLevel.Count)
		{
			return 0;
		}
		return ExpRequiredForNextLevel[level];
	}

	public (int currentLevel, int minExpAtCurrentLevel, int maxExpAtCurrentLevel) GetCurrentLevelAndExpBorder(int totalExp)
	{
		int i;
		for (i = 1; i < ExpRequiredForNextLevel.Count - 1 && totalExp >= ExpRequiredForNextLevel[i + 1]; i++)
		{
		}
		return (currentLevel: i, minExpAtCurrentLevel: ExpRequiredForNextLevel[i], maxExpAtCurrentLevel: (i + 1 < ExpRequiredForNextLevel.Count) ? ExpRequiredForNextLevel[i + 1] : 0);
	}
}
