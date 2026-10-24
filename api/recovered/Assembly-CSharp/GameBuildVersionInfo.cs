using System;
using UnityEngine;

[Serializable]
public class GameBuildVersionInfo : ScriptableObject
{
	[SerializeField]
	private string m_buildDateTimeString = new DateTime(2000, 1, 1).ToString();

	private const string VERSION_INFO_PATH = "GameBuildVersionInfo";

	private const string ASSET_PATH = "Assets/MFG/Resources/GameBuildVersionInfo.asset";

	public string BuildDateTimeString
	{
		get
		{
			return m_buildDateTimeString;
		}
		private set
		{
			m_buildDateTimeString = value;
		}
	}

	public static GameBuildVersionInfo LoadBuildVersionInfo()
	{
		return Load("GameBuildVersionInfo");
	}

	private static GameBuildVersionInfo Load(string path)
	{
		return Resources.Load<GameBuildVersionInfo>(path);
	}
}
