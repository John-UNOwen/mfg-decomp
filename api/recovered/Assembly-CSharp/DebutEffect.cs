using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.Costume;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class DebutEffect : MonoBehaviour
{
	public enum AssetType
	{
		Bright,
		Dark,
		CharaName
	}

	[SerializeField]
	private GameObject _parent2D;

	[SerializeField]
	private Image _charaName;

	[SerializeField]
	private StandingCharaManager _charaManager;

	[NonSerialized]
	private StandingCharaController _chara;

	private ItemID _itemID;

	public async UniTask Play(CharaType charaType, ItemID itemID, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (_itemID != 0)
		{
			_chara.CharaAnimator.speed = 1f;
			_chara.gameObject.SetActive(value: false);
			_charaManager.UnLoad(_itemID);
		}
		_itemID = itemID;
		_chara = await _charaManager.Load(itemID, _parent2D, cancellationToken);
		base.gameObject.SetActive(value: true);
		_parent2D.transform.localPosition = new Vector3(0f, MasterGameDataManager.Character.Properties(charaType).MyRoomDebutOffsetParam);
		if (CostumeItemMaster.GetItem(itemID).IsDebutItem)
		{
			IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.Resolve(PathType.CharaSelect, charaType, CharaSelectAssetType.DebutEffectCharaName), null, cancellationToken);
			disposableAsset.AddTo(this);
			_charaName.sprite = disposableAsset.Asset;
			base.gameObject.GetComponent<Animator>().SetTrigger("In");
			PlayMotion(cancellationToken).Forget();
			SoundManager.PlaySe("SFX_SYS_DEBUT1").Forget();
			SoundManager.PlayMyCharaVoice("VOX_GXX_DEBUT").Forget();
			await UniTask.Delay(TimeSpan.FromSeconds(1.0));
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(5.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResidentFlow.CustomInput.GetMouseButtonDown(0), PlayerLoopTiming.Update, cancellationToken));
			base.gameObject.GetComponent<Animator>().SetTrigger("End");
		}
		else
		{
			base.gameObject.transform.Find("All").Find("TextCostume").Find("CostumeName")
				.GetComponent<TextMeshProUGUI>()
				.text = "衣装 " + CostumeItemMaster.GetItem(itemID).Name.Replace(" ", "\n");
			base.gameObject.GetComponent<Animator>().SetTrigger("CostumeIn");
			PlayMotion(cancellationToken).Forget();
			SoundManager.PlaySe("SFX_SYS_DEBUT1").Forget();
			SoundManager.PlayMyCharaVoice("VOX_GXX_ITEM1").Forget();
			await UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
			await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(5.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResidentFlow.CustomInput.GetMouseButtonDown(0), PlayerLoopTiming.Update, cancellationToken));
			base.gameObject.GetComponent<Animator>().SetTrigger("CostumeEnd");
		}
	}

	public async UniTask PlayMotion(CancellationToken cancellationToken)
	{
		if (!(_chara != null))
		{
			return;
		}
		if (_chara.CharaType == CharaType.Chara14)
		{
			_chara.PlayMotion("Zakozako_LOOP");
			await UniTask.Delay(TimeSpan.FromSeconds(3.0));
		}
		else
		{
			_chara.PlayMotion("ResultPlus0");
			await UniTask.WaitUntil(() => _chara.CharaAnimator.GetCurrentAnimatorStateInfo(0).IsName("ResultPlus1"), PlayerLoopTiming.Update, cancellationToken);
		}
		_chara.CharaAnimator.speed = 0f;
	}
}
