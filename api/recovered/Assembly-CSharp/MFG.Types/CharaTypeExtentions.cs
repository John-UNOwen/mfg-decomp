using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MFG.Types;

public static class CharaTypeExtentions
{
	private static Regex _charaTypeRegex = new Regex("OID_CHARACTER_(\\d+)");

	public static string ToObjectID(this CharaType charaType)
	{
		return $"OID_CHARACTER_{(int)(charaType + 1)}";
	}

	public static CharaType ToCharaType(string oid)
	{
		try
		{
			return Enum.Parse<CharaType>($"Chara{int.Parse(_charaTypeRegex.Match(oid).Groups[1].Value):D2}");
		}
		catch
		{
			Debug.LogError("CharaTypeExtentions.ToCharaType CharaTypeに変換できませんでした oid:" + oid);
			return CharaType.Chara01;
		}
	}

	public static bool IsFakeClear(this CharaType charaType, int charaVariantID)
	{
		if (charaType == CharaType.Chara12)
		{
			return charaVariantID > 0;
		}
		return false;
	}
}
