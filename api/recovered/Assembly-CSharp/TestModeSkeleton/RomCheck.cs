using System;
using System.Collections;
using System.Collections.Generic;
using KAMUNITY;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TestModeSkeleton;

public class RomCheck : TestModeBase
{
	private enum Status
	{
		Checking,
		Ok,
		Bad
	}

	private enum CheckStep
	{
		STEP_DRM,
		STEP_EXTDRM,
		STEP_OK,
		STEP_BAD
	}

	[SerializeField]
	[Tooltip("進行バー本体")]
	private Slider progressBarSlider;

	[SerializeField]
	[Tooltip("進行パーセンテージ")]
	private Text percentageText;

	[SerializeField]
	[Tooltip("進行バー画像")]
	private Image progressBarImage;

	[SerializeField]
	[Tooltip("結果表示")]
	private Text resultText;

	protected override string TitleName => "<l>ROM CHECK</l>";

	private IEnumerator Start()
	{
		ArrowkeyGameObject.SetActive(value: true);
		ArrowkeyEnter.SetActive(value: true);
		ArrowkeyUp.SetActive(value: false);
		ArrowkeyDown.SetActive(value: false);
		ArrowkeyLeft.SetActive(value: false);
		ArrowkeyRight.SetActive(value: false);
		SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
		Status status = Status.Checking;
		yield return CoCheck(delegate(Status s)
		{
			status = s;
		});
		if (status == Status.Checking)
		{
			yield return CoAbort();
		}
		Exit();
	}

	private IEnumerator CoCheck(UnityAction<Status> action)
	{
		help.Set(TestModeHelp.HelpType.Abort);
		Dictionary<Status, Tuple<Color, string>> statusInfo = new Dictionary<Status, Tuple<Color, string>>
		{
			[Status.Checking] = Tuple.Create(Color.gray, "---"),
			[Status.Ok] = Tuple.Create(Color.green, "<color=lime>OK</color>"),
			[Status.Bad] = Tuple.Create(Color.red, "<color=red>BAD</color>")
		};
		ulong drmTotalSize = 0uL;
		ulong drmProcSize = 0uL;
		ulong drmTotalFiles = 0uL;
		ulong drmProcFiles = 0uL;
		ulong extTotalSize = 0uL;
		ulong extProcSize = 0uL;
		ulong extTotalFiles = 0uL;
		ulong extProcFiles = 0uL;
		if (KamunityInstance.isActive)
		{
			ExtDrm.ReleaseInstance();
			ExtDrm.CreateInstance();
			extTotalSize = ExtDrm.GetTotalFileSize();
			extTotalFiles = ExtDrm.GetTotalFiles();
			ExtDrm.ReleaseInstance();
			Debug.Log("ExtDrmFs: extTotalFiles = " + extTotalFiles + " extTotalSize = " + extTotalSize);
		}
		Bootstrap.FcheckInit(1, Bootstrap.BT_FCHECK_MODE.FULL);
		CheckStep step = CheckStep.STEP_DRM;
		while (true)
		{
			Status status = Status.Checking;
			switch (step)
			{
			case CheckStep.STEP_DRM:
			{
				Bootstrap.bt_fcheck_stat st;
				switch (Bootstrap.FcheckMain(out st))
				{
				case Bootstrap.BT_FCHECK.RUNNING:
					status = Status.Checking;
					break;
				case Bootstrap.BT_FCHECK.ERROR_NONE:
					Debug.Log("DRM: drmTotalFiles = " + drmTotalFiles + " drmTotalSize = " + drmTotalSize);
					if (KamunityInstance.isActive)
					{
						ExtDrm.ReleaseInstance();
						ExtDrm.CreateInstance();
						ExtDrm.CheckStart(ExtDrm.CheckMode.Full);
						extTotalSize = ExtDrm.GetTotalFileSize();
						extTotalFiles = ExtDrm.GetTotalFiles();
					}
					status = Status.Checking;
					step = CheckStep.STEP_EXTDRM;
					break;
				default:
					Debug.Log("DRM: drmTotalFiles = " + drmTotalFiles + " drmTotalSize = " + drmTotalSize);
					if (drmTotalFiles == 0L)
					{
						if (KamunityInstance.isActive)
						{
							ExtDrm.ReleaseInstance();
							ExtDrm.CreateInstance();
							ExtDrm.CheckStart(ExtDrm.CheckMode.Full);
							extTotalSize = ExtDrm.GetTotalFileSize();
							extTotalFiles = ExtDrm.GetTotalFiles();
						}
						status = Status.Checking;
						step = CheckStep.STEP_EXTDRM;
					}
					else
					{
						status = Status.Bad;
						step = CheckStep.STEP_BAD;
					}
					break;
				}
				drmTotalSize = st.total_size;
				drmProcSize = st.checked_size;
				drmTotalFiles = st.total_files;
				drmProcFiles = st.checked_files;
				break;
			}
			case CheckStep.STEP_EXTDRM:
				if (KamunityInstance.isActive)
				{
					extProcSize = ExtDrm.GetProcessedFileSize();
					extProcFiles = ExtDrm.GetProcessedFiles();
					switch (ExtDrm.GetResult())
					{
					case ExtDrm.Result.InProgress:
						status = Status.Checking;
						break;
					case ExtDrm.Result.Ok:
						status = Status.Ok;
						step = CheckStep.STEP_OK;
						break;
					case ExtDrm.Result.Bad:
						if (extTotalFiles == 0L)
						{
							status = Status.Ok;
							step = CheckStep.STEP_OK;
						}
						else
						{
							status = Status.Bad;
							step = CheckStep.STEP_BAD;
						}
						break;
					default:
						step = CheckStep.STEP_BAD;
						break;
					case ExtDrm.Result.Idle:
					case ExtDrm.Result.Aborted:
						break;
					}
				}
				else
				{
					step = CheckStep.STEP_BAD;
				}
				break;
			case CheckStep.STEP_OK:
				status = Status.Ok;
				break;
			case CheckStep.STEP_BAD:
				status = Status.Bad;
				break;
			}
			action(status);
			ulong num = drmTotalSize + extTotalSize;
			ulong num2 = extProcSize + drmProcSize;
			ulong num3 = drmTotalFiles + extTotalFiles;
			ulong num4 = extProcFiles + drmProcFiles;
			float num5 = ((num == 0L) ? 0f : (1f * (float)num2 / (float)num));
			progressBarSlider.value = num5;
			percentageText.text = $"{num5:P}";
			progressBarImage.color = statusInfo[status].Item1;
			resultText.text = $"HD : {statusInfo[status].Item2}  ({num4} / {num3})";
			if (status != 0)
			{
				help.Set(TestModeHelp.HelpType.Exit);
			}
			if (SingletonBehaviour<TestModeInput>.Instance.IsMenuExecute())
			{
				break;
			}
			yield return null;
		}
		if (KamunityInstance.isActive)
		{
			ExtDrm.CheckAbort();
			ExtDrm.ReleaseInstance();
		}
		Bootstrap.FcheckFinish();
	}

	private IEnumerator CoAbort()
	{
		notice.StartAbort();
		float start = Time.time;
		yield return null;
		yield return new WaitUntil(() => Time.time - start >= 1f || SingletonBehaviour<TestModeInput>.Instance.IsMenuExecute());
		notice.Stop();
	}
}
