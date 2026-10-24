using UnityEngine;

public class CustomColor
{
	public static readonly Color Normal = new Color(0.101960786f, 0.101960786f, 0.101960786f);

	public static readonly Color Emphasis = new Color(0f, 0.3529412f, 1f);

	public static readonly Color Caution = new Color(1f, 44f / 85f, 0f);

	public static readonly Color Warrning = new Color(1f, 0f, 0f);

	public static Color GetColor(int value)
	{
		if (value > 0)
		{
			return Emphasis;
		}
		if (value < 0)
		{
			return Warrning;
		}
		return Normal;
	}
}
