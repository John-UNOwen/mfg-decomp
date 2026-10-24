using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TestModeSkeleton;

public class ScreenCheck : TestModeBase
{
	protected override string TitleName => "<l>SCREEN CHECK</l>";

	private IEnumerator Start()
	{
		help.Set(TestModeHelp.HelpType.Exit);
		ArrowkeyGameObject.SetActive(value: true);
		ArrowkeyEnter.SetActive(value: true);
		ArrowkeyUp.SetActive(value: false);
		ArrowkeyDown.SetActive(value: false);
		ArrowkeyLeft.SetActive(value: false);
		ArrowkeyRight.SetActive(value: false);
		List<Image> images = new List<Image>
		{
			GameObject.Find("TestModeMain/Canvas/Title/Main")?.GetComponent<Image>(),
			GameObject.Find("TestModeMain/Canvas/Help/Help0")?.GetComponent<Image>(),
			GameObject.Find("TestModeMain/Canvas/Help/Help1")?.GetComponent<Image>()
		};
		GameObject backGround = GameObject.Find("TestModeMain/Canvas/BackGround");
		Transform transform = base.transform.Find("CanvasSub");
		if (transform != null)
		{
			transform.gameObject.SetActive(SystemConfigGameData.IsDualMonitor);
		}
		UnityAction<bool> Set = delegate(bool isStart)
		{
			images.ForEach(delegate(Image img)
			{
				if ((bool)img)
				{
					img.enabled = isStart;
				}
			});
			backGround?.SetActive(!isStart);
		};
		Set(arg0: true);
		yield return new WaitUntil(() => SingletonBehaviour<TestModeInput>.Instance.IsTrigger(TestModeInput.Key.Enter) || SingletonBehaviour<TestModeInput>.Instance.IsTrigger(TestModeInput.Key.Service));
		Set(arg0: false);
		Exit();
	}
}
