using System.Linq;
using TestModeSkeleton.Price;
using UnityEngine;
using UnityEngine.UI;

namespace TestModeSkeleton;

public class VirtualCoinContentsPrice : MonoBehaviour
{
	[SerializeField]
	[Tooltip("通常価格テキスト(GameContentsInfo.GameContentsと並びをそろえること)")]
	private Text[] basicTexts;

	[SerializeField]
	[Tooltip("サービスタイム価格テキスト(GameContentsInfo.GameContentsと並びをそろえること)")]
	private Text[] serviceTimeTexts;

	[SerializeField]
	[Tooltip("テキストの色を白固定にするか？")]
	private bool isWhiteFixedTextColor;

	public void SetBasicPrice(GameContentsInfo.GameContents contents, int price, bool isLimitOrReferCredit)
	{
		Text text = basicTexts.ElementAtOrDefault((int)contents);
		SetPrice(text, price, isLimitOrReferCredit, isEnableServiceTime: true);
	}

	public void SetTimeServicePrice(GameContentsInfo.GameContents contents, int price, bool isLimitOrReferCredit, bool isEnableServiceTime)
	{
		Text text = serviceTimeTexts.ElementAtOrDefault((int)contents);
		SetPrice(text, price, isLimitOrReferCredit, isEnableServiceTime);
	}

	private void SetPrice(Text text, int price, bool isReferenceToCredit, bool isEnableServiceTime)
	{
		if (text != null)
		{
			if (!isEnableServiceTime)
			{
				text.text = "OFF";
				text.color = Color.gray;
			}
			else if (price == 0)
			{
				text.text = "FREE";
				text.color = Color.red;
			}
			else
			{
				text.text = price.ToString();
				text.color = (isReferenceToCredit ? Color.yellow : Color.white);
			}
			if (isWhiteFixedTextColor)
			{
				text.color = Color.white;
			}
		}
	}

	public int GetBasicPrice(GameContentsInfo.GameContents contents)
	{
		Text text = basicTexts.ElementAtOrDefault((int)contents);
		if (text != null && int.TryParse(text.text, out var result))
		{
			return result;
		}
		return 0;
	}
}
