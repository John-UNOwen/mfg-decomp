using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameRequest;
using UnityEngine;

namespace MFG.GameData;

[Serializable]
public class CardConnectGameData
{
	private const int CurrentDataVersion = 1;

	public int DataVersion = 1;

	public const string ClientStateName = "cc_request";

	public int now_idx = 1;

	public int done_idx;

	public List<CardConnectInfo> m_ccInfos = new List<CardConnectInfo>();

	public async UniTask Save(CancellationToken cancellationToken = default(CancellationToken))
	{
		string text = JsonUtility.ToJson(this);
		Debug.Log("CardConnectGameData Save : " + text);
		await RequestApiBase<ClientStateWrite, ClientStateWrite.ResultData, (string, string)>.Send(("cc_request", text), enableErrorScreenTransition: false, enableSendLog: true, cancellationToken);
	}

	public static CardConnectGameData LoadFromJson(string jsonString)
	{
		Debug.Log("CardConnectGameData Load : " + jsonString);
		CardConnectGameData cardConnectGameData = JsonUtility.FromJson<CardConnectGameData>(jsonString);
		if (cardConnectGameData == null)
		{
			cardConnectGameData = new CardConnectGameData();
		}
		cardConnectGameData.Validate();
		Debug.Log("CardConnectGameData now_idx -> " + cardConnectGameData.now_idx + "  ,  done_idx -> " + cardConnectGameData.done_idx + "  ,  m_ccInfos -> " + string.Join(',', cardConnectGameData.m_ccInfos));
		return cardConnectGameData;
	}

	private void Validate()
	{
		List<CardConnectInfo> list = new List<CardConnectInfo>();
		foreach (CardConnectInfo ccInfo in m_ccInfos)
		{
			if (ccInfo.idx <= done_idx)
			{
				list.Add(ccInfo);
			}
		}
		foreach (CardConnectInfo item in list)
		{
			m_ccInfos.Remove(item);
		}
		now_idx = done_idx + m_ccInfos.Count + 1;
	}
}
