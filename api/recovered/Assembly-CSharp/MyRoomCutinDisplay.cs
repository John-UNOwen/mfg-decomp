using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.Types.Customize.CutinItem;
using UnityEngine;

public class MyRoomCutinDisplay : MyRoomDisplayBase
{
	[SerializeField]
	[Header("0:無,1:鳴き,2:リーチ,3:小アガリ,4:大アガリ")]
	private List<CustomizeItemPanel> actIconList;

	public override void SetStatus(CancellationToken cancellationToken)
	{
		SetCutinThumbnail(Category.Naki, cancellationToken);
		SetCutinThumbnail(Category.Reach, cancellationToken);
		SetCutinThumbnail(Category.AgariSmall, cancellationToken);
		SetCutinThumbnail(Category.AgariBig, cancellationToken);
	}

	private void SetCutinThumbnail(Category category, CancellationToken cancellationToken)
	{
		int count = (from a in GameDataManager.Customize.CutinItems
			where a.Master.CharaType == GameDataManager.Player.SelectChara
			where a.Master.Category == category
			select a).ToList().Count;
		List<CutinItemInfo> list = (from a in GameDataManager.Customize.CutinItems
			where a.Master.CharaType == GameDataManager.Player.SelectChara
			where a.Master.Category == category
			where a.IsOn
			select a).ToList();
		CustomizeItemPanel customizeItemPanel = actIconList[(int)category];
		if (customizeItemPanel != null)
		{
			if (list.Count == 1)
			{
				CutinItemInfo info = list.First();
				customizeItemPanel.Reflesh(info, isDispState: false, cancellationToken).Forget();
			}
			else if (count == 0)
			{
				customizeItemPanel.SetNoneSprite(category);
			}
			else
			{
				customizeItemPanel.SetRandomSprite(category);
			}
		}
	}
}
