using System.Collections.Generic;
using MFG.Types;
using UnityEngine;
using UnityEngine.UI;

public class MachihaiPanel : MonoBehaviour
{
	[SerializeField]
	public GameObject m_machihai;

	[SerializeField]
	public GameObject m_machipaiText;

	[SerializeField]
	public GameObject m_ishantenText;

	[SerializeField]
	public GameObject m_ishantenOverText;

	[SerializeField]
	public Sprite m_ishantenFrameSprite;

	[SerializeField]
	public Sprite m_tenpaiFrameSprite;

	[SerializeField]
	public float m_panelHeight = 100f;

	private const int Spacing = 10;

	private const int MachihaiSize = 62;

	private const int Margin = 60;

	private const int IshantenHaiMaxDispNum = 18;

	private const float MachiHaiScale = 0.8f;

	private Pai m_lastPai;

	private bool m_isReach;

	private bool m_isDispIshantenMachihai;

	private bool m_isJichaSutehaiFuriten;

	private List<GameObject> m_machihais = new List<GameObject>();

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void Reset()
	{
		m_lastPai = Pai.NONE;
		m_isReach = false;
	}

	public void SetMachiPaiRyuKyoku(int[] pais)
	{
		base.gameObject.SetActive(value: true);
		m_isDispIshantenMachihai = false;
		m_isJichaSutehaiFuriten = false;
		GameObject gameObject = base.transform.Find("Panel").gameObject;
		gameObject.GetComponent<Image>().sprite = m_tenpaiFrameSprite;
		m_machihais.Clear();
		foreach (Transform item in gameObject.transform)
		{
			Object.Destroy(item.gameObject);
		}
		float num = 0f;
		for (int i = 0; i < pais.Length; i++)
		{
			if (pais[i] != 0)
			{
				GameObject gameObject2 = Object.Instantiate(m_machihai, gameObject.transform);
				m_machihais.Add(gameObject2);
				gameObject2.GetComponent<Machihai>().SetPai((Pai)pais[i]);
				num += 57.600002f;
			}
		}
		GameObject gameObject3 = Object.Instantiate(m_machipaiText, gameObject.transform);
		num += gameObject3.GetComponent<RectTransform>().sizeDelta.x + 10f;
		gameObject3.transform.Find("FuritenWarning").gameObject.SetActive(value: false);
		num += 60f;
		gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(num, m_panelHeight);
		LayoutRebuilder.ForceRebuildLayoutImmediate(gameObject.GetComponent<RectTransform>());
	}

	public void SetIshantenMachiPai(int[] pais, Pai lastPai)
	{
		base.gameObject.SetActive(value: true);
		m_isDispIshantenMachihai = true;
		if (m_lastPai == lastPai)
		{
			return;
		}
		m_lastPai = lastPai;
		GameObject gameObject = base.transform.Find("Panel").gameObject;
		gameObject.GetComponent<Image>().sprite = m_ishantenFrameSprite;
		m_machihais.Clear();
		foreach (Transform item in gameObject.transform)
		{
			Object.Destroy(item.gameObject);
		}
		float num = 0f;
		for (int i = 0; i < pais.Length && i < 18; i++)
		{
			if (pais[i] != 0)
			{
				GameObject gameObject2 = Object.Instantiate(m_machihai, gameObject.transform);
				m_machihais.Add(gameObject2);
				gameObject2.GetComponent<Machihai>().SetPai((Pai)pais[i]);
				if (MahjongUtility.GetRestPaiNum((Pai)pais[i]) == 0)
				{
					gameObject2.transform.Find("Machinashi").gameObject.SetActive(value: true);
				}
				num += 57.600002f;
			}
		}
		if (pais.Length > 18)
		{
			Object.Instantiate(m_machihai, gameObject.transform);
			num += 57.600002f;
			GameObject gameObject3 = Object.Instantiate(m_ishantenOverText, gameObject.transform);
			num += gameObject3.GetComponent<RectTransform>().sizeDelta.x + 10f;
			gameObject3.GetComponent<Text>().text = "他" + (pais.Length - 18) + "種の牌でテンパイ!!";
		}
		else
		{
			GameObject gameObject4 = Object.Instantiate(m_ishantenText, gameObject.transform);
			num += gameObject4.GetComponent<RectTransform>().sizeDelta.x + 10f;
		}
		num += 60f;
		gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(num, m_panelHeight);
		LayoutRebuilder.ForceRebuildLayoutImmediate(gameObject.GetComponent<RectTransform>());
	}

