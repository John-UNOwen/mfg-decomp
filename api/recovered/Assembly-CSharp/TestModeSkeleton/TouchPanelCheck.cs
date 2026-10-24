using System;
using System.Collections;
using MFG.Types;
using TestModeSkeleton.TestModeExtensions;
using UnityEngine;
using UnityEngine.UI;

namespace TestModeSkeleton;

public class TouchPanelCheck : TestModeBase
{
	private enum STEP
	{
		CHECK,
		CALIBRATION_STEP1,
		CALIBRATION_STEP2,
		CALIBRATION_COMPLETE,
		CALIBRATION_ABORT,
		EXIT
	}

	[SerializeField]
	[Tooltip("タッチの状態")]
	private Text m_touchStatusTxt;

	[SerializeField]
	[Tooltip("タッチポイント")]
	private Text m_touchPoint;

	[SerializeField]
	[Tooltip("タッチポイント座標")]
	private Text m_touchPositionTxt;

	[SerializeField]
	[Tooltip("キャリブレーションタッチポイント")]
	private Text[] m_calibrationPoint;

	private Vector2[] m_calibratteTargetPoints = new Vector2[2];

	private Vector2[] m_calibratedPoints = new Vector2[2];

	private Vector2[] m_orgCalibratedPoints = new Vector2[2];

	private STEP m_step;

	protected override string TitleName => "<l>TOUCH PANEL CHECK</l>";

	private new void Awake()
	{
		base.Awake();
		m_touchPositionTxt.text = "OFF";
		m_touchStatusTxt.SetLocalizedText("<l>TOUCH STATUS</l>");
	}

	private TestModeHelp.HelpType GetTopMenuHelpType()
	{
		if (SystemConfigGameData.CabinetType == CabinetType.HG || SystemConfigGameData.CabinetType == CabinetType.UKS || SystemConfigGameData.CabinetType == CabinetType.TBS || SystemConfigGameData.CabinetType == CabinetType.UJK)
		{
			return TestModeHelp.HelpType.ServiceExit;
		}
		return TestModeHelp.HelpType.CalibrationExit;
	}

	private IEnumerator Start()
	{
		help.Set(GetTopMenuHelpType());
		ArrowkeyGameObject.SetActive(value: true);
		ArrowkeyEnter.SetActive(value: false);
		ArrowkeyUp.SetActive(value: false);
		ArrowkeyDown.SetActive(value: false);
		ArrowkeyLeft.SetActive(value: false);
		ArrowkeyRight.SetActive(value: false);
		SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
		Debug.Log($"Carlibrated:{CabinetManager.Calibration.Data.IsCalibrated} UL:{CabinetManager.Calibration.Data.UpperLeft} RL{CabinetManager.Calibration.Data.LowerRight}");
		Canvas componentInChildren = base.gameObject.GetComponentInChildren<Canvas>();
		componentInChildren.GetComponent<UIAttachCam>().Attatch();
		m_calibratteTargetPoints[0] = componentInChildren.worldCamera.WorldToScreenPoint(GameObject.Find("TouchPanelCheck/Canvas/CalibrationStep1").transform.position);
		m_calibratteTargetPoints[1] = componentInChildren.worldCamera.WorldToScreenPoint(GameObject.Find("TouchPanelCheck/Canvas/CalibrationStep2").transform.position);
		m_orgCalibratedPoints[0] = CabinetManager.Calibration.Data.UpperLeft;
		m_orgCalibratedPoints[1] = CabinetManager.Calibration.Data.LowerRight;
		Array.Copy(m_orgCalibratedPoints, m_calibratedPoints, m_calibratedPoints.Length);
		notice.StartTouchPanel();
		yield return CoCheck();
		notice.Stop();
		Exit();
	}

