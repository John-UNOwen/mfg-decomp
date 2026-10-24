using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using UnityEngine;

public class MyRoomMatchingBGDisplay : MyRoomDisplayBase
{
	[SerializeField]
	private List<MatchingBGPanel> panels;

	public override void SetStatus(CancellationToken cancellationToken)
	{
		_ = GameDataManager.Customize.MatchingBGItems.Where((MatchingBGInfo a) => a.Master.CharaType == GameDataManager.Player.SelectChara).ToList().Count;
		List<MatchingBGInfo> list = (from a in GameDataManager.Customize.MatchingBGItems
			where a.Master.CharaType == GameDataManager.Player.SelectChara
			where a.IsOn
			orderby a.Master.ItemID
			select a).ToList();
		if (list.Count <= 0)
		{
			list = (from a in GameDataManager.Customize.MatchingBGItems
				where a.Master.CharaType == GameDataManager.Player.SelectChara
				orderby a.Master.ItemID
				select a).ToList();
		}
		for (int i = 0; i < 4; i++)
		{
			if (i >= list.Count)
			{
				panels[i].gameObject.SetActive(value: false);
			}
			else if (i == 3 && list.Count > 4)
			{
				panels[i].gameObject.SetActive(value: true);
				panels[i].SetOverSprite();
			}
			else
			{
				panels[i].gameObject.SetActive(value: true);
				panels[i].Reflesh(list[i], isDispState: false, cancellationToken).Forget();
			}
		}
	}
}
