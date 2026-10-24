using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace MFG.StoryPlay;

public class ImageRotateY : Command
{
	private float rotate1;

	private float rotate2;

	public override async UniTask Parse(string[] values)
	{
		await base.Parse(values);
		type = CommandType.ImageRotateY;
		targetName = values[3];
		sec = Utility.ParseSec(values[6]);
		if (values[15] == "")
		{
			enable1 = false;
		}
		else
		{
			rotate1 = float.Parse(values[15]);
		}
		if (values[16] == "")
		{
			enable2 = false;
		}
		else
		{
			rotate2 = float.Parse(values[16]);
		}
	}

	public override async UniTask Exec()
	{
		if (enable1 || enable2)
		{
			await base.Exec();
			Transform transform = StoryPlayFlow.GetInstance().imageTop.transform.Find(targetName);
			float x = transform.transform.localRotation.x;
			float z = transform.transform.localRotation.z;
			if (enable1)
			{
				transform.transform.localRotation = Quaternion.Euler(x, rotate1, z);
			}
			if (enable2)
			{
				transform.transform.DORotateQuaternion(Quaternion.Euler(x, rotate2, z), sec).SetEase(Ease.OutQuart).ToUniTask()
					.Forget();
			}
		}
	}
}
