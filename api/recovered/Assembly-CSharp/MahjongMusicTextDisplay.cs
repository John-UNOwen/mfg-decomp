using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.CustomizeItem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MahjongMusicTextDisplay : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _text;

	private Color textColor;

	private Color imageColor;

	private bool isPlaying;

	private void Start()
	{
		textColor = _text.color;
		imageColor = GetComponent<Image>().color;
	}

	public string MusicNameResolover(string musicName)
	{
		return SaniTize.SanitizedString(musicName.Replace(" / オフボーカル ", "").Replace(" / オンボーカル ", "").Replace("（ショート）", "")
			.Replace("（フル）", ""));
	}

	public void MusicIn(ItemID itemID)
	{
		Item item = CustomizeItemMaster.GetItem(itemID);
		if (item.Category == Category.ReachBgm)
		{
			MusicAnim("♪ " + MusicNameResolover(item.DispName)).Forget();
		}
	}

	public void MusicIn(string label)
	{
		MusicAnim("♪ " + MusicNameResolover(label)).Forget();
	}

	public async UniTask MusicAnim(string name)
	{
		await UniTask.WaitWhile(() => isPlaying);
		isPlaying = true;
		Color color = textColor;
		color.a = 0f;
		_text.color = color;
		_text.text = name;
		Color color2 = imageColor;
		color2.a = 0f;
		GetComponent<Image>().color = color2;
		_text.transform.GetComponent<ContentSizeFitter>().SetLayoutHorizontal();
		float x = _text.transform.GetComponent<RectTransform>().sizeDelta.x;
		float num = x + 20f;
		_text.transform.GetComponent<RectTransform>().localPosition = new Vector3(num, 0f);
		base.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(x, 50f);
		color.a = 1f;
		_text.color = color;
		color2.a = 1f;
		GetComponent<Image>().color = color2;
		bool isPlayInEnd = false;
		DOVirtual.Float(num, 0f, 0.4f, delegate(float value)
		{
			_text.transform.GetComponent<RectTransform>().localPosition = new Vector3(value, 0f);
		}).OnComplete(delegate
		{
			isPlayInEnd = true;
		});
		await UniTask.WaitUntil(() => isPlayInEnd);
		await UniTask.Delay(TimeSpan.FromSeconds(5.0));
		bool isPlayOutEnd = false;
		DOVirtual.Float(0f, -50f, 0.4f, delegate(float value)
		{
			_text.transform.GetComponent<RectTransform>().localPosition = new Vector3(0f, value);
		}).OnComplete(delegate
		{
			isPlayOutEnd = true;
		});
		await UniTask.WaitUntil(() => isPlayOutEnd);
		color.a = 0f;
		_text.color = color;
		color2.a = 0f;
		try
		{
			GetComponent<Image>().color = color2;
		}
		catch
		{
		}
		isPlaying = false;
	}
}
