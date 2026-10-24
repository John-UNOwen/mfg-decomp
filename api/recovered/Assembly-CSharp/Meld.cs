using System.Collections.Generic;
using MFG.MahjonPlay.Hand;
using MFG.MahjongPlay.Character.Arm;
using MFG.Types;
using UnityEngine;

public class Meld : MonoBehaviour
{
	[SerializeField]
	private Player player;

	[SerializeField]
	private GameObject slideEffect;

	private TileManager tileManager;

	private List<Tile> Tiles = new List<Tile>();

	private BlockSet FuroBlock = new BlockSet();

	private List<List<Tile>> blockedTiles = new List<List<Tile>>();

	private Tile m_lastKanHai;

	private const float SpaceX = 0.01f;

	private float posx;

	public int MeldCount => FuroBlock.Count();

	private void Start()
	{
		tileManager = GameObject.Find("TileManager").GetComponent<TileManager>();
		foreach (Transform item in base.transform)
		{
			if (item.gameObject.tag == "Debug")
			{
				Object.Destroy(item.gameObject);
			}
		}
	}

	private void Update()
	{
	}

	public void AddPon(Cha cha, Tile item, Tile item1, Tile item2)
	{
		Tile tile = null;
		switch (cha)
		{
		case Cha.Simocha:
			Add(item, side: true);
			Add(item1);
			Add(item2);
			tile = item1;
			break;
		case Cha.Toimen:
			Add(item1);
			Add(item, side: true);
			Add(item2);
			tile = item;
			break;
		case Cha.Kamicha:
			Add(item1);
			Add(item2);
			Add(item, side: true);
			tile = item2;
			break;
		}
		FuroBlock.AddBlock(Block.CreatePon(cha, item, item1, item2));
		blockedTiles.Add(new List<Tile> { item, item1, item2 });
		if (MahjongUtility.GetViewType() == ViewType.Type3 && tile != null)
		{
			player.character.Action_PlayArmMotion(CharacterArmController.MotionID.Naki, tile.transform.position);
		}
		posx -= 0.01f;
		EffectStart(item);
	}

	public void AddChi(Tile item, Tile item1, Tile item2)
	{
		Add(item2);
		Add(item1);
		Add(item, side: true);
		FuroBlock.AddBlock(Block.CreateChi(item, item1, item2));
		blockedTiles.Add(new List<Tile> { item, item1, item2 });
		if (MahjongUtility.GetViewType() == ViewType.Type3)
		{
			player.character.Action_PlayArmMotion(CharacterArmController.MotionID.Naki, item1.transform.position);
		}
		posx -= 0.01f;
		EffectStart(item);
	}

	public void AddMinkan(Cha cha, Tile item, Tile item1, Tile item2, Tile item3)
	{
		switch (cha)
		{
		case Cha.Simocha:
			Add(item, side: true);
			Add(item2);
			Add(item3);
			AddKan(item1, item);
			break;
		case Cha.Toimen:
			Add(item2);
			Add(item, side: true);
			Add(item3);
			AddKan(item1, item);
			break;
		case Cha.Kamicha:
			Add(item2);
			Add(item3);
			Add(item, side: true);
			AddKan(item1, item);
			break;
		}
		FuroBlock.AddBlock(Block.CreateMinkan(cha, item, item1, item2, item3));
		blockedTiles.Add(new List<Tile> { item, item1, item2, item3 });
		if (MahjongUtility.GetViewType() == ViewType.Type3)
		{
			player.character.Action_PlayArmMotion(CharacterArmController.MotionID.Naki, item1.transform.position);
		}
		posx -= 0.01f;
		EffectStart(item);
	}

	public void AddAnkan(Tile item1, Tile item2, Tile item3, Tile item4)
	{
		Add(item3, side: false, flip: true);
		Add(item1, side: true);
		AddKan(item2, item1);
		Add(item4, side: false, flip: true);
		Debug.Log($"Meld AddAnkan{item1.pai}/{item2.pai}/{item3.pai}/{item4.pai}");
		FuroBlock.AddBlock(Block.CreateAnkan(item1, item2, item3, item4));
		blockedTiles.Add(new List<Tile> { item1, item2, item3, item4 });
		if (MahjongUtility.GetViewType() == ViewType.Type3)
		{
			player.character.Action_PlayArmMotion(CharacterArmController.MotionID.Naki, item2.transform.position);
		}
		posx -= 0.01f;
		EffectStart(item1);
		m_lastKanHai = item2;
		MahjongUtility.SetLastTargetTile(item2);
	}

	public void AddKakan(Tile item, Pai pai)
	{
		Tile itemBase = null;
		pai = MahjongUtility.AkaToNormal(pai);
		itemBase = Tiles.Find((Tile a) => MahjongUtility.AkaToNormal(a.pai) == pai && a.m_isSide);
		AddKan(item, itemBase);
		FuroBlock.AddKakan(pai.ToHaiID());
		blockedTiles.Find((List<Tile> tiles) => tiles.Find((Tile tile) => tile == itemBase)).Add(item);
		if (MahjongUtility.GetViewType() == ViewType.Type3)
		{
			player.character.Action_PlayArmMotion(CharacterArmController.MotionID.Naki, item.transform.position);
		}
		EffectStart(item);
		m_lastKanHai = item;
		MahjongUtility.SetLastTargetTile(item);
		item.StartBlink();
	}

