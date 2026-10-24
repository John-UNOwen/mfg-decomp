using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.Types;
using MFG.Types.Customize.CustomizeItem;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class GuideFlow : BaseFlow
{
	private enum Select
	{
		None,
		Choice1,
		Choice2
	}

	[SerializeField]
	private GameObject m_guide;

	[SerializeField]
	private Button m_ruleYesButton;

	[SerializeField]
	private Button m_ruleNoButton;

	[SerializeField]
	private Button m_pageNextButton;

	[SerializeField]
	private Button m_page3Hai1Button;

	[SerializeField]
	private Button m_page3Hai2Button;

	[SerializeField]
	private Button m_page3DecideButton;

	[SerializeField]
	private Button m_page4View1Button;

	[SerializeField]
	private Button m_page4View2Button;

	[SerializeField]
	private Button m_page4DecideButton;

	private Select m_decide;

	private bool m_haiTypeDecided;

	private bool m_viewTypeDecided;

	private ItemID m_tileSurfaceItemID = ItemID.TileSurface001;

	protected override async UniTask OnInitialize(CancellationToken cancellationToken = default(CancellationToken))
	{
		m_ruleYesButton.OnClickAsObservable().Subscribe(delegate
		{
			if (m_decide == Select.None)
			{
				m_decide = Select.Choice1;
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			}
		}).AddTo(this);
		m_ruleNoButton.OnClickAsObservable().Subscribe(delegate
		{
			if (m_decide == Select.None)
			{
				m_decide = Select.Choice2;
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			}
		}).AddTo(this);
		m_pageNextButton.OnClickAsObservable().Subscribe(delegate
		{
			if (m_decide == Select.None)
			{
				m_decide = Select.Choice1;
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			}
		}).AddTo(this);
		m_page3Hai1Button.OnClickAsObservable().Subscribe(delegate
		{
			if (!m_haiTypeDecided)
			{
				m_page3Hai1Button.GetComponent<Animator>().SetTrigger("Selected");
				m_page3Hai2Button.GetComponent<Animator>().SetTrigger("Normal");
				m_tileSurfaceItemID = ItemID.TileSurface001;
				SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			}
		}).AddTo(this);
		m_page3Hai2Button.OnClickAsObservable().Subscribe(delegate
		{
			if (!m_haiTypeDecided)
			{
				m_page3Hai1Button.GetComponent<Animator>().SetTrigger("Normal");
				m_page3Hai2Button.GetComponent<Animator>().SetTrigger("Selected");
				m_tileSurfaceItemID = ItemID.TileSurface002;
				SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			}
		}).AddTo(this);
		m_page3DecideButton.OnClickAsObservable().Subscribe(delegate
		{
			if (!m_haiTypeDecided)
			{
				m_haiTypeDecided = true;
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			}
		}).AddTo(this);
		m_page4View1Button.OnClickAsObservable().Subscribe(delegate
		{
			if (!m_viewTypeDecided)
			{
				m_page4View1Button.GetComponent<Animator>().SetTrigger("Selected");
				m_page4View2Button.GetComponent<Animator>().SetTrigger("Normal");
				GameDataManager.PlayerConfig.ViewType = ViewType.Type1;
				SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			}
		}).AddTo(this);
		m_page4View2Button.OnClickAsObservable().Subscribe(delegate
		{
			if (!m_viewTypeDecided)
			{
				m_page4View1Button.GetComponent<Animator>().SetTrigger("Normal");
				m_page4View2Button.GetComponent<Animator>().SetTrigger("Selected");
				GameDataManager.PlayerConfig.ViewType = ViewType.Type3;
				SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
			}
		}).AddTo(this);
		m_page4DecideButton.OnClickAsObservable().Subscribe(delegate
		{
			if (!m_viewTypeDecided)
			{
				m_viewTypeDecided = true;
				SoundManager.PlaySe("SFX_SYS_INPUT1").Forget();
			}
		}).AddTo(this);
		ResidentFlow.GetInstance().GetResidentBG().SetActive(value: true);
	}

	protected override async UniTask OnExecute(CancellationToken cancellationToken)
	{
		SoundManager.PlayMyCharaMyRoomBGM().Forget();
		ResidentFlow.GetInstance().GetTimer().SetTimer(30f, disp: true);
		ResidentFlow.GetInstance().GetTimer().StartTimer();
		m_guide.GetComponent<Animator>().SetTrigger("In");
		try
		{
			await UniTask.WhenAny(UniTask.WaitUntil(() => ResidentFlow.GetInstance().GetTimer().IsEnd()), UniTask.WaitWhile(() => m_decide == Select.None, PlayerLoopTiming.Update, cancellationToken));
			if (m_decide == Select.Choice1)
			{
				m_guide.GetComponent<Animator>().SetTrigger("SelectIn2");
				m_decide = Select.None;
			}
			else
			{
				m_guide.GetComponent<Animator>().SetTrigger("PageIn1");
				m_decide = Select.None;
				ResidentFlow.GetInstance().GetTimer().SetTimer(30f, disp: true);
				ResidentFlow.GetInstance().GetTimer().StartTimer();
				await UniTask.WhenAny(UniTask.WaitUntil(() => ResidentFlow.GetInstance().GetTimer().IsEnd()), UniTask.WaitWhile(() => m_decide == Select.None, PlayerLoopTiming.Update, cancellationToken));
				m_guide.GetComponent<Animator>().SetTrigger("PageIn2");
				await UniTask.Delay(TimeSpan.FromSeconds(1.0));
				m_decide = Select.None;
				ResidentFlow.GetInstance().GetTimer().SetTimer(30f, disp: true);
				ResidentFlow.GetInstance().GetTimer().StartTimer();
				await UniTask.WhenAny(UniTask.WaitUntil(() => ResidentFlow.GetInstance().GetTimer().IsEnd()), UniTask.WaitWhile(() => m_decide == Select.None, PlayerLoopTiming.Update, cancellationToken));
				m_guide.GetComponent<Animator>().SetTrigger("SelectIn");
				m_decide = Select.None;
			}
			ResidentFlow.GetInstance().GetTimer().SetTimer(30f, disp: true);
			ResidentFlow.GetInstance().GetTimer().StartTimer();
			await UniTask.WaitUntil(() => m_guide.transform.Find("Select").gameObject.activeSelf);
			m_tileSurfaceItemID = GameDataManager.Player.GetCurrentChara().CustomizeItemEquips[Category.TileSurface][0];
			if (m_tileSurfaceItemID == ItemID.TileSurface001)
			{
				m_page3Hai1Button.GetComponent<Animator>().SetTrigger("Selected");
			}
			else
			{
				m_page3Hai2Button.GetComponent<Animator>().SetTrigger("Selected");
			}
			await UniTask.WhenAny(UniTask.WaitUntil(() => ResidentFlow.GetInstance().GetTimer().IsEnd()), UniTask.WaitUntil(() => m_haiTypeDecided, PlayerLoopTiming.Update, cancellationToken));
			m_haiTypeDecided = true;
			if (m_tileSurfaceItemID == ItemID.TileSurface001)
			{
				m_page3Hai1Button.GetComponent<Animator>().SetTrigger("Highlighted");
				m_page3Hai2Button.GetComponent<Animator>().SetTrigger("Disabled");
			}
			else
			{
				m_page3Hai1Button.GetComponent<Animator>().SetTrigger("Disabled");
				m_page3Hai2Button.GetComponent<Animator>().SetTrigger("Highlighted");
			}
			foreach (CharaGameData charaDatum in GameDataManager.Player.CharaData)
			{
				List<ItemID> list = charaDatum.CustomizeItemEquips[Category.TileSurface];
				list.Clear();
				list.Add(m_tileSurfaceItemID);
			}
			m_guide.GetComponent<Animator>().SetTrigger("SelectOut");
			ResidentFlow.GetInstance().GetTimer().SetTimer(30f, disp: true);
			ResidentFlow.GetInstance().GetTimer().StartTimer();
			await UniTask.WaitUntil(() => m_guide.transform.Find("Select2").gameObject.activeSelf);
			m_page4View1Button.GetComponent<Animator>().SetTrigger("Selected");
			await UniTask.WhenAny(UniTask.WaitUntil(() => ResidentFlow.GetInstance().GetTimer().IsEnd()), UniTask.WaitUntil(() => m_viewTypeDecided, PlayerLoopTiming.Update, cancellationToken));
			m_viewTypeDecided = true;
			switch (GameDataManager.PlayerConfig.ViewType)
			{
			case ViewType.Type1:
				m_page4View1Button.GetComponent<Animator>().SetTrigger("Highlighted");
				m_page4View2Button.GetComponent<Animator>().SetTrigger("Disabled");
				break;
			case ViewType.Type3:
				m_page4View1Button.GetComponent<Animator>().SetTrigger("Disabled");
				m_page4View2Button.GetComponent<Animator>().SetTrigger("Highlighted");
				break;
			}
			await UniTask.Delay(TimeSpan.FromSeconds(1.0));
			m_guide.GetComponent<Animator>().SetTrigger("SelectViewOut");
		}
		catch
		{
		}
		GotoNextScene("Matching");
	}
}
