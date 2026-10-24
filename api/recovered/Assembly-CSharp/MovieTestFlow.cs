using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.Types;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class MovieTestFlow : MonoBehaviour
{
	private class CutinMovie : IDisposable
	{
		private GameObject m_parent;

		private GameObject m_rawImageGamObject;

		private RawImage m_rawImage;

		private RenderTexture m_renderTexture;

		private VideoPlayer m_videPlayer;

		private string m_fileName;

		private CancellationToken m_cancellationToken;

		public void Create(GameObject parentGameObject, string movieFullPathFileName, CancellationToken cancellationToken = default(CancellationToken))
		{
			m_parent = parentGameObject;
			m_fileName = movieFullPathFileName;
			m_cancellationToken = cancellationToken;
			m_rawImageGamObject = parentGameObject.transform.Find("RawImage").gameObject;
			m_rawImage = m_rawImageGamObject.GetComponent<RawImage>();
			m_renderTexture = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGB32);
			m_renderTexture.Create();
			m_videPlayer = m_parent.AddComponent<VideoPlayer>();
			m_videPlayer.playOnAwake = false;
			m_videPlayer.waitForFirstFrame = true;
			m_videPlayer.source = VideoSource.Url;
			m_videPlayer.url = movieFullPathFileName;
			m_videPlayer.renderMode = VideoRenderMode.RenderTexture;
			m_videPlayer.targetTexture = m_renderTexture;
			m_videPlayer.audioOutputMode = VideoAudioOutputMode.None;
			m_videPlayer.isLooping = false;
		}

		public async UniTask Prepare()
		{
			m_videPlayer.Prepare();
			await UniTask.WaitUntil(() => m_videPlayer.isPrepared, PlayerLoopTiming.Update, m_cancellationToken);
			m_videPlayer.Play();
			m_videPlayer.Pause();
			m_videPlayer.frame = 0L;
		}

		public async UniTask Play(bool afterPlayPrepare = true)
		{
			m_rawImageGamObject.SetActive(value: true);
			m_rawImage.texture = m_renderTexture;
			m_videPlayer.Play();
			await UniTask.WaitWhile(() => m_videPlayer.isPlaying, PlayerLoopTiming.Update, m_cancellationToken);
			m_videPlayer.Pause();
			m_rawImageGamObject.SetActive(value: false);
			if (afterPlayPrepare)
			{
				m_videPlayer.frame = 0L;
			}
		}

		public bool IsPlaying()
		{
			return m_videPlayer.isPlaying;
		}

		public void Stop()
		{
			m_videPlayer.Stop();
		}

		public void Dispose()
		{
			m_videPlayer.Stop();
			UnityEngine.Object.Destroy(m_renderTexture);
		}
	}

	[SerializeField]
	private GameObject m_movieParent;

	[SerializeField]
	private Text m_debugText;

	private List<Dictionary<CutinAssetType, CutinMovie>> m_movieCutins = new List<Dictionary<CutinAssetType, CutinMovie>>();

	private List<CutinAssetType> m_assetTypes = new List<CutinAssetType>();

	private async void Start()
	{
		await UniTask.WaitUntil(() => ResidentFlow.IsInitialized);
		try
		{
			CancellationToken token = this.GetCancellationTokenOnDestroy();
			m_debugText.text = "Create And Prepare";
			Movie_Init();
			int j = 5;
			while (j > 0)
			{
				m_debugText.text = $"Delay {j}";
				await UniTask.Delay(1000);
				int num = j - 1;
				j = num;
			}
			int counter = 0;
			while (true)
			{
				m_debugText.text = $"Play ({m_assetTypes[counter % 2]})";
				CutinMovie movie = m_movieCutins[0][m_assetTypes[counter % 2]];
				movie.Play().Forget();
				await UniTask.WaitWhile(() => movie.IsPlaying(), PlayerLoopTiming.Update, token);
				int num;
				for (j = 5; j > 0; j = num)
				{
					m_debugText.text = $"Delay {j}";
					await UniTask.Delay(1000);
					num = j - 1;
				}
				num = counter + 1;
				counter = num;
			}
		}
		catch (OperationCanceledException)
		{
		}
	}

	private void Movie_Init()
	{
		CharaType charaType = CharaType.Chara01;
		Dictionary<CutinAssetType, CutinMovie> dictionary = new Dictionary<CutinAssetType, CutinMovie>();
		for (int i = 0; i < m_assetTypes.Count; i++)
		{
			CutinAssetType cutinAssetType = m_assetTypes[i];
			CutinMovie cutinMovie = new CutinMovie();
			cutinMovie.Create(m_movieParent, AssetPathResolver.Resolve(PathType.CutinItem, charaType, cutinAssetType));
			cutinMovie.AddTo(this);
			cutinMovie.Prepare().Forget();
			dictionary.Add(cutinAssetType, cutinMovie);
		}
		m_movieCutins.Add(dictionary);
	}

	public void OnClickTitle()
	{
		ResidentFlow.GetInstance().LoadScene("Logo");
	}
}
