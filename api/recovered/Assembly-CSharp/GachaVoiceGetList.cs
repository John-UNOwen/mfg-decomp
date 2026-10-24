using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.CustomizeItem;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class GachaVoiceGetList : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI itemNo;

	[SerializeField]
	private TextMeshProUGUI soundText;

	[SerializeField]
	private Button previewButton;

	[SerializeField]
	private GameObject frameDark;

	public void Init(GachaVoiceListWindow window, GachaSeriesServerInfo info, string oid, int no)
	{
		bool flag = ItemUtility.IsPossession(oid);
		frameDark.SetActive(!flag);
		previewButton.enabled = flag;
		itemNo.text = $"No.{no}";
		soundText.text = ItemUtility.ItemName(oid) ?? "";
		if (ItemUtility.IsVoiceItem(oid))
		{
			previewButton.OnClickAsObservable().Subscribe(delegate
			{
				window.cancel?.Cancel();
				SoundManager.PlayMusicGachaBGM(info).Forget();
				SoundManager.PlayVoice(SoundManager.GetCharaVoiceLabel(VoiceMaster.GetItem(oid).Label, info.PickUpCharas[0])).Forget();
			}).AddTo(this);
			return;
		}
		previewButton.OnClickAsObservable().Subscribe(async delegate
		{
			window.cancel?.Cancel();
			await UniTask.NextFrame();
			window.cancel = new CancellationTokenSource();
			window.token = window.cancel.Token;
			ItemID itemID = CustomizeItemMaster.GetItem(oid).ItemID;
			if (itemID != 0)
			{
				SoundManager.StopBgm().Forget();
				SoundManager.PlayBgm(itemID.ReachBgmToThumbBgmLabel() + "G").Forget();
				try
				{
					await UniTask.Delay(TimeSpan.FromSeconds(10.0), ignoreTimeScale: false, PlayerLoopTiming.Update, window.token);
					SoundManager.PlayMusicGachaBGM(info).Forget();
				}
				catch (OperationCanceledException)
				{
					Debug.Log("曲preview変更");
				}
			}
		}).AddTo(this);
	}
}
