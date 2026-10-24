using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.CustomizeItem;
using UnityEngine;
using UnityEngine.U2D;

internal class TileManager : MonoBehaviour
{
	private class TileMaterial
	{
		public Material normal;

		public Material gray;

		public Material dark;

		public Material blink;

		public void Init(Material material, Color grayColor, Color darkColor, bool isUnityChan = false)
		{
			SetMaterial(material);
			if (isUnityChan)
			{
				gray.SetColor("_BaseColor", new Color32(18, 13, 96, byte.MaxValue));
				dark.SetColor("_BaseColor", Color.black);
			}
			else
			{
				gray.color = grayColor;
				dark.color = darkColor;
			}
		}

		private void SetMaterial(Material material)
		{
			normal = new Material(material);
			gray = new Material(material);
			dark = new Material(material);
			blink = new Material(material);
		}

		public void SetTexture(Texture texture)
		{
			normal.mainTexture = texture;
			gray.mainTexture = texture;
			dark.mainTexture = texture;
			blink.mainTexture = texture;
		}
	}

	public enum MaterialType
	{
		Cube,
		CubeGray,
		CubeDark,
		CubeBlink,
		Hai,
		HaiGray,
		HaiDark,
		HaiBlink,
		Se,
		SeGray,
		SeDark,
		SeBlink,
		Kira,
		KiraGray,
		KiraDark,
		KiraBlink
	}

	[SerializeField]
	private Tile tilePrefab;

	[SerializeField]
	public Material cubeMaterial;

	[SerializeField]
	public Material haiMaterial;

	[SerializeField]
	public Material seMaterial;

	[SerializeField]
	public Material kiraMaterial;

	[NonSerialized]
	public ItemID paiItemID;

	[SerializeField]
	private Tile atomicTile;

	private TileMaterial m_cubeMaterial = new TileMaterial();

	private TileMaterial m_haiMaterial = new TileMaterial();

	private TileMaterial m_seMaterial = new TileMaterial();

	private TileMaterial m_kiraMaterial = new TileMaterial();

	[SerializeField]
	public Color grayColor;

	[SerializeField]
	public Color darkColor;

	[SerializeField]
	public float dahaiAnimeDelay;

	[SerializeField]
	public float dahaiAnimeTime = 0.3f;

	[SerializeField]
	public float dahaiAnimeDistance = 0.19f;

	[SerializeField]
	public float dahaiAnimeWait;

	[SerializeField]
	public bool dahaiAnimeIsWaitFinish = true;

	public List<Tile> Tiles = new List<Tile>();

	private const int TilePoolSize = 220;

	public HaiTextureType m_haiTextureType;

	private IDisposableAsset<Texture> m_texture;

	private IDisposableAsset<SpriteAtlas> m_spriteAtlas;

	private Dictionary<Pai, Sprite> TileSprites = new Dictionary<Pai, Sprite>();

	private static TileManager m_instance;

	private float time;

	public static TileManager GetInstance()
	{
		if (!m_instance)
		{
			m_instance = GameObject.Find("TileManager")?.GetComponent<TileManager>();
		}
		return m_instance;
	}

	private void Awake()
	{
		for (int i = 0; i < 220; i++)
		{
			Tile tile = UnityEngine.Object.Instantiate(tilePrefab, base.transform);
			tile.transform.gameObject.SetActive(value: false);
			Tiles.Add(tile);
		}
		m_cubeMaterial.Init(cubeMaterial, grayColor, darkColor);
		m_haiMaterial.Init(haiMaterial, grayColor, darkColor);
		m_seMaterial.Init(seMaterial, grayColor, darkColor);
		m_kiraMaterial.Init(kiraMaterial, grayColor, darkColor);
		TileSprites.Clear();
	}

	private void Update()
	{
		time += Time.deltaTime;
		float num = Mathf.Sin(time * 15f) * 0.25f + 0.75f;
		Color color = new Color(num, num, num, 1f);
		GetMaterial(MaterialType.CubeBlink).color = color;
		GetMaterial(MaterialType.HaiBlink).color = color;
	}

	private void OnDestroy()
	{
		foreach (Tile tile in Tiles)
		{
			tile.Destruct();
			UnityEngine.Object.Destroy(tile.gameObject);
		}
		Tiles.Clear();
		UnityEngine.Object.Destroy(m_cubeMaterial.normal);
		UnityEngine.Object.Destroy(m_cubeMaterial.gray);
		UnityEngine.Object.Destroy(m_cubeMaterial.dark);
		UnityEngine.Object.Destroy(m_cubeMaterial.blink);
		UnityEngine.Object.Destroy(m_haiMaterial.normal);
		UnityEngine.Object.Destroy(m_haiMaterial.gray);
		UnityEngine.Object.Destroy(m_haiMaterial.dark);
		UnityEngine.Object.Destroy(m_haiMaterial.blink);
		UnityEngine.Object.Destroy(m_seMaterial.normal);
		UnityEngine.Object.Destroy(m_seMaterial.gray);
		UnityEngine.Object.Destroy(m_seMaterial.dark);
		UnityEngine.Object.Destroy(m_seMaterial.blink);
		UnityEngine.Object.Destroy(m_kiraMaterial.normal);
		UnityEngine.Object.Destroy(m_kiraMaterial.gray);
		UnityEngine.Object.Destroy(m_kiraMaterial.dark);
		UnityEngine.Object.Destroy(m_kiraMaterial.blink);
		m_texture?.Dispose();
		m_spriteAtlas?.Dispose();
		foreach (KeyValuePair<Pai, Sprite> tileSprite in TileSprites)
		{
			UnityEngine.Object.Destroy(tileSprite.Value);
		}
		TileSprites.Clear();
	}

