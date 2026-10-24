using System;
using System.Collections.Generic;
using UnityEngine;

namespace MFG.MahjongPlay.Character.Face;

public class FaceController : MonoBehaviour
{
	private class MaterialContext : IDisposable
	{
		public Material FaceMat;

		public Material EyeLMat;

		public Material EyeRMat;

		public Material MouthMat;

		public void AffectToSharedMaterial(GameObject faceNode, GameObject eyeLNode, GameObject eyeRNode, GameObject mouthNode)
		{
			faceNode.GetComponent<SkinnedMeshRenderer>().sharedMaterial = FaceMat;
			eyeLNode.GetComponent<SkinnedMeshRenderer>().sharedMaterial = EyeLMat;
			eyeRNode.GetComponent<SkinnedMeshRenderer>().sharedMaterial = EyeRMat;
			mouthNode.GetComponent<SkinnedMeshRenderer>().sharedMaterial = MouthMat;
		}

		public void ReserveSharedMaterial(GameObject faceNode, GameObject eyeLNode, GameObject eyeRNode, GameObject mouthNode)
		{
			FaceMat = faceNode.GetComponent<SkinnedMeshRenderer>().sharedMaterial;
			EyeLMat = eyeLNode.GetComponent<SkinnedMeshRenderer>().sharedMaterial;
			EyeRMat = eyeRNode.GetComponent<SkinnedMeshRenderer>().sharedMaterial;
			MouthMat = mouthNode.GetComponent<SkinnedMeshRenderer>().sharedMaterial;
		}

		public void Clone(MaterialContext org)
		{
			FaceMat = new Material(org.FaceMat);
			EyeLMat = new Material(org.EyeLMat);
			EyeRMat = new Material(org.EyeRMat);
			MouthMat = new Material(org.MouthMat);
		}

		public void ChangeBaseTexture(Texture texture)
		{
			FaceMat.mainTexture = texture;
			EyeLMat.mainTexture = texture;
			EyeRMat.mainTexture = texture;
			MouthMat.mainTexture = texture;
		}

		public void Dispose()
		{
			if ((bool)FaceMat)
			{
				UnityEngine.Object.DestroyImmediate(FaceMat);
				FaceMat = null;
			}
			if ((bool)EyeLMat)
			{
				UnityEngine.Object.DestroyImmediate(EyeLMat);
				EyeLMat = null;
			}
			if ((bool)EyeRMat)
			{
				UnityEngine.Object.DestroyImmediate(EyeRMat);
				EyeRMat = null;
			}
			if ((bool)MouthMat)
			{
				UnityEngine.Object.DestroyImmediate(MouthMat);
				MouthMat = null;
			}
		}

		public bool IsValid()
		{
			if ((bool)FaceMat && (bool)EyeLMat && (bool)EyeRMat)
			{
				return MouthMat;
			}
			return false;
		}
	}

	[SerializeField]
	private GameObject _faceMatNode;

	[SerializeField]
	private GameObject _eyeLeftMatNode;

	[SerializeField]
	private GameObject _eyeRightMatNode;

	[SerializeField]
	private GameObject _mouthMatNode;

	[SerializeField]
	private GameObject _eyeNode;

	private MaterialContext _orgSharedMatContext = new MaterialContext();

	private MaterialContext _curMatContext = new MaterialContext();

	[HideInInspector]
	public Vector2 CurLeftPupliOffset;

	[HideInInspector]
	public Vector2 CurRightPupliOffset;

	[HideInInspector]
	public Vector2 CurLeftEyebrowOffset;

	[HideInInspector]
	public Vector2 CurRightEyebrowOffset;

	[HideInInspector]
	public Vector2 CurLeftEyeOffset;

	[HideInInspector]
	public Vector2 CurRightEyeOffset;

	[HideInInspector]
	public Vector2 CurMouthOffset;

	[SerializeField]
	private List<Texture> _baseTextures = new List<Texture>();

	public int BaseTextureIndex;

	private int _prevBaseTextureIndex;

	[HideInInspector]
	public int CurLeftPupliIndex;

	[HideInInspector]
	public int CurLeftEyeIndex;

	[HideInInspector]
	public int CurLeftEyebrowIndex;

	[HideInInspector]
	public int CurRightPupliIndex;

	[HideInInspector]
	public int CurRightEyeIndex;

	[HideInInspector]
	public int CurRightEyebrowIndex;

	[HideInInspector]
	public int CurMouthIndex;

	[Range(0f, 1f)]
	[HideInInspector]
	public float CheekValue;

