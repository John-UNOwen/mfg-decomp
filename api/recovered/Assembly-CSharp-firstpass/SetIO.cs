using System.Text;
using UnityEngine;

public class SetIO : MonoBehaviour
{
	public static void SetButton(NewLabel button, bool state)
	{
		if (button != null)
		{
			if (state)
			{
				button.Opacity = 1.0;
			}
			else
			{
				button.Opacity = 0.2;
			}
		}
	}

	public static void SetMedalButton(NewLabel button, bool state)
	{
		if (state)
		{
			button.Opacity = 1.0;
		}
		else
		{
			button.Opacity = 0.2;
		}
	}

	public static void SetMedal(byte[] data, NewLabel dataview, NewLabel ctrl, string label)
	{
		if (data != null)
		{
			dataview.Content = label + BytesToString(data, 0, data.Length);
			ctrl.Content = BytesToString(data, 2, 8);
			_ = data[0];
			_ = 4;
		}
	}

	public static string BytesToString(byte[] data, int start, int len)
	{
		if (data != null)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < len; i++)
			{
				stringBuilder.Append(data[i + start].ToString("X2"));
			}
			for (int j = 0; j < data.Length; j++)
			{
				_ = data[j];
			}
			return stringBuilder.ToString();
		}
		return "";
	}
}
