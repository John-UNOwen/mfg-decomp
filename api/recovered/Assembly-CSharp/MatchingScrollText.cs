using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchingScrollText : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI baseText;

	private List<TextMeshProUGUI> texts = new List<TextMeshProUGUI>();

	private float textSize;

	private float textIntervalWidth = 1920f;

	private float scrollSpeed = 250f;

	private bool isPlay;

	public void Init()
	{
		isPlay = GameDataManager.IsReachSongToytoyNoticeEnable() && (PaseliUtility.PPEX.IsEnablePPEX || GameUtility.IsKonastaOrNetcafeBehavior);
		string text = "おいコチ、リーチソング＆ボイスガチャ「トイトイ」で新しいリーチソング「こイかイめイかイ」とボイスを入手し、服従者たる自己を再認識するがいい。オリジナルCDをGETできるキャンペーンも開催中。詳しくは公式サイトをチェックだ。";
		base.gameObject.SetActive(isPlay);
		if (isPlay)
		{
			baseText.text = text;
			baseText.transform.GetComponent<ContentSizeFitter>().SetLayoutHorizontal();
			textSize = baseText.transform.GetComponent<RectTransform>().rect.width;
			SetPosition(baseText, 1920f);
			texts.Add(baseText);
			for (int i = 0; (float)i < (1920f + textIntervalWidth) / (textSize + textIntervalWidth); i++)
			{
				TextMeshProUGUI textMeshProUGUI = Object.Instantiate(baseText, base.transform);
				textMeshProUGUI.text = text;
				textMeshProUGUI.transform.GetComponent<ContentSizeFitter>().SetLayoutHorizontal();
				SetPosition(textMeshProUGUI, (textSize + textIntervalWidth) * (float)(i + 1) + 1920f);
				texts.Add(textMeshProUGUI);
			}
		}
	}

	public async UniTask Play()
	{
		try
		{
			while (isPlay)
			{
				float time = Time.time;
				await UniTask.NextFrame();
				float pos = scrollSpeed * (Time.time - time);
				foreach (TextMeshProUGUI text in texts)
				{
					SetPosition(text, pos, isDiff: true);
				}
				ResetPosition();
			}
		}
		catch
		{
		}
	}

	public void SetPosition(TextMeshProUGUI text, float pos, bool isDiff = false)
	{
		if (isDiff)
		{
			pos = text.transform.GetComponent<RectTransform>().anchoredPosition.x - pos;
		}
		text.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(pos, 0f);
	}

	public void ResetPosition()
	{
		TextMeshProUGUI textMeshProUGUI = texts.OrderBy((TextMeshProUGUI x) => x.transform.GetComponent<RectTransform>().anchoredPosition.x).First();
		if (textMeshProUGUI.transform.GetComponent<RectTransform>().anchoredPosition.x <= -1f * textSize)
		{
			Debug.Log($"text.transform.GetComponent<RectTransform>().anchoredPosition .x : {textMeshProUGUI.transform.GetComponent<RectTransform>().anchoredPosition.x}");
			float num = texts.Max((TextMeshProUGUI x) => x.transform.GetComponent<RectTransform>().anchoredPosition.x);
			textMeshProUGUI.transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(num + textSize + textIntervalWidth, 0f);
		}
	}

	private void OnDestroy()
	{
		isPlay = false;
	}
}
