using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.Types.Customize.CutinItem;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace MFG.Scenes.GachaSelect.SupplyItem;

public class SupplyItemListWindow : MonoBehaviour
{
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
	private GameObject m_panelInstanceParent;

	[SerializeField]
	private GameObject m_panelPrefab;

	[SerializeField]
	private GameObject m_wariaiParent;

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
		foreach (Transform item in m_panelInstanceParent.transform)
		{
			Object.Destroy(item.gameObject);
		}
		m_text.text = "対局を熱く演出する、様々なガールズカットインがGETできます。\nガチャを引くと「ファイトスピリット」と、ガールズカットインとの交換に使用する「交換点」が\nおまけでもらえます。\n\n＜ファイトスピリットをGET＞\nファイトガールのデビューに必要なファイトスピリットが、解禁済みのファイトガールの中からランダムでGETできます。\n「1回ガチャ」なら1個、「5回ガチャ」なら6個GETできます。\n\n＜交換点が貯まります＞\nガチャ1回につき、" + gachaSeriesInfo.GetExchangePointLabel() + "が1点貯まります。\n「交換リスト」で交換可能なガールズカットイン一覧の確認と、" + gachaSeriesInfo.GetExchangePointLabel() + "を使用して交換が可能です。";
		m_wariaiParent.transform.Find("TextUR").GetComponent<TextMeshProUGUI>().text = "UR          " + gachaSeriesInfo.ProbabilityTable[0][Rarity.UR].ToString("F1") + "%";
		m_wariaiParent.transform.Find("TextSR").GetComponent<TextMeshProUGUI>().text = "SR          " + gachaSeriesInfo.ProbabilityTable[0][Rarity.SR].ToString("F1") + "%";
		m_wariaiParent.transform.Find("TextR").GetComponent<TextMeshProUGUI>().text = "R            " + gachaSeriesInfo.ProbabilityTable[0][Rarity.R].ToString("F1") + "%";
		m_wariaiParent.transform.Find("TextN").GetComponent<TextMeshProUGUI>().text = "N            " + gachaSeriesInfo.ProbabilityTable[0][Rarity.N].ToString("F1") + "%";
		foreach (ItemID item2 in gachaSeriesInfo.Items)
		{
			Object.Instantiate(m_panelPrefab, m_panelInstanceParent.transform).GetComponent<SupplyItemPanel>().Reflesh(item2);
		}
		base.gameObject.SetActive(value: true);
	}
}
