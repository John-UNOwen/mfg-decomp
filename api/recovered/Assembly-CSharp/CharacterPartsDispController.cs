using System.Collections.Generic;
using UnityEngine;

public class CharacterPartsDispController : MonoBehaviour
{
	[SerializeField]
	private List<Transform> _parts = new List<Transform>();

	public void SetExclusiveDisp(List<int> indexes)
	{
		for (int i = 0; i < _parts.Count; i++)
		{
			Parts_SetDisp(i, isDisp: false);
		}
		foreach (int index in indexes)
		{
			Parts_SetDisp(index, isDisp: true);
		}
	}

	public void SetExclusiveDisp(int index)
	{
		SetExclusiveDisp(new List<int> { index });
	}

	public void SetDispAll(bool isDisp)
	{
		for (int i = 0; i < _parts.Count; i++)
		{
			Parts_SetDisp(i, isDisp);
		}
	}

	private void Parts_SetDisp(int index, bool isDisp)
	{
		if (IsValid(index))
		{
			Transform transform = _parts[index];
			if (transform != null)
			{
				transform.gameObject.SetActive(isDisp);
			}
		}
	}

	private bool IsValid(int index)
	{
		if (0 <= index)
		{
			return index < _parts.Count;
		}
		return false;
	}
}
