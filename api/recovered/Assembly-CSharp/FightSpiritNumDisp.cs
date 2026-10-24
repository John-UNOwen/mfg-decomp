using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MFG.Assets;
using MFG.Types;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class FightSpiritNumDisp : MonoBehaviour, IDisposable
{
	[SerializeField]
	private TextMeshProUGUI _charaNumText;

	[SerializeField]
	private TextMeshProUGUI _wildNumText;

	[SerializeField]
	private Image _charaSpiritImage;

	public bool EnableMultiplierText = true;

	private CharaType _currentCharaType = (CharaType)(-1);

	private IDisposableAsset<Sprite> _charaSpiritIconSprite;

	private SemaphoreSlim _loadSemaphore = new SemaphoreSlim(1, 1);

	public void Dispose()
	{
		if (_charaSpiritIconSprite != null)
		{
			_charaSpiritIconSprite.Dispose();
			_charaSpiritIconSprite = null;
		}
	}

	private void OnDestroy()
	{
		Dispose();
	}

	public async UniTask Reflesh(CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		CharaType selectChara = GameDataManager.Player.SelectChara;
		await Reflesh(selectChara, cancellationToken);
	}

	public async UniTask Reflesh(CharaType charaType, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		var (charaSpiritNum, wildSpiritNum) = GameDataManager.PlayerItem.GetFightSpiritCount(charaType);
		await Reflesh(charaType, charaSpiritNum, wildSpiritNum, cancellationToken);
	}

	public async UniTask Reflesh(CharaType charaType, int charaSpiritNum, int wildSpiritNum, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await _loadSemaphore.WaitAsync();
		try
		{
			if (_charaNumText != null)
			{
				_charaNumText.text = string.Format("{0}{1:N0}", EnableMultiplierText ? "×" : "", charaSpiritNum);
			}
			if (_wildNumText != null)
			{
				_wildNumText.text = string.Format("{0}{1:N0}", EnableMultiplierText ? "×" : "", wildSpiritNum);
			}
			if (_charaSpiritImage != null && _currentCharaType != charaType)
			{
				await CharaSpiritIcon_Reflesh(charaType, cancellationToken);
			}
			_currentCharaType = charaType;
		}
		finally
		{
			_loadSemaphore.Release();
		}
	}

	private async UniTask CharaSpiritIcon_Reflesh(CharaType charaType, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (_charaSpiritIconSprite != null)
		{
			_charaSpiritIconSprite.Dispose();
		}
		_charaSpiritImage.SetAlpha(0f);
		_charaSpiritIconSprite = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.Resolve(MFG.Assets.PathType.Dojo, charaType, DojoFlow.AssetType.FightSpirit), null, cancellationToken);
		_charaSpiritIconSprite.AddTo(this);
		if (_charaSpiritImage.sprite != null)
		{
			_charaSpiritImage.sprite = _charaSpiritIconSprite.Asset;
			await _charaSpiritImage.DOFade(1f, 0.05f).ToUniTask(TweenCancelBehaviour.Kill, cancellationToken);
		}
	}

	public Sprite GetCharaIconSprite()
	{
		return _charaSpiritIconSprite.Asset;
	}
}
