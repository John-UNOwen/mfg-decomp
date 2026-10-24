using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace MFG;

public abstract class DebugTapper : MonoBehaviour
{
	public struct LogItem
	{
		public enum EventType
		{
			Down,
			Up,
			Click,
			BeginDrag,
			Drag,
			EndDrag,
			Drop,
			Enter,
			Exit
		}

		public readonly DateTime Time;

		public readonly int PointerId;

		public readonly EventType Type;

		public readonly Vector2 Position;

		public readonly GameObject GameObject;

		public LogItem(int pointerId, EventType type, Vector2 position, GameObject gameObject)
		{
			Time = DateTime.Now;
			Type = type;
			PointerId = pointerId;
			Position = position;
			GameObject = gameObject;
		}

		public override string ToString()
		{
			object[] array = new object[5];
			DateTime time = Time;
			array[0] = time.ToString("HH:mm:ss.fff");
			array[1] = PointerId;
			array[2] = Type;
			array[3] = Position;
			array[4] = GameObject.name;
			return string.Format("{0}\t{1}\t{2}\t{3}\t{4}", array);
		}
	}

	private class Tap
	{
		public Image mark;

		public Vector2 dragFrom;

		public Vector2 dragTo;

		public PointerEventData pointerEvent;

		public float time;

		public float duration;

		public Tap(Image mark, int pointerId)
		{
			this.mark = mark;
			time = float.MaxValue;
			pointerEvent = new PointerEventData(EventSystem.current);
			pointerEvent.pointerId = pointerId;
		}
	}

	[SerializeField]
	private Sprite markSprite;

	[SerializeField]
	private bool autoStartEnabled;

	[SerializeField]
	private int tapCount = 8;

	[SerializeField]
	private int logSize = 8192;

	[SerializeField]
	private float doubleClickThreshold = 0.3f;

	public const int DefaultLogSize = 8192;

	private Tap[] taps;

	private List<RaycastResult> tmpRaycastResults;

	private CustomSampler upSampler;

	private CustomSampler downSampler;

	private CustomSampler clickSampler;

	private CustomSampler beginDragSampler;

	private CustomSampler dragSampler;

	private CustomSampler endDragSampler;

	private CustomSampler enterSampler;

	private CustomSampler exitSampler;

	private CustomSampler dropSampler;

	private LogItem[] logItems;

	private int nextLogPosition;

	public IEnumerable<LogItem> LogItems
	{
		get
		{
			for (int i = 0; i < logItems.Length; i++)
			{
				int num = nextLogPosition + i;
				if (num >= logItems.Length)
				{
					num -= logItems.Length;
				}
				if (logItems[num].GameObject != null)
				{
					yield return logItems[num];
				}
			}
		}
	}

