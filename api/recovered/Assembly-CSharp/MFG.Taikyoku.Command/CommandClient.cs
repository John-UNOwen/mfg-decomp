using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameRequest;
using MFG.Taikyoku.Command.Receive;
using MFG.Taikyoku.Command.Send;
using UniRx;
using UnityEngine;

namespace MFG.Taikyoku.Command;

[Serializable]
public class CommandClient
{
	private const int CurrentDataVersion = 1;

	public int DataVersion = 1;

	private bool _isReadyToKyokuStartSelf;

	private bool EnableDataTransmission = true;

	protected bool _isReadyToKyokuStartAll;

	[SerializeField]
	protected int m_currentCommandPosition;

	[SerializeField]
	protected List<ReceiveCommand> m_receivedCommands = new List<ReceiveCommand>();

	[SerializeField]
	protected int m_cmdFetchPosition;

	[SerializeField]
	protected TaikyokuStartInfo m_reconnectTaikyokuInfo = new TaikyokuStartInfo();

	[SerializeField]
	protected Subject<MatchingData> m_onMatchingData = new Subject<MatchingData>();

	[SerializeField]
	protected Subject<TaikyokuStartInfo> m_onReconnectData = new Subject<TaikyokuStartInfo>();

	protected Subject<StampData> m_onStampData = new Subject<StampData>();

	protected Subject<(int playerIndex, bool isCpu)> m_onCpuStatus = new Subject<(int, bool)>();

	private bool[] _isDaisous = new bool[4];

	[field: SerializeField]
	public int TakuID { get; set; }

	[field: SerializeField]
	public int PlayerIndex { get; set; }

	[field: SerializeField]
	public int PlayerID { get; set; }

	public IObservable<MatchingData> OnMatchingData => m_onMatchingData.AsObservable();

	public IObservable<TaikyokuStartInfo> OnReconnectData => m_onReconnectData.AsObservable();

	public IObservable<StampData> OnStampData => m_onStampData.AsObservable();

	public IObservable<(int playerIndex, bool isCpu)> OnCpuPlayerChangedData => m_onCpuStatus.AsObservable();

	public bool IsReadyToKyokuStartAll => _isReadyToKyokuStartAll;

	[field: SerializeField]
	public bool IsBusy { get; private set; }

	public IEnumerable<ReceiveCommand> Commands => m_receivedCommands;

	public void NotifyReadyToKyokustart()
	{
		_isReadyToKyokuStartSelf = true;
	}

	public ReceiveCommand Current()
	{
		if (0 <= m_currentCommandPosition && m_currentCommandPosition < m_receivedCommands.Count)
		{
			return m_receivedCommands[m_currentCommandPosition];
		}
		return null;
	}

	public bool MoveNext()
	{
		m_currentCommandPosition++;
		if (m_currentCommandPosition > m_receivedCommands.Count)
		{
			m_currentCommandPosition = m_receivedCommands.Count;
		}
		return m_currentCommandPosition < m_receivedCommands.Count;
	}

	public int ExecutableCount()
	{
		int num = ((m_currentCommandPosition >= 0) ? m_currentCommandPosition : 0);
		return m_receivedCommands.Count - num;
	}

	public async UniTask Fetch(CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (!EnableDataTransmission)
		{
			return;
		}
		if (IsBusy)
		{
			UnityEngine.Debug.LogWarning("CommandClient: Fetch called in Busy");
			await UniTask.WaitWhile(() => IsBusy, PlayerLoopTiming.Update, cancellationToken);
			cancellationToken.ThrowIfCancellationRequested();
		}
		try
		{
			IsBusy = true;
			int receivedCommandCount = 0;
			GGet.ResultData resultData = await RequestApiBase<GGet, GGet.ResultData, (int, int, int, bool, int)>.Send((TakuID, PlayerIndex, PlayerID, _isReadyToKyokuStartSelf, m_cmdFetchPosition), enableErrorScreenTransition: true, enableSendLog: true, cancellationToken);
			MatchingData matching = resultData.Matching;
			List<ReceiveCommand> receivedCommands = resultData.Taikyoku.ReceivedCommands;
			if (m_receivedCommands.Any())
			{
				int maxSeqNo = m_receivedCommands.Max((ReceiveCommand x) => x.SeqNo);
				int num = receivedCommands.RemoveAll((ReceiveCommand x) => x.SeqNo <= maxSeqNo);
				if (num > 0)
				{
					UnityEngine.Debug.Log($" <color=red>Removed Count: {num}</color>");
				}
			}
			if (receivedCommands.Count > 0)
			{
				m_receivedCommands.AddRange(receivedCommands);
				_ = receivedCommandCount + m_receivedCommands.Count;
				m_cmdFetchPosition = resultData.Taikyoku.NextFetchSeqNo;
			}
			ApplyCpu(resultData.Taikyoku.IsDaisou);
			if (matching.PlayerInfos.Exists((PlayerInfo info) => info.IsActive) || matching.IsMatchingEnd)
			{
				UnityEngine.Debug.Log("onMatchingData");
				m_onMatchingData.OnNext(matching);
			}
			StampData stamp = resultData.Stamp;
			if (stamp.Elements.Count > 0)
			{
				m_onStampData.OnNext(stamp);
			}
			if (resultData.IsReadyToKyokuStartAll)
			{
				_isReadyToKyokuStartAll = resultData.IsReadyToKyokuStartAll;
			}
		}
		finally
		{
			IsBusy = false;
		}
	}

