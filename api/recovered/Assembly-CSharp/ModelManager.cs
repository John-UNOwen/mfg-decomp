using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.Costume;
using UnityEngine;

public class ModelManager : MonoBehaviour
{
	[SerializeField]
	[Header("着脱指定アイテム群。\nデフォルトを0として設定して1以降に変異体での差分を指定すること")]
	private List<RemovableItems> RemovableItems = new List<RemovableItems>();

	private ItemID costumeID;

	private Animator animator;

	private List<string> adulationList;

	private CharaType chara => CostumeItemMaster.GetItem(costumeID).CharaType;

	public Transform GetCharaHead()
	{
		Transform transform = animator.gameObject.transform.Find("Hips/Spine/Spine1/Spine2/Neck/Head");
		if (transform != null)
		{
			return transform;
		}
		return animator.gameObject.transform.Find("Reference/Hips/Spine/Spine1/Spine2/Neck/Head");
	}

	public void Init(ItemID itemID, RuntimeAnimatorController controller, bool isMahjong, int removableIndex = 0)
	{
		costumeID = itemID;
		animator = base.transform.GetComponentInChildren<Animator>();
		animator.runtimeAnimatorController = controller;
		adulationList = MasterGameDataManager.Character.Properties(chara).CameraAdulationList;
		List<RemovableItems> removableItems = RemovableItems;
		if (removableItems == null || removableItems.Count <= 0)
		{
			return;
		}
		try
		{
			RemovableItems.Find((RemovableItems x) => x.Index == removableIndex).Set();
		}
		catch
		{
		}
	}

	public void PlayMotionMahjong(string triggerName, bool isReach, bool isReachAway, float awaySpeed, int lookDirection)
	{
		if (animator != null)
		{
			animator.Update(0f);
			animator.SetBool("IsReach", isReach);
			animator.SetBool("IsReachAway", isReachAway);
			animator.SetFloat("AwaySpeed", awaySpeed);
			animator.SetInteger("LookDirection", lookDirection);
			animator.SetTrigger(triggerName);
			animator.Update(0f);
		}
	}

	public AnimatorStateInfo GetCurrentAnimatorState()
	{
		return animator.GetCurrentAnimatorStateInfo(0);
	}

	public void PlayCrossFadeInFixedTimeInMahjong(string stateName, bool isReach, int lookDirection, int layerID, float transitionTimeSec = -1f)
	{
		if (animator != null)
		{
			animator.Update(0f);
			animator.SetBool("IsReach", isReach);
			animator.SetFloat("AwaySpeed", 1f);
			animator.SetInteger("LookDirection", lookDirection);
			if (transitionTimeSec > 0f)
			{
				animator.CrossFadeInFixedTime(stateName, transitionTimeSec, layerID);
			}
			else
			{
				animator.Play(stateName, layerID);
			}
			animator.Update(0f);
		}
	}

	public bool IsCharaAdulation()
	{
		return adulationList?.Exists((string x) => animator.GetCurrentAnimatorStateInfo(0).IsName(x)) ?? false;
	}

	public static async UniTask<RuntimeAnimatorController> MotionLoad(ItemID itemID, bool isMahjong)
	{
		return (await AssetLoader.LoadAssetAsync<RuntimeAnimatorController>(MotionPathResolver(itemID, isMahjong))).Asset;
	}

