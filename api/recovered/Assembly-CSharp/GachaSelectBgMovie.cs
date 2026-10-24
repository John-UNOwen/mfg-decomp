using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.GameData;
using MFG.MahjongPlay.Cutin;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.CutinItem;
using UniRx;
using UnityEngine;
using UnityEngine.Video;

public class GachaSelectBgMovie : MonoBehaviour
{
	[SerializeField]
	private GameObject bgMovie;

	[SerializeField]
	private CutinManager cutinManager;

	[SerializeField]
	private List<PreviewPlayerInteractor> previewPlayerInteractors = new List<PreviewPlayerInteractor>();

	[SerializeField]
	private GameObject bgWhite;

	[SerializeField]
	private GameObject musicGachaBgParent;

	private GameObject musicGachaBgObj;

	private CancellationTokenSource playCancellation;

	private CancellationTokenSource cutinPlayCancellation;

	private GachaSeriesServerInfo currentGachaSeriesInfo;

	private int bgMovieIndex;

	private List<string> movies = new List<string>();

	private List<ItemID> LimitedCharaReturnsMovieItemIDs = new List<ItemID>
	{
		ItemID.Item10AgariSR02,
		ItemID.Item11AgariSR01,
		ItemID.Item12AgariSR02,
		ItemID.Item13AgariSR02,
		ItemID.Item14AgariSR02,
		ItemID.Item15AgariSR02,
		ItemID.Item16AgariSR02,
		ItemID.Item17AgariSR02,
		ItemID.Item18AgariSR02,
		ItemID.Item19AgariSR02,
		ItemID.Item20AgariSR02
	};

	private async UniTask PlayCutIn(List<ItemID> items, CancellationToken cancellation)
	{
		if (items != null && items.Count > 0)
		{
			foreach (ItemID item in items)
			{
				cutinPlayCancellation = new CancellationTokenSource();
				try
				{
					PlayPreview(item, cutinPlayCancellation).Forget();
					await UniTask.Delay(5000, ignoreTimeScale: false, PlayerLoopTiming.Update, cancellation);
				}
				catch (OperationCanceledException)
				{
					return;
				}
				cutinPlayCancellation?.Cancel();
			}
		}
		BgMovie_Change(bgMovie.GetComponent<VideoPlayer>());
	}

	private async UniTask PlayPreview(ItemID itemID, CancellationTokenSource cts)
	{
		Item itemInfo = CutinItemMaster.GetItem(itemID);
		Cha cha = Cha.Jicha;
		_ = previewPlayerInteractors[(int)cha];
		CutinType cutinType;
		switch (itemInfo.Category)
		{
		case Category.Naki:
			cutinType = CutinType.Pon;
			break;
		case Category.AgariSmall:
		case Category.AgariBig:
			cutinType = CutinType.Ron;
			break;
		case Category.Reach:
			cutinType = CutinType.Reach;
			break;
		default:
			cutinType = CutinType.Pon;
			break;
		}
		CutinType cutinType2 = cutinType;
		try
		{
			cts.Token.ThrowIfCancellationRequested();
			List<CharaType> list = (from CharaType x in Enum.GetValues(typeof(CharaType))
				where itemInfo.CharaType != x
				select x).ToList();
			for (Cha cha2 = Cha.Simocha; cha2 <= Cha.Kamicha; cha2++)
			{
				CharaType charaType = list[UnityEngine.Random.Range(0, list.Count)];
				list.Remove(charaType);
				previewPlayerInteractors[(int)cha2].m_charaType = charaType;
				previewPlayerInteractors[(int)cha2].SetPaymentInfo(Han.Mangan, isScorePaymentPlayer: true);
			}
			Debug.Log("CutinPreview Play ItemID:" + itemID);
			cutinManager.Play(cutinType2, new CutinItemInfo
			{
				ItemID = itemID,
				IsOn = true
			}, cha, previewPlayerInteractors, null, cts.Token).Forget();
			if (itemInfo.Category == Category.AgariSmall || itemInfo.Category == Category.AgariBig)
			{
				await cutinManager.AgariCutin_WaitIntroductionEnd(cts.Token);
				cutinManager.CurrentCutin_End(cts.Token).Forget();
			}
			if (cutinManager.IsPlaying)
			{
				await UniTask.WaitWhile(() => cutinManager.IsPlaying, PlayerLoopTiming.Update, cts.Token);
			}
		}
		catch (OperationCanceledException)
		{
			Debug.Log("CutinPreview Canceled ItemID:" + itemID);
		}
		catch (ObjectDisposedException)
		{
			Debug.Log("ObjectDisposedException Canceled ItemID:" + itemID);
		}
	}

