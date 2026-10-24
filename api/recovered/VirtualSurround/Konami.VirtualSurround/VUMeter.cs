using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Konami.VirtualSurround;

public class VUMeter : MonoBehaviour
{
	public float minDecibel = -120f;

	public int max_channels { get; private set; }

	public float[] vu { get; private set; }

	[DllImport("VirtualSurroundNative")]
	private static extern int VUMeter_GetMaxChannels();

	[DllImport("VirtualSurroundNative")]
	private static extern void VUMeter_GetVU(float[] vu_buf);

	public void Initialize()
	{
		max_channels = VUMeter_GetMaxChannels();
		vu = Enumerable.Repeat(0f, max_channels).ToArray();
	}

	public void UpdateVU()
	{
		VUMeter_GetVU(vu);
		for (int i = 0; i < vu.Length; i++)
		{
			vu[i] = Mathf.Max(vu[i], minDecibel);
		}
	}

	public static float FromDecibel(float vu)
	{
		return 0.05f * Mathf.Pow(10f, vu);
	}
}
