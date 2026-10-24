using System;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class BaseMenuWindow : MonoBehaviour, IExclusiveWindow
{
	[SerializeField]
	protected Button _closeButton;

	[SerializeField]
	protected Button _closeArea;

	protected bool _isOpen;

	protected Subject<Unit> _onClose = new Subject<Unit>();

	public IObservable<Unit> OnClose => _onClose.AsObservable();

	public virtual bool IsOpen => _isOpen;

	protected virtual void Start()
	{
		if (_closeButton != null)
		{
			_closeButton.OnClickAsObservable().Subscribe(delegate
			{
				Close();
			}).AddTo(this);
		}
		if (_closeArea != null)
		{
			_closeArea.OnClickAsObservable().Subscribe(delegate
			{
				Close();
			}).AddTo(this);
		}
	}

	public virtual void Open(bool isSeEnable = true)
	{
		if (!_isOpen)
		{
			if (isSeEnable)
			{
				SoundManager.PlaySe("SFX_MJG_INFO1").Forget();
			}
			base.gameObject.SetActive(value: true);
			_isOpen = true;
		}
	}

	public virtual void Close(bool isSeEnable = true)
	{
		if (_isOpen)
		{
			if (isSeEnable)
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			}
			base.gameObject.SetActive(value: false);
			_isOpen = false;
			_onClose.OnNext(Unit.Default);
		}
	}

	public virtual void ExclusiveOpen()
	{
		Open();
	}

	public virtual void OnExclusiveClose()
	{
		Close(isSeEnable: false);
	}
}
