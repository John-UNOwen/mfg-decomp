using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.Types;
using MFG.Types.Customize.Catchphrase;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class FightSpiritConsumeWindow : MonoBehaviour
{
	public enum ResultType
	{
		None,
		Consumed,
		Canceled
	}

	[SerializeField]
	private Button _consumeButton;

	[SerializeField]
	private Button _cancelButton;

	[SerializeField]
	private FightSpiritNumDisp _fightSpiritNumDisp;

	[SerializeField]
	private GameObject _bg;

	private bool _isOpen;

	private CharaType _charaType;

	private ItemID _itemID;

	private int _consumeCharaSpiritCount;

	private int _consumeWildSpiritCount;

	private IDisposableAsset<Sprite> _charaSpiritIconSprite;

	private ResultType _resultType;

	private Subject<Unit> _onCancel = new Subject<Unit>();

	private Subject<(ItemID itemID, CharaType charaType, int consumeCharaSpiritCount, int consumeWildSpiritCount)> _onConsume = new Subject<(ItemID, CharaType, int, int)>();

	public IObservable<(ItemID itemID, CharaType charaType, int consumeCharaSpiritCount, int consumeWildSpiritCount)> OnConsume => _onConsume.AsObservable();

	public IObservable<Unit> OnCancel => _onCancel.AsObservable();

	public ResultType GetResult()
	{
		return _resultType;
	}

	public void Close(bool isPlaySE = true)
	{
		if (_isOpen)
		{
			if (isPlaySE)
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			}
			if (_bg != null)
			{
				_bg.SetActive(value: false);
			}
			base.gameObject.SetActive(value: false);
			_isOpen = false;
		}
	}

	private void Start()
	{
		_consumeButton.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			_resultType = ResultType.Consumed;
			_onConsume.OnNext((_itemID, _charaType, _consumeCharaSpiritCount, _consumeWildSpiritCount));
		}).AddTo(this);
		_cancelButton.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			_resultType = ResultType.Canceled;
			_onCancel.OnNext(Unit.Default);
		}).AddTo(this);
	}

	public async UniTask Open(ItemID itemID, CharaType charaType, int consumeCharaSpiritCount, int consumeWildSpiritCount, bool isPlaySE = true, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (!_isOpen)
		{
			_isOpen = true;
			_itemID = itemID;
			_charaType = charaType;
			_consumeCharaSpiritCount = consumeCharaSpiritCount;
			_consumeWildSpiritCount = consumeWildSpiritCount;
			_resultType = ResultType.None;
			if (_bg != null)
			{
				_bg.SetActive(value: true);
			}
			if (isPlaySE)
			{
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			}
			_fightSpiritNumDisp.EnableMultiplierText = false;
			_fightSpiritNumDisp.Reflesh(_charaType, _consumeCharaSpiritCount, _consumeWildSpiritCount, cancellationToken).Forget();
			base.gameObject.SetActive(value: true);
			await UniTask.WaitWhile(() => _isOpen, PlayerLoopTiming.Update, cancellationToken);
		}
	}
}
