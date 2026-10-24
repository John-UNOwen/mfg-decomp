using UnityEngine;

namespace Konami.VirtualSurround.Demo;

[RequireComponent(typeof(AudioSource))]
public class Player : MonoBehaviour
{
	private AudioSource source;

	private void Start()
	{
		source = GetComponent<AudioSource>();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			if (source.isPlaying)
			{
				source.Pause();
			}
			else
			{
				source.Play();
			}
		}
		if (Input.GetKeyDown(KeyCode.S))
		{
			source.Stop();
		}
	}
}
