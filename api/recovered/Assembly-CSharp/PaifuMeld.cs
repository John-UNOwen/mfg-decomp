using System.Collections.Generic;
using System.Linq;
using MFG.Taikyoku.Command;
using MFG.Taikyoku.Command.Receive;
using MFG.Types;

public class PaifuMeld
{
	public PaifuMeldType MeldType;

	public HaiID MeldHaiID;

	public HaiID KakanHaiID = HaiID.None;

	public List<HaiID> HaiIDs = new List<HaiID>();

	public HaiID MeldNormalHaiID => MeldHaiID.ToNormal();

	public bool IsKakan
	{
		get
		{
			if (MeldType >= PaifuMeldType.KaKanShimo && MeldType <= PaifuMeldType.KaKanKami)
			{
				return KakanHaiID != HaiID.None;
			}
			return false;
		}
	}

	public bool IsDaiMinkan
	{
		get
		{
			if (MeldType >= PaifuMeldType.MinKanShimo)
			{
				return MeldType <= PaifuMeldType.MinKanKami;
			}
			return false;
		}
	}

	public PaifuMeld(ReceiveCommand cmd, PaifuPlayerData player, PaifuPlayerData meldedPlayer)
	{
		Cha cha = ((meldedPlayer != null) ? ((Cha)((meldedPlayer.JiKaze + 4 - player.JiKaze) % 4)) : Cha.Jicha);
		switch (cmd.Kind)
		{
		case RECEIVE_COMMAND_TYPE.PON:
			MeldType = (PaifuMeldType)(4 + cha - 1);
			MeldHaiID = ((Pai)((Pon)cmd).PaiID).ToHaiID();
			meldedPlayer.Dumps.Last().SetMeld();
			{
				foreach (HaiID item in ((Pon)cmd).PonPaiID.Select((int x) => ((Pai)x).ToHaiID()))
				{
					HaiIDs.Add(item);
					player.TehaiIDs.Remove(item);
				}
				break;
			}
		case RECEIVE_COMMAND_TYPE.CHI:
			MeldType = (PaifuMeldType)(1 + cha - 1);
			MeldHaiID = ((Pai)((Chi)cmd).PaiID).ToHaiID();
			meldedPlayer.Dumps.Last().SetMeld();
			{
				foreach (HaiID item2 in ((Chi)cmd).ChiPaiID.Select((int x) => ((Pai)x).ToHaiID()))
				{
					HaiIDs.Add(item2);
					player.TehaiIDs.Remove(item2);
				}
				break;
			}
		case RECEIVE_COMMAND_TYPE.ANKAN:
		{
			MeldType = PaifuMeldType.Ankan;
			MeldHaiID = ((Pai)((Ankan)cmd).PaiID).ToHaiID();
			for (int j = 0; j < 3; j++)
			{
				HaiIDs.Add(MeldHaiID);
				if (player.TehaiIDs.Contains(MeldHaiID))
				{
					player.TehaiIDs.Remove(MeldHaiID);
				}
				else
				{
					player.TehaiIDs.Remove(MeldHaiID + 64);
				}
			}
			break;
		}
		case RECEIVE_COMMAND_TYPE.MINKAN:
		{
			MeldType = (PaifuMeldType)(10 + cha - 1);
			MeldHaiID = ((Pai)((Minkan)cmd).PaiID).ToHaiID();
			meldedPlayer.Dumps.Last().SetMeld();
			for (int i = 0; i < 3; i++)
			{
				HaiIDs.Add(MeldHaiID);
				if (player.TehaiIDs.Contains(MeldHaiID))
				{
					player.TehaiIDs.Remove(MeldHaiID);
				}
				else
				{
					player.TehaiIDs.Remove(MeldHaiID + 64);
				}
			}
			break;
		}
		}
	}

	public void SetKakan(Kakan cmd)
	{
		MeldType += 3;
		KakanHaiID = ((Pai)cmd.PaiID).ToHaiID();
	}
}
