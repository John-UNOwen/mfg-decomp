using UnityEngine;

namespace MFG.Taikyoku.Command.Receive;

public class ReceiveCommandFactory
{
	public static ReceiveCommand Create(RECEIVE_COMMAND_TYPE commandType, int seqNo)
	{
		ReceiveCommand result = null;
		switch (commandType)
		{
		case RECEIVE_COMMAND_TYPE.TSUMO:
			result = new Tsumo(RECEIVE_COMMAND_TYPE.TSUMO, seqNo);
			break;
		case RECEIVE_COMMAND_TYPE.SUTEHAI:
			result = new Sutehai(RECEIVE_COMMAND_TYPE.SUTEHAI, seqNo);
			break;
		case RECEIVE_COMMAND_TYPE.TSUMOAGARI:
			result = new TsumoAgari(RECEIVE_COMMAND_TYPE.TSUMOAGARI, seqNo);
			break;
		case RECEIVE_COMMAND_TYPE.RON:
			result = new Ron(RECEIVE_COMMAND_TYPE.RON, seqNo);
			break;
		case RECEIVE_COMMAND_TYPE.RYUKYOKU:
			result = new RyuKyoku(RECEIVE_COMMAND_TYPE.RYUKYOKU, seqNo);
			break;
		case RECEIVE_COMMAND_TYPE.PON:
			result = new Pon(RECEIVE_COMMAND_TYPE.PON, seqNo);
			break;
		case RECEIVE_COMMAND_TYPE.CHI:
			result = new Chi(RECEIVE_COMMAND_TYPE.CHI, seqNo);
			break;
		case RECEIVE_COMMAND_TYPE.ANKAN:
			result = new Ankan(RECEIVE_COMMAND_TYPE.ANKAN, seqNo);
			break;
		case RECEIVE_COMMAND_TYPE.MINKAN:
			result = new Minkan(RECEIVE_COMMAND_TYPE.MINKAN, seqNo);
			break;
		case RECEIVE_COMMAND_TYPE.KAKAN:
			result = new Kakan(RECEIVE_COMMAND_TYPE.KAKAN, seqNo);
			break;
		case RECEIVE_COMMAND_TYPE.TYOKO:
			result = new TyoKo(RECEIVE_COMMAND_TYPE.TYOKO, seqNo);
			break;
		case RECEIVE_COMMAND_TYPE.TSUMOCHOICES:
			result = new TsumoChoices(RECEIVE_COMMAND_TYPE.TSUMOCHOICES, seqNo);
			break;
		case RECEIVE_COMMAND_TYPE.SUTECHOICES:
			result = new SuteChoices(RECEIVE_COMMAND_TYPE.SUTECHOICES, seqNo);
			break;
		case RECEIVE_COMMAND_TYPE.KYOKUSTART:
			result = new KyokuStart(RECEIVE_COMMAND_TYPE.KYOKUSTART, seqNo);
			break;
		case RECEIVE_COMMAND_TYPE.KYOKUEND:
			result = new KyokuEnd(RECEIVE_COMMAND_TYPE.KYOKUEND, seqNo);
			break;
		case RECEIVE_COMMAND_TYPE.SCORERANK:
			result = new ScoreRank(RECEIVE_COMMAND_TYPE.SCORERANK, seqNo);
			break;
		default:
			Debug.Log($"ReceiveCommandFactory Create: cmd not found {(int)commandType}");
			break;
		}
		return result;
	}
}
