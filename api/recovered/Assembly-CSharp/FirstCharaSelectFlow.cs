using System;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using Konami.System.Net;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class FirstCharaSelectFlow : BaseFlow
{
	[SerializeField]
	private Canvas _canvas;

	[SerializeField]
	private CharaSelectWindow _charaSelectWindow;

	[SerializeField]
	private GameObject m_charaSelectAll;

	[SerializeField]
	private GameObject m_charaData;

	[SerializeField]
	private GameObject m_charaMove;

	[SerializeField]
	private StandingCharaManager m_charaManager;

	[SerializeField]
	private GameObject m_copyright;

	[SerializeField]
	private SchoolEmblemIcon _schoolEmblemIcon;

	[SerializeField]
	private GameObject _myRoomGuide;

	private bool m_isDecided;

	protected override async UniTask OnInitialize(CancellationToken cancellationToken = default(CancellationToken))
	{
		await _charaSelectWindow.Init();
		ResidentFlow.GetInstance().GetHeader().SetDisp(disp: true);
		ResidentFlow.GetInstance().GetHeader().SetStatusDisp(disp: false);
		ResidentFlow.GetInstance().GetHeader().SetSceneName("ガール選択");
		SoundManager.PlayMyCharaMyRoomBGM().Forget();
		_myRoomGuide.SetActive(!GameDataManager.Account.IsGuestUser);
		try
		{
			ResidentFlow.GetInstance().GetTimer().SetTimer(60f, disp: true);
			ResidentFlow.GetInstance().GetTimer().StartTimer();
			base.OnTimerEnd.Subscribe(async delegate
			{
				await Decide();
			}).AddTo(this);
			_charaSelectWindow.OnCharaChange.Subscribe(async delegate(CharaType charaType)
			{
				if (!m_isDecided)
				{
					await ChangeSelectChara(charaType);
				}
			}).AddTo(this);
			await ChangeSelectChara(CharaType.Chara01);
		}
		catch (OperationCanceledException)
		{
			Debug.Log(GetType().Name + MethodBase.GetCurrentMethod().Name + ": OperationCanceledException Catched.");
		}
	}

	protected override async UniTask OnExecute(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		try
		{
			m_charaSelectAll.GetComponent<Animator>().SetTrigger("In");
			await _charaSelectWindow.Play(CharaType.Chara01, dispoffWhenDecided: false);
			await Decide();
		}
		catch (OperationCanceledException)
		{
			Debug.Log(GetType().Name + MethodBase.GetCurrentMethod().Name + ": OperationCanceledException Catched.");
		}
	}

	private async UniTask ChangeSelectChara(CharaType charaType, bool isEnableCharaVoiceSE = true)
	{
		_charaSelectWindow.m_isSelecting = true;
		GameDataManager.Player.SelectChara = charaType;
		Transform trs = m_charaData.transform;
		string text = MasterGameDataManager.Character.Properties(charaType).Profile.Name;
		trs.Find("CharaNameText").GetComponent<TextMeshProUGUI>().text = text;
		await _schoolEmblemIcon.Reflesh(MasterGameDataManager.Character.Properties(charaType).Profile.SchoolID);
		string cV = MasterGameDataManager.Character.Properties(charaType).Profile.CV;
		trs.Find("Cv").GetComponent<Text>().text = cV;
		string profileText = MasterGameDataManager.Character.Properties(charaType).Profile.ProfileText;
		trs.Find("CharaProfile").GetComponent<TextMeshProUGUI>().text = profileText;
		m_copyright.SetActive(charaType == CharaType.Chara10);
		m_charaMove.GetComponent<Animator>().SetTrigger("Out");
		await m_charaManager.SingleCharaLoad(CostumeItemMaster.GetDefaultEquipItemID(charaType));
		await UniTask.WaitWhile(() => m_charaMove.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("CharaMoveOut"));
		m_charaManager.SetExclusiveDisp(charaType);
		m_charaMove.GetComponent<Animator>().ResetTrigger("Out");
		m_charaMove.GetComponent<Animator>().SetTrigger("In");
		if (isEnableCharaVoiceSE)
		{
			SoundManager.StopVoiceAll().Forget();
			SoundManager.PlayVoice("VOX_GXX_APPEAL1", charaType).Forget();
		}
		_charaSelectWindow.m_isSelecting = false;
	}

	private async UniTask Decide()
	{
		if (m_isDecided)
		{
			return;
		}
		m_isDecided = true;
		if (!GameDataManager.IsCharaSelectable(GameDataManager.Player.SelectChara))
		{
			await ChangeSelectChara(CharaType.Chara01, isEnableCharaVoiceSE: false);
		}
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
		_canvas.GetComponent<GraphicRaycaster>().enabled = false;
		m_charaManager.IsTouchReactionEnable = false;
		SoundManager.StopVoiceAll().Forget();
		SoundManager.PlayVoice("VOX_GXX_APPEAL2_2", GameDataManager.Player.SelectChara).Forget();
		CancellationToken cancellationTokenOnDestroy = this.GetCancellationTokenOnDestroy();
		try
		{
			foreach (CharaGameData charaDatum in GameDataManager.Player.CharaData)
			{
				charaDatum.ResetAllEqipToDefault();
			}
			await GameDataManager.Player.Save(cancellationTokenOnDestroy);
		}
		catch (OperationCanceledException ex)
		{
			Debug.Log("OperationCanceledException Catched. " + ex.Message);
		}
		catch (NetworkResponseException ex2)
		{
			GameDataManager.PlayerConfig.IsDataSaveFailed = true;
			Debug.Log("NetworkResponseException Catched. " + ex2.Message);
		}
		await UniTask.WaitWhile(() => SoundManager.IsPlayLastCalledVoice());
		if (!GameDataManager.Account.IsGuestUser)
		{
			if (GameDataManager.Story.JudgeAndSetEventStoryPlay())
			{
				ResidentFlow.GetInstance().LoadScene("StoryPlay");
				return;
			}
			Debug.Log("イベントストーリー持ってるけど、「イベント開催していない」or「イベントストーリーもう読んだ」のでHomeに飛ばす");
			ResidentFlow.GetInstance().LoadScene(GameDataManager.Competition.GetNextSceneWhenGotoHome());
		}
		else
		{
			ResidentFlow.GetInstance().LoadScene(GameDataManager.Competition.GetNextSceneWhenGotoHome());
		}
	}
}
