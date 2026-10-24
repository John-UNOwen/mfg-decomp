using UnityEngine;

namespace UniSimpleProfiler.Internal;

public class FPSCounter
{
	private const float INTERVAL = 0.5f;

	private float m_accum;

	private int m_frames;

	private float m_timeleft;

	private float m_fps;

	public float Fps => m_fps;

	public void Update()
	{
		m_timeleft -= Time.deltaTime;
		m_accum += Time.timeScale / Time.deltaTime;
		m_frames++;
		if (!(0f < m_timeleft))
		{
			m_fps = m_accum / (float)m_frames;
			m_timeleft = 0.5f;
			m_accum = 0f;
			m_frames = 0;
		}
	}
}
