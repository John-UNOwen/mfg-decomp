using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace MFG.MahjongPlay.Cutin;

public class YakumanSubCutin : CutinController
{
	private CutinMovie m_cutinMovie;

	private bool m_isPlaying;

	private bool m_isPlayingIntro;

	private void Awake()
	{
		m_cutinMovie = GetComponent<CutinMovie>();
		ObservableStateMachineTrigger behaviour = GetComponent<Animator>().GetBehaviour<ObservableStateMachineTrigger>();
		behaviour.OnStateEnterAsObservable().Subscribe(delegate
		{
		}).AddTo(this);
		behaviour.OnStateExitAsObservable().Subscribe(delegate(ObservableStateMachineTrigger.OnStateInfo onStateInfo)
		{
			if (onStateInfo.StateInfo.IsName("YakumanIn"))
			{
				m_isPlayingIntro = false;
			}
		}).AddTo(this);
	}

	public override async UniTask Prepare(CancellationToken cancellationToken = default(CancellationToken))
	{
	}

	public override async UniTask Play(CancellationToken cancellationToken = default(CancellationToken))
	{
		m_isPlaying = true;
		m_isPlayingIntro = true;
		GetComponent<Animator>().SetTrigger("In");
		GetComponent<Animator>().Update(0f);
		PlaySound(isPlay: true).Forget();
		await UniTask.WaitWhile(() => CurrentAnimator_IsPlayingWhithState(new List<string> { "YakumanIn", "YakumanLoop", "YakumanEnd" }), PlayerLoopTiming.Update, cancellationToken);
		m_isPlaying = false;
		m_isPlayingIntro = false;
	}

	public override async UniTask End(CancellationToken cancellationToken = default(CancellationToken))
	{
		GetComponent<Animator>().SetTrigger("End");
		GetComponent<Animator>().Update(0f);
		await UniTask.WaitUntil(() => CurrentAnimator_IsPlayingWhithState(new List<string> { "YakumanWait" }), PlayerLoopTiming.Update, cancellationToken);
	}

	public override async UniTask Stop(CancellationToken cancellationToken = default(CancellationToken))
	{
		m_cutinMovie.Stop();
		PlaySound(isPlay: false, cancellationToken).Forget();
		m_isPlaying = false;
		m_isPlayingIntro = false;
	}

	protected override async UniTask PlaySound(bool isPlay, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		_ = AudioOutputEnable;
	}

	public override bool IsPlaying()
	{
		return m_isPlaying;
	}

	public override bool IsPlayingIntro()
	{
		return m_isPlayingIntro;
	}

	public override bool IsPrepared()
	{
		return m_cutinMovie.IsPrepared;
	}
}
