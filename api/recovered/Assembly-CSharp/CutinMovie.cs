using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class CutinMovie : MonoBehaviour, IDisposable
{
	[SerializeField]
	private GameObject m_rawImageGamObject;

	[SerializeField]
	[Tooltip("ムービーファイル名(Assets/StreamingAssets以下ファイルパスで指定)")]
	private string m_movieFilePath;

	[SerializeField]
	[Tooltip("ループ再生するか")]
	private bool m_loop;

	[SerializeField]
	[Tooltip("Prepareするか")]
	public bool m_prepareOnAwake = true;

	[SerializeField]
	[Tooltip("終了時に次の再生に向けてPrepareするか")]
	private bool m_prepareForNextPlay = true;

	[SerializeField]
	[Tooltip("Activeと同時に再生を開始するか")]
	public bool m_playOnActive = true;

	private RawImage m_rawImage;

	private RenderTexture m_renderTexture;

	private VideoPlayer m_videoPlayer;

	private bool m_initialized;

	private bool m_disposed;

	private bool _isPrepareCalled;

	private bool _isPrepared;

	public string MovieFilePath
	{
		get
		{
			return m_movieFilePath;
		}
		set
		{
			m_movieFilePath = value;
		}
	}

	public bool IsPrepared => m_videoPlayer.isPrepared;

	private void Awake()
	{
		Init();
		if (m_prepareOnAwake)
		{
			Prepare().Forget();
		}
	}

	private void Init()
	{
		if (!m_initialized)
		{
			m_initialized = true;
			m_rawImage = m_rawImageGamObject.GetComponent<RawImage>();
			m_renderTexture = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGB32);
			m_renderTexture.Create();
			m_videoPlayer = base.gameObject.AddComponent<VideoPlayer>();
			m_videoPlayer.playOnAwake = false;
			m_videoPlayer.waitForFirstFrame = true;
			m_videoPlayer.source = VideoSource.Url;
			m_videoPlayer.url = Path.Combine(Application.streamingAssetsPath, m_movieFilePath);
			m_videoPlayer.renderMode = VideoRenderMode.RenderTexture;
			m_videoPlayer.targetTexture = m_renderTexture;
			m_videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
			m_videoPlayer.isLooping = m_loop;
		}
	}

	private async void Start()
	{
		this.GetCancellationTokenOnDestroy();
		if (m_playOnActive)
		{
			m_rawImage.gameObject.OnEnableAsObservable().Subscribe(async delegate
			{
				if (m_playOnActive)
				{
					_ = m_prepareOnAwake;
					Play().Forget();
				}
			}).AddTo(this);
		}
		if (m_rawImage.gameObject.activeSelf && m_playOnActive)
		{
			_ = m_prepareOnAwake;
			Play().Forget();
		}
	}

	private void OnDestroy()
	{
		Dispose();
	}

	public async UniTask Prepare()
	{
		Init();
		_isPrepareCalled = true;
		if (!m_videoPlayer.isPrepared)
		{
			CancellationToken cancellationTokenOnDestroy = this.GetCancellationTokenOnDestroy();
			m_videoPlayer.Prepare();
			Debug.Log("Prepare Begin: " + m_videoPlayer.url);
			await UniTask.WaitUntil(() => m_videoPlayer.isPrepared, PlayerLoopTiming.Update, cancellationTokenOnDestroy);
			m_videoPlayer.targetTexture = m_renderTexture;
			Debug.Log("Prepare End: " + m_videoPlayer.url);
			_isPrepared = true;
		}
	}

	public async UniTask Play()
	{
		Debug.Log("Play Begin: " + m_videoPlayer.url);
		CancellationToken token = this.GetCancellationTokenOnDestroy();
		m_rawImage.texture = m_renderTexture;
		if (_isPrepareCalled && !_isPrepared)
		{
			Debug.Log("Play: Prepare Wait Begin");
			await UniTask.WaitUntil(() => _isPrepared, PlayerLoopTiming.Update, token);
			Debug.Log("Play: Prepare Wait End");
		}
		m_videoPlayer.Play();
		await UniTask.WaitWhile(() => m_videoPlayer.isPlaying, PlayerLoopTiming.Update, token);
		Debug.Log($"Play End: {m_videoPlayer.url} frame:{m_videoPlayer.frame}, frameCount:{m_videoPlayer.frameCount}");
	}

	public bool IsPlaying()
	{
		return m_videoPlayer.isPlaying;
	}

	public void Stop()
	{
		m_videoPlayer.Stop();
	}

	public void Dispose()
	{
		if (!m_initialized)
		{
			m_disposed = true;
		}
		else if (!m_disposed)
		{
			m_videoPlayer.Stop();
			UnityEngine.Object.Destroy(m_renderTexture);
			Debug.Log("Cutin Movie Destroy: " + m_videoPlayer.url);
		}
	}
}
