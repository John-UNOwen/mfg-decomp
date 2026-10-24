using System;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MissionEventAcquireWindow : MonoBehaviour
{
	[SerializeField]
	private GameObject FrameParent;

	private void Start()
	{
	}

	public async UniTask Display()
	{
		if (GameDataManager.Notice.MusicAcquireInfos.Count <= 0)
		{
			return;
		}
		try
		{
			GetComponent<SoundCall>();
			base.gameObject.SetActive(value: true);
			await UniTask.Yield();
			Animator animator = GetComponent<Animator>();
			foreach (MusicAcquireInfo info in GameDataManager.Notice.MusicAcquireInfos)
			{
				Transform acquireObject = FrameParent.transform.Find($"Frame{info.Master.Category}");
				acquireObject.gameObject.SetActive(value: true);
				TextMeshProUGUI component = acquireObject.Find("NameText").GetComponent<TextMeshProUGUI>();
				TextMeshProUGUI component2 = acquireObject.Find("Category/CategoryText").GetComponent<TextMeshProUGUI>();
				component.text = info.GetItemName().name;
				component2.text = info.GetItemName().category;
				IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(info.GetImagePath());
				disposableAsset.AddTo(this);
				base.transform.Find("Notice").Find("NoticeBase").Find("ItemImage")
					.GetComponent<Image>()
					.sprite = disposableAsset.Asset;
				string label = info.GetMusicLabel();
				animator.SetTrigger("In");
				SoundManager.PlaySe(label).Forget();
				await UniTask.Delay(TimeSpan.FromSeconds(0.6166666746139526));
				await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(9.383333206176758)), UniTask.WaitUntil(() => ResidentFlow.CustomInput.GetMouseButtonDown(0) ? true : false));
				animator.SetTrigger("Out");
				SoundManager.StopSe(label).Forget();
				SoundManager.PlayBgm("BGM_RESULT3").Forget();
				await UniTask.Delay(TimeSpan.FromSeconds(0.5));
				acquireObject.gameObject.SetActive(value: false);
			}
		}
		catch (MissingReferenceException)
		{
		}
		base.gameObject.SetActive(value: false);
		GameDataManager.Notice.MusicAcquireInfos.Clear();
	}
}
