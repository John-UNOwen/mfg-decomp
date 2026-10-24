using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.MasterGameData;
using MFG.Taikyoku.Command;
using MFG.Types;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MahjongActorNotice : MonoBehaviour, IDisposable
{
	public class NoticeInfo
	{
		public MahjongActorID MahjongActorID;

		public bool IsStaying;

		public GAME_MODE LatestGameMode;
	}

	[SerializeField]
	private Image _actorIconImage;

	[SerializeField]
	private TextMeshProUGUI _noticeText;

	private NoticeInfo _noticeInfo;

	private IDisposableAsset<Sprite> _actorIconSprite;

	private SemaphoreSlim _loadSemaphore = new SemaphoreSlim(1, 1);

	public void Dispose()
	{
		ActorIcon_Dispose();
	}

	private void OnDestroy()
	{
		Dispose();
	}

	public async UniTask SetDisp(bool isDisp)
	{
		await _loadSemaphore.WaitAsync();
		bool flag = GetComponent<CanvasGroup>().alpha > 0f;
		try
		{
			GetComponent<CanvasGroup>().alpha = (isDisp ? 1f : 0f);
			if (flag != isDisp)
			{
				_noticeInfo = null;
			}
		}
		finally
		{
			_loadSemaphore.Release();
		}
	}

	private async UniTask Fadeout(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (base.gameObject.activeInHierarchy)
		{
			Animator animator = GetComponent<Animator>();
			animator.SetTrigger("Out");
			await UniTask.Delay(TimeSpan.FromSeconds(0.5), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
			animator.ResetTrigger("Out");
		}
	}

	private async UniTask Fadein(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (base.gameObject.activeInHierarchy)
		{
			Animator animator = GetComponent<Animator>();
			animator.SetTrigger("In");
			await UniTask.Delay(TimeSpan.FromSeconds(0.5), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
			animator.ResetTrigger("In");
		}
	}

	public async UniTask Reflesh(NoticeInfo noticeInfo, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		await _loadSemaphore.WaitAsync();
		try
		{
			if (noticeInfo.MahjongActorID == MahjongActorID.None)
			{
				await Fadeout(cancellationToken);
			}
			else if (_noticeInfo == null || _noticeInfo.MahjongActorID != noticeInfo.MahjongActorID || _noticeInfo.LatestGameMode != noticeInfo.LatestGameMode)
			{
				if (_noticeInfo != null)
				{
					await Fadeout(cancellationToken);
				}
				await ActorIcon_Reflesh(noticeInfo.MahjongActorID, cancellationToken);
				_noticeInfo = noticeInfo;
				_noticeText.text = CreateNoticeText(noticeInfo);
				Fadein(cancellationToken).Forget();
			}
		}
		catch (OperationCanceledException)
		{
		}
		finally
		{
			_loadSemaphore.Release();
		}
	}

	private async UniTask ActorIcon_Reflesh(MahjongActorID mahjongActorID, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (_noticeInfo == null || _noticeInfo.MahjongActorID != mahjongActorID)
		{
			ActorIcon_Dispose();
			_actorIconSprite = await AssetLoader.LoadAssetAsync<Sprite>(MahjongActorMaster.GetItem(mahjongActorID).ActorIconPath, null, cancellationToken);
			_actorIconImage.sprite = _actorIconSprite.Asset;
		}
	}

	private void ActorIcon_Dispose()
	{
		if (_actorIconSprite != null)
		{
			_actorIconSprite.Dispose();
			_actorIconSprite = null;
		}
	}

	private string CreateNoticeText(NoticeInfo noticeInfo)
	{
		string fullName = MahjongActorMaster.GetItem(noticeInfo.MahjongActorID).FullName;
		string text = CreateModeNameText(noticeInfo.LatestGameMode);
		if (noticeInfo.LatestGameMode <= (GAME_MODE)0 || noticeInfo.LatestGameMode == GAME_MODE.GLOBAL_NIMA)
		{
			return "MA「" + fullName + "」プロが現在プレー中！プロ雀士と対局のチャンス！";
		}
		return "MA「" + fullName + "」プロが現在【" + text + "】でプレー中！プロ雀士と対局のチャンス！";
	}

	private string CreateModeNameText(GAME_MODE gameMode)
	{
		return gameMode switch
		{
			GAME_MODE.GLOBAL_NIMA => "2人打ち", 
			GAME_MODE.GLOBAL_SANMA => "3人打ち", 
			GAME_MODE.GLOBAL_TONPU => "4人打ち", 
			GAME_MODE.GLOBAL_HANCHAN => "4人打ち", 
			_ => "イベント対局", 
		};
	}
}
