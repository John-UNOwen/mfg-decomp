using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.Types.Competition;
using TMPro;
using UniRx;
using UnityEngine;

namespace MFG.GameData;

public static class CompetitionUtility
{
	public static async UniTask TitleBanner_Instantiate(GameObject parent, IReadOnlyList<CompetitionID> ignoreCompetitionIDs = null)
	{
		Dictionary<GameEventType, CompetitionID> dictionary = new Dictionary<GameEventType, CompetitionID>();
		List<CompetitionID> instantiatedIDs = new List<CompetitionID>();
		Regex regex = new Regex("^Competition(\\d+)");
		foreach (GameEventType value2 in Enum.GetValues(typeof(GameEventType)))
		{
			Match match = regex.Match(value2.ToString());
			if (match.Success)
			{
				int value = int.Parse(match.Groups[1].Value);
				dictionary.Add(value2, (CompetitionID)value);
			}
		}
		GameEventType competitionEventType2;
		foreach (KeyValuePair<GameEventType, CompetitionID> item in dictionary)
		{
			competitionEventType2 = item.Key;
			if (!GameDataManager.System.GameControlInfo.EventStatus.IsActive(competitionEventType2))
			{
				continue;
			}
			CompetitionID competitionID = item.Value;
			if (ignoreCompetitionIDs == null || !ignoreCompetitionIDs.Contains(competitionID))
			{
				IDisposableAsset<GameObject> obj = await AssetLoader.LoadAssetAsync<GameObject>(AssetPathResolver.ResolveCompetitionAssets(competitionID, CompetitionAssetType.TitleScreenBannerPrefab));
				obj.AddTo(parent);
				GameObject gameObject = UnityEngine.Object.Instantiate(obj.Asset, parent.transform, worldPositionStays: false);
				instantiatedIDs.Add(competitionID);
				GameEventData gameEventData = GameDataManager.System.GameControlInfo.EventStatus.GetGameEventData(competitionEventType2);
				string text = "";
				if (gameEventData != null)
				{
					text = gameEventData.ParamValue("Period", "");
				}
				gameObject.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = text;
			}
		}
		if (instantiatedIDs != null && instantiatedIDs.Count > 0)
		{
			return;
		}
		competitionEventType2 = GameDataManager.ShopCompetition.CurrentGameEventType;
		if (GameDataManager.System.GameControlInfo.EventStatus.IsActive(competitionEventType2))
		{
			IDisposableAsset<GameObject> obj2 = await AssetLoader.LoadAssetAsync<GameObject>($"ShopCompetition/{competitionEventType2}/BannerCompe.prefab");
			obj2.AddTo(parent);
			GameObject gameObject2 = UnityEngine.Object.Instantiate(obj2.Asset, parent.transform, worldPositionStays: false);
			GameEventData gameEventData2 = GameDataManager.System.GameControlInfo.EventStatus.GetGameEventData(competitionEventType2);
			string text2 = "";
			if (gameEventData2 != null)
			{
				text2 = gameEventData2.ParamValue("Period", "");
			}
			gameObject2.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = text2;
		}
	}
}
