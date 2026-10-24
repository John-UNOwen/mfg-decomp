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
using UnityEngine.UI;

namespace MFG.MahjongPlay.Cutin;

public class KnockdownTileAgariCutin : CutinController
{
	private int m_lostScoreCharaCount;

	private CutinAssetContainer m_yakumanSubCutin;

	private bool m_isPlaying;

	private bool m_isPlayingIntro;

	private bool m_isPlayingYakumanIntro;

	private GameObject m_openingEffect;

	private void Awake()
	{
		ObservableStateMachineTrigger behaviour = GetComponent<Animator>().GetBehaviour<ObservableStateMachineTrigger>();
		behaviour.OnStateEnterAsObservable().Subscribe(delegate
		{
		}).AddTo(this);
		behaviour.OnStateExitAsObservable().Subscribe(delegate(ObservableStateMachineTrigger.OnStateInfo onStateInfo)
		{
			if (onStateInfo.StateInfo.IsName("NewAgariCutIn"))
			{
				m_isPlayingIntro = false;
			}
		}).AddTo(this);
	}

	public override async UniTask Prepare(CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		m_lostScoreCharaCount = 0;
		Transform parent = base.transform.Find("TujoAgariRival");
		foreach (IPlayerInteractor p in m_players)
		{
			if (p.IsActive() && p.IsScorePaymentPlayer())
			{
				IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.Resolve(PathType.CutinItem, p.CharaType(), CutinAssetType.HoujuCutin), null, cancellationToken);
				disposableAsset.AddTo(this);
				Transform obj = parent.Find($"Rival{m_lostScoreCharaCount + 1}").Find("Mask");
				obj.Find("Chara").GetComponent<Image>().sprite = disposableAsset.Asset;
				obj.Find("Bg").GetComponent<Image>().color = GetChaColor(p.Cha());
				m_lostScoreCharaCount++;
			}
		}
		if (m_sourcePlayer.IsCatharsisEffectActive())
		{
			Transform effectBase = base.transform.Find("EF");
			IDisposableAsset<GameObject> disposableAsset2 = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/Comeback/Prefabs/SmallAgariComebackEffect.prefab", null, cancellationToken);
			disposableAsset2.AddTo(this);
			m_openingEffect = Object.Instantiate(disposableAsset2.Asset, effectBase.transform);
			m_openingEffect.SetActive(value: false);
		}
		else if (m_sourcePlayer.IsMeldBonusEffectActive())
		{
			Transform effectBase = base.transform.Find("EF");
			IDisposableAsset<GameObject> disposableAsset3 = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/MeldBonus/Prefab/SmallAgariMeldBonusEffect.prefab", null, cancellationToken);
			disposableAsset3.AddTo(this);
			m_openingEffect = Object.Instantiate(disposableAsset3.Asset, effectBase.transform);
			m_openingEffect.SetActive(value: false);
		}
		else if (m_sourcePlayer.IsOpenReachEffectActive())
		{
			Transform effectBase = base.transform.Find("EF");
			IDisposableAsset<GameObject> disposableAsset4 = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/OpenReach/Prefabs/SmallAgariEffect.prefab", null, cancellationToken);
			disposableAsset4.AddTo(this);
			m_openingEffect = Object.Instantiate(disposableAsset4.Asset, effectBase.transform);
			m_openingEffect.SetActive(value: false);
		}
		else if (m_sourcePlayer.IsMentanpinLowEffectActive())
		{
			Transform effectBase = base.transform.Find("EF");
			IDisposableAsset<GameObject> disposableAsset5 = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/Mentanpin/Prefabs/SmallAgariEffect.prefab", null, cancellationToken);
			disposableAsset5.AddTo(this);
			m_openingEffect = Object.Instantiate(disposableAsset5.Asset, effectBase.transform);
			m_openingEffect.SetActive(value: false);
		}
		else if (m_sourcePlayer.IsFireReachEffectActive())
		{
			Transform effectBase = base.transform.Find("EF");
			effectBase.GetComponent<Image>().color = new Color32(221, 53, 47, byte.MaxValue);
			IDisposableAsset<GameObject> disposableAsset6 = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/SmallAgariFireEffect.prefab", null, cancellationToken);
			disposableAsset6.AddTo(this);
			m_openingEffect = Object.Instantiate(disposableAsset6.Asset, effectBase.transform);
			m_openingEffect.SetActive(value: false);
		}
		else if (m_sourcePlayer.IsBombEffectActive())
		{
			Transform effectBase = base.transform.Find("EF");
			IDisposableAsset<GameObject> disposableAsset7 = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/Bomb/Prefab/SmallAgariBombEffect.prefab", null, cancellationToken);
			disposableAsset7.AddTo(this);
			m_openingEffect = Object.Instantiate(disposableAsset7.Asset, effectBase.transform);
			m_openingEffect.SetActive(value: false);
		}
		else if (m_sourcePlayer.IsAllGreenEffectActive())
		{
			Transform effectBase = base.transform.Find("EF");
			IDisposableAsset<GameObject> disposableAsset8 = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/AllGreen/Prefab/SmallAgariAllGreenAgariEffect.prefab", null, cancellationToken);
			disposableAsset8.AddTo(this);
			m_openingEffect = Object.Instantiate(disposableAsset8.Asset, effectBase.transform);
			m_openingEffect.SetActive(value: false);
		}
		else if (m_sourcePlayer.IsKirisameEffectActive())
		{
			Transform effectBase = base.transform.Find("EF");
			IDisposableAsset<GameObject> disposableAsset9 = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/Kirisame/Prefab/SmallAgariKirisameEffect.prefab", null, cancellationToken);
			disposableAsset9.AddTo(this);
			m_openingEffect = Object.Instantiate(disposableAsset9.Asset, effectBase.transform);
			m_openingEffect.SetActive(value: false);
		}
		else if (m_sourcePlayer.IsAtomicReachEffectActive())
		{
			Transform effectBase = base.transform.Find("EF");
			effectBase.GetComponent<Image>().color = new Color32(221, 53, 47, byte.MaxValue);
			IDisposableAsset<GameObject> disposableAsset10 = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/AtomicReach/Prefabs/SmallAgariEffect.prefab", null, cancellationToken);
			disposableAsset10.AddTo(this);
			m_openingEffect = Object.Instantiate(disposableAsset10.Asset, effectBase.transform);
			m_openingEffect.SetActive(value: false);
		}
		else if (m_sourcePlayer.IsAccelDoraEffectActive())
		{
			Transform effectBase = base.transform.Find("EF");
			effectBase.GetComponent<Image>().color = new Color32(221, 53, 47, byte.MaxValue);
			IDisposableAsset<GameObject> disposableAsset11 = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/SmallAgariAccelDoraEffect.prefab", null, cancellationToken);
			disposableAsset11.AddTo(this);
			m_openingEffect = Object.Instantiate(disposableAsset11.Asset, effectBase.transform);
			m_openingEffect.SetActive(value: false);
		}
		if (m_sourcePlayer.PaymentAmount() >= Han.Yakuman)
		{
			string text = ResolveYakumanCutin(m_sourcePlayer.PaymentAmount(), m_sourcePlayer.PaymentYakus());
			if (!string.IsNullOrEmpty(text))
			{
				m_yakumanSubCutin = new CutinAssetContainer();
				await m_yakumanSubCutin.LoadAndInstantiate(text, m_sourcePlayer, GetComponent<CutinEventNotifier>(), cancellationToken);
			}
		}
		static Color GetChaColor(Cha cha)
		{
			ColorUtility.TryParseHtmlString((new string[4] { "#20CA00", "#CB0064", "#C8CA00", "#009CCA" })[(int)cha], out var color);
			return color;
		}
	}

	public override async UniTask Play(CancellationToken cancellationToken = default(CancellationToken))
	{
		_ = 2;
		try
		{
			cancellationToken.ThrowIfCancellationRequested();
			m_isPlaying = true;
			m_isPlayingIntro = true;
			m_isPlayingYakumanIntro = true;
			await Prepare(cancellationToken);
			GameObject gameObject = m_sourcePlayer.KnockdownTileParent();
			GetComponent<CutinEventNotifier>().m_cutinHandTilesParent = gameObject;
			Animator component = GetComponent<Animator>();
			component.SetTrigger("In");
			component.Update(0f);
			if (m_openingEffect != null)
			{
				m_openingEffect.SetActive(value: true);
			}
			SubCutin_Reaction_SetTrigger($"Rival{m_lostScoreCharaCount}");
			Transform transform = gameObject.transform.Find("Player");
			if (transform != null)
			{
				GetComponentInChildren<CutinHaiController>().m_tileParent = transform.gameObject;
				m_sourcePlayer.CreateKnockdownTiles(transform.gameObject);
			}
			PlaySound(isPlay: true, cancellationToken).Forget();
			await UniTask.WaitWhile(() => CurrentAnimator_IsPlayingWhithState(new List<string> { "NewAgariCutInEnd", "NewAgariCutInLoop", "NewAgariCutIn" }), PlayerLoopTiming.Update, cancellationToken);
			if (m_yakumanSubCutin != null)
			{
				CutinController yakumanSubCutinController = m_yakumanSubCutin.Instance.GetComponent<CutinController>();
				await UniTask.WaitWhile(() => yakumanSubCutinController.IsPlaying(), PlayerLoopTiming.Update, cancellationToken);
			}
		}
		finally
		{
			m_yakumanSubCutin?.Dispose();
		}
	}

	public override async UniTask End(CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		SubCutin_Reaction_SetTrigger($"Rival{m_lostScoreCharaCount}End");
		GetComponent<Animator>().SetTrigger("End");
		GetComponent<Animator>().Update(0f);
		m_yakumanSubCutin?.Instance.GetComponent<Animator>().SetTrigger("End");
		await UniTask.WaitUntil(() => CurrentAnimator_IsPlayingWhithState(new List<string> { "NewAgariCutInWait" }), PlayerLoopTiming.Update, cancellationToken);
		m_isPlaying = false;
		m_isPlayingIntro = false;
	}

	protected override async UniTask PlaySound(bool isPlay, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (!AudioOutputEnable)
		{
			return;
		}
		Item item = CutinItemMaster.GetItem(m_itemID);
		CharaType charaType = item.CharaType;
		if (item.IsComical)
		{
			SoundManager.PlaySe(isPlay, "SFX_MJG_CALL3_SR_COMIC1").Forget();
		}
		else
		{
			SoundManager.PlaySe(isPlay, "SFX_MJG_CALL3_N").Forget();
		}
		foreach (IPlayerInteractor player in m_players)
		{
			if (player.IsActive() && player.IsScorePaymentPlayer())
			{
				await SoundManager.PlayVoiceCha(isPlay, player.IsFakeClear() ? ("VOX_G12_KAGE_" + VoiceManager.FakeClear_ToLabel(player.CharaVariantID()) + "_KO") : "VOX_GXX_MJG_KO1", player.CharaType(), Cha.Toimen);
			}
		}
		await UniTask.Delay(1500, ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
		if (m_sourcePlayer.IsFakeClear())
		{
			SoundManager.PlayVoice(isPlay, "VOX_G12_KAGE_" + VoiceManager.FakeClear_ToLabel(m_sourcePlayer.CharaVariantID()) + "_AGARI", charaType).Forget();
			return;
		}
		VoiceManager.PlayVoiceAgariN(isPlay, new Player
		{
			m_charaType = charaType,
			m_level = m_sourcePlayer.FavoLevel()
		});
	}

	public override async UniTask Stop(CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		SubCutin_Reaction_SetTrigger($"Rival{m_lostScoreCharaCount}End");
		PlaySound(isPlay: false, cancellationToken).Forget();
		m_isPlaying = false;
		m_isPlayingIntro = false;
	}

	public override bool IsPlaying()
	{
		return m_isPlaying;
	}

	public override bool IsPlayingIntro()
	{
		if (!m_isPlayingIntro)
		{
			return (m_yakumanSubCutin?.Instance?.GetComponent<CutinController>().IsPlayingIntro()).GetValueOrDefault();
		}
		return true;
	}

	public override bool IsPrepared()
	{
		return true;
	}

	private void SubCutin_Reaction_SetTrigger(string triggerName)
	{
		(base.transform.Find("TujoAgariRival")?.GetComponent<Animator>())?.SetTrigger(triggerName);
	}
}
