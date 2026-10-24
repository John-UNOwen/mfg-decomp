using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TenkeyManager : PopupWindow
{
	[SerializeField]
	private Transform m_numberPanelRoot;

	[SerializeField]
	private Button m_cancelButton;

	[SerializeField]
	private Button m_resetButton;

	[SerializeField]
	private GameObject m_panelObj;

	[SerializeField]
	private GameObject m_panelCheckObj;

	[SerializeField]
	private TextMeshProUGUI m_announceText;

	[SerializeField]
	private GameObject m_infoText;

	[SerializeField]
	private TenkeyNumberParts[] m_buttonParts;

	private static int TENKEY_BUTTON_PATTERN_MAX = 14;

	private static int[,] TENKEY_BUTTON_PATTERN_LIST = new int[14, 10]
	{
		{ 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 },
		{ 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 },
		{ 0, 4, 7, 1, 5, 8, 2, 6, 9, 3 },
		{ 1, 5, 8, 2, 6, 9, 3, 7, 0, 4 },
		{ 2, 1, 0, 5, 4, 3, 8, 7, 6, 9 },
		{ 3, 2, 1, 6, 5, 4, 9, 8, 7, 0 },
		{ 6, 3, 0, 7, 4, 1, 8, 5, 2, 9 },
		{ 7, 4, 1, 8, 5, 2, 9, 6, 3, 0 },
		{ 3, 6, 9, 2, 5, 8, 1, 4, 7, 0 },
		{ 4, 7, 0, 3, 6, 9, 2, 5, 8, 1 },
		{ 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 },
		{ 0, 9, 8, 7, 6, 5, 4, 3, 2, 1 },
		{ 7, 8, 9, 4, 5, 6, 1, 2, 3, 0 },
		{ 8, 9, 0, 5, 6, 7, 2, 3, 4, 1 }
	};

	private List<int> m_tenkeyIndexList = new List<int>();

	private int m_tenkeyPattern = -1;

	private string m_password = "";

	private bool m_bComplete;

	private bool bFirstInput = true;

	private bool bFirstEntry;

	private int m_frameIndex;

	private bool m_bInputPassword;

	private List<TenkeyInputPanel> m_tenkeyInputPanelList = new List<TenkeyInputPanel>();

	private int m_panelIndex;

	private UnityAction<int> m_inputAction;

	private UnityAction m_authAction;

	private string m_nowAnnounceText;

	public string SetAnnounceText
	{
		set
		{
			m_announceText.gameObject.SetActive(value: true);
			m_announceText.text = value;
		}
	}

	public bool SetActiveTenkeyObject
	{
		set
		{
			base.gameObject.SetActive(value);
		}
	}

	public bool IsFirstInput => bFirstInput;

	public void CreatePasswordAuthTenkey(UnityAction _okAction, UnityAction _cancelAction)
	{
		m_tenkeyIndexList.Clear();
		m_tenkeyIndexList = GetTenkeyPattern();
		m_tenkeyPattern = -1;
		int num = m_tenkeyPattern + 1;
		for (int i = 0; i < m_buttonParts.Length; i++)
		{
			num = Mathf.Clamp(num, 0, TENKEY_BUTTON_PATTERN_MAX - 1);
			int number = TENKEY_BUTTON_PATTERN_LIST[m_tenkeyIndexList[num], i];
			m_buttonParts[i].Create(SetPasswordNumber, number);
		}
		m_tenkeyPattern = num;
		m_inputAction = SetPasswordNumber;
		m_tenkeyInputPanelList.Clear();
		m_panelObj.SetActive(value: true);
		m_tenkeyInputPanelList.Add(m_panelObj.GetComponent<TenkeyInputPanel>());
		m_tenkeyInputPanelList[0].CreatePanel(_bFirst: false);
		m_panelCheckObj.SetActive(value: false);
		ResetInput();
		m_authAction = _okAction;
		m_infoText.SetActive(value: false);
		m_cancelButton.onClick.RemoveAllListeners();
		m_cancelButton.onClick.AddListener(delegate
		{
			m_resetButton.interactable = false;
			for (int j = 0; j < m_buttonParts.Length; j++)
			{
				m_buttonParts[j].SetButtonInteractable(_bEnable: false);
			}
			SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			_cancelAction();
		});
		m_resetButton.onClick.RemoveAllListeners();
		m_resetButton.onClick.AddListener(delegate
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			m_announceText.text = TextManager.Get(TextManager.KEY.ENTRY_PASSWORD);
			ResetInput();
		});
	}

	public void CreateNewCardDataTenkey(UnityAction _okAction, UnityAction _cancelAction)
	{
		m_tenkeyIndexList.Clear();
		m_tenkeyIndexList = GetTenkeyPattern();
		m_tenkeyPattern = -1;
		int num = m_tenkeyPattern + 1;
		for (int i = 0; i < m_buttonParts.Length; i++)
		{
			num = Mathf.Clamp(num, 0, TENKEY_BUTTON_PATTERN_MAX - 1);
			int number = TENKEY_BUTTON_PATTERN_LIST[m_tenkeyIndexList[num], i];
			m_buttonParts[i].Create(SetNewEntryPasswordNumber, number);
		}
		m_tenkeyPattern = num;
		m_inputAction = SetNewEntryPasswordNumber;
		m_tenkeyInputPanelList.Clear();
		m_panelObj.SetActive(value: true);
		m_tenkeyInputPanelList.Add(m_panelObj.GetComponent<TenkeyInputPanel>());
		m_tenkeyInputPanelList.Add(m_panelCheckObj.GetComponent<TenkeyInputPanel>());
		m_panelCheckObj.SetActive(value: true);
		m_tenkeyInputPanelList[0].CreatePanel(_bFirst: true);
		m_tenkeyInputPanelList[1].CreatePanel(_bFirst: false);
		ResetInput();
		m_tenkeyInputPanelList[1].SetInputPartsNowPosition(0, _enable: false);
		m_authAction = _okAction;
		bFirstEntry = true;
		m_infoText.SetActive(value: true);
		m_cancelButton.onClick.RemoveAllListeners();
		m_cancelButton.onClick.AddListener(delegate
		{
			m_resetButton.interactable = false;
			for (int j = 0; j < m_buttonParts.Length; j++)
			{
				m_buttonParts[j].SetButtonInteractable(_bEnable: false);
			}
			SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			_cancelAction();
		});
		m_resetButton.onClick.RemoveAllListeners();
		m_resetButton.onClick.AddListener(delegate
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			m_announceText.text = TextManager.Get(TextManager.KEY.NEW_NUMBER_INPUT);
			ResetInput();
		});
	}

	private void SetPasswordNumber(int _number)
	{
		SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
		if (m_frameIndex >= m_tenkeyInputPanelList[0].PanelLength || m_bInputPassword)
		{
			return;
		}
		m_tenkeyInputPanelList[0].UpdateDispStatus(m_frameIndex);
		m_password += _number;
		if (m_frameIndex == m_tenkeyInputPanelList[0].PanelLength - 1)
		{
			m_resetButton.interactable = false;
			for (int i = 0; i < m_buttonParts.Length; i++)
			{
				m_buttonParts[i].SetButtonInteractable(_bEnable: false);
			}
			m_bInputPassword = true;
			_ = m_password;
			m_tenkeyInputPanelList[0].SetInputPartsNowPosition(m_frameIndex, _enable: false);
			m_cancelButton.enabled = false;
			m_authAction();
		}
		else
		{
			m_frameIndex++;
		}
	}

	private void SetNewEntryPasswordNumber(int _number)
	{
		SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
		if (m_frameIndex >= m_tenkeyInputPanelList[m_panelIndex].PanelLength || m_bInputPassword)
		{
			return;
		}
		m_tenkeyInputPanelList[m_panelIndex].UpdateDispStatus(m_frameIndex);
		m_password += _number;
		if (m_frameIndex == m_tenkeyInputPanelList[m_panelIndex].PanelLength - 1)
		{
			m_resetButton.interactable = false;
			for (int i = 0; i < m_buttonParts.Length; i++)
			{
				m_buttonParts[i].SetButtonInteractable(_bEnable: false);
			}
			m_bInputPassword = true;
			_ = m_password;
			m_tenkeyInputPanelList[m_panelIndex].SetInputPartsNowPosition(m_frameIndex, _enable: false);
			m_cancelButton.enabled = false;
			m_panelIndex++;
		}
		m_frameIndex++;
		if (m_bInputPassword)
		{
			m_authAction();
		}
	}

	private List<int> GetTenkeyPattern()
	{
		new System.Random().Next(TENKEY_BUTTON_PATTERN_MAX);
		List<int> list = new List<int>();
		for (int j = 0; j < TENKEY_BUTTON_PATTERN_MAX; j++)
		{
			list.Add(j);
		}
		return list.OrderBy((int i) => Guid.NewGuid()).ToList();
	}

	public void ResetInput()
	{
		for (int i = 0; i < m_buttonParts.Length; i++)
		{
			int number = TENKEY_BUTTON_PATTERN_LIST[m_tenkeyIndexList[m_tenkeyPattern], i];
			m_buttonParts[i].ResetSettings();
			m_buttonParts[i].Create(m_inputAction, number);
			m_buttonParts[i].SetButtonInteractable(_bEnable: true);
		}
		for (int j = 0; j < m_tenkeyInputPanelList.Count; j++)
		{
			for (int k = 0; k < m_tenkeyInputPanelList[j].PanelLength; k++)
			{
				m_tenkeyInputPanelList[j].SetInputPartsNowPosition(k, _enable: false);
				m_tenkeyInputPanelList[j].SetInputPartsNumberPosition(k, _enable: false);
			}
		}
		m_resetButton.interactable = true;
		m_bInputPassword = false;
		m_frameIndex = 0;
		m_password = "";
		m_tenkeyInputPanelList[0].SetInputPartsNowPosition(0, _enable: true);
		bFirstInput = true;
		m_cancelButton.enabled = true;
		m_panelIndex = 0;
	}

	public void ResetTenkeyNumber()
	{
		int num = m_tenkeyPattern + 1;
		for (int i = 0; i < m_buttonParts.Length; i++)
		{
			num = Mathf.Clamp(num, 0, TENKEY_BUTTON_PATTERN_MAX - 1);
			int number = TENKEY_BUTTON_PATTERN_LIST[m_tenkeyIndexList[num], i];
			m_buttonParts[i].ResetSettings();
			m_buttonParts[i].Create(m_inputAction, number);
			m_buttonParts[i].SetButtonInteractable(_bEnable: true);
		}
		m_tenkeyPattern = num;
	}

	public void NextSetNumber()
	{
		ResetTenkeyNumber();
		m_password = "";
		m_cancelButton.enabled = true;
		m_bInputPassword = false;
		m_frameIndex = 0;
		m_tenkeyInputPanelList[1].SetInputPartsNowPosition(0, _enable: true);
		m_resetButton.interactable = true;
		bFirstInput = false;
	}

	public bool IsInputPassword()
	{
		return m_bInputPassword;
	}

	public string GetInputPassword()
	{
		return m_password;
	}

	public bool IsCompleteCheck()
	{
		return m_bComplete;
	}

	public void Exit()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
