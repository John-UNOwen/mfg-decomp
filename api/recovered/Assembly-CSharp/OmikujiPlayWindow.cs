using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Omikuji;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class OmikujiPlayWindow : MonoBehaviour
{
	[SerializeField]
	private GameObject m_window;

	[SerializeField]
	private Button m_closeButton;

	[SerializeField]
	private Animator m_janponAnim;

	[SerializeField]
	private Image m_itemImage;

	[SerializeField]
	private TextMeshProUGUI m_itemName;

	public bool _isOpen;

	public virtual bool IsOpen => _isOpen;

	public void Start()
	{
		if (m_closeButton != null)
		{
			m_closeButton.OnClickAsObservable().Subscribe(delegate
			{
				Close();
			}).AddTo(this);
		}
	}

	private async UniTask<bool> LotteryPlay()
	{
		try
		{
			OmikujiEventInfo curInfo = GameDataManager.OthersEvent.CurrentOmikujiInfo;
			Item master = curInfo.Master;
			List<CharaType> lotteryCharaTypes = curInfo.GetLotteryCharaTypes();
			Debug.Log("lotteryCharaTypes / " + string.Join(",", lotteryCharaTypes));
			CharaType chara = ((lotteryCharaTypes.Count <= 0) ? ((CharaType)UnityEngine.Random.Range(0, (int)(master.MaxCharaType + 1))) : lotteryCharaTypes[UnityEngine.Random.Range(0, lotteryCharaTypes.Count)]);
			GameDataManager.OthersEvent.CurrentOmikujiInfo.TodayCharaTypes = chara;
			IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(master.ItemImagePath(chara));
			disposableAsset.AddTo(this);
			m_itemImage.sprite = disposableAsset.Asset;
			m_itemName.text = master.GetItemName(chara);
			CardConnectSend(master.OmikujiType, master.CardConnectMissionID, chara);
			curInfo.OmikujiDone(chara);
			return true;
		}
		catch
		{
			return false;
		}
	}

	public async UniTask Open(bool isFirstDisp = false)
	{
		if (_isOpen || !GameDataManager.OthersEvent.CurrentOmikujiInfo.IsOmikujiPlayEnable())
		{
			return;
		}
		if (!isFirstDisp)
		{
			ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
		}
		m_closeButton.gameObject.SetActive(value: false);
		m_window.SetActive(value: true);
		_isOpen = true;
		if (await LotteryPlay())
		{
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			m_window.GetComponent<Animator>().SetTrigger("In");
			m_janponAnim.SetTrigger("In");
			await UniTask.WaitUntil(() => m_janponAnim.GetCurrentAnimatorStateInfo(0).IsName("JanPonPlayLoop"));
			m_closeButton.gameObject.SetActive(value: true);
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(10.0)), UniTask.WaitUntil(() => !_isOpen));
		}
		if (_isOpen)
		{
			Close();
		}
		if (!isFirstDisp)
		{
			ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
		}
	}

	public void Close(bool isPlaySe = true)
	{
		if (_isOpen)
		{
			if (isPlaySe)
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			}
			if (m_window != null)
			{
				m_window.GetComponent<Animator>().SetTrigger("Out");
				m_window.SetActive(value: false);
			}
			_isOpen = false;
		}
	}

	public static void CardConnectSend(OmikujiType omikujiType, CardConnectMissionID id, CharaType chara)
	{
		Item item = OmikujiMaster.GetItem(omikujiType);
		string text = "OID_" + ((int)(chara + 1)).ToString("D2") + "MissionEvent" + ((int)(id + 1)).ToString("D2");
		string itemName = item.GetItemName(chara);
		GameDataManager.CardConnect.m_ccInfos.Add(new CardConnectInfo(GameDataManager.CardConnect.now_idx, "MemorialCard", text, itemName));
		GameDataManager.CardConnect.now_idx++;
		MissionMemorialPrintNoticeInfo item2 = new MissionMemorialPrintNoticeInfo
		{
			CCID = id,
			Chara = chara,
			ItemName = itemName,
			ItemOID = text
		};
		GameDataManager.Notice.MissionMemorialPrintNoticeInfos.Add(item2);
	}
}
