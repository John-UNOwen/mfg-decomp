using System;
using System.Collections.Generic;
using MFG.Types;
using UnityEngine;

[Serializable]
public class PlayerAccessoryViewData : MonoBehaviour
{
	[Serializable]
	public class ViewElements
	{
		[Header("ViewType毎のデータ")]
		public List<ViewElement> _viewElements = new List<ViewElement>(new ViewElement[3]);
	}

	[Serializable]
	public class ViewElement
	{
		[SerializeField]
		public Vector2 position;

		[SerializeField]
		public float scaleX = 1f;

		[SerializeField]
		public float scaleY = 1f;
	}

	[Header("家毎のデータ")]
	[SerializeField]
	private List<ViewElements> _chaElements = new List<ViewElements>(new ViewElements[4]);

	public ViewElement GetViewElement(Cha cha, ViewType viewType)
	{
		return _chaElements[(int)cha]._viewElements[(int)viewType];
	}
}
