using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.GameData;
using MFG.Types;
using MFG.Types.FightGirlNews;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class HomeFightGirlNews : MonoBehaviour
{
	[Serializable]
	public class NewsAssets
	{
		[SerializeField]
		public NewsType m_newsType;

		[SerializeField]
		public Sprite m_obiSprite;

		[SerializeField]
		public Sprite m_titleSprite;

		[SerializeField]
		public Sprite m_circleSprite;

		[SerializeField]
		public Sprite m_namePlateSprite;
	}

	[SerializeField]
	private Animator m_animator;

	[SerializeField]
	private GameObject m_bgMovie;

	[SerializeField]
	private Image m_appearImage;

	[SerializeField]
	private RawImage m_screen;

	[SerializeField]
	private VideoPlayer m_videoPlayer;

	[SerializeField]
	private Image m_obi;

	[SerializeField]
	private Image m_title;

	[SerializeField]
	private Image m_circle;

	[SerializeField]
	private Image m_namePlate;

	[SerializeField]
	private Image m_nameImage;

	[SerializeField]
	private TextMeshProUGUI m_cvText;

	[SerializeField]
	private SchoolEmblemIcon m_schoolEmblem;

	[SerializeField]
	private Button m_btSkip;

	[SerializeField]
	private FightGirlNewsProperties m_masters;

	[SerializeField]
	private List<NewsAssets> m_newsAssets;

	private bool isSkip;

	private bool isMovieError;

	private bool isBGMChange;

	private bool isMoviePlay;

	private NewsID newsID;

	private FightGirlNewsProperties.Item Master => m_masters.GetItem(newsID);

	private void Start()
	{
		m_btSkip.OnClickAsObservable().Subscribe(async delegate
		{
			isSkip = true;
			StopVideo(m_videoPlayer);
			SoundManager.StopSeAll().Forget();
		}).AddTo(this);
	}

	public async UniTask<Sprite> GetSprite(string path)
	{
		bool flag = !string.IsNullOrEmpty(path);
		if (flag)
		{
			flag = await AssetLoader.Exists(path);
		}
		if (flag)
		{
			IDisposableAsset<Sprite> obj = await AssetLoader.LoadAssetAsync<Sprite>(path);
			obj.AddTo(this);
			return obj.Asset;
		}
		return null;
	}

	public async UniTask Init()
	{
		isSkip = false;
		isMovieError = false;
		m_btSkip.gameObject.SetActive(value: false);
		Image appearImage = m_appearImage;
		appearImage.sprite = await GetSprite(Master.AppearImagePath);
		NewsAssets newsAssets = m_newsAssets.Find((NewsAssets x) => x.m_newsType == Master.NewsType);
		if (newsAssets == null)
		{
			return;
		}
		m_obi.sprite = newsAssets.m_obiSprite;
		m_title.sprite = newsAssets.m_titleSprite;
		m_circle.sprite = newsAssets.m_circleSprite;
		if (Master.IsNamePlate && newsAssets.m_namePlateSprite != null)
		{
			m_namePlate.gameObject.SetActive(value: true);
			m_namePlate.sprite = newsAssets.m_namePlateSprite;
			appearImage = m_nameImage;
			appearImage.sprite = await GetSprite(Master.NameImagePath);
			m_cvText.gameObject.SetActive(Master.IsNewGirl);
			m_cvText.text = Master.Profile?.CV ?? "";
			m_schoolEmblem.gameObject.SetActive(Master.IsNewGirl);
			if (Master.IsNewGirl)
			{
				await m_schoolEmblem.Reflesh(Master.Profile.SchoolID);
			}
		}
		else
		{
			m_namePlate.gameObject.SetActive(value: false);
		}
		isMoviePlay = !string.IsNullOrEmpty(Master.MoviePath);
		if (isMoviePlay)
		{
			m_videoPlayer.targetTexture.Release();
			m_videoPlayer.url = Master.MovieAssetsPath;
			m_videoPlayer.loopPointReached += StopVideo;
			m_videoPlayer.errorReceived += Movie_Error;
		}
	}

	public bool IsDisp()
	{
		if (Master == null)
		{
			return false;
		}
		if (Master.ID <= NewsID.UnlockJun && Master.ID != NewsID.UnlockChaos)
		{
			return false;
		}
		if (Master.IsEnable())
		{
			return !GameDataManager.PlayerConfig.FightGirlNewsFinishIDs.Contains(Master.ID);
		}
		return false;
	}

	public async UniTask Play(NewsID id, CancellationToken cancellationToken)
	{
		newsID = id;
		if (IsDisp())
		{
			GameDataManager.PlayerConfig.FightGirlNewsFinishIDs.Add(newsID);
			await PresentAddition();
			await Init();
			base.gameObject.SetActive(value: true);
			m_animator.SetTrigger("TitleIn");
			await UniTask.WhenAny(NewsProcess(cancellationToken), UniTask.WaitUntil(() => isSkip, PlayerLoopTiming.Update, cancellationToken));
			m_btSkip.gameObject.SetActive(value: false);
			if (isSkip)
			{
				m_animator.SetTrigger("Skip");
			}
			await UniTask.Delay(TimeSpan.FromSeconds(0.30000001192092896), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(isSkip ? 3f : 20f), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResidentFlow.CustomInput.GetMouseButtonDown(0), PlayerLoopTiming.Update, cancellationToken));
			m_animator.SetTrigger("ImageOut");
			await UniTask.WaitUntil(() => m_animator.GetCurrentAnimatorStateInfo(0).IsName("Wait"), PlayerLoopTiming.Update, cancellationToken);
			FinishingTask();
		}
	}

	private void Movie_Error(VideoPlayer source, string message)
	{
		isMovieError = true;
	}

	private async UniTask NewsProcess(CancellationToken cancellationToken)
	{
		await UniTask.WhenAny(UniTask.WaitUntil(() => isSkip, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => m_animator.GetCurrentAnimatorStateInfo(0).IsName("WhiteWait"), PlayerLoopTiming.Update, cancellationToken));
		if (Master.NewsType == NewsType.MusicGacha)
		{
			SoundManager.PlayBgm(Master.BgmLabel).Forget();
		}
		if (isMoviePlay)
		{
			if (!isSkip)
			{
				m_videoPlayer.Play();
				if (isMovieError)
				{
					m_animator.SetTrigger("ImageIn");
				}
				else
				{
					m_animator.SetTrigger("MovieIn");
					m_screen.gameObject.SetActive(value: true);
					MovieSoundProsess(cancellationToken).Forget();
				}
			}
			else
			{
				m_animator.SetTrigger("MovieIn");
			}
		}
		else
		{
			m_animator.SetTrigger("ImageIn");
		}
		await UniTask.WhenAny(UniTask.WaitUntil(() => isSkip, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => m_animator.GetCurrentAnimatorStateInfo(0).IsName("ImageLoop"), PlayerLoopTiming.Update, cancellationToken));
	}

	private async UniTask PresentAddition()
	{
		if (Master.NewsType == NewsType.Present)
		{
			GameDataManager.Gacha.GachaSeriesInfos.Find((GachaSeriesInfo x) => x.SeriesID == 0).ExchangePoint += 100;
			await GameDataManager.Gacha.Save();
			await GameDataManager.PlayerConfig.Save();
		}
	}

	public async UniTask MovieSoundProsess(CancellationToken cancellationToken)
	{
		if ((Master.MovieSoundList?.Count ?? 0) <= 0)
		{
			return;
		}
		int num = 0;
		foreach (FightGirlNewsProperties.Item.MovieSoundManager soundInfo in Master.MovieSoundList.OrderBy((FightGirlNewsProperties.Item.MovieSoundManager x) => x.Frame))
		{
			await UniTask.WhenAny(UniTask.WaitUntil(() => isSkip, PlayerLoopTiming.Update, cancellationToken), UniTask.Delay(TimeSpan.FromSeconds((float)(soundInfo.Frame - num) / 60f), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken));
			if (isSkip)
			{
				return;
			}
			if (soundInfo.IsBGM)
			{
				isBGMChange = true;
				SoundManager.PlayBgm(soundInfo.Label).Forget();
			}
			else
			{
				SoundManager.PlaySe(soundInfo.Label).Forget();
			}
			num = soundInfo.Frame;
		}
	}

	public void StopVideo(VideoPlayer videoPlayer)
	{
		m_screen.gameObject.SetActive(value: false);
		videoPlayer.Stop();
		if (isBGMChange)
		{
			isBGMChange = false;
			SoundManager.PlayMyCharaMyRoomBGM().Forget();
		}
		if (!isSkip)
		{
			m_animator.SetTrigger("MovieOut");
		}
	}

	private void FinishingTask()
	{
		SoundManager.StopVoiceAll().Forget();
		SoundManager.StopSeAll().Forget();
		base.gameObject.SetActive(value: false);
		base.gameObject.transform.Find("Black").gameObject.SetActive(value: true);
		base.gameObject.transform.Find("Cut1").gameObject.SetActive(value: true);
		base.gameObject.transform.Find("Cut2").gameObject.SetActive(value: true);
		base.gameObject.transform.Find("White").gameObject.SetActive(value: true);
		if (Master.NewsType == NewsType.MusicGacha)
		{
			SoundManager.PlayMyCharaMyRoomBGM().Forget();
		}
	}

	private void PlayTOJO1()
	{
		SoundManager.PlaySe("SFX_SYS_TOJO1").Forget();
	}

	private void PlaySLIDE3()
	{
		SoundManager.PlaySe("SFX_SYS_SLIDE3").Forget();
	}

	private void PlayBONUS5()
	{
		if (Master.NewsType != NewsType.MusicGacha)
		{
			SoundManager.PlaySe("SFX_SYS_BONUS5").Forget();
		}
	}

	private void PlayNanori()
	{
		if (Master.CharaType >= CharaType.Chara01)
		{
			SoundManager.PlayVoice("VOX_GXX_NANORI", Master.CharaType).Forget();
		}
	}

	private void SkipButtonIn()
	{
		m_btSkip.gameObject.SetActive(value: true);
	}
}
