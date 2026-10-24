using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Types.Customize.Costume;
using UnityEngine;

namespace MFG.StoryPlay;

public class CharaCommand : Command
{
	protected ItemID costumeItemID;

	protected GameObject obj2D;

	protected GameObject obj3D;

	public override async UniTask Parse(string[] values)
	{
		await base.Parse(values);
		costumeItemID = Utility.GetCostumeItemID(values[2], values[28]);
		if (!StoryPlayFlow.GetInstance().existCostumes.Exists((ItemID x) => x == costumeItemID))
		{
			StoryPlayFlow.GetInstance().existCostumes.Add(costumeItemID);
			await UniTask.WaitWhile(() => StoryPlayFlow.GetInstance().m_loadingCharacter);
			StoryPlayFlow.GetInstance().m_loadingCharacter = true;
			if (!values[28].Contains("RemovableIndex") || !int.TryParse(values[28].Replace("RemovableIndex", ""), out var result))
			{
				await StoryPlayFlow.GetInstance().charaManager.Load(costumeItemID);
			}
			else
			{
				StandingCharaManager charaManager = StoryPlayFlow.GetInstance().charaManager;
				ItemID itemID = costumeItemID;
				int removableIndex = result;
				await charaManager.Load(itemID, null, default(CancellationToken), removableIndex);
			}
			obj2D = StoryPlayFlow.GetInstance().GetChara2DObj(costumeItemID);
			obj3D = StoryPlayFlow.GetInstance().GetChara3DObj(costumeItemID);
			if (obj2D != null)
			{
				obj2D.SetActive(value: false);
			}
			if (obj3D != null)
			{
				obj3D.SetActive(value: false);
			}
			StoryPlayFlow.GetInstance().m_loadingCharacter = false;
		}
		else
		{
			obj2D = StoryPlayFlow.GetInstance().GetChara2DObj(costumeItemID);
			obj3D = StoryPlayFlow.GetInstance().GetChara3DObj(costumeItemID);
		}
	}

	public override async UniTask Exec()
	{
		await UniTask.WaitUntil(() => StoryPlayFlow.GetInstance().GetChara2DObj(costumeItemID) != null);
		obj2D = StoryPlayFlow.GetInstance().GetChara2DObj(costumeItemID);
		if (obj2D != null)
		{
			obj2D.SetActive(value: true);
		}
		obj3D = StoryPlayFlow.GetInstance().GetChara3DObj(costumeItemID);
		if (obj3D != null)
		{
			obj3D.SetActive(value: true);
			obj3D.transform.Find("Camera").gameObject.SetActive(value: true);
			obj3D.transform.Find("Parent").Find("Parent3D").gameObject.SetActive(value: true);
		}
	}

	public async UniTask Hide()
	{
		await UniTask.WaitUntil(() => Mathf.Abs(obj2D.transform.localPosition.x) > 1000f || Mathf.Abs(obj2D.transform.localPosition.y) > 1000f);
		obj3D.transform.Find("Camera").gameObject.SetActive(value: false);
		obj3D.transform.Find("Parent").Find("Parent3D").gameObject.SetActive(value: false);
	}
}
