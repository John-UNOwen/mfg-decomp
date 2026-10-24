using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TestModeSkeleton;

public class ICCardCheck : TestModeBase
{
	[SerializeField]
	[Tooltip("状態テキスト")]
	private Text statusText;

	protected override string TitleName => "<l>IC CARD CHECK</l>";

	private IEnumerator Start()
	{
		Bi2xInput.Instance.ICReaderOn();
		help.Set(TestModeHelp.HelpType.Exit);
		ArrowkeyGameObject.SetActive(value: true);
		ArrowkeyEnter.SetActive(value: true);
		ArrowkeyUp.SetActive(value: false);
		ArrowkeyDown.SetActive(value: false);
		ArrowkeyLeft.SetActive(value: false);
		ArrowkeyRight.SetActive(value: false);
		SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
		yield return CoCheck();
		Exit();
	}

	private IEnumerator CoCheck()
	{
		_ = Time.time;
		statusText.text = "---";
		while (true)
		{
			if (Bi2xInput.IsBi2xReady)
			{
				bool flag = Bi2xInput.Instance.IsCardOn();
				int cardType = (int)Bi2xInput.Instance.GetCardType();
				statusText.text = (flag ? $"<color=lime>OK[{cardType}]</color>" : "---");
			}
			if (SingletonBehaviour<TestModeInput>.Instance.IsMenuExecute())
			{
				break;
			}
			yield return null;
		}
		Bi2xInput.Instance.ICReaderOff();
	}
}
