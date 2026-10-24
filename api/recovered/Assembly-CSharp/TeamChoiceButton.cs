using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.Catchphrase;
using MFG.Types.ScrambleFestival;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class TeamChoiceButton : MonoBehaviour
{
	public TeamType m_teamType;

	[SerializeField]
	protected CatchphrasePlate Catchphrase;

	[SerializeField]
	protected Color BgColor;

	[SerializeField]
	protected Image BgImage;

	[SerializeField]
	protected Button BtChoise;

	private bool _isDecideOpen;

	public virtual void Reflesh(TeamChoiceWindow teamChoiceWindow, CatchphrasePlate homeCatchphrasePlate)
	{
		BgImage.color = BgColor;
		ScrambleFestivalEventInfo currentInfo = GameDataManager.ScrambleFestival.CurrentInfo;
		Catchphrase.Reflesh(currentInfo.Master.CatchPhrases[m_teamType].Normal).Forget();
		BtChoise.OnClickAsObservable().Subscribe(async delegate
		{
			BtChoise.interactable = false;
			await TeamDecide(teamChoiceWindow, homeCatchphrasePlate);
		}).AddTo(this);
	}

	private async UniTask TeamDecide(TeamChoiceWindow teamChoiceWindow, CatchphrasePlate homeCatchphrasePlate)
	{
		if (_isDecideOpen || GameDataManager.ScrambleFestival.CurrentInfo.Team != 0)
		{
			return;
		}
		_isDecideOpen = true;
		ScrambleFestivalEventInfo info = GameDataManager.ScrambleFestival.CurrentInfo;
		info.Team = m_teamType;
		teamChoiceWindow.m_infoWindow.ResetItem();
		ItemID item = info.Master.CatchPhrases[m_teamType].Normal;
		string catchphraseOID = CatchphraseItemMaster.ToObjectID(item);
		foreach (CharaGameData charaDatum in GameDataManager.Player.CharaData)
		{
			GameDataManager.ScrambleFestival.BeforeCatchphrases.Add(CatchphraseItemMaster.ToObjectID(charaDatum.CatchphraseItemEquip));
			charaDatum.CatchphraseItemEquip = item;
		}
		if (homeCatchphrasePlate != null)
		{
			homeCatchphrasePlate.Reflesh(item).Forget();
		}
		IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(ItemUtility.ItemIconGraphicPath(CatchphraseItemMaster.ToObjectID(item)));
		disposableAsset.AddTo(this);
		await teamChoiceWindow.m_infoWindow.AddItem(ItemUtility.ItemName(catchphraseOID), disposableAsset.Asset, ItemUtility.ItemName(catchphraseOID), catchphraseOID);
		if (info.Master.ChoiceBonusCount > 0)
		{
			ExtraPlayerItemMaster fesMedalMaster = info.Master.LotteryItem;
			ItemUtility.AddItem(fesMedalMaster.ObjectID, info.Master.ChoiceBonusCount);
			disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(fesMedalMaster.IconGraphicPath);
			disposableAsset.AddTo(this);
			teamChoiceWindow.m_infoWindow.AddItem($"{fesMedalMaster.Name} × {info.Master.ChoiceBonusCount}", disposableAsset.Asset, fesMedalMaster.Name, fesMedalMaster.ObjectID).Forget();
		}
		await teamChoiceWindow.m_infoWindow.Open();
		if (teamChoiceWindow.m_infoWindow.m_isOpen)
		{
			await teamChoiceWindow.m_infoWindow.Close();
		}
		await teamChoiceWindow.Close();
	}
}
