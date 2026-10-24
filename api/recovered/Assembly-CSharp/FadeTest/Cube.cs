using DG.Tweening;
using UnityEngine;

namespace FadeTest;

public class Cube : MonoBehaviour
{
	[SerializeField]
	private GameObject m_cube;

	private void Start()
	{
		base.transform.DOLocalRotate(new Vector3(30f, 10f, 60f), 1f).SetEase(Ease.Linear).SetRelative()
			.SetLoops(-1, LoopType.Incremental);
	}

	private void Update()
	{
	}
}
