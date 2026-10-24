using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class PlayCountResultWindow : MonoBehaviour
{
	public enum ResultType
	{
		None,
		MissionEvent,
		EventTaku,
		BingoEvent
	}

	[SerializeField]
	private Animator window;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private TextMeshProUGUI dateText;

	[SerializeField]
	private Image bgImage;

	[SerializeField]
	private Image titleImage;

	[SerializeField]
	private EventProgressMap progressMap;

	public GameEventType EventType;

	private bool isOpen;

	private TimeoutController timeoutController = new TimeoutController();

	private GameEventData EventData => GameDataManager.System.GameControlInfo.EventStatus.GetGameEventData(EventType);

	public bool IsOpen => isOpen;

	private void Start()
	{
		if (closeButton != null)
		{
			closeButton.OnClickAsObservable().Subscribe(delegate
			{
				Close();
			}).AddTo(this);
		}
	}

	private string BgName(EventProgressMap.ProgressInfo resultInfo)
	{
		return resultInfo.resultType switch
		{
			ResultType.MissionEvent => $"{resultInfo.resultType}/{EventType}/Bg.png", 
			ResultType.EventTaku => $"{resultInfo.Master.EventType}/Textures/Bg.png", 
			ResultType.BingoEvent => "Assets/MFG/Addressables/SceneParts/Bingo/Textures/BingoBg.png", 
			_ => "", 
		};
	}

	private string TitleName(EventProgressMap.ProgressInfo resultInfo)
	{
		return resultInfo.resultType switch
		{
			ResultType.MissionEvent => $"{resultInfo.resultType}/{EventType}/Title{SpriteNo(EventType)}.png", 
			ResultType.EventTaku => $"{resultInfo.Master.EventType}/Textures/Title{SpriteNo(EventType)}.png", 
			ResultType.BingoEvent => "Assets/MFG/Addressables/SceneParts/Bingo/Textures/EventTitle.png", 
			_ => "", 
		};
	}

	public async UniTask Open(GameEventType gameEventType, EventProgressMap.ProgressInfo resultInfo, Func<bool> skipFunc = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (resultInfo == null || isOpen)
		{
			return;
		}
		isOpen = true;
		AllButtonEnable(isEnable: false);
		EventType = gameEventType;
		IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(BgName(resultInfo), null, cancellationToken);
		disposableAsset.AddTo(this);
		bgImage.sprite = disposableAsset.Asset;
		disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(TitleName(resultInfo), null, cancellationToken);
		disposableAsset.AddTo(this);
		titleImage.sprite = disposableAsset.Asset;
		await progressMap.Init(resultInfo);
		CancellationToken token2 = timeoutController.Timeout(TimeSpan.FromSeconds((GameUtility.IsAppTerminatDecided || Cloudstrap.IsDisconnect) ? 1f : progressMap.PlayTime(resultInfo)));
		using CancellationTokenSource linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, token2);
		CancellationToken token = linkedCancellationTokenSource.Token;
		try
		{
			if (dateText != null)
			{
				dateText.text = EventData?.End.ToString("yyyy年MM月dd日（ddd）HH：mmまで") ?? "";
			}
			base.gameObject.SetActive(value: true);
			SoundManager.PlaySe("SFX_SYS_SLIDE3").Forget();
			window.SetTrigger("In");
			await progressMap.Play(token);
			AllButtonEnable(isEnable: true);
			await UniTask.WaitWhile(() => IsOpen, PlayerLoopTiming.Update, token);
		}
		catch (OperationCanceledException)
		{
			if (timeoutController.IsTimeout())
			{
				Close();
			}
		}
		finally
		{
			timeoutController.Reset();
		}
	}

	public void Close()
	{
		if (isOpen)
		{
			SoundManager.StopSeAll().Forget();
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			window.SetTrigger("Out");
			isOpen = false;
			progressMap.CharaInit();
		}
	}

	private void AllButtonEnable(bool isEnable)
	{
		if (closeButton != null)
		{
			closeButton.enabled = isEnable;
			closeButton.gameObject.SetActive(isEnable);
		}
	}

	private string SpriteNo(GameEventType eventType)
	{
		return eventType switch
		{
			GameEventType.MeldBonusTakuEvent2 => "2", 
			GameEventType.ComebackTakuEvent2 => "2", 
			_ => "", 
		};
	}
}
