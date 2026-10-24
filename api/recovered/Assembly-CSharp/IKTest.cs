using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class IKTest : MonoBehaviour
{
	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private Transform _lookTarget;

	[SerializeField]
	private Transform _leftHandIkTarget;

	public Transform RightHandIkTarget;

	private bool _isDiscardActive;

	private bool _isMeldActive;

	private void Start()
	{
		ObservableStateMachineTrigger[] behaviours = GetComponent<Animator>().GetBehaviours<ObservableStateMachineTrigger>();
		foreach (ObservableStateMachineTrigger trigger in behaviours)
		{
			SubscribeDiscardIK(trigger);
		}
	}

	private void SubscribeDiscardIK(ObservableStateMachineTrigger trigger)
	{
		string stateName = "Hand_Dahai";
		(from x in trigger.OnStateEnterAsObservable()
			where x.StateInfo.IsName(stateName)
			select x).Subscribe(delegate
		{
			_isDiscardActive = true;
		}).AddTo(this);
		(from x in trigger.OnStateExitAsObservable()
			where x.StateInfo.IsName(stateName)
			select x).Subscribe(delegate
		{
			_isDiscardActive = false;
		}).AddTo(this);
		(from x in trigger.OnStateIKAsObservable()
			where x.StateInfo.IsName(stateName)
			select x).Subscribe(delegate(ObservableStateMachineTrigger.OnStateInfo onStateInfo)
		{
			if (_isDiscardActive)
			{
				float num = Mathf.Repeat(onStateInfo.StateInfo.normalizedTime, 1f);
				float value = ((!(num <= 0.5f)) ? Mathf.Lerp(1f, 0f, (num - 0.5f) / 0.5f) : Mathf.Lerp(0f, 1f, num / 0.5f));
				Animator animator = onStateInfo.Animator;
				animator.SetIKPositionWeight(AvatarIKGoal.RightHand, value);
				animator.SetIKPosition(AvatarIKGoal.RightHand, RightHandIkTarget.position);
			}
		}).AddTo(this);
	}

	private void Update()
	{
		if (!_isDiscardActive && Input.GetKeyDown(KeyCode.I))
		{
			_animator.SetTrigger("Emotion7");
		}
	}

	private void OnAnimatorIK(int layerIndex)
	{
		_ = _animator == null;
	}
}
