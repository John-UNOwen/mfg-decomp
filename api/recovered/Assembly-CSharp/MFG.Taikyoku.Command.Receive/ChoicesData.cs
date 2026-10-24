using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using MFG.Sytem.Utilities.Xml;
using UnityEngine;

namespace MFG.Taikyoku.Command.Receive;

public class ChoicesData
{
	public class ChiPatternData
	{
		public int[] PaiID;
	}

	public class PonPatternData
	{
		public int[] PaiID;
	}

	public class KanPatternData
	{
		public int PaiID;

		public int KanType;
	}

	public class KuikaePatternData
	{
		public int PaiID;
	}

	public class MachiPatternData
	{
		public int SutePaiID;

		public int MachiPaiNum;

		public int[] MachiPaiID;

		public int[] MachiPaiStatus;

		public void Parse(XElement patternElem)
		{
			try
			{
				SutePaiID = patternElem.NeedNotElement("sute_pai").ValueInt();
				MachiPaiNum = patternElem.NeedNotElement("machi_num").ValueInt();
				MachiPaiID = patternElem.NeedNotElement("machi_pai").ValueArrayInt();
				MachiPaiStatus = patternElem.NeedNotElement("stat").ValueArrayInt();
			}
			catch (NeedNotNodeNotFoundException)
			{
			}
		}

		public bool IsMachiPaiRemain()
		{
			int[] machiPaiStatus = MachiPaiStatus;
			for (int i = 0; i < machiPaiStatus.Length; i++)
			{
				if ((machiPaiStatus[i] & 2) == 0)
				{
					return true;
				}
			}
			return false;
		}
	}

	public SELECTABLE_TYPE_FLAG SelectableTypeFlag;

	public SELECTABLE_TYPE_FLAG NakiSelectableTypeFlag;

	public int PlayerIndex;

	public int PaiID;

	public List<ChiPatternData> ChiPatterns = new List<ChiPatternData>();

	public List<PonPatternData> PonPatterns = new List<PonPatternData>();

	public List<KanPatternData> KanPatterns = new List<KanPatternData>();

	public List<MachiPatternData> MachiPatterns = new List<MachiPatternData>();

	public List<KuikaePatternData> KuikaePatterns = new List<KuikaePatternData>();

	public void ParseTsumoChoices(XElement cellDataElement)
	{
		try
		{
			SelectableTypeFlag = (SELECTABLE_TYPE_FLAG)cellDataElement.NeedNotElement("select").ValueInt();
		}
		catch (NeedNotNodeNotFoundException)
		{
			return;
		}
		ParseKanPattern(cellDataElement);
		ParseMachiPattern(cellDataElement);
		ParseKuikae(cellDataElement);
	}

	public void ParseSuteChoices(XElement cellDataElement)
	{
		try
		{
			SelectableTypeFlag = (SELECTABLE_TYPE_FLAG)cellDataElement.NeedNotElement("select").ValueInt();
			NakiSelectableTypeFlag = (SELECTABLE_TYPE_FLAG)cellDataElement.NeedNotElement("naki").ValueInt();
			PlayerIndex = cellDataElement.NeedNotElement("pindex").ValueInt();
			PaiID = cellDataElement.NeedNotElement("sute_pai").ValueInt();
		}
		catch (NeedNotNodeNotFoundException)
		{
			return;
		}
		ParseChiPattern(cellDataElement);
		ParsePonPattern(cellDataElement);
		ParseKanPattern(cellDataElement);
	}

	public void ParseKanPattern(XElement cellDataElement)
	{
		try
		{
			KanPatternData[] array = new KanPatternData[4];
			for (int i = 0; i < 4; i++)
			{
				KanPatternData kanPatternData = new KanPatternData();
				array[i] = kanPatternData;
			}
			int[] array2 = cellDataElement.NeedNotElement("kan_pai").ValueArrayInt();
			for (int j = 0; j < array2.Length; j++)
			{
				array[j].PaiID = array2[j];
			}
			int[] array3 = cellDataElement.NeedNotElement("kan_type").ValueArrayInt();
			for (int k = 0; k < array3.Length; k++)
			{
				array[k].KanType = array3[k];
			}
			KanPatterns = array.Where((KanPatternData pattern) => pattern.PaiID != 0).ToList();
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}

	public void ParseChiPattern(XElement cellDataElement)
	{
		try
		{
			Debug.Log("ParseChiPattern : " + cellDataElement.ToString());
			int[] array = cellDataElement.NeedNotElement("chi_pai").ValueArrayInt();
			int[] array2 = null;
			XElement xElement = cellDataElement.Element("extra_chi_pai");
			if (xElement != null)
			{
				array2 = xElement.ValueArrayInt();
			}
			int[] array3 = ((array2 != null) ? array.ToList().Concat(array2.ToList()).ToArray() : array);
			Debug.Log("ChiPattern : " + string.Join(",", array3));
			ChiPatternData[] array4 = new ChiPatternData[12];
			for (int i = 0; i < 12; i++)
			{
				ChiPatternData chiPatternData = new ChiPatternData();
				array4[i] = chiPatternData;
				array4[i].PaiID = new int[2];
			}
			for (int j = 0; j < array3.Length; j++)
			{
				int num = j / 2;
				int num2 = j % 2;
				array4[num].PaiID[num2] = array3[j];
			}
			ChiPatterns = array4.Where((ChiPatternData pattern) => pattern.PaiID[0] != 0).ToList();
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}

	public void ParsePonPattern(XElement cellDataElement)
	{
		try
		{
			int[] array = cellDataElement.NeedNotElement("pon_pai").ValueArrayInt();
			PonPatternData[] array2 = new PonPatternData[2];
			for (int i = 0; i < 2; i++)
			{
				PonPatternData ponPatternData = new PonPatternData();
				array2[i] = ponPatternData;
				array2[i].PaiID = new int[2];
			}
			for (int j = 0; j < array.Length; j++)
			{
				int num = j / 2;
				int num2 = j % 2;
				array2[num].PaiID[num2] = array[j];
			}
			PonPatterns = array2.Where((PonPatternData pattern) => pattern.PaiID[0] != 0).ToList();
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}

	public void ParseMachiPattern(XElement cellDataElement)
	{
		int num = 0;
		try
		{
			num = cellDataElement.NeedNotElement("ptn_num").ValueInt();
		}
		catch (NeedNotNodeNotFoundException)
		{
			return;
		}
		MachiPatternData[] array = new MachiPatternData[num];
		for (int i = 0; i < num; i++)
		{
			XElement patternElem = cellDataElement.NeedNotElement("ptn" + i);
			MachiPatternData machiPatternData = new MachiPatternData();
			machiPatternData.Parse(patternElem);
			array[i] = machiPatternData;
		}
		MachiPatterns = array.ToList();
		if (MachiPatterns.Count <= 0)
		{
			SelectableTypeFlag &= ~SELECTABLE_TYPE_FLAG.REACH;
		}
	}

	public void ParseKuikae(XElement cellDataElement)
	{
		try
		{
			int[] array = cellDataElement.NeedNotElement("kuikae_pai").ValueArrayInt();
			for (int i = 0; i < array.Length; i++)
			{
				KuikaePatternData kuikaePatternData = new KuikaePatternData();
				kuikaePatternData.PaiID = array[i];
				KuikaePatterns.Add(kuikaePatternData);
			}
		}
		catch (NeedNotNodeNotFoundException)
		{
		}
	}
}
