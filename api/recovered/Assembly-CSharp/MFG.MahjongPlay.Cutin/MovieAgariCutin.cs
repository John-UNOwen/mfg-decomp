using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.CutinItem;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace MFG.MahjongPlay.Cutin;

public class MovieAgariCutin : CutinController
{
	private CutinMovie m_cutinMovie;

	private CutinAssetContainer m_yakumanSubCutin;

	private bool m_isPlaying;

	private bool m_isPlayingIntro;

	private bool m_isPlayingYakumanIntro;

	private GameObject m_openingEffect;

	private void Awake()
	{
		m_cutinMovie = GetComponent<CutinMovie>();
		ObservableStateMachineTrigger behaviour = GetComponent<Animator>().GetBehaviour<ObservableStateMachineTrigger>();
		behaviour.OnStateEnterAsObservable().Subscribe(delegate
		{
		}).AddTo(this);
		behaviour.OnStateExitAsObservable().Subscribe(delegate(ObservableStateMachineTrigger.OnStateInfo onStateInfo)
		{
			if (onStateInfo.StateInfo.IsName("UrAgariIn"))
			{
				m_isPlayingIntro = false;
			}
		}).AddTo(this);
	}

	public override async UniTask Prepare(CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (m_sourcePlayer.IsCatharsisEffectActive())
		{
			IDisposableAsset<GameObject> disposableAsset = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/Comeback/Prefabs/BigComebackAgariEffect.prefab", null, cancellationToken);
			disposableAsset.AddTo(this);
			m_openingEffect = Object.Instantiate(disposableAsset.Asset, base.transform);
			m_openingEffect.SetActive(value: false);
		}
		else if (m_sourcePlayer.IsMeldBonusEffectActive())
		{
			IDisposableAsset<GameObject> disposableAsset2 = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/MeldBonus/Prefab/BigMeldBonusAgariEffect.prefab", null, cancellationToken);
			disposableAsset2.AddTo(this);
			m_openingEffect = Object.Instantiate(disposableAsset2.Asset, base.transform);
			m_openingEffect.SetActive(value: false);
		}
		else if (m_sourcePlayer.IsMentanpinHighEffectActive())
		{
			IDisposableAsset<GameObject> disposableAsset3 = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/Mentanpin/Prefabs/BigAgariHighEffect.prefab", null, cancellationToken);
			disposableAsset3.AddTo(this);
			m_openingEffect = Object.Instantiate(disposableAsset3.Asset, base.transform);
			m_openingEffect.SetActive(value: false);
		}
		else if (m_sourcePlayer.IsMentanpinLowEffectActive())
		{
			IDisposableAsset<GameObject> disposableAsset4 = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/Mentanpin/Prefabs/BigAgariLowEffect.prefab", null, cancellationToken);
			disposableAsset4.AddTo(this);
			m_openingEffect = Object.Instantiate(disposableAsset4.Asset, base.transform);
			m_openingEffect.SetActive(value: false);
		}
		else if (m_sourcePlayer.IsFireReachEffectActive())
		{
			IDisposableAsset<GameObject> disposableAsset5 = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/BigAgariFireEffect.prefab", null, cancellationToken);
			disposableAsset5.AddTo(this);
			m_openingEffect = Object.Instantiate(disposableAsset5.Asset, base.transform);
			m_openingEffect.SetActive(value: false);
		}
		else if (m_sourcePlayer.IsOpenReachEffectActive())
		{
			IDisposableAsset<GameObject> disposableAsset6 = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/OpenReach/Prefabs/BigAgariEffect.prefab", null, cancellationToken);
			disposableAsset6.AddTo(this);
			m_openingEffect = Object.Instantiate(disposableAsset6.Asset, base.transform);
			m_openingEffect.SetActive(value: false);
		}
		else if (m_sourcePlayer.IsBombEffectActive())
		{
			IDisposableAsset<GameObject> disposableAsset7 = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/Bomb/Prefab/BigBombAgariEffect.prefab", null, cancellationToken);
			disposableAsset7.AddTo(this);
			m_openingEffect = Object.Instantiate(disposableAsset7.Asset, base.transform);
			m_openingEffect.SetActive(value: false);
		}
		else if (m_sourcePlayer.IsAllGreenEffectActive())
		{
			IDisposableAsset<GameObject> disposableAsset8 = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/AllGreen/Prefab/BigAllGreenAgariEffect.prefab", null, cancellationToken);
			disposableAsset8.AddTo(this);
			m_openingEffect = Object.Instantiate(disposableAsset8.Asset, base.transform);
			m_openingEffect.SetActive(value: false);
		}
		else if (m_sourcePlayer.IsKirisameEffectActive())
		{
			IDisposableAsset<GameObject> disposableAsset9 = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/Kirisame/Prefab/BigKirisameAgariEffect.prefab", null, cancellationToken);
			disposableAsset9.AddTo(this);
			m_openingEffect = Object.Instantiate(disposableAsset9.Asset, base.transform);
			m_openingEffect.SetActive(value: false);
		}
		else if (m_sourcePlayer.IsAtomicReachEffectActive())
		{
			IDisposableAsset<GameObject> disposableAsset10 = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/AtomicReach/Prefabs/BigAgariEffect.prefab", null, cancellationToken);
			disposableAsset10.AddTo(this);
			m_openingEffect = Object.Instantiate(disposableAsset10.Asset, base.transform);
			m_openingEffect.SetActive(value: false);
		}
		else if (m_sourcePlayer.IsAccelDoraEffectActive())
		{
			IDisposableAsset<GameObject> disposableAsset11 = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/BigAgariAccelDoraEffect.prefab", null, cancellationToken);
			disposableAsset11.AddTo(this);
			m_openingEffect = Object.Instantiate(disposableAsset11.Asset, base.transform);
			m_openingEffect.SetActive(value: false);
		}
		else if (m_sourcePlayer.IsChocolateReachEffectActive())
		{
			IDisposableAsset<GameObject> disposableAsset12 = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/Chocolate/Prefabs/BigAgariChocolateEffect.prefab", null, cancellationToken);
			disposableAsset12.AddTo(this);
			m_openingEffect = Object.Instantiate(disposableAsset12.Asset, base.transform);
			m_openingEffect.SetActive(value: false);
		}
		if (m_sourcePlayer.PaymentAmount() >= Han.Yakuman)
		{
			string text = ResolveYakumanCutin(m_sourcePlayer.PaymentAmount(), m_sourcePlayer.PaymentYakus());
			if (!string.IsNullOrEmpty(text))
			{
				m_yakumanSubCutin = new CutinAssetContainer();
				m_yakumanSubCutin.AddTo(this);
				await m_yakumanSubCutin.LoadAndInstantiate(text, m_sourcePlayer, GetComponent<CutinEventNotifier>(), cancellationToken);
			}
		}
	}