	public void Add(Tile item, bool side = false, bool flip = false)
	{
		item.gameObject.SetActive(value: true);
		item.Meld();
		item.transform.SetParent(base.gameObject.transform, worldPositionStays: false);
		item.SetLayer(player.IsJicha());
		if (side)
		{
			float x = posx - item.transform.localScale.z * 0.059999987f / 2f;
			float z = (0f - item.transform.localScale.z) * 0.059999987f / 2f;
			item.transform.localPosition = new Vector3(x, 0f, z);
			posx -= item.transform.localScale.x * 0.26f;
			item.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
		}
		else
		{
			item.transform.localPosition = new Vector3(posx, 0f, 0f);
			posx -= item.transform.localScale.x * 0.2f;
			item.transform.localRotation = Quaternion.identity;
		}
		if (flip)
		{
			item.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
		}
		Tiles.Add(item);
		item.SetOpen();
		item.m_isSide = side;
	}

	public void AddKan(Tile item, Tile itemBase)
	{
		item.Meld();
		item.transform.SetParent(base.gameObject.transform, worldPositionStays: false);
		item.SetLayer(player.IsJicha());
		float x = itemBase.transform.localPosition.x;
		float z = itemBase.transform.localPosition.z + itemBase.transform.localScale.x * 0.2f;
		item.transform.localPosition = new Vector3(x, 0f, z);
		item.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
		Tiles.Add(item);
		item.SetOpen();
	}

	public void Destruct()
	{
		foreach (Tile tile in Tiles)
		{
			tileManager.ReturnTile(tile);
		}
		Tiles.Clear();
		FuroBlock.Clear();
		blockedTiles.Clear();
		posx = 0f;
	}

	private void EffectStart(Tile item)
	{
		slideEffect.transform.position = item.transform.position;
		slideEffect.transform.Find("Line").GetComponent<ParticleSystem>().Play();
		slideEffect.transform.Find("Kira").GetComponent<ParticleSystem>().Play();
	}

	public Tile GetLastKanHai()
	{
		return m_lastKanHai;
	}

	public Counter GetHaiCounter(bool kantsuCountAs3 = false)
	{
		Counter counter = new Counter();
		foreach (Tile tile in Tiles)
		{
			HaiID haiID = tile.pai.ToHaiID();
			counter.Add(haiID);
		}
		if (kantsuCountAs3)
		{
			for (int i = 0; i < 34; i++)
			{
				if (counter.CheckCount((HaiID)i, 4))
				{
					counter.Dec((HaiID)i);
				}
			}
		}
		return counter;
	}

	public BlockSet GetBlockSet()
	{
		return FuroBlock;
	}

	public void StartDoraEffect(bool isReachAgari = true)
	{
		MahjongUtility.DoraRange doraRange = MahjongUtility.DoraRange.All;
		if (!isReachAgari)
		{
			doraRange = MahjongUtility.DoraRange.Omote;
		}
		foreach (Transform item in base.transform)
		{
			Tile component = item.GetComponent<Tile>();
			if (component != null && MahjongUtility.IsDora(component.pai, doraRange))
			{
				component.StartDoraEffect();
			}
		}
	}

	public void StartYakuChanceEffect(Block _block, ref int usedIndexFlag)
	{
		for (int i = 0; i < MeldCount; i++)
		{
			if ((usedIndexFlag & (1 << i)) > 0)
			{
				continue;
			}
			Block block = FuroBlock[i];
			if (block.BlockType != _block.BlockType || block.HaiID != _block.HaiID)
			{
				continue;
			}
			foreach (Tile item in blockedTiles[i])
			{
				item.StartYakuChanceEffect();
			}
			usedIndexFlag |= 1 << i;
			break;
		}
	}

	public int GetDoraCount()
	{
		int num = 0;
		foreach (Tile tile in Tiles)
		{
			num += MahjongUtility.GetPureDoraCount(tile.pai);
			if (MahjongUtility.IsAkahai(tile.pai))
			{
				num++;
			}
		}
		return num;
	}

	public int GetOmoteDoraCount()
	{
		int num = 0;
		foreach (Tile tile in Tiles)
		{
			num += MahjongUtility.GetPureDoraCount(tile.pai, MahjongUtility.DoraRange.Omote);
		}
		return num;
	}

	public int GetUraDoraCount()
	{
		int num = 0;
		foreach (Tile tile in Tiles)
		{
			num += MahjongUtility.GetPureDoraCount(tile.pai, MahjongUtility.DoraRange.Ura);
		}
		return num;
	}

	public int GetAkaDoraCount()
	{
		int num = 0;
		foreach (Tile tile in Tiles)
		{
			if (MahjongUtility.IsAkahai(tile.pai))
			{
				num++;
			}
		}
		return num;
	}

	public bool IsContainDora(Pai dora)
	{
		return Tiles.Exists((Tile a) => a.pai == dora);
	}

	public void StartTenpaiEffect(bool isMangan, bool isRestart)
	{
		foreach (Transform item in base.transform)
		{
			Tile component = item.GetComponent<Tile>();
			if (component != null)
			{
				component.StartTenpaiEffect(isMangan, isRestart);
			}
		}
	}

	public void StopTenpaiEffect()
	{
		foreach (Transform item in base.transform)
		{
			Tile component = item.GetComponent<Tile>();
			if (component != null)
			{
				component.StopTenpaiEffect();
			}
		}
	}

	public void SetDark(bool set)
	{
		foreach (Tile tile in Tiles)
		{
			tile.SetDark(set);
		}
	}

	public void DouhaiSearchReflesh()
	{
		foreach (Tile tile in Tiles)
		{
			tile.DouhaiSearchReflesh();
		}
	}
}
