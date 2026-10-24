using System.Collections.Generic;
using MFG.Types;
using UnityEngine;

internal class TsumikomiTileManager : TileManager
{
	private class TileMaterial
	{
		public Material normal;

		public Material gray;

		public Material dark;

		public Material blink;

		public void Init(Material material, Color grayColor, Color darkColor)
		{
			normal = new Material(material);
			gray = new Material(material);
			gray.color = grayColor;
			dark = new Material(material);
			dark.color = darkColor;
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

	private static TsumikomiTileManager m_instance;

	private TileMaterial m_cubeMaterial = new TileMaterial();

	private TileMaterial m_haiMaterial = new TileMaterial();

	private Dictionary<Pai, Sprite> TileSprites = new Dictionary<Pai, Sprite>();

	public new static TsumikomiTileManager GetInstance()
	{
		if (!m_instance)
		{
			m_instance = GameObject.Find("TsumikomiSet")?.GetComponent<TsumikomiTileManager>();
		}
		return m_instance;
	}

	private void Awake()
	{
		m_cubeMaterial.Init(cubeMaterial, grayColor, darkColor);
		m_haiMaterial.Init(haiMaterial, grayColor, darkColor);
		TileSprites.Clear();
	}

	private void Update()
	{
	}
}
