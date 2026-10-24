using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.MahjongPlay.Character;
using MFG.Types;
using UnityEngine;

namespace MFG.MasterGameData;

public class CharaReactionData
{
	public enum ReactionID
	{
		Naki,
		NakiDora,
		IppatsuBlock,
		Tsumogiri3,
		Reach,
		ReachFirst,
		OkkakeReach,
		KikenHai,
		AnzenHai,
		UrameHai,
		HadakaTanki,
		TyunBeam,
		TyunBeam2,
		TyunBeam3,
		TyunBeam4,
		TyunBeamOther,
		NewDoraMeld,
		DoraSute,
		FewLeft,
		TaikyokuStart,
		TaikyokuStartOrasu,
		TaikyokuStartTop,
		TaikyokuStartBottom,
		DoraSuteOther,
		NakiOther,
		KikenHaiOther,
		ReachOther,
		OkkakeReachOther,
		NakiSkip,
		IppatsuBlockOther,
		Tyoukou,
		TyoukouEnemyReach,
		None
	}

	public class ReactionData
	{
		public ReactionID ID;

		public bool executed;

		public string activateConditionText;

		public MotionID motionID;

		public string backScreenCharaTextureName;

		public int delayMilliSec;

		public int priority;

		public void Parse(string[] values)
		{
			ID = String2ReacionID(values[0]);
			activateConditionText = values[1];
			motionID = String2MotionID(values[2]);
			backScreenCharaTextureName = values[3];
			int.TryParse(values[4], out delayMilliSec);
			int.TryParse(values[5], out priority);
		}

		private ReactionID String2ReacionID(string reacion)
		{
			for (int i = 0; i < Enum.GetNames(typeof(ReactionID)).Length; i++)
			{
				if (Enum.GetName(typeof(ReactionID), (ReactionID)i) == reacion)
				{
					return (ReactionID)i;
				}
			}
			return ReactionID.None;
		}

		private MotionID String2MotionID(string motionIDStr)
		{
			if (Enum.TryParse<MotionID>(motionIDStr, ignoreCase: true, out var result) && Enum.IsDefined(typeof(MotionID), result))
			{
				return result;
			}
			return MotionID.None;
		}

		public async UniTask ExecDelay(CancellationToken cancellationToken = default(CancellationToken))
		{
			if (delayMilliSec > 0)
			{
				await UniTask.Delay(delayMilliSec, ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
			}
		}
	}

	public List<ReactionData> m_reactionDatas = new List<ReactionData>();

	public void Load(CharaType type)
	{
		m_reactionDatas.Clear();
		using StringReader stringReader = new StringReader(Resources.Load<TextAsset>("CharaData/CharaReactionTable_" + ((int)(type + 1)).ToString("00")).text);
		stringReader.ReadLine();
		while (stringReader.Peek() > -1)
		{
			string[] array = stringReader.ReadLine().Split(',');
			if (!string.IsNullOrEmpty(array[0]))
			{
				ReactionData reactionData = new ReactionData();
				reactionData.Parse(array);
				m_reactionDatas.Add(reactionData);
			}
		}
	}
}
