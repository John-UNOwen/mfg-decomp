using System;
using System.Collections.Generic;
using System.Linq;
using MFG.Types;
using UnityEngine;

public class ViewManager : MonoBehaviour
{
	[Serializable]
	public class Transform3D
	{
		[SerializeField]
		public Vector3 position;

		[SerializeField]
		public Vector3 rotation;

		[SerializeField]
		public float scale;
	}

	[Serializable]
	public class ViewData3D
	{
		[SerializeField]
		public GameObject obj3D;

		[SerializeField]
		public List<Transform3D> transforms = new List<Transform3D>();

		public void Reflesh(ViewType viewType)
		{
			if (!(obj3D == null))
			{
				Transform3D transform3D = transforms[(int)viewType];
				obj3D.transform.localPosition = transform3D.position;
				obj3D.transform.localRotation = Quaternion.Euler(transform3D.rotation);
				obj3D.transform.localScale = new Vector3(transform3D.scale, transform3D.scale, transform3D.scale);
			}
		}
	}

	[Serializable]
	public class ViewData3D_TakuType
	{
		[SerializeField]
		public GameObject obj3D;

		[SerializeField]
		public List<Transform3D> transforms = new List<Transform3D>();

		[SerializeField]
		public List<Transform3D> transformsNima = new List<Transform3D>();

		public void Reflesh(ViewType viewType, TakuType takuType)
		{
			if (!(obj3D == null))
			{
				Transform3D transform3D = transforms[(int)viewType];
				if (takuType == TakuType.Nima)
				{
					transform3D = transformsNima[(int)viewType];
				}
				obj3D.transform.localPosition = transform3D.position;
				obj3D.transform.localRotation = Quaternion.Euler(transform3D.rotation);
				obj3D.transform.localScale = new Vector3(transform3D.scale, transform3D.scale, transform3D.scale);
			}
		}
	}

	[Serializable]
	public class ViewData2D
	{
		[SerializeField]
		public GameObject obj2D;

		[SerializeField]
		public List<Vector2> positions = new List<Vector2>();

		public void Reflesh(ViewType viewType)
		{
			if (!(obj2D == null))
			{
				Vector2 anchoredPosition = positions[(int)viewType];
				obj2D.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
			}
		}
	}

	[Serializable]
	public class ViewData2DDetail
	{
		[Serializable]
		public class ViewElement
		{
			[SerializeField]
			public GameObject parent;

			[SerializeField]
			public Vector2 position;

			[SerializeField]
			public float scaleX = 1f;

			[SerializeField]
			public float scaleY = 1f;
		}

		[SerializeField]
		public GameObject obj2D;

		[SerializeField]
		public List<ViewElement> viewElements = new List<ViewElement>();

		public void Reflesh(ViewType viewType, Cha cha = Cha.Jicha)
		{
			if (!(obj2D == null))
			{
				ViewElement viewElement = viewElements[(int)viewType];
				RectTransform component = obj2D.GetComponent<RectTransform>();
				PlayerAccessoryViewData componentInChildren = obj2D.GetComponentInChildren<PlayerAccessoryViewData>();
				Vector2 vector = new Vector2(0f, 0f);
				Vector2 vector2 = new Vector2(1f, 1f);
				if (componentInChildren != null)
				{
					PlayerAccessoryViewData.ViewElement viewElement2 = componentInChildren.GetViewElement(cha, viewType);
					vector = viewElement2.position;
					vector2.x = viewElement2.scaleX;
					vector2.y = viewElement2.scaleY;
				}
				component.anchoredPosition = viewElement.position + vector;
				component.localScale = new Vector3(viewElement.scaleX * vector2.x, viewElement.scaleY * vector2.y, 1f);
				if (viewElement.parent != null)
				{
					obj2D.transform.SetParent(viewElement.parent.transform, worldPositionStays: false);
				}
			}
		}
	}

	[Serializable]
	public class ViewDataActive
	{
		[SerializeField]
		public GameObject obj;

		[SerializeField]
		public List<bool> actives = new List<bool>();

