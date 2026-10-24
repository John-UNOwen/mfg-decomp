using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game.Core.Keyboard.ViewController;

[RequireComponent(typeof(Button))]
public class KeyButton : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI label;

	private KeyDefinition keyDefinition = new KeyDefinition
	{
		operationType = KeyOperationType.Disable
	};

	private InputMode inputMode;

	public UnityAction<KeyDefinition> OnKeyPress;

	public KeyDefinition KeyDefinition
	{
		get
		{
			return keyDefinition;
		}
		set
		{
			keyDefinition = value;
			Apply();
		}
	}

	public InputMode InputMode
	{
		get
		{
			return inputMode;
		}
		set
		{
			inputMode = value;
			Apply();
		}
	}

	private void Awake()
	{
		Apply();
	}

	private void Apply()
	{
		Button component = GetComponent<Button>();
		if (keyDefinition.operationType != KeyOperationType.Disable)
		{
			component.interactable = (InputMode & KeyDefinition.inputMode) == KeyDefinition.inputMode;
			label.text = keyDefinition.displayText;
		}
		else
		{
			component.interactable = false;
			label.text = "";
		}
	}

	public void OnClick()
	{
		if (KeyDefinition.operationType != KeyOperationType.Disable)
		{
			OnKeyPress?.Invoke(KeyDefinition);
		}
	}
}
