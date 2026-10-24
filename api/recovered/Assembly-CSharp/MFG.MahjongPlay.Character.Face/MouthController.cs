using UnityEngine;

namespace MFG.MahjongPlay.Character.Face;

[ExecuteAlways]
public class MouthController : MonoBehaviour
{
	[SerializeField]
	private FaceController _faceController;

	[SerializeField]
	private int _mouthIndex;

	[SerializeField]
	private Vector2 _mouthOffset;

	[Range(0f, 1f)]
	[SerializeField]
	public float CheekValue;

	private void Update()
	{
		ForceUpdate();
	}

	public void ForceUpdate()
	{
		if (_faceController != null)
		{
			_faceController.CheekValue = CheekValue;
			_faceController.CurMouthIndex = _mouthIndex;
			_faceController.CurMouthOffset = _mouthOffset;
		}
	}

	public void Attatch(FaceController faceController)
	{
		_faceController = faceController;
		_mouthIndex = 0;
		_mouthOffset = Vector2.zero;
		CheekValue = 0f;
	}
}
