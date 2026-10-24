using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.GameData.EventTaku;
using MFG.Types;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace MFG.MahjongPlay.Cutin;

public class BlowAwayEventCutin : MonoBehaviour
{
	private int _lostScoreCharaCount;

	private bool _isPlayingIntro;

	private bool _isPlaying;

	private ResultInfo _resultInfo = new ResultInfo();

	private void Awake()
	{
		ObservableStateMachineTrigger behaviour = GetComponent<Animator>().GetBehaviour<ObservableStateMachineTrigger>();
		behaviour.OnStateEnterAsObservable().Subscribe(delegate(ObservableStateMachineTrigger.OnStateInfo onStateInfo)
		{
			if (onStateInfo.StateInfo.IsName("EventCutIn1In") || onStateInfo.StateInfo.IsName("EventCutIn2In") || onStateInfo.StateInfo.IsName("EventCutIn3In"))
			{
				_isPlaying = true;
				_isPlayingIntro = true;
			}
		}).AddTo(this);
		behaviour.OnStateExitAsObservable().Subscribe(delegate(ObservableStateMachineTrigger.OnStateInfo onStateInfo)
		{
			if (onStateInfo.StateInfo.IsName("EventCutIn1In") || onStateInfo.StateInfo.IsName("EventCutIn2In") || onStateInfo.StateInfo.IsName("EventCutIn3In"))
			{
				_isPlayingIntro = false;
			}
			else if (onStateInfo.StateInfo.IsName("EventCutIn1End") || onStateInfo.StateInfo.IsName("EventCutIn2End") || onStateInfo.StateInfo.IsName("EventCutIn3End"))
			{
				_isPlaying = false;
			}
		}).AddTo(this);
	}

	private async UniTask Prepare(CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		_lostScoreCharaCount = 0;
		Transform rivalParent = base.transform.Find("Cut/Rival");
		foreach (ChaInfo chaInfo in _resultInfo.BlowAwayInfo.ChaInfos.Values)
		{
			Transform rivalBase = rivalParent.Find($"HoujuChara{_lostScoreCharaCount + 1}");
			if (chaInfo.BlowawayState == BlowawayState.Blowawayed && _lostScoreCharaCount < 3)
			{
				IDisposableAsset<Sprite> smashedBeginCharaSpriteAsset = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.Resolve(PathType.CutinItem, chaInfo.CharaType, CutinAssetType.Houju), null, cancellationToken);
				smashedBeginCharaSpriteAsset.AddTo(this);
				IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.Resolve(PathType.CutinItem, chaInfo.CharaType, CutinAssetType.Houju2), null, cancellationToken);
				disposableAsset.AddTo(this);
				SmashedChara_SetActive(rivalBase, isActive: true, smashedBeginCharaSpriteAsset.Asset, disposableAsset.Asset);
				_lostScoreCharaCount++;
			}
		}
		for (int i = _lostScoreCharaCount; i < 3; i++)
		{
			Transform transform = rivalParent.Find($"HoujuChara{i + 1}");
			if (transform != null)
			{
				SmashedChara_SetActive(transform, isActive: false);
			}
		}
	}

	private void SmashedChara_SetActive(Transform rivalBase, bool isActive, Sprite smashedCharaBegin = null, Sprite smashedCharaEnd = null)
	{
		rivalBase.gameObject.SetActive(isActive);
		Image component = rivalBase.Find("Chara").GetComponent<Image>();
		component.enabled = isActive;
		if (smashedCharaBegin != null)
		{
			component.sprite = smashedCharaBegin;
		}
		Image component2 = rivalBase.Find("Houju2").GetComponent<Image>();
		component2.enabled = isActive;
		if (smashedCharaEnd != null)
		{
			component2.sprite = smashedCharaEnd;
		}
	}

	public async UniTask Play(ResultInfo resultInfo, CancellationToken cancellationToken = default(CancellationToken))
	{
		_resultInfo = resultInfo;
		cancellationToken.ThrowIfCancellationRequested();
		_isPlaying = true;
		_isPlayingIntro = true;
		base.gameObject.SetActive(value: true);
		await Prepare(cancellationToken);
		Animator component = GetComponent<Animator>();
		component.SetInteger("CharaNum", _lostScoreCharaCount);
		component.SetTrigger("In");
		component.Update(0f);
		await UniTask.WaitWhile(() => _isPlayingIntro, PlayerLoopTiming.Update, cancellationToken);
	}

	public async UniTask End(CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		GetComponent<Animator>().SetTrigger("End");
		GetComponent<Animator>().Update(0f);
		await UniTask.WaitWhile(() => _isPlaying, PlayerLoopTiming.Update, cancellationToken);
		_isPlayingIntro = false;
	}

	private async UniTask PlayEyeFlashSound(bool isPlay, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		SoundManager.PlaySe(isPlay, "SFX_MJG_DISPLAY15").Forget();
		if (!isPlay)
		{
			CancelInvoke();
		}
	}

	private async UniTask PlayBlowAwaySoundKo1(bool isPlay, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		foreach (ChaInfo value in _resultInfo.BlowAwayInfo.ChaInfos.Values)
		{
			if (value.BlowawayState == BlowawayState.Blowawayed)
			{
				await SoundManager.PlayVoiceCha(isPlay, "VOX_GXX_MJG_KO1", value.CharaType, Cha.Toimen);
			}
		}
		if (!isPlay)
		{
			CancelInvoke();
		}
	}

	private async UniTask PlayBlowAwaySoundKo2(bool isPlay, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		foreach (ChaInfo value in _resultInfo.BlowAwayInfo.ChaInfos.Values)
		{
			if (value.BlowawayState == BlowawayState.Blowawayed)
			{
				await SoundManager.PlayVoiceCha(isPlay, "VOX_GXX_MJG_KO2", value.CharaType, Cha.Toimen);
			}
		}
		if (!isPlay)
		{
			CancelInvoke();
		}
	}

	public async UniTask Stop(CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		base.gameObject.SetActive(value: false);
		PlayBlowAwaySoundKo1(isPlay: false, cancellationToken).Forget();
		_isPlaying = false;
		_isPlayingIntro = false;
	}

	public bool IsPlaying()
	{
		return _isPlaying;
	}

	public bool IsPlayingIntro()
	{
		return _isPlayingIntro;
	}

	public async void PlayBlowAwayKo1SE()
	{
		PlayBlowAwaySoundKo1(isPlay: true).Forget();
	}

	public async void PlayBlowAwayKo2SE()
	{
		PlayBlowAwaySoundKo2(isPlay: true).Forget();
	}

	public async void PlayEyeFlashSE()
	{
		PlayEyeFlashSound(isPlay: true).Forget();
	}
}
