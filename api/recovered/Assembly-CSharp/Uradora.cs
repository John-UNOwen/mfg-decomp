using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MFG.Types;
using UnityEngine;

public class Uradora : MonoBehaviour
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

	private void Update()
	{
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

	public async UniTask SetOpenNum(int openNum)
	{
		await base.transform.DOLocalMove(new Vector3(m_defaultLocalPosition.x, m_defaultLocalPosition.y, -0.035f * (float)(1 + (Tiles.Count - 1) / 5)), 0.2f).SetEase(Ease.InQuad).ToAwaiter();
		string[] openSE = new string[5] { "SFX_MJG_URADORA1", "SFX_MJG_URADORA2", "SFX_MJG_URADORA3", "SFX_MJG_URADORA4", "SFX_MJG_URADORA5" };
		for (int i = 0; i < openNum; i++)
		{
			await UniTask.Delay(500);
			MahjongUtility.AddDora(MahjongUtility.DoraHyoujiToDora(Tiles[i].pai), isOmote: false);
			Tiles[i].SetOpen();
			SoundManager.PlaySe(openSE[(i >= openSE.Length) ? (openSE.Length - 1) : i]).Forget();
			Tiles[i].DoraOpenEffect_SetDisp(isDisp: true);
			Tiles[i].DoraOpenEffect_SetDisp(isDisp: false, isPlayOpenSe: true, MahjongUtility.AccelDora.IsEffectiveTaiyoku() ? 2f : 1f).Forget();
			await Tiles[i].transform.DOLocalRotateQuaternion(Quaternion.Euler(0f, 0f, 0f), 0.5f).SetEase(Ease.OutBounce).ToAwaiter();
		}
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

	public void AlignOneLine()
	{
		base.transform.localPosition = new Vector3(m_defaultLocalPosition.x, 0f, -0.035f);
		int num = 0;
		foreach (Transform item in base.transform)
		{
			item.transform.localPosition = new Vector3(item.transform.localScale.x * (float)num * 0.2f, 0f, 0f);
			num++;
		}
	}
}
