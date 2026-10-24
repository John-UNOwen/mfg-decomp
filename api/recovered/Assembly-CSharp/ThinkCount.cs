using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MFG.Types;
using TMPro;
using UnityEngine;

public class ThinkCount : MonoBehaviour
{
	public enum AttentionMessage
	{
		MessageActionSelect,
		MessageNakiSelect,
		MessageHaiSelect
	}

	[SerializeField]
	private float thinkLimitTime4ma = 5f;

	[SerializeField]
	private float thinkLimitTime2ma = 15f;

	[SerializeField]
	private float extraThinkTime = 20f;

	[SerializeField]
	private TextMeshProUGUI m_timeText;

	[SerializeField]
	private TextMeshProUGUI m_exTimeText;

	[SerializeField]
	private GameObject BarNormal;

	[SerializeField]
	private GameObject BarAdvance;

	[SerializeField]
	private GameObject m_MessageActionSelect;

	[SerializeField]
	private GameObject m_MessageNakiSelect;

	[SerializeField]
	private GameObject m_MessageHaiSelect;

	[SerializeField]
	private List<GameObject> tyoukous = new List<GameObject>();

	[SerializeField]
	private Camera m_camera;

	private bool m_isThinkingEnable;

	private float m_thinkTime;

	private float m_extraThinkTime;

	private bool m_isThinking;

	private bool m_isEnableAgari;

	private bool m_isThinkingTyoukou;

	private float m_thinkingTime;

	private Player m_player;

	public bool m_isThinkingTyoukouActionPlayed = true;

	public bool[] m_tyoukouPlayers = new bool[4];

	private int m_dispTime;

	private int m_extraDispTime;

	private void Start()
	{
	}

	private void Update()
	{
		if (!m_isThinkingEnable)
		{
			return;
		}
		if (m_isThinking)
		{
			if (!DebugFlag.IsActive(DebugFlag.Flag.PauseThinkCount))
			{
				m_thinkTime -= Time.deltaTime;
			}
			if (m_thinkTime < 0f)
			{
				m_thinkTime = 0f;
			}
			if (m_thinkTime <= 0f && m_extraThinkTime > 0f)
			{
				GetComponent<Animator>().SetTrigger("Advance");
				m_extraThinkTime -= Time.deltaTime;
				if (m_extraThinkTime < 0f)
				{
					m_extraThinkTime = 0f;
				}
			}
			SetTimeText();
		}
		if (m_isThinkingTyoukou)
		{
			m_thinkingTime += Time.deltaTime;
			if (m_thinkingTime > GetThinkLimitTime() && !tyoukous[(int)m_player.m_cha].activeSelf)
			{
				tyoukous[(int)m_player.m_cha].SetActive(value: true);
				m_player.character.m_charaReaction.ReactionTyoukou();
				SoundManager.PlaySe("SFX_MJG_INFO3").Forget();
				VoiceManager.PlayVoiceThink(m_player);
			}
		}
	}

	public void InitKyoku()
	{
		m_thinkTime = GetThinkLimitTime();
		m_extraThinkTime = extraThinkTime;
		for (int i = 0; i < 4; i++)
		{
			m_tyoukouPlayers[i] = false;
		}
		m_dispTime = (int)Math.Ceiling(m_thinkTime);
		m_extraDispTime = (int)Math.Ceiling(m_extraThinkTime);
	}

	public bool IsTimeUp()
	{
		if (m_isEnableAgari)
		{
			if (m_thinkTime <= 0f)
			{
				return true;
			}
		}
		else if (m_thinkTime <= 0f && m_extraThinkTime <= 0f)
		{
			return true;
		}
		return false;
	}

