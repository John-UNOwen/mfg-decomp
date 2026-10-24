using System;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.CutinItem;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class CardConnectWindow : MonoBehaviour
{
	private void Start()
	{
	}

	public async UniTask DispMemorialPrintNotice()
	{
		if (GameDataManager.Notice.MemorialPrintNoticeInfos.Count <= 0)
		{
			return;
		}
		try
		{
			SoundCall soundCall = GetComponent<SoundCall>();
			await UniTask.Yield();
			Animator animator = GetComponent<Animator>();
			CharaType charaType = CharaType.Chara01;
			foreach (MemorialPrintNoticeInfo memorialPrintNoticeInfo in GameDataManager.Notice.MemorialPrintNoticeInfos)
			{
				SoundManager.StopSe(SoundManager.GetCharaVoiceLabel("VOX_GXX_SYSTEM8", charaType)).Forget();
				Item itemInfo = CutinItemMaster.GetItem(memorialPrintNoticeInfo.ItemID);
				base.transform.Find("Notice").Find("NoticeBase").Find("CardNameText")
					.GetComponent<TextMeshProUGUI>()
					.text = itemInfo.Name;
				IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(itemInfo.MemorialPrintGraphicPath);
				disposableAsset.AddTo(this);
				base.transform.Find("Notice").Find("NoticeBase").Find("Card")
					.GetComponent<Image>()
					.sprite = disposableAsset.Asset;
				soundCall.MemorialPrintCharaType = itemInfo.CharaType;
				charaType = itemInfo.CharaType;
				base.gameObject.SetActive(value: true);
				animator.SetTrigger("Skip");
				await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(3.0)), UniTask.WaitUntil(() => ResidentFlow.CustomInput.GetMouseButtonDown(0) ? true : false));
				await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(7.0)), UniTask.WaitUntil(() => ResidentFlow.CustomInput.GetMouseButtonDown(0) ? true : false));
				animator.SetTrigger("Skip");
				await UniTask.Delay(TimeSpan.FromSeconds(0.5));
				base.gameObject.SetActive(value: false);
			}
		}
		catch (MissingReferenceException)
		{
		}
		base.gameObject.SetActive(value: false);
		GameDataManager.Notice.MemorialPrintNoticeInfos.Clear();
	}

	public async UniTask DispMissionMemorialPrintNotice()
	{
		if (GameDataManager.Notice.MissionMemorialPrintNoticeInfos.Count <= 0)
		{
			return;
		}
		try
		{
			SoundCall soundCall = GetComponent<SoundCall>();
			await UniTask.Yield();
			Animator animator = GetComponent<Animator>();
			CharaType charaType = CharaType.Chara01;
			foreach (MissionMemorialPrintNoticeInfo noticeInfo in GameDataManager.Notice.MissionMemorialPrintNoticeInfos)
			{
				SoundManager.StopSe(SoundManager.GetCharaVoiceLabel("VOX_GXX_SYSTEM8", charaType)).Forget();
				base.transform.Find("Notice").Find("NoticeBase").Find("CardNameText")
					.GetComponent<TextMeshProUGUI>()
					.text = noticeInfo.ItemName;
				IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(noticeInfo.GetAddressablePath);
				disposableAsset.AddTo(this);
				base.transform.Find("Notice").Find("NoticeBase").Find("Card")
					.GetComponent<Image>()
					.sprite = disposableAsset.Asset;
				soundCall.MemorialPrintCharaType = noticeInfo.Chara;
				charaType = noticeInfo.Chara;
				base.gameObject.SetActive(value: true);
				animator.SetTrigger("Skip");
				await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(3.0)), UniTask.WaitUntil(() => ResidentFlow.CustomInput.GetMouseButtonDown(0) ? true : false));
				await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(7.0)), UniTask.WaitUntil(() => ResidentFlow.CustomInput.GetMouseButtonDown(0) ? true : false));
				animator.SetTrigger("Skip");
				await UniTask.Delay(TimeSpan.FromSeconds(0.5));
				base.gameObject.SetActive(value: false);
			}
		}
		catch (MissingReferenceException)
		{
		}
		base.gameObject.SetActive(value: false);
		GameDataManager.Notice.MissionMemorialPrintNoticeInfos.Clear();
	}
}
