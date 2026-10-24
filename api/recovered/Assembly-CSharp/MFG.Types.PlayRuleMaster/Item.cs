using System.Collections.Generic;
using MFG.Scenes.Home;

namespace MFG.Types.PlayRuleMaster;

public class Item
{
	public PlayRuleType RuleType;

	public TakuType TakuType;

	public string Name;

	public List<ChangeRule> ChangeRules = new List<ChangeRule>();

	public List<Yaku> EnableYaku = new List<Yaku>();

	public List<Yaku> UnableYaku = new List<Yaku>();

	public List<(Yaku, Han)> ChangeYaku = new List<(Yaku, Han)>();
}
