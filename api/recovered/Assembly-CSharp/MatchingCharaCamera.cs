using System;
using Cysharp.Threading.Tasks;
using MFG.MasterGameData;
using MFG.Types;
using UnityEngine;

public class MatchingCharaCamera : MonoBehaviour
{
	private float RotateTime = 1.6f;

	[SerializeField]
	[Header("中心オブジェ")]
	private GameObject RotateCenter;

	[Header("カメラ")]
	public Camera Cam;

	[SerializeField]
	[Header("カメラ角度用アニメーター")]
	private Animator CamRotateAnimator;

	[SerializeField]
	[Header("角度取得用オブジェ RadObjの相対角度をposition.xで指定")]
	private Transform RotateObj;

	[SerializeField]
	[Header("目線オブジェ position xから順に 中心との距離/未実装/煽り俯瞰角度(負:煽り)")]
	private Transform EyeLevelObj;

	[SerializeField]
	[Header("中心移動オブジェ")]
	private Transform CenterMoveObj;

	public async UniTask CameraMove(CharaType chara)
	{
		RotateTime = MasterGameDataManager.Character.Properties(chara).MatchingCharaCameraRotateTime;
		Cam.enabled = true;
		Vector3 centerPos = RotateCenter.transform.position;
		CamRotateAnimator.SetTrigger($"{chara}");
		float startTime = Time.time;
		while (Time.time - startTime < RotateTime)
		{
			float x = RotateObj.transform.localPosition.x;
			double num = Math.PI * (double)x / 180.0;
			Cam.transform.localPosition = new Vector3((float)((double)EyeLevelObj.transform.localPosition.x * Math.Cos(num)), EyeLevelObj.transform.localPosition.y, (float)((double)EyeLevelObj.transform.localPosition.x * Math.Sin(num)));
			Cam.transform.LookAt(RotateCenter.transform);
			Cam.transform.localEulerAngles = new Vector3(EyeLevelObj.transform.localPosition.z, Cam.transform.localEulerAngles.y, Cam.transform.localEulerAngles.z);
			RotateCenter.transform.position = new Vector3(centerPos.x + CenterMoveObj.transform.localPosition.x, centerPos.y + CenterMoveObj.transform.localPosition.y, centerPos.z + CenterMoveObj.transform.localPosition.z);
			await UniTask.DelayFrame(1);
		}
		SoundManager.PlaySe("SFX_SYS_ENTRY2").Forget();
	}
}
