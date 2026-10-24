using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace MFG.StoryPlay;

public class PlayAnime : Command
{
	private string _triggerName;

	private string _waitStateName = "";

	private Animator _animator;

	private bool _isExec;

	private float _hiSpeedRatio;

	public override async UniTask Parse(string[] values)
	{
		await base.Parse(values);
		type = CommandType.PlayAnime;
		targetName = values[3];
		_hiSpeedRatio = HiSpeedRatio();
		try
		{
			_hiSpeedRatio = float.Parse(values[13]);
		}
		catch
		{
		}
		string[] array = values[5].Split(' ');
		_triggerName = array[0];
		if (array.Length >= 2)
		{
			_waitStateName = array[1];
		}
	}

	private void PlaySpeed_Reflesh(bool isHiSpeed)
	{
		if (_animator != null && !string.IsNullOrEmpty(_waitStateName))
		{
			_animator.speed = (isHiSpeed ? _hiSpeedRatio : 1f);
		}
	}

	private bool IsHiSpeed()
	{
		return StoryPlayFlow.GetInstance().IsHiseed;
	}

	private float HiSpeedRatio()
	{
		return StoryPlayFlow.GetInstance().fastForwardSpeed;
	}

	public override async UniTask Exec()
	{
		_isExec = true;
		(from _ in StoryPlayFlow.GetInstance().ObserveEveryValueChanged((StoryPlayFlow x) => x.IsHiseed)
			where _isExec
			select _).Subscribe(delegate(bool isHispeed)
		{
			PlaySpeed_Reflesh(isHispeed);
		}).AddTo(StoryPlayFlow.GetInstance());
		string n = targetName + "(Clone)";
		Transform animTransform = StoryPlayFlow.GetInstance().animeTop.transform.Find(n);
		if (animTransform == null)
		{
			GameObject gameObject = await StoryPlayFlow.GetInstance().InstantiateAnime("Story/" + targetName + ".prefab");
			if (gameObject != null)
			{
				animTransform = gameObject.transform;
			}
		}
		if ((bool)animTransform)
		{
			animTransform.gameObject.SetActive(value: true);
			if (_triggerName != "")
			{
				_animator = animTransform.GetComponent<Animator>();
				_animator.SetTrigger(_triggerName);
				PlaySpeed_Reflesh(IsHiSpeed());
				if (!string.IsNullOrEmpty(_waitStateName))
				{
					await UniTask.WaitUntil(() => _animator.GetCurrentAnimatorStateInfo(0).IsName(_waitStateName), PlayerLoopTiming.Update, token);
					await UniTask.WaitWhile(() => _animator.GetCurrentAnimatorStateInfo(0).IsName(_waitStateName), PlayerLoopTiming.Update, token);
				}
			}
		}
		else
		{
			Debug.LogError("アニメが見つかりません:" + targetName);
		}
		_isExec = false;
	}
}
