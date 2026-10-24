using UnityEngine;

public class SdChara : MonoBehaviour
{
	[SerializeField]
	private GameObject m_stand;

	[SerializeField]
	private GameObject m_tsumo;

	[SerializeField]
	private GameObject m_pickup;

	[SerializeField]
	private GameObject m_dahai;

	[SerializeField]
	private Camera m_camera;

	private void Start()
	{
		m_tsumo.SetActive(value: false);
		m_pickup.SetActive(value: false);
		m_dahai.SetActive(value: false);
	}

	public void SetCamera(Camera camera)
	{
		m_camera = camera;
	}

	private void Update()
	{
	}

	public void TsumoAnime(Tile item)
	{
		DispOffAnime();
		if (!(item == null))
		{
			SetPosition(item);
			m_tsumo.SetActive(value: true);
		}
	}

	public void PickUpAnime(Tile item)
	{
		if (!(item != null) || !item.IsDiscardStart)
		{
			DispOffAnime();
			if (!(item == null))
			{
				SetPosition(item);
				m_pickup.SetActive(value: true);
			}
		}
	}

	public void DahaiAnime(Tile item)
	{
		DispOffAnime();
		if (!(item == null))
		{
			SetPosition(item);
			m_dahai.SetActive(value: true);
		}
	}

	public void StandAnime()
	{
		DispOffAnime();
		m_stand.SetActive(value: true);
	}

	public void DispOffAnime()
	{
		foreach (Transform item in base.transform)
		{
			item.gameObject.SetActive(value: false);
		}
	}

	private void SetPosition(Tile item)
	{
		Vector3 vector = m_camera.WorldToViewportPoint(item.transform.position);
		vector.z = 0f;
		GetComponent<RectTransform>().anchorMax = vector;
		GetComponent<RectTransform>().anchorMin = vector;
	}
}
