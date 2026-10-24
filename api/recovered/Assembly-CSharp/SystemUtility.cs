using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using KAM;
using KAMUNITY;
using SYSTEM;
using UnityEngine;

public class SystemUtility
{
	[Serializable]
	public class StorageInfo
	{
		[Serializable]
		public class Item
		{
			public string Name;

			public int Size;

			public int Free;
		}

		public List<Item> Items = new List<Item>();

		public static string GenerateStorageInfoJsonString()
		{
			StorageInfo storageInfo = new StorageInfo();
			DriveInfo[] drives = DriveInfo.GetDrives();
			foreach (DriveInfo driveInfo in drives)
			{
				if (driveInfo.IsReady)
				{
					storageInfo.Items.Add(new Item
					{
						Name = driveInfo.Name,
						Free = (int)(driveInfo.TotalFreeSpace / 1024 / 1024),
						Size = (int)(driveInfo.TotalSize / 1024 / 1024)
					});
				}
			}
			return JsonUtility.ToJson(storageInfo);
		}
	}

	private static SystemUtility instance;

	private string _locationID = "";

	public bool IsFactoryBoot;

	public static SystemUtility Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new SystemUtility();
			}
			return instance;
		}
		private set
		{
			instance = value;
		}
	}

	public string SoftID { get; private set; } = "VFG:J:A:A:2021092900";


	public string SystemID { get; private set; } = "012010000016B6132D36";


	public string HardwareID { get; private set; } = "01000016B6132D36";


	public string LocationID
	{
		get
		{
			if (string.IsNullOrEmpty(_locationID) && GameUtility.IsKamunityBootEnable())
			{
				Debug.LogWarning("locationID is empty");
				StringBuilder stringBuilder = new StringBuilder(32);
				AvsEa3.Ea3QueryFacilityPathString(AvsEa3.FACILITY_PATH_STRING.LOCATION_ID, stringBuilder, stringBuilder.Capacity);
				LocationID = stringBuilder.ToString();
			}
			return _locationID;
		}
		private set
		{
			_locationID = value;
		}
	}

	public REGIONTYPE RegionType { get; private set; } = REGIONTYPE.NONE;


	public PARTICIPATIONTYPE PPtype { get; private set; } = PARTICIPATIONTYPE.NONE;


	public void Initialize()
	{
		if (GameUtility.IsKamunityBootEnable())
		{
			SoftID = Avs.GetSoftIdCode();
			SystemID = Avs.GetSystemId();
			HardwareID = Avs.GetHardwareId();
			switch (SoftID[4])
			{
			case 'J':
				RegionType = REGIONTYPE.JAPAN;
				PPtype = PARTICIPATIONTYPE.NORMAL;
				break;
			case 'A':
				RegionType = REGIONTYPE.ASIA;
				PPtype = PARTICIPATIONTYPE.ASIA;
				break;
			case 'K':
				RegionType = REGIONTYPE.KOREA;
				PPtype = PARTICIPATIONTYPE.APS;
				break;
			case 'C':
				RegionType = REGIONTYPE.CHINA;
				PPtype = PARTICIPATIONTYPE.NONE;
				break;
			default:
				RegionType = REGIONTYPE.JAPAN;
				break;
			}
			StringBuilder stringBuilder = new StringBuilder(32);
			AvsEa3.Ea3QueryFacilityPathString(AvsEa3.FACILITY_PATH_STRING.LOCATION_ID, stringBuilder, stringBuilder.Capacity);
			LocationID = stringBuilder.ToString();
			EvaluateFactoryBootConditions();
		}
	}

	public string GetSoftVersionYear()
	{
		return SoftID.Substring(10, 4);
	}

	private void EvaluateFactoryBootConditions()
	{
		if (GameUtility.IsKamunityBootEnable())
		{
			AvsTypes.AVS_FS_STAT st;
			bool isFactoryBoot = Avs.FsFileIsExist("/dev/drive/g/vfgfact00.txt", out st);
			IsFactoryBoot = isFactoryBoot;
		}
	}

	public static string GenerateOSActivationDateString()
	{
		if (GameUtility.IsKonastaOrNetcafeBehavior)
		{
			return "-";
		}
		Win10ActLog.GetLastActLog(out var info);
		StringBuilder stringBuilder = new StringBuilder(32);
		if (info.count != 0)
		{
			Win10ActLog.GetTimestampString(info.lastGenerateTime, stringBuilder, stringBuilder.Capacity);
			stringBuilder.Append($"-{info.count}");
		}
		else
		{
			stringBuilder.Append("-");
		}
		return stringBuilder.ToString();
	}
}
