using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class HomeGachaModeSelectWindow : MonoBehaviour
{
	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private Button cutinGachaButton;

	[SerializeField]
	private Button spGachaButton;

	[SerializeField]
	private Button musicGachaButton;

	[SerializeField]
	private GameObject spGachaBanner;

	[SerializeField]
	private GameObject musicGachaBanner;

	private bool _isInitialized;

	private bool _isOpen;

	public bool IsDecided;

	public bool IsOpen => _isOpen;

	private async void Start()
	{
		try
		{
			this.GetCancellationTokenOnDestroy();
			await UniTask.WaitUntil(() => ResidentFlow.IsInitialized);
			closeButton.OnClickAsObservable().Subscribe(delegate
			{
				Close();
			}).AddTo(this);
			ResidentFlow.GetInstance().GetTimer().OnEnd.Where((Unit _) => ResidentFlow.IsInitialized && _isInitialized && _isOpen).Subscribe(delegate
			{
				Close();
			}).AddTo(this);
			cutinGachaButton.OnClickAsObservable().Subscribe(delegate
			{
				OnClickButton(GachaSeriesType.Pickup);
			}).AddTo(this);
			spGachaButton.OnClickAsObservable().Subscribe(delegate
			{
				OnClickButton(GachaSeriesType.Unlock);
			}).AddTo(this);
			musicGachaButton.OnClickAsObservable().Subscribe(delegate
			{
				OnClickButton(GachaSeriesType.Music);
			}).AddTo(this);
			_isInitialized = true;
		}
		catch (OperationCanceledException ex)
		{
			Debug.Log("OperationCanceledException Catched. " + ex.Message);
		}
	}

	public async UniTask<bool> Open(CancellationToken cancellationToken)
	{
		if (_isOpen)
		{
			return false;
		}
		_isOpen = true;
		bool flag = DebugFlag.IsActive(DebugFlag.Flag.EnableUnlimitedButton);
		spGachaBanner.SetActive(GameDataManager.GachaInfo.ModeGachaInfo(GachaSeriesType.Unlock).Count() > 0);
		List<GachaSeriesServerInfo> list = GameDataManager.GachaInfo.ModeGachaInfo(GachaSeriesType.Music);
		musicGachaBanner.SetActive((PaseliUtility.PPEX.IsEnablePurchaseItem || GameUtility.IsKonastaOrNetcafeBehavior) && list != null && list.Count() > 0);
		cutinGachaButton.interactable = !GameDataManager.PlayerConfig.IsCutinGachaSceneVisited || flag;
		spGachaButton.interactable = !GameDataManager.PlayerConfig.IsSpGachaSceneVisited || flag;
		musicGachaButton.interactable = (!GameDataManager.PlayerConfig.IsMusicGachaSceneVisited || flag) && (list?.Exists((GachaSeriesServerInfo x) => x.IsDisplay()) ?? false);
		base.gameObject.SetActive(value: true);
		GetComponent<Animator>().SetTrigger("In");
		SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
		await UniTask.WhenAny(UniTask.WaitWhile(() => !IsDecided, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitWhile(() => _isOpen, PlayerLoopTiming.Update, cancellationToken));
		return IsDecided;
	}

	public void Close()
	{
		if (_isOpen)
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			GetComponent<Animator>().SetTrigger("Out");
			_isOpen = false;
		}
	}

	private void OnClickButton(GachaSeriesType gachaType)
	{
		if (!IsDecided)
		{
			switch (gachaType)
			{
			case GachaSeriesType.Unlock:
				GameDataManager.PlayerConfig.IsSpGachaSceneVisited = true;
				GameDataManager.Gacha.EntryGachaSelectSceneMode = GachaSeriesType.Unlock;
				break;
			case GachaSeriesType.Music:
				GameDataManager.PlayerConfig.IsMusicGachaSceneVisited = true;
				GameDataManager.Gacha.EntryGachaSelectSceneMode = GachaSeriesType.Music;
				break;
			default:
				GameDataManager.PlayerConfig.IsCutinGachaSceneVisited = true;
				GameDataManager.Gacha.EntryGachaSelectSceneMode = GachaSeriesType.Pickup;
				break;
			}
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			IsDecided = true;
		}
	}
}
