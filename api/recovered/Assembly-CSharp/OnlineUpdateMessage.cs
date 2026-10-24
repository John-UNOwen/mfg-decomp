using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameContents;
using KAMUNITY;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OnlineUpdateMessage : SingletonBehaviour<OnlineUpdateMessage>
{
	public enum DispType
	{
		None,
		Simple,
		Detail
	}

	[SerializeField]
	private GameObject m_root;

	[SerializeField]
	private GameObject m_completeMessageForSimpeView;

	[SerializeField]
	private TextMeshProUGUI m_message;

	[SerializeField]
	private GameObject m_layoutRect;

	[SerializeField]
	private GameObject m_progressBarRoot;

	[SerializeField]
	private Image m_progrresBar;

	[SerializeField]
	private GameObject m_progressTextRoot;

	[SerializeField]
	private TextMeshProUGUI m_progressText;

	private AvsTypes.EA3_STATUS m_status = AvsTypes.EA3_STATUS.ONLINE;

	private bool m_bExist;

	private bool m_bFinish;

	private int m_progress;

	private bool m_isPcbEventSend;

	private DispType m_dispType;

	private bool _downloadProgressIsBusy;

	private bool _downloadProgressReportFinished;

	private DateTimeOffset _logLastSendTime;

	private void Update()
	{
		if (!ResidentFlow.IsInitialized || !EagCore.Instance.IsInitialized)
		{
			return;
		}
		if (GameUtility.IsKamunityBootEnable())
		{
			AvsPackage.AvsPackageGetStatus(out var is_exists, out var is_finish, out var progress);
			m_bExist = Convert.ToBoolean(is_exists);
			m_bFinish = Convert.ToBoolean(is_finish);
			if (m_progress != progress)
			{
				m_progress = progress;
			}
		}
		DownloadProgressLog_Update();
		UpdateDisp();
	}

	private void UpdateDisp()
	{
		if (!m_bExist)
		{
			SetActiveByDispType(DispType.None);
			return;
		}
		SetActiveByDispType(m_dispType);
		string text = "";
		if (m_bFinish)
		{
			text = TextManager.Get("ONLINEUPDATE_READY");
			m_completeMessageForSimpeView.transform.Find("Message/Text").GetComponent<TextMeshProUGUI>().text = text;
			if (!m_isPcbEventSend)
			{
				GameUtility.SendPcbEvent(PCB_EVENT.EVENT.ONLINEUPDATE_REBOOTWAIT);
				m_isPcbEventSend = true;
			}
		}
		else
		{
			float num = (float)m_progress / 10f;
			string arg = $"{num:0.0}";
			text = string.Format(TextManager.Get("ONLINEUPDATE_PROCESS_MSG"), arg);
		}
		m_message.text = text;
		m_progressText.text = text;
		switch (m_dispType)
		{
		case DispType.Detail:
			m_root.SetActive(value: true);
			m_progressBarRoot.SetActive(!m_bFinish);
			m_progrresBar.fillAmount = (float)m_progress / 1000f;
			{
				foreach (ContentSizeFitter item in m_root.GetComponentsInChildren<ContentSizeFitter>().Reverse())
				{
					LayoutRebuilder.ForceRebuildLayoutImmediate(item.GetComponent<RectTransform>());
				}
				break;
			}
		case DispType.Simple:
			m_progressTextRoot.SetActive(!m_bFinish);
			m_completeMessageForSimpeView.SetActive(m_bFinish);
			break;
		}
	}

	public void SetDispType(DispType dispType)
	{
		m_dispType = dispType;
	}

	private void SetActiveByDispType(DispType dispType)
	{
		switch (dispType)
		{
		case DispType.None:
			m_root.SetActive(value: false);
			m_completeMessageForSimpeView.SetActive(value: false);
			m_progressTextRoot.SetActive(value: false);
			break;
		case DispType.Simple:
			m_root.SetActive(value: false);
			m_completeMessageForSimpeView.SetActive(value: false);
			m_progressTextRoot.SetActive(value: true);
			break;
		case DispType.Detail:
			m_root.SetActive(value: true);
			m_completeMessageForSimpeView.SetActive(value: false);
			m_progressTextRoot.SetActive(value: false);
			break;
		}
	}

	public void DownloadProgressLog_Update()
	{
		if (m_bExist && !_downloadProgressReportFinished && !_downloadProgressIsBusy)
		{
			_downloadProgressIsBusy = true;
			bool flag = false;
			if (_logLastSendTime == default(DateTimeOffset))
			{
				flag = true;
			}
			else if (m_bFinish && !_downloadProgressReportFinished)
			{
				_downloadProgressReportFinished = true;
				flag = true;
			}
			else if (DateTimeOffsetManager.GetJSTTime() - _logLastSendTime >= new TimeSpan(0, 30, 0))
			{
				flag = true;
			}
			if (flag)
			{
				_logLastSendTime = DateTimeOffsetManager.GetJSTTime();
				GameUtility.ServerLog.Send("dlprogress", $"{m_progress}").Forget();
			}
			_downloadProgressIsBusy = false;
		}
	}
}
