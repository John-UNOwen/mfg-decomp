using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.CustomizeItem;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

public class SubMonitorScoreChara : MonoBehaviour
{
	[SerializeField]
	private RawImage rawImage;

	[SerializeField]
	private RectTransform plate;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private TextMeshProUGUI scoreText;

	[SerializeField]
	private Transform accessoryParent;

	[SerializeField]
	private Transform reachAuraParent;

	[SerializeField]
	private DanBanner danBanner;

	[SerializeField]
	private Transform danBannerParent;

	[SerializeField]
	private GameObject cpuIcon;

	[SerializeField]
	private CatchphrasePlate catchphrasePlate;

	[SerializeField]
	private TextMeshProUGUI jikaze;

	private GameObject reachAura;

	[HideInInspector]
	public Cha cha;

	private RenderTexture renderTexture;

	private Player player;

	private async void Start()
	{
		IDisposableAsset<GameObject> disposableAsset = await AssetLoader.LoadAssetAsync<GameObject>("MahjongPlayAura/Prefabs/CharaAura.prefab");
		disposableAsset.AddTo(this);
		reachAura = Object.Instantiate(disposableAsset.Asset, reachAuraParent);
		reachAura.GetComponent<Image>().material = null;
		reachAura.SetActive(value: false);
	}

	public void SetCameraEnable(bool isEnable)
	{
		try
		{
			player.character.m_subMonitorScoreCamera.enabled = isEnable;
		}
		catch
		{
		}
	}

	public void Init(Player p)
	{
		player = p;
		cha = player.m_cha;
		int playerNum = MahjongUtility.GetPlayerNum(GameDataManager.Mahjong.TakuType);
		nameText.text = player.m_name;
		int width = (SystemConfigGameData.IsSXGAMonitor ? 1200 : 1920) / playerNum;
		int height = (SystemConfigGameData.IsSXGAMonitor ? 1024 : 1080);
		renderTexture = new RenderTexture(width, height, GraphicsFormat.R8G8B8A8_UNorm, GraphicsFormat.D32_SFloat_S8_UInt)
		{
			antiAliasing = 4,
			filterMode = FilterMode.Bilinear
		};
		renderTexture.Create();
		rawImage.texture = renderTexture;
		player.character.m_subMonitorScoreCamera.targetTexture = renderTexture;
		if (player.m_isPro)
		{
			danBanner.SetDanBannerPro(danBannerParent, MasterGameDataManager.Character.Properties(player.m_charaType).ProProfile.proDanLevelType);
		}
		else
		{
			danBanner.SetDanBanner(danBannerParent, player.m_danLevel);
		}
		catchphrasePlate.Reflesh(player.CatchphraseItemEquip).Forget();
		cpuIcon.SetActive(player.m_isCPU);
		AccessoryInit(p.GetCustomizeItemEquip(Category.PlayerAccessory)).Forget();
		SetScore("", Color.white);
		jikaze.text = "";
		TextMeshProUGUI textMeshProUGUI = scoreText;
		float fontSizeMax = ((!SystemConfigGameData.IsSXGAMonitor || playerNum != 4) ? 51.9f : 33.85f);
		textMeshProUGUI.fontSizeMax = fontSizeMax;
	}

	public void ReachAuraSetActive(bool isActive, Color color)
	{
		reachAura.transform.GetComponent<Image>().color = color;
		reachAura.SetActive(isActive);
	}

	public async UniTask AccessoryInit(ItemID itemID)
	{
		foreach (Transform item in accessoryParent)
		{
			Object.Destroy(item.gameObject);
		}
		if (itemID != ItemID.PlayerAccessoryNone)
		{
			IDisposableAsset<GameObject> obj = await AssetLoader.LoadAssetAsync<GameObject>(CustomizeItemMaster.GetItem(itemID).MahjongPlayDataPath);
			obj.AddTo(this);
			Object.Instantiate(obj.Asset, accessoryParent);
		}
	}

	public async void SetScore(string score, Color color)
	{
		scoreText.text = score;
		scoreText.color = color;
	}

	public void SetKaze(Kaze kaze)
	{
		string[] array = new string[4] { "東", "南", "西", "北" };
		jikaze.text = array[(int)kaze];
		jikaze.color = ((kaze == Kaze.Ton) ? Color.yellow : Color.white);
	}
}
