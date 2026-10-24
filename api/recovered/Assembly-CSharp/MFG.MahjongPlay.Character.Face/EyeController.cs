using UnityEngine;

namespace MFG.MahjongPlay.Character.Face;

[ExecuteAlways]
public class EyeController : MonoBehaviour
{
	[SerializeField]
	private FaceController _faceController;

	public Transform LookPoint;

	[SerializeField]
	private Vector2 _lookOffset;

	[SerializeField]
	[Range(0f, 1f)]
	public float Lookweight = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _lookWeightNow;

	[SerializeField]
	private float _pupliMoveMax = 0.03f;

	[SerializeField]
	public bool IsChaseEye;

	[SerializeField]
	private float _pupliMoveDelta = 30f;

	[SerializeField]
	private Vector2 _leftEyebrowOffset = Vector2.zero;

	[SerializeField]
	private Vector2 _leftEyeOffset = Vector2.zero;

	[SerializeField]
	private Vector2 _leftPupliOffset = Vector2.zero;

	[SerializeField]
	private int _leftEyebrowIndex;

	[SerializeField]
	private int _leftPupliIndex;

	[SerializeField]
	private int _leftEyeIndex;

	[SerializeField]
	private Vector2 _rightEyebrowOffset = Vector2.zero;

	[SerializeField]
	private Vector2 _rightEyeOffset = Vector2.zero;

	[SerializeField]
	private Vector2 _rightPupliOffset = Vector2.zero;

	[SerializeField]
	private int _rightEyebrowIndex;

	[SerializeField]
	private int _rightPupliIndex;

	[SerializeField]
	private int _rightEyeIndex;

	[SerializeField]
	[Range(0f, 1f)]
	private float _inSpeed = 0.01f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _outSpeed = 0.01f;

	public float InSpeed
	{
		get
		{
			return _inSpeed;
		}
		set
		{
			_inSpeed = value;
		}
	}

	public float OutSpeed
	{
		get
		{
			return _outSpeed;
		}
		set
		{
			_outSpeed = value;
		}
	}

	private Vector3 GetIntersectPoint(Vector3 lookingPosition, Vector3 lookingForward, Vector3 targetForward, Vector3 targetPosition)
	{
		return lookingPosition + (Vector3.Dot(targetForward, targetPosition) - Vector3.Dot(targetForward, lookingPosition)) / Vector3.Dot(targetForward, lookingForward) * lookingForward;
	}

	private void LateUpdate()
	{
		ForceUpdate();
	}

	public void CalcLookPoint()
	{
		if (_faceController == null)
		{
			return;
		}
		if (IsChaseEye && (bool)LookPoint && (bool)_faceController.EyeTransform)
		{
			Vector3 position = _faceController.EyeTransform.position;
			Vector3 vector = _faceController.EyeTransform.right * -1f;
			Vector3 right = _faceController.EyeTransform.right;
			Vector3 position2 = LookPoint.transform.position;
			_ = (position2 - position).normalized + position;
			Vector3 intersectPoint = GetIntersectPoint(position, vector, right, position2);
			Debug.DrawLine(position, intersectPoint);
			Debug.DrawLine(intersectPoint, position2, Color.red);
			Debug.DrawRay(position2, right, Color.blue);
			if (Vector3.Dot(intersectPoint - position, vector) >= 0f)
			{
				Vector3 dir = _faceController.EyeTransform.InverseTransformDirection(intersectPoint - position2);
				Debug.DrawRay(position2, dir, Color.green);
				_lookOffset.x = Mathf.Clamp(dir.y / _pupliMoveDelta * _pupliMoveMax, 0f - _pupliMoveMax, _pupliMoveMax);
				_lookOffset.y = Mathf.Clamp(dir.z / _pupliMoveDelta * _pupliMoveMax, 0f - _pupliMoveMax, _pupliMoveMax);
				_lookWeightNow += (Lookweight - _lookWeightNow) * InSpeed;
			}
			else
			{
				_lookWeightNow += (0f - _lookWeightNow) * OutSpeed;
			}
		}
		else
		{
			_lookWeightNow += (0f - _lookWeightNow) * OutSpeed;
		}
	}

	public void ForceUpdate()
	{
		CalcLookPoint();
		if (_faceController != null)
		{
			_faceController.CurLeftEyebrowIndex = _leftEyebrowIndex;
			_faceController.CurLeftEyebrowOffset = _leftEyebrowOffset;
			_faceController.CurLeftEyeIndex = _leftEyeIndex;
			_faceController.CurLeftEyeOffset = _leftEyeOffset;
			_faceController.CurLeftPupliIndex = _leftPupliIndex;
			_faceController.CurLeftPupliOffset = _lookOffset * _lookWeightNow + _leftPupliOffset;
			_faceController.CurRightEyebrowIndex = _rightEyebrowIndex;
			_faceController.CurRightEyebrowOffset = _rightEyebrowOffset;
			_faceController.CurRightEyeIndex = _rightEyeIndex;
			_faceController.CurRightEyeOffset = _rightEyeOffset;
			_faceController.CurRightPupliIndex = _rightPupliIndex;
			_faceController.CurRightPupliOffset = _lookOffset * _lookWeightNow + _rightPupliOffset;
		}
	}

	public void Attatch(FaceController faceController)
	{
		_faceController = faceController;
	}
}
