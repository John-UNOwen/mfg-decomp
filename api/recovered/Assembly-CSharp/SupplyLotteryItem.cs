using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.MasterGameData;
using MFG.Types;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class SupplyLotteryItem : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI m_categoryText;

	[SerializeField]
	private TextMeshProUGUI m_itemNameText;

	[SerializeField]
	private Image m_itemIcon;

	[SerializeField]
	private Image m_alreadyIcon;

	[SerializeField]
	private TextMeshProUGUI m_charaName;

	[SerializeField]
	private Image m_charaColor;

	public string ObjectID;

	public JanponObject.JanponKind JanponKind;

	public bool IsPossession;

	public async UniTask Init(string oid, bool isNew)
	{
		if (ItemUtility.IsCatchphrase(oid) || ItemUtility.IsCustomizeItem(oid) || ItemUtility.IsCutinItem(oid))
		{
			ObjectID = oid;
			JanponKind = ((!ObjectID.Contains("OID_TableSticker")) ? ((ObjectID.Contains("OID_ReachBgm") || ObjectID.Contains("OID_MyRoomBgm")) ? JanponObject.JanponKind.Music : JanponObject.JanponKind.Others) : JanponObject.JanponKind.Sticker);
			m_categoryText.text = ItemUtility.ItemCategory(oid);
			m_itemNameText.text = ItemUtility.ItemName(oid);
			m_categoryText.color = (isNew ? Color.red : Color.black);
			m_itemNameText.color = (isNew ? Color.red : Color.black);
			IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(ItemUtility.ItemIconGraphicPath(oid));
			disposableAsset.AddTo(this);
			m_itemIcon.sprite = disposableAsset.Asset;
			IsPossession = ItemUtility.IsPossession(ObjectID);
			CharaType charaType = (ItemUtility.IsCutinItem(ObjectID) ? CutinItemMaster.GetItem(CutinItemMaster.GetItemID(ObjectID)).CharaType : (ObjectID.Contains("OID_ReachBgm") ? CustomizeItemMaster.GetItem(ObjectID).charaType : ((CharaType)(-1))));
			if (charaType >= CharaType.Chara01)
			{
				CharaProfile profile = MasterGameDataManager.Character.Properties(charaType).Profile;
				m_charaColor.color = profile.Color;
				m_charaName.text = profile.ShortName;
			}
			else
			{
				m_charaColor.color = new Color(0f, 0f, 0f, 0f);
				m_charaName.text = "";
			}
			AlreadyIconRefresh();
		}
	}

	public void AlreadyIconRefresh()
	{
		IsPossession = ItemUtility.IsPossession(ObjectID);
		m_alreadyIcon.color = new Color(1f, 1f, 1f, IsPossession ? 1 : 0);
	}
}
