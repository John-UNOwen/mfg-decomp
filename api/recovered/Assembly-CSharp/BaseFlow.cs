using System;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

public abstract class BaseFlow : MonoBehaviour
{
	protected bool _isInitialized;

	protected bool _isNextSceneDecided;

	protected bool _enableResidentFlowInitializeWait = true;

	protected bool _enableDebugLogin = true;

	protected bool _enableLoadingTipsDisp;

	protected Subject<Unit> _onUpdateSubject = new Subject<Unit>();

	public IObservable<Unit> OnUpdateAsObservable => _onUpdateSubject.AsObservable();

	public IObservable<Unit> OnTimerEnd => GetCountDownTimer().OnEnd.Where((Unit _) => IsPossibleCallOnUpdate());

	private async UniTask ResidentFlowInitializeWait(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (_enableResidentFlowInitializeWait && !ResidentFlow.IsInitialized)
		{
			await UniTask.WaitUntil(() => ResidentFlow.IsInitialized, PlayerLoopTiming.Update, cancellationToken);
		}
	}

	protected virtual void Awake()
	{
	}

	protected virtual async void Start()
	{
		_ = 3;
		try
		{
			CancellationToken cancellationTokenOnDestroy = this.GetCancellationTokenOnDestroy();
			if (_enableLoadingTipsDisp)
			{
				await ExecLoadingTipsDisp();
			}
			await ResidentFlowInitializeWait(cancellationTokenOnDestroy);
			if (_enableDebugLogin)
			{
				await DebugLogin(cancellationTokenOnDestroy);
			}
			await OnInitialize(cancellationTokenOnDestroy);
			_isInitialized = true;
			OnExecute(cancellationTokenOnDestroy).Forget();
			OnUpdateAsObservable.Subscribe(delegate
			{
				OnUpdate();
			}).AddTo(this);
		}
		catch (OperationCanceledException)
		{
			Debug.Log(GetType().Name + ": OperationCanceledException catched.");
		}
	}

	protected virtual void Update()
	{
		if (IsPossibleCallOnUpdate())
		{
			_onUpdateSubject.OnNext(Unit.Default);
			if (GameUtility.IsCloudStreaming)
			{
				ScopeConnectCheck();
			}
		}
	}

	protected virtual async UniTask OnInitialize(CancellationToken cancellationToken = default(CancellationToken))
	{
	}

	protected virtual async UniTask OnExecute(CancellationToken cancellationToken = default(CancellationToken))
	{
	}

	protected virtual void OnUpdate()
	{
	}

	protected virtual bool IsPossibleCallOnUpdate()
	{
		if (_isInitialized)
		{
			return !_isNextSceneDecided;
		}
		return false;
	}

	protected virtual async UniTask DebugLogin(CancellationToken cancellationToken = default(CancellationToken))
	{
	}

	protected virtual async UniTask ExecLoadingTipsDisp()
	{
		await ResidentFlow.GetInstance().StartLoading();
	}

	protected virtual void GotoNextScene(string sceneName)
	{
		GotoNextScene(sceneName, enableDecidedCheck: true);
	}

	protected virtual void GotoNextScene(string sceneName, bool enableDecidedCheck)
	{
		if (!enableDecidedCheck || !_isNextSceneDecided)
		{
			_isNextSceneDecided = true;
			LoadScene(sceneName);
		}
	}

	protected virtual void LoadScene(string sceneName)
	{
		ResidentFlow.GetInstance().LoadScene(sceneName);
	}

	protected virtual CountDownTimer GetCountDownTimer()
	{
		return ResidentFlow.GetInstance().GetTimer();
	}

	[DllImport("kernel32.dll")]
	private static extern short QueryPerformanceFrequency(ref long x);

	[DllImport("kernel32.dll")]
	private static extern short QueryPerformanceCounter(ref long x);

	public void ScopeConnectCheck()
	{
		if (Cloudstrap.ScopeStatus == StreamingScopeStatus.None)
		{
			return;
		}
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ScopeLib.ScopeCloudGamingStatusInformationParam)));
		try
		{
			ScopeLib.ScopeCloudGamingGetStatusInformation(intPtr);
			ScopeLib.ScopeCloudGamingStatusInformationParam scopeCloudGamingStatusInformationParam = (ScopeLib.ScopeCloudGamingStatusInformationParam)Marshal.PtrToStructure(intPtr, typeof(ScopeLib.ScopeCloudGamingStatusInformationParam));
			long x = 0L;
			long x2 = 0L;
			QueryPerformanceFrequency(ref x);
			QueryPerformanceCounter(ref x2);
			if (x == 0L && x2 == 0L)
			{
				return;
			}
			if (((double)x2 - (double)scopeCloudGamingStatusInformationParam.LastAccessRawTime) / (double)x >= 10.0)
			{
				if (Cloudstrap.ScopeStatus == StreamingScopeStatus.Connect)
				{
					Debug.Log("ScopeCloudGamingGetStatusInformation CLM_DISCONNECT\n");
					Cloudstrap.ScopeStatus = StreamingScopeStatus.Disconnect;
				}
			}
			else if (Cloudstrap.IsDisconnect)
			{
				Debug.Log("ScopeCloudGamingGetStatusInformation CLM_RECONNECT\n");
				Cloudstrap.ScopeStatus = StreamingScopeStatus.Connect;
			}
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
		finally
		{
			Marshal.FreeHGlobal(intPtr);
		}
	}
}