	private static string MotionPathResolver(ItemID itemID, bool isMahjong)
	{
		CharaType charaType = CostumeItemMaster.GetItem(itemID).CharaType;
		string text = string.Format("Standing3D/Motion/{0}/{1}", charaType, isMahjong ? "Game" : "Story");
		string text2 = string.Format("Standing3D/Motion/{0}/Humanoid_{1}", charaType, isMahjong ? "Game" : "Story");
		switch (charaType)
		{
		case CharaType.Chara01:
			switch (itemID)
			{
			case ItemID.Costume01_001:
			case ItemID.Costume01_002:
				return text + ".controller";
			case ItemID.Costume01_003:
			case ItemID.Costume01_004:
				return text + "_SwimWear.overrideController";
			case ItemID.Costume01_005:
			case ItemID.Costume01_009:
				return text + "_RaceQueen.overrideController";
			case ItemID.Costume01_006:
			case ItemID.Costume01_007:
			case ItemID.Costume01_008:
			case ItemID.Costume01_012:
				return text + "_Toho.overrideController";
			default:
				return text2 + ".overrideController";
			}
		case CharaType.Chara02:
			switch (itemID)
			{
			case ItemID.Costume02_001:
			case ItemID.Costume02_002:
				return text + ".controller";
			case ItemID.Costume02_003:
			case ItemID.Costume02_004:
			case ItemID.Costume02_005:
			case ItemID.Costume02_006:
			case ItemID.Costume02_007:
			case ItemID.Costume02_008:
			case ItemID.Costume02_009:
			case ItemID.Costume02_010:
			case ItemID.Costume02_011:
			case ItemID.Costume02_012:
				return text + "_SwimWear.overrideController";
			case ItemID.Costume02_013:
			case ItemID.Costume02_014:
				return text + "_Gal.overrideController";
			default:
				return text2 + ".overrideController";
			}
		case CharaType.Chara03:
			switch (itemID)
			{
			case ItemID.Costume03_001:
			case ItemID.Costume03_002:
				return text + ".controller";
			case ItemID.Costume03_003:
			case ItemID.Costume03_004:
			case ItemID.Costume03_005:
			case ItemID.Costume03_006:
				return text + "_SwimWear.overrideController";
			case ItemID.Costume03_007:
			case ItemID.Costume03_008:
			case ItemID.Costume03_009:
			case ItemID.Costume03_010:
				return text + "_newjoint_Sister.overrideController";
			default:
				return text2 + ".overrideController";
			}
		case CharaType.Chara04:
			switch (itemID)
			{
			case ItemID.Costume04_001:
			case ItemID.Costume04_002:
				return text + ".controller";
			case ItemID.Costume04_003:
			case ItemID.Costume04_004:
			case ItemID.Costume04_005:
			case ItemID.Costume04_006:
			case ItemID.Costume04_008:
				return text + "_SwimWear.overrideController";
			default:
				return text + "_Toho.overrideController";
			}
		case CharaType.Chara05:
			switch (itemID)
			{
			case ItemID.Costume05_001:
			case ItemID.Costume05_002:
				return text + ".controller";
			case ItemID.Costume05_003:
			case ItemID.Costume05_004:
			case ItemID.Costume05_005:
			case ItemID.Costume05_006:
			case ItemID.Costume05_007:
			case ItemID.Costume05_009:
				return text + "_SwimWear.overrideController";
			default:
				return text + "_Uniform.overrideController";
			}
		case CharaType.Chara06:
			switch (itemID)
			{
			case ItemID.Costume06_001:
			case ItemID.Costume06_002:
				return text + ".controller";
			case ItemID.Costume06_003:
			case ItemID.Costume06_004:
			case ItemID.Costume06_005:
			case ItemID.Costume06_006:
			case ItemID.Costume06_007:
			case ItemID.Costume06_008:
				return text + "_SwimWear.overrideController";
			default:
				return text + "_Darkness.overrideController";
			}
		case CharaType.Chara07:
			switch (itemID)
			{
			case ItemID.Costume07_001:
			case ItemID.Costume07_002:
				return text + ".controller";
			case ItemID.Costume07_003:
			case ItemID.Costume07_004:
			case ItemID.Costume07_005:
			case ItemID.Costume07_006:
			case ItemID.Costume07_007:
			case ItemID.Costume07_009:
			case ItemID.Costume07_010:
				return text + "_SwimWear.overrideController";
			default:
				return text + "_Yukata.overrideController";
			}
		case CharaType.Chara08:
			switch (itemID)
			{
			case ItemID.Costume08_001:
			case ItemID.Costume08_002:
				return text + ".controller";
			case ItemID.Costume08_003:
			case ItemID.Costume08_004:
			case ItemID.Costume08_005:
			case ItemID.Costume08_006:
				return text + "_SwimWear.overrideController";
			case ItemID.Costume08_007:
			case ItemID.Costume08_008:
			case ItemID.Costume08_009:
			case ItemID.Costume08_010:
				return text + "_Fundoshi.overrideController";
			default:
				return text2 + ".overrideController";
			}
		case CharaType.Chara09:
			switch (itemID)
			{
			case ItemID.Costume09_001:
			case ItemID.Costume09_002:
				return text + ".controller";
			case ItemID.Costume09_003:
			case ItemID.Costume09_004:
			case ItemID.Costume09_005:
			case ItemID.Costume09_006:
			case ItemID.Costume09_007:
			case ItemID.Costume09_008:
			case ItemID.Costume09_009:
				return text + "_SwimWear.overrideController";
			default:
				return text + "_Bomber.overrideController";
			}
		case CharaType.Chara10:
			return itemID switch
			{
				ItemID.Costume10_003 => text + "_DefaultMFG.overrideController", 
				ItemID.Costume10_004 => text + "_DressMFG.overrideController", 
				_ => text + ".controller", 
			};
		case CharaType.Chara11:
			switch (itemID)
			{
			case ItemID.Costume11_001:
			case ItemID.Costume11_002:
				return text + ".controller";
			case ItemID.Costume11_003:
			case ItemID.Costume11_004:
			case ItemID.Costume11_005:
			case ItemID.Costume11_006:
			case ItemID.Costume11_007:
				return text + "_SwimWear.overrideController";
			default:
				return text2 + ".overrideController";
			}
		case CharaType.Chara13:
			if ((uint)(itemID - 73) <= 3u || (uint)(itemID - 101) <= 1u)
			{
				return text + ".controller";
			}
			return text2 + ".controller";
		case CharaType.Chara14:
			if ((uint)(itemID - 91) <= 2u || (uint)(itemID - 120) <= 1u || itemID == ItemID.Costume14_008)
			{
				return text + ".controller";
			}
			return text2 + ".controller";
		case CharaType.Chara15:
			if ((uint)(itemID - 98) <= 2u || (uint)(itemID - 122) <= 1u)
			{
				return text + ".controller";
			}
			return text2 + ".overrideController";
		case CharaType.Chara16:
			if ((uint)(itemID - 113) <= 2u || (uint)(itemID - 126) <= 1u)
			{
				return text + ".controller";
			}
			return text2 + ".controller";
		case CharaType.Chara17:
			if ((uint)(itemID - 133) <= 2u)
			{
				return text + ".controller";
			}
			return text2 + ".controller";
		default:
			return text + ".controller";
		}
	}
}
