using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class GachaVoiceListWindow : MonoBehaviour
{
	[SerializeField]
	private ScrollView scrollView;

	[SerializeField]
	private GameObject voiceListPrefab;

	[SerializeField]
	private Button closeButton;

	public CancellationTokenSource cancel;

	public CancellationToken token;

	private GachaSeriesServerInfo currentInfo;

	private void Start()
	{
		closeButton.OnClickAsObservable().Subscribe(delegate
		{
			cancel?.Cancel();
			cancel = null;
			SoundManager.PlayMusicGachaBGM(currentInfo).Forget();
			SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			base.gameObject.SetActive(value: false);
		}).AddTo(this);
	}

	public void Open(GachaSeriesServerInfo gachaInfo)
	{
		SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
		currentInfo = gachaInfo;
		InitList();
		base.gameObject.SetActive(value: true);
	}

	private void InitList()
	{
		scrollView.ResetList();
		foreach (var (oid, num) in currentInfo.GetSortedMusicItems())
		{
			scrollView.Add(voiceListPrefab, num - 1).transform.GetComponent<GachaVoiceGetList>().Init(this, currentInfo, oid, num);
		}
		scrollView.ResetScroll();
	}
}
