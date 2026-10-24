using UnityEngine;

public class CutinHaiController : MonoBehaviour
{
	[SerializeField]
	public GameObject m_tileParent;

	private void Awake()
	{
	}

	public void Play()
	{
		Animator component = GetComponent<Animator>();
		component.gameObject.SetActive(value: false);
		component.gameObject.SetActive(value: true);
		component.Play("AgariHaiPutOutIn", 0, 0f);
		component.Update(0f);
	}
}
