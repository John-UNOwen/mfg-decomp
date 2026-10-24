using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.GameRequest;
using MFG.Taikyoku;
using MFG.Taikyoku.Command;
using MFG.Taikyoku.Command.Receive;
using MFG.Taikyoku.Command.Send;
using MFG.Types;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TsumoGiriTestFlow : MonoBehaviour
{
	private CommandClient m_commandClient = new CommandClient();

	private Text m_taikyokusuText;

	private Text m_taikyokuProgressText;

	private Text m_taikyokuInfoText;

	private Text m_taikyokuNokoriHaiText;

	private Text m_timeText;

	private Button m_endButton;

	private int m_kyokuNum;

	private int m_myTsumoHaiID;

	private int m_nokoriHaiNum;

	private bool m_isTaikyokuActive;

	private bool m_isKikenActive;

	private bool m_isKikenRequest;

	private bool m_isEndRequest;

	private int m_step;

	private int m_subStep;

	private EntryTakuInfo m_entryTakuInfo => GameDataManager.Taikyoku.TakuInfo;

	private AccountGameData m_accountGameData => GameDataManager.Account;

	private async void Start()
	{
		await UniTask.WaitUntil(() => ResidentFlow.IsInitialized);
		CancellationToken token = this.GetCancellationTokenOnDestroy();
		m_taikyokusuText = GameObject.Find("TaikyokusuText").GetComponent<Text>();
		m_taikyokuInfoText = GameObject.Find("TaikyokuInfoText").GetComponent<Text>();
		m_taikyokuNokoriHaiText = GameObject.Find("TaikyokuNokoriHaiText").GetComponent<Text>();
		m_taikyokuProgressText = GameObject.Find("TaikyokuProgressText").GetComponent<Text>();
		m_timeText = GameObject.Find("TimeText").GetComponent<Text>();
		m_endButton = GameObject.Find("EndButton").GetComponent<Button>();
		bool isLoopActive = true;
		while (isLoopActive)
		{
			m_kyokuNum++;
			m_taikyokusuText.text = $"{m_kyokuNum}局目";
			EntryGame.ResultData resultData = await RequestApiBase<EntryGame, EntryGame.ResultData>.Send(enableErrorScreenTransition: true, enableSendLog: true, token);
			if (!m_isEndRequest)
			{
				GameDataManager.Taikyoku.Apply(resultData.TakuInfo);
				m_commandClient.Reset(m_entryTakuInfo.NextSeqNo);
				m_commandClient.TakuID = m_entryTakuInfo.TakuID;
				m_commandClient.PlayerIndex = m_entryTakuInfo.PlayerIndex;
				m_commandClient.PlayerID = m_accountGameData.PlayerID;
				int machingLimitTime = GameDataManager.Taikyoku.TakuInfo.MachingLimitTime;
				ResidentFlow.GetInstance().GetTimer().SetTimer(machingLimitTime, disp: true);
				ResidentFlow.GetInstance().GetTimer().StartTimer();
				await m_commandClient.Send(new EntryOK(), token);
				if (!m_isEndRequest)
				{
					m_isTaikyokuActive = true;
					m_isKikenRequest = false;
					await UniTask.WaitUntil(() => !m_isTaikyokuActive);
					if (!m_isKikenRequest)
					{
						await RequestApiBase<EndGame, EndGame.ResultData, (int, int, int)>.Send((m_entryTakuInfo.TakuID, m_entryTakuInfo.PlayerIndex, m_accountGameData.PlayerID));
					}
					else
					{
						await UniTask.WaitUntil(() => !m_commandClient.IsBusy);
						await m_commandClient.Send(new Kiken());
						await RequestApiBase<KikenGame, KikenGame.ResultData, (int, int, int)>.Send((m_entryTakuInfo.TakuID, m_entryTakuInfo.PlayerIndex, m_accountGameData.PlayerID));
					}
				}
			}
			if (m_isEndRequest)
			{
				isLoopActive = false;
				await SceneManager.UnloadSceneAsync(base.gameObject.scene);
			}
		}
	}

	public void Onclick_End()
	{
		m_isEndRequest = true;
		m_isTaikyokuActive = false;
		if (m_isKikenActive)
		{
			m_isKikenRequest = true;
			m_isKikenActive = false;
		}
		m_endButton.interactable = false;
	}

	private void Update()
	{
		if (!ResidentFlow.IsInitialized)
		{
			return;
		}
		m_timeText.text = DateTime.Now.ToString();
		if (!m_isTaikyokuActive)
		{
			return;
		}
		switch (m_step)
		{
		case 0:
			if (m_commandClient.ExecutableCount() <= 0)
			{
				m_commandClient.Fetch().Forget();
			}
			m_step++;
			break;
		case 1:
			if (!m_commandClient.IsBusy)
			{
				m_step++;
			}
			break;
		case 2:
		{
			if (m_commandClient.ExecutableCount() <= 0)
			{
				m_step = 0;
				break;
			}
			ReceiveCommand receiveCommand = m_commandClient.Current();
			m_taikyokuProgressText.text = $"{receiveCommand.Kind} {receiveCommand.SeqNo}";
			bool flag = true;
			switch (receiveCommand.Kind)
			{
			case RECEIVE_COMMAND_TYPE.KYOKUSTART:
				ExecKyokuStart((KyokuStart)receiveCommand);
				break;
			case RECEIVE_COMMAND_TYPE.TSUMO:
				ExecTsumo((Tsumo)receiveCommand);
				break;
			case RECEIVE_COMMAND_TYPE.TSUMOCHOICES:
				flag = ExecTsumoChoices((TsumoChoices)receiveCommand);
				break;
			case RECEIVE_COMMAND_TYPE.SUTECHOICES:
				flag = ExecSuteChoices((SuteChoices)receiveCommand);
				break;
			case RECEIVE_COMMAND_TYPE.KYOKUEND:
				flag = ExecKyokuEnd((KyokuEnd)receiveCommand);
				break;
			}
			if (flag)
			{
				m_commandClient.MoveNext();
			}
			break;
		}
		}
	}

	private void ExecKyokuStart(KyokuStart cmd)
	{
		string arg = ((cmd.Ba == 0) ? "東" : "南");
		m_taikyokuInfoText.text = $"{arg}{cmd.Kyoku + 1}局{cmd.Honba}本場";
		m_nokoriHaiNum = cmd.Nokori;
		m_taikyokuNokoriHaiText.text = $"残り {m_nokoriHaiNum}";
		m_isKikenActive = true;
	}

	private void ExecTsumo(Tsumo cmd)
	{
		m_nokoriHaiNum--;
		m_taikyokuNokoriHaiText.text = $"残り {m_nokoriHaiNum}";
		if (cmd.PlayerIndexFlag.HasFlag((PLAYER_INDEX_FLAG)(1 << m_entryTakuInfo.PlayerIndex)))
		{
			m_myTsumoHaiID = cmd.PaiID;
		}
	}

	private bool ExecTsumoChoices(TsumoChoices cmd)
	{
		switch (m_subStep)
		{
		case 0:
			if (!cmd.PlayerIndexFlag.HasFlag((PLAYER_INDEX_FLAG)(1 << m_entryTakuInfo.PlayerIndex)))
			{
				m_subStep = 0;
				return true;
			}
			if (!cmd.Choices.SelectableTypeFlag.HasFlag(SELECTABLE_TYPE_FLAG.SUTE))
			{
				m_subStep = 0;
				return true;
			}
			m_subStep++;
			break;
		case 1:
			m_commandClient.Send(new MFG.Taikyoku.Command.Send.Sutehai(13, m_myTsumoHaiID, bRichi: false, bTsumoGiri: true, ReachLevel.Normal)).Forget();
			m_subStep++;
			break;
		case 2:
			if (!m_commandClient.IsBusy)
			{
				m_subStep = 0;
				return true;
			}
			break;
		}
		return false;
	}

	private bool ExecSuteChoices(SuteChoices cmd)
	{
		switch (m_subStep)
		{
		case 0:
			if (!cmd.PlayerIndexFlag.HasFlag((PLAYER_INDEX_FLAG)(1 << m_entryTakuInfo.PlayerIndex)))
			{
				m_subStep = 0;
				return true;
			}
			m_subStep++;
			break;
		case 1:
			m_commandClient.Send(new Tsugi()).Forget();
			m_subStep++;
			break;
		case 2:
			if (!m_commandClient.IsBusy)
			{
				m_subStep = 0;
				return true;
			}
			break;
		}
		return false;
	}

	private bool ExecKyokuEnd(KyokuEnd cmd)
	{
		switch (m_subStep)
		{
		case 0:
		{
			if (cmd.IsEnd)
			{
				m_isTaikyokuActive = false;
				m_subStep = 0;
				m_isKikenActive = false;
				return true;
			}
			NextKyokuReady tasCmd = new NextKyokuReady();
			m_commandClient.Send(tasCmd).Forget();
			m_isKikenActive = false;
			m_subStep++;
			break;
		}
		case 1:
			if (!m_commandClient.IsBusy)
			{
				m_subStep = 0;
				return true;
			}
			break;
		}
		return false;
	}
}
