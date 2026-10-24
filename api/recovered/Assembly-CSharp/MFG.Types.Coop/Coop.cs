using System.Collections.Generic;

namespace MFG.Types.Coop;

public class Coop
{
	public string ObjectID;

	public CoopID CoopID;

	public string Name;

	public Category Category;

	public int SubCategory;

	public CharaType CharaType;

	public List<string> BoughtOIDs = new List<string>();

	public bool Available = true;

	public GameEventType UnlockEventType;
}
