using System;
using UnityEngine;

[Serializable]
public class RemovableItem
{
	[Header("オブジェクト")]
	public GameObject Object;

	[Header("着脱")]
	public bool IsDisp;
}
