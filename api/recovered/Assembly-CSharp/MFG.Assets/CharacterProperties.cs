using System.Collections.Generic;
using UnityEngine;

namespace MFG.Assets;

[CreateAssetMenu(fileName = "newCharacterProperties", menuName = "MFGキャラクターProperties")]
public class CharacterProperties : ScriptableObject
{
	[SerializeField]
	[Header("キャラ")]
	public CharaProfile Profile = new CharaProfile();

	[SerializeField]
	[Header("プロ雀士")]
	public ProProfile ProProfile = new ProProfile();

	[SerializeField]
	[Header("曲名")]
	public string MusicName;

	[SerializeField]
	[Header("カットイン")]
	public Color NakiCutinBaseColor;

	[SerializeField]
	[Header("マッチング")]
	public Color MatchingColor1;

	public Color MatchingColor2;

	public Vector2 MatchingStampOffset;

	[SerializeField]
	[Header("対局中キャラクターカメラ高さ調整値")]
	public float MahjongHeightOffsetParam;

	[SerializeField]
	[Header("自家カメラオフセット値。y座標調整は原則↑で行うこと！というか基本的にこれを当てにしないで")]
	public OffsetParam JichaCameraOffsetParam = new OffsetParam();

	[SerializeField]
	[Header("他家立ち位置オフセット値。y座標調整は原則↑で行うこと！")]
	public OffsetParam[] CharaOffsetParam = new OffsetParam[4];

	[SerializeField]
	[Header("武装筐体足場の位置調整 CostumeNumberは「C009」ならば「009」と入力")]
	public List<TBSFloorAdjuster> TBSFloorAdjustList = new List<TBSFloorAdjuster>();

	[SerializeField]
	[Header("カメラ追従モーションリスト ※ステート名指定")]
	public List<string> CameraAdulationList = new List<string>();

	[SerializeField]
	[Header("アガリ時のモーションを指定")]
	public AgariMotion AgariMotion = new AgariMotion();

	[SerializeField]
	[Header("リザルト画面のモーションを指定")]
	public ResultMotion ScorePanelResultMotion = new ResultMotion();

	[SerializeField]
	[Header("流局時モーションを指定\u3000※ステート名で指定")]
	public string RyukyokuMotion = "Think0";

	[SerializeField]
	[Header("スコアパネル最終リザルトカメラサイズ")]
	public float ResultOrthographicSize = 0.28f;

	[SerializeField]
	[Header("衣装解禁オフセット値")]
	public float MyRoomDebutOffsetParam;

	[SerializeField]
	[Header("ライティング個別調整")]
	public Vector3 LightingOffsetParam = new Vector3(0f, 0f, 0f);

	[SerializeField]
	[Header("MahjongPlay用ライティング個別調整")]
	public Vector3 MahjongPlayLightingOffsetParam = new Vector3(0f, 0f, 0f);

	[SerializeField]
	[Header("Matching用カメラ開店時間")]
	public float MatchingCharaCameraRotateTime = 1.6f;

	[SerializeField]
	[Header("Matching用決めポーズフレーム数")]
	public int MatchingCharaKimePoseParam;

	[SerializeField]
	[Header("ガチャ解禁時の色")]
	public Color GachaGetDisplayColor;

	[SerializeField]
	[Header("漫符の位置")]
	public Vector3[] ManpuPosition = new Vector3[17];
}
