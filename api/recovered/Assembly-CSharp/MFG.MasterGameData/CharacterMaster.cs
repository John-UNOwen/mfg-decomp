using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.Types;
using UnityEngine.AddressableAssets;

namespace MFG.MasterGameData;

[Serializable]
public class CharacterMaster : MasterGameDataBase
{
	private List<CharacterProperties> m_characterProperties = new List<CharacterProperties>();

	private List<CharaReactionData> m_charaReactionDatas = new List<CharaReactionData>();

	private CharacterCommonProperties m_characterCommonProperties;

	public CharacterLevel CharacterLevel { get; private set; } = new CharacterLevel();


	public CharacterProperties Properties(CharaType charaType)
	{
		return m_characterProperties[(int)charaType];
	}

	public CharaReactionData ReactionDatas(CharaType charaType)
	{
		return m_charaReactionDatas[(int)charaType];
	}

	public CharacterCommonProperties CommonProperties()
	{
		return m_characterCommonProperties;
	}

	public async UniTask Load()
	{
		foreach (CharaType value in Enum.GetValues(typeof(CharaType)))
		{
			CharaReactionData charaReactionData = new CharaReactionData();
			charaReactionData.Load(value);
			m_charaReactionDatas.Add(charaReactionData);
			CharacterProperties item = await Addressables.LoadAssetAsync<CharacterProperties>(AssetPathResolver.Resolve(PathType.CharaProperties, value));
			m_characterProperties.Add(item);
		}
		m_characterCommonProperties = await Addressables.LoadAssetAsync<CharacterCommonProperties>("Assets/MFG/Characters/Common/properties/CharacterCommonProperties.asset");
	}
}
