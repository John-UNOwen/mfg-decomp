using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.CutinItem;
using UnityEngine;

namespace MFG.MahjongPlay.Cutin;

public class NakiCutin : CutinController
{
	public override async UniTask Prepare(CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
	}

	public override async UniTask Play(CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		base.gameObject.SetActive(value: true);
		switch (m_cutinPlayMode)
		{
		case CutinPlayMode.Normal:
			GetComponent<Animator>().SetTrigger("In");
			break;
		case CutinPlayMode.Extra:
			GetComponent<Animator>().SetTrigger("Extra");
			break;
		default:
			GetComponent<Animator>().SetTrigger("Gacha");
			break;
		}
		PlaySound(isPlay: true).Forget();
		await UniTask.WaitWhile(() => IsPlaying(), PlayerLoopTiming.Update, cancellationToken);
	}

	public override async UniTask End(CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		GetComponent<Animator>().SetTrigger("End");
	}

	public override async UniTask Stop(CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
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
			switch (m_cutinType)
			{
			case CutinType.Pon:
				SoundManager.PlaySe(isPlay, "SFX_MJG_CALL1_" + text).Forget();
				SoundManager.PlayVoiceCha(isPlay, "VOX_GXX_MJG_PONG", charaType, cha).Forget();
				break;
			case CutinType.Chi:
				SoundManager.PlaySe(isPlay, "SFX_MJG_CALL1_" + text).Forget();
				SoundManager.PlayVoiceCha(isPlay, "VOX_GXX_MJG_CHOW", charaType, cha).Forget();
				break;
			case CutinType.Kan:
				SoundManager.PlaySe(isPlay, "SFX_MJG_CALL1_" + text).Forget();
				SoundManager.PlayVoiceCha(isPlay, "VOX_GXX_MJG_KONG", charaType, cha).Forget();
				break;
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
