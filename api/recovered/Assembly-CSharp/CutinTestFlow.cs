using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.GameData;
using MFG.MahjongPlay.Cutin;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.CutinItem;
using UnityEngine;

public class CutinTestFlow : MonoBehaviour
{
	[SerializeField]
	private List<PreviewPlayerInteractor> m_previewPlayerInteractors;

	private bool _isPlaying;

	private CancellationTokenSource _cts = new CancellationTokenSource();

	[Header("アイテム設定")]
	public CharaType _charaType;

	public Category _category;

	public ItemID CurrentItemID { get; set; }

	private void Start()
	{
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Return))
		{
			Play();
		}
	}

	public async void Play()
	{
		if (_isPlaying)
		{
			return;
		}
		_isPlaying = true;
		_cts = new CancellationTokenSource();
		ItemID currentItemID = CurrentItemID;
		Item item = CutinItemMaster.GetItem(currentItemID);
		int nakiType = 0;
		bool isRon = true;
		CutinManager m_cutinManager = GameObject.Find("CutinManager").GetComponent<CutinManager>();
		Cha cha = Cha.Jicha;
		_ = m_previewPlayerInteractors[(int)cha];
		CutinType cutinType = CreateDebugCutinType(item.Category, nakiType, isRon);
		try
		{
			m_cutinManager.Play(cutinType, new CutinItemInfo
			{
				ItemID = currentItemID,
				IsOn = true
			}, cha, m_previewPlayerInteractors, null, _cts.Token).Forget();
			if (item.Category == Category.AgariSmall || item.Category == Category.AgariBig)
			{
				await m_cutinManager.AgariCutin_WaitIntroductionEnd(_cts.Token);
				m_cutinManager.CurrentCutin_End(_cts.Token).Forget();
			}
			else
			{
				_ = item.Category;
				_ = 2;
			}
			await UniTask.WaitWhile(() => m_cutinManager.IsPlaying, PlayerLoopTiming.Update, _cts.Token);
		}
		catch (OperationCanceledException)
		{
		}
		_isPlaying = false;
		static CutinType CreateDebugCutinType(Category _category, int _nakiType, bool _isRon)
		{
			CutinType result = CutinType.Pon;
			switch (_category)
			{
			case Category.Naki:
				switch (_nakiType)
				{
				case 0:
					result = CutinType.Pon;
					break;
				case 1:
					result = CutinType.Chi;
					break;
				case 2:
					result = CutinType.Kan;
					break;
				}
				break;
			case Category.AgariSmall:
			case Category.AgariBig:
				result = ((!_isRon) ? CutinType.Tsumo : CutinType.Ron);
				break;
			case Category.Reach:
				result = CutinType.Reach;
				break;
			}
			return result;
		}
	}
}
