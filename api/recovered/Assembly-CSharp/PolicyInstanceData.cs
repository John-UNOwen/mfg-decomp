using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Konami.System.Net;
using UnityEngine;

public class PolicyInstanceData
{
	[SerializeField]
	private Dictionary<Policy, int> revision = new Dictionary<Policy, int>();

	public bool IsEmpty => revision.Count() == 0;

	public int GetRevision(Policy kind)
	{
		if (revision.ContainsKey(kind))
		{
			return revision[kind];
		}
		return 0;
	}

	public void Parse(object _obj)
	{
		Dictionary<string, object> dictionary = _obj as Dictionary<string, object>;
		if (!dictionary.ContainsKey("need_kind") || !dictionary.ContainsKey("need_rev"))
		{
			return;
		}
		IList list = dictionary["need_kind"] as IList;
		IList list2 = dictionary["need_rev"] as IList;
		if (list.Count != list2.Count)
		{
			return;
		}
		for (int i = 0; i < list2.Count; i++)
		{
			Policy key = (Policy)Enum.Parse(typeof(Policy), list[i].ToString());
			int value = int.Parse(list2[i].ToString());
			if (revision.ContainsKey(key))
			{
				revision[key] = value;
			}
			else
			{
				revision.Add(key, value);
			}
		}
	}
}
