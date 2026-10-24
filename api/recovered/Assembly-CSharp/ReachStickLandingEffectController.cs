using MFG.MahjongPlay.Cutin;
using UnityEngine;

public class ReachStickLandingEffectController : MonoBehaviour
{
	[SerializeField]
	private Camera m_landingEffectCamera;

	public void Play(IPlayerInteractor playerInteractor)
	{
		Vector3 position = playerInteractor.LandedReachStickParent().transform.position;
		Play(position);
	}

	public void Play(Vector3 effectWorldPos)
	{
		Vector3 vector = m_landingEffectCamera.WorldToViewportPoint(effectWorldPos);
		vector.z = 0f;
		RectTransform component = GetComponent<RectTransform>();
		component.anchoredPosition = new Vector3(-180f, 0f, 0f);
		component.anchorMax = vector;
		component.anchorMin = vector;
		GetComponentInChildren<Animator>().SetTrigger("Zi");
	}
}
