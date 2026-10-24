using System;
using System.Diagnostics;
using System.Text;
using TMPro;
using UnityEngine;

public class FPSInfo : MonoBehaviour
{
	private int m_frame;

	private float m_passTime;

	private static float m_fps;

	private StringBuilder m_stringBuilder = new StringBuilder();

	private GameObject m_info;

	private TextMeshProUGUI m_text;

	private static bool m_bExec;

	private bool m_bMain;

	private static string m_logMouse;

	private static bool m_bLogGPUClear;

	private static string m_logGPU;

	private static Process m_process;

	private static void OnStandardOut(object sender, DataReceivedEventArgs e)
	{
		if (e.Data != null)
		{
			if (m_bLogGPUClear)
			{
				m_logGPU = "";
				m_bLogGPUClear = false;
			}
			m_logGPU = m_logGPU + e.Data + "\n";
		}
	}

	private void DisposeProcess(object sender, EventArgs e)
	{
		if (m_process != null && !m_process.HasExited)
		{
			m_process.StandardInput.Close();
			m_process.CloseMainWindow();
			m_process.Dispose();
			m_process = null;
		}
	}

	private string GetString()
	{
		m_stringBuilder.Clear();
		m_stringBuilder.Append("FPS: ").Append(m_fps).AppendLine();
		m_stringBuilder.Append(m_logMouse).AppendLine();
		m_stringBuilder.Append(m_logGPU).AppendLine();
		return m_stringBuilder.ToString();
	}

	private void Exec()
	{
	}

	private void Awake()
	{
		if (!m_bExec)
		{
			m_bMain = true;
			m_bExec = true;
		}
	}

	private void Start()
	{
		m_info = base.transform.Find("FPSInfo").gameObject;
		m_text = base.transform.Find("FPSInfo/Text").GetComponent<TextMeshProUGUI>();
	}

	private void Update()
	{
	}
}
