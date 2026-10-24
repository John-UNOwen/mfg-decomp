using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CautionSet : MonoBehaviour
{
	[SerializeField]
	private GameObject _window;

	[SerializeField]
	private GameObject _bg;

	[SerializeField]
	private GameObject _batten;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public async UniTask Play()
	{
		await _bg.GetComponent<Image>().DOFade(0f, 0f);
		await _batten.GetComponent<Image>().DOFade(0f, 0f);
		base.gameObject.SetActive(value: true);
		_bg.SetActive(value: true);
		_batten.SetActive(value: true);
		UniTask uniTask = _bg.GetComponent<Image>().DOFade(1f, 0.5f).ToUniTask();
		UniTask uniTask2 = _batten.GetComponent<Image>().DOFade(1f, 0.5f).ToUniTask();
		await UniTask.WhenAll(uniTask, uniTask2);
		await UniTask.Delay(TimeSpan.FromSeconds(0.5));
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(1.5)), UniTask.WaitUntil(() => ResidentFlow.CustomInput.GetMouseButtonDown(0)));
	}

	public async UniTask Close()
	{
		base.gameObject.SetActive(value: false);
		_bg.SetActive(value: false);
		_batten.SetActive(value: false);
	}
}