	public override async UniTask Play(CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		m_isPlaying = true;
		m_isPlayingIntro = true;
		await Prepare(cancellationToken);
		GetComponent<Animator>().SetTrigger("In");
		GetComponent<Animator>().Update(0f);
		PlaySound(isPlay: true, cancellationToken).Forget();
		if (m_openingEffect != null)
		{
			m_openingEffect.SetActive(value: true);
			if (m_sourcePlayer.IsCatharsisEffectActive())
			{
				SoundManager.PlaySe("SFX_MJG_DISPLAY25").Forget();
			}
			else if (m_sourcePlayer.IsMeldBonusEffectActive())
			{
				SoundManager.PlaySe("SFX_MJG_DISPLAY25").Forget();
			}
			else if (m_sourcePlayer.IsMentanpinHighEffectActive())
			{
				SoundManager.PlaySe("SFX_MJG_DISPLAY25").Forget();
			}
			else if (m_sourcePlayer.IsMentanpinLowEffectActive())
			{
				SoundManager.PlaySe("SFX_MJG_DISPLAY25").Forget();
			}
			else if (m_sourcePlayer.IsFireReachEffectActive())
			{
				SoundManager.PlaySe("SFX_MJG_DISPLAY20").Forget();
			}
			else if (m_sourcePlayer.IsOpenReachEffectActive())
			{
				SoundManager.PlaySe("SFX_MJG_DISPLAY33").Forget();
			}
			else if (m_sourcePlayer.IsBombEffectActive())
			{
				SoundManager.PlaySe("SFX_MJG_DISPLAY25").Forget();
			}
			else if (m_sourcePlayer.IsAllGreenEffectActive())
			{
				SoundManager.PlaySe("SFX_MJG_DISPLAY25").Forget();
			}
			else if (m_sourcePlayer.IsKirisameEffectActive())
			{
				SoundManager.PlaySe("SFX_MJG_DISPLAY25").Forget();
			}
			else if (m_sourcePlayer.IsAtomicReachEffectActive())
			{
				SoundManager.PlaySe("SFX_MJG_DISPLAY33").Forget();
			}
			else if (m_sourcePlayer.IsAccelDoraEffectActive())
			{
				SoundManager.PlaySe("SFX_MJG_DISPLAY25").Forget();
			}
			else if (m_sourcePlayer.IsChocolateReachEffectActive())
			{
				SoundManager.PlaySe("SFX_MJG_DISPLAY32").Forget();
			}
		}
		await UniTask.WaitWhile(() => CurrentAnimator_IsPlayingWhithState(new List<string> { "UrAgariIn", "UrAgariLoop", "UrAgariEnd" }), PlayerLoopTiming.Update, cancellationToken);
		if (m_yakumanSubCutin != null)
		{
			CutinController yakumanSubCutinController = m_yakumanSubCutin.Instance.GetComponent<CutinController>();
			await UniTask.WaitWhile(() => yakumanSubCutinController.IsPlaying(), PlayerLoopTiming.Update, cancellationToken);
			m_yakumanSubCutin.Dispose();
			m_isPlayingIntro = false;
		}
	}

