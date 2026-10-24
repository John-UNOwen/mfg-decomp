using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using MFG.MasterGameData;
using UnityEngine;

internal class CharaReactionManager : MonoBehaviour
{
	private struct RequestData
	{
		public Player player;

		public CharaReactionData.ReactionID id;

		public int count;

		public RequestData(Player _player, CharaReactionData.ReactionID _id, int _count)
		{
			player = _player;
			id = _id;
			count = _count;
		}
	}

	private struct RequestVoiceData
	{
		public Player player;

		public VoiceManager.ReactionVoiceID id;

		public RequestVoiceData(Player _player, VoiceManager.ReactionVoiceID _id)
		{
			player = _player;
			id = _id;
		}
	}

	private static CharaReactionManager m_instance = null;

	private List<Dictionary<CharaReactionData.ReactionID, int>> m_playCounts = new List<Dictionary<CharaReactionData.ReactionID, int>>();

	private List<RequestData> m_reactionRequest = new List<RequestData>();

	private List<RequestVoiceData> m_reactionVoiceRequest = new List<RequestVoiceData>();

	private RequestData _sutehaiOwnReaction;

	private RequestData _sutehaiOtherPlayerReaction;

	private static RandomValueGenerator m_randomValueReactionVoice = new RandomValueGenerator();

	public static CharaReactionManager GetInstance()
	{
		if (!m_instance)
		{
			m_instance = GameObject.Find("CharaReactionManager").GetComponent<CharaReactionManager>();
		}
		return m_instance;
	}

	private void Awake()
	{
		m_playCounts.Clear();
		for (int i = 0; i < 4; i++)
		{
			m_playCounts.Add(new Dictionary<CharaReactionData.ReactionID, int>());
			foreach (CharaReactionData.ReactionID value in Enum.GetValues(typeof(CharaReactionData.ReactionID)))
			{
				m_playCounts[i].Add(value, 0);
			}
		}
	}

	public void TurnInit(int seqNo = 0)
	{
		DebugLog_TurnInit();
		m_reactionRequest.Clear();
		m_reactionVoiceRequest.Clear();
		_sutehaiOwnReaction.id = CharaReactionData.ReactionID.None;
		_sutehaiOtherPlayerReaction.id = CharaReactionData.ReactionID.None;
		SetReactionVoiceRandomSeed(GameDataManager.Taikyoku.TakuInfo.TakuID + seqNo);
	}

	public void TurnExec()
	{
		DebugLog_RequestDump("TurnExec");
		if (m_reactionRequest.Count > 0)
		{
			List<RequestData> source = m_reactionRequest.OrderBy((RequestData a) => a.count).ToList();
			ReactionExec(source.First());
		}
		m_reactionRequest.Clear();
		_sutehaiOwnReaction.id = CharaReactionData.ReactionID.None;
		_sutehaiOtherPlayerReaction.id = CharaReactionData.ReactionID.None;
		if (m_reactionVoiceRequest.Count > 0)
		{
			int index = m_randomValueReactionVoice.Range(0, m_reactionVoiceRequest.Count);
			ReactionVoiceExec(m_reactionVoiceRequest[index]);
		}
		m_reactionVoiceRequest.Clear();
	}

	public void SutehaiReaction_Decide(int sutePlayerIndex)
	{
		DebugLog_RequestDump("SutehaiReaction_Decide");
		_sutehaiOwnReaction.id = CharaReactionData.ReactionID.None;
		_sutehaiOtherPlayerReaction.id = CharaReactionData.ReactionID.None;
		if (m_reactionRequest.Count <= 0)
		{
			return;
		}
		try
		{
			_sutehaiOwnReaction = (from a in m_reactionRequest
				where a.player.m_playerIndexAtCommand == sutePlayerIndex
				orderby a.count
				select a).First();
		}
		catch
		{
		}
		try
		{
			_sutehaiOtherPlayerReaction = (from a in m_reactionRequest
				where a.player.m_playerIndexAtCommand != sutePlayerIndex
				orderby a.count
				select a).First();
		}
		catch
		{
		}
	}

