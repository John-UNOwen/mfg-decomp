using MFG.Assets;
using MFG.Types;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class CharaExpDisp : MonoBehaviour
{
	private CharaType chara;

	[SerializeField]
	private TextMeshProUGUI expText;

	[SerializeField]
	private Button expUp1Button;

	[SerializeField]
	private Button expDown1Button;

	[SerializeField]
	private Button expUp10Button;

	[SerializeField]
	private Button expDown10Button;

	[SerializeField]
	private Button expUp100Button;

	[SerializeField]
	private Button expDown100Button;

	[SerializeField]
	private Button level10UpButton;

	[SerializeField]
	private Button level10DownButton;

	[SerializeField]
	private Button level100UpButton;

	[SerializeField]
	private Button level100DownButton;

	[SerializeField]
	private Button level1000UpButton;

	[SerializeField]
	private Button level1000DownButton;

	private void Start()
	{
		expUp1Button.onClick.AddListener(delegate
		{
			ChangeExp(1);
		});
		expDown1Button.onClick.AddListener(delegate
		{
			ChangeExp(-1);
		});
		expUp10Button.onClick.AddListener(delegate
		{
			ChangeExp(10);
		});
		expDown10Button.onClick.AddListener(delegate
		{
			ChangeExp(-10);
		});
		expUp100Button.onClick.AddListener(delegate
		{
			ChangeExp(100);
		});
		expDown100Button.onClick.AddListener(delegate
		{
			ChangeExp(-100);
		});
		level10UpButton.onClick.AddListener(delegate
		{
			ChangeLevel(10);
		});
		level10DownButton.onClick.AddListener(delegate
		{
			ChangeLevel(-10);
		});
		level100UpButton.onClick.AddListener(delegate
		{
			ChangeLevel(100);
		});
		level100DownButton.onClick.AddListener(delegate
		{
			ChangeLevel(-100);
		});
		level1000UpButton.onClick.AddListener(delegate
		{
			ChangeLevel(1000);
		});
		level1000DownButton.onClick.AddListener(delegate
		{
			ChangeLevel(-1000);
		});
	}

	private void Update()
	{
	}

	private void ChangeExp(int change)
	{
		GameDataManager.Player.CharaData[(int)chara].Exp += change;
		if (GameDataManager.Player.CharaData[(int)chara].Exp < 0)
		{
			GameDataManager.Player.CharaData[(int)chara].Exp = 0;
		}
		DispExp();
	}

	private void ChangeLevel(int change)
	{
		int num = GameDataManager.Player.CharaData[(int)chara].Level + change;
		if (num <= GameDataManager.Player.CharaData[(int)chara].MaxLevel && num >= 0)
		{
			GameDataManager.Player.CharaData[(int)chara].Exp = GameDataManager.Player.CharaData[(int)chara].Master.GetTotalExpRequired(num);
			DispExp();
		}
	}

	public async void SetCharaType(CharaType charaType)
	{
		chara = charaType;
		IDisposableAsset<GameObject> obj = await AssetLoader.LoadAssetAsync<GameObject>(AssetPathResolver.Resolve(PathType.SdChara, chara));
		obj.AddTo(this);
		Object.Instantiate(obj.Asset, base.gameObject.transform).GetComponent<SdChara>().StandAnime();
		DispExp();
	}

	private void DispExp()
	{
		expText.text = GameDataManager.Player.CharaData[(int)chara].Exp + "(好感度レベル：" + GameDataManager.Player.CharaData[(int)chara].Level + ")";
	}
}
