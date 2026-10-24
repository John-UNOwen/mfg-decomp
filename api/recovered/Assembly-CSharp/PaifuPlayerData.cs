using System;
using System.Collections.Generic;
using System.Linq;
using MFG.Types;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PaifuPlayerData : MonoBehaviour
{
	[NonSerialized]
	public List<HaiID> TehaiIDs = new List<HaiID>();

	[NonSerialized]
	public List<PaifuDump> Dumps = new List<PaifuDump>();

	[NonSerialized]
	public List<PaifuMeld> Melds = new List<PaifuMeld>();

	[NonSerialized]
	public Kaze JiKaze;

	[NonSerialized]
	public long Score;

	[NonSerialized]
	public int ReachLevel;

	[SerializeField]
	private GameObject TepaiParent;

	[SerializeField]
	private GameObject DumpParent;

	[SerializeField]
	private GameObject MeldParent;

	[SerializeField]
	private GameObject ScorePanel;

	private List<string> kazeText = new List<string> { "東", "南", "西", "北" };

	private PaifuPlayFlow paifuPlayFlow;

	private Sprite GetPaiSprite(HaiID haiID)
	{
		return paifuPlayFlow.PaiSprites[(int)haiID.ToNormal()];
	}

	public void Init(PaifuPlayFlow flow)
	{
		ClearPais();
		paifuPlayFlow = flow;
	}

	public void CreateGUI()
	{
		ClearPais();
		if (TehaiIDs.Count() == 14 || (TehaiIDs.Count() == 11 && Melds.Count == 1) || (TehaiIDs.Count() == 8 && Melds.Count == 2) || (TehaiIDs.Count() == 5 && Melds.Count == 3) || (TehaiIDs.Count() == 2 && Melds.Count == 4))
		{
			HaiID item = TehaiIDs.Last();
			TehaiIDs.RemoveAt(TehaiIDs.Count() - 1);
			TehaiIDs = TehaiIDs.OrderBy((HaiID x) => x.ToNormal()).ToList();
			TehaiIDs.Add(item);
		}
		else
		{
			TehaiIDs = TehaiIDs.OrderBy((HaiID x) => x.ToNormal()).ToList();
		}
		foreach (HaiID tehaiID in TehaiIDs)
		{
			UnityEngine.Object.Instantiate(paifuPlayFlow.PaiPrefab, TepaiParent.transform).transform.GetComponent<Image>().sprite = GetPaiSprite(tehaiID);
		}
		foreach (PaifuMeld meld in Melds)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(paifuPlayFlow.MeldPrefabs[(int)meld.MeldType], MeldParent.transform);
			gameObject.transform.Find("MeldPai").GetComponent<Image>().sprite = GetPaiSprite(meld.MeldHaiID);
			if (meld.IsKakan)
			{
				gameObject.transform.Find("Kakan").gameObject.SetActive(value: true);
				gameObject.transform.Find("Kakan").GetComponent<Image>().sprite = GetPaiSprite(meld.KakanHaiID);
			}
			for (int i = 1; i <= (meld.IsDaiMinkan ? 3 : 2); i++)
			{
				gameObject.transform.Find($"Pai{i}").GetComponent<Image>().sprite = GetPaiSprite(meld.HaiIDs[i - 1]);
			}
		}
		foreach (PaifuDump dump in Dumps)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(paifuPlayFlow.PaiPrefab, DumpParent.transform);
			gameObject2.transform.GetComponent<Image>().sprite = GetPaiSprite(dump.HaiID);
			switch (dump.State)
			{
			case DumpState.TsumoGiri:
				gameObject2.transform.GetComponent<Image>().color = new Color32(127, 191, byte.MaxValue, byte.MaxValue);
				break;
			case DumpState.Reach:
				gameObject2.transform.GetComponent<RectTransform>().localEulerAngles = new Vector3(0f, 0f, 90f);
				break;
			case DumpState.TsumoGiriReach:
				gameObject2.transform.GetComponent<RectTransform>().localEulerAngles = new Vector3(0f, 0f, 90f);
				gameObject2.transform.GetComponent<Image>().color = new Color32(127, 191, byte.MaxValue, byte.MaxValue);
				break;
			case DumpState.Melded:
				gameObject2.transform.GetComponent<Image>().color = new Color32(byte.MaxValue, 191, 127, byte.MaxValue);
				break;
			case DumpState.TsumoGiriMelded:
				gameObject2.transform.GetComponent<Image>().color = new Color32(byte.MaxValue, byte.MaxValue, 127, byte.MaxValue);
				break;
			case DumpState.MeldedReach:
				gameObject2.transform.GetComponent<RectTransform>().localEulerAngles = new Vector3(0f, 0f, 90f);
				gameObject2.transform.GetComponent<Image>().color = new Color32(byte.MaxValue, 191, 127, byte.MaxValue);
				break;
			case DumpState.TsumoGiriMeldedReach:
				gameObject2.transform.GetComponent<RectTransform>().localEulerAngles = new Vector3(0f, 0f, 90f);
				gameObject2.transform.GetComponent<Image>().color = new Color32(byte.MaxValue, byte.MaxValue, 127, byte.MaxValue);
				break;
			case DumpState.RonPai:
				gameObject2.transform.GetComponent<Image>().color = new Color32(byte.MaxValue, 127, 127, byte.MaxValue);
				break;
			case DumpState.TsumoGiriRonPai:
				gameObject2.transform.GetComponent<Image>().color = new Color32(byte.MaxValue, 127, 191, byte.MaxValue);
				break;
			}
		}
		SetScore();
		SetJiKaze();
		SetReach();
	}

	private void ClearPais()
	{
		foreach (Transform item in TepaiParent.transform)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		foreach (Transform item2 in DumpParent.transform)
		{
			UnityEngine.Object.Destroy(item2.gameObject);
		}
		foreach (Transform item3 in MeldParent.transform)
		{
			UnityEngine.Object.Destroy(item3.gameObject);
		}
	}

	public void SetScore()
	{
		ScorePanel.transform.Find("Score").GetComponent<TextMeshProUGUI>().text = Score.ToString();
	}

	public void SetJiKaze()
	{
		ScorePanel.transform.Find("JiKaze").GetComponent<TextMeshProUGUI>().text = kazeText[(int)JiKaze];
	}

	public void SetReach()
	{
		ScorePanel.transform.Find("ReachStick").gameObject.SetActive(ReachLevel > 0);
	}
}
