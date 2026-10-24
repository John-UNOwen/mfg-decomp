using MFG.Types;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NakiPattern : MonoBehaviour
{
	[SerializeField]
	private GameObject nakiPanel;

	[SerializeField]
	private GameObject m_panel;

	[SerializeField]
	private MachihaiPanel m_machihaiPanel;

	private int m_patternNum;

	private void Start()
	{
	}

	private void Update()
	{
		try
		{
			foreach (Transform item in m_panel.transform)
			{
				item.transform.Find("Key_No").gameObject.SetActive(GameDataManager.PlayerConfig.IsKeyboardGuideDisplay());
			}
		}
		catch
		{
		}
	}

	public void AddNakiPattern(Pai pai1, Pai pai2 = Pai.NONE, Pai pai3 = Pai.NONE)
	{
		GameObject gameObject = Object.Instantiate(nakiPanel, m_panel.transform);
		gameObject.transform.Find("Images/Image_Pai1").GetComponent<Image>().sprite = TileManager.GetInstance().GetTileSprite(pai1);
		if (pai2 != 0)
		{
			gameObject.transform.Find("Images/Image_Pai2").GetComponent<Image>().sprite = TileManager.GetInstance().GetTileSprite(pai2);
		}
		else
		{
			gameObject.transform.Find("Images/Image_Pai2").gameObject.SetActive(value: false);
		}
		if (pai3 != 0)
		{
			gameObject.transform.Find("Images/Image_Pai3").GetComponent<Image>().sprite = TileManager.GetInstance().GetTileSprite(pai3);
		}
		else
		{
			gameObject.transform.Find("Images/Image_Pai3").gameObject.SetActive(value: false);
		}
		gameObject.transform.Find("Text_No").GetComponent<TextMeshProUGUI>().text = m_patternNum.ToString();
		gameObject.transform.Find("Key_No").GetComponent<TextMeshProUGUI>().text = MahjongUtility.KeyBoard.KeyCodeText[m_patternNum];
		m_patternNum++;
	}

	private void Reset()
	{
		foreach (Transform item in m_panel.transform)
		{
			Object.Destroy(item.gameObject);
		}
		m_patternNum = 0;
	}

	public void Play()
	{
		base.gameObject.SetActive(value: true);
		Reset();
		if (m_machihaiPanel.gameObject.activeSelf)
		{
			m_machihaiPanel.gameObject.SetActive(value: false);
		}
	}

	public void Exit()
	{
		base.gameObject.SetActive(value: false);
	}

	public int Select(GameObject panel)
	{
		foreach (Transform item in m_panel.transform)
		{
			if (item.gameObject == panel)
			{
				return int.Parse(item.transform.Find("Text_No").GetComponent<TextMeshProUGUI>().text);
			}
		}
		return -1;
	}
}
