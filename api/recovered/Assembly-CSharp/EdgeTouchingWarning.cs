using UnityEngine;
using UnityEngine.EventSystems;

public class EdgeTouchingWarning : MonoBehaviour
{
	private enum State
	{
		None,
		Disp
	}

	[SerializeField]
	private GameObject _window;

	[SerializeField]
	private float _thresholdTime = 1f;

	[SerializeField]
	private float _hideThresholdTime = 1f;

	[SerializeField]
	private float _edgeThickness = 50f;

	private float _duration;

	private State _state;

	private void OnEnable()
	{
		_window.SetActive(value: false);
		_duration = 0f;
		_state = State.None;
	}

	private void Update()
	{
		BaseInput customInput = ResidentFlow.CustomInput;
		if (customInput == null || !base.gameObject.activeInHierarchy)
		{
			_window.SetActive(value: false);
			_duration = 0f;
			_state = State.None;
			return;
		}
		switch (_state)
		{
		case State.None:
			if (customInput.GetMouseButton(0) && IsInEdgeArea(customInput.mousePosition, _edgeThickness))
			{
				_duration += Time.deltaTime;
			}
			else
			{
				_duration = 0f;
			}
			if (_duration >= _thresholdTime)
			{
				_window.SetActive(value: true);
				_duration = 0f;
				_state = State.Disp;
			}
			break;
		case State.Disp:
			if (!customInput.GetMouseButton(0) || (customInput.GetMouseButton(0) && !IsInEdgeArea(customInput.mousePosition, _edgeThickness)))
			{
				_duration += Time.deltaTime;
			}
			else
			{
				_duration = 0f;
			}
			if (_duration >= _hideThresholdTime)
			{
				_window.SetActive(value: false);
				_duration = 0f;
				_state = State.None;
			}
			break;
		}
	}

	private bool IsInEdgeArea(Vector2 mousePosition, float edgeThickness)
	{
		if (mousePosition.x < edgeThickness || mousePosition.x > (float)Screen.width - edgeThickness || mousePosition.y < edgeThickness || mousePosition.y > (float)Screen.height - edgeThickness)
		{
			return true;
		}
		return false;
	}
}