	public void SetMachiPai(int[] pais, int[] status, bool isReach, Pai lastPai, List<Pai> furitenPais)
	{
		base.gameObject.SetActive(value: true);
		m_isDispIshantenMachihai = false;
		if (m_lastPai == lastPai && m_isReach == isReach)
		{
			return;
		}
		m_lastPai = lastPai;
		m_isReach = isReach;
		GameObject gameObject = base.transform.Find("Panel").gameObject;
		gameObject.GetComponent<Image>().sprite = m_tenpaiFrameSprite;
		m_machihais.Clear();
		foreach (Transform item in gameObject.transform)
		{
			Object.Destroy(item.gameObject);
		}
		bool active = false;
		m_isJichaSutehaiFuriten = false;
		float num = 0f;
		for (int i = 0; i < pais.Length; i++)
		{
			if (pais[i] != 0 && status != null)
			{
				GameObject gameObject2 = Object.Instantiate(m_machihai, gameObject.transform);
				m_machihais.Add(gameObject2);
				gameObject2.GetComponent<Machihai>().SetPai((Pai)pais[i]);
				if (((uint)status[i] & (true ? 1u : 0u)) != 0)
				{
					active = true;
					m_isJichaSutehaiFuriten = true;
				}
				if (furitenPais.Contains((Pai)pais[i]))
				{
					active = true;
				}
				if ((status[i] & 4) == 0 && !isReach)
				{
					gameObject2.transform.Find("Yakunashi").gameObject.SetActive(value: true);
				}
				int restPaiNum = MahjongUtility.GetRestPaiNum((Pai)pais[i]);
				if (restPaiNum == 0)
				{
					gameObject2.transform.Find("Machinashi").gameObject.SetActive(value: true);
				}
				else
				{
					gameObject2.transform.Find("ZanpaiFrame").gameObject.SetActive(value: true);
					gameObject2.transform.Find("ZanpaiFrame").Find("Num").GetComponent<Text>()
						.text = restPaiNum.ToString();
				}
				num += 57.600002f;
			}
		}
		GameObject gameObject3 = Object.Instantiate(m_machipaiText, gameObject.transform);
		num += gameObject3.GetComponent<RectTransform>().sizeDelta.x + 10f;
		gameObject3.transform.Find("FuritenWarning").gameObject.SetActive(active);
		num += 60f;
		gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(num, m_panelHeight);
		LayoutRebuilder.ForceRebuildLayoutImmediate(gameObject.GetComponent<RectTransform>());
	}

	public void OpenMachiDisp(bool isDisp, Pai[] pais)
	{
		if (!isDisp)
		{
			Destruct();
			return;
		}
		base.gameObject.SetActive(value: true);
		m_isDispIshantenMachihai = false;
		m_isJichaSutehaiFuriten = false;
		GameObject gameObject = base.transform.Find("Panel").gameObject;
		gameObject.GetComponent<Image>().sprite = m_tenpaiFrameSprite;
		m_machihais.Clear();
		foreach (Transform item in gameObject.transform)
		{
			Object.Destroy(item.gameObject);
		}
		float num = 0f;
		for (int i = 0; i < pais.Length; i++)
		{
			if (pais[i] != 0)
			{
				GameObject gameObject2 = Object.Instantiate(m_machihai, gameObject.transform);
				m_machihais.Add(gameObject2);
				gameObject2.GetComponent<Machihai>().SetPai(pais[i]);
				num += 57.600002f;
			}
		}
		GameObject gameObject3 = Object.Instantiate(m_machipaiText, gameObject.transform);
		num += gameObject3.GetComponent<RectTransform>().sizeDelta.x + 10f;
		gameObject3.transform.Find("FuritenWarning").gameObject.SetActive(value: false);
		num += 60f;
		gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(num, m_panelHeight);
		LayoutRebuilder.ForceRebuildLayoutImmediate(gameObject.GetComponent<RectTransform>());
	}

