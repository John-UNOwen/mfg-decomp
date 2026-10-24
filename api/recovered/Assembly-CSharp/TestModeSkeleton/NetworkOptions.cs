using System;
using System.Collections.Generic;
using System.Linq;
using TestModeSkeleton.CabinetDef;
using UnityEngine;
using UnityEngine.UI;

namespace TestModeSkeleton;

public class NetworkOptions : TestModeBase
{
	[SerializeField]
	[Tooltip("シーン固有オブジェクト")]
	private GameObject[] uniqueObjects;

	[SerializeField]
	[Tooltip("e-amusement設定")]
	private Text eAmusementText;

	[SerializeField]
	[Tooltip("店舗地域")]
	private Text shopAreaText;

	[SerializeField]
	[Tooltip("店舗名")]
	private Text shopNameText;

	private Dictionary<string, TestModeValue> options = new Dictionary<string, TestModeValue>
	{
		["EAmusement"] = new TestModeValue(NetworkOptionsDef.EAmusementSetting, isLoop: true, TestModeValue.TextColorType.Default, TestModeValue.ToOffOnString),
		["ShopArea"] = new TestModeValue(NetworkOptionsDef.ShopAreaSetting, isLoop: true, TestModeValue.TextColorType.Default, NetworkOptionsDef.ToShopAreaString)
	};

	private string optionsShopName = NetworkOptionsDef.ShopNameDefault;

	protected override string TitleName => "<l>NETWORK OPTIONS</l>";

	private void Start()
	{
		options["EAmusement"].Set((int)CabinetManager.NetWork.Data.EAmusement, eAmusementText);
		options["ShopArea"].Set((int)CabinetManager.NetWork.Data.ShopArea, shopAreaText);
		optionsShopName = CabinetManager.NetWork.Data.ShopName;
		menuElements.FindAll((TestModeMenuElement e) => e.Event == "EAmusement").ForEach(delegate(TestModeMenuElement e)
		{
			e.State = TestModeMenuElement.TextState.UnSelctable;
		});
		StartOrResumeMainCoroutine();
		ArrowkeyGameObject.SetActive(value: true);
		ArrowkeyUp.SetActive(value: true);
		ArrowkeyDown.SetActive(value: true);
		ArrowkeyEnter.SetActive(value: true);
	}

	protected override void OnUpdate(TestModeMenuElement element)
	{
		if ((SingletonBehaviour<TestModeInput>.Instance.IsAddValue() || SingletonBehaviour<TestModeInput>.Instance.IsSubValue()) && options.ContainsKey(element.Event))
		{
			options[element.Event].Change(SingletonBehaviour<TestModeInput>.Instance.IsAddValue());
		}
		shopNameText.text = optionsShopName;
		shopNameText.color = Color.red;
		if (shopNameText.text == "")
		{
			shopNameText.text = NetworkOptionsDef.ShopNameForNotSet;
			shopNameText.color = Color.green;
		}
	}

	protected override void OnCall(TestModeMenuElement element)
	{
		Array.ForEach(uniqueObjects, delegate(GameObject o)
		{
			o.SetActive(value: false);
		});
	}

	protected override void OnReturn(TestModeMenuElement element)
	{
		Array.ForEach(uniqueObjects, delegate(GameObject o)
		{
			o.SetActive(value: true);
		});
		ArrowkeyUp.SetActive(value: true);
		ArrowkeyDown.SetActive(value: true);
		ArrowkeyEnter.SetActive(value: true);
	}

	protected override void OnChangeMenuOn(TestModeMenuElement element)
	{
		string @event = element.Event;
		if (!(@event == "ShopArea"))
		{
			if (@event == "ShopNameSetting")
			{
				help.Set(TestModeHelp.HelpType.SelectSetting);
				ArrowkeyLeft.SetActive(value: false);
				ArrowkeyRight.SetActive(value: false);
				ArrowkeyEnter.SetActive(value: true);
			}
			else
			{
				help.Set(TestModeHelp.HelpType.SelectExecute);
				ArrowkeyLeft.SetActive(value: false);
				ArrowkeyRight.SetActive(value: false);
				ArrowkeyEnter.SetActive(value: true);
			}
		}
		else
		{
			help.Set(TestModeHelp.HelpType.SelectChange);
			ArrowkeyLeft.SetActive(value: true);
			ArrowkeyRight.SetActive(value: true);
			ArrowkeyEnter.SetActive(value: false);
		}
		SingletonBehaviour<TestModeInput>.Instance.ResetArrowKey();
	}

	protected override void OnFactorySettings(TestModeMenuElement element)
	{
		options.Values.ToList().ForEach(delegate(TestModeValue v)
		{
			v.ToDefault();
		});
		optionsShopName = NetworkOptionsDef.ShopNameDefault;
	}

	protected override void OnSave()
	{
		CabinetManager.NetWork.Data.EAmusement = (NetworkOptionsDef.EAmusement)options["EAmusement"].Current;
		CabinetManager.NetWork.Data.ShopArea = (NetworkOptionsDef.ShopArea)options["ShopArea"].Current;
		CabinetManager.NetWork.Data.ShopName = optionsShopName;
		CabinetManager.NetWork.Save();
	}

	protected override bool IsModification()
	{
		bool flag = optionsShopName != CabinetManager.NetWork.Data.ShopName;
		return options.Values.Any((TestModeValue v) => v.IsModification) || flag;
	}

	public void SetShopName(string name)
	{
		optionsShopName = name;
	}

	public string GetShopName()
	{
		return optionsShopName;
	}
}
