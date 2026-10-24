using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.MasterGameData;
using MFG.Types.Customize.Catchphrase;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class FightSpiritConsumeResultWindow : MonoBehaviour
{
	[SerializeField]
	private Button _closeButton;

	[SerializeField]
	private GameObject _bg;

	[SerializeField]
	private TextMeshProUGUI _text;

	[SerializeField]
	private Animator _window;

	private bool _isOpen;

	private ItemID _itemID;

	private void Start()
	{
		CancellationToken cancellationToken = this.GetCancellationTokenOnDestroy();
		_closeButton.OnClickAsObservable().Subscribe(async delegate
		{
			try
			{
				await Close(isPlaySE: true, cancellationToken);
			}
			catch (OperationCanceledException)
			{
			}
		}).AddTo(this);
	}

	public async UniTask Open(ItemID itemID, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (!_isOpen)
		{
			_closeButton.gameObject.SetActive(value: true);
			base.gameObject.SetActive(value: true);
			_itemID = itemID;
			_isOpen = true;
			if (_bg != null)
			{
				_bg.SetActive(value: true);
			}
			_window.SetTrigger("In");
			SoundManager.PlaySe("SFX_SYS_BONUS5").Forget();
			SoundManager.PlayMyCharaVoice("VOX_GXX_ITEM2").Forget();
			Item item = CatchphraseItemMaster.GetItem(itemID);
			_text.text = "「" + item.DispName + "」を\n解禁しました。";
			await UniTask.WhenAny(UniTask.Delay(4000, ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => !_isOpen, PlayerLoopTiming.Update, cancellationToken));
			await Close(isPlaySE: false, cancellationToken);
		}
	}

	public async UniTask Close(bool isPlaySE = true, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (_isOpen)
		{
			if (isPlaySE)
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			}
			_window.SetTrigger("Out");
			await UniTask.Delay(250, ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
			if (_bg != null)
			{
				_bg.SetActive(value: false);
			}
			base.gameObject.SetActive(value: false);
			_isOpen = false;
		}
	}
}
