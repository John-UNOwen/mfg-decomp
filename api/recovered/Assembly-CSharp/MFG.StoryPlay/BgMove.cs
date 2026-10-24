using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace MFG.StoryPlay;

public class BgMove : Command
{
	private int posX1;

	private int posX2;

	private int posY1;

	private int posY2;

	private string valueX1;

	private string valueX2;

	private string valueY1;

	private string valueY2;

	public override async UniTask Parse(string[] values)
	{
		await base.Parse(values);
		type = CommandType.BgMove;
		sec = Utility.ParseSec(values[6]);
		valueX1 = values[7];
		valueX2 = values[8];
		valueY1 = values[9];
		valueY2 = values[10];
		if (valueX1 == "" && valueY1 == "")
		{
			enable1 = false;
		}
		if (valueX2 == "" && valueY2 == "")
		{
			enable2 = false;
		}
	}

	public override async UniTask Exec()
	{
		if (StoryPlayFlow.GetInstance().bgName == "")
		{
			Debug.LogError("背景が設定されていません:BgMove");
			return;
		}
		GameObject bgObj = StoryPlayFlow.GetInstance().GetBgObj();
		bgObj.SetActive(value: true);
		posX1 = Utility.GetPosFromValue(valueX1, bgObj.transform.localPosition.x);
		posY1 = Utility.GetPosFromValue(valueY1, bgObj.transform.localPosition.y);
		posX2 = Utility.GetPosFromValue(valueX2, bgObj.transform.localPosition.x);
		posY2 = Utility.GetPosFromValue(valueY2, bgObj.transform.localPosition.y);
		if (enable1)
		{
			bgObj.transform.localPosition = new Vector3(posX1, posY1, 0f);
		}
		if (enable2)
		{
			bgObj.transform.DOLocalMove(new Vector3(posX2, posY2, 0f), sec).SetEase(Ease.Linear).ToUniTask()
				.Forget();
		}
	}
}
