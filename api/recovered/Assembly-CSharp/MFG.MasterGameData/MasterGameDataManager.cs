using System;
using Cysharp.Threading.Tasks;

namespace MFG.MasterGameData;

[Serializable]
public static class MasterGameDataManager
{
	public static CharacterMaster Character { get; private set; } = new CharacterMaster();


	public static async UniTask Load()
	{
		await Character.Load();
	}
}
