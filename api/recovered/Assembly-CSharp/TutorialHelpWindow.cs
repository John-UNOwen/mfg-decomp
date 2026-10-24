using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class TutorialHelpWindow : MonoBehaviour, IExclusiveWindow
{
	[SerializeField]
	private Button _closeButton;

	[SerializeField]
	private Button _closeArea;

	private bool _isOpen;

	private void Start()
	{
		_closeButton.OnClickAsObservable().Subscribe(delegate
		{
			Close();
		}).AddTo(this);
		if (_closeArea != null)
		{
			_closeArea.OnClickAsObservable().Subscribe(delegate
			{
				Close();
			}).AddTo(this);
		}
	}

	public void Open(bool isSeEnable = true)
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

	public void Close(bool isSeEnable = true)
	{
		if (_isOpen)
		{
			if (isSeEnable)
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			}
			base.gameObject.SetActive(value: false);
			_isOpen = false;
		}
	}

	public void ExclusiveOpen()
	{
		Open();
	}

	public void OnExclusiveClose()
	{
		Close(isSeEnable: false);
	}
}
