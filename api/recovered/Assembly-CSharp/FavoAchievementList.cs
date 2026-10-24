using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Types;
using MFG.Types.Customize.CustomizeItem;
using MFG.Types.Favo;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class FavoAchievementList : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI favoNum;

	[SerializeField]
	private TextMeshProUGUI achievementText;

	[SerializeField]
	private List<Button> bts;

	[SerializeField]
	private GameObject iconDone;

	[SerializeField]
	private GameObject frameDark;

	private Button button;

	private FavoMasterData favo;

	public void BtRefresh()
	{
		bool flag = GameDataManager.Player.GetCurrentChara().Level >= favo.FavoNum;
		iconDone.SetActive(flag);
		frameDark.SetActive(!flag);
		if (favo.type == FavoItemType.Voice || favo.type == FavoItemType.Bgm)
		{
			button.interactable = flag;
		}
	}

	public void Init(MyRoomFavoWindow window, FavoMasterData favoMaster, CancellationToken cancellationToken)
	{
		CharaType chara = GameDataManager.Player.SelectChara;
		favo = favoMaster;
		favoNum.text = favo.FavoNum.ToString();
		achievementText.text = favo.GetName(chara);
		button = bts[(int)favo.type];
		button.gameObject.SetActive(value: true);
		BtRefresh();
		switch (favo.type)
		{
		case FavoItemType.Voice:
			button.OnClickAsObservable().Subscribe(delegate
			{
				SoundManager.PlayMyCharaVoice(favo.unlockVoiceID).Forget();
			}).AddTo(this);
			break;
		case FavoItemType.Bgm:
			button.OnClickAsObservable().Subscribe(async delegate
			{
				window.BgmCancelToken(isNullCheck: true);
				ItemID itemID = ItemIDExtentions.ReachBgmFromFavoOID(favo.ObjectID, chara);
				if (itemID != 0)
				{
					SoundManager.StopBgm().Forget();
					SoundManager.PlayBgm(itemID.ReachBgmToThumbBgmLabel()).Forget();
					try
					{
						await UniTask.Delay(TimeSpan.FromSeconds(10.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
						SoundManager.PlayMyCharaMyRoomBGM().Forget();
					}
					catch (OperationCanceledException)
					{
						Debug.Log("曲preview変更");
					}
				}
			}).AddTo(this);
			break;
		}
	}
}
