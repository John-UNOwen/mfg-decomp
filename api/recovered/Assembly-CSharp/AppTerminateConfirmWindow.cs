using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class AppTerminateConfirmWindow : BaseMenuWindow
{
	[SerializeField]
	protected Button _yesButton;

	[SerializeField]
	protected Button _noButton;

	protected Subject<bool> _onDecided = new Subject<bool>();

	public IObservable<bool> OnDecided => _onDecided.AsObservable();

	protected override void Start()
	{
		base.Start();
		_yesButton.OnClickAsObservable().Subscribe(delegate
		{
			bool flag = !ResidentFlow.GetInstance().IsSceneLoading;
			Debug.Log($"AppTerminateConfirmWindow: yes{flag}");
			_onDecided.OnNext(value: true);
			Close(isSeEnable: false);
		}).AddTo(this);
		_noButton.OnClickAsObservable().Subscribe(delegate
		{
			Debug.Log("AppTerminateConfirmWindow: no");
			_onDecided.OnNext(value: false);
			Close(isSeEnable: false);
		}).AddTo(this);
	}

	protected void Update()
	{
		if (IsOpen && GameUtility.IsCloudStreaming && (Cloudstrap.IsDisconnect || ResidentFlow.GetInstance().IsSceneLoading))
		{
			Debug.Log("AppTerminateConfirmWindow: force no");
			_onDecided.OnNext(value: false);
			Close(isSeEnable: false);
		}
	}
}
