using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class PrivateMatchTenkeyWindow : MonoBehaviour
{
	[SerializeField]
	private GameObject m_windowObj;

	[SerializeField]
	private Button m_closeButton;

	[SerializeField]
	private Button m_decideButton;

	[SerializeField]
	private TextMeshProUGUI dateText;

	[SerializeField]
	private Button m_resetButton;

	[SerializeField]
	private List<Button> m_buttonList;

	[SerializeField]
	private List<GameObject> m_inputNumbers;

	[SerializeField]
	private List<GameObject> m_inputNoneNumbers;

	private string inputPassCode = "";

	private bool _isOpen;

	private bool _isTimerAdded;

	public bool IsOpen => _isOpen;

	private void Start()
	{
		m_closeButton.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			GameDataManager.PlayerConfig.PrivateMatchPassCode = "";
			GameDataManager.PlayerConfig.IsPrivateMatch = false;
			CloseWindow().Forget();
		}).AddTo(this);
		m_decideButton.OnClickAsObservable().Subscribe(delegate
		{
			try
			{
				int num = int.Parse(inputPassCode);
				if (inputPassCode.Length == 4 && 0 <= num && num <= 9999)
				{
					GameDataManager.PlayerConfig.PrivateMatchPassCode = inputPassCode;
					GameDataManager.PlayerConfig.IsPrivateMatch = true;
					if (!_isTimerAdded)
					{
						ResidentFlow.GetInstance().GetTimer().AddTimer(20f);
						_isTimerAdded = true;
					}
					SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
					CloseWindow().Forget();
				}
			}
			catch
			{
			}
		}).AddTo(this);
		for (int i = 0; i < 10; i++)
		{
			m_buttonList[i].transform.GetComponent<TenkeyNumberParts>().Create(NumberInputFlow, i);
		}
		m_resetButton.OnClickAsObservable().Subscribe(delegate
		{
			inputPassCode = "";
		}).AddTo(this);
	}

	private void NumberInputFlow(int _number)
	{
		SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
		if (inputPassCode.Length < 4)
		{
			inputPassCode += $"{_number}";
		}
	}

	private void Update()
	{
		m_decideButton.interactable = inputPassCode.Length == 4;
		RefleshInputField();
	}

	private void RefleshInputField()
	{
		for (int i = 0; i < 4; i++)
		{
			if (i < inputPassCode.Length)
			{
				m_inputNumbers[i].transform.GetComponent<TextMeshProUGUI>().text = $"{inputPassCode[i]}";
				m_inputNoneNumbers[i].SetActive(value: false);
				m_inputNumbers[i].SetActive(value: true);
			}
			else
			{
				m_inputNoneNumbers[i].SetActive(value: true);
				m_inputNumbers[i].SetActive(value: false);
			}
		}
	}

	public void OpenWindow()
	{
		if (!_isOpen)
		{
			inputPassCode = GameDataManager.PlayerConfig.PrivateMatchPassCode;
			RefleshInputField();
			_isOpen = true;
			base.gameObject.SetActive(value: true);
			SoundManager.PlaySe("SFX_MJG_INFO1").Forget();
			m_windowObj.GetComponent<Animator>().SetTrigger("In");
		}
	}

	public async UniTask CloseWindow()
	{
		if (_isOpen)
		{
			m_windowObj.GetComponent<Animator>().SetTrigger("Out");
			base.gameObject.SetActive(value: false);
			_isOpen = false;
			await UniTask.Delay(TimeSpan.FromSeconds(1.0));
		}
	}
}
