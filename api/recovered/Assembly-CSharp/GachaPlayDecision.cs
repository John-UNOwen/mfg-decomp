using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.MasterGameData;
using MFG.Types.Customize.CutinItem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaPlayDecision : MonoBehaviour
{
	[SerializeField]
	private GachaPlayFlow m_gachaPlayFlow;

	[SerializeField]
	private TextMeshProUGUI m_charaName;

	[SerializeField]
	private TextMeshProUGUI m_cutinName;

	[SerializeField]
	private Image m_band;

	[SerializeField]
	private Image m_rarity;

	[SerializeField]
	private Image m_rarityAdd;

	[SerializeField]
	private Image m_rarityBig;

	[SerializeField]
	private List<Sprite> m_raritySprites;

	[SerializeField]
	private List<Sprite> m_rarityBigSprites;

	[SerializeField]
	private List<Sprite> m_rarityBandSprites;

	private Animator anim => base.transform.GetComponent<Animator>();

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void CutinPlayIn(ItemID itemID)
	{
		Item item = CutinItemMaster.GetItem(itemID);
		m_charaName.text = MasterGameDataManager.Character.Properties(item.CharaType).Profile.ShortName;
		m_cutinName.text = item.ReplacedShortName ?? "";
		Color color = m_charaName.color;
		color.a = 1f;
		m_charaName.color = color;
		color = m_cutinName.color;
		color.a = 1f;
		m_cutinName.color = color;
		m_band.sprite = m_rarityBandSprites[(int)item.Rarity];
		m_rarity.sprite = m_raritySprites[(int)item.Rarity];
		m_rarityAdd.sprite = m_raritySprites[(int)item.Rarity];
		m_rarityBig.sprite = m_rarityBigSprites[(int)item.Rarity];
		base.gameObject.SetActive(value: true);
		anim.SetTrigger((item.Rarity >= Rarity.SR) ? "SrIn" : "In");
		anim.Update(0f);
	}

	public async UniTask GachaDecisionExit(CancellationToken cancellationToken)
	{
		anim.Play("GachaDecisionEnd", 0, 0f);
		anim.Update(0f);
		await UniTask.WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName("GachaDecisionWait"), PlayerLoopTiming.Update, cancellationToken);
	}
}
