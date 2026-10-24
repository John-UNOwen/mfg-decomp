using UnityEngine;

public class StandingCharaFPSAdjuster : MonoBehaviour
{
	[SerializeField]
	public Camera m_camera;

	public float m_fps = -1f;

	private float m_skippedTime;

	private float m_thresholdTime => 1f / m_fps;

	public void CameraFpsSetting(float fps)
	{
		m_fps = fps;
		if (m_fps > 0f)
		{
			m_skippedTime = Random.Range(0f, m_thresholdTime);
			return;
		}
		m_fps = -1f;
		m_camera.enabled = true;
	}

	private void Update()
	{
		if (m_fps > 0f)
		{
			m_camera.enabled = false;
			m_skippedTime += Time.deltaTime;
			if (!(m_thresholdTime > m_skippedTime))
			{
				m_skippedTime = 0f;
				m_camera.enabled = true;
			}
		}
	}
}
