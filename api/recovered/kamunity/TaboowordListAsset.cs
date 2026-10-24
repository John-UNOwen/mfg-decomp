using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "tabooword_list", menuName = "KAMUNITY/tabooword", order = 1)]
public class TaboowordListAsset : ScriptableObject
{
	[SerializeField]
	public List<string> m_PartialMatchList;

	[SerializeField]
	public List<string> m_PerfectMatchList;

	[SerializeField]
	public List<string> m_ForwardMatchList;

	[SerializeField]
	public List<string> m_BackwardMatchList;

	[SerializeField]
	public List<string> m_WhiteList;
}
