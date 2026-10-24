using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace MFG.StoryPlay;

public class ImageMove : Command
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
		type = CommandType.ImageMove;
		targetName = values[3];
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
		if (!enable1 && !enable2)
		{
			return;
		}
		await base.Exec();
		Transform transform = StoryPlayFlow.GetInstance().imageTop.transform.Find(targetName);
		posX1 = Utility.GetPosFromValue(valueX1, transform.transform.localPosition.x);
		posY1 = Utility.GetPosFromValue(valueY1, transform.transform.localPosition.y);
		posX2 = Utility.GetPosFromValue(valueX2, transform.transform.localPosition.x);
		posY2 = Utility.GetPosFromValue(valueY2, transform.transform.localPosition.y);
		if (enable1)
		{
			transform.transform.localPosition = new Vector3(posX1, posY1, 0f);
		}
		if (enable2)
		{
			transform.transform.DOLocalMove(new Vector3(posX2, posY2, 0f), sec).SetEase(Ease.OutQuart).ToUniTask()
				.Forget();
			if (Mathf.Abs(posX2) <= 1000)
			{
				Mathf.Abs(posY2);
				_ = 1000;
			}
		}
	}
}
