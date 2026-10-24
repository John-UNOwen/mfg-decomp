using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MFG.Types;
using MFG.Types.Vote;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class VoteHomeWindow : MonoBehaviour
{
	[SerializeField]
	private Animator _windowAnimator;

	[SerializeField]
	private GameObject _rateWindowGameObject;

	[SerializeField]
	private GameObject _benefitWindowGameObject;

	[SerializeField]
	private Button _closeButton;

	[SerializeField]
	private Button _rateOpenButton;

	[SerializeField]
	private Button _rateCloseButton;

	[SerializeField]
	private Button _benefitOpenButton;

	[SerializeField]
	private Button _benefitCloseButton;

	[SerializeField]
	private TextMeshProUGUI _dateText;

	[SerializeField]
	private GameObject _jandriSupport;

	[SerializeField]
	private VoteWindow _voteWindow;

	[SerializeField]
	private Button _voteButton;

	[SerializeField]
	private TextMeshProUGUI _itemCount;

	[SerializeField]
	private List<TextMeshProUGUI> _tonpuRankBonusList;

	[SerializeField]
	private List<TextMeshProUGUI> _sanmaRankBonusList;

	[SerializeField]
	private List<TextMeshProUGUI> _nimaRankBonusList;

	public bool IsOpen;

	public bool IsRateOpen;

	public bool IsBenefitOpen;

	public void Start()
	{
		if (_closeButton != null)
		{
			_closeButton.OnClickAsObservable().Subscribe(async delegate
			{
				await Close();
			}).AddTo(this);
		}
		if (_rateOpenButton != null)
		{
			_rateOpenButton.OnClickAsObservable().Subscribe(delegate
			{
				RateWindow_Open();
			}).AddTo(this);
		}
		if (_rateCloseButton != null)
		{
			_rateCloseButton.OnClickAsObservable().Subscribe(delegate
			{
				RateWindow_Close();
			}).AddTo(this);
		}
		if (_benefitOpenButton != null)
		{
			_benefitOpenButton.OnClickAsObservable().Subscribe(delegate
			{
				BenefitWindow_Open();
			}).AddTo(this);
		}
		if (_benefitCloseButton != null)
		{
			_benefitCloseButton.OnClickAsObservable().Subscribe(delegate
			{
				BenefitWindow_Close();
			}).AddTo(this);
		}
		if (_dateText != null)
		{
			_dateText.text = GameDataManager.Vote.CurrentInfo.Master.DispDate();
		}
		if (_voteButton != null && _voteWindow != null)
		{
			_voteButton.OnClickAsObservable().Subscribe(delegate
			{
				_voteWindow.OpenWindow(this);
			}).AddTo(this);
		}
		if (_jandriSupport != null)
		{
			_jandriSupport.SetActive(GameDataManager.IsLoadJandriAppear());
		}
		try
		{
			Item master = GameDataManager.Vote.CurrentInfo.Master;
			int num = 0;
			foreach (TextMeshProUGUI tonpuRankBonus in _tonpuRankBonusList)
			{
				tonpuRankBonus.text = $"{master.GetRankBonus(TakuType.Tonpu, (Rank)num)}<size=15>票</size>";
				num++;
			}
			num = 0;
			foreach (TextMeshProUGUI sanmaRankBonus in _sanmaRankBonusList)
			{
				sanmaRankBonus.text = $"{master.GetRankBonus(TakuType.Sanma, (Rank)num)}<size=15>票</size>";
				num++;
			}
			num = 0;
			foreach (TextMeshProUGUI nimaRankBonus in _nimaRankBonusList)
			{
				nimaRankBonus.text = $"{master.GetRankBonus(TakuType.Nima, (Rank)num)}<size=15>票</size>";
				num++;
			}
		}
		catch
		{
		}
	}

	public void Open()
	{
		if (!IsOpen)
		{
			ItemCountRefresh();
			IsOpen = true;
			base.gameObject.SetActive(value: true);
			SoundManager.PlaySe("SFX_MJG_INFO1").Forget();
			if (_windowAnimator != null)
			{
				_windowAnimator.SetTrigger("In");
			}
		}
	}

	public void ItemCountRefresh()
	{
		if (_itemCount != null)
		{
			_itemCount.text = $"{GameDataManager.Vote.CurrentInfo.Point:N0}";
		}
	}

	public async UniTask Close()
	{
		if (IsOpen)
		{
			SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			RateWindow_Close(isPlaySe: false);
			BenefitWindow_Close(isPlaySe: false);
			if (_voteWindow != null)
			{
				_voteWindow.CloseWindow().Forget();
			}
			if (_windowAnimator != null)
			{
				_windowAnimator.SetTrigger("Out");
			}
			IsOpen = false;
			await UniTask.Delay(TimeSpan.FromSeconds(1.0));
		}
	}

	private void RateWindow_Open(bool isPlaySe = true)
	{
		if (!IsRateOpen)
		{
			IsRateOpen = true;
			if (isPlaySe)
			{
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			}
			_rateWindowGameObject.SetActive(value: true);
		}
	}

	private void RateWindow_Close(bool isPlaySe = true)
	{
		if (IsRateOpen)
		{
			if (isPlaySe)
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			}
			_rateWindowGameObject.SetActive(value: false);
			IsRateOpen = false;
		}
	}

	private void BenefitWindow_Open(bool isPlaySe = true)
	{
		if (!IsBenefitOpen)
		{
			IsBenefitOpen = true;
			if (isPlaySe)
			{
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			}
			_benefitWindowGameObject.SetActive(value: true);
		}
	}

	private void BenefitWindow_Close(bool isPlaySe = true)
	{
		if (IsBenefitOpen)
		{
			if (isPlaySe)
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL2").Forget();
			}
			_benefitWindowGameObject.SetActive(value: false);
			IsBenefitOpen = false;
		}
	}
}
