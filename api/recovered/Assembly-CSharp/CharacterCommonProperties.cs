using UnityEngine;

[CreateAssetMenu(fileName = "CharacterCommonProperties", menuName = "CharacterCommonProperties")]
public class CharacterCommonProperties : ScriptableObject
{
	[SerializeField]
	[Header("キャラクターのライティング共通設定")]
	public Vector3 CharacterLightingVector = new Vector3(80f, 180f, 0f);

	[SerializeField]
	[Header("対局中ライティング共通設定")]
	public Vector3 MahjongPlayLightingVector = new Vector3(-255f, 0f, 0f);
}
