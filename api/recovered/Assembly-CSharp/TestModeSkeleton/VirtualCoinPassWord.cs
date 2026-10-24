using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace TestModeSkeleton;

public class VirtualCoinPassWord : MonoBehaviour
{
	[SerializeField]
	[Tooltip("パスワード本体")]
	private GameObject body;

	[SerializeField]
	[Tooltip("パスワード結果")]
	private List<Text> texts;

	private List<char> inputs;

	private int count;

	private ReadOnlyDictionary<TestModeInput.Key, string> tenkeyToString;

	public GameObject Body => body;

	public string VInput => string.Join("", inputs);

	private void Awake()
	{
		tenkeyToString = SingletonBehaviour<TestModeInput>.Instance.CreateTenkeyToStringDictionary();
		inputs = new List<char>(Enumerable.Repeat(' ', texts.Count));
		texts.ForEach(delegate(Text t)
		{
			t.text = "";
		});
		body.SetActive(value: false);
	}

	private void Update()
	{
		if (!body.activeSelf || IsFinished())
		{
			return;
		}
		if (SingletonBehaviour<TestModeInput>.Instance.IsTrigger(TestModeInput.Key.TenkeyClear))
		{
			inputs.ForEach(delegate(char c)
			{
				c = ' ';
			});
			texts.ForEach(delegate(Text t)
			{
				t.text = "";
			});
			count = 0;
			return;
		}
		foreach (KeyValuePair<TestModeInput.Key, string> item in tenkeyToString)
		{
			if (!SingletonBehaviour<TestModeInput>.Instance.IsTrigger(item.Key))
			{
				continue;
			}
			string value = item.Value;
			foreach (char value2 in value)
			{
				if (count < texts.Count)
				{
					inputs[count] = value2;
					texts[count].text = "*";
					count++;
				}
			}
		}
	}

	public void SetActive(bool isEnable)
	{
		body.SetActive(isEnable);
	}

	public bool IsFinished()
	{
		return count >= texts.Count;
	}

	public string InputWithHyphen()
	{
		return VInput.Insert(VInput.Length / 2, "-");
	}
}
