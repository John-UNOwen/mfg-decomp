using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MFG.StoryPlay;

public class ImageColor : Command
{
	public Color color;

	public override async UniTask Parse(string[] values)
	{
		await base.Parse(values);
		type = CommandType.ImageColor;
		sec = Utility.ParseSec(values[6], 0.1f);
		targetName = values[3];
		switch (values[5])
		{
		case "":
			color = Color.white;
			return;
		case "red":
			color = Color.red;
			return;
		case "blue":
			color = Color.blue;
			return;
		case "yellow":
			color = Color.yellow;
			return;
		case "green":
			color = Color.green;
			return;
		case "black":
			color = Color.black;
			return;
		case "white":
			color = Color.white;
			return;
		case "on":
			color = new Color(1f, 1f, 1f);
			return;
		case "off":
			color = new Color(0.5f, 0.5f, 0.5f);
			return;
		}
		try
		{
			string[] array = values[5].Split(" ");
			color.r = float.Parse(array[0]);
			color.g = float.Parse(array[1]);
			color.b = float.Parse(array[2]);
			color.a = float.Parse(array[3]);
		}
		catch
		{
			Debug.Log("想定外の色入力");
			color = Color.white;
		}
	}

	public override async UniTask Exec()
	{
		await base.Exec();
		if (targetName == "ALL")
		{
			foreach (Transform item in StoryPlayFlow.GetInstance().imageTop.transform)
			{
				Utility.SetImageColor(item.gameObject, color, sec);
			}
			return;
		}
		Utility.SetImageColor(StoryPlayFlow.GetInstance().imageTop.transform.Find(targetName).gameObject, color, sec);
	}
}
