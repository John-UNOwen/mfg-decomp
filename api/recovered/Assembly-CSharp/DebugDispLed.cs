using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugDispLed : MonoBehaviour
{
	[SerializeField]
	private Transform left;

	[SerializeField]
	private Transform right;

	[SerializeField]
	private Transform up;

	[SerializeField]
	private Transform down;

	[SerializeField]
	private GameObject led_object;

	private List<Image> image_L = new List<Image>();

	private List<Image> image_R = new List<Image>();

	private List<Image> image_UP = new List<Image>();

	private List<Image> image_DOWN = new List<Image>();

	private void Awake()
	{
		Object.Destroy(base.gameObject);
	}

	private void Start()
	{
		LEDManager instance = LEDManager.Instance;
		for (int i = 0; i < instance.GetLedNum(Bi2xInput.TAPELED.TAPELED_LEFT_PANEL); i++)
		{
			GameObject gameObject = Object.Instantiate(led_object, left);
			gameObject.name = led_object.name + i;
			gameObject.SetActive(value: true);
			image_L.Add(gameObject.GetComponent<Image>());
		}
		for (int j = 0; j < instance.GetLedNum(Bi2xInput.TAPELED.TAPELED_RIGHT_PANEL); j++)
		{
			GameObject gameObject2 = Object.Instantiate(led_object, right);
			gameObject2.name = led_object.name + j;
			gameObject2.SetActive(value: true);
			image_R.Add(gameObject2.GetComponent<Image>());
		}
		for (int k = 0; k < instance.GetLedNum(Bi2xInput.TAPELED.TAPELED_CTRL_PANEL) / 2; k++)
		{
			GameObject gameObject3 = Object.Instantiate(led_object, up);
			gameObject3.name = led_object.name + k;
			gameObject3.SetActive(value: true);
			image_UP.Add(gameObject3.GetComponent<Image>());
		}
		for (int l = 0; l < instance.GetLedNum(Bi2xInput.TAPELED.TAPELED_CTRL_PANEL) / 2; l++)
		{
			GameObject gameObject4 = Object.Instantiate(led_object, down);
			gameObject4.name = led_object.name + l;
			gameObject4.SetActive(value: true);
			image_DOWN.Add(gameObject4.GetComponent<Image>());
		}
	}

	private void Update()
	{
		if (!DebugFlag.IsActive(DebugFlag.Flag.LedDisp))
		{
			return;
		}
		LEDManager instance = LEDManager.Instance;
		for (int i = 0; i < instance.GetLedNum(Bi2xInput.TAPELED.TAPELED_LEFT_PANEL); i++)
		{
			image_L[i].color = instance.GetLedColor(Bi2xInput.TAPELED.TAPELED_LEFT_PANEL, i);
		}
		for (int j = 0; j < instance.GetLedNum(Bi2xInput.TAPELED.TAPELED_RIGHT_PANEL); j++)
		{
			image_R[j].color = instance.GetLedColor(Bi2xInput.TAPELED.TAPELED_RIGHT_PANEL, j);
		}
		for (int k = 0; k < instance.GetLedNum(Bi2xInput.TAPELED.TAPELED_CTRL_PANEL); k++)
		{
			if (k < instance.GetLedNum(Bi2xInput.TAPELED.TAPELED_CTRL_PANEL) / 2)
			{
				image_UP[k].color = instance.GetLedColor(Bi2xInput.TAPELED.TAPELED_CTRL_PANEL, k);
			}
			else
			{
				image_DOWN[k - instance.GetLedNum(Bi2xInput.TAPELED.TAPELED_CTRL_PANEL) / 2].color = instance.GetLedColor(Bi2xInput.TAPELED.TAPELED_CTRL_PANEL, k);
			}
		}
	}
}