	public void SutehaiReaction_ExecOwn()
	{
		DebugLog_RequestDump("SutehaiReaction_ExecOwn");
		if (_sutehaiOwnReaction.id != CharaReactionData.ReactionID.None)
		{
			ReactionExec(_sutehaiOwnReaction);
			m_reactionRequest.Clear();
			_sutehaiOwnReaction.id = CharaReactionData.ReactionID.None;
			_sutehaiOtherPlayerReaction.id = CharaReactionData.ReactionID.None;
		}
	}

	public void SutehaiReaction_ExecOther()
	{
		DebugLog_RequestDump("SutehaiReaction_ExecOther");
		if (_sutehaiOtherPlayerReaction.id != CharaReactionData.ReactionID.None)
		{
			ReactionExec(_sutehaiOtherPlayerReaction);
			m_reactionRequest.Clear();
			_sutehaiOwnReaction.id = CharaReactionData.ReactionID.None;
			_sutehaiOtherPlayerReaction.id = CharaReactionData.ReactionID.None;
		}
	}

	private void ReactionExec(RequestData data)
	{
		Player player = data.player;
		CharaReactionData.ReactionID id = data.id;
		DebugLog_Request(player, id, "ReactionExec");
		player.character.m_charaReaction.Reaction(id);
		m_playCounts[player.m_index][id]++;
	}

	public void ReactionRequest(Player player, CharaReactionData.ReactionID id)
	{
		DebugLog_Request(player, id, "ReactionRequest");
		int count = m_playCounts[player.m_index][id];
		if ((id != CharaReactionData.ReactionID.Tsumogiri3 && id != CharaReactionData.ReactionID.UrameHai) || UnityEngine.Random.Range(0, 100) < 50)
		{
			m_reactionRequest.Add(new RequestData(player, id, count));
		}
	}

	private void ReactionVoiceExec(RequestVoiceData data)
	{
		Player player = data.player;
		switch (data.id)
		{
		case VoiceManager.ReactionVoiceID.Comment1:
			VoiceManager.PlayVoiceComment1(player).Forget();
			break;
		case VoiceManager.ReactionVoiceID.Comment2:
			VoiceManager.PlayVoiceComment2(player);
			break;
		case VoiceManager.ReactionVoiceID.Comment3:
			VoiceManager.PlayVoiceComment3(player);
			break;
		case VoiceManager.ReactionVoiceID.Comment4:
			VoiceManager.PlayVoiceComment4(player);
			break;
		case VoiceManager.ReactionVoiceID.Comment5:
			VoiceManager.PlayVoiceComment5(player);
			break;
		case VoiceManager.ReactionVoiceID.Comment6:
			VoiceManager.PlayVoiceComment6(player);
			break;
		case VoiceManager.ReactionVoiceID.Comment7:
			VoiceManager.PlayVoiceComment7(player);
			break;
		}
	}

	private void DebugLog_TurnInit()
	{
		if (DebugFlag.IsActive(DebugFlag.Flag.EnableCharaActionLog))
		{
			Debug.Log($"<color=magenta>TurnInit. Request Count:{m_reactionRequest.Count}, Voice Count:{m_reactionVoiceRequest.Count}</color>");
		}
	}

	private void DebugLog_Request(Player player, CharaReactionData.ReactionID id, [CallerMemberName] string memberName = "")
	{
		if (DebugFlag.IsActive(DebugFlag.Flag.EnableCharaActionLog))
		{
			Debug.Log($"<color=magenta>{memberName}. Index:{player.m_index}, Cha:{player.m_cha}, ID:{id}</color>");
		}
	}

	private void DebugLog_RequestDump([CallerMemberName] string memberName = "")
	{
		if (!DebugFlag.IsActive(DebugFlag.Flag.EnableCharaActionLog))
		{
			return;
		}
		Debug.Log($"<color=magenta>{memberName}. Request Count:{m_reactionRequest.Count}</color>");
		foreach (RequestData item in m_reactionRequest)
		{
			Debug.Log($" <color=magenta>{item.player.m_cha}, {item.id}</color>");
		}
	}

	public void ReactionVoiceRequest(Player player, VoiceManager.ReactionVoiceID id)
	{
		m_reactionVoiceRequest.Add(new RequestVoiceData(player, id));
	}

	public static void SetReactionVoiceRandomSeed(int seed)
	{
		m_randomValueReactionVoice.Init(seed);
	}
}
