using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.CutinItem;
using UniRx;
using UnityEngine;

namespace MFG.MahjongPlay.Cutin;

public class ReachCutin : CutinController
{
	private Subject<IPlayerInteractor> m_onReachStickThrow = new Subject<IPlayerInteractor>();

	private Subject<IPlayerInteractor> m_onPlayBgmRequest = new Subject<IPlayerInteractor>();

	public IObservable<IPlayerInteractor> OnReachStickThrow => m_onReachStickThrow.AsObservable();

	public IObservable<IPlayerInteractor> OnPlayBgmRequest => m_onPlayBgmRequest.AsObservable();

	public override async UniTask Prepare(CancellationToken cancellationToken = default(CancellationToken))
	{
	}

	public override async UniTask Play(CancellationToken cancellationToken = default(CancellationToken))
	{
		base.gameObject.SetActive(value: true);
		switch (m_cutinPlayMode)
		{
		case CutinPlayMode.Normal:
			if (m_sourcePlayer.ReachLevel() >= ReachLevel.FireReach)
			{
				GetComponent<Animator>().SetTrigger("Flame");
			}
			else if (m_sourcePlayer.ReachLevel() == ReachLevel.AtomicReach)
			{
				GetComponent<Animator>().SetTrigger("Atomic");
			}
			else if (m_sourcePlayer.ReachLevel() == ReachLevel.OpenReach)
			{
				GetComponent<Animator>().SetTrigger("Open");
			}
			else if (m_sourcePlayer.IsChocolateReachEffectActive())
			{
				GetComponent<Animator>().SetTrigger("Valentine");
			}
			else
			{
				GetComponent<Animator>().SetTrigger("In");
			}
			break;
		default:
			GetComponent<Animator>().SetTrigger("Gacha");
			break;
		}
		m_onReachStickThrow.OnNext(m_sourcePlayer);
		PlaySound(isPlay: true).Forget();
		await UniTask.WaitWhile(() => IsPlaying(), PlayerLoopTiming.Update, cancellationToken);
	}

	public override async UniTask End(CancellationToken cancellationToken = default(CancellationToken))
	{
		GetComponent<Animator>().SetTrigger("End");
	}

	public override async UniTask Stop(CancellationToken cancellationToken = default(CancellationToken))
	{
		base.gameObject.SetActive(value: false);
		PlaySound(isPlay: false).Forget();
	}

	protected override async UniTask PlaySound(bool isPlay, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (AudioOutputEnable)
		{
			Item item = CutinItemMaster.GetItem(m_itemID);
			string text = RarityToSoundLabel(item.Rarity);
			CharaType charaType = item.CharaType;
			Cha cha = m_sourcePlayer.Cha();
			SoundManager.PlaySe(isPlay, "SFX_MJG_CALL2_" + text).Forget();
			VoiceManager.PlayVoiceInReachCutin(isPlay, m_sourcePlayer.ReachType(), charaType, cha, m_sourcePlayer.GachaVoices());
			if (m_cutinPlayMode == CutinPlayMode.Normal)
			{
				SoundManager.StopBgmWithFadeout().Forget();
				await UniTask.Delay(1500, ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
				m_onPlayBgmRequest.OnNext(m_sourcePlayer);
			}
		}
		static string RarityToSoundLabel(Rarity _rarity)
		{
			return _rarity switch
			{
				Rarity.R => "R", 
				Rarity.SR => "SR", 
				Rarity.UR => "UR", 
				_ => "N", 
			};
		}
	}

	public override bool IsPlaying()
	{
		return GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime < 1f;
	}

	public override bool IsPlayingIntro()
	{
		return IsPlaying();
	}

	public override bool IsPrepared()
	{
		return true;
	}
}
