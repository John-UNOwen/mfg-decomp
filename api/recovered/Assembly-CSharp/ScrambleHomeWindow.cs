using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ScrambleHomeWindow : MonoBehaviour
{
	[SerializeField]
	protected Animator _windowAnimator;

	[SerializeField]
	protected Button _closeButton;

	[SerializeField]
	protected TextMeshProUGUI _dateText;

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

	public virtual void Open()
	{
		if (!_isOpen)
		{
			_isOpen = true;
			Reflesh(isInit: true);
			base.gameObject.SetActive(value: true);
			SoundManager.PlaySe("SFX_MJG_INFO1").Forget();
			if (_windowAnimator != null)
			{
				_windowAnimator.SetTrigger("In");
			}
		}
	}

	public virtual async UniTask Close()
	{
		if (_isOpen)
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			if (_windowAnimator != null)
			{
				_windowAnimator.SetTrigger("Out");
			}
			_isOpen = false;
			await UniTask.Delay(TimeSpan.FromSeconds(1.0));
		}
	}

	protected virtual void Reflesh(bool isInit = false)
	{
		if (_dateText != null)
		{
			_dateText.text = GameDataManager.ScrambleFestival.CurrentInfo.Master.DispDate();
		}
	}
}