	public string LogText
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (LogItem logItem in LogItems)
			{
				stringBuilder.AppendLine(logItem.ToString());
			}
			return stringBuilder.ToString();
		}
	}

	protected int TapCount => taps.Length;

	private void Start()
	{
		if (autoStartEnabled)
		{
			ManualStart(tapCount, markSprite, logSize);
		}
	}

	public void ManualStart(int tapCount, Sprite markSprite, int logSize = 8192)
	{
		logItems = new LogItem[logSize];
		nextLogPosition = 0;
		taps = new Tap[tapCount];
		Canvas canvas = base.gameObject.AddComponent<Canvas>();
		canvas.sortingOrder = 32767;
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		for (int i = 0; i < tapCount; i++)
		{
			GameObject obj = new GameObject("TapMark" + i);
			obj.transform.SetParent(base.gameObject.transform, worldPositionStays: false);
			Image image = obj.AddComponent<Image>();
			image.rectTransform.anchorMin = Vector2.zero;
			image.rectTransform.anchorMax = Vector2.zero;
			image.enabled = false;
			image.sprite = markSprite;
			image.SetNativeSize();
			image.rectTransform.sizeDelta = new Vector2(10f, 10f);
			taps[i] = new Tap(image, 100 + i);
		}
		tmpRaycastResults = new List<RaycastResult>();
		upSampler = CustomSampler.Create("OnPointerUp");
		downSampler = CustomSampler.Create("OnPointerDown");
		clickSampler = CustomSampler.Create("OnPointerClick");
		beginDragSampler = CustomSampler.Create("OnBeginDrag");
		endDragSampler = CustomSampler.Create("OnEndDrag");
		dragSampler = CustomSampler.Create("OnDrag");
		enterSampler = CustomSampler.Create("OnPointerEnter");
		exitSampler = CustomSampler.Create("OnPointerExit");
		dropSampler = CustomSampler.Create("OnDrop");
	}

	protected abstract void UpdateTap(int tapIndex);

	protected virtual bool ToBeIgnored(GameObject gameObject)
	{
		return false;
	}

	protected void Fire(int tapIndex, Vector2 fromScreenPosition, Vector2 toScreenPosition, float duration)
	{
		Tap tap = taps[tapIndex];
		tap.pointerEvent.position = fromScreenPosition;
		tap.dragFrom = fromScreenPosition;
		tap.dragTo = toScreenPosition;
		tap.mark.rectTransform.anchoredPosition = tap.pointerEvent.position;
		tap.duration = duration;
		GameObject gameObject = Raycast(tap.pointerEvent);
		if (gameObject != null)
		{
			Down(tap, gameObject);
		}
		tap.mark.enabled = true;
	}

	private GameObject Raycast(PointerEventData pointerEventData)
	{
		tmpRaycastResults.Clear();
		EventSystem.current.RaycastAll(pointerEventData, tmpRaycastResults);
		for (int i = 0; i < tmpRaycastResults.Count; i++)
		{
			if (!ToBeIgnored(tmpRaycastResults[i].gameObject))
			{
				pointerEventData.pointerCurrentRaycast = tmpRaycastResults[i];
				return tmpRaycastResults[i].gameObject;
			}
		}
		return null;
	}

	private void OnDestroy()
	{
		if (taps != null)
		{
			for (int i = 0; i < taps.Length; i++)
			{
				UnityEngine.Object.Destroy(taps[i].mark.gameObject);
			}
		}
	}

	private void Update()
	{
		if (taps != null && !(EventSystem.current == null))
		{
			float deltaTime = Time.deltaTime;
			for (int i = 0; i < taps.Length; i++)
			{
				UpdateTap(i, deltaTime);
			}
		}
	}

	private void OnDisable()
	{
		if (taps != null)
		{
			Tap[] array = taps;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].mark.enabled = false;
			}
		}
	}

	private void UpdateTap(int tapIndex, float deltaTime)
	{
		Tap tap = taps[tapIndex];
		if (tap.time < tap.duration)
		{
			tap.time += deltaTime;
			float value = ((!(tap.duration <= 0f)) ? (tap.time / tap.duration) : 1f);
			value = Mathf.Clamp01(value);
			Vector2 vector = Vector2.Lerp(tap.dragFrom, tap.dragTo, value);
			Vector2 delta = vector - tap.pointerEvent.position;
			tap.pointerEvent.delta = delta;
			tap.mark.rectTransform.anchoredPosition = vector;
			tap.pointerEvent.position = vector;
			GameObject hitObject = Raycast(tap.pointerEvent);
			if (value >= 1f)
			{
				Up(tap, hitObject);
				return;
			}
			if (tap.pointerEvent.dragging)
			{
				Drag(tap, hitObject);
				return;
			}
			float sqrMagnitude = (vector - tap.dragFrom).sqrMagnitude;
			int pixelDragThreshold = EventSystem.current.pixelDragThreshold;
			if (sqrMagnitude >= (float)(pixelDragThreshold * pixelDragThreshold))
			{
				BeginDrag(tap);
			}
		}
		else
		{
			if (tap.pointerEvent.dragging)
			{
				EndDrag(tap, null);
			}
			UpdateTap(tapIndex);
		}
	}

	private void Log(LogItem.EventType type, PointerEventData ev, GameObject gameObject)
	{
		logItems[nextLogPosition] = new LogItem(ev.pointerId, type, ev.position, gameObject);
		nextLogPosition++;
		if (nextLogPosition >= logItems.Length)
		{
			nextLogPosition = 0;
		}
	}

	private void Down(Tap tap, GameObject hitObject)
	{
		PointerEventData pointerEvent = tap.pointerEvent;
		pointerEvent.delta = Vector2.zero;
		pointerEvent.dragging = false;
		pointerEvent.useDragThreshold = true;
		pointerEvent.pressPosition = pointerEvent.position;
		pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;
		pointerEvent.rawPointerPress = hitObject;
		GameObject gameObject = ExecuteEvents.ExecuteHierarchy(hitObject, pointerEvent, delegate(IPointerDownHandler handler, BaseEventData data)
		{
			handler.OnPointerDown((PointerEventData)data);
		});
		if (gameObject != null)
		{
			pointerEvent.pointerPress = gameObject;
			Log(LogItem.EventType.Down, pointerEvent, gameObject);
		}
		else
		{
			gameObject = ExecuteEvents.GetEventHandler<IPointerClickHandler>(hitObject);
		}
		float unscaledTime = Time.unscaledTime;
		int clickCount = 1;
		if (gameObject == pointerEvent.lastPress && unscaledTime - pointerEvent.clickTime < doubleClickThreshold)
		{
			clickCount = pointerEvent.clickCount + 1;
		}
		pointerEvent.clickTime = unscaledTime;
		pointerEvent.clickCount = clickCount;
		tap.time = 0f;
	}

	private void Up(Tap tap, GameObject hitObject)
	{
		PointerEventData pointerEvent = tap.pointerEvent;
		GameObject rawPointerPress = pointerEvent.rawPointerPress;
		if (rawPointerPress != null)
		{
			GameObject gameObject = ExecuteEvents.ExecuteHierarchy(rawPointerPress, pointerEvent, delegate(IPointerUpHandler handler, BaseEventData data)
			{
				handler.OnPointerUp((PointerEventData)data);
			});
			if (gameObject != null)
			{
				Log(LogItem.EventType.Up, pointerEvent, gameObject);
			}
			if (rawPointerPress == hitObject)
			{
				Click(tap);
			}
			pointerEvent.pointerPress = null;
			pointerEvent.rawPointerPress = null;
		}
		if (pointerEvent.dragging)
		{
			EndDrag(tap, hitObject);
		}
		tap.time = float.MaxValue;
		tap.mark.enabled = false;
	}

	private void Click(Tap tap)
	{
		PointerEventData pointerEvent = tap.pointerEvent;
		GameObject gameObject = ExecuteEvents.ExecuteHierarchy(pointerEvent.rawPointerPress, pointerEvent, delegate(IPointerClickHandler handler, BaseEventData data)
		{
			handler.OnPointerClick((PointerEventData)data);
		});
		if (gameObject != null)
		{
			Log(LogItem.EventType.Click, pointerEvent, gameObject);
		}
	}

	private void BeginDrag(Tap tap)
	{
		PointerEventData pointerEvent = tap.pointerEvent;
		GameObject gameObject = pointerEvent.rawPointerPress;
		if (gameObject == null)
		{
			gameObject = pointerEvent.pointerPress;
		}
		if (!(gameObject != null))
		{
			return;
		}
		pointerEvent.dragging = true;
		pointerEvent.useDragThreshold = true;
		GameObject gameObject2 = ExecuteEvents.ExecuteHierarchy(gameObject, pointerEvent, delegate(IBeginDragHandler handler, BaseEventData data)
		{
			handler.OnBeginDrag((PointerEventData)data);
		});
		if (gameObject2 != null)
		{
			Log(LogItem.EventType.BeginDrag, pointerEvent, gameObject2);
		}
		else
		{
			gameObject2 = ExecuteEvents.GetEventHandler<IDragHandler>(gameObject);
			if (gameObject2 == null)
			{
				gameObject2 = ExecuteEvents.GetEventHandler<IEndDragHandler>(gameObject);
			}
		}
		pointerEvent.pointerDrag = gameObject2;
	}

	private void Drag(Tap tap, GameObject hitObject)
	{
		PointerEventData pointerEvent = tap.pointerEvent;
		GameObject pointerDrag = pointerEvent.pointerDrag;
		if (pointerDrag == null)
		{
			Up(tap, hitObject);
		}
		else if (ExecuteEvents.ExecuteHierarchy(pointerDrag, pointerEvent, delegate(IDragHandler handler, BaseEventData data)
		{
			handler.OnDrag((PointerEventData)data);
		}) != null)
		{
			Log(LogItem.EventType.Drag, pointerEvent, pointerDrag);
		}
	}

	private void EndDrag(Tap tap, GameObject hitObject)
	{
		PointerEventData pointerEvent = tap.pointerEvent;
		Drop(tap, hitObject);
		GameObject pointerDrag = pointerEvent.pointerDrag;
		if (pointerDrag != null && ExecuteEvents.ExecuteHierarchy(pointerDrag, pointerEvent, delegate(IEndDragHandler handler, BaseEventData data)
		{
			handler.OnEndDrag((PointerEventData)data);
		}) != null)
		{
			Log(LogItem.EventType.EndDrag, pointerEvent, pointerDrag);
		}
		pointerEvent.dragging = false;
		pointerEvent.pointerDrag = null;
		pointerEvent.useDragThreshold = false;
	}

	private void Drop(Tap tap, GameObject hitObject)
	{
		PointerEventData pointerEvent = tap.pointerEvent;
		GameObject gameObject = ExecuteEvents.ExecuteHierarchy(hitObject, pointerEvent, delegate(IDropHandler handler, BaseEventData data)
		{
			handler.OnDrop((PointerEventData)data);
		});
		if (gameObject != null)
		{
			Log(LogItem.EventType.Drop, pointerEvent, gameObject);
		}
	}
}
