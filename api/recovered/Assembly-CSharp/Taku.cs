using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.CustomizeItem;
using UnityEngine;

public class Taku : MonoBehaviour
{
	[Serializable]
	private class DumpFrame
	{
		[SerializeField]
		private List<GameObject> DumpFrameObjs = new List<GameObject>();

		public void SetActive(bool active)
		{
			foreach (GameObject dumpFrameObj in DumpFrameObjs)
			{
				if (dumpFrameObj != null)
				{
					dumpFrameObj.SetActive(active);
				}
			}
		}
	}

	[Serializable]
	private class Sticker
	{
		[SerializeField]
		private List<GameObject> m_stickerParents = new List<GameObject>();

		public async UniTask SetSticker(StickerEditInfo editInfo, GameObject stickerObj, CancellationToken cancellationToken = default(CancellationToken))
		{
			ItemID itemID = editInfo.ItemID;
			cancellationToken.ThrowIfCancellationRequested();
			if (CustomizeItemMaster.GetItem(itemID).Category != Category.TableSticker || itemID == ItemID.TableStickerNone)
			{
				return;
			}
			IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(CustomizeItemMaster.GetItem(itemID).MahjongPlayDataPath, null, cancellationToken);
			foreach (GameObject stickerParent in m_stickerParents)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(stickerObj, stickerParent.transform);
				gameObject.transform.GetComponent<SpriteRenderer>().sprite = disposableAsset.Asset;
				gameObject.transform.GetComponent<SpriteRenderer>().sortingOrder = editInfo.Priority - 8;
				gameObject.transform.localPosition = new Vector2((float)editInfo.PositionX / 100f, (float)editInfo.PositionY / 100f);
				gameObject.transform.localEulerAngles = new Vector3(0f, 0f, -editInfo.EulerRotate);
			}
		}

		public void GetLog()
		{
			try
			{
				int num = 0;
				foreach (Transform item in m_stickerParents[2].transform)
				{
					Debug.Log($"{num} / Enable : {item.gameObject.activeSelf} / Color : {item.transform.GetComponent<SpriteRenderer>().color} / Sprite : {item.transform.GetComponent<SpriteRenderer>().sprite} / SortingOrder : {item.transform.GetComponent<SpriteRenderer>().sortingOrder} / Pos : {item.transform.localPosition}");
					num++;
				}
			}
			catch
			{
			}
		}

		public void SetDispOption(bool isAllDisp, StickerEditInfo highestPriorityStickerEditInfo)
		{
			foreach (GameObject stickerParent in m_stickerParents)
			{
				int num = stickerParent.transform.childCount;
				if (num <= 0)
				{
					continue;
				}
				foreach (Transform item in stickerParent.transform)
				{
					num--;
					item.gameObject.SetActive(isAllDisp || num == 0);
					if (num == 0)
					{
						if (isAllDisp)
						{
							item.localPosition = new Vector2((float)highestPriorityStickerEditInfo.PositionX / 100f, (float)highestPriorityStickerEditInfo.PositionY / 100f);
							item.localEulerAngles = new Vector3(0f, 0f, -highestPriorityStickerEditInfo.EulerRotate);
						}
						else
						{
							item.localPosition = new Vector2(1f, -0.5f);
							item.localEulerAngles = new Vector3(0f, 0f, 0f);
						}
					}
				}
			}
		}

		public void InitStickers()
		{
			foreach (GameObject stickerParent in m_stickerParents)
			{
				try
				{
					foreach (Transform item in stickerParent.transform)
					{
						UnityEngine.Object.Destroy(item.gameObject);
					}
				}
				catch
				{
				}
			}
		}
	}

	[SerializeField]
	private List<Sticker> Stickers = new List<Sticker>();

	[SerializeField]
	private List<DumpFrame> DumpFrames = new List<DumpFrame>();

	[SerializeField]
	private GameObject StickerObject;

	private void Start()
	{
		foreach (DumpFrame dumpFrame in DumpFrames)
		{
			dumpFrame.SetActive(active: false);
		}
	}

	private void Update()
	{
		if (!Bi2xInput.GetKeyDown(KeyCode.Space))
		{
			return;
		}
		Debug.Log("Taku情報");
		foreach (Sticker sticker in Stickers)
		{
			sticker.GetLog();
		}
	}

	public async UniTask Init(Cha cha, List<ItemID> itemIDs, List<StickerEditInfo> stickerEditInfos, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		DumpFrames[(int)cha].SetActive(active: true);
		Stickers[(int)cha].InitStickers();
		if (itemIDs.Count <= 0)
		{
			return;
		}
		foreach (StickerEditInfo item in stickerEditInfos.OrderBy((StickerEditInfo x) => x.Priority))
		{
			await Stickers[(int)cha].SetSticker(item, StickerObject, cancellationToken);
		}
	}

	public void StickerOption(Cha cha, bool isAllDisp, List<ItemID> itemIDs, List<StickerEditInfo> stickerEditInfos)
	{
		if (itemIDs.Count > 0)
		{
			Stickers[(int)cha].SetDispOption(isAllDisp, stickerEditInfos.MaxElement((StickerEditInfo x) => x.Priority));
		}
	}
}
