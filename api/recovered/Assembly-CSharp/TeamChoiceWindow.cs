using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class TeamChoiceWindow : MonoBehaviour
{
	public TextMeshProUGUI _themeChoiceText;

	[SerializeField]
	protected Button _closeButton;

	[SerializeField]
	protected Animator _window;

	[SerializeField]
	protected Transform _choiceButtonParent;

	public InfoWindow m_infoWindow;

	protected bool _isOpen;

	public virtual bool IsOpen => _isOpen;

	protected virtual void Start()
	{
		if (_closeButton != null)
		{
			_closeButton.OnClickAsObservable().Subscribe(async delegate
			{
				await Close();
			}).AddTo(this);
		}
	}

	public virtual void Init(CatchphrasePlate homeCatchphrasePlate)
	{
		foreach (Transform item in _choiceButtonParent)
		{
			item.GetComponent<TeamChoiceButton>().Reflesh(this, homeCatchphrasePlate);
		}
	}

	public virtual void Open()
	{
		if (!_isOpen)
		{
			_isOpen = true;
			base.gameObject.SetActive(value: true);
			SoundManager.PlaySe("SFX_MJG_INFO1").Forget();
			if (_window != null)
			{
				_window.SetTrigger("In");
			}
		}
	}

	public virtual async UniTask Close()
	{
		if (_isOpen)
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			if (_window != null)
			{
				_window.SetTrigger("Out");
			}
			_isOpen = false;
			await UniTask.Delay(TimeSpan.FromSeconds(1.0));
		}
	}
}
