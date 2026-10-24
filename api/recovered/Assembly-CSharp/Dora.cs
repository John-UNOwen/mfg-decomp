using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MFG.MahjonPlay.Hand;
using MFG.Types;
using UnityEngine;

public class Dora : MonoBehaviour
{
	private TileManager tileManager;

	private List<Tile> Tiles = new List<Tile>();

	private Vector3 m_defaultLocalPosition;

	private void Start()
	{
		m_defaultLocalPosition = base.transform.localPosition;
		tileManager = GameObject.Find("TileManager").GetComponent<TileManager>();
		foreach (Transform item in base.transform)
		{
			if (item.gameObject.tag == "Debug")
			{
				Object.Destroy(item.gameObject);
			}
		}
	}

	public void Add(Pai pai)
	{
		Tile tile = tileManager.GetTile(base.transform, pai);
		Vector3 localPosition = tile.transform.localPosition;
		localPosition.x += tile.transform.localScale.x * 0.2f * (float)(Tiles.Count % 5);
		localPosition.z -= 0.07f * (float)(Tiles.Count / 5);
		tile.transform.localPosition = localPosition;
		tile.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
		Tiles.Add(tile);
	}

	public async UniTask SetOpenAt(int index, bool isPlayOpenSe = true)
	{
		if (index >= 0 && index < Tiles.Count)
		{
			MahjongUtility.AddDora(MahjongUtility.DoraHyoujiToDora(Tiles[index].pai), isOmote: true);
			Tiles[index].SetOpen();
			Tiles[index].DoraOpenEffect_SetDisp(isDisp: true, isPlayOpenSe).Forget();
			Tiles[index].DoraOpenEffect_SetDisp(isDisp: false, isPlayOpenSe: true, MahjongUtility.AccelDora.IsEffectiveTaiyoku() ? 2f : 1f).Forget();
			await Tiles[index].transform.DORotateQuaternion(Quaternion.Euler(0f, 0f, 0f), 0.5f).SetEase(Ease.OutBounce).ToAwaiter();
		}
	}

	public Counter GetOpenHaiCounter()
	{
		Counter counter = new Counter();
		foreach (Tile tile in Tiles)
		{
			if (tile.IsOpen())
			{
				HaiID haiID = tile.pai.ToHaiID();
				counter.Add(haiID);
			}
		}
		return counter;
	}

	public async UniTask MoveUradoraOpenPosition()
	{
		await base.transform.DOLocalMove(new Vector3(m_defaultLocalPosition.x, 0f, 0.035f * (float)(1 + (Tiles.Count - 1) / 5)), 0.2f).SetEase(Ease.InQuad).ToAwaiter();
	}

	public void Destruct()
	{
		base.transform.localPosition = m_defaultLocalPosition;
		foreach (Tile tile in Tiles)
		{
			tileManager.ReturnTile(tile);
		}
		Tiles.Clear();
	}

	public void DouhaiSearchReflesh()
	{
		foreach (Tile tile in Tiles)
		{
			tile.DouhaiSearchReflesh();
		}
	}

	public Vector3 GetAgariWanpaiScale()
	{
		float num = 1f - 0.1f * (float)((Tiles.Count > 0) ? (Tiles.Count - 1) : 0) / 5f;
		return new Vector3(num, num, num);
	}

	public void AlignOneLine()
	{
		base.transform.localPosition = new Vector3(m_defaultLocalPosition.x, 0f, 0.035f);
		int num = 0;
		foreach (Transform item in base.transform)
		{
			item.transform.localPosition = new Vector3(item.transform.localScale.x * (float)num * 0.2f, 0f, 0f);
			num++;
		}
	}
}
