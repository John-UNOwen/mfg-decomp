using System;
using System.Linq;
using TestModeSkeleton.CabinetDef;
using TestModeSkeleton.TestModeExtensions;
using UnityEngine;
using UnityEngine.UI;

namespace TestModeSkeleton;

public class TestModeValue
{
	public enum TextColorType
	{
		Default,
		OnlyYellow,
		OnlyWhite,
		OnlyGray
	}

	private int current;

	public static readonly Converter<int, string> ToOffOnString = (int n) => new string[2] { "OFF", "ON" }.ElementAtOrDefault(n) ?? n.ToString();

	public static readonly Converter<int, string> ToAbleString = (int n) => new string[2] { "無効", "有効" }.ElementAtOrDefault(n) ?? n.ToString();

	private Text uiText;

	private Slider slider;

	private Image sliderFillArea;

	private int start;

	private int @default;

	private int delta;

	private bool isLoop;

	private TextColorType textColorType;

	private Converter<int, string> converter;

	public int Current
	{
		get
		{
			return current;
		}
		private set
		{
			current = value;
			UpdateUI();
		}
	}

	public string CurrentString => converter?.Invoke(Current) ?? Current.ToString();

	public bool IsDefault => Current == @default;

	public bool IsModification => Current != start;

	public int Min { get; }

	public int Max { get; }

	public int Delta
	{
		get
		{
			return delta;
		}
		set
		{
			delta = Math.Max(value, 0);
		}
	}

	public TestModeValue(int min, int max, int @default, int delta = 1, bool isLoop = true, TextColorType textColorType = TextColorType.Default, Converter<int, string> converter = null)
	{
		Min = min;
		Max = max;
		this.@default = @default;
		Delta = delta;
		this.isLoop = isLoop;
		this.textColorType = textColorType;
		this.converter = converter;
	}

	public TestModeValue(ParamSetting param, bool isLoop = true, TextColorType textColorType = TextColorType.Default, Converter<int, string> converter = null)
		: this(param.Min, param.Max, param.Default, param.Delta, isLoop, textColorType, converter)
	{
	}

	public void Set(int start, Text uiText = null, Slider slider = null, Image sliderFillArea = null)
	{
		if (uiText != null)
		{
			this.uiText = uiText;
		}
		if (slider != null)
		{
			this.slider = slider;
		}
		if (sliderFillArea != null)
		{
			this.sliderFillArea = sliderFillArea;
		}
		Current = (this.start = Mathf.Clamp(start, Min, Max));
	}

	public void Change(bool isAdd)
	{
		int num = Current + Delta * (isAdd ? 1 : (-1));
		if (isLoop)
		{
			if (num < Min)
			{
				num = Max;
			}
			if (num > Max)
			{
				num = Min;
			}
		}
		else
		{
			num = Mathf.Clamp(num, Min, Max);
		}
		Current = num;
	}

	public void ForcedChange(int value)
	{
		if (isLoop)
		{
			if (value < Min)
			{
				value = Max;
			}
			if (value > Max)
			{
				value = Min;
			}
		}
		else
		{
			value = Mathf.Clamp(value, Min, Max);
		}
		Current = value;
	}

	public void ToDefault()
	{
		Current = @default;
	}

	public void ToStart()
	{
		Current = start;
	}

	public void ToUnSelectable(bool isTime = false)
	{
		if ((bool)uiText)
		{
			uiText.color = Color.gray;
		}
		if (isTime)
		{
			uiText.text = "--:--";
		}
	}

	public void FromUnSelectable()
	{
		UpdateUI();
	}

	private void UpdateUI()
	{
		if ((bool)uiText)
		{
			uiText.SetLocalizedText(CurrentString);
			switch (textColorType)
			{
			case TextColorType.Default:
				uiText.color = (IsDefault ? Color.green : Color.red);
				break;
			case TextColorType.OnlyYellow:
				uiText.color = Color.yellow;
				break;
			case TextColorType.OnlyWhite:
				uiText.color = Color.white;
				break;
			case TextColorType.OnlyGray:
				uiText.color = Color.gray;
				break;
			}
		}
		if ((bool)slider)
		{
			slider.value = 1f * (float)Current / (float)(Max - Min);
		}
		if ((bool)sliderFillArea)
		{
			sliderFillArea.color = (IsDefault ? Color.green : Color.red);
		}
	}
}
