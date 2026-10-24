using System;
using System.Collections.Generic;
using DG.Tweening;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.Costume;
using UnityEngine;
using UnityEngine.UI;

namespace MFG.StoryPlay;

internal class Utility
{
	private static Dictionary<string, string> charaNameToObjectName = new Dictionary<string, string>();

	private static Dictionary<string, string> bgNameToObjectName = new Dictionary<string, string>();

	public static void ResetDictionary()
	{
		charaNameToObjectName.Clear();
		bgNameToObjectName.Clear();
	}

	public static void AddCharaName(string charaName, string objectName)
	{
		charaNameToObjectName.Add(charaName, objectName);
	}

	public static bool IsCharaNameContain(string charaName)
	{
		return charaNameToObjectName.ContainsKey(charaName);
	}

	public static ItemID GetCostumeItemID(string charaName, string costumeIDString)
	{
		if (Enum.TryParse<ItemID>(costumeIDString, ignoreCase: true, out var result))
		{
			return result;
		}
		if (Enum.TryParse<ItemID>(charaName, ignoreCase: true, out result))
		{
			return result;
		}
		if (!int.TryParse(costumeIDString, out var result2))
		{
			int num = ((costumeIDString == "制服") ? 1 : ((!(costumeIDString == "デビュー")) ? 1 : 2));
			result2 = num;
		}
		if (!charaNameToObjectName.ContainsKey(charaName))
		{
			return ItemID.None;
		}
		if (!Enum.TryParse<ItemID>($"{charaNameToObjectName[charaName]}_{result2:D3}", ignoreCase: true, out result))
		{
			return CostumeItemMaster.GetDefaultEquipItemID(GetCharaType(charaName));
		}
		return result;
	}

	public static CharaType GetCharaType(string charaName)
	{
		if (charaNameToObjectName.ContainsKey(charaName))
		{
			return (CharaType)(int.Parse(charaNameToObjectName[charaName].Replace("Costume", "")) - 1);
		}
		Debug.LogError("変換表にキャラ名が見つかりません:" + charaName);
		return CharaType.Chara01;
	}

	public static string GetBgObjectName(string bgName)
	{
		if (bgNameToObjectName.ContainsKey(bgName))
		{
			return bgNameToObjectName[bgName];
		}
		Debug.LogError("変換表に背景名が見つかりません:" + bgName);
		return null;
	}

	public static void AddBgName(string bgName, string objectName)
	{
		bgNameToObjectName.Add(bgName, objectName);
	}

	public static void SetCharaColor(GameObject charaObj, Color color, float sec = 0.1f)
	{
		charaObj.transform.Find("Base").Find("RawImage").GetComponent<RawImage>()
			.DOColor(color, sec);
	}

	public static void SetImageColor(GameObject imageObj, Color color, float sec = 0.1f)
	{
		imageObj.transform.GetComponent<Image>().DOColor(color, sec);
	}

	public static float ParseSec(string value, float sec = 1f)
	{
		if (value != "")
		{
			sec = float.Parse(value);
		}
		return sec;
	}

	public static int GetPosFromValue(string value, float pos)
	{
		if (value == "")
		{
			return (int)pos;
		}
		return int.Parse(value);
	}
}
