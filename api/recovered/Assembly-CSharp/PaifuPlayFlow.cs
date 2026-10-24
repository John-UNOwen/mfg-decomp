using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using MFG.Sytem.Utilities.Xml;
using MFG.Taikyoku.Command;
using MFG.Taikyoku.Command.Receive;
using MFG.Types;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PaifuPlayFlow : MonoBehaviour
{
	public List<Sprite> PaiSprites = new List<Sprite>();

	public GameObject PaiPrefab;

	public List<GameObject> MeldPrefabs;

	[SerializeField]
	private TextMeshProUGUI KyokuText;

	[SerializeField]
	private TextMeshProUGUI KyotakuText;

	[SerializeField]
	private GameObject OmoteDoraParent;

	[SerializeField]
	private GameObject UraDoraParent;

	private List<PaifuPlayBaData> PaifuDatas = new List<PaifuPlayBaData>();

	[SerializeField]
	private List<PaifuPlayerData> Players = new List<PaifuPlayerData>();

	private int[] Zasekis = new int[4];

	private int KyokuIndex;

	private int HandIndex;

	private PaifuPlayBaData CurrentData => PaifuDatas[KyokuIndex];

	private int GetZaseki(int pid)
	{
		return Zasekis[pid];
	}

	private void Start()
	{
		string text = ((TextAsset)Resources.Load("tid_core_2536480")).text.Replace("<?xml version=\"1.0\" encoding=\"UTF-8\"?>", "");
		text = "<root>" + text + "</root>";
		XElement xElement = XDocument.Parse(text).Root.Element("taikyoku").Element("cell_info");
		int num = 0;
		for (XElement xElement2 = xElement?.Element("cell_data_" + num); xElement2 != null; xElement2 = xElement?.Element("cell_data_" + num))
		{
			RECEIVE_COMMAND_TYPE rECEIVE_COMMAND_TYPE = (RECEIVE_COMMAND_TYPE)xElement2.AttributeValueInt("kind");
			ReceiveCommand receiveCommand = ReceiveCommandFactory.Create(rECEIVE_COMMAND_TYPE, num);
			if (receiveCommand != null)
			{
				receiveCommand.Parse(xElement2);
				if (rECEIVE_COMMAND_TYPE == RECEIVE_COMMAND_TYPE.KYOKUSTART)
				{
					PaifuDatas.Add(new PaifuPlayBaData());
					PaifuDatas[PaifuDatas.Count() - 1].Ba = (Ba)((KyokuStart)receiveCommand).Ba;
					PaifuDatas[PaifuDatas.Count() - 1].Kyoku = ((KyokuStart)receiveCommand).Kyoku;
					PaifuDatas[PaifuDatas.Count() - 1].Honba = ((KyokuStart)receiveCommand).Honba;
					if (num == 0)
					{
						for (int i = 0; i < 4; i++)
						{
							Players[i].Init(this);
							Zasekis[i] = ((KyokuStart)receiveCommand).Players[i].Jikaze;
						}
					}
				}
				PaifuDatas[PaifuDatas.Count() - 1].ReceivedCommands.Add(receiveCommand);
			}
			num++;
		}
		KyokuIndex = 0;
		HandIndex = 0;
		Set();
	}

	private void Update()
	{
		if (Bi2xInput.GetKeyDown(KeyCode.LeftArrow))
		{
			if (HandIndex <= 0)
			{
				KyokuIndex = ((KyokuIndex <= 0) ? (PaifuDatas.Count() - 1) : (KyokuIndex - 1));
				HandIndex = CurrentData.ReceivedCommands.Count() - 1;
			}
			else
			{
				HandIndex--;
			}
			Set();
		}
		else if (Bi2xInput.GetKeyDown(KeyCode.RightArrow))
		{
			if (HandIndex >= CurrentData.ReceivedCommands.Count() - 1)
			{
				KyokuIndex = ((KyokuIndex < PaifuDatas.Count() - 1) ? KyokuIndex++ : 0);
				HandIndex = 0;
			}
			else
			{
				HandIndex++;
			}
			Set();
		}
		else if (Bi2xInput.GetKeyDown(KeyCode.UpArrow))
		{
			if (KyokuIndex > 0)
			{
				KyokuIndex--;
				HandIndex = 0;
			}
			Set();
		}
		else if (Bi2xInput.GetKeyDown(KeyCode.DownArrow))
		{
			if (KyokuIndex < PaifuDatas.Count() - 1)
			{
				KyokuIndex++;
				HandIndex = 0;
			}
			Set();
		}
	}

	private void Set()
	{
		for (int i = 0; i <= HandIndex; i++)
		{
			ReceiveCommand cmd = CurrentData.ReceivedCommands[i];
			switch (cmd.Kind)
			{
			case RECEIVE_COMMAND_TYPE.TSUMO:
				Players[GetZaseki(((Tsumo)cmd).PlayerIndex)].TehaiIDs.Add(((Pai)((Tsumo)cmd).PaiID).ToHaiID());
				break;
			case RECEIVE_COMMAND_TYPE.SUTEHAI:
				Players[GetZaseki(((Sutehai)cmd).PlayerIndex)].TehaiIDs.Remove(((Pai)((Sutehai)cmd).PaiID).ToHaiID());
				Players[GetZaseki(((Sutehai)cmd).PlayerIndex)].Dumps.Add(new PaifuDump((Sutehai)cmd));
				Players[GetZaseki(((Sutehai)cmd).PlayerIndex)].ReachLevel = (((Sutehai)cmd).IsFireRichi ? 5 : (((Sutehai)cmd).IsAtomicRichi ? 3 : (((Sutehai)cmd).IsOpenRichi ? 2 : (((Sutehai)cmd).IsRichi ? 1 : 0))));
				break;
			case RECEIVE_COMMAND_TYPE.RON:
				Players[GetZaseki(((Ron)cmd).PlayerIndex)].Dumps.Last().SetRon();
				break;
			case RECEIVE_COMMAND_TYPE.PON:
				Players[GetZaseki(((Pon)cmd).PlayerIndex)].Melds.Add(new PaifuMeld(cmd, Players[GetZaseki(((Pon)cmd).PlayerIndex)], Players[GetZaseki(((Pon)cmd).SutePlayerIndex)]));
				break;
			case RECEIVE_COMMAND_TYPE.CHI:
				Players[GetZaseki(((Chi)cmd).PlayerIndex)].Melds.Add(new PaifuMeld(cmd, Players[GetZaseki(((Chi)cmd).PlayerIndex)], Players[GetZaseki(((Chi)cmd).SutePlayerIndex)]));
				break;
			case RECEIVE_COMMAND_TYPE.ANKAN:
				Players[GetZaseki(((Ankan)cmd).PlayerIndex)].Melds.Add(new PaifuMeld(cmd, Players[GetZaseki(((Ankan)cmd).PlayerIndex)], null));
				break;
			case RECEIVE_COMMAND_TYPE.MINKAN:
				Players[GetZaseki(((Minkan)cmd).PlayerIndex)].Melds.Add(new PaifuMeld(cmd, Players[GetZaseki(((Minkan)cmd).PlayerIndex)], Players[GetZaseki(((Minkan)cmd).SutePlayerIndex)]));
				break;
			case RECEIVE_COMMAND_TYPE.KAKAN:
				Players[GetZaseki(((Kakan)cmd).PlayerIndex)].Melds.Find((PaifuMeld x) => x.MeldNormalHaiID == ((Pai)((Kakan)cmd).PaiID).ToNormal().ToHaiID()).SetKakan((Kakan)cmd);
				break;
			case RECEIVE_COMMAND_TYPE.KYOKUSTART:
			{
				foreach (Transform item in OmoteDoraParent.transform)
				{
					Object.Destroy(item.gameObject);
				}
				foreach (Transform item2 in UraDoraParent.transform)
				{
					Object.Destroy(item2.gameObject);
				}
				for (int k = 0; k < 5; k++)
				{
					Object.Instantiate(PaiPrefab, OmoteDoraParent.transform).transform.GetComponent<Image>().sprite = PaiSprites[(int)((Pai)((KyokuStart)cmd).OmoteDoraData[k]).ToNormal().ToHaiID()];
					Object.Instantiate(PaiPrefab, UraDoraParent.transform).transform.GetComponent<Image>().sprite = PaiSprites[(int)((Pai)((KyokuStart)cmd).UraDoraData[k]).ToNormal().ToHaiID()];
				}
				KyokuText.text = string.Format("{0}{1}局 {2}本場", (((KyokuStart)cmd).Ba == 0) ? "東" : "南", ((KyokuStart)cmd).Kyoku + 1, ((KyokuStart)cmd).Honba);
				KyotakuText.text = $"{((KyokuStart)cmd).Kyotaku}";
				for (int l = 0; l < 4; l++)
				{
					Players[GetZaseki(l)].TehaiIDs = ((KyokuStart)cmd).Players[l].TePai.Select((int x) => ((Pai)x).ToHaiID()).ToList();
					Players[GetZaseki(l)].Melds = new List<PaifuMeld>();
					Players[GetZaseki(l)].Dumps = new List<PaifuDump>();
					Players[GetZaseki(l)].JiKaze = (Kaze)((KyokuStart)cmd).Players[l].Jikaze;
					Players[GetZaseki(l)].Score = ((KyokuStart)cmd).Players[l].Score;
				}
				break;
			}
			case RECEIVE_COMMAND_TYPE.SCORERANK:
			{
				for (int j = 0; j < 4; j++)
				{
					Players[GetZaseki(j)].Score = ((ScoreRank)cmd).RichiAfterDatas[j].score;
				}
				KyotakuText.text = $"{((ScoreRank)cmd).Kyotaku}";
				break;
			}
			}
		}
		foreach (PaifuPlayerData player in Players)
		{
			player.CreateGUI();
		}
	}
}