		public void Reflesh(ViewType viewType)
		{
			if (!(obj == null))
			{
				bool active = actives[(int)viewType];
				obj.SetActive(active);
			}
		}
	}

	[Serializable]
	public class ViewDataChara
	{
		[Serializable]
		public class ViewElement
		{
			[SerializeField]
			public GameObject objTop;

			[SerializeField]
			public GameObject objBottom;

			[SerializeField]
			public Vector2 position;
		}

		[SerializeField]
		public Character character;

		[SerializeField]
		public GameObject objFrame;

		[SerializeField]
		public List<ViewElement> viewElements = new List<ViewElement>();

		public void Reflesh(ViewType viewType, Cha cha)
		{
			ViewElement viewElement = viewElements[(int)viewType];
			objFrame.SetActive(viewType == ViewType.Type3);
			string[] array = new string[4] { "Zi", "Shimo", "Toi", "Kami" };
			Transform transform = objFrame.transform.Find(array[(int)cha] ?? "");
			if (character.player.IsValid)
			{
				transform?.gameObject.SetActive(value: true);
			}
			else
			{
				transform?.gameObject.SetActive(value: false);
			}
			if (viewElement.objTop != null)
			{
				viewElement.objTop.GetComponent<RectTransform>().anchoredPosition = viewElement.position;
			}
			if (viewElement.objBottom != null)
			{
				viewElement.objBottom.GetComponent<RectTransform>().anchoredPosition = viewElement.position;
			}
		}
	}

	[Serializable]
	public class ViewDataPlayer
	{
		[SerializeField]
		public Player player;

		[SerializeField]
		public ViewData3D hand;

		[SerializeField]
		public ViewData3D meld;

		[SerializeField]
		public ViewData3D_TakuType dump;

		[SerializeField]
		public ViewData3D plate;

		[SerializeField]
		public ViewDataChara chara;

		[SerializeField]
		public ViewData2D machihaiPanel;

		[SerializeField]
		public ViewData2D name;

		[SerializeField]
		public ViewData2DDetail accessory;

		[SerializeField]
		public ViewData2D excall;

		[SerializeField]
		public ViewData2DDetail proStand;

		public void Reflesh(ViewType viewType, Cha cha)
		{
			hand.Reflesh(viewType);
			meld.Reflesh(viewType);
			dump.Reflesh(viewType, GameDataManager.Mahjong.TakuType);
			plate.Reflesh(viewType);
			chara.Reflesh(viewType, cha);
			machihaiPanel.Reflesh(viewType);
			name.Reflesh(viewType);
			accessory.Reflesh(viewType, cha);
			excall.Reflesh(viewType);
			proStand.Reflesh(viewType);
			dump.obj3D.GetComponent<Dump>().Reflesh();
			player.OpenReflesh();
		}
	}

	[Serializable]
	public class ActiveSwitcher
	{
		[SerializeField]
		public List<GameObject> viewElements = new List<GameObject>();

		public void Reflesh(ViewType viewType)
		{
			foreach (GameObject viewElement in viewElements)
			{
				if (!(viewElement == null))
				{
					viewElement.SetActive(value: false);
				}
			}
			viewElements[(int)viewType].SetActive(value: true);
		}
	}

	[Header("[プレイヤー]")]
	[SerializeField]
	public List<ViewDataPlayer> players = new List<ViewDataPlayer>();

	[Header("[3Dオブジェクト]")]
	[SerializeField]
	public ViewData3D camera3D;

	[SerializeField]
	public ViewData3D wanpai;

	[SerializeField]
	public ActiveSwitcher playerCamera;

	[Header("[2Dオブジェクト]")]
	[SerializeField]
	public ViewData2D hainavi;

	[SerializeField]
	public ViewData2DDetail mychara;

	[SerializeField]
	public ViewData2DDetail scoreDiffChangeArea;

	[SerializeField]
	public ViewData2D accelDoraRemainCounter;

	[SerializeField]
	public ViewData2D meldBonusRemainCounter;

	[SerializeField]
	public ViewData2D bombRemainCounter;

	[Header("[active切替オブジェクト]")]
	[SerializeField]
	public ViewDataActive wanpaiTexture;

