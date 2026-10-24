using UnityEngine;
using UnityEngine.UI;

namespace Konami.VirtualSurround.Demo;

[RequireComponent(typeof(RectTransform))]
public class VUMeterUI : MonoBehaviour
{
	[SerializeField]
	private VUMeter vuMeter;

	private float minDb = -96f;

	private float maxDb = 20f;

	private Image[] vuImages;

	private float maxWidth;

	private RectTransform vuMeterGroup;

	private void Start()
	{
		vuMeter.Initialize();
		vuMeterGroup = GetComponent<RectTransform>();
		vuImages = vuMeterGroup.GetComponentsInChildren<Image>();
		maxWidth = vuMeterGroup.sizeDelta.x;
	}

	private void OnGUI()
	{
		vuMeter.UpdateVU();
		float[] vu = vuMeter.vu;
		for (int i = 0; i < vuImages.Length; i++)
		{
			Vector2 sizeDelta = vuImages[i].rectTransform.sizeDelta;
			sizeDelta.x = maxWidth * Mathf.Clamp01(vu[i] / Mathf.Abs(maxDb - minDb) + 1f);
			vuImages[i].rectTransform.sizeDelta = sizeDelta;
		}
	}
}
