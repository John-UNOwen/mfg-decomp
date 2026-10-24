using UnityEngine;
using UnityEngine.EventSystems;

namespace UniSimpleProfiler.Internal;

[DisallowMultipleComponent]
public sealed class Dragable : MonoBehaviour, IDragHandler, IEventSystemHandler
{
	[SerializeField]
	[HideInInspector]
	private RectTransform m_rectTransform;

	private void Reset()
	{
		m_rectTransform = GetComponent<RectTransform>();
	}

	public void OnDrag(PointerEventData e)
	{
		m_rectTransform.position += new Vector3(e.delta.x, e.delta.y, 0f);
	}
}
