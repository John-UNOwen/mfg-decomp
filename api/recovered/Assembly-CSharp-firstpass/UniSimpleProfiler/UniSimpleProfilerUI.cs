using System;
using TMPro;
using UniSimpleProfiler.Internal;
using UnityEngine;
using UnityEngine.Profiling;

namespace UniSimpleProfiler;

[DisallowMultipleComponent]
public sealed class UniSimpleProfilerUI : MonoBehaviour
{
	private const float INTERVAL = 0.5f;

	[SerializeField]
	private TextMeshProUGUI m_fpsTextUI;

	[SerializeField]
	private TextMeshProUGUI m_gcTextUI;

	[SerializeField]
	private TextMeshProUGUI m_monoUsedTextUI;

	[SerializeField]
	private TextMeshProUGUI m_monoTotalTextUI;

	[SerializeField]
	private TextMeshProUGUI m_unityUsedTextUI;

	[SerializeField]
	private TextMeshProUGUI m_unityTotalTextUI;

	private readonly FPSCounter m_fPSCounter = new FPSCounter();

	private float m_timer;

	private void Update()
	{
		m_fPSCounter.Update();
		m_timer += Time.unscaledDeltaTime;
		if (!(m_timer < 0.5f))
		{
			m_timer -= 0.5f;
			float num = (float)(Profiler.GetMonoUsedSizeLong() >> 10) / 1024f;
			float num2 = (float)(Profiler.GetMonoHeapSizeLong() >> 10) / 1024f;
			float num3 = (float)(Profiler.GetTotalAllocatedMemoryLong() >> 10) / 1024f;
			float num4 = (float)(Profiler.GetTotalReservedMemoryLong() >> 10) / 1024f;
			m_fpsTextUI.SetText("{0}", (int)m_fPSCounter.Fps);
			m_gcTextUI.SetText("{0}", GC.CollectionCount(0));
			m_monoUsedTextUI.SetText("{0}", (int)num);
			m_monoTotalTextUI.SetText("{0}", (int)num2);
			m_unityUsedTextUI.SetText("{0}", (int)num3);
			m_unityTotalTextUI.SetText("{0}", (int)num4);
		}
	}
}