	public override async UniTask End(CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		GetComponent<Animator>().SetTrigger("End");
		GetComponent<Animator>().Update(0f);
		if (m_yakumanSubCutin != null && m_yakumanSubCutin.Instance != null)
		{
			m_yakumanSubCutin.Instance.GetComponent<Animator>().SetTrigger("End");
		}
		await UniTask.WaitUntil(() => CurrentAnimator_IsPlayingWhithState(new List<string> { "UrAgariWait" }), PlayerLoopTiming.Update, cancellationToken);
		m_isPlaying = false;
		m_isPlayingIntro = false;
	}

	public override async UniTask Stop(CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		m_cutinMovie.Stop();
		PlaySound(isPlay: false, cancellationToken).Forget();
		m_isPlaying = false;
		m_isPlayingIntro = false;
	}

	protected override async UniTask PlaySound(bool isPlay, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (AudioOutputEnable)
		{
			Item item = CutinItemMaster.GetItem(m_itemID);
			CharaType charaType = item.CharaType;
			if (item.Rarity == Rarity.UR)
			{
				SoundManager.PlaySe(isPlay, $"SFX_MJG_CALL3_UR_G{(int)(charaType + 1):D2}").Forget();
			}
			else
			{
				SoundManager.PlaySe(isPlay, $"SFX_MJG_CALL3_SP_G{(int)(charaType + 1):D2}").Forget();
			}
			if (m_sourcePlayer.IsFakeClear())
			{
				SoundManager.PlayVoice(isPlay, "VOX_G12_KAGE_" + VoiceManager.FakeClear_ToLabel(m_sourcePlayer.CharaVariantID()) + "_AGARI", charaType).Forget();
			}
			else if (item.Rarity == Rarity.UR)
			{
				SoundManager.PlayVoice(isPlay, "VOX_GXX_MJG_AGARI_UR", charaType).Forget();
			}
			else
			{
				SoundManager.PlayVoice(isPlay, "VOX_GXX_MJG_AGARI_SP", charaType).Forget();
			}
		}
	}

	public override bool IsPlaying()
	{
		return m_isPlaying;
	}

	public override bool IsPlayingIntro()
	{
		if (m_isPlayingIntro)
		{
			return true;
		}
		if (m_yakumanSubCutin != null && m_yakumanSubCutin.Instance != null)
		{
			CutinController component = m_yakumanSubCutin.Instance.GetComponent<CutinController>();
			if (component != null)
			{
				return component.IsPlayingIntro();
			}
		}
		return false;
	}

	public override bool IsPrepared()
	{
		return m_cutinMovie.IsPrepared;
	}
}
