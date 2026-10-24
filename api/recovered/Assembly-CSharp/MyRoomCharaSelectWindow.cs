using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Types;
using UniRx;
using UnityEngine;

public class MyRoomCharaSelectWindow : MonoBehaviour
{
	[SerializeField]
	private CharaSelectWindow _charaSelectWindow;

	private MyRoomFlow _myRoomFlow;

	protected void Start()
	{
		CancellationToken cancellationToken = this.GetCancellationTokenOnDestroy();
		_charaSelectWindow.OnCharaChange.Subscribe(async delegate(CharaType charaType)
		{
			_charaSelectWindow.m_isSelecting = true;
			try
			{
				if (GameDataManager.Player.SelectChara != charaType)
				{
					GameDataManager.Mission.IsChangeChara = true;
				}
				GameDataManager.Player.SelectChara = charaType;
				GameDataManager.YellCampaign.RefreshYellRank();
				_myRoomFlow.m_charaMove.GetComponent<Animator>().SetTrigger("Out");
				await UniTask.WaitWhile(() => _myRoomFlow.m_charaMove.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("CharaMoveOut"));
				_myRoomFlow.CharaStatusRefresh(cancellationToken);
				await _myRoomFlow.m_charaManager.SingleCharaLoad(GameDataManager.Player.GetCurrentChara().CostumeItemEquip);
				_myRoomFlow.m_charaMove.GetComponent<Animator>().ResetTrigger("Out");
				_myRoomFlow.m_charaMove.GetComponent<Animator>().SetTrigger("In");
				SoundManager.StopVoiceAll().Forget();
				SoundManager.PlayVoice("VOX_GXX_APPEAL1", charaType).Forget();
			}
			catch (OperationCanceledException)
			{
			}
			_charaSelectWindow.m_isSelecting = false;
		}).AddTo(this);
	}

	public async UniTask Init(MyRoomFlow mrf, CancellationToken cancellationToken)
	{
		_myRoomFlow = mrf;
		await _charaSelectWindow.Init(cancellationToken);
	}

	public async UniTask Play()
	{
		await _charaSelectWindow.Play(GameDataManager.Player.SelectChara);
		SoundManager.PlayMyCharaMyRoomBGM().Forget();
	}
}
