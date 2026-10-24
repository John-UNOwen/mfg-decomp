using TMPro;
using UnityEngine;

public class ErrorMode_NonConsumed : MonoBehaviour
{
	[SerializeField]
	private GameObject go_frame;

	[SerializeField]
	private TextMeshProUGUI tmp_message;

	private void Awake()
	{
		go_frame.SetActive(value: false);
		tmp_message.text = "";
	}

	private void Start()
	{
	}
}
