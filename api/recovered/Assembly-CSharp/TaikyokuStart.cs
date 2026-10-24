using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.GameData;
using MFG.Types;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class TaikyokuStart : MonoBehaviour
{
	[SerializeField]
	private MahjongPlayFlow m_mahjongPlayFlow;

	[SerializeField]
	private GameObject m_topTitleInstanceRoot;

	public async UniTask TaikyokuStartEffectPlay(CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		bool isProEffectPlayed = m_mahjongPlayFlow.Players.Exists((Player a) => a.IsProAnimEnable());
		bool isScrambleMatch = GameDataManager.Taikyoku.IsScrambleMatch;
		bool isMotherWars = GameDataManager.Taikyoku.IsMotherWars;
		await SplashLogo_Play(!isProEffectPlayed && !isScrambleMatch && !isMotherWars, cancellationToken);
		foreach (Player player in m_mahjongPlayFlow.Players)
		{
			if (player.IsProAnimEnable())
			{
				await ProStandInit(player.m_charaType, player.m_cha, cancellationToken);
			}
		}
		if (isProEffectPlayed)
		{
			await UniTask.Delay(1000, ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
		}
	}

	public async UniTask TaikyokuEndEffectPlay(CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		bool isProEffectPlayed = m_mahjongPlayFlow.Players.Exists((Player a) => a.IsProAnimEnable());
		foreach (Player player in m_mahjongPlayFlow.Players)
		{
			if (player.IsProAnimEnable())
			{
				await ProTaikyokuEndGreet(player.m_charaType, player.m_cha, cancellationToken);
			}
		}
		if (isProEffectPlayed)
		{
			await UniTask.Delay(1000, ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
		}
	}

	private async UniTask SplashLogo_Play(bool skipEnable = true, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		IDisposableAsset<GameObject> disposableAsset = await AssetLoader.LoadAssetAsync<GameObject>(SplashLogo_ResolveAddress());
		disposableAsset.AddTo(this);
		GameObject logoInstance = UnityEngine.Object.Instantiate(disposableAsset.Asset, m_topTitleInstanceRoot.transform, worldPositionStays: false);
		if (!(logoInstance == null))
		{
			bool isScrambleMatchTrigger = GameDataManager.Taikyoku.IsScrambleMatch;
			bool isMotherWarsTrigger = GameDataManager.Taikyoku.IsMotherWars;
			logoInstance.SetActive(value: true);
			logoInstance.GetComponent<Animator>().SetTrigger(isMotherWarsTrigger ? "MotherWarsIn" : (isScrambleMatchTrigger ? "ScrambleIn" : "TriggerIn"));
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds((isScrambleMatchTrigger || isMotherWarsTrigger) ? 4.7f : 3f), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => skipEnable && ResidentFlow.CustomInput.GetMouseButtonDown(0), PlayerLoopTiming.Update, cancellationToken));
			logoInstance.GetComponent<Animator>().SetTrigger(isMotherWarsTrigger ? "MotherWarsOut" : (isScrambleMatchTrigger ? "ScrambleOut" : "TriggerOut"));
		}
	}

	private string SplashLogo_ResolveAddress()
	{
		if (GameDataManager.EventTaku.ActiveAtTaikyokuGameDatas().Any())
		{
			return GameDataManager.EventTaku.ActiveAtTaikyokuGameDatas().First().Master.TopTitlePrefabPath;
		}
		if (GameDataManager.Taikyoku.IsValidCompetition)
		{
			CompetitionInfo inProgressEntriedCompetitionInfo = GameDataManager.Competition.GetInProgressEntriedCompetitionInfo();
			if (inProgressEntriedCompetitionInfo != null)
			{
				return AssetPathResolver.ResolveCompetitionAssets(inProgressEntriedCompetitionInfo.CompetitionID, CompetitionAssetType.MahjongPlayCaptionPrefab);
			}
		}
		return "MahjongPlayTopTitle/Prefabs/TopTitle.prefab";
	}

	private async UniTask ProStandInit(CharaType charaType, Cha cha, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		ProAssetType[] array = new ProAssetType[4]
		{
			ProAssetType.StandToi,
			ProAssetType.StandShimo,
			ProAssetType.StandToi,
			ProAssetType.StandKami
		};
		GameObject proStand = m_mahjongPlayFlow.m_proStands[(int)cha];
		proStand.SetActive(value: true);
		IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.Resolve(PathType.Pro, charaType, array[(int)cha]), null, cancellationToken);
		disposableAsset.AddTo(this);
		proStand.transform.Find("Pro").GetComponent<Image>().sprite = disposableAsset.Asset;
		IDisposableAsset<Sprite> disposableAsset2 = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.Resolve(PathType.Pro, charaType, ProAssetType.Stand), null, cancellationToken);
		disposableAsset2.AddTo(this);
		proStand.transform.Find("ProGreet").GetComponent<Image>().sprite = disposableAsset2.Asset;
		proStand.GetComponent<Animator>().SetTrigger("In");
		await UniTask.WaitWhile(() => proStand.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime < 1f, PlayerLoopTiming.Update, cancellationToken);
		await SoundManager.PlayVoice("VOX_GXX_PRO_GREET", charaType);
		proStand.GetComponent<Animator>().SetTrigger("Greet");
		await UniTask.WaitWhile(() => proStand.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime < 1f, PlayerLoopTiming.Update, cancellationToken);
		if (cha == Cha.Jicha)
		{
			proStand.GetComponent<Animator>().SetTrigger("Loop");
		}
		else
		{
			proStand.GetComponent<Animator>().SetTrigger("Out");
			await UniTask.WaitWhile(() => proStand.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime < 1f, PlayerLoopTiming.Update, cancellationToken);
		}
		await UniTask.WaitWhile(() => SoundManager.IsPlayLastCalledVoice(), PlayerLoopTiming.Update, cancellationToken);
	}

	private async UniTask ProTaikyokuEndGreet(CharaType charaType, Cha cha, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		GameObject proStand = m_mahjongPlayFlow.m_proStands[(int)cha];
		proStand.SetActive(value: true);
		if (cha != 0)
		{
			proStand.GetComponent<Animator>().SetTrigger("In");
			await UniTask.WaitWhile(() => proStand.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime < 1f, PlayerLoopTiming.Update, cancellationToken);
		}
		await SoundManager.PlayVoice("VOX_GXX_PRO_THANK", charaType);
		proStand.GetComponent<Animator>().SetTrigger("Greet");
		await UniTask.WaitWhile(() => proStand.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime < 1f, PlayerLoopTiming.Update, cancellationToken);
		proStand.GetComponent<Animator>().SetTrigger("Loop");
		await UniTask.WaitWhile(() => SoundManager.IsPlayLastCalledVoice(), PlayerLoopTiming.Update, cancellationToken);
	}
}
