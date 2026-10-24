using System;
using UnityEngine;

namespace TestModeSkeleton;

public class TestModeTenkey : MonoBehaviour
{
	[Flags]
	public enum Click
	{
		None = 0,
		Clear = 1,
		N0 = 2,
		N1 = 4,
		N2 = 8,
		N3 = 0x10,
		N4 = 0x20,
		N5 = 0x40,
		N6 = 0x80,
		N7 = 0x100,
		N8 = 0x200,
		N9 = 0x400,
		N00 = 0x800
	}

	private Click clickFlags;

	public Click ClickKey()
	{
		return clickFlags;
	}

	public bool IsClick(Click click)
	{
		return (clickFlags & click) != 0;
	}

	public void Clear()
	{
		clickFlags = Click.None;
	}

	public void OnClick(string value)
	{
		if (Enum.TryParse<Click>(value, out var result))
		{
			clickFlags |= result;
		}
	}
}