	public async UniTask Send(SendCommand tasCmd, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!EnableDataTransmission)
		{
			return;
		}
		try
		{
			IsBusy = true;
			TaikyokuStartInfo reconnect = null;
			TaikyokuData taikyoku;
			MatchingData matching;
			StampData stamp;
			if (tasCmd.Kind == SEND_COMMAND_TYPE.ENTRY)
			{
				GGet.ResultData resultData = await RequestApiBase<GGet, GGet.ResultData, (int, int, int, bool, int)>.Send((TakuID, PlayerIndex, PlayerID, _isReadyToKyokuStartSelf, m_cmdFetchPosition), enableErrorScreenTransition: true, enableSendLog: true, cancellationToken);
				taikyoku = resultData.Taikyoku;
				matching = resultData.Matching;
				stamp = resultData.Stamp;
				ApplyCpu(resultData.Taikyoku.IsDaisou);
			}
			else
			{
				tasCmd.SeqNo = EvaluateGpostSeqNo(tasCmd.Kind);
				GPost.ResultData resultData2 = await RequestApiBase<GPost, GPost.ResultData, (SendCommand, int, int, int)>.Send((tasCmd, TakuID, PlayerIndex, PlayerID), enableErrorScreenTransition: true, enableSendLog: true, cancellationToken);
				taikyoku = resultData2.Taikyoku;
				matching = resultData2.Matching;
				stamp = resultData2.Stamp;
				ApplyCpu(resultData2.Taikyoku.IsDaisou);
				if (resultData2.IsReconnect)
				{
					reconnect = resultData2.Reconnect;
				}
			}
			if (reconnect != null)
			{
				m_currentCommandPosition = 0;
				m_receivedCommands.Clear();
				m_receivedCommands.AddRange(taikyoku.ReceivedCommands);
				m_cmdFetchPosition = reconnect.NextSeqNo;
				m_onReconnectData.OnNext(reconnect);
			}
			else
			{
				List<ReceiveCommand> receivedCommands = taikyoku.ReceivedCommands;
				if (m_receivedCommands.Any())
				{
					int maxSeqNo = m_receivedCommands.Max((ReceiveCommand x) => x.SeqNo);
					int num = receivedCommands.RemoveAll((ReceiveCommand x) => x.SeqNo <= maxSeqNo);
					if (num > 0)
					{
						UnityEngine.Debug.Log($" <color=red>Removed Count: {num}</color>");
					}
				}
				if (receivedCommands.Count > 0)
				{
					m_receivedCommands.AddRange(receivedCommands);
					m_cmdFetchPosition = taikyoku.NextFetchSeqNo;
				}
			}
			if (matching.PlayerInfos.Exists((PlayerInfo info) => info.IsActive) || matching.IsMatchingEnd)
			{
				m_onMatchingData.OnNext(matching);
			}
			if (stamp.Elements.Count > 0)
			{
				m_onStampData.OnNext(stamp);
			}
		}
		finally
		{
			IsBusy = false;
		}
	}

	private int EvaluateGpostSeqNo(SEND_COMMAND_TYPE cmd)
	{
		switch (cmd)
		{
		case SEND_COMMAND_TYPE.RON_AGARI:
		case SEND_COMMAND_TYPE.PON:
		case SEND_COMMAND_TYPE.CHI:
		case SEND_COMMAND_TYPE.MINKAN:
		case SEND_COMMAND_TYPE.NAKINASHI:
			return m_receivedCommands.Where((ReceiveCommand x) => x.Kind == RECEIVE_COMMAND_TYPE.SUTECHOICES).Max((ReceiveCommand x) => x.SeqNo) + 1;
		default:
			return m_cmdFetchPosition;
		}
	}

	public void Reset(int commandFetchPosition = 0)
	{
		m_currentCommandPosition = 0;
		m_receivedCommands.Clear();
		m_cmdFetchPosition = commandFetchPosition;
		EnableDataTransmission = true;
		_isReadyToKyokuStartSelf = false;
		_isReadyToKyokuStartAll = false;
		IsBusy = false;
	}

	public bool Find(Predicate<ReceiveCommand> match)
	{
		if (m_receivedCommands.Find(match) != null)
		{
			return true;
		}
		return false;
	}

	public void Advance(Predicate<ReceiveCommand> match)
	{
		int num = m_receivedCommands.FindIndex(match);
		if (num < 0)
		{
			m_currentCommandPosition = m_receivedCommands.Count;
		}
		else
		{
			m_currentCommandPosition = num;
		}
	}

	public void Apply(HaifuData haifuData, MahjongPlayDebug.PaifuDemoMaster paifu)
	{
		m_currentCommandPosition = haifuData.KyokuIndexList[paifu.KyokuIndex];
		m_cmdFetchPosition = haifuData.NextFetchSeqNo;
		m_receivedCommands = haifuData.ReceivedCommands;
		EnableDataTransmission = false;
	}

	private void ApplyCpu(bool[] isDaisous)
	{
		for (int i = 0; i < isDaisous.Length; i++)
		{
			if (i < _isDaisous.Length && _isDaisous[i] != isDaisous[i])
			{
				m_onCpuStatus.OnNext((i, isDaisous[i]));
			}
		}
		_isDaisous = isDaisous;
	}

	[Conditional("DEBUG")]
	private void Log_ReceivedCommands(List<ReceiveCommand> cmds)
	{
		if (!DebugFlag.IsActive(DebugFlag.Flag.EnableCommandClientLog))
		{
			return;
		}
		UnityEngine.Debug.Log($" <color=green>ReceivedCommands Count: {cmds.Count}</color>");
		foreach (ReceiveCommand cmd in cmds)
		{
			UnityEngine.Debug.Log($"  <color=green>seqNo:{cmd.SeqNo}: Kind:{cmd.Kind} PindexFlag:{cmd.PlayerIndexFlag}</color>");
		}
	}
}
