using System;
using MFG.MasterGameData;
using MFG.Types;
using UnityEngine;

namespace MFG.Assets;

[Serializable]
public class CharaProfile
{
	public CharaType CharaType;

	public string Name;

	public string ShortName;

	public string CV;

	public Color Color;

	public SchoolID SchoolID;

	public Yaku Yaku1;

	public Yaku Yaku2;

	[TextArea(1, 4)]
	public string ProfileText;

	public string SchoolName => SchoolMaster.GetItem(SchoolID)?.Name ?? "";
}
