using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TestModeSkeleton;

public class ColorCheck : TestModeBase
{
	[SerializeField]
	[Tooltip("カラーフィールド")]
	private Transform[] colorFields;

	[SerializeField]
	private GameObject colorPanel;

	protected override string TitleName => "<l>COLOR CHECK</l>";

	private IEnumerator Start()
	{
		colorPanel.transform.GetComponent<RectTransform>().localScale = (SystemConfigGameData.IsSXGAMonitor ? new Vector3(0.7f, 0.7f, 0.7f) : Vector3.one);
		help.Set(TestModeHelp.HelpType.Exit);
		ArrowkeyGameObject.SetActive(value: true);
		ArrowkeyEnter.SetActive(value: true);
		ArrowkeyUp.SetActive(value: false);
		ArrowkeyDown.SetActive(value: false);
		ArrowkeyLeft.SetActive(value: false);
		ArrowkeyRight.SetActive(value: false);
		SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
		Transform transform = base.transform.Find("CanvasSub");
		if (transform != null)
		{
			transform.gameObject.SetActive(SystemConfigGameData.IsDualMonitor);
		}
		Transform[] array = colorFields;
		for (int i = 0; i < array.Length; i++)
		{
			foreach (Transform item in array[i])
			{
				int num = 0;
				foreach (Transform item2 in item)
				{
					Image component = item2.GetComponent<Image>();
					if (!(component == null))
					{
						Color color = component.color * num / item.childCount;
						color.a = 1f;
						component.color = color;
						num++;
					}
				}
			}
		}
		yield return new WaitUntil(() => SingletonBehaviour<TestModeInput>.Instance.IsMenuExecute());
		Exit();
	}
}
