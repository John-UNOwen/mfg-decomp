using System.Collections.Generic;

namespace MFG.Types;

public class ExtraPlayerItemMaster
{
	public string ObjectID;

	public string Name;

	public string Description;

	public string IconGraphicPath = "";

	public string Unit = "";

	public List<string> ExtentionItems = new List<string>();
}
