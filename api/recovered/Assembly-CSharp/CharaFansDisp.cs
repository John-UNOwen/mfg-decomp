using MFG.Assets;
using MFG.Types;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class CharaFansDisp : MonoBehaviour
{
	private CharaType chara;

	private FanType fan;

	[SerializeField]
	private TextMeshProUGUI fansText;

	[SerializeField]
	private Button up1Button;

	[SerializeField]
	private Button down1Button;

	[SerializeField]
	private Button up10Button;

	[SerializeField]
	private Button down10Button;

	[SerializeField]
	private Button up100Button;

	[SerializeField]
	private Button down100Button;

	[SerializeField]
	private Button up1000Button;

	[SerializeField]
	private Button down1000Button;

	[SerializeField]
	private Button up10000Button;

	[SerializeField]
	private Button down10000Button;

	[SerializeField]
	private Button up100000Button;

	[SerializeField]
	private Button down100000Button;

	private void Start()
	{
		up1Button.onClick.AddListener(delegate
		{
			ChangeFans(1, fan);
		});
		down1Button.onClick.AddListener(delegate
		{
			ChangeFans(-1, fan);
		});
		up10Button.onClick.AddListener(delegate
		{
			ChangeFans(10, fan);
		});
		down10Button.onClick.AddListener(delegate
		{
			ChangeFans(-10, fan);
		});
		up100Button.onClick.AddListener(delegate
		{
			ChangeFans(100, fan);
		});
		down100Button.onClick.AddListener(delegate
		{
			ChangeFans(-100, fan);
		});
		up1000Button.onClick.AddListener(delegate
		{
			ChangeFans(1000, fan);
		});
		down1000Button.onClick.AddListener(delegate
		{
			ChangeFans(-1000, fan);
		});
		up10000Button.onClick.AddListener(delegate
		{
			ChangeFans(10000, fan);
		});
		down10000Button.onClick.AddListener(delegate
		{
			ChangeFans(-10000, fan);
		});
		up100000Button.onClick.AddListener(delegate
		{
			ChangeFans(100000, fan);
		});
		down100000Button.onClick.AddListener(delegate
		{
			ChangeFans(-100000, fan);
		});
	}

	private void Update()
	{
	}

	private void ChangeFans(int change, FanType fantype)
	{
		GameDataManager.Player.CharaData[(int)chara].m_fans[(int)fantype] += change;
		if (GameDataManager.Player.CharaData[(int)chara].m_fans[(int)fantype] < 0)
		{
			GameDataManager.Player.CharaData[(int)chara].m_fans[(int)fantype] = 0;
		}
		DispFans();
	}

	public async void SetCharaType(CharaType charaType)
	{
		chara = charaType;
		IDisposableAsset<GameObject> obj = await AssetLoader.LoadAssetAsync<GameObject>(AssetPathResolver.Resolve(PathType.SdChara, chara));
		obj.AddTo(this);
		Object.Instantiate(obj.Asset, base.gameObject.transform).GetComponent<SdChara>().StandAnime();
		DispFans();
	}

	public async void SetFanType(FanType fanType)
	{
		fan = fanType;
		DispFans();
	}

	private void DispFans()
	{
		string text = "";
		switch (fan)
		{
		case FanType.Seiryu:
			text = "青龍";
			break;
		case FanType.Suzaku:
			text = "朱雀";
			break;
		case FanType.Byakko:
			text = "白虎";
			break;
		case FanType.Genbu:
			text = "玄武";
			break;
		}
		fansText.text = text + "：" + GameDataManager.Player.CharaData[(int)chara].m_fans[(int)fan];
	}
}