	public void ThinkStart(AttentionMessage message)
	{
		m_isThinking = true;
		m_thinkTime = GetThinkLimitTime();
		GetComponent<Animator>().SetTrigger("In");
		GetComponent<Animator>().ResetTrigger("Advance");
		GetComponent<Animator>().ResetTrigger("Exit");
		m_MessageActionSelect.SetActive(value: false);
		m_MessageNakiSelect.SetActive(value: false);
		m_MessageHaiSelect.SetActive(value: false);
		switch (message)
		{
		case AttentionMessage.MessageActionSelect:
			m_MessageActionSelect.SetActive(value: true);
			break;
		case AttentionMessage.MessageNakiSelect:
			m_MessageNakiSelect.SetActive(value: true);
			break;
		case AttentionMessage.MessageHaiSelect:
			m_MessageHaiSelect.SetActive(value: true);
			break;
		}
		SetTimeText();
	}

	public void ThinkEnd()
	{
		m_isThinking = false;
		m_isEnableAgari = false;
		GetComponent<Animator>().SetTrigger("Exit");
		ThinkingTyoukouEnd();
	}

	private void SetTimeText()
	{
		if (m_thinkTime > 0f)
		{
			if (m_dispTime != (int)Math.Ceiling(m_thinkTime))
			{
				m_dispTime = (int)Math.Ceiling(m_thinkTime);
				if (m_dispTime <= 5 && m_extraThinkTime <= 0f)
				{
					SoundManager.PlaySe("SFX_MJG_COUNT1").Forget();
				}
			}
		}
		else if (m_extraDispTime != (int)Math.Ceiling(m_extraThinkTime))
		{
			m_extraDispTime = (int)Math.Ceiling(m_extraThinkTime);
			if (m_extraDispTime <= 5)
			{
				SoundManager.PlaySe("SFX_MJG_COUNT1").Forget();
			}
		}
		m_timeText.text = m_dispTime.ToString("0");
		if (m_extraDispTime > 0)
		{
			m_exTimeText.gameObject.SetActive(value: true);
			if (m_thinkTime > 0f)
			{
				m_exTimeText.text = "+" + m_extraDispTime.ToString("0");
			}
			else
			{
				m_exTimeText.text = m_extraDispTime.ToString("0");
			}
		}
		else
		{
			m_exTimeText.gameObject.SetActive(value: false);
		}
		BarNormal.transform.localScale = new Vector3(1f - m_thinkTime / GetThinkLimitTime(), 1f, 1f);
		BarAdvance.transform.localScale = new Vector3(1f - m_extraThinkTime / extraThinkTime, 1f, 1f);
	}

	public void ThinkingTyoukouStart(Player player)
	{
		m_player = player;
		m_thinkingTime = 0f;
		m_isThinkingTyoukou = true;
		m_isThinkingEnable = true;
		m_isThinkingTyoukouActionPlayed = false;
		foreach (GameObject tyoukou in tyoukous)
		{
			tyoukou.SetActive(value: false);
		}
		if (m_tyoukouPlayers[(int)m_player.m_cha])
		{
			m_player.character.Action(Character.EventID.Tyoko3).Forget();
		}
		else
		{
			m_player.character.Action(Character.EventID.Tyoko1).Forget();
		}
	}

	public void ThinkingTyoukouEnd()
	{
		m_isThinkingTyoukou = false;
		m_isThinkingTyoukouActionPlayed = true;
		foreach (GameObject tyoukou in tyoukous)
		{
			tyoukou.SetActive(value: false);
		}
	}

	public float GetThinkLimitTime()
	{
		if (GameDataManager.Mahjong.TakuType == TakuType.Nima)
		{
			return thinkLimitTime2ma;
		}
		return thinkLimitTime4ma;
	}

	public void ThinkingDisable()
	{
		m_isThinkingEnable = false;
	}

	public void ThinkingEnable()
	{
		m_isThinkingEnable = true;
	}

	public float GetThinkingTime()
	{
		return m_thinkingTime;
	}

	public bool IsFast()
	{
		return m_thinkingTime < 1f;
	}

	public bool IsTyoukou()
	{
		return m_thinkingTime > GetThinkLimitTime();
	}

	public Player GetThinkingPlayer()
	{
		return m_player;
	}
}
