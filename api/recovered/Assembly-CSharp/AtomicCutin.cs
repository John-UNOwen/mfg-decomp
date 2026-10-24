using Cysharp.Threading.Tasks;
using MFG.Types;
using UnityEngine;

public class AtomicCutin : MonoBehaviour
{
	[SerializeField]
	private Tile tile;

	[SerializeField]
	private GameObject screen;

	[SerializeField]
	private Camera camera;

	[SerializeField]
	private Animator screenAnim;

	private Animator animator => base.transform.GetComponent<Animator>();

	public async UniTask Play(Pai pai)
	{
		tile.SetHaiTexture(pai);
		camera.enabled = true;
		animator.SetTrigger("In");
		screenAnim.SetTrigger("In");
		Debug.Log($"Atomic Reach Play : {pai}");
		await UniTask.WaitUntil(() => screenAnim.GetCurrentAnimatorStateInfo(0).IsName("AtomicCut1"));
		await UniTask.WaitUntil(() => screenAnim.GetCurrentAnimatorStateInfo(0).IsName("AtomicCut2"));
		await UniTask.WaitUntil(() => screenAnim.GetCurrentAnimatorStateInfo(0).IsName("AtomicWait"));
		camera.enabled = false;
	}
}
