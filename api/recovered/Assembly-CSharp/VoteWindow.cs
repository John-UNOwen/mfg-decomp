using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.GameData;
using MFG.MahjongPlay.Cutin;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.CutinItem;
using MFG.Types.Vote;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class VoteWindow : MonoBehaviour
{
	[SerializeField]
	private GameObject windowObj;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private TextMeshProUGUI dateText;

	[SerializeField]
	private TextMeshProUGUI votingRightsText;

	[SerializeField]
	private Transform voteParent;

	[SerializeField]
	private GameObject votingRightsAttention;

	[SerializeField]
	private GameObject confirmWindow;

	[SerializeField]
	private TextMeshProUGUI confirmAttention;

	[SerializeField]
	private Button confirmOKButton;

	[SerializeField]
	private Button confirmNGButton;

	[SerializeField]
	private GameObject consumeWindowObj;

	[SerializeField]
	private TextMeshProUGUI votingRightsTextInConsume;

	[SerializeField]
	private TextMeshProUGUI consumeVotingRightsText;

	[SerializeField]
	private Button minusButton;

	[SerializeField]
	private Button pulsButton;

	[SerializeField]
	private Button puls10Button;

	[SerializeField]
	private Button votingButton;

	[SerializeField]
	private Button closeConsumeButton;

	[SerializeField]
	private Image charaImage;

	[SerializeField]
	private GameObject voteCompleteWindow;

	[SerializeField]
	private Image voteCharaImage;

	[SerializeField]
	private Image voteCharaNameImage;

	[SerializeField]
	private Image voteCharaBg;

	[SerializeField]
	[Tooltip("背景色 : キャラ順")]
	private List<Color> voteCharaBgColors;

	[SerializeField]
	private CutinManager m_cutinManager;

	[SerializeField]
	private List<PreviewPlayerInteractor> m_previewPlayerInteractors = new List<PreviewPlayerInteractor>();

	private CancellationTokenSource _listCts;

	private CancellationTokenSource m_cutinPreviewCts;

	[SerializeField]
	private TextMeshProUGUI itemName;

	public bool isOpen;

	public bool isConsumeOpen;

	public bool isConfirmOpen;

	public GameEventType eventType;

	private int useVotingRights;

	private int usableVotingRights;

	private int allVotingRights;

	private CharaType selectingChara;

	private Dictionary<CharaType, Sprite> Sprites = new Dictionary<CharaType, Sprite>();

	private int voteTicketMax = 200;

	private VoteHomeWindow HomeWindow;

	private bool isHomeVoteSaved = true;

	private bool isCardConnectSaved = true;

	private MFG.Types.Vote.Item Master => GameDataManager.Vote.CurrentInfo.Master;

	private async void Start()
	{
		CountButtonAllEnable(isActive: false);
		if (confirmOKButton != null)
		{
			confirmOKButton.interactable = false;
		}
		closeButton.OnClickAsObservable().Subscribe(async delegate
		{
			try
			{
				if (Master.IsPointMaxEnable && GameDataManager.Vote.CurrentInfo.Point > voteTicketMax)
				{
					await OpenConfirmWindow();
				}
				else
				{
					CloseWindow().Forget();
				}
			}
			catch
			{
				CloseWindow().Forget();
			}
		}).AddTo(this);
		if (confirmOKButton != null)
		{
			confirmOKButton.OnClickAsObservable().Subscribe(delegate
			{
				if (isConfirmOpen)
				{
					GameDataManager.Vote.CurrentInfo.PointDiscard();
					CloseWindow().Forget();
				}
			}).AddTo(this);
		}
		if ((bool)confirmNGButton)
		{
			confirmNGButton.OnClickAsObservable().Subscribe(delegate
			{
				isConfirmOpen = false;
				confirmOKButton.interactable = false;
				SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
				confirmWindow.GetComponent<Animator>().SetTrigger("Out");
				confirmWindow.SetActive(value: false);
			}).AddTo(this);
		}
		if (closeConsumeButton != null)
		{
			closeConsumeButton.OnClickAsObservable().Subscribe(delegate
			{
				CloseConsumeWindow().Forget();
			}).AddTo(this);
		}
		if (votingButton != null)
		{
			votingButton.OnClickAsObservable().Subscribe(async delegate
			{
				await Voting();
			}).AddTo(this);
		}
		if (minusButton != null)
		{
			minusButton.OnClickAsObservable().Subscribe(delegate
			{
				if (useVotingRights > 0)
				{
					useVotingRights--;
					usableVotingRights = allVotingRights - useVotingRights;
					SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
				}
				else
				{
					SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
				}
			}).AddTo(this);
		}
		if (pulsButton != null)
		{
			pulsButton.OnClickAsObservable().Subscribe(delegate
			{
				if (useVotingRights < allVotingRights)
				{
					useVotingRights++;
					usableVotingRights = allVotingRights - useVotingRights;
					SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
				}
				else
				{
					SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
				}
			}).AddTo(this);
		}
		if (puls10Button != null)
		{
			puls10Button.OnClickAsObservable().Subscribe(delegate
			{
				if (useVotingRights < allVotingRights)
				{
					int num = useVotingRights + 10;
					useVotingRights = ((num >= allVotingRights) ? allVotingRights : num);
					usableVotingRights = allVotingRights - useVotingRights;
					SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
				}
				else
				{
					SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
				}
			}).AddTo(this);
		}
		foreach (Transform child in voteParent)
		{
			if (!Enum.TryParse<CharaType>(child.name, out var chara))
			{
				continue;
			}
			if (!Master.Characters.Contains(chara))
			{
				child.gameObject.SetActive(value: false);
				continue;
			}
			IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(Master.ConsumeWindowIconPath(chara));
			disposableAsset.AddTo(this);
			Sprites.Add(chara, disposableAsset.Asset);
			child.Find("BtVote").GetComponent<Button>().OnClickAsObservable()
				.Subscribe(delegate
				{
					OpenCharaWindow(chara);
				})
				.AddTo(this);
		}
	}

	public async UniTask OpenConfirmWindow()
	{
		if (!isConfirmOpen)
		{
			isConfirmOpen = true;
			confirmWindow.SetActive(value: true);
			SoundManager.PlaySe("SFX_MJG_INFO1").Forget();
			confirmWindow.GetComponent<Animator>().SetTrigger("In");
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(1.0)), UniTask.WaitUntil(() => !isConfirmOpen));
			confirmOKButton.interactable = isConfirmOpen;
		}
	}

	public void RefleshVoteCount()
	{
		VoteEventInfo currentInfo = GameDataManager.Vote.CurrentInfo;
		MFG.Types.Vote.Item master = currentInfo.Master;
		if (votingRightsText != null)
		{
			votingRightsText.text = currentInfo.Point.ToString();
		}
		if (votingRightsAttention != null)
		{
			votingRightsAttention.SetActive(master.IsPointMaxEnable && GameDataManager.Vote.CurrentInfo.Point > voteTicketMax);
		}
		if (!master.IsOneShotVote)
		{
			allVotingRights = currentInfo.Point;
			useVotingRights = 0;
			usableVotingRights = allVotingRights;
		}
		else
		{
			useVotingRights = master.VoteMaxToOneChara;
			votingRightsTextInConsume.text = currentInfo.Point.ToString();
			consumeVotingRightsText.text = (currentInfo.Point - useVotingRights).ToString();
		}
		foreach (Transform item in voteParent)
		{
			if (!Enum.TryParse<CharaType>(item.name, out var chara))
			{
				continue;
			}
			if (master.IsOnlyEnableChara)
			{
				bool flag = GameDataManager.IsCharaSelectable(chara);
				item.Find("BtVote").GetComponent<Button>().interactable = flag;
				item.Find("UnableChara").gameObject.SetActive(!flag);
				if (!flag)
				{
					continue;
				}
			}
			VoteInfo voteInfo = currentInfo.VoteInfos.Find((VoteInfo x) => x.Chara == chara);
			if (voteInfo == null)
			{
				continue;
			}
			if (master.IsOneShotVote)
			{
				bool flag2 = voteInfo.VoteCount < useVotingRights;
				item.Find("BtVote").GetComponent<Button>().interactable = currentInfo.Point >= useVotingRights && flag2;
				item.Find("BtVote/Text").GetComponent<TextMeshProUGUI>().text = (flag2 ? "交換する" : "交換済");
				if (master.IsUnableIcon)
				{
					item.Find("UnableIcon").gameObject.SetActive(flag2);
					item.Find("Chara").gameObject.SetActive(!flag2);
				}
			}
			else
			{
				item.Find("CountBase/Count").GetComponent<TextMeshProUGUI>().text = voteInfo.VoteCount.ToString();
			}
		}
		if (HomeWindow != null)
		{
			HomeWindow.ItemCountRefresh();
		}
	}

	public void OpenWindow(VoteHomeWindow voteHomeWindow = null)
	{
		if (!isOpen)
		{
			HomeWindow = voteHomeWindow;
			if (Master.IsPointMaxEnable)
			{
				voteTicketMax = Master.AvailablePointMax();
				votingRightsAttention.transform.GetComponent<TextMeshProUGUI>().text = "所持票数が" + voteTicketMax.ToString("N0") + "以下になるように投票してください。\n" + voteTicketMax.ToString("N0") + "票を超えている分の投票権はこの画面を閉じると消失いたします。";
				confirmAttention.text = voteTicketMax.ToString("N0") + "票を超えている分の\n投票権が消失いたします。";
			}
			if (!Master.IsHomeVote)
			{
				ResidentFlow.GetInstance().GetTimer().SetTimer((Master.IsPointMaxEnable && GameDataManager.Vote.CurrentInfo.Point > voteTicketMax) ? 99 : 60, disp: true);
				ResidentFlow.GetInstance().GetTimer().StartTimer();
			}
			if (dateText != null)
			{
				dateText.text = Master.DispDate();
			}
			RefleshVoteCount();
			isOpen = true;
			base.gameObject.SetActive(value: true);
			SoundManager.PlaySe("SFX_MJG_INFO1").Forget();
			windowObj.GetComponent<Animator>().SetTrigger("In");
		}
	}

	public async UniTask CloseWindow(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (isOpen)
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			windowObj.GetComponent<Animator>().SetTrigger("Out");
			isOpen = false;
			base.gameObject.SetActive(value: true);
			if (!Master.IsHomeVote)
			{
				ResidentFlow.GetInstance().GetTimer().SetDisp(disp: false);
			}
			await UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
		}
	}

	public void OpenCharaWindow(CharaType chara)
	{
		if (!isConsumeOpen)
		{
			selectingChara = chara;
			charaImage.sprite = Sprites[chara];
			if (itemName != null)
			{
				itemName.text = Master.GetItemName(chara);
			}
			else
			{
				votingButton.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = MasterGameDataManager.Character.Properties(chara).Profile.Name + "\nに投票する";
			}
			isConsumeOpen = true;
			consumeWindowObj.SetActive(value: true);
			SoundManager.PlaySe("SFX_MJG_INFO1").Forget();
			consumeWindowObj.GetComponent<Animator>().SetTrigger("In");
			CountButtonAllEnable(isActive: true);
		}
	}

	public async UniTask CloseConsumeWindow()
	{
		if (isConsumeOpen)
		{
			CountButtonAllEnable(isActive: false);
			SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			consumeWindowObj.GetComponent<Animator>().SetTrigger("Out");
			consumeWindowObj.SetActive(value: false);
			isConsumeOpen = false;
			RefleshVoteCount();
			await UniTask.Delay(TimeSpan.FromSeconds(1.0));
		}
	}

	public void CountButtonAllEnable(bool isActive)
	{
		if (!Master.IsOneShotVote)
		{
			if (votingButton != null)
			{
				votingButton.interactable = isActive;
			}
			if (minusButton != null)
			{
				minusButton.interactable = isActive;
			}
			if (pulsButton != null)
			{
				pulsButton.interactable = isActive;
			}
			if (puls10Button != null)
			{
				puls10Button.interactable = isActive;
			}
			if (closeConsumeButton != null)
			{
				closeConsumeButton.interactable = isActive;
			}
		}
	}

	public async UniTask Voting()
	{
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
		CountButtonAllEnable(isActive: false);
		if (GameDataManager.Vote.CurrentInfo.Vote(selectingChara, useVotingRights))
		{
			Save().Forget();
			if (m_cutinPreviewCts != null)
			{
				m_cutinPreviewCts.Cancel();
				m_cutinPreviewCts.Dispose();
				m_cutinPreviewCts = null;
			}
			await UniTask.WaitWhile(() => !isCardConnectSaved);
			string text = Master.VoteCompleteWindowIconPath(selectingChara);
			if (!string.IsNullOrEmpty(text))
			{
				IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(text);
				disposableAsset.AddTo(this);
				voteCharaImage.sprite = disposableAsset.Asset;
			}
			text = Master.VoteCompleteWindowItemNamePath(selectingChara);
			if (!string.IsNullOrEmpty(text))
			{
				IDisposableAsset<Sprite> disposableAsset2 = await AssetLoader.LoadAssetAsync<Sprite>(text);
				disposableAsset2.AddTo(this);
				voteCharaNameImage.sprite = disposableAsset2.Asset;
			}
			voteCharaBg.color = voteCharaBgColors[(int)selectingChara];
			voteCompleteWindow.SetActive(value: true);
			CloseConsumeWindow().Forget();
			voteCompleteWindow.GetComponent<Animator>().SetTrigger("In");
			SoundManager.PlaySe("SFX_SYS_BONUS5").Forget();
			if (Master.IsCutinPlayInResult)
			{
				try
				{
					string text2 = Master?.RewardItemList?[Master.Characters.IndexOf(selectingChara)] ?? "";
					if (!string.IsNullOrEmpty(text2))
					{
						m_cutinPreviewCts = new CancellationTokenSource();
						PlayPreview(text2, m_cutinPreviewCts).Forget();
					}
				}
				catch
				{
				}
			}
			await UniTask.WaitUntil(() => voteCompleteWindow.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("CharaGetAnimeLoop"));
			if (!Master.IsCutinPlayInResult)
			{
				await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(3.0)), UniTask.WaitUntil(() => ResultFlow.Skip()));
			}
			else
			{
				await UniTask.WhenAny(UniTask.WaitWhile(() => m_cutinManager.IsPlaying), UniTask.WaitUntil(() => ResultFlow.Skip()));
			}
			if (!ResultFlow.Skip())
			{
				voteCompleteWindow.GetComponent<Animator>().SetTrigger("End");
				await UniTask.WhenAny(UniTask.WaitUntil(() => ResultFlow.Skip()), UniTask.WaitUntil(() => voteCompleteWindow.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("CharaGetAnimeWait")));
			}
			RefleshVoteCount();
			await UniTask.WaitWhile(() => !isHomeVoteSaved);
			voteCompleteWindow.SetActive(value: false);
		}
		else
		{
			CloseConsumeWindow().Forget();
		}
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
	}

	private async UniTask Save()
	{
		if (Master.IsHomeVote)
		{
			isHomeVoteSaved = false;
			await GameDataManager.Customize.Save();
			await GameDataManager.Vote.Save();
			isHomeVoteSaved = true;
		}
	}

	private void Update()
	{
		if (isConsumeOpen && !Master.IsOneShotVote)
		{
			votingButton.interactable = useVotingRights > 0;
			votingRightsTextInConsume.text = usableVotingRights.ToString();
			consumeVotingRightsText.text = useVotingRights.ToString();
		}
		if (ResidentFlow.GetInstance().GetTimer().IsEnd() || GameUtility.IsAppTerminatDecided || Cloudstrap.IsDisconnect)
		{
			GameDataManager.Vote.CurrentInfo.PointDiscard();
			CloseWindow().Forget();
		}
	}

	private async UniTask PlayPreview(string itemOID, CancellationTokenSource cts)
	{
		ItemID itemID = CutinItemMaster.GetItemID(itemOID);
		if (CutinItemMaster.GetItem(itemID) == null)
		{
			return;
		}
		Cha sourceCha = Cha.Jicha;
		CutinType cutinType = CutinType.Pon;
		try
		{
			CutinItemInfo item = new CutinItemInfo
			{
				ItemID = itemID,
				IsOn = true
			};
			m_cutinManager.Play(cutinType, item, sourceCha, m_previewPlayerInteractors, null, cts.Token).Forget();
			if (Master.IsCardConnectEnable)
			{
				await SendCardConnect(itemID);
			}
		}
		catch (OperationCanceledException)
		{
			Debug.Log("CutinPreview Canceled");
		}
	}

	private async UniTask SendCardConnect(ItemID id)
	{
		isCardConnectSaved = false;
		MFG.Types.Customize.CutinItem.Item item = CutinItemMaster.GetItem(id);
		if (item != null)
		{
			GameDataManager.CardConnect.m_ccInfos.Add(new CardConnectInfo(GameDataManager.CardConnect.now_idx, "MemorialCard", item.ObjectID, item.ShortName));
			GameDataManager.CardConnect.now_idx++;
			await GameDataManager.CardConnect.Save();
			MemorialPrintNoticeInfo item2 = new MemorialPrintNoticeInfo
			{
				ItemID = id
			};
			GameDataManager.Notice.MemorialPrintNoticeInfos.Add(item2);
		}
		isCardConnectSaved = true;
	}
}
