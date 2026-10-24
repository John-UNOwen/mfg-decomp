using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.GameData;
using MFG.Types.YellCampaign;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class YellResult : MonoBehaviour
{
	[SerializeField]
	private GameObject windowObj;

	[SerializeField]
	private GameObject scoreObj;

	[SerializeField]
	private GameObject rankObj;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private Image charaImage;

	[SerializeField]
	private TextMeshProUGUI addYellNumText;

	[SerializeField]
	private Image nowYellRank;

	[SerializeField]
	private TextMeshProUGUI yellNumText;

	[SerializeField]
	private Image beforeYellRank;

	[SerializeField]
	private Image afterYellRank;

	public bool isOpen;

	public bool isRankUp;

	private int afterYellNum;

	private int beforeYellNum;

	private int addYellNum;

	private async void Start()
	{
		YellInfo info = GameDataManager.YellCampaign.GetCurrentCampaignInfo().GetCurrentCharaYellInfo();
		IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.Resolve(PathType.StandingPose, info.Chara, StandingPoseAssetType.SpriteLaugh));
		disposableAsset.AddTo(charaImage);
		charaImage.sprite = disposableAsset.Asset;
		beforeYellNum = info.YellCount;
		yellNumText.text = beforeYellNum.ToString();
		YellRank beforeRank = info.GetYellRank();
		string path2 = AssetPathResolver.ResolveYellRankImage(beforeRank);
		if (await AssetLoader.Exists(path2))
		{
			IDisposableAsset<Sprite> disposableAsset2 = await AssetLoader.LoadAssetAsync<Sprite>(path2);
			disposableAsset2.AddTo(this);
			nowYellRank.GetComponent<Image>().sprite = disposableAsset2.Asset;
			beforeYellRank.GetComponent<Image>().sprite = disposableAsset2.Asset;
		}
		addYellNum = GameDataManager.YellCampaign.GetCurrentCampaignInfo().ThisPlayGetYell;
		addYellNumText.text = addYellNum.ToString();
		GameDataManager.YellCampaign.AddYellToChara();
		afterYellNum = info.YellCount;
		isRankUp = info.GetYellRank() > beforeRank;
		path2 = AssetPathResolver.ResolveYellRankImage(info.GetYellRank());
		if (await AssetLoader.Exists(path2))
		{
			IDisposableAsset<Sprite> disposableAsset3 = await AssetLoader.LoadAssetAsync<Sprite>(path2);
			disposableAsset3.AddTo(this);
			afterYellRank.GetComponent<Image>().sprite = disposableAsset3.Asset;
		}
		closeButton.OnClickAsObservable().Subscribe(delegate
		{
			CloseWindow().Forget();
		}).AddTo(this);
	}

	private void Update()
	{
		RefleshText();
	}

	public async UniTask OpenWindow(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (isOpen)
		{
			return;
		}
		isOpen = true;
		base.gameObject.SetActive(value: true);
		windowObj.GetComponent<Animator>().SetTrigger("In");
		scoreObj.GetComponent<Animator>().SetTrigger("In");
		await UniTask.WaitUntil(() => scoreObj.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("YellScoreLoop"), PlayerLoopTiming.Update, cancellationToken);
		await CountUp(cancellationToken);
		if (isRankUp)
		{
			rankObj.GetComponent<Animator>().SetTrigger("RunkUpIn");
			SoundManager.PlaySe("SFX_SYS_RANKUP1").Forget();
			await UniTask.WhenAny(UniTask.WaitUntil(() => rankObj.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("PopDaniUpLoop"), PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResultFlow.Skip(), PlayerLoopTiming.Update, cancellationToken));
		}
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(8.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResultFlow.Skip(), PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => !isOpen, PlayerLoopTiming.Update, cancellationToken));
		await CloseWindow(cancellationToken);
	}

	public async UniTask CloseWindow(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (isOpen)
		{
			SoundManager.StopSeAll().Forget();
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			if (isRankUp)
			{
				rankObj.GetComponent<Animator>().SetTrigger("RunkUpOut");
			}
			windowObj.GetComponent<Animator>().SetTrigger("Out");
			isOpen = false;
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResultFlow.Skip(), PlayerLoopTiming.Update, cancellationToken));
		}
	}

	private async UniTask CountUp(CancellationToken cancellationToken = default(CancellationToken))
	{
		await YellCountUp(cancellationToken);
		addYellNum = 0;
		beforeYellNum = afterYellNum;
	}

	private async UniTask YellCountUp(CancellationToken cancellationToken = default(CancellationToken))
	{
		float countupFlame = 60f;
		float addRate = (float)addYellNum / countupFlame;
		float addYellFloat = addYellNum;
		float beforeFloat = beforeYellNum;
		SoundManager.PlaySe("SFX_SYS_METER1").Forget();
		float i = addYellNum;
		while (i >= 1f && !ResultFlow.Skip())
		{
			await UniTask.Delay(TimeSpan.FromSeconds(countupFlame / 60f / 60f), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
			addYellFloat -= addRate;
			beforeFloat += addRate;
			addYellNum = (int)Math.Floor(addYellFloat);
			beforeYellNum = (int)Math.Ceiling(beforeFloat);
			i -= addRate;
		}
		SoundManager.StopSe("SFX_SYS_METER1").Forget();
	}

	private void RefleshText()
	{
		yellNumText.text = beforeYellNum.ToString("F0");
		addYellNumText.text = addYellNum.ToString("F0");
	}
}
