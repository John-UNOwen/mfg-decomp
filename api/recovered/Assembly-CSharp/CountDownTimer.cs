using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CountDownTimer : MonoBehaviour
{
	private enum STATE
	{
		IDLE,
		RUN,
		PAUSE,
		END
	}

	[SerializeField]
	private Image m_imageNum100;

	[SerializeField]
	private Image m_imageNum10;

	[SerializeField]
	private Image m_imageNum01;

	[SerializeField]
	private List<Sprite> m_numSprites = new List<Sprite>();

	[SerializeField]
	private float m_startTime = 20f;

	[SerializeField]
	private int m_alertTime = 5;

	private string m_alertLabel = "SFX_SYS_COUNT1";

	private float m_time = 20f;

	private STATE m_state;

	private int m_dispTime;

	private bool m_disp = true;

	private bool m_isAlertSeEnable = true;

	public int MaxTime = 99;

	public bool ZeroPaddingDisable;

	private Subject<Unit> _onEnd = new Subject<Unit>();

	public UnityAction OnPrevDestroy { private get; set; }

	public IObservable<Unit> OnEnd => _onEnd.AsObservable();

	private void Start()
	{
		if (GameUtility.IsEventMode())
		{
			SetDisp(disp: false);
		}
	}

	private void OnDestroy()
	{
		OnPrevDestroy?.Invoke();
	}

	public int GetDispTime()
	{
		return m_dispTime;
	}

	public void DispTime()
	{
		int num = (int)Math.Ceiling(m_time);
		if (num > MaxTime)
		{
			num = MaxTime;
		}
		if (m_dispTime == num)
		{
			return;
		}
		if (m_time < 0f)
		{
			if (m_isAlertSeEnable)
			{
				SoundManager.PlaySe(m_alertLabel).Forget();
			}
			SetNumTexture(0);
			m_state = STATE.END;
			_onEnd.OnNext(Unit.Default);
		}
		else if (num < 1)
		{
			SetNumTexture(0);
		}
		else
		{
			SetNumTexture(num);
		}
		if (m_alertLabel != "" && m_state == STATE.RUN && num <= m_alertTime && m_isAlertSeEnable)
		{
			SoundManager.PlaySe(m_alertLabel).Forget();
		}
		m_dispTime = num;
	}

	public void SetTimer(float start_time, bool disp)
	{
		m_startTime = start_time;
		m_time = m_startTime;
		SetDisp(disp);
		if (disp)
		{
			DispTime();
		}
	}

	public void AddTimer(float add_time)
	{
		if (m_state == STATE.RUN && m_time > 0f)
		{
			m_time += add_time;
		}
	}

	public void SetDisp(bool disp)
	{
		m_disp = disp;
		m_isAlertSeEnable = disp;
		if (GameUtility.IsEventMode() && disp)
		{
			return;
		}
		foreach (Transform item in base.transform)
		{
			item.gameObject.SetActive(disp);
		}
	}

	public void AlertSeEnable(bool isEnable)
	{
		m_isAlertSeEnable = isEnable;
	}

	public void SetTimer(bool disp)
	{
		SetTimer(m_startTime, disp);
	}

	public void StartTimer(int time = -1)
	{
		m_time = ((time > 0) ? ((float)time) : m_startTime);
		m_state = STATE.RUN;
		if (m_time <= (float)m_alertTime && m_isAlertSeEnable)
		{
			SoundManager.PlaySe(m_alertLabel).Forget();
		}
	}

	public void ResetTimer(bool disp)
	{
		m_time = m_startTime;
		SetDisp(disp);
		m_state = STATE.IDLE;
	}

	public bool IsEnd()
	{
		return m_state == STATE.END;
	}

	public bool IsRunning()
	{
		return m_state == STATE.RUN;
	}

	public float GetLeftTime()
	{
		return m_time;
	}

	public bool IsPause()
	{
		return m_state == STATE.PAUSE;
	}

	public void SetPause(bool pause)
	{
		m_state = ((!pause) ? STATE.RUN : STATE.PAUSE);
	}

	public void Destroy()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void SetNumTexture(int num)
	{
		int index = num % 10;
		int num2 = num / 10 % 10;
		m_imageNum01.sprite = m_numSprites[index];
		m_imageNum10.enabled = !ZeroPaddingDisable || num >= 100 || num2 != 0;
		m_imageNum10.sprite = m_numSprites[num2];
		if (MaxTime >= 100)
		{
			int num3 = num / 100 % 10;
			if (m_imageNum100 != null)
			{
				m_imageNum100.enabled = !ZeroPaddingDisable || num >= 1000 || num3 != 0;
				m_imageNum100.sprite = m_numSprites[num3];
			}
		}
		else
		{
			m_imageNum100.enabled = false;
		}
	}

	private void Update()
	{
		if (GameUtility.IsEventMode())
		{
			return;
		}
		switch (m_state)
		{
		case STATE.RUN:
			DispTime();
			if (!GameDataManager.Player.IsPro() || !(SceneManager.GetActiveScene().name != "Matching"))
			{
				m_time -= Time.deltaTime;
			}
			break;
		case STATE.IDLE:
		case STATE.PAUSE:
		case STATE.END:
			break;
		}
	}
}
