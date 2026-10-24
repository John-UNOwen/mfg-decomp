using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class TestModeTextBlink : MonoBehaviour
{
	[SerializeField]
	[Tooltip("点滅間隔の秒数")]
	private float blinkIntervalSec = 0.5f;

	private Text text;

	private bool isStart;

	private void Awake()
	{
		text = GetComponent<Text>();
	}

	public void StartBlink()
	{
		isStart = true;
	}

	public void StopBlink()
	{
		if (isStart)
		{
			Color color = text.color;
			color.a = 1f;
			text.color = color;
			isStart = false;
		}
	}

	private void Update()
	{
		if (isStart)
		{
			Color color = text.color;
			color.a = ((Time.time % (blinkIntervalSec * 2f) < blinkIntervalSec) ? 0.6f : 1f);
			text.color = color;
		}
	}
}
