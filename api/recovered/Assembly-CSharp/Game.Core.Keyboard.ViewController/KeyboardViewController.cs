using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game.Core.Keyboard.ViewController;

public class KeyboardViewController : MonoBehaviour
{
	[Serializable]
	public struct KeyboardLayoutButton
	{
		[SerializeField]
		public KeyboardLayoutType layoutType;

		[SerializeField]
		public Button changeButton;

		[SerializeField]
		public KeyboardLayout layout;
	}

	[SerializeField]
	private InputMode inputMode;

	[SerializeField]
	private KeyboardLayoutButton[] keyboardLayout;

	[SerializeField]
	private KeyButton[] keyButton;

	[SerializeField]
	private Button acceptButton;

	[SerializeField]
	private Button deleteButton;

	private KeyboardLayout useKeyboardLayout;

	public UnityAction<KeyOperationType, string> OnKeyPress;

	public InputMode InputMode
	{
		get
		{
			return inputMode;
		}
		set
		{
			inputMode = value;
			ApplyInputMode();
		}
	}

	public bool AllowAccept
	{
		get
		{
			return acceptButton.interactable;
		}
		set
		{
			acceptButton.interactable = value;
		}
	}

	private void Start()
	{
		KeyButton[] array = keyButton;
		foreach (KeyButton obj in array)
		{
			obj.OnKeyPress = (UnityAction<KeyDefinition>)Delegate.Combine(obj.OnKeyPress, new UnityAction<KeyDefinition>(OnKeyboardKeyPress));
			obj.InputMode = inputMode;
		}
		acceptButton.onClick.AddListener(delegate
		{
			OnKeyboardKeyPress(new KeyDefinition
			{
				operationType = KeyOperationType.Return
			});
		});
		deleteButton.onClick.AddListener(delegate
		{
			OnKeyboardKeyPress(new KeyDefinition
			{
				operationType = KeyOperationType.Delete
			});
		});
		KeyboardLayoutButton[] array2 = keyboardLayout;
		for (int i = 0; i < array2.Length; i++)
		{
			KeyboardLayoutButton layout = array2[i];
			layout.changeButton.onClick.AddListener(delegate
			{
				OnChangeKeyboardLayout(layout);
			});
		}
		ApplyInputMode();
	}

	private void ApplyInputMode()
	{
		if (useKeyboardLayout != null && !useKeyboardLayout.HasEnableKey(inputMode))
		{
			useKeyboardLayout = null;
		}
		KeyboardLayoutButton[] array = keyboardLayout;
		for (int i = 0; i < array.Length; i++)
		{
			KeyboardLayoutButton keyboardLayoutButton = array[i];
			if (keyboardLayoutButton.layout.HasEnableKey(inputMode))
			{
				keyboardLayoutButton.changeButton.interactable = true;
				if (useKeyboardLayout == null)
				{
					useKeyboardLayout = keyboardLayoutButton.layout;
					ApplyKeyLayout();
					keyboardLayoutButton.changeButton.GetComponent<Animator>().SetTrigger("Selected");
				}
			}
			else
			{
				keyboardLayoutButton.changeButton.interactable = false;
			}
		}
	}

	private void ApplyKeyLayout()
	{
		for (int i = 0; i < keyButton.Length; i++)
		{
			keyButton[i].InputMode = inputMode;
			KeyButton obj = keyButton[i];
			KeyboardLayout obj2 = useKeyboardLayout;
			obj.KeyDefinition = (((object)obj2 != null) ? obj2.KeyList[i] : new KeyDefinition
			{
				operationType = KeyOperationType.Disable
			});
		}
	}

	private void OnChangeKeyboardLayout(KeyboardLayoutButton layout)
	{
		useKeyboardLayout = layout.layout;
		ApplyKeyLayout();
		SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
		KeyboardLayoutButton[] array = keyboardLayout;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].changeButton.GetComponent<Animator>().SetTrigger("Normal");
		}
		layout.changeButton.GetComponent<Animator>().ResetTrigger("Normal");
		layout.changeButton.GetComponent<Animator>().SetTrigger("Selected");
	}

	private void OnKeyboardKeyPress(KeyDefinition keyDefinition)
	{
		OnKeyPress?.Invoke(keyDefinition.operationType, keyDefinition.paramter);
	}
}
