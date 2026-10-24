using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MFG.StoryPlay;

public class CharaColor : CharaCommand
{
	public Color color;

	public override async UniTask Parse(string[] values)
	{
		await base.Parse(values);
		type = CommandType.CharaColor;
		charaName = values[2];
		sec = Utility.ParseSec(values[6], 0.1f);
		switch (values[3])
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
			string[] array = values[3].Split(" ");
			color.r = float.Parse(array[0]);
			color.g = float.Parse(array[1]);
			color.b = float.Parse(array[2]);
			color.a = float.Parse(array[3]);
		}
		catch (Exception ex)
		{
			Debug.Log("想定外の入力：" + ex);
			color = Color.white;
		}
	}

	public override async UniTask Exec()
	{
		await base.Exec();
		if (charaName == "ALL")
		{
			foreach (Transform item in StoryPlayFlow.GetInstance().chara2DSet.transform)
			{
				Utility.SetCharaColor(item.gameObject, color, sec);
			}
			return;
		}
		Utility.SetCharaColor(obj2D, color, sec);
	}
}
