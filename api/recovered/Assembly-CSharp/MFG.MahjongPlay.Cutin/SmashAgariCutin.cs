using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.CutinItem;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace MFG.MahjongPlay.Cutin;

public class SmashAgariCutin : CutinController
{
	public enum HanTextType
	{
		Normal,
		Pink,
		Pink3,
		KAC1,
		KAC2,
		KAC3,
		Stamp,
		Manga,
		Train
	}

	public enum KimeHojuCharaColor
	{
		None,
		HoujuBlue,
		Houju
	}

	[Serializable]
	public class SmashedUnableItems
	{
		public List<GameObject> objs = new List<GameObject>();
	}

	[SerializeField]
	private HanTextType m_hanTextType;

	[SerializeField]
	private string _hanTextSELabel = "SFX_MJG_DISPLAY10";

	[SerializeField]
	private string _smashSELabel = "SFX_MJG_CALL3_SR_PUNCH1";

	[SerializeField]
	private int _randomSECount;

	[SerializeField]
	private KimeHojuCharaColor _hojuKime = KimeHojuCharaColor.HoujuBlue;

	[SerializeField]
	[Header("Good演出の数字")]
	private TextMeshProUGUI _addText;

	[SerializeField]
	private List<GameObject> _houjuCharas = new List<GameObject>();

	[SerializeField]
	[Header("人数に応じて非アクティブにするオブジェクトを指定")]
	private List<SmashedUnableItems> _smashedItems = new List<SmashedUnableItems>();

	[SerializeField]
	[Header("rivalParent,_houjuCharas共にキャラを格納する")]
	private bool _isExpansionSmashedItems;

	[SerializeField]
	private bool _isCutinHandEnable;