	[SerializeField]
	public ViewDataActive wanpaiCamera;

	[SerializeField]
	public ViewDataActive bgShadowOutFrame;

	[SerializeField]
	public ViewDataActive bgShadowCenter;

	[Header("[その他]")]
	[SerializeField]
	public GameObject taku;

	[SerializeField]
	public GameObject proStand;

	[SerializeField]
	public GameObject proStandParent1;

	[SerializeField]
	public GameObject proStandParent2;

	public void ChangeViewType(ViewType viewType)
	{
		foreach (var (viewDataPlayer, cha) in players.Select((ViewDataPlayer p, int i) => (p: p, i: i)))
		{
			viewDataPlayer.Reflesh(viewType, (Cha)cha);
		}
		camera3D.Reflesh(viewType);
		wanpai.Reflesh(viewType);
		playerCamera.Reflesh(viewType);
		hainavi.Reflesh(viewType);
		scoreDiffChangeArea.Reflesh(viewType);
		accelDoraRemainCounter.Reflesh(viewType);
		meldBonusRemainCounter.Reflesh(GameDataManager.Mahjong.IsAotenjoMode ? ViewType.Type2 : viewType);
		bombRemainCounter.Reflesh(GameDataManager.Mahjong.IsAotenjoMode ? ViewType.Type2 : viewType);
		wanpaiTexture.Reflesh(viewType);
		wanpaiCamera.Reflesh(viewType);
		bgShadowOutFrame.Reflesh(viewType);
		bgShadowCenter.Reflesh(viewType);
		ChangeViewTaku(viewType);
		ChangeViewProStand(viewType);
	}

	public void ForceAllUIDispRefresh(ViewType viewType)
	{
		accelDoraRemainCounter.Reflesh(viewType);
		meldBonusRemainCounter.Reflesh(ViewType.Type2);
		bombRemainCounter.Reflesh(ViewType.Type2);
	}

	public void ChangeProView(bool isPro)
	{
		if (isPro)
		{
			mychara.Reflesh(ViewType.Type3);
		}
		else
		{
			mychara.Reflesh(ViewType.Type1);
		}
	}

	private void ChangeViewProStand(ViewType viewType)
	{
		if (viewType == ViewType.Type1)
		{
			proStand.transform.SetParent(proStandParent1.transform);
		}
		else
		{
			proStand.transform.SetParent(proStandParent2.transform);
		}
	}

	private void ChangeViewTaku(ViewType viewType)
	{
		string trigger = "";
		foreach (Transform item in taku.transform)
		{
			if (item.gameObject.activeSelf)
			{
				Animator component = item.GetComponent<Animator>();
				if (component.GetCurrentAnimatorStateInfo(0).IsName("TakuWait"))
				{
					trigger = "Wait";
				}
				else if (component.GetCurrentAnimatorStateInfo(0).IsName("TakuInZi"))
				{
					trigger = "Zi";
				}
				else if (component.GetCurrentAnimatorStateInfo(0).IsName("TakuInShimo"))
				{
					trigger = "Shimo";
				}
				else if (component.GetCurrentAnimatorStateInfo(0).IsName("TakuInToi"))
				{
					trigger = "Toi";
				}
				else if (component.GetCurrentAnimatorStateInfo(0).IsName("TakuInKami"))
				{
					trigger = "Kami";
				}
			}
		}
		foreach (Transform item2 in taku.transform)
		{
			item2.gameObject.SetActive(value: false);
		}
		if (GameDataManager.Mahjong.TakuType == TakuType.Nima)
		{
			string[] array = new string[3] { "View1B", "View2B", "View3B" };
			taku.transform.Find(array[(int)MahjongUtility.GetViewType()]).gameObject.SetActive(value: true);
		}
		else
		{
			string[] array2 = new string[3] { "View1A", "View2A", "View3A" };
			taku.transform.Find(array2[(int)MahjongUtility.GetViewType()]).gameObject.SetActive(value: true);
		}
		foreach (Transform item3 in taku.transform)
		{
			if (item3.gameObject.activeSelf)
			{
				item3.GetComponent<Animator>().SetTrigger(trigger);
			}
		}
	}
}
