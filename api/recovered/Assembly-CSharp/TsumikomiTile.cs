using DG.Tweening;
using MFG.Types;
using UnityEngine;

public class TsumikomiTile : MonoBehaviour
{
	public enum ColorType
	{
		Normal,
		Gray,
		Dark,
		Blink
	}

	[SerializeField]
	public MeshRenderer meshRendererHai;

	[SerializeField]
	public MeshRenderer meshRendererCube;

	private Mesh haiMesh;

	public Pai pai;

	public DebugTsumikomi.Place placeP;

	public int placeQ;

	public bool m_isSelected;

	public Tweener m_tweener;

	public ColorType m_colorType;

	private void Awake()
	{
	}

	public void StartChoice()
	{
		SetColorType(ColorType.Gray);
	}

	public void EndChoice()
	{
		SetColorType(ColorType.Normal);
	}

	public void SetHaiTexture(Pai pai)
	{
		haiMesh = base.transform.Find("Hai").GetComponent<MeshFilter>().mesh;
		Vector2[] uv = haiMesh.uv;
		switch (pai)
		{
		case Pai.M5R:
			pai = (Pai)10;
			break;
		case Pai.S5R:
			pai = (Pai)20;
			break;
		case Pai.P5R:
			pai = (Pai)30;
			break;
		}
		pai--;
		float num = 0.1f * (float)pai;
		float num2 = 0.25f * (float)((int)pai / 10);
		uv[3].x = num;
		uv[3].y = num2;
		uv[2].x = num + 0.1f;
		uv[2].y = num2;
		uv[1].x = num;
		uv[1].y = num2 + 0.25f;
		uv[0].x = num + 0.1f;
		uv[0].y = num2 + 0.25f;
		haiMesh.uv = uv;
	}

	public void Destruct()
	{
		Object.Destroy(meshRendererHai.material);
		Object.Destroy(meshRendererCube.material);
		Object.Destroy(haiMesh);
	}

	private void OnDestroy()
	{
		Destruct();
	}

	public void ResetParameter()
	{
		SetColorType(ColorType.Normal);
		GetComponent<BoxCollider>().enabled = false;
		foreach (Transform item in base.transform)
		{
			item.gameObject.layer = 0;
		}
	}

	public void SetLayer(bool isJichaTehai)
	{
		RecursiveSetLayer(base.gameObject, isJichaTehai);
	}

	private void SetLayer(GameObject obj, bool isJichaTehai)
	{
		obj.layer = (isJichaTehai ? LayerMask.NameToLayer("3DTehai") : 0);
	}

	private void RecursiveSetLayer(GameObject obj, bool isJichaTehai)
	{
		foreach (Transform item in obj.transform)
		{
			GameObject obj2 = item.gameObject;
			SetLayer(obj2, isJichaTehai);
			RecursiveSetLayer(obj2, isJichaTehai);
		}
	}

	public void SetColorType(ColorType type)
	{
		m_colorType = type;
		RefleshTextureType();
	}

	public void RefleshTextureType()
	{
		TsumikomiTileManager instance = TsumikomiTileManager.GetInstance();
		if (!(instance == null))
		{
			switch (m_colorType)
			{
			case ColorType.Normal:
				meshRendererHai.material = instance.GetMaterial(TileManager.MaterialType.Hai);
				meshRendererCube.material = instance.GetMaterial(TileManager.MaterialType.Cube);
				break;
			case ColorType.Gray:
				meshRendererHai.material = instance.GetMaterial(TileManager.MaterialType.HaiGray);
				meshRendererCube.material = instance.GetMaterial(TileManager.MaterialType.CubeGray);
				break;
			case ColorType.Dark:
				meshRendererHai.material = instance.GetMaterial(TileManager.MaterialType.HaiDark);
				meshRendererCube.material = instance.GetMaterial(TileManager.MaterialType.CubeDark);
				break;
			case ColorType.Blink:
				meshRendererHai.material = instance.GetMaterial(TileManager.MaterialType.HaiBlink);
				meshRendererCube.material = instance.GetMaterial(TileManager.MaterialType.CubeBlink);
				break;
			}
		}
	}
}
