using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Konami.VirtualSurround.Demo;

public class VolumeController : MonoBehaviour
{
	private struct VolumeFader
	{
		public Slider slider;

		public Text value;

		public Text label;
	}

	[SerializeField]
	private AudioMixer mixer;

	[SerializeField]
	private GameObject frontSlider;

	[SerializeField]
	private GameObject surroundSlider;

	[SerializeField]
	private GameObject wooferSlider;

	private VolumeFader frontFader;

	private VolumeFader surroundFader;

	private VolumeFader wooferFader;

	private VirtualSurroundGain surroundGain = new VirtualSurroundGain();

	private void Start()
	{
		frontFader = CreateVolumeFader(frontSlider.transform);
		surroundFader = CreateVolumeFader(surroundSlider.transform);
		wooferFader = CreateVolumeFader(wooferSlider.transform);
		surroundGain.mixer = mixer;
	}

	private VolumeFader CreateVolumeFader(Transform parent)
	{
		VolumeFader result = default(VolumeFader);
		result.slider = parent.GetComponent<Slider>();
		result.label = parent.Find("Label")?.GetComponent<Text>();
		result.value = parent.Find("Text")?.GetComponent<Text>();
		return result;
	}

	private void Update()
	{
	}

	private void OnGUI()
	{
		float value = frontFader.slider.value;
		float value2 = surroundFader.slider.value;
		float value3 = wooferFader.slider.value;
		surroundGain.SetFrontGain(value);
		surroundGain.SetSurroundGain(value2);
		surroundGain.SetWooferGain(value3);
		frontFader.value.text = $"{frontFader.slider.value:F2} dB";
		surroundFader.value.text = $"{surroundFader.slider.value:F2} dB";
		wooferFader.value.text = $"{wooferFader.slider.value:F2} dB";
	}
}
