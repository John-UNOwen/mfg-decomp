using System.Collections;
using Cysharp.Threading.Tasks;
using Game.Core.Keyboard.ViewController;
using UnityEngine;

namespace Game.Core.Keyboard;

public class KeyboardInputModel : MonoBehaviour
{
	[SerializeField]
	private KeyboardViewController keyboardView;

	[SerializeField]
	private Animator anime;

	[SerializeField]
	private CanvasGroup canvasGroup;

	private KeyboardInputField inputField;

	private void Start()
	{
		keyboardView.OnKeyPress = OnKeyPress;
	}

	public void ActivateKeyboard(string animatorTriggerName = "in")
	{
		anime?.SetTrigger(animatorTriggerName);
		canvasGroup.blocksRaycasts = true;
	}

	public void DeactivateKeyboard(string animatorTriggerName = "out")
	{
		anime?.SetTrigger(animatorTriggerName);
		StartCoroutine(DelayOpenBlockRaycasts());
	}

	private IEnumerator DelayOpenBlockRaycasts()
	{
		yield return new WaitForSeconds(0.1f);
		canvasGroup.blocksRaycasts = false;
	}

	public void Bind(KeyboardInputField inputField)
	{
		if (inputField != this.inputField)
		{
			if (this.inputField != null)
			{
				this.inputField.OnLostFocus?.Invoke();
			}
			this.inputField = inputField;
			if (this.inputField != null)
			{
				keyboardView.InputMode = this.inputField.InputMode;
			}
			if (inputField != null)
			{
				keyboardView.AllowAccept = inputField.allowAcceptButton;
			}
			if (this.inputField != null)
			{
				this.inputField.OnGetFocus?.Invoke();
			}
		}
	}

	private void OnKeyPress(KeyOperationType type, string parameter)
	{
		if (!(inputField != null))
		{
			return;
		}
		switch (type)
		{
		case KeyOperationType.InputCharactor:
		case KeyOperationType.InputSmallCharactor:
			inputField.AppendText(parameter);
			SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			break;
		case KeyOperationType.Delete:
			inputField.DeleteText();
			SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			break;
		case KeyOperationType.ToggleDakuon:
		{
			string cursorText2 = inputField.GetCursorText();
			if (DakuonConverter.IsDakuon(cursorText2))
			{
				inputField.ConvertText(DakuonConverter.RemoveDakuon(cursorText2));
			}
			else if (DakuonConverter.IsHalfDakuon(cursorText2))
			{
				string text2 = DakuonConverter.RemoveHalfDakuon(cursorText2);
				inputField.ConvertText(DakuonConverter.AddDakuon(text2));
			}
			else
			{
				inputField.ConvertText(DakuonConverter.AddDakuon(cursorText2));
			}
			SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			break;
		}
		case KeyOperationType.ToggleHalfDakuon:
		{
			string cursorText = inputField.GetCursorText();
			if (DakuonConverter.IsHalfDakuon(cursorText))
			{
				inputField.ConvertText(DakuonConverter.RemoveHalfDakuon(cursorText));
			}
			else if (DakuonConverter.IsDakuon(cursorText))
			{
				string text = DakuonConverter.RemoveDakuon(cursorText);
				inputField.ConvertText(DakuonConverter.AddHalfDakuon(text));
			}
			else
			{
				inputField.ConvertText(DakuonConverter.AddHalfDakuon(cursorText));
			}
			SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			break;
		}
		case KeyOperationType.Return:
			inputField.FinishInput();
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			break;
		}
	}
}
