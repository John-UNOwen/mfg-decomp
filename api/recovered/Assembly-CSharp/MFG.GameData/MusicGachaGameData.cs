using System;
using System.Collections.Generic;
using MFG.Types;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class MusicGachaGameData
{
	private const int CurrentDataVersion = 1;

	public int DataVersion = 1;

	public const string ClientStateName = "music_gacha";

	public List<MusicGachaInfo> MusicGachaInfos = new List<MusicGachaInfo>();

	public bool IsAllItemPossession(int seriesID)
	{
		return MusicGachaInfos.Find((MusicGachaInfo x) => x.GachaID == seriesID)?.IsComplete ?? false;
	}

	public static MusicGachaGameData LoadFromJson(string jsonString)
	{
		Debug.Log("MusicGachaGameData Load : " + jsonString);
		MusicGachaGameData musicGachaGameData = JsonUtility.FromJson<MusicGachaGameData>(jsonString);
		if (musicGachaGameData == null)
		{
			musicGachaGameData = new MusicGachaGameData();
		}
		musicGachaGameData.Validate();
		return musicGachaGameData;
	}

	private void Validate()
	{
	}

	public void AcquisitionProcess()
	{
		foreach (MusicGachaInfo musicGachaInfo in MusicGachaInfos)
		{
			foreach (string possessionItemOID in musicGachaInfo.PossessionItemOIDs)
			{
				ItemUtility.AddItem(possessionItemOID);
			}
		}
	}
}
