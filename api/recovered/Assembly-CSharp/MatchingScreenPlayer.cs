using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.MasterGameData;
using MFG.Taikyoku;
using MFG.Types;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchingScreenPlayer : MonoBehaviour
{
	[SerializeField]
	private Image m_color;

	[SerializeField]
	private Image m_flame;

	[SerializeField]
	private TextMeshProUGUI m_name;

	[SerializeField]
	private Transform matchingBGParent;

	[SerializeField]
	private CatchphrasePlate m_catchphrasePlate;

	[SerializeField]
	private Image m_trophy;

	[SerializeField]
	private Transform m_dan;

	public GameObject MatchingScreenPlayerSetup(PlayerInfo info, CharaType charaType, DanBanner danBanner, GameObject matchingBG, Sprite trophy)
	{
		CharacterProperties characterProperties = MasterGameDataManager.Character.Properties(charaType);
		m_color.color = characterProperties.MatchingColor1;
		m_flame.color = characterProperties.MatchingColor2;
		m_name.text = info.GetPlayerName();
		m_catchphrasePlate.Reflesh(info.PlayerGameData.GetCurrentChara().CatchphraseItemEquip).Forget();
		if (trophy != null)
		{
			m_trophy.gameObject.SetActive(value: true);
			m_trophy.sprite = trophy;
		}
		else
		{
			m_trophy.gameObject.SetActive(value: false);
		}
		if (info.PlayerGameData.IsPro())
		{
			ProProfile proProfile = characterProperties.ProProfile;
			danBanner.SetDanBannerPro(m_dan, proProfile.proDanLevelType);
		}
		else
		{
			danBanner.SetDanBanner(m_dan, info.DanLevel);
		}
		GameObject gameObject = Object.Instantiate(matchingBG, matchingBGParent);
		if (gameObject.transform.GetComponent<MatchingBGController>() != null)
		{
			gameObject.transform.GetComponent<MatchingBGController>().ImageAdjuster();
		}
		return gameObject;
	}
}