	private IEnumerator CoCheck()
	{
		while (true)
		{
			TestModeInput instance = SingletonBehaviour<TestModeInput>.Instance;
			bool flag = instance.IsTrigger(TestModeInput.Key.Service);
			bool flag2 = instance.IsTrigger(TestModeInput.Key.Enter) || flag;
			bool flag3 = instance.IsPress(TestModeInput.Key.Test) && flag;
			if (SystemConfigGameData.CabinetType == CabinetType.HG || SystemConfigGameData.CabinetType == CabinetType.UKS || SystemConfigGameData.CabinetType == CabinetType.TBS || SystemConfigGameData.CabinetType == CabinetType.UJK)
			{
				flag3 = false;
			}
			switch (m_step)
			{
			case STEP.CHECK:
				if (flag3)
				{
					m_touchStatusTxt.enabled = false;
					m_touchPoint.enabled = false;
					m_touchPositionTxt.text = "";
					ArrowkeyEnter.SetActive(value: false);
					GameObject.Find("TouchPanelCheck/Canvas/CalibrationStep1").SetActive(value: true);
					SetSubTitle("<l>TOUCH PANEL CALIBRATION</l>");
					notice.StartCalibration();
					help.Set(TestModeHelp.HelpType.CalibrationStep1);
					m_step = STEP.CALIBRATION_STEP1;
					break;
				}
				if (flag2)
				{
					yield return CoMainExit();
					OnBeforeExit();
					yield break;
				}
				if (SingletonBehaviour<TestModeInput>.Instance.IsTouchPress())
				{
					Vector2 touchPosition = SingletonBehaviour<TestModeInput>.Instance.GetTouchPosition();
					Canvas componentInChildren = base.gameObject.GetComponentInChildren<Canvas>();
					RectTransformUtility.ScreenPointToLocalPointInRectangle(componentInChildren.transform as RectTransform, touchPosition, componentInChildren.worldCamera, out var localPoint);
					m_touchPoint.transform.localPosition = localPoint;
					m_touchPoint.enabled = true;
					m_touchPositionTxt.text = $"({(int)touchPosition.x},{(int)touchPosition.y})";
				}
				else
				{
					m_touchPoint.enabled = false;
					m_touchPositionTxt.text = "OFF";
				}
				break;
			case STEP.CALIBRATION_STEP1:
				if (SingletonBehaviour<TestModeInput>.Instance.IsTouchTrigger())
				{
					m_calibratedPoints[0] = SingletonBehaviour<TestModeInput>.Instance.GetNoAdjustTouchPosition();
					GameObject.Find("TouchPanelCheck/Canvas/CalibrationStep1").SetActive(value: false);
					GameObject.Find("TouchPanelCheck/Canvas/CalibrationStep2").SetActive(value: true);
					help.Set(TestModeHelp.HelpType.CalibrationStep2);
					m_step = STEP.CALIBRATION_STEP2;
				}
				else if (flag)
				{
					m_step = STEP.CALIBRATION_ABORT;
				}
				break;
			case STEP.CALIBRATION_STEP2:
				if (SingletonBehaviour<TestModeInput>.Instance.IsTouchTrigger())
				{
					m_calibratedPoints[1] = SingletonBehaviour<TestModeInput>.Instance.GetNoAdjustTouchPosition();
					GameObject.Find("TouchPanelCheck/Canvas/CalibrationStep2").SetActive(value: false);
					m_step = STEP.CALIBRATION_COMPLETE;
				}
				else if (flag)
				{
					m_step = STEP.CALIBRATION_ABORT;
				}
				break;
			case STEP.CALIBRATION_COMPLETE:
				m_touchPoint.enabled = false;
				Calibrate(m_calibratedPoints[0], m_calibratedPoints[1]);
				notice.StartCalibrationComplete();
				yield return new WaitForSeconds(2f);
				notice.Stop();
				m_touchStatusTxt.enabled = true;
				m_touchPositionTxt.text = "OFF";
				ArrowkeyEnter.SetActive(value: false);
				help.Set(GetTopMenuHelpType());
				notice.StartTouchPanel();
				SetSubTitle("");
				m_step = STEP.CHECK;
				break;
			case STEP.CALIBRATION_ABORT:
				m_touchPoint.enabled = false;
				GameObject.Find("TouchPanelCheck/Canvas/CalibrationStep1").SetActive(value: false);
				GameObject.Find("TouchPanelCheck/Canvas/CalibrationStep2").SetActive(value: false);
				notice.StartAbort();
				yield return new WaitForSeconds(2f);
				notice.Stop();
				m_touchStatusTxt.enabled = true;
				m_touchPositionTxt.text = "OFF";
				ArrowkeyEnter.SetActive(value: false);
				help.Set(GetTopMenuHelpType());
				notice.StartTouchPanel();
				SetSubTitle("");
				m_step = STEP.CHECK;
				break;
			}
			yield return null;
		}
	}

	protected override bool IsModification()
	{
		if (!(m_calibratedPoints[0] != m_orgCalibratedPoints[0]))
		{
			return m_calibratedPoints[1] != m_orgCalibratedPoints[1];
		}
		return true;
	}

	protected override void OnSave()
	{
		CabinetManager.Calibration.Data.UpperLeft = m_calibratedPoints[0];
		CabinetManager.Calibration.Data.LowerRight = m_calibratedPoints[1];
		CabinetManager.Calibration.Data.IsCalibrated = true;
		CabinetManager.Calibration.Save();
		m_orgCalibratedPoints[0] = CabinetManager.Calibration.Data.UpperLeft;
		m_orgCalibratedPoints[1] = CabinetManager.Calibration.Data.LowerRight;
		Calibrate(CabinetManager.Calibration.Data.UpperLeft, CabinetManager.Calibration.Data.LowerRight);
	}

	private void Calibrate(Vector2 upperLeft, Vector2 lowerRight)
	{
		Bi2xInput.Instance.TouchCalibrate(m_calibratteTargetPoints[0], m_calibratteTargetPoints[1], upperLeft, lowerRight);
	}

	protected override void OnBeforeExit()
	{
		Calibrate(CabinetManager.Calibration.Data.UpperLeft, CabinetManager.Calibration.Data.LowerRight);
	}
}
