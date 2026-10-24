using System;
using MFG.Types.Customize.CustomizeItem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MyRoomStickerDrag : MonoBehaviour, IDragHandler, IEventSystemHandler
{
	private bool IsSelect;

	private MyRoomStickerEditor Editor;

	private ItemID ItemID;

	public void Init(MyRoomStickerEditor editor, ItemID itemID)
	{
		Editor = editor;
		ItemID = itemID;
	}

	public void ChangeImage(bool isSelect)
	{
		IsSelect = isSelect;
		Color color = base.transform.Find("Image").GetComponent<Image>().color;
		color.a = (IsSelect ? 1f : 0.5f);
		base.transform.Find("Image").GetComponent<Image>().color = color;
		base.transform.Find("Choice").gameObject.SetActive(IsSelect);
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (IsSelect)
		{
			Vector3 vector = ResidentFlow.GetInstance().MainCamera().WorldToScreenPoint(base.transform.position);
			Vector3 position = new Vector3(Bi2xInput.Instance.GetTouchPosition().x, Bi2xInput.Instance.GetTouchPosition().y, vector.z);
			Vector3 position2 = ResidentFlow.GetInstance().MainCamera().ScreenToWorldPoint(position);
			base.transform.position = position2;
			Vector2 vector2 = new Vector2(base.transform.localPosition.x, base.transform.localPosition.y);
			Debug.Log($"{vector2.x} {vector2.y}");
			int num = ((vector2.x <= (float)Editor.PosXMinValue) ? Editor.PosXMinValue : ((vector2.x >= (float)Editor.PosXMaxValue) ? Editor.PosXMaxValue : ((int)(Math.Round(vector2.x / 10f, 0, MidpointRounding.AwayFromZero) * 10.0))));
			int num2 = ((vector2.y <= (float)Editor.PosYMinValue) ? Editor.PosYMinValue : ((vector2.y >= (float)Editor.PosYMaxValue) ? Editor.PosYMaxValue : ((int)(Math.Round(vector2.y / 10f, 0, MidpointRounding.AwayFromZero) * 10.0))));
			if (Editor.PosYMinValue >= Editor.PosYMaxValue)
			{
				num2 = -30;
			}
			base.transform.localPosition = new Vector2(num, num2);
			MyRoomStickerEditor.EditInfo editInfo = Editor.EditInfos.Find((MyRoomStickerEditor.EditInfo x) => x.Info.ItemID == ItemID);
			editInfo.Info.IsEdited = true;
			editInfo.Info.PositionX = num;
			editInfo.Info.PositionY = num2;
			editInfo.Obj.transform.localPosition = new Vector2(num, num2);
			Editor.CurrentPos = new Vector2(num, num2);
			Editor.RefreshButton();
		}
	}
}
