using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.MasterGameData;
using MFG.Types;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StandingCharaController : MonoBehaviour
{
	public enum MotionID
	{
		Normal,
		Joy,
		Sad,
		Surprise,
		Anger,
		Proud,
		Serious,
		Shy,
		Cry,
		Sleep,
		Panic,
		Shock
	}

	public enum ManpuID
	{
		Manpu1,
		Manpu2,
		Manpu3,
		Manpu4,
		Manpu5,
		Manpu6,
		Manpu7,
		Manpu8,
		Manpu9,
		Manpu10,
		Manpu11,
		Manpu12,
		Manpu13,
		Manpu14,
		Manpu15,
		Manpu16,
		Manpu17
	}

	public CharaType CharaType;

	public GameObject Parent2D;

	public GameObject Parent3D;

	public Animator CharaAnimator;

	public Animator ManpuAnimator;

	private Canvas _parent2DCanvas;

	private bool _isParent2DCanvasSearched;

	public Transform ManpuParent;

	public bool EnableTouchRotate;

	private int preVoiceNum;

	private bool playingTouchVoice;

	private Vector2 _touchStartPosition = Vector2.zero;

	private Vector2 _touchCurrentPosition = Vector2.zero;

	private float _startRot;

	private bool _isToubhBegin;

	private float _flickThreshold = 3f;

	private float _flickLengthMax = 10f;

	private float _touchRotThreshold = 20f;

	private float _flickRotSpeedMax = 10f;

	private float _rotDecayRate = 0.9f;

	private float _touchRotSensitivity = 1000f;

	private float _touchRotDegInFrame = 10f;

	private float _rotSpeed;

	private bool _isOnRotateTrigger;

	private bool _isOnRotatePress;

	public IObservable<PointerEventData> OnCharaTouched => Parent2D.transform.Find("Base/Touch").GetComponent<ObservableEventTrigger>().OnPointerDownAsObservable();

	public void Init(CharaType charaType, GameObject parent3D, GameObject parent2D)
	{
		CharaType = charaType;
		Parent3D = parent3D;
		Parent2D = parent2D;
		CharaAnimator = Parent3D.GetComponentInChildren<Animator>();
		foreach (Transform item in ManpuParent)
		{
			try
			{
				if (int.TryParse(item.name.Replace("Manpu", ""), out var result))
				{
					item.GetComponent<RectTransform>().localPosition = MasterGameDataManager.Character.Properties(charaType).ManpuPosition[result - 1];
				}
			}
			catch
			{
			}
		}
	}

	private Canvas SearchParent2DCanvas(Transform parent)
	{
		if (parent == null)
		{
			return null;
		}
		Canvas component = parent.GetComponent<Canvas>();
		if (component != null)
		{
			return component;
		}
		return SearchParent2DCanvas(parent.parent);
	}

	public void PlayMotion(MotionID motionID)
	{
		PlayMotion(motionID.ToString());
	}

	public void PlayMotion(string motionIDString)
	{
		if (CharaAnimator != null)
		{
			CharaAnimator.SetTrigger(motionIDString);
		}
	}

	public void PlayMotion(string motionIDString, int motionNum)
	{
		if (CharaAnimator != null)
		{
			CharaAnimator.SetInteger(motionIDString, motionNum);
		}
	}

	public void PlayTouchMotion(int motionNum)
	{
		PlayMotion("TouchNum", motionNum);
		PlayMotion("Touch");
	}

	public void PlayManpu(ManpuID manpuID)
	{
		PlayManpu(manpuID.ToString());
	}

	public void PlayManpu(string manpuIDString)
	{
		if (ManpuAnimator != null)
		{
			ManpuAnimator.SetTrigger(manpuIDString);
		}
	}

	public bool IsState(string state)
	{
		if (CharaAnimator != null)
		{
			return CharaAnimator.GetCurrentAnimatorStateInfo(0).IsName(state);
		}
		return false;
	}

	public void SetDisp(bool isDisp)
	{
		Parent2D.transform.Find("Base").gameObject.SetActive(isDisp);
		Parent3D.transform.Find("Camera").GetComponent<Camera>().enabled = isDisp;
		Light componentInChildren = Parent3D.transform.GetComponentInChildren<Light>();
		if (componentInChildren != null)
		{
			componentInChildren.enabled = isDisp;
		}
	}

	public void SetTargetTexture(RenderTexture renderTexture, bool isMatching = false)
	{
		RawImage componentInChildren = Parent2D.GetComponentInChildren<RawImage>(includeInactive: true);
		if (isMatching)
		{
			componentInChildren.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(1760f, 1600f);
		}
		componentInChildren.texture = renderTexture;
	}

	private void Start()
	{
		Parent2D.transform.Find("Base/Touch").GetComponent<ObservableEventTrigger>().OnPointerDownAsObservable()
			.Subscribe(delegate
			{
				_isOnRotateTrigger = true;
				_isOnRotatePress = true;
			})
			.AddTo(this);
	}

	private void Update()
	{
		ControlRotation();
		_isOnRotateTrigger = false;
	}

	public async UniTask PlayTouchedVoice(CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		await UniTask.WaitWhile(() => playingTouchVoice);
		playingTouchVoice = true;
		SoundManager.StopVoiceAll().Forget();
		int num;
		for (num = UnityEngine.Random.Range(1, 4); num == preVoiceNum; num = UnityEngine.Random.Range(1, 4))
		{
		}
		SoundManager.PlayVoice("VOX_GXX_TOUCH_" + num, CharaType).Forget();
		PlayTouchMotion(num);
		preVoiceNum = num;
		await UniTask.Delay(TimeSpan.FromSeconds(2.0));
		playingTouchVoice = false;
	}

	private void RotateY(float degreeY)
	{
		Parent3D.transform.Find("Parent/Parent3D/CharaModel").Rotate(0f, degreeY, 0f);
	}

	public void AddRotateY(float degreeY)
	{
		Transform obj = Parent3D.transform.Find("Parent/Parent3D/CharaModel");
		Vector3 localEulerAngles = obj.localEulerAngles;
		localEulerAngles.y += degreeY;
		if (localEulerAngles.y > 0f)
		{
			localEulerAngles.y = Mathf.Repeat(localEulerAngles.y, 360f);
		}
		else if (localEulerAngles.y < 0f)
		{
			localEulerAngles.y = Mathf.Repeat(0f - localEulerAngles.y, 360f);
			localEulerAngles.y = 360f - localEulerAngles.y;
		}
		obj.localEulerAngles = localEulerAngles;
	}

	public float GetRotateY()
	{
		return Parent3D.transform.Find("Parent/Parent3D/CharaModel").localEulerAngles.y;
	}

	private void ControlRotation()
	{
		if (!EnableTouchRotate)
		{
			_rotSpeed = 0f;
			_isToubhBegin = false;
			_isOnRotateTrigger = false;
			_isOnRotatePress = false;
			_isParent2DCanvasSearched = false;
			return;
		}
		if (!_isParent2DCanvasSearched)
		{
			_parent2DCanvas = SearchParent2DCanvas(Parent2D.transform);
			_isParent2DCanvasSearched = true;
		}
		_rotSpeed *= _rotDecayRate;
		if (_isOnRotateTrigger)
		{
			Vector2 mousePosition = ResidentFlow.CustomInput.mousePosition;
			RectTransform obj = Parent2D.transform.Find("Base/Touch") as RectTransform;
			Camera cam = ((_parent2DCanvas != null) ? _parent2DCanvas.worldCamera : null);
			RectTransformUtility.ScreenPointToLocalPointInRectangle(obj, mousePosition, cam, out var localPoint);
			if (obj.rect.Contains(localPoint))
			{
				_startRot = GetRotateY();
				_touchStartPosition = ResidentFlow.CustomInput.mousePosition;
				_isToubhBegin = true;
			}
		}
		if (_isToubhBegin)
		{
			if (_isOnRotatePress)
			{
				Vector2 mousePosition2 = ResidentFlow.CustomInput.mousePosition;
				float num = -360f * ((mousePosition2 - _touchStartPosition).x / _touchRotSensitivity);
				num = ((!(Math.Abs(num) >= _touchRotDegInFrame)) ? num : ((num >= 0f) ? _touchRotDegInFrame : (-1f * _touchRotDegInFrame)));
				if (num > 0f)
				{
					num = Mathf.Repeat(num, 360f);
				}
				else if (num < 0f)
				{
					num = Mathf.Repeat(0f - num, 360f);
					num = 360f - num;
				}
				AddRotateY(num);
				_rotSpeed = 0f;
				_touchCurrentPosition = mousePosition2;
				_touchStartPosition = mousePosition2;
			}
			if (!ResidentFlow.CustomInput.GetMouseButton(0))
			{
				Vector2 vector = ResidentFlow.CustomInput.mousePosition - _touchCurrentPosition;
				int num2 = ((!(vector.x >= 0f)) ? 1 : (-1));
				float num3 = Mathf.Abs(vector.x);
				if (num3 >= _flickThreshold)
				{
					float value = (num3 - _flickThreshold) / _flickLengthMax;
					value = Mathf.Clamp(value, 0f, 1f);
					_rotSpeed += (float)num2 * value * _flickRotSpeedMax;
					_rotSpeed = Mathf.Clamp(_rotSpeed, 0f - _flickRotSpeedMax, _flickRotSpeedMax);
				}
				_isToubhBegin = false;
			}
		}
		RotateY(_rotSpeed);
	}
}
