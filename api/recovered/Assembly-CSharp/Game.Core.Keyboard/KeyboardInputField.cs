using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Game.Core.Keyboard;

[RequireComponent(typeof(TextMeshProUGUI))]
public class KeyboardInputField : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private InputMode inputMode;

	[SerializeField]
	private int maxCharactorLength = -1;

	[SerializeField]
	private KeyboardInputModel inputModel;

	private TextMeshProUGUI textField;

	public UnityEvent OnFinish;

	public UnityEvent OnValueChange;

	public UnityEvent OnGetFocus;

	public UnityEvent OnLostFocus;

	public string Text = "";

	private string defaultWord = "\uff3f";

	[SerializeField]
	private bool autoSelect;

	[SerializeField]
	public bool allowAcceptButton;

	private int cursorPos;

	public InputMode InputMode => inputMode;

	private TextMeshProUGUI TextField => textField ?? (textField = GetComponent<TextMeshProUGUI>());

	public void SetDispText()
	{
		if (maxCharactorLength <= 0)
		{
			TextField.text = Text;
			return;
		}
		TextField.text = Text + "<voffset=-0.15em><color=#A0A0A0>";
		for (int i = 0; i < maxCharactorLength - Text.Length; i++)
		{
			TextField.text += defaultWord;
		}
		TextField.text += "</color></voffset>";
	}

	public void SetText(string text)
	{
		Text = text;
		OnValueChange?.Invoke();
		cursorPos = Mathf.Min(cursorPos, Text.Length);
		SetDispText();
	}

	private void Start()
	{
		if (autoSelect)
		{
			BindKeyboard();
		}
		SetDispText();
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
	{
		BindKeyboard();
	}

	private void BindKeyboard()
	{
		inputModel.Bind(this);
		cursorPos = Text.Length;
	}

	public void AppendText(string text)
	{
		string text2 = Text;
		if (maxCharactorLength == -1 || text2.Length + text.Length <= maxCharactorLength)
		{
			SetText(text2.Substring(0, cursorPos) + text + text2.Substring(cursorPos));
			cursorPos += text.Length;
		}
	}

	public void DeleteText()
	{
		if (cursorPos >= 1)
		{
			string text = Text;
			SetText(text.Substring(0, cursorPos - 1) + text.Substring(cursorPos));
			cursorPos = Mathf.Min(Text.Length, cursorPos);
		}
	}

	public void ConvertText(string text)
	{
		if (cursorPos >= 1)
		{
			string text2 = Text;
			SetText(text2.Substring(0, cursorPos - 1) + text + text2.Substring(cursorPos));
		}
	}

	public string GetCursorText()
	{
		if (cursorPos < 1)
		{
			return "";
		}
		return Text.Substring(cursorPos - 1, 1);
	}

	public void FinishInput()
	{
		OnFinish?.Invoke();
	}
}
