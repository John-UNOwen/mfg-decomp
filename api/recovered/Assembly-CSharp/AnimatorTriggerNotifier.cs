using UnityEngine;

public class AnimatorTriggerNotifier : MonoBehaviour
{
	[SerializeField]
	private Animator TargetAnimator;

	public void NotifySetTrigger(string triggerName)
	{
		if (TargetAnimator != null)
		{
			TargetAnimator.SetTrigger(triggerName);
		}
	}
}
