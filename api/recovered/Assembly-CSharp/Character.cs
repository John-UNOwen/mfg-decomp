using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.MahjongPlay.Character;
using MFG.MahjongPlay.Character.Arm;
using MFG.MahjongPlay.Cutin;
using MFG.MasterGameData;
using MFG.Types;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class Character : MonoBehaviour
{
	public enum EventID
	{
		TsumoAgari1,
		TsumoAgari2,
		RonAgari1,
		RonAgari2,
		Reach,
		DReach,
		Pon,
		Chi,
		Kan,
		Tenpai,
		Noten,
		Tyoko1,
		Tyoko2,
		Tyoko3,
		Tsumogiri,
		Furikomi,
		Skip,
		DoraDiscard,
		DoraTsumo,
		TaikyokuStart,
		Tsumo1,
		Tsumo2,
		Discard,
		Panipi,
		Result1,
		Result2,
		Result3,
		Result4,
		ReceiveAgari,
		ReceiveReach,
		ReceiveTyoko,
		EyeContact
	}

	public enum BodyPartsID
	{
		UpperBody,
		LowerBody
	}

	public enum Motion
	{
		None,
		Tsumo,
		Sutehai,
		Tyoko
	}

	public enum MotionState
	{
		Trigger,
		Wait,
		Pickup,
		Think,
		Away
	}

	[SerializeField]
	public Player player;

	[SerializeField]
	public GameObject info;

	[SerializeField]
	public CutinManager m_cutinManager;

	[SerializeField]
	public GameObject m_charaBottomBody;

	[SerializeField]
	public GameObject m_charaTopBody;

	private GameObject m_reachAura;

	[SerializeField]
	public GameObject m_reachAuraParent;

	[SerializeField]
	public Camera m_discardedTileCamera;

	[SerializeField]
	public Camera m_meldedTileCamera;

	[SerializeField]
	public GameObject m_test3DModel;

	[SerializeField]
	public Camera m_scorePanelCamera;

	[SerializeField]
	public Camera m_subMonitorScoreCamera;

	public CharaReaction m_charaReaction = new CharaReaction();

	[SerializeField]
	public List<Player> m_players = new List<Player>();

	public CharacterModelController m_characterModelController = new CharacterModelController();

	public CharacterArmController m_characterArmController = new CharacterArmController();

	[SerializeField]
	private AsyncSubject<bool> onInitialized = new AsyncSubject<bool>();

	public IObservable<bool> OnInitialized => onInitialized.AsObservable();

	public async UniTask Initialize(GameObject armParent, GameObject parent3D, Vector3 discardedTileRenderTextureDiff, Vector3 meldedTileRenderTextureDiff, MotionID initialMotionID = MotionID.Wait0, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		m_charaReaction.Init(this);
		_ = player.m_charaType;
		await m_characterModelController.Initialize(player, parent3D, initialMotionID, cancellationToken);
		await m_characterArmController.Initialize(player, armParent, m_discardedTileCamera, m_meldedTileCamera, discardedTileRenderTextureDiff, meldedTileRenderTextureDiff, cancellationToken);
		if (player.m_charaVariantID > 0)
		{
			int prtsIndex = player.m_charaVariantID - 1;
			m_characterModelController.Parts_SetExclusiveDisp(prtsIndex);
		}
		else
		{
			m_characterModelController.Parts_SetDispAllOff();
		}
		await ReachAura_Create(cancellationToken);
		ReachAura_SetDisp(isDisp: false);
	}

	public void SetOffset(Vector3 positionOffset, Vector3 rotationOffset)
	{
		m_characterModelController.SetOffset(positionOffset, rotationOffset);
	}

	private void OnDestroy()
	{
		m_characterModelController.Dispose();
		m_characterArmController.Dispose();
	}

	public async UniTask KyokuInit()
	{
		Action_Reset();
	}

	public void ResultAction(EventID id)
	{
		ResultMotion scorePanelResultMotion = MasterGameDataManager.Character.Properties(player.m_charaType).ScorePanelResultMotion;
		switch (id)
		{
		case EventID.Result1:
			m_characterModelController.PlayMotion(scorePanelResultMotion.MotionWin).Forget();
			break;
		case EventID.Result2:
			m_characterModelController.PlayMotion(scorePanelResultMotion.MotionLose).Forget();
			break;
		case EventID.Result3:
			m_characterModelController.PlayMotion(scorePanelResultMotion.MotionNormal).Forget();
			break;
		default:
			SetCharaMotion(id).Forget();
			break;
		}
		ReachAura_SetDisp(isDisp: false);
	}

	public async UniTask AgariAction(EventID id, Han han = Han.Nothing, int doraCount = 0, int meldBonusHan = 0, int comebackBonusHan = 0, int allgreenBonusHan = 0, List<Yaku> yakus = null, Player furikomiPlayer = null, int akadoraCount = 0, Cha bombCha = (Cha)(-1), Pai pai = Pai.NONE, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		AgariMotion agariMotion = MasterGameDataManager.Character.Properties(player.m_charaType).AgariMotion;
		switch (id)
		{
		case EventID.Result1:
			m_characterModelController.PlayMotion(agariMotion.Agari).Forget();
			break;
		case EventID.Result2:
			m_characterModelController.PlayMotion(agariMotion.Furikomi).Forget();
			break;
		default:
			SetCharaMotion(id).Forget();
			break;
		}
		await Action_Cutin(id, han, doraCount, meldBonusHan, comebackBonusHan, allgreenBonusHan, yakus, furikomiPlayer, akadoraCount, null, bombCha, pai, cancellationToken);
		Action_ReachAura(id);
	}

	public async UniTask Action(EventID id, Han han = Han.Nothing, int doraCount = 0, int meldBonusHan = 0, int comebackBonusHan = 0, int allgreenBonusHan = 0, List<Yaku> yakus = null, int akadoraCount = 0, Player furikomiPlayer = null, MahjongMusicTextDisplay musicText = null, Cha bombCha = (Cha)(-1), Pai pai = Pai.NONE, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		SetCharaMotion(id).Forget();
		await Action_Cutin(id, han, doraCount, meldBonusHan, comebackBonusHan, allgreenBonusHan, yakus, furikomiPlayer, akadoraCount, musicText, bombCha, pai, cancellationToken);
		Action_ReachAura(id);
	}

	public void Action_PlayArmMotion(CharacterArmController.MotionID motionID, Vector3 discardTilePos)
	{
		m_characterArmController.PlayMotion(motionID, discardTilePos);
	}

	private void Action_Reset()
	{
		ReachAura_SetDisp(isDisp: false);
	}

	private async UniTask ReachAura_Create(CancellationToken cancellationToken)
	{
		IDisposableAsset<GameObject> disposableAsset = await AssetLoader.LoadAssetAsync<GameObject>("MahjongPlayAura/Prefabs/CharaAura.prefab", null, cancellationToken);
		disposableAsset.AddTo(this);
		m_reachAura = UnityEngine.Object.Instantiate(disposableAsset.Asset, m_reachAuraParent.transform, worldPositionStays: false);
		if (player.IsJicha())
		{
			m_reachAura.GetComponent<Image>().material = null;
		}
	}

	public void ReachAura_SetDisp(bool isDisp)
	{
		Color color = ((player.m_isFireReach || player.m_isOpenReach || player.m_isAtomicReach) ? new Color32(221, 53, 47, byte.MaxValue) : new Color32(47, 155, 221, byte.MaxValue));
		try
		{
			if (((IPlayerInteractor)m_cutinManager.PlayerInteractors.Find((PlayerInteractor x) => x.Cha() == player.m_cha)).IsOpenReachEffectActive())
			{
				color = new Color(1f, 0.9568f, 0.3098f);
			}
			else if (((IPlayerInteractor)m_cutinManager.PlayerInteractors.Find((PlayerInteractor x) => x.Cha() == player.m_cha)).IsAtomicReachEffectActive())
			{
				color = new Color32(244, 133, 152, byte.MaxValue);
			}
			else if (((IPlayerInteractor)m_cutinManager.PlayerInteractors.Find((PlayerInteractor x) => x.Cha() == player.m_cha)).IsChocolateReachEffectActive())
			{
				color = new Color(0.8773f, 0.4924f, 0.5787f);
			}
		}
		catch
		{
		}
		ReachAura_SetColor(color);
		m_reachAura.SetActive(isDisp);
		if (SystemConfigGameData.IsDualMonitor)
		{
			SubMonitorFlow.GetInstance().ReachAuraSetActive(player.m_cha, isDisp, color);
		}
	}

	private void ReachAura_SetColor(Color color)
	{
		m_reachAura.GetComponent<Image>().color = color;
	}

	private void Action_ReachAura(EventID id)
	{
		switch (id)
		{
		case EventID.Reach:
		case EventID.DReach:
			ReachAura_SetDisp(isDisp: true);
			break;
		case EventID.Result1:
		case EventID.Result2:
		case EventID.Result3:
		case EventID.Result4:
			ReachAura_SetDisp(isDisp: false);
			break;
		}
	}

	public async UniTask Action_Cutin(EventID id, Han han, int doraCount, int meldBonusHan, int comebackBonusHan, int allgreenBonusHan, List<Yaku> yakus, Player furikomiPlayer, int akadoraCount, MahjongMusicTextDisplay musicText = null, Cha bombCha = (Cha)(-1), Pai pai = Pai.NONE, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		Cha cha = player.m_cha;
		switch (id)
		{
		case EventID.Pon:
			await m_cutinManager.PlayPon(cha, cancellationToken);
			break;
		case EventID.Chi:
			await m_cutinManager.PlayChi(cha, cancellationToken);
			break;
		case EventID.Kan:
			await m_cutinManager.PlayKan(cha, cancellationToken);
			break;
		case EventID.Reach:
		case EventID.DReach:
			await m_cutinManager.PlayReach(cha, musicText, pai, cancellationToken);
			break;
		case EventID.TsumoAgari1:
		case EventID.TsumoAgari2:
			await m_cutinManager.PlayTsumo(cha, han, doraCount, meldBonusHan, comebackBonusHan, allgreenBonusHan, yakus, akadoraCount, cha == bombCha, cancellationToken);
			break;
		case EventID.RonAgari1:
		case EventID.RonAgari2:
			await m_cutinManager.PlayRon(cha, furikomiPlayer.m_cha, han, doraCount, meldBonusHan, comebackBonusHan, allgreenBonusHan, yakus, akadoraCount, bombCha, furikomiPlayer.m_isCatharsisKing, cancellationToken);
			break;
		}
	}

	public bool IsTsumohaiTiming()
	{
		if (m_characterModelController.IsTsumohaiTiming())
		{
			return true;
		}
		return false;
	}

	public void ResetTsumohaiTiming()
	{
		m_characterModelController.ResetTsumohaiTiming();
	}

	public bool IsSutehaiTiming()
	{
		if (m_characterModelController.IsSutehaiTiming())
		{
			return true;
		}
		return false;
	}

	public void ResetSutehaiTiming()
	{
		m_characterModelController.ResetSutehaiTiming();
	}

	public async UniTask SetCharaMotion(EventID id)
	{
		await m_characterModelController.PlayMotion(id);
	}
}
