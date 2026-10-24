using UnityEngine;

public class DialogManager : MonoBehaviour
{
	[SerializeField]
	private GameObject m_prefabNormalWindow;

	[SerializeField]
	private GameObject m_prefabPaseliWindow;

	[SerializeField]
	private GameObject m_consumeWindow;

	private static DialogManager m_self;

	public static DialogManager Instance => m_self;

	private void Awake()
	{
		if (m_self == null)
		{
			m_self = this;
		}
	}

	public DialogWindow CreateNormalDialog(Transform parent)
	{
		DialogWindow dialogWindow = CreateNormalDialog();
		dialogWindow.transform.SetParent(parent, worldPositionStays: false);
		return dialogWindow;
	}

	private DialogWindow CreateNormalDialog()
	{
		GameObject obj = Object.Instantiate(m_prefabNormalWindow);
		obj.transform.SetParent(base.transform, worldPositionStays: false);
		return obj.GetComponent<DialogWindow>();
	}

	public PaseliWindow CreatePaseliDialog(Transform parent)
	{
		PaseliWindow paseliWindow = CreatePaseliDialog();
		paseliWindow.transform.SetParent(parent, worldPositionStays: false);
		return paseliWindow;
	}

	private PaseliWindow CreatePaseliDialog()
	{
		GameObject obj = Object.Instantiate(m_prefabPaseliWindow);
		obj.transform.SetParent(base.transform, worldPositionStays: false);
		return obj.GetComponent<PaseliWindow>();
	}

	public ConsumeWindow CreateConsumeDialog(Transform parent)
	{
		ConsumeWindow consumeWindow = CreateConsumeDialog();
		consumeWindow.transform.SetParent(parent, worldPositionStays: false);
		return consumeWindow;
	}

	private ConsumeWindow CreateConsumeDialog()
	{
		GameObject obj = Object.Instantiate(m_consumeWindow);
		obj.transform.SetParent(base.transform, worldPositionStays: false);
		return obj.GetComponent<ConsumeWindow>();
	}

	private void DialogAllDelete()
	{
		PaseliWindow[] array = Resources.FindObjectsOfTypeAll<PaseliWindow>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Exit();
		}
	}
}
