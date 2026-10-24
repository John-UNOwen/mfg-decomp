using UnityEngine;
using UnityEngine.UI;

public class SliderSnap : MonoBehaviour
{
	public Slider slider;

	public float step = 0.5f;

	private void Start()
	{
		slider.onValueChanged.AddListener(OnSliderValueChanged);
	}

	private void OnSliderValueChanged(float value)
	{
		float value2 = Mathf.Round(value / step) * step;
		slider.value = value2;
	}
}
