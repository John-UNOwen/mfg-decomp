using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ScrambleLotteryExchangeWindow : MonoBehaviour
{
	[SerializeField]
	private GameObject m_window;

	[SerializeField]
	private Button m_closeButton;

	[SerializeField]
	private Button m_decideButton;

	[SerializeField]
	private TextMeshProUGUI m_medalText;

	[SerializeField]
	private TextMeshProUGUI m_spiritChoosingText;

	[SerializeField]
	private Button m_plusButton;

	[SerializeField]
	private Button m_plus10Button;

	[SerializeField]
	private Button m_minusButton;

	[SerializeField]
	private GameObject m_exchangeConfirmWindow;

	[SerializeField]
	private TextMeshProUGUI m_exchangeConfirmText;

	[SerializeField]
	private Button m_closeConfirmButton;

	public bool _isOpen;

	public bool _isConfirmOpen;

	private int _exchangeCounter;

	public virtual bool IsOpen => _isOpen;

	public virtual bool IsConfirmOpen => _isConfirmOpen;

	public virtual void Start()
	{
		if (m_closeButton != null)
		{
			m_closeButton.OnClickAsObservable().Subscribe(delegate
			{
				Close();
			}).AddTo(this);
		}
		if (m_closeConfirmButton != null)
		{
			m_closeConfirmButton.OnClickAsObservable().Subscribe(delegate
			{
				CloseConfirmWindow();
			}).AddTo(this);
		}
		if (m_decideButton != null)
		{
			m_decideButton.OnClickAsObservable().Subscribe(async delegate
			{
				await Exchange();
			}).AddTo(this);
		}
		int needConsumeItemCount = GameDataManager.ScrambleFestival.CurrentInfo.Master.SpiritExchangeMedal;
		m_plusButton.GetComponent<Button>().OnClickAsObservable().Subscribe(delegate
		{
			if (needConsumeItemCount * (_exchangeCounter + 1) <= GameDataManager.ScrambleFestival.CurrentInfo.Medal)
			{
				_exchangeCounter++;
				SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			}
			else
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			}
		})
			.AddTo(this);
		m_minusButton.GetComponent<Button>().OnClickAsObservable().Subscribe(delegate
		{
			if (_exchangeCounter > 0)
			{
				_exchangeCounter--;
				SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			}
			else
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			}
		})
			.AddTo(this);
	}

	public virtual void Reflesh()
	{
		m_medalText.text = GameDataManager.ScrambleFestival.CurrentInfo.Medal.ToString();
		m_spiritChoosingText.text = "0";
		_exchangeCounter = 0;
	}

	public virtual void Update()
	{
		if (!_isConfirmOpen)
		{
			m_spiritChoosingText.text = $"{_exchangeCounter}";
			int num = GameDataManager.ScrambleFestival.CurrentInfo.Medal - _exchangeCounter * GameDataManager.ScrambleFestival.CurrentInfo.Master.SpiritExchangeMedal;
			m_medalText.text = $"{num}";
		}
		m_decideButton.interactable = !_isConfirmOpen && _exchangeCounter > 0;
	}

	public async UniTask Open(bool isPlaySe = true)
	{
		if (!_isOpen)
		{
			_isOpen = true;
			Reflesh();
			if (isPlaySe)
			{
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			}
			if (m_window != null)
			{
				m_window.SetActive(value: true);
				m_window.GetComponent<Animator>().SetTrigger("In");
			}
			await UniTask.WaitWhile(() => _isOpen);
		}
	}

	public void Close(bool isPlaySe = true)
	{
		if (_isOpen)
		{
			if (isPlaySe)
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			}
			if (m_window != null)
			{
				m_window.GetComponent<Animator>().SetTrigger("Out");
				m_window.SetActive(value: false);
			}
			_isOpen = false;
		}
	}

	public async UniTask Exchange()
	{
		if (!_isConfirmOpen && GameDataManager.ScrambleFestival.CurrentInfo.ExchangeFightSpirit(_exchangeCounter))
		{
			ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
			_isConfirmOpen = true;
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			m_exchangeConfirmText.text = $"アニバーサリーメダル{_exchangeCounter * GameDataManager.ScrambleFestival.CurrentInfo.Master.SpiritExchangeMedal}枚を\nファイトスピリット(だれでも){_exchangeCounter}個に交換しました。";
			if (m_exchangeConfirmWindow != null)
			{
				m_exchangeConfirmWindow.SetActive(value: true);
				m_exchangeConfirmWindow.GetComponent<Animator>().SetTrigger("In");
				await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(10.0)), UniTask.WaitUntil(() => !_isConfirmOpen));
			}
		}
		if (_isConfirmOpen)
		{
			CloseConfirmWindow();
		}
		Close(isPlaySe: false);
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
	}

	public void CloseConfirmWindow(bool isPlaySe = true)
	{
		if (_isConfirmOpen)
		{
			if (isPlaySe)
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			}
			if (m_exchangeConfirmWindow != null)
			{
				m_exchangeConfirmWindow.GetComponent<Animator>().SetTrigger("Out");
				m_exchangeConfirmWindow.SetActive(value: false);
			}
			_isConfirmOpen = false;
		}
	}
}
