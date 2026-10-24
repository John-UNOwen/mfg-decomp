using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types.Customize.CutinItem;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace MFG.Scenes.GachaSelect.SupplyItem;

public class SupplyPickupItemListWindow : MonoBehaviour
{
	[Serializable]
	public class WariaiText
	{
		public List<TextMeshProUGUI> wariais;
	}

	[SerializeField]
	private Button m_closeButton;

	[SerializeField]
	private TextMeshProUGUI m_text;

	[SerializeField]
	private Button m_upButton;

	[SerializeField]
	private Button m_downButton;

	[SerializeField]
	private ScrollRect m_scrollRect;

	[SerializeField]
	private GameObject m_panelInstanceParentLimited;

	[SerializeField]
	private GameObject m_panelInstanceParentPickup;

	[SerializeField]
	private GameObject m_panelInstanceParent;

	[SerializeField]
	private GameObject m_panelPrefab;

	[SerializeField]
	[Header("0: 通常UR～N, 1: ピックアップ, 2: 限定")]
	private List<WariaiText> m_wariaiText;

	[SerializeField]
	private GameObject m_beforeVersionNotice;

	[SerializeField]
	private GameObject m_pickupWariaiObj;

	private void Start()
	{
		m_closeButton.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
			base.gameObject.SetActive(value: false);
		}).AddTo(this);
		m_upButton?.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			m_scrollRect.velocity = new Vector2(0f, -450f);
		}).AddTo(this);
		m_downButton?.OnClickAsObservable().Subscribe(delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			m_scrollRect.velocity = new Vector2(0f, 450f);
		}).AddTo(this);
	}

	public void Open(GachaSeriesServerInfo gachaSeriesInfo)
	{
		SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
		m_scrollRect.verticalNormalizedPosition = 1f;
		bool flag = gachaSeriesInfo.PickupKind == PickupKind.NewGirl;
		if (m_pickupWariaiObj != null)
		{
			m_pickupWariaiObj.SetActive(!flag);
		}
		if (m_panelInstanceParentLimited != null)
		{
			foreach (Transform item in m_panelInstanceParentLimited.transform)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
		}
		if (m_panelInstanceParentPickup != null)
		{
			m_panelInstanceParentPickup.SetActive(!flag);
			foreach (Transform item2 in m_panelInstanceParentPickup.transform)
			{
				UnityEngine.Object.Destroy(item2.gameObject);
			}
		}
		if (m_panelInstanceParent != null)
		{
			foreach (Transform item3 in m_panelInstanceParent.transform)
			{
				UnityEngine.Object.Destroy(item3.gameObject);
			}
		}
		m_text.text = GetSupplyText(gachaSeriesInfo);
		if (gachaSeriesInfo.SeriesType != GachaSeriesType.Music)
		{
			string[] array = new string[4] { "UR", "SR", "R", "N" };
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					m_wariaiText[i].wariais[j].text = array[j] + new string(' ', (j <= 1) ? 10 : 12) + gachaSeriesInfo.ProbabilityTable[i][(Rarity)(5 - j)].ToString("F1") + "%";
				}
			}
		}
		if (gachaSeriesInfo.SeriesType == GachaSeriesType.Music)
		{
			if (m_panelInstanceParentPickup != null)
			{
				List<(string oid, int no)> sortedMusicItems = gachaSeriesInfo.GetSortedMusicItems();
				string shortName = MasterGameDataManager.Character.Properties(gachaSeriesInfo.PickUpCharas[0]).Profile.ShortName;
				foreach (var (oid, itemNo) in sortedMusicItems)
				{
					UnityEngine.Object.Instantiate(m_panelPrefab, m_panelInstanceParentPickup.transform).GetComponent<SupplyItemPanel>().MusicReflesh(shortName, oid, itemNo);
				}
			}
		}
		else
		{
			if (m_panelInstanceParentLimited != null)
			{
				foreach (ItemID item4 in gachaSeriesInfo.LimitedItems.SortItemID())
				{
					UnityEngine.Object.Instantiate(m_panelPrefab, m_panelInstanceParentLimited.transform).GetComponent<SupplyItemPanel>().Reflesh(item4);
				}
			}
			if (m_panelInstanceParentPickup != null)
			{
				foreach (ItemID item5 in gachaSeriesInfo.PickupItems.SortItemID())
				{
					UnityEngine.Object.Instantiate(m_panelPrefab, m_panelInstanceParentPickup.transform).GetComponent<SupplyItemPanel>().Reflesh(item5);
				}
			}
			if (m_panelInstanceParent != null)
			{
				foreach (ItemID item6 in gachaSeriesInfo.Items.SortItemID())
				{
					UnityEngine.Object.Instantiate(m_panelPrefab, m_panelInstanceParent.transform).GetComponent<SupplyItemPanel>().Reflesh(item6);
				}
			}
		}
		if (m_beforeVersionNotice != null)
		{
			m_beforeVersionNotice.SetActive(gachaSeriesInfo.GachaSeriesName == GachaSeriesName.PickupAlotGrimAroe || gachaSeriesInfo.GachaSeriesName == GachaSeriesName.PickupDarknessRobeMitsuba2);
		}
		base.gameObject.SetActive(value: true);
	}

	public string GetSupplyText(GachaSeriesServerInfo info)
	{
		string exchangePointLabel = info.GetExchangePointLabel();
		switch (info.SeriesType)
		{
		case GachaSeriesType.Music:
			return "対局中に流れるリーチソングやボイスがGETできます。\n<color=red>本ガチャから所持済みのアイテムが出現することはありません。</color>\n\nまた、ガチャの結果とは別にギフト×２をGETできます。\n";
		case GachaSeriesType.Unlock:
		{
			CharaProfile profile = MasterGameDataManager.Character.Properties(info.PickUpCharas[0]).Profile;
			return "対局を熱く演出する、様々なガールズカットインがGETできます。\nガチャを引くと「ファイトスピリット」と、ガールズカットインとの交換に使用する「交換点」がおまけでもらえます。\n<color=red>ピックアップ期間中は、対象ファイトガールの「ガールズカットイン」の提供割合が高くなります。</color>\n\n＜ファイトスピリットをGET＞\nファイトガールのデビューに必要なファイトスピリットが、解禁済みのファイトガールの中からランダムでGETできます。\n「1回ガチャ」なら1個、「5回ガチャ」なら6個GETできます。\n\n<color=blue>＜SPガチャの特殊仕様＞\n" + profile.Name + "のガールズカットインをGETすると、ファイトスピリット(" + profile.ShortName + ")を獲得でき、" + profile.Name + "が解禁されます。</color>\n※ファイトスピリット(" + profile.ShortName + ")は上記おまけとして獲得できます。\n\n＜交換点が貯まります＞\nガチャ1回につき、" + exchangePointLabel + "が1点貯まります。\n「交換リスト」で交換可能なガールズカットイン一覧の確認と、" + exchangePointLabel + "を使用して交換が可能です。\n<color=red>※" + exchangePointLabel + "はピックアップ期間を終了するとリセットされます。</color> ";
		}
		default:
		{
			string text = "<color=red>ピックアップ期間中は、対象ファイトガールの「ガールズカットイン」と「ファイトスピリット」の提供割合が高くなります。</color>\n";
			if (info.PickUpCharas.Count >= 5)
			{
				text = "<color=red>ピックアップ期間中は、対象ファイトガールの「ガールズカットイン」の提供割合が高くなります。</color>\n";
			}
			return "対局を熱く演出する、様々なガールズカットインがGETできます。\nガチャを引くと「ファイトスピリット」と、ガールズカットインとの交換に使用する「交換点」がおまけでもらえます。\n" + text + "\n＜ファイトスピリットをGET＞\nファイトガールのデビューに必要なファイトスピリットが、解禁済みのファイトガールの中からランダムでGETできます。\n「1回ガチャ」なら1個、「5回ガチャ」なら6個GETできます。\n<color=red>50%</color>の確率でピックアップ対象ファイトガールのファイトスピリットが抽選され、残り50％の確率でそれ以外の解禁済みのファイトガールのファイトスピリットが抽選されます。\n\n＜交換点が貯まります＞\nガチャ1回につき、" + exchangePointLabel + "が1点貯まります。\n「交換リスト」で交換可能なガールズカットイン一覧の確認と、" + exchangePointLabel + "を使用して交換が可能です。\n<color=red>※" + exchangePointLabel + "はピックアップ期間を終了するとリセットされます。</color> ";
		}
		}
	}
}
