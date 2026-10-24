using System;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class InfoWindow : MonoBehaviour, IDisposable
{
	[SerializeField]
	private GameObject m_itemParent;

	[SerializeField]
	private GameObject m_textParent;

	[SerializeField]
	private GameObject m_itemPrefab;

	[SerializeField]
	private TextMeshProUGUI m_text;

	[SerializeField]
	private Button m_closeButton;

	[SerializeField]
	private Animator m_animator;

	[SerializeField]
	private GameObject m_contents;

	[SerializeField]
	private ItemPopupManager _itemPopupManager;

	private IDisposableAsset<Sprite> m_iconSprite;

	public bool m_isOpen;

	private int m_addNum;

	public const int GetItemDispMax = 10;

	public void ResetItem()
	{
		foreach (Transform item in m_itemParent.transform)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		m_text.text = "";
		m_addNum = 0;
	}

	public async UniTask AddItem(string text, Sprite sprite, string name, string oid)
	{
		if (m_addNum >= 10)
		{
			Debug.LogError("InfoWindow に10個を超えるアイテムは追加できません");
			return;
		}
		GameObject obj = UnityEngine.Object.Instantiate(m_itemPrefab, m_itemParent.transform);
		obj.transform.Find("Item").GetComponent<Image>().sprite = sprite;
		obj.transform.Find("Item").GetComponent<Button>().OnClickAsObservable()
			.Subscribe(async delegate
			{
				RectTransform component = obj.transform.Find("Item").GetComponent<RectTransform>();
				await _itemPopupManager.Open(oid, component.position, component.sizeDelta);
			})
			.AddTo(this);
		TextMeshProUGUI text2 = m_text;
		text2.text = text2.text + text + "\n";
		m_addNum++;
	}

	public async UniTask Open()
	{
		m_isOpen = true;
		base.gameObject.SetActive(value: true);
		base.transform.Find("BlackScreen").gameObject.SetActive(value: true);
		m_contents.GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical();
		m_contents.GetComponent<VerticalLayoutGroup>().SetLayoutVertical();
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_contents.GetComponent<RectTransform>());
		m_animator.SetTrigger("In");
		SoundManager.PlaySe("SFX_SYS_BONUS5").Forget();
		SoundManager.PlayMyCharaVoice("VOX_GXX_ITEM2").Forget();
		await UniTask.WhenAny(UniTask.WaitUntil(() => !m_isOpen), UniTask.Delay(TimeSpan.FromSeconds(7.0)));
	}

	public async UniTask Close()
	{
		m_animator.SetTrigger("Out");
		_itemPopupManager.Close().Forget();
		base.transform.Find("BlackScreen").gameObject.SetActive(value: false);
		base.gameObject.SetActive(value: false);
		m_isOpen = false;
	}

	public void Dispose()
	{
		if (m_iconSprite != null)
		{
			m_iconSprite.Dispose();
			m_iconSprite = null;
		}
	}

	private void OnDestroy()
	{
		Dispose();
	}

	private void Start()
	{
		m_closeButton.OnClickAsObservable().Subscribe(async delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			Close().Forget();
		}).AddTo(this);
	}
}