	public void GachaSeriesChange(GachaSeriesServerInfo gachaSeriesInfo)
	{
		if (GameDataManager.Gacha.EntryGachaSelectSceneMode != GachaSeriesType.Music)
		{
			bgMovie.GetComponent<VideoPlayer>().Stop();
			cutinPlayCancellation?.Cancel();
			playCancellation?.Cancel();
		}
		currentGachaSeriesInfo = gachaSeriesInfo;
		bgMovie.transform.GetComponent<Animator>().SetTrigger("In");
		Play();
	}

	public void Init(GachaSeriesServerInfo gachaSeriesInfo)
	{
		currentGachaSeriesInfo = gachaSeriesInfo;
		if (GameDataManager.Gacha.EntryGachaSelectSceneMode != GachaSeriesType.Music)
		{
			VideoPlayer component = bgMovie.GetComponent<VideoPlayer>();
			component.source = VideoSource.Url;
			component.loopPointReached += BgMovie_Change;
		}
		Play();
		bgMovie.transform.Find("Bg").gameObject.SetActive(value: false);
	}

	private void Play()
	{
		if (GameDataManager.Gacha.EntryGachaSelectSceneMode == GachaSeriesType.Music)
		{
			MusicGachaBgSetup(currentGachaSeriesInfo).Forget();
			return;
		}
		VideoPlayer component = bgMovie.GetComponent<VideoPlayer>();
		bgMovieIndex = 0;
		movies = currentGachaSeriesInfo.GetGachaSelectMovies();
		string text = movies[bgMovieIndex];
		if (text == "CutinPlay")
		{
			bgWhite.SetActive(value: true);
			playCancellation = new CancellationTokenSource();
			if (currentGachaSeriesInfo.PickupKind == PickupKind.LimitedReturns)
			{
				PlayCutIn(LimitedCharaReturnsMovieItemIDs, playCancellation.Token).Forget();
			}
			else
			{
				PlayCutIn(currentGachaSeriesInfo.LimitedItems, playCancellation.Token).Forget();
			}
		}
		else
		{
			bgWhite.SetActive(value: false);
			component.url = text;
			component.Play();
		}
	}

	public void BgMovie_Change(VideoPlayer player)
	{
		bgMovieIndex++;
		if (bgMovieIndex >= movies.Count)
		{
			bgMovieIndex = 0;
		}
		string text = movies[bgMovieIndex];
		if (text == "CutinPlay")
		{
			bgWhite.SetActive(value: true);
			player.Stop();
			playCancellation = new CancellationTokenSource();
			if (currentGachaSeriesInfo.PickupKind == PickupKind.LimitedReturns)
			{
				PlayCutIn(LimitedCharaReturnsMovieItemIDs, playCancellation.Token).Forget();
			}
			else
			{
				PlayCutIn(currentGachaSeriesInfo.LimitedItems, playCancellation.Token).Forget();
			}
		}
		else
		{
			bgWhite.SetActive(value: false);
			player.url = text;
			player.Play();
		}
	}

	private void OnDestroy()
	{
		if (playCancellation != null)
		{
			playCancellation.Cancel();
			playCancellation.Dispose();
			playCancellation = null;
		}
		if (cutinPlayCancellation != null)
		{
			cutinPlayCancellation.Cancel();
			cutinPlayCancellation.Dispose();
			cutinPlayCancellation = null;
		}
	}

	public async UniTask MusicGachaBgSetup(GachaSeriesServerInfo gachaInfo)
	{
		if (musicGachaBgObj != null)
		{
			UnityEngine.Object.Destroy(musicGachaBgObj);
		}
		IDisposableAsset<GameObject> disposableAsset = await AssetLoader.LoadAssetAsync<GameObject>($"MusicGachaBG/{gachaInfo.SeriesID}/Bg.prefab");
		disposableAsset.AddTo(this);
		musicGachaBgObj = UnityEngine.Object.Instantiate(disposableAsset.Asset, musicGachaBgParent.transform);
	}
}
