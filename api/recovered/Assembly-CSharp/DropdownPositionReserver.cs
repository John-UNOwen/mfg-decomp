using UnityEngine;
using UnityEngine.UI;

public class DropdownPositionReserver : MonoBehaviour
{
	private void Start()
	{
		Dropdown componentInParent = GetComponentInParent<Dropdown>();
		if (!(componentInParent == null))
		{
			RectTransform component = base.transform.Find("Viewport").GetComponent<RectTransform>();
			RectTransform component2 = component.Find("Content").GetComponent<RectTransform>();
			RectTransform component3 = component2.Find("Item").GetComponent<RectTransform>();
			float num = component2.rect.height - component.rect.height;
			float value = component3.rect.height * (float)componentInParent.value / num;
			GetComponent<ScrollRect>().verticalNormalizedPosition = 1f - Mathf.Clamp(value, 0f, 1f);
		}
	}
}