	private int _randomVoiceIndex;

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
			if (onStateInfo.StateInfo.IsName("NewManganCutIn"))
			{
				m_isPlayingIntro = false;
			}
		}).AddTo(this);
	}

	public override async UniTask Prepare(CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		m_lostScoreCharaCount = 0;
		Transform rivalParent = base.transform.Find("Cut").Find("Rival");
		List<GameObject> houjuCharas = _houjuCharas;
		bool isHoujuCharaMove = houjuCharas != null && houjuCharas.Count() == 3;
		foreach (IPlayerInteractor p in m_players)
		{
			if (p.IsActive() && p.IsScorePaymentPlayer())
			{
				IDisposableAsset<Sprite> smashedBeginCharaSpriteAsset = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.Resolve(PathType.CutinItem, p.CharaType(), CutinAssetType.Houju), null, cancellationToken);
				smashedBeginCharaSpriteAsset.AddTo(this);
				IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.Resolve(PathType.CutinItem, p.CharaType(), (_hojuKime == KimeHojuCharaColor.Houju) ? CutinAssetType.Houju : CutinAssetType.HoujuBlue), null, cancellationToken);
				disposableAsset.AddTo(this);
				if (_isExpansionSmashedItems || isHoujuCharaMove)
				{
					SmashedChara_SetActive(_houjuCharas[m_lostScoreCharaCount].transform, isActive: true, smashedBeginCharaSpriteAsset.Asset, disposableAsset.Asset);
				}
				if (_isExpansionSmashedItems || !isHoujuCharaMove)
				{
					SmashedChara_SetActive(rivalParent.Find($"HoujuChara{m_lostScoreCharaCount + 1}"), isActive: true, smashedBeginCharaSpriteAsset.Asset, disposableAsset.Asset);
				}
				m_lostScoreCharaCount++;
			}
		}
		Transform transform = base.transform.Find("Cut/AgariText");
		Image image = transform.GetComponentInChildren<Image>();
		string hanTextSpriteAddress = ResolveHanText(m_hanTextType, m_sourcePlayer.PaymentAmount(), m_cutinPlayMode);
		bool flag = await AssetLoader.Exists(hanTextSpriteAddress);
		image.enabled = flag;
		if (flag)
		{
			IDisposableAsset<Sprite> disposableAsset2 = await AssetLoader.LoadAssetAsync<Sprite>(hanTextSpriteAddress, null, cancellationToken);
			disposableAsset2.AddTo(this);
			image.sprite = disposableAsset2.Asset;
		}
		if (_addText != null)
		{
			long num = Math.Max(8000L, MahjongUtility.GetScore(GameDataManager.Mahjong.TakuType, m_sourcePlayer.PaymentAmount(), 40, m_sourcePlayer.Kaze() == Kaze.Ton, isTsumo: true).totalPayment);
			_addText.text = (GameDataManager.Mahjong.IsAotenjoMode ? "9,999,999" : (num.ToString("N0") ?? ""));
		}
		for (int i = m_lostScoreCharaCount; i < 3; i++)
		{
			if (_isExpansionSmashedItems || isHoujuCharaMove)
			{
				Transform transform2 = _houjuCharas[i].transform;
				if (transform2 != null)
				{
					SmashedChara_SetActive(transform2, isActive: false);
				}
			}
			if (_isExpansionSmashedItems || !isHoujuCharaMove)
			{
				Transform transform3 = rivalParent.Find($"HoujuChara{i + 1}");
				if (transform3 != null)
				{
					SmashedChara_SetActive(transform3, isActive: false);
				}
			}
			try
			{
				List<SmashedUnableItems> smashedItems = _smashedItems;
				if (smashedItems == null || !(smashedItems[i]?.objs?.Count > 0))
				{
					continue;
				}
				foreach (GameObject obj2 in _smashedItems[i].objs)
				{
					obj2.SetActive(value: false);
				}
			}
			catch
			{
			}
		}
		if (m_sourcePlayer.IsCatharsisEffectActive())
		{
			IDisposableAsset<GameObject> disposableAsset3 = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/Comeback/Prefabs/BigComebackAgariEffect.prefab", null, cancellationToken);
			disposableAsset3.AddTo(this);
			m_openingEffect = UnityEngine.Object.Instantiate(disposableAsset3.Asset, base.transform);
			m_openingEffect.SetActive(value: false);
		}
		else if (m_sourcePlayer.IsMeldBonusEffectActive())
		{
			IDisposableAsset<GameObject> disposableAsset4 = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/MeldBonus/Prefab/BigMeldBonusAgariEffect.prefab", null, cancellationToken);
			disposableAsset4.AddTo(this);
			m_openingEffect = UnityEngine.Object.Instantiate(disposableAsset4.Asset, base.transform);
			m_openingEffect.SetActive(value: false);
		}
		else if (m_sourcePlayer.IsMentanpinHighEffectActive())
		{
			IDisposableAsset<GameObject> disposableAsset5 = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/Mentanpin/Prefabs/BigAgariHighEffect.prefab", null, cancellationToken);
			disposableAsset5.AddTo(this);
			m_openingEffect = UnityEngine.Object.Instantiate(disposableAsset5.Asset, base.transform);
			m_openingEffect.SetActive(value: false);
		}
		else if (m_sourcePlayer.IsMentanpinLowEffectActive())
		{
			IDisposableAsset<GameObject> disposableAsset6 = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/Mentanpin/Prefabs/BigAgariLowEffect.prefab", null, cancellationToken);
			disposableAsset6.AddTo(this);
			m_openingEffect = UnityEngine.Object.Instantiate(disposableAsset6.Asset, base.transform);
			m_openingEffect.SetActive(value: false);
		}
		else if (m_sourcePlayer.IsFireReachEffectActive())
		{
			IDisposableAsset<GameObject> disposableAsset7 = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/BigAgariFireEffect.prefab", null, cancellationToken);
			disposableAsset7.AddTo(this);
			m_openingEffect = UnityEngine.Object.Instantiate(disposableAsset7.Asset, base.transform);
			m_openingEffect.SetActive(value: false);
		}
		else if (m_sourcePlayer.IsOpenReachEffectActive())
		{
			IDisposableAsset<GameObject> disposableAsset8 = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/OpenReach/Prefabs/BigAgariEffect.prefab", null, cancellationToken);
			disposableAsset8.AddTo(this);
			m_openingEffect = UnityEngine.Object.Instantiate(disposableAsset8.Asset, base.transform);
			m_openingEffect.SetActive(value: false);
		}
		else if (m_sourcePlayer.IsBombEffectActive())
		{
			IDisposableAsset<GameObject> disposableAsset9 = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/Bomb/Prefab/BigBombAgariEffect.prefab", null, cancellationToken);
			disposableAsset9.AddTo(this);
			m_openingEffect = UnityEngine.Object.Instantiate(disposableAsset9.Asset, base.transform);
			m_openingEffect.SetActive(value: false);
		}
		else if (m_sourcePlayer.IsAllGreenEffectActive())
		{
			IDisposableAsset<GameObject> disposableAsset10 = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/AllGreen/Prefab/BigAllGreenAgariEffect.prefab", null, cancellationToken);
			disposableAsset10.AddTo(this);
			m_openingEffect = UnityEngine.Object.Instantiate(disposableAsset10.Asset, base.transform);
			m_openingEffect.SetActive(value: false);
		}
		else if (m_sourcePlayer.IsKirisameEffectActive())
		{
			IDisposableAsset<GameObject> disposableAsset11 = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/Kirisame/Prefab/BigKirisameAgariEffect.prefab", null, cancellationToken);
			disposableAsset11.AddTo(this);
			m_openingEffect = UnityEngine.Object.Instantiate(disposableAsset11.Asset, base.transform);
			m_openingEffect.SetActive(value: false);
		}
		else if (m_sourcePlayer.IsAtomicReachEffectActive())
		{
			IDisposableAsset<GameObject> disposableAsset12 = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/AtomicReach/Prefabs/BigAgariEffect.prefab", null, cancellationToken);
			disposableAsset12.AddTo(this);
			m_openingEffect = UnityEngine.Object.Instantiate(disposableAsset12.Asset, base.transform);
			m_openingEffect.SetActive(value: false);
		}
		else if (m_sourcePlayer.IsAccelDoraEffectActive())
		{
			IDisposableAsset<GameObject> disposableAsset13 = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/BigAgariAccelDoraEffect.prefab", null, cancellationToken);
			disposableAsset13.AddTo(this);
			m_openingEffect = UnityEngine.Object.Instantiate(disposableAsset13.Asset, base.transform);
			m_openingEffect.SetActive(value: false);
		}
		else if (m_sourcePlayer.IsChocolateReachEffectActive())
		{
			IDisposableAsset<GameObject> disposableAsset14 = await AssetLoader.LoadAssetAsync<GameObject>("Cutin/Chocolate/Prefabs/BigAgariChocolateEffect.prefab", null, cancellationToken);
			disposableAsset14.AddTo(this);
			m_openingEffect = UnityEngine.Object.Instantiate(disposableAsset14.Asset, base.transform);
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
		static string ResolveHanText(HanTextType hanTextType, Han han, CutinPlayMode mode)
		{
			if (GameDataManager.Mahjong.IsAotenjoMode && mode != CutinPlayMode.Preview)
			{
				if (han >= Han.Mangan)
				{
					return $"Cutin/HanText/{hanTextType}/Aotenjo.png";
				}
				return "";
			}
			if (han == Han.Nothing)
			{
				return "";
			}
			if (han > Han.Yakuman)
			{
				han = Han.Yakuman;
			}
			return $"Cutin/HanText/{hanTextType}/{(int)han}_{han}.png";
		}
	}

	private void SmashedChara_SetActive(Transform rivalBase, bool isActive, Sprite smashedCharaBegin = null, Sprite smashedCharaEnd = null)
	{
		rivalBase.gameObject.SetActive(isActive);
		Image component = rivalBase.GetComponent<Image>();
		component.enabled = isActive;
		if (smashedCharaBegin != null)
		{
			component.sprite = smashedCharaBegin;
		}
		component = rivalBase.Find("Change").GetComponent<Image>();
		component.enabled = isActive;
		if (smashedCharaEnd != null)
		{
			component.sprite = smashedCharaEnd;
		}
	}

	public override async UniTask Play(CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		m_isPlaying = true;
		m_isPlayingIntro = true;
		m_isPlayingYakumanIntro = true;
		_randomVoiceIndex = ((_randomSECount < 2) ? 1 : (m_randomValueForVoice % _randomSECount + 1));
		base.gameObject.SetActive(value: true);
		await Prepare(cancellationToken);
		if (_isCutinHandEnable)
		{
			GameObject gameObject = m_sourcePlayer.KnockdownTileParent();
			GetComponent<CutinEventNotifier>().m_cutinHandTilesParent = gameObject;
			Transform transform = gameObject.transform.Find("Player");
			if (transform != null)
			{
				GetComponentInChildren<CutinHaiController>().m_tileParent = transform.gameObject;
				m_sourcePlayer.CreateKnockdownTiles(transform.gameObject);
			}
		}
		Animator component = GetComponent<Animator>();
		component.SetTrigger("In");
		component.Update(0f);
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
		await UniTask.WaitWhile(() => CurrentAnimator_IsPlayingWhithState(new List<string> { "NewManganCutInEnd", "NewManganCutInLoop", "NewManganCutIn" }), PlayerLoopTiming.Update, cancellationToken);
		if (m_yakumanSubCutin != null)
		{
			CutinController yakumanSubCutinController = m_yakumanSubCutin.Instance.GetComponent<CutinController>();
			await UniTask.WaitWhile(() => yakumanSubCutinController.IsPlaying(), PlayerLoopTiming.Update, cancellationToken);
			m_yakumanSubCutin.Dispose();
		}
	}

	public override async UniTask End(CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		GetComponent<Animator>().SetTrigger("End");
		GetComponent<Animator>().Update(0f);
		m_yakumanSubCutin?.Instance.GetComponent<Animator>().SetTrigger("End");
		await UniTask.WaitUntil(() => CurrentAnimator_IsPlayingWhithState(new List<string> { "NewManganCutInWait" }), PlayerLoopTiming.Update, cancellationToken);
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
		string label = CreateRandomVoiceLabel(string.IsNullOrEmpty(_smashSELabel) ? "SFX_MJG_CALL3_SR_PUNCH1" : _smashSELabel, _randomVoiceIndex);
		SoundManager.PlaySe(isPlay, label).Forget();
		foreach (IPlayerInteractor player in m_players)
		{
			if (player.IsActive() && player.IsScorePaymentPlayer())
			{
				await SoundManager.PlayVoiceCha(isPlay, player.IsFakeClear() ? ("VOX_G12_KAGE_" + VoiceManager.FakeClear_ToLabel(player.CharaVariantID()) + "_KO") : "VOX_GXX_MJG_KO1", player.CharaType(), Cha.Toimen);
			}
		}
		await UniTask.Delay(1500);
		if (m_sourcePlayer.IsFakeClear())
		{
			SoundManager.PlayVoice(isPlay, "VOX_G12_KAGE_" + VoiceManager.FakeClear_ToLabel(m_sourcePlayer.CharaVariantID()) + "_AGARI", charaType).Forget();
			return;
		}
		VoiceManager.PlayVoiceAgariSR(isPlay, new Player
		{
			m_charaType = charaType,
			m_level = m_sourcePlayer.FavoLevel()
		});
	}

	public override async UniTask Stop(CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		base.gameObject.SetActive(value: false);
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

	public void NotifyPlayHantextSE()
	{
		if (AudioOutputEnable && !_hanTextSELabel.Equals("None"))
		{
			SoundManager.PlaySe(CreateRandomVoiceLabel(string.IsNullOrEmpty(_hanTextSELabel) ? "SFX_MJG_DISPLAY10" : _hanTextSELabel, _randomVoiceIndex)).Forget();
		}
	}

	private string CreateRandomVoiceLabel(string originalSELabel, int indexValue)
	{
		if (_randomSECount <= 0)
		{
			return originalSELabel;
		}
		return Regex.Replace(originalSELabel, "\\d$", indexValue.ToString());
	}
}
