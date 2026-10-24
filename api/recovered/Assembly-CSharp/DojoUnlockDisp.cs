using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DojoUnlockDisp : MonoBehaviour
{
	[SerializeField]
	private Button unlockDojo1;

	[SerializeField]
	private Button unlockDojo2;

	[SerializeField]
	private Button unlockDojo3;

	[SerializeField]
	private Button unlockDojo4;

	private void Start()
	{
		unlockDojo1.onClick.AddListener(delegate
		{
			ChangeUnlockDojo(1);
		});
		unlockDojo2.onClick.AddListener(delegate
		{
			ChangeUnlockDojo(2);
		});
		unlockDojo3.onClick.AddListener(delegate
		{
			ChangeUnlockDojo(3);
		});
		unlockDojo4.onClick.AddListener(delegate
		{
			ChangeUnlockDojo(4);
		});
	}

	private void Update()
	{
		unlockDojo1.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = "道場1：" + (GameDataManager.Player.m_dojoSlotUnlocked[0] ? "オン" : "オフ");
		unlockDojo2.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = "道場2：" + (GameDataManager.Player.m_dojoSlotUnlocked[1] ? "オン" : "オフ");
		unlockDojo3.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = "道場3：" + (GameDataManager.Player.m_dojoSlotUnlocked[2] ? "オン" : "オフ");
		unlockDojo4.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = "道場4：" + (GameDataManager.Player.m_dojoSlotUnlocked[3] ? "オン" : "オフ");
	}

	private void ChangeUnlockDojo(int num)
	{
		if (GameDataManager.Player.m_dojoSlotUnlocked[num - 1])
		{
			GameDataManager.Player.m_dojoSlotUnlocked[num - 1] = false;
		}
		else
		{
			GameDataManager.Player.m_dojoSlotUnlocked[num - 1] = true;
		}
	}
}
