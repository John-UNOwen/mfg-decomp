using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MFG.StoryPlay;

public class CharaPartsDisp : CharaCommand
{
	private List<int> _partsIndexes = new List<int>();

	private CharacterPartsDispController _partsDispController;

	public override async UniTask Parse(string[] values)
	{
		await base.Parse(values);
		type = CommandType.CharaPartsDisp;
		string text = values[17];
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		try
		{
			_partsIndexes = (from x in text.Split(' ')
				select int.Parse(x) - 1).ToList();
			_partsDispController = obj3D.GetComponentInChildren<CharacterPartsDispController>(includeInactive: true);
		}
		catch
		{
			Debug.Log("CharaPartsDisp: インデックスの取得に失敗しました index:" + text);
		}
	}

	public override async UniTask Exec()
	{
		await base.Exec();
		if (_partsDispController != null)
		{
			_partsDispController.SetExclusiveDisp(_partsIndexes);
		}
	}
}
