using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MFG.MasterGameData;
using MFG.Scenes.Home;
using MFG.Types;
using MFG.Types.PlayRuleMaster;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class PlayRuleWindow : MonoBehaviour
{
	private PlayRuleType playRuleType;

	[SerializeField]
	private GameObject windowObj;

	[SerializeField]
	private GameObject ruleObj;

	[SerializeField]
	private List<PlayRuleYakuFrame> yakus;

	[SerializeField]
	private TextMeshProUGUI ruleText;

	[SerializeField]
	private List<Sprite> enablePais;

	[SerializeField]
	private Image paiImage;

	[SerializeField]
	private List<Sprite> redDoras;

	[SerializeField]
	private Image redImage;

	[SerializeField]
	private List<GameObject> nextPais;

	[SerializeField]
	private Button ruleButton;

	[SerializeField]
	private Button yakuButton;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private List<GameObject> layoutObjects;

	[SerializeField]
	private RectTransform yakuObj;

	[SerializeField]
	private VerticalLayoutGroup contentsObj;

	[SerializeField]
	private GameObject baseObj;

	[SerializeField]
	private Button ruleExButton;

	[SerializeField]
	private Button ruleExCloseButton;

	[SerializeField]
	private GameObject ruleExWindow;

	private MahjongPlayFlow m_mahjongPlayFlow;

	private bool isRuleOpen;

	private void Start()
	{
		closeButton.OnClickAsObservable().Subscribe(delegate
		{
			Close().Forget();
		}).AddTo(this);
		ruleButton.OnClickAsObservable().Subscribe(async delegate
		{
			if (!isRuleOpen)
			{
				isRuleOpen = !isRuleOpen;
				ruleObj.SetActive(value: true);
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
				ruleButton.GetComponent<Animator>().SetTrigger("Selected");
				yakuButton.GetComponent<Animator>().SetTrigger("Normal");
			}
		}).AddTo(this);
		yakuButton.OnClickAsObservable().Subscribe(async delegate
		{
			if (isRuleOpen)
			{
				isRuleOpen = !isRuleOpen;
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
				ruleObj.SetActive(value: false);
				ruleButton.GetComponent<Animator>().SetTrigger("Normal");
				yakuButton.GetComponent<Animator>().SetTrigger("Selected");
			}
		}).AddTo(this);
		ruleExButton.OnClickAsObservable().Subscribe(async delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			ruleExWindow.SetActive(value: true);
		}).AddTo(this);
		ruleExCloseButton.OnClickAsObservable().Subscribe(async delegate
		{
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			ruleExWindow.SetActive(value: false);
		}).AddTo(this);
	}

	public void Open(PlayRuleType type)
	{
		if (!isRuleOpen)
		{
			isRuleOpen = true;
			SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			ruleExButton.gameObject.SetActive(type == PlayRuleType.Aotenjo);
			base.gameObject.SetActive(value: true);
			InitializeWindow(type);
		}
	}

	public void Open(TakuType type, MFG.Types.EventType eventType, MahjongPlayFlow mahjongPlayFlow)
	{
		m_mahjongPlayFlow = mahjongPlayFlow;
		Open(PlayRuleSelector(type, eventType));
	}

	public PlayRuleType PlayRuleSelector(TakuType type, MFG.Types.EventType eventType)
	{
		return MahjongTypesUtility.GetPlayRuleType(type, eventType);
	}

	public async UniTask Close()
	{
		base.gameObject.SetActive(value: false);
		if (m_mahjongPlayFlow != null)
		{
			m_mahjongPlayFlow.m_isRuleDisplaying = false;
		}
		SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
		isRuleOpen = false;
	}

	public void InitializeWindow(PlayRuleType type)
	{
		ruleObj.SetActive(value: true);
		playRuleType = type;
		Item item = PlayRuleMaster.GetItem(type);
		Item item2 = PlayRuleMaster.GetItem(PlayRuleSelector(item.TakuType, MFG.Types.EventType.None));
		List<Han> list = new List<Han>
		{
			Han.Han1,
			Han.Han2,
			Han.Han3,
			Han.Haneman,
			Han.Yakuman,
			Han.Yakuman2
		};
		for (int i = 0; i < 6; i++)
		{
			yakus[i].Set(list[i], item, item2);
		}
		foreach (Transform item3 in ruleObj.transform)
		{
			foreach (Transform item4 in item3)
			{
				RuleUnit ruleUnit = (RuleUnit)Enum.Parse(typeof(RuleUnit), item4.name);
				RuleInit(ruleUnit, item4.gameObject);
				ChangeRule changeRule = item2.ChangeRules.Find((ChangeRule x) => x.Unit == ruleUnit);
				ChangeRule changeRule2 = item.ChangeRules.Find((ChangeRule x) => x.Unit == ruleUnit);
				if (changeRule2 != null)
				{
					RuleSetUp(ruleUnit, item4, changeRule2);
				}
				else if (changeRule != null)
				{
					RuleSetUp(ruleUnit, item4, changeRule);
				}
			}
		}
		ruleText.text = item.Name;
		foreach (GameObject layoutObject in layoutObjects)
		{
			PlayRuleUtility.RefreshLayout(layoutObject);
		}
		yakuObj.sizeDelta = new Vector2(1160f, ruleObj.transform.GetComponent<RectTransform>().rect.size.y);
		contentsObj.spacing = 0f - yakuObj.sizeDelta.y;
		PlayRuleUtility.RefreshLayout(contentsObj.gameObject);
		PlayRuleUtility.RefreshLayout(baseObj);
	}

	private void RuleSetUp(RuleUnit ruleUnit, Transform rule, ChangeRule changeRule)
	{
		if (changeRule == null)
		{
			return;
		}
		if (!string.IsNullOrEmpty(changeRule.ChangeText))
		{
			rule.Find("Text").GetComponent<TextMeshProUGUI>().text = changeRule.ChangeText;
		}
		if (changeRule.Height > -1f)
		{
			rule.GetComponent<RectTransform>().sizeDelta = new Vector2(520f, changeRule.Height);
		}
		switch (ruleUnit)
		{
		case RuleUnit.EnablePai:
			paiImage.sprite = enablePais[changeRule.ImageIndex];
			break;
		case RuleUnit.RedDora:
			if (changeRule.ImageIndex >= 0)
			{
				redImage.sprite = redDoras[changeRule.ImageIndex];
				break;
			}
			rule.transform.Find("Text").GetComponent<RectTransform>().anchoredPosition = new Vector2(470f, 0f);
			redImage.gameObject.SetActive(value: false);
			break;
		case RuleUnit.PaymentTsumo:
		case RuleUnit.FireReach:
		case RuleUnit.Mentanpin:
		case RuleUnit.AccelDora:
		case RuleUnit.Catharsis:
		case RuleUnit.PCK:
		case RuleUnit.Reversal:
		case RuleUnit.AllGreen:
		case RuleUnit.Atomic:
			rule.gameObject.SetActive(value: true);
			break;
		case RuleUnit.NextPai:
		{
			rule.gameObject.SetActive(value: true);
			for (int i = 0; i < nextPais.Count; i++)
			{
				nextPais[i].SetActive(i == changeRule.ImageIndex);
			}
			break;
		}
		}
	}

	private void RuleInit(RuleUnit ruleUnit, GameObject rule)
	{
		switch (ruleUnit)
		{
		case RuleUnit.EnablePai:
			paiImage.sprite = enablePais[0];
			break;
		case RuleUnit.RankScore:
		case RuleUnit.Pao:
			rule.GetComponent<RectTransform>().sizeDelta = new Vector2(520f, 60f);
			break;
		case RuleUnit.RedDora:
			redImage.sprite = redDoras[0];
			redImage.gameObject.SetActive(value: true);
			rule.GetComponent<RectTransform>().sizeDelta = new Vector2(520f, 115f);
			rule.transform.Find("Text").GetComponent<RectTransform>().anchoredPosition = new Vector2(83.8f, 0f);
			break;
		case RuleUnit.PaymentTsumo:
		case RuleUnit.NextPai:
		case RuleUnit.FireReach:
		case RuleUnit.Mentanpin:
		case RuleUnit.AccelDora:
		case RuleUnit.Catharsis:
		case RuleUnit.PCK:
		case RuleUnit.Reversal:
		case RuleUnit.Bomber:
		case RuleUnit.AllGreen:
		case RuleUnit.Atomic:
			rule.SetActive(value: false);
			break;
		}
		ChangeRule changeRule = PlayRuleMaster.DefaultRules.Find((ChangeRule x) => x.Unit == ruleUnit);
		if (changeRule != null)
		{
			rule.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = changeRule.ChangeText ?? "";
		}
	}
}
