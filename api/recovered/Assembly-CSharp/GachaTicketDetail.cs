using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.Types;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class GachaTicketDetail : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI m_ticketNameText;

	[SerializeField]
	private TextMeshProUGUI m_itemCountText;

	[SerializeField]
	private Image m_itemIcon;

	public string ObjectID;

	public async UniTask Reflesh(string oid, int count, string expire)
	{
		if (ItemUtility.IsGachaTicket(oid))
		{
			ObjectID = oid;
			m_ticketNameText.text = ItemUtility.ItemName(oid) + ((!string.IsNullOrEmpty(expire)) ? ("\n使用期限 : " + expire) : "");
			m_itemCountText.text = $"×{count}";
			IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(ItemUtility.ItemIconGraphicPath(oid));
			disposableAsset.AddTo(this);
			m_itemIcon.sprite = disposableAsset.Asset;
		}
	}
}