	private Vector2 _prevLeftPupliOffset;

	private Vector2 _prevRightPupliOffset;

	private Vector2 _prevLeftEyebrowOffset;

	private Vector2 _prevRightEyebrowOffset;

	private Vector2 _prevLeftEyeOffset;

	private Vector2 _prevRightEyeOffset;

	private Vector2 _prevMouthOffset;

	private int _prevLeftPupliIndex;

	private int _prevLeftEyeIndex;

	private int _prevLeftEyebrowIndex;

	private int _prevRightPupliIndex;

	private int _prevRightEyeIndex;

	private int _prevRightEyebrowIndex;

	private int _prevMouthIndex;

	[Range(0f, 1f)]
	private float _prevCheekValue;

	private float _cheekVelocity;

	[SerializeField]
	private float _cheekChangeTime = 0.5f;

	[SerializeField]
	public float _defaultCheekValue = 0.5f;

	private bool _forceUpdate = true;

	[HideInInspector]
	public float CurDampedCheekValue;

	public Transform EyeTransform { get; private set; }

	private void Awake()
	{
	}

	private void Start()
	{
		MaterialContext_Init();
		if ((bool)_eyeNode)
		{
			EyeTransform = _eyeNode.transform;
		}
		CurDampedCheekValue = CheekValue;
		UpdateFacePattern(forceUpdate: true);
	}

	private void LateUpdate()
	{
		CurDampedCheekValue = Mathf.SmoothDamp(CurDampedCheekValue, CheekValue, ref _cheekVelocity, _cheekChangeTime, 1f, Time.unscaledDeltaTime);
		UpdateFacePattern(_forceUpdate);
		_forceUpdate = false;
	}

