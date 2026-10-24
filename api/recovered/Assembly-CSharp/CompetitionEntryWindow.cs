using System;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.GameData;
using MFG.GameRequest;
using MFG.Types.Competition;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class CompetitionEntryWindow : MonoBehaviour
{
	[SerializeField]
	private Canvas _canvas;

	[SerializeField]
	private GameObject _entry;

	[SerializeField]
	private GameObject _introduction;

	[SerializeField]
	private GameObject _entryComplete;

	[SerializeField]
	private CompetitionPrizeWindow _prize;

	[SerializeField]
	private CompetitionRuleWindow _ruleDetail;

	[SerializeField]
	private Button _joinButton;

	[SerializeField]
	private Button _notJoinButton;

	[SerializeField]
	private Button _prizeButton;

	[SerializeField]
	private Button _closeButton;

	[SerializeField]
	private Button _ruleDetailButton;

	[SerializeField]
	private GameObject _kacQR;

	public bool m_isOpen;

	private bool m_isOpenAtHome;

	public async UniTask Init(bool isOpenAtHome = false)
	{
		m_isOpenAtHome = isOpenAtHome;
		await EntryWindowInit();
		_prize.Init();
		GameObject gameObject = _ruleDetailButton.gameObject;
		gameObject.SetActive(await _ruleDetail.Init());
	}

	public async UniTask WindowOpen()
	{
		if (m_isOpen)
		{
			return;
		}
		CompetitionInfo inProgressCompetitionInfo = GameDataManager.Competition.GetInProgressCompetitionInfo();
		if (inProgressCompetitionInfo != null)
		{
			base.gameObject.SetActive(value: true);
			_canvas.GetComponent<GraphicRaycaster>().enabled = false;
			m_isOpen = true;
			_entryComplete.SetActive(value: false);
			if (inProgressCompetitionInfo != null && inProgressCompetitionInfo.IsEntried)
			{
				_joinButton.gameObject.SetActive(value: false);
				_notJoinButton.gameObject.SetActive(value: false);
				_closeButton.gameObject.SetActive(value: true);
			}
			else
			{
				_joinButton.gameObject.SetActive(value: true);
				_notJoinButton.gameObject.SetActive(value: true);
				_closeButton.gameObject.SetActive(value: false);
			}
			if (!m_isOpenAtHome)
			{
				_introduction.SetActive(value: true);
				await UniTask.Delay(2000);
			}
			_entry.SetActive(value: true);
			_canvas.GetComponent<GraphicRaycaster>().enabled = true;
			await UniTask.WaitWhile(() => m_isOpen);
			if (m_isOpenAtHome)
			{
				base.gameObject.SetActive(value: false);
			}
		}
	}

	public void WindowClose()
	{
		if (m_isOpen)
		{
			m_isOpen = false;
			_canvas.GetComponent<GraphicRaycaster>().enabled = false;
		}
	}

	private async UniTask EntryWindowInit()
	{
		CompetitionInfo info = GameDataManager.Competition.GetInProgressCompetitionInfo();
		if (info == null)
		{
			return;
		}
		Transform contents = _entry.transform.Find("EntryWindow").Find("Window").Find("Base")
			.Find("Contents");
		_entry.transform.Find("EntryWindow/Window/Base/TitleText").GetComponent<TextMeshProUGUI>().text = (info.IsEntried ? "大会概要" : "大会参加確認");
		IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.ResolveCompetitionAssets(info.CompetitionID, CompetitionAssetType.Title));
		disposableAsset.AddTo(this);
		contents.Find("Title").Find("TitleLogo").GetComponent<Image>()
			.sprite = disposableAsset.Asset;
		_kacQR.SetActive(info.CompetitionID == CompetitionID.Competition003 || info.CompetitionID == CompetitionID.Competition004);
		contents.Find("Period").Find("MainText").GetComponent<TextMeshProUGUI>()
			.text = info.Period;
		contents.Find("Join").Find("MainText").GetComponent<TextMeshProUGUI>()
			.text = info.Join;
		contents.Find("Rule").Find("MainText").GetComponent<TextMeshProUGUI>()
			.text = info.Rule;
		contents.Find("Mode").Find("MainText").GetComponent<TextMeshProUGUI>()
			.text = info.Mode;
		contents.Find("InPrize").Find("MainText").GetComponent<TextMeshProUGUI>()
			.text = info.Remarks;
		_joinButton.OnClickAsObservable().Subscribe(async delegate
		{
			_canvas.GetComponent<GraphicRaycaster>().enabled = false;
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			_entryComplete.SetActive(value: true);
			await UniTask.WaitWhile(() => _entryComplete.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime < 1f);
			CompetitionInfo info2 = GameDataManager.Competition.GetInProgressCompetitionInfo();
			if (info2 != null)
			{
				try
				{
					info2.IsEntried = (await RequestApiBase<CompetitionEntry, CompetitionEntry.ResultData, CompetitionEntry.Param>.Send(new CompetitionEntry.Param
					{
						CompetitionID = info2.CompetitionID,
						IsEntryRequest = true
					}, enableErrorScreenTransition: false)).IsEntried;
				}
				catch (Exception)
				{
					Debug.LogWarning("CompetitionEntry: エントリー通信に失敗しました。");
					info2.IsEntried = false;
				}
				if (DebugFlag.IsActive(DebugFlag.Flag.Competition9ForceActive))
				{
					info2.IsEntried = true;
				}
				else if (info2.IsEntried)
				{
					try
					{
						GameDataManager.Apply(await RequestApiBase<ClientStateRead, ClientStateRead.ResultData, ClientStateRead.Param>.Send(new ClientStateRead.Param
						{
							OneKindType = "competition_game"
						}, enableErrorScreenTransition: false));
					}
					catch
					{
					}
				}
			}
			m_isOpen = false;
		}).AddTo(this);
		_notJoinButton.OnClickAsObservable().Subscribe(async delegate
		{
			_canvas.GetComponent<GraphicRaycaster>().enabled = false;
			SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			m_isOpen = false;
		}).AddTo(this);
		_closeButton.OnClickAsObservable().Subscribe(async delegate
		{
			_canvas.GetComponent<GraphicRaycaster>().enabled = false;
			SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			m_isOpen = false;
		}).AddTo(this);
		_prizeButton.OnClickAsObservable().Subscribe(delegate
		{
			_prize.Open();
		}).AddTo(this);
		_ruleDetailButton.OnClickAsObservable().Subscribe(delegate
		{
			_ruleDetail.Open();
		}).AddTo(this);
	}
}
