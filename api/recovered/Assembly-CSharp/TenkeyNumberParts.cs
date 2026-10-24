using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TenkeyNumberParts : MonoBehaviour
{
	[SerializeField]
	private Button m_button;

	[SerializeField]
	private TextMeshProUGUI m_text;

	private Dictionary<string, (KeyCode keytop, KeyCode keypad)> _keyMaps = new Dictionary<string, (KeyCode, KeyCode)>
	{
		{
			"0",
			(KeyCode.Alpha0, KeyCode.Keypad0)
		},
		{
			"1",
			(KeyCode.Alpha1, KeyCode.Keypad1)
		},
		{
			"2",
			(KeyCode.Alpha2, KeyCode.Keypad2)
		},
		{
			"3",
			(KeyCode.Alpha3, KeyCode.Keypad3)
		},
		{
			"4",
			(KeyCode.Alpha4, KeyCode.Keypad4)
		},
		{
			"5",
			(KeyCode.Alpha5, KeyCode.Keypad5)
		},
		{
			"6",
			(KeyCode.Alpha6, KeyCode.Keypad6)
		},
		{
			"7",
			(KeyCode.Alpha7, KeyCode.Keypad7)
		},
		{
			"8",
			(KeyCode.Alpha8, KeyCode.Keypad8)
		},
		{
			"9",
			(KeyCode.Alpha9, KeyCode.Keypad9)
		}
	};

	public string Number
	{
		get
		{
			return m_text.ToString();
		}
		set
		{
			m_text.text = value;
		}
	}

	public void Create(UnityAction<int> _action, int _number)
	{
		Number = _number.ToString();
		m_button.onClick.AddListener(delegate
		{
			_action(_number);
		});
	}

	public void ResetSettings()
	{
		Number = "";
		m_button.onClick.RemoveAllListeners();
	}

	public void SetButtonInteractable(bool _bEnable)
	{
		m_button.interactable = _bEnable;
	}

	private void Update()
	{
		if (GameUtility.IsKonastaOrNetcafeBehavior && _keyMaps.TryGetValue(m_text.text, out (KeyCode, KeyCode) value) && (Bi2xInput.GetKeyDown(value.Item1) || Bi2xInput.GetKeyDown(value.Item2)))
		{
			m_button.onClick.Invoke();
		}
	}
}