	public Tile GetTile(Transform parent, Pai pai)
	{
		if (Tiles.Count > 0)
		{
			Tile tile = Tiles[0];
			Tiles.Remove(tile);
			tile.gameObject.SetActive(value: true);
			tile.ResetParameter();
			tile.transform.SetParent(parent, worldPositionStays: false);
			tile.SetHaiTexture(pai);
			tile.pai = pai;
			return tile;
		}
		Debug.LogError("TileManager:牌オブジェクトが足りません");
		return null;
	}

	public void ReturnTile(Tile tile)
	{
		tile.transform.SetParent(base.transform, worldPositionStays: false);
		tile.transform.localPosition = tilePrefab.transform.localPosition;
		tile.transform.localRotation = tilePrefab.transform.localRotation;
		tile.transform.localScale = tilePrefab.transform.localScale;
		tile.ResetParameter();
		tile.gameObject.SetActive(value: false);
		Tiles.Add(tile);
	}

	public Tile GetTilePrefab()
	{
		return tilePrefab;
	}

	public Material GetMaterial(MaterialType type)
	{
		return type switch
		{
			MaterialType.Cube => m_cubeMaterial.normal, 
			MaterialType.CubeGray => m_cubeMaterial.gray, 
			MaterialType.CubeDark => m_cubeMaterial.dark, 
			MaterialType.CubeBlink => m_cubeMaterial.blink, 
			MaterialType.Hai => m_haiMaterial.normal, 
			MaterialType.HaiGray => m_haiMaterial.gray, 
			MaterialType.HaiDark => m_haiMaterial.dark, 
			MaterialType.HaiBlink => m_haiMaterial.blink, 
			MaterialType.Se => m_seMaterial.normal, 
			MaterialType.SeGray => m_seMaterial.gray, 
			MaterialType.SeDark => m_seMaterial.dark, 
			MaterialType.SeBlink => m_seMaterial.blink, 
			MaterialType.Kira => m_kiraMaterial.normal, 
			MaterialType.KiraGray => m_kiraMaterial.gray, 
			MaterialType.KiraDark => m_kiraMaterial.dark, 
			MaterialType.KiraBlink => m_kiraMaterial.blink, 
			_ => null, 
		};
	}

	public async UniTask Init(ItemID itemID, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		m_texture?.Dispose();
		paiItemID = itemID;
		string assetPath = CustomizeItemMaster.GetItem(paiItemID).MahjongPlayDataPath;
		m_texture = await AssetLoader.LoadAssetAsync<Texture>(assetPath, null, cancellationToken);
		m_haiMaterial.SetTexture(m_texture.Asset);
		assetPath = ItemIDExtentions.GetTileSeMaterialPath(paiItemID);
		bool flag = !string.IsNullOrEmpty(assetPath);
		if (flag)
		{
			flag = await AssetLoader.Exists(assetPath);
		}
		if (flag)
		{
			Debug.Log("牌背張り替え");
			IDisposableAsset<Material> disposableAsset = await AssetLoader.LoadAssetAsync<Material>(assetPath, null, cancellationToken);
			m_seMaterial.Init(disposableAsset.Asset, grayColor, darkColor);
		}
		assetPath = ItemIDExtentions.GetTileCubeMaterialPath(paiItemID);
		flag = !string.IsNullOrEmpty(assetPath);
		if (flag)
		{
			flag = await AssetLoader.Exists(assetPath);
		}
		if (flag)
		{
			Debug.Log("牌色張り替え");
			IDisposableAsset<Material> disposableAsset2 = await AssetLoader.LoadAssetAsync<Material>(assetPath, null, cancellationToken);
			m_cubeMaterial.Init(disposableAsset2.Asset, grayColor, darkColor, paiItemID == ItemID.TileSurface025);
		}
		assetPath = ItemIDExtentions.GetTileKiraMaterialPath(paiItemID);
		flag = !string.IsNullOrEmpty(assetPath);
		if (flag)
		{
			flag = await AssetLoader.Exists(assetPath);
		}
		if (flag)
		{
			Debug.Log("聴牌パーティクル張り替え");
			IDisposableAsset<Material> disposableAsset3 = await AssetLoader.LoadAssetAsync<Material>(assetPath, null, cancellationToken);
			m_kiraMaterial.Init(disposableAsset3.Asset, grayColor, darkColor);
		}
		foreach (Tile tile in Tiles)
		{
			tile.RefleshTextureType();
		}
		if (atomicTile != null)
		{
			atomicTile.RefleshTextureType();
		}
		m_spriteAtlas?.Dispose();
		m_spriteAtlas = await AssetLoader.LoadAssetAsync<SpriteAtlas>(AssetPathResolver.ResolveTileSurfaceSpriteAtlas(paiItemID), null, cancellationToken);
		foreach (Pai value in Enum.GetValues(typeof(Pai)))
		{
			TileSprites.Add(value, m_spriteAtlas.Asset.GetSprite(value.ToString()));
		}
	}

	public Sprite GetTileSprite(Pai pai)
	{
		return TileSprites[pai];
	}
}