	public void UpdateFacePattern(bool forceUpdate)
	{
		if (!_curMatContext.IsValid())
		{
			return;
		}
		if (forceUpdate)
		{
			CurDampedCheekValue = CheekValue;
		}
		if (CurDampedCheekValue != _prevCheekValue || forceUpdate)
		{
			_curMatContext.FaceMat.SetFloat("_GBlend", CurDampedCheekValue + _defaultCheekValue);
			_curMatContext.EyeLMat.SetFloat("_GBlend", CurDampedCheekValue + _defaultCheekValue);
			_curMatContext.MouthMat.SetFloat("_GBlend", CurDampedCheekValue + _defaultCheekValue);
			_curMatContext.EyeRMat.SetFloat("_GBlend", CurDampedCheekValue + _defaultCheekValue);
			_prevCheekValue = CurDampedCheekValue;
		}
		if (BaseTextureIndex != _prevBaseTextureIndex || forceUpdate)
		{
			if (BaseTextureIndex < _baseTextures.Count)
			{
				_curMatContext.ChangeBaseTexture(_baseTextures[BaseTextureIndex]);
			}
			_prevBaseTextureIndex = BaseTextureIndex;
		}
		if (CurMouthIndex != _prevMouthIndex || forceUpdate)
		{
			_curMatContext.MouthMat.SetFloat("_MouthIndex", CurMouthIndex);
			_prevMouthIndex = CurMouthIndex;
		}
		if (!CurMouthOffset.Equals(_prevMouthOffset) || forceUpdate)
		{
			_curMatContext.MouthMat.SetFloat("_MouthOffsetX", CurMouthOffset.x);
			_curMatContext.MouthMat.SetFloat("_MouthOffsetY", CurMouthOffset.y);
			_prevMouthOffset = CurMouthOffset;
		}
		if (CurLeftPupliIndex != _prevLeftPupliIndex || forceUpdate)
		{
			_curMatContext.EyeLMat.SetFloat("_PupilIndex", CurLeftPupliIndex);
			_prevLeftPupliIndex = CurLeftPupliIndex;
		}
		if (CurLeftEyeIndex != _prevLeftEyeIndex || forceUpdate)
		{
			_curMatContext.EyeLMat.SetFloat("_EyeIndex", CurLeftEyeIndex);
			_prevLeftEyeIndex = CurLeftEyeIndex;
		}
		if (CurLeftEyebrowIndex != _prevLeftEyebrowIndex || forceUpdate)
		{
			_curMatContext.EyeLMat.SetFloat("_EyeBrowIndex", CurLeftEyebrowIndex);
			_prevLeftEyebrowIndex = CurLeftEyebrowIndex;
		}
		if (!CurLeftPupliOffset.Equals(_prevLeftPupliOffset) || forceUpdate)
		{
			_curMatContext.EyeLMat.SetFloat("_PupliOffsetX", CurLeftPupliOffset.x);
			_curMatContext.EyeLMat.SetFloat("_PupliOffsetY", CurLeftPupliOffset.y);
			_prevLeftPupliOffset = CurLeftPupliOffset;
		}
		if (!CurLeftEyeOffset.Equals(_prevLeftEyeOffset) || forceUpdate)
		{
			_curMatContext.EyeLMat.SetFloat("_EyeOffsetX", CurLeftEyeOffset.x);
			_curMatContext.EyeLMat.SetFloat("_EyeOffsetY", CurLeftEyeOffset.y);
			_prevLeftEyeOffset = CurLeftEyeOffset;
		}
		if (!CurLeftEyebrowOffset.Equals(_prevLeftEyebrowOffset) || forceUpdate)
		{
			_curMatContext.EyeLMat.SetFloat("_EyeBrowOffsetX", CurLeftEyebrowOffset.x);
			_curMatContext.EyeLMat.SetFloat("_EyeBrowOffsetY", CurLeftEyebrowOffset.y);
			_prevLeftEyebrowOffset = CurLeftEyebrowOffset;
		}
		if (CurRightPupliIndex != _prevRightPupliIndex || forceUpdate)
		{
			_curMatContext.EyeRMat.SetFloat("_PupilIndex", CurRightPupliIndex);
			_prevRightPupliIndex = CurRightPupliIndex;
		}
		if (CurRightEyeIndex != _prevRightEyeIndex || forceUpdate)
		{
			_curMatContext.EyeRMat.SetFloat("_EyeIndex", CurRightEyeIndex);
			_prevRightEyeIndex = CurRightEyeIndex;
		}
		if (CurRightEyebrowIndex != _prevRightEyebrowIndex || forceUpdate)
		{
			_curMatContext.EyeRMat.SetFloat("_EyeBrowIndex", CurRightEyebrowIndex);
			_prevRightEyebrowIndex = CurRightEyebrowIndex;
		}
		if (!CurRightPupliOffset.Equals(_prevRightPupliOffset) || forceUpdate)
		{
			_curMatContext.EyeRMat.SetFloat("_PupliOffsetX", CurRightPupliOffset.x);
			_curMatContext.EyeRMat.SetFloat("_PupliOffsetY", CurRightPupliOffset.y);
			_prevRightPupliOffset = CurRightPupliOffset;
		}
		if (!CurRightEyeOffset.Equals(_prevRightEyeOffset) || forceUpdate)
		{
			_curMatContext.EyeRMat.SetFloat("_EyeOffsetX", CurRightEyeOffset.x);
			_curMatContext.EyeRMat.SetFloat("_EyeOffsetY", CurRightEyeOffset.y);
			_prevRightEyeOffset = CurRightEyeOffset;
		}
		if (!CurRightEyebrowOffset.Equals(_prevRightEyebrowOffset) || forceUpdate)
		{
			_curMatContext.EyeRMat.SetFloat("_EyeBrowOffsetX", CurRightEyebrowOffset.x);
			_curMatContext.EyeRMat.SetFloat("_EyeBrowOffsetY", CurRightEyebrowOffset.y);
			_prevRightEyebrowOffset = CurRightEyebrowOffset;
		}
	}

	private void OnDestroy()
	{
		MaterialContext_Destroy();
	}

	private void MaterialContext_Init()
	{
		if ((bool)_faceMatNode && (bool)_eyeLeftMatNode && (bool)_eyeRightMatNode && (bool)_mouthMatNode)
		{
			_orgSharedMatContext.ReserveSharedMaterial(_faceMatNode, _eyeLeftMatNode, _eyeRightMatNode, _mouthMatNode);
			_curMatContext.Clone(_orgSharedMatContext);
			_curMatContext.AffectToSharedMaterial(_faceMatNode, _eyeLeftMatNode, _eyeRightMatNode, _mouthMatNode);
		}
	}

	private void MaterialContext_Destroy()
	{
		_curMatContext.Dispose();
	}

	public void Attatch(GameObject face, GameObject eyeL, GameObject eyeR, GameObject mouth, GameObject eyeNode, List<Texture> baseTexes)
	{
		_faceMatNode = face;
		_eyeLeftMatNode = eyeL;
		_eyeRightMatNode = eyeR;
		_mouthMatNode = mouth;
		_eyeNode = eyeNode;
		_baseTextures = baseTexes;
		BaseTextureIndex = 0;
		_cheekChangeTime = 0.5f;
		_defaultCheekValue = 0.5f;
	}
}
