using UnityEngine;

public class Standing3DManager : MonoBehaviour
{
	[SerializeField]
	private GameObject m_parent3D;

	private GameObject instance3D;

	public void CharaLoad(GameObject prefab)
	{
		instance3D = Object.Instantiate(prefab, m_parent3D.transform, worldPositionStays: false);
		instance3D.name = instance3D.name.Replace("(Clone)", "");
	}
}
