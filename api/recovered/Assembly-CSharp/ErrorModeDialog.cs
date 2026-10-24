using TMPro;
using UnityEngine;

public class ErrorModeDialog : MonoBehaviour
{
	[SerializeField]
	private TMP_Text m_Title;

	[SerializeField]
	private TMP_Text m_ErrorCode;

	[SerializeField]
	private TMP_Text m_Message;

	public void SetData(string title, string errorCode, string message)
	{
		m_Title.text = title;
		m_ErrorCode.text = errorCode;
		m_Message.text = message;
	}

	public void SetData(ErrorData.Item item, string extraString = "")
	{
		if (item != null)
		{
			string errorCode = $"5-{(int)item.errCode:0000}-{item.subCode:0000}";
			string text = item.message;
			if (!string.IsNullOrEmpty(extraString))
			{
				text = ((!string.IsNullOrEmpty(text)) ? (text + "\n" + extraString) : extraString);
			}
			text = text.Replace("\\n", "\n");
			SetData(item.title, errorCode, text);
		}
	}
}