	public void DebugMachiDisp(bool isDisp, Pai[] pais, int shantenCount, bool isOpenReachPlayer)
	{
		if (isOpenReachPlayer)
		{
			return;
		}
		if (!isDisp)
		{
			Destruct();
			return;
		}
		base.gameObject.SetActive(value: true);
		m_isDispIshantenMachihai = false;
		m_isJichaSutehaiFuriten = false;
		GameObject gameObject = base.transform.Find("Panel").gameObject;
		gameObject.GetComponent<Image>().sprite = m_tenpaiFrameSprite;
		m_machihais.Clear();
		foreach (Transform item in gameObject.transform)
		{
			Object.Destroy(item.gameObject);
		}
		float num = 0f;
		if (shantenCount <= 0)
		{
			for (int i = 0; i < pais.Length; i++)
			{
				if (pais[i] != 0)
				{
					GameObject gameObject2 = Object.Instantiate(m_machihai, gameObject.transform);
					m_machihais.Add(gameObject2);
					gameObject2.GetComponent<Machihai>().SetPai(pais[i]);
					num += 57.600002f;
				}
			}
		}
		if (shantenCount <= 0)
		{
			GameObject gameObject3 = Object.Instantiate(m_machipaiText, gameObject.transform);
			num += gameObject3.GetComponent<RectTransform>().sizeDelta.x + 10f;
			gameObject3.transform.Find("FuritenWarning").gameObject.SetActive(value: false);
		}
		else
		{
			GameObject gameObject4 = Object.Instantiate(m_ishantenText, gameObject.transform);
			num += gameObject4.GetComponent<RectTransform>().sizeDelta.x + 10f;
			gameObject4.GetComponent<Text>().text = $"{shantenCount} 向聴";
		}
		num += 60f;
		gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(num, m_panelHeight);
		LayoutRebuilder.ForceRebuildLayoutImmediate(gameObject.GetComponent<RectTransform>());
	}

	public void Destruct()
	{
		m_machihais.Clear();
		foreach (Transform item in base.transform.Find("Panel"))
		{
			Object.Destroy(item.gameObject);
		}
		base.gameObject.SetActive(value: false);
		m_lastPai = Pai.NONE;
		m_isReach = false;
		m_isDispIshantenMachihai = false;
		m_isJichaSutehaiFuriten = false;
	}

	public void DispOffIfIshantenDisp()
	{
		if (m_isDispIshantenMachihai)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public void RefleshMachihaiNum()
	{
		foreach (GameObject machihai in m_machihais)
		{
			int restPaiNum = MahjongUtility.GetRestPaiNum(machihai.GetComponent<Machihai>().m_pai);
			if (restPaiNum == 0)
			{
				machihai.transform.Find("ZanpaiFrame").gameObject.SetActive(value: false);
				machihai.transform.Find("Machinashi").gameObject.SetActive(value: true);
			}
			else if (!m_isDispIshantenMachihai)
			{
				machihai.transform.Find("ZanpaiFrame").gameObject.SetActive(value: true);
				machihai.transform.Find("ZanpaiFrame").Find("Num").GetComponent<Text>()
					.text = restPaiNum.ToString();
			}
		}
	}

	public void RefleshFuriten(List<Pai> furitenPais, bool isDispFuriten)
	{
		bool active = m_isJichaSutehaiFuriten;
		Transform transform = base.transform.Find("Panel").Find("MachihaiText(Clone)");
		if (!(transform != null))
		{
			return;
		}
		foreach (GameObject machihai in m_machihais)
		{
			if (furitenPais.Contains(machihai.GetComponent<Machihai>().m_pai))
			{
				active = true;
			}
		}
		if (!isDispFuriten)
		{
			active = false;
		}
		transform.Find("FuritenWarning").gameObject.SetActive(active);
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform.Find("Panel").GetComponent<RectTransform>());
	}
}
