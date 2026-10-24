using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TestModeSkeleton;

[Serializable]
public class TestModeMenuElement
{
	public enum EventMode
	{
		NextScene,
		Event,
		FactorySettings,
		SaveAndExit,
		Exit
	}

	public enum TextState
	{
		Default,
		Selecting,
		UnSelctable,
		NotActive
	}

	[SerializeField]
	[Tooltip("UIテキスト")]
	[FormerlySerializedAs("text")]
	private Text text;

	[SerializeField]
	[Tooltip("イベントの扱い方")]
	[FormerlySerializedAs("mode")]
	private EventMode mode;

	[SerializeField]
	[Tooltip("イベント")]
	[FormerlySerializedAs("event")]
	private string @event = "";

	[SerializeField]
	[Tooltip("上記パラメータで判別できない場合に使用")]
	[FormerlySerializedAs("extra")]
	private string extra = "";

	private TextState state;

	public Text Text => text;

	public EventMode Mode => mode;

	public string Event => @event;

	public string Extra => extra;

	public TextState State
	{
		get
		{
			return state;
		}
		set
		{
			state = value;
			if (!(text == null))
			{
				switch (state)
				{
				case TextState.Default:
					text.color = Color.white;
					text.gameObject.SetActive(value: true);
					break;
				case TextState.Selecting:
					text.color = Color.red;
					text.gameObject.SetActive(value: true);
					break;
				case TextState.UnSelctable:
					text.color = Color.gray;
					text.gameObject.SetActive(value: true);
					break;
				case TextState.NotActive:
					text.color = Color.white;
					text.gameObject.SetActive(value: false);
					break;
				}
			}
		}
	}

	public bool IsSelectable
	{
		get
		{
			if (state != 0)
			{
				return state == TextState.Selecting;
			}
			return true;
		}
	}

	public string TextString => text?.text ?? "";

	public void StartTextBlink()
	{
		(text?.GetComponent<TestModeTextBlink>())?.StartBlink();
	}

	public void StopTextBlink()
	{
		(text?.GetComponent<TestModeTextBlink>())?.StopBlink();
	}
}
