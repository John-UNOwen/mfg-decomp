using System.Collections.Generic;
using UnityEngine;

namespace MFG.Assets;

[CreateAssetMenu(fileName = "MahjongPlayCommonProperties", menuName = "MahjongPlayCommonProperties")]
public class MahjongPlayCommonProperties : ScriptableObject
{
	[Header("フレーム色 0:期待翻数 1:向聴数 2:待ち牌数 3:ドラ数 4:テンパイ")]
	[SerializeField]
	public List<Color> m_frameColors = new List<Color>();

	[SerializeField]
	public Color m_frameColorDefault;

	[Space]
	[SerializeField]
	[Header("カットイン")]
	public Color NakiCutinBaseColor;
}
