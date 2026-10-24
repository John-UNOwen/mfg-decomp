using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

public class MahjongActorNoticeManager : MonoBehaviour, IDisposable
{
	[SerializeField]
	private MahjongActorNotice _lowerDisplayNotice;

	private MahjongActorNotice _upperDisplayNotice;

	private List<MahjongActorNotice.NoticeInfo> _noticeInfos = new List<MahjongActorNotice.NoticeInfo>();

	private IDisposable _reflesDisposable;

	private bool _isUpperDisp;

	private bool _isLowerDisp;

	private int _currentNoticeIndex = -1;

	private CancellationTokenSource _dispCts;

	public void Dispose()
	{
		_lowerDisplayNotice.Dispose();
		try
		{
			_upperDisplayNotice.Dispose();
		}
		catch
		{
		}
		RefleshTask_Dispose();
	}

	private void OnDestroy()
	{
		Dispose();
	}

	public void SetUpperParent(MahjongActorNotice notice)
	{
		_upperDisplayNotice = notice;
	}

	public void SetUpperDisp(bool isUpperDisp)
	{
		SetDisp(_isLowerDisp, isUpperDisp);
	}

	public void SetLowerDisp(bool isLowerDisp)
	{
		SetDisp(isLowerDisp, _isUpperDisp);
	}

	public void SetDisp(bool isLowerDisp, bool isUpperDisp)
	{
		bool flag = _isLowerDisp || _isUpperDisp;
		bool flag2 = isLowerDisp || isUpperDisp;
		if (!flag && flag2)
		{
			_currentNoticeIndex = -1;
			RefleshTask_Renew();
		}
		else if (flag && !flag2)
		{
			RefleshTask_Dispose();
		}
		if (_isLowerDisp != isLowerDisp)
		{
			_lowerDisplayNotice.SetDisp(isLowerDisp).Forget();
			_isLowerDisp = isLowerDisp;
		}
		if (SystemConfigGameData.IsDualMonitor && _isUpperDisp != isUpperDisp)
		{
			if (_upperDisplayNotice != null)
			{
				_upperDisplayNotice.SetDisp(isUpperDisp).Forget();
			}
			_isUpperDisp = isUpperDisp;
		}
	}

	public void Apply(ProStats proStats)
	{
		List<MahjongActorNotice.NoticeInfo> list = new List<MahjongActorNotice.NoticeInfo>();
		foreach (ProStats.ProStayData proStayData in proStats.ProStayDatas)
		{
			list.Add(new MahjongActorNotice.NoticeInfo
			{
				MahjongActorID = proStayData.MahjongActorID,
				IsStaying = true,
				LatestGameMode = proStayData.LatestGameMode
			});
		}
		SetNoticeInfos(list);
	}

	private void SetNoticeInfos(List<MahjongActorNotice.NoticeInfo> noticeInfos)
	{
		_noticeInfos = noticeInfos;
		_currentNoticeIndex = -1;
		RefleshTask_Renew();
	}

	private void Reflesh()
	{
		if ((!_isLowerDisp && !_isUpperDisp) || (_noticeInfos.Count == 1 && _currentNoticeIndex == 0))
		{
			return;
		}
		if (_noticeInfos.Count <= 0)
		{
			SetDisp(isLowerDisp: false, isUpperDisp: false);
			return;
		}
		if (_currentNoticeIndex < 0)
		{
			_currentNoticeIndex = 0;
		}
		else
		{
			_currentNoticeIndex++;
		}
		if (_currentNoticeIndex >= _noticeInfos.Count)
		{
			_currentNoticeIndex = 0;
		}
		if (_dispCts != null)
		{
			_dispCts.Cancel();
			_dispCts.Dispose();
			_dispCts = null;
		}
		_dispCts = new CancellationTokenSource();
		_lowerDisplayNotice.Reflesh(_noticeInfos[_currentNoticeIndex], _dispCts.Token).Forget();
		if (_upperDisplayNotice != null)
		{
			_upperDisplayNotice.Reflesh(_noticeInfos[_currentNoticeIndex], _dispCts.Token).Forget();
		}
	}

	private void RefleshTask_Dispose()
	{
		if (_reflesDisposable != null)
		{
			_reflesDisposable.Dispose();
			_reflesDisposable = null;
		}
		if (_lowerDisplayNotice != null)
		{
			_lowerDisplayNotice.gameObject.SetActive(value: false);
		}
		if (_upperDisplayNotice != null)
		{
			_upperDisplayNotice.gameObject.SetActive(value: false);
		}
	}

	private void RefleshTask_Renew()
	{
		RefleshTask_Dispose();
		if (_lowerDisplayNotice != null)
		{
			_lowerDisplayNotice.gameObject.SetActive(value: true);
		}
		if (_upperDisplayNotice != null)
		{
			_upperDisplayNotice.gameObject.SetActive(value: true);
		}
		_reflesDisposable = Observable.Timer(TimeSpan.FromSeconds(0.0), TimeSpan.FromSeconds(5.0)).Subscribe(delegate
		{
			Reflesh();
		}).AddTo(this);
	}
}
