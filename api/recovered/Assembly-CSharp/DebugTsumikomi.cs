using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using MFG.Types;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class DebugTsumikomi : MonoBehaviour
{
	public enum Place
	{
		Ton,
		Nan,
		Sha,
		Pe,
		Tsumo0,
		Tsumo1,
		Tsumo2,
		Tsumo3,
		Tsumo4,
		WanPai,
		PlaceMax
	}

	[SerializeField]
	private Button _returnButton;

	[SerializeField]
	private GameObject mainCamera;

	[SerializeField]
	private Camera m_3dCameraTsumikomi;

	[SerializeField]
	private GameObject m_3dTextureTsumikomi;

	[SerializeField]
	private GameObject m_YonmaTiles;

	[SerializeField]
	private Button _onOffButton;

	[SerializeField]
	private Button _chichaButton;

	[SerializeField]
	private Button _cpuTsumogiriButton;

	[SerializeField]
	private TMP_InputField _presetNameInput;

	[SerializeField]
	private Button _savePresetButton;

	[SerializeField]
	private Button _loadPresetButton;

	public static Tsumikomi tsumikomi = new Tsumikomi(new List<List<Pai>>());

	public static Preset tsumikomiPreset = new Preset();

	public List<Pai> TsumikomiPos = new List<Pai>();

	private int m_width = Screen.width;

	private int m_height = Screen.height;

	private bool isSelected;

	private TsumikomiTile tmp;

	private static string GetSavePath()
	{
		return Application.persistentDataPath + "/";
	}

	public static void Save()
	{
		Debug.Log("積み込み情報を筐体に保存");
		tsumikomi.YonmaTilesToPais();
		Save("Tsumikomi.json", JsonUtility.ToJson(tsumikomi));
	}

	public async UniTask SavePreset()
	{
		Debug.Log("プリセット情報を筐体に保存");
		tsumikomiPreset.tsumikomiInfo = tsumikomi.Clone();
		tsumikomiPreset.tsumikomiInfo.YonmaTilesToPais();
		tsumikomiPreset.name = _presetNameInput.text;
		Save("TsumikomiPreset.json", JsonUtility.ToJson(tsumikomiPreset));
	}

	private static void Save(string fileName, string jsonData)
	{
		using StreamWriter streamWriter = new StreamWriter(GetSavePath() + fileName, append: false);
		try
		{
			streamWriter.Write(jsonData);
		}
		catch (Exception message)
		{
			Debug.Log(message);
		}
	}

	public static void Load()
	{
		string text = Load("/Tsumikomi.json");
		if (text != null)
		{
			tsumikomi = JsonUtility.FromJson<Tsumikomi>(text);
			tsumikomi.YonmaTiles = new List<List<Pai>>();
			tsumikomi.PaisToYonmaTiles();
		}
	}

	public async UniTask LoadPreset()
	{
		if (Load("/TsumikomiPreset.json") != null)
		{
			LoadPresetName();
			tsumikomiPreset.tsumikomiInfo.YonmaTiles = new List<List<Pai>>();
			tsumikomiPreset.tsumikomiInfo.PaisToYonmaTiles();
			tsumikomi = tsumikomiPreset.tsumikomiInfo.Clone();
			YonmaSetTexture();
		}
	}

	public void LoadPresetName()
	{
		string text = Load("/TsumikomiPreset.json");
		if (text != null)
		{
			tsumikomiPreset = JsonUtility.FromJson<Preset>(text);
			_presetNameInput.text = tsumikomiPreset.name;
		}
	}

	private static string Load(string fileName)
	{
		try
		{
			using FileStream stream = new FileStream(GetSavePath() + fileName, FileMode.Open);
			using StreamReader streamReader = new StreamReader(stream);
			return streamReader.ReadToEnd();
		}
		catch (Exception message)
		{
			Debug.Log(message);
			return null;
		}
	}

	private async void Start()
	{
		await UniTask.WaitUntil(() => ResidentFlow.IsInitialized);
		try
		{
			this.GetCancellationTokenOnDestroy();
			_returnButton.OnClickAsObservable().Subscribe(async delegate
			{
				SoundManager.PlaySe("SFX_SYS_CANCEL1").Forget();
				await Close();
			}).AddTo(this);
			_onOffButton.OnClickAsObservable().Subscribe(async delegate
			{
				SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
				await OnOff();
			}).AddTo(this);
			_chichaButton.OnClickAsObservable().Subscribe(async delegate
			{
				SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
				await ChichaChange();
			}).AddTo(this);
			_cpuTsumogiriButton.OnClickAsObservable().Subscribe(async delegate
			{
				SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
				await CpuTsumogiriChange();
			}).AddTo(this);
			_savePresetButton.OnClickAsObservable().Subscribe(async delegate
			{
				SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
				await SavePreset();
			}).AddTo(this);
			_loadPresetButton.OnClickAsObservable().Subscribe(async delegate
			{
				SoundManager.PlaySe("SFX_SYS_INPUT2").Forget();
				await LoadPreset();
			}).AddTo(this);
		}
		catch (OperationCanceledException ex)
		{
			Debug.Log("OperationCanceledException Catched. " + ex.Message);
		}
	}

	public async UniTask Play()
	{
		Load();
		LoadPresetName();
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
		base.gameObject.SetActive(value: true);
		GetComponent<Animator>().SetTrigger("In");
		if (GameDataManager.Mahjong.TsumikomiID == 0)
		{
			_onOffButton.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = "オフ";
			_onOffButton.transform.Find("BtDisp").gameObject.GetComponent<Image>().color = new Color(0f, 0f, 0f, 1f);
		}
		else if (GameDataManager.Mahjong.TsumikomiID == 1)
		{
			_onOffButton.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = "オン";
			_onOffButton.transform.Find("BtDisp").gameObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
		}
		SetTexture();
	}

	public async UniTask Close()
	{
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
		GetComponent<Animator>().SetTrigger("Out");
		TsumikomiPos.Clear();
		foreach (List<Pai> yonmaTile in tsumikomi.YonmaTiles)
		{
			foreach (Pai item in yonmaTile)
			{
				TsumikomiPos.Add(item);
			}
		}
		Debug.Log(string.Join(",", TsumikomiPos));
		GameDataManager.Mahjong.CustomTsumikomi = TsumikomiPos;
		Save();
	}

	public async UniTask OnOff()
	{
		if (GameDataManager.Mahjong.TsumikomiID == 0)
		{
			GameDataManager.Mahjong.TsumikomiID = 1;
			_onOffButton.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = "オン";
			_onOffButton.transform.Find("BtDisp").GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
		}
		else if (GameDataManager.Mahjong.TsumikomiID == 1)
		{
			GameDataManager.Mahjong.TsumikomiID = 0;
			_onOffButton.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = "オフ";
			_onOffButton.transform.Find("BtDisp").GetComponent<Image>().color = new Color(0f, 0f, 0f, 1f);
		}
	}

	public async UniTask ChichaChange()
	{
		GameDataManager.Mahjong.TsumikomiChicha++;
		if (GameDataManager.Mahjong.TsumikomiChicha > Kaze.Pe)
		{
			GameDataManager.Mahjong.TsumikomiChicha = Kaze.Ton;
		}
		string[] array = new string[4] { "東家", "南家", "西家", "北家" };
		_chichaButton.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = array[(int)GameDataManager.Mahjong.TsumikomiChicha];
	}

	public async UniTask CpuTsumogiriChange()
	{
		if (GameDataManager.Mahjong.TsumikomiCpuTsumogiri)
		{
			GameDataManager.Mahjong.TsumikomiCpuTsumogiri = false;
			_cpuTsumogiriButton.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = "CPU思考";
		}
		else
		{
			GameDataManager.Mahjong.TsumikomiCpuTsumogiri = true;
			_cpuTsumogiriButton.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = "CPUツモギリ";
		}
	}

	public void SetTexture()
	{
		m_YonmaTiles.SetActive(value: true);
		YonmaSetTexture();
	}

	private void YonmaSetTexture()
	{
		if (tsumikomi.YonmaTiles.Count <= 0)
		{
			Debug.Log("YonmaTilesの初期設定");
			int num = 0;
			for (Place place = Place.Ton; place <= Place.WanPai; place++)
			{
				int num2 = ((place <= Place.Pe) ? 13 : 14);
				tsumikomi.YonmaTiles.Add(new List<Pai>());
				for (int i = 0; i < num2; i++)
				{
					Pai pai = (Pai)(1 + num / 4);
					if ((pai == Pai.M5 || pai == Pai.S5 || pai == Pai.P5) && num % 4 == 0)
					{
						pai += 64;
					}
					tsumikomi.YonmaTiles[(int)place].Add(pai);
					num = (((pai != Pai.M9 && pai != Pai.S9 && pai != Pai.P9) || num % 4 != 3) ? (num + 1) : (num + 5));
				}
			}
		}
		Debug.Log("YonmaTilesを基に表示");
		for (Place place2 = Place.Ton; place2 <= Place.WanPai; place2++)
		{
			int num3 = ((place2 <= Place.Pe) ? 13 : 14);
			Transform transform = m_YonmaTiles.transform.Find(Enum.GetName(typeof(Place), place2));
			for (int j = 0; j < num3; j++)
			{
				Pai pai2 = tsumikomi.YonmaTiles[(int)place2][j];
				Transform obj = transform.Find("TsumikomiTileRoot" + (j + 1));
				obj.gameObject.SetActive(value: true);
				TsumikomiTile component = obj.GetComponent<TsumikomiTile>();
				component.SetHaiTexture(pai2);
				component.pai = pai2;
				component.placeP = place2;
				component.placeQ = j;
			}
		}
	}

	public TsumikomiTile GetMouseOnTile()
	{
		return GetMouseOnTile(m_3dCameraTsumikomi, m_3dTextureTsumikomi);
	}

	private TsumikomiTile GetMouseOnTile(Camera camera, GameObject renderTexture)
	{
		float num = 0f;
		float num2 = 0f;
		num = ResidentFlow.CustomInput.mousePosition.x / (float)m_width * 1920f;
		num2 = ResidentFlow.CustomInput.mousePosition.y / (float)m_height * 1080f;
		Vector3 pos = new Vector3(num, num2, 0f);
		Ray ray = camera.ScreenPointToRay(pos);
		RaycastHit hitInfo = default(RaycastHit);
		if (Physics.Raycast(ray, out hitInfo))
		{
			return hitInfo.collider.gameObject.GetComponent<TsumikomiTile>();
		}
		return null;
	}

	public async UniTask TsumikomiHaiSelect()
	{
		TsumikomiTile mouseOnTile = GetMouseOnTile();
		if (!ResidentFlow.CustomInput.GetMouseButtonDown(0) || !(mouseOnTile != null))
		{
			return;
		}
		Debug.Log("catch tile : " + mouseOnTile.placeP.ToString() + "," + mouseOnTile.placeQ);
		if (isSelected)
		{
			if (mouseOnTile.pai != tmp.pai)
			{
				Pai pai = tmp.pai;
				tsumikomi.YonmaTiles[(int)tmp.placeP][tmp.placeQ] = mouseOnTile.pai;
				tsumikomi.YonmaTiles[(int)mouseOnTile.placeP][mouseOnTile.placeQ] = pai;
				tmp.pai = mouseOnTile.pai;
				mouseOnTile.pai = pai;
				tmp.SetHaiTexture(tmp.pai);
				mouseOnTile.SetHaiTexture(mouseOnTile.pai);
			}
			isSelected = false;
		}
		else
		{
			tmp = mouseOnTile;
			isSelected = true;
		}
	}

	private void Update()
	{
		if (ResidentFlow.IsInitialized)
		{
			TsumikomiHaiSelect().Forget();
		}
	}
}
