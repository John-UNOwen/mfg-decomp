using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.MasterGameData;
using MFG.Types;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class GachaCharaGetAnimation : MonoBehaviour
{
	[SerializeField]
	private Image m_charaImage;

	[SerializeField]
	private Image m_charaName;

	[SerializeField]
	private Image m_charaText;

	public async UniTask GachaGetCharaNotice(CancellationToken cancellationToken)
	{
		List<CharaType> infoNewGirlNoticeFromGacha = GameDataManager.Notice.InfoNewGirlNoticeFromGacha;
		if (infoNewGirlNoticeFromGacha == null || infoNewGirlNoticeFromGacha.Count <= 0)
		{
			return;
		}
		foreach (CharaType item in infoNewGirlNoticeFromGacha)
		{
			switch (item)
			{
			case CharaType.Chara12:
				if (!GameDataManager.PlayerConfig.IsChara12GetNoticeFinish)
				{
					await CharaGetNotice(item, cancellationToken);
					GameDataManager.PlayerConfig.IsChara12GetNoticeFinish = true;
				}
				break;
			case CharaType.Chara13:
				if (!GameDataManager.PlayerConfig.IsChara13GetNoticeFinish)
				{
					await CharaGetNotice(item, cancellationToken);
					GameDataManager.PlayerConfig.IsChara13GetNoticeFinish = true;
				}
				break;
			case CharaType.Chara14:
				if (!GameDataManager.PlayerConfig.IsChara14GetNoticeFinish)
				{
					await CharaGetNotice(item, cancellationToken);
					GameDataManager.PlayerConfig.IsChara14GetNoticeFinish = true;
				}
				break;
			case CharaType.Chara15:
				if (!GameDataManager.PlayerConfig.IsChara15GetNoticeFinish)
				{
					await CharaGetNotice(item, cancellationToken);
					GameDataManager.PlayerConfig.IsChara15GetNoticeFinish = true;
				}
				break;
			case CharaType.Chara16:
				if (!GameDataManager.PlayerConfig.IsChara16GetNoticeFinish)
				{
					await CharaGetNotice(item, cancellationToken);
					GameDataManager.PlayerConfig.IsChara16GetNoticeFinish = true;
				}
				break;
			case CharaType.Chara17:
				if (!GameDataManager.PlayerConfig.IsChara17GetNoticeFinish)
				{
					await CharaGetNotice(item, cancellationToken);
					GameDataManager.PlayerConfig.IsChara17GetNoticeFinish = true;
				}
				break;
			case CharaType.Chara18:
				if (!GameDataManager.PlayerConfig.IsChara18GetNoticeFinish)
				{
					await CharaGetNotice(item, cancellationToken);
					GameDataManager.PlayerConfig.IsChara18GetNoticeFinish = true;
				}
				break;
			case CharaType.Chara19:
				if (!GameDataManager.PlayerConfig.IsChara19GetNoticeFinish)
				{
					await CharaGetNotice(item, cancellationToken);
					GameDataManager.PlayerConfig.IsChara19GetNoticeFinish = true;
				}
				break;
			case CharaType.Chara20:
				if (!GameDataManager.PlayerConfig.IsChara20GetNoticeFinish)
				{
					await CharaGetNotice(item, cancellationToken);
					GameDataManager.PlayerConfig.IsChara20GetNoticeFinish = true;
				}
				break;
			case CharaType.Chara21:
				if (!GameDataManager.PlayerConfig.IsChara21GetNoticeFinish)
				{
					await CharaGetNotice(item, cancellationToken);
					GameDataManager.PlayerConfig.IsChara21GetNoticeFinish = true;
				}
				break;
			}
		}
		GameDataManager.Notice.InfoNewGirlNoticeFromGacha.Clear();
	}

	private async UniTask SetSprite(Image image, string path)
	{
		bool flag = !string.IsNullOrEmpty(path);
		if (flag)
		{
			flag = await AssetLoader.Exists(path);
		}
		if (flag)
		{
			IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(path);
			disposableAsset.AddTo(this);
			image.sprite = disposableAsset.Asset;
		}
	}

	private async UniTask CharaGetNotice(CharaType chara, CancellationToken cancellationToken)
	{
		await SetSprite(m_charaImage, AssetPathResolver.GachaSelect.ResolveGetCharaImagePath(chara));
		await SetSprite(m_charaName, AssetPathResolver.GachaSelect.ResolveGetCharaNamePath(chara));
		await SetSprite(m_charaText, AssetPathResolver.GachaSelect.ResolveGetCharaTextPath(chara));
		CharacterProperties characterProperties = MasterGameDataManager.Character.Properties(chara);
		base.transform.Find("Bg/CharaGetBg").GetComponent<Image>().color = characterProperties.GachaGetDisplayColor;
		base.gameObject.SetActive(value: true);
		GetComponent<Animator>().SetTrigger("In");
		SoundManager.PlaySe("SFX_SYS_BONUS5").Forget();
		await UniTask.WaitUntil(() => GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("CharaGetAnimeLoop"), PlayerLoopTiming.Update, cancellationToken);
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(3.5), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResultFlow.Skip(), PlayerLoopTiming.Update, cancellationToken));
		GetComponent<Animator>().SetTrigger("End");
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(0.5), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResultFlow.Skip(), PlayerLoopTiming.Update, cancellationToken));
		base.gameObject.SetActive(value: false);
	}
}
