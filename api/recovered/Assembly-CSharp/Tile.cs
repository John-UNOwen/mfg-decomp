using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MFG.Types;
using TMPro;
using UnityEngine;

public class Tile : MonoBehaviour
{
	[Serializable]
	public class KiraColor
	{
		public int tileSurfaceNum;

		public Color color;
	}

	public enum HaiJudgeMark
	{
		OpenDanger,
		Danger,
		Warning,
		Safety,
		None
	}

	public enum ShantenMark
	{
		Up,
		Down,
		None
	}

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

	[SerializeField]
	public MeshRenderer meshRendererSe;

	[SerializeField]
	public ParticleSystemRenderer meshRendererKira;

	[SerializeField]
	private List<KiraColor> kiraColors;

	[SerializeField]
	public ParticleSystem kiraParticle;

	[SerializeField]
	public ParticleSystem kiraGlow;

	[SerializeField]
	public ParticleSystem kiraGlowLoop;

	[SerializeField]
	public GameObject selectedPlane;

	[SerializeField]
	public GameObject tsumogiriAnime;

	[SerializeField]
	private Animator anpaiMark;

	[SerializeField]
	private Animator kikenhaiMark;

	[SerializeField]
	private Animator tenpaiTargetIcon;

	[SerializeField]
	private Animator openTargetIcon;

	private Mesh haiMesh;

	[SerializeField]
	private ParticleSystem m_doraEffect;

	[SerializeField]
	private ParticleSystem m_yakuChanceEffect;

	[SerializeField]
	public GameObject tenpaiEffect;

	[SerializeField]
	public GameObject tenpaiStrengthEffect;

	[SerializeField]
	public GameObject rinshanTsumoEffect;

	[SerializeField]
	public GameObject haiJudgeAllGreen;

	[SerializeField]
	public GameObject haiJudgeOpenDanger;

	[SerializeField]
	public GameObject haiJudgeDanger;

	[SerializeField]
	public GameObject haiJudgeWarning;

	[SerializeField]
	public GameObject haiJudgeSafety;

	[SerializeField]
	public GameObject shantenUp;

	[SerializeField]
	public GameObject shantenDown;

	[SerializeField]
	private GameObject _accelDoraOpenEffect;

	[SerializeField]
	private GameObject _keyboardText;

	public Pai pai;

	public bool m_enableDiscard = true;

	public bool m_isSelected;

	public bool m_isOpen;

	public bool m_isSide;

	public bool IsTsumogiri;

	public bool IsBlink;

	public bool IsMoving;

	public Tweener m_tweener;

	public ColorType m_colorType;

	public bool IsDiscardStart;

	public bool IsTenpaiTarget;

	public bool IsOpenTarget;

	private void Awake()
	{
		SetColorType(ColorType.Normal);
	}

	public void DouhaiSearchReflesh()
	{
		Tile touchedTile = MahjongUtility.GetTouchedTile();
		if (m_isOpen && touchedTile != null && MahjongUtility.AkaToNormal(touchedTile.pai) == MahjongUtility.AkaToNormal(pai))
		{
			selectedPlane.SetActive(value: true);
		}
		else
		{
			selectedPlane.SetActive(value: false);
		}
	}

	public void StartDoraEffect()
	{
		m_doraEffect.Play();
		ParticleSystem.MainModule main = m_doraEffect.main;
		main.loop = true;
	}

	public void StartDoraEffectOnce()
	{
		m_doraEffect.Play();
		ParticleSystem.MainModule main = m_doraEffect.main;
		main.loop = false;
	}

	public void StartYakuChanceEffect()
	{
		m_yakuChanceEffect.Play();
		ParticleSystem.MainModule main = m_yakuChanceEffect.main;
		main.loop = false;
	}

	public void SetOpen()
	{
		if (!m_isOpen)
		{
			MahjongUtility.SubRestPaiNum(pai);
			m_isOpen = true;
		}
	}

	public bool IsOpen()
	{
		return m_isOpen;
	}

	public void SetDark(bool set)
	{
		if (!(IsBlink && set))
		{
			if (set)
			{
				SetColorType(ColorType.Dark);
			}
			else if (IsTsumogiri)
			{
				SetColorType(ColorType.Gray);
			}
			else
			{
				SetColorType(ColorType.Normal);
			}
		}
	}

	public void SetTsumogiriMark(bool set, bool isSide = false)
	{
		if (set)
		{
			IsTsumogiri = true;
			SetColorType(ColorType.Gray);
			tsumogiriAnime.SetActive(value: true);
			tsumogiriAnime.GetComponent<Animator>().SetTrigger("Play");
			tsumogiriAnime.transform.localRotation = Quaternion.Euler(90f, isSide ? 90f : 0f, 0f);
		}
		else
		{
			IsTsumogiri = false;
			SetDark(set: false);
			tsumogiriAnime.SetActive(value: false);
		}
	}

	public void SetAnpaiMark(bool set)
	{
	}

	public void SetTenpaiTargetIcon(bool set)
	{
		if (set)
		{
			tenpaiTargetIcon.gameObject.SetActive(value: true);
			tenpaiTargetIcon.SetTrigger("In");
		}
		else
		{
			tenpaiTargetIcon.gameObject.SetActive(value: false);
		}
	}

	public void SetOpenTargetIcon(bool set)
	{
		if (set)
		{
			openTargetIcon.gameObject.SetActive(value: true);
			openTargetIcon.SetTrigger("In");
		}
		else
		{
			openTargetIcon.gameObject.SetActive(value: false);
		}
	}

	public void SetKikenhaiMark(bool set)
	{
	}

	public void SetAllGreenMark(bool set)
	{
		if (GameDataManager.Mahjong.IsAllGreenBonusMode && HaiUtility.RyuisoHais().Contains(pai.ToHaiID()))
		{
			haiJudgeAllGreen.SetActive(set);
		}
		else
		{
			haiJudgeAllGreen.SetActive(value: false);
		}
	}

	public void SetHaiJudgeMark(HaiJudgeMark haiJudge)
	{
		switch (haiJudge)
		{
		case HaiJudgeMark.OpenDanger:
			haiJudgeOpenDanger.SetActive(value: true);
			haiJudgeDanger.SetActive(value: false);
			haiJudgeWarning.SetActive(value: false);
			haiJudgeSafety.SetActive(value: false);
			break;
		case HaiJudgeMark.Safety:
			haiJudgeOpenDanger.SetActive(value: false);
			haiJudgeDanger.SetActive(value: false);
			haiJudgeWarning.SetActive(value: false);
			haiJudgeSafety.SetActive(value: true);
			break;
		case HaiJudgeMark.None:
			haiJudgeOpenDanger.SetActive(value: false);
			haiJudgeDanger.SetActive(value: false);
			haiJudgeWarning.SetActive(value: false);
			haiJudgeSafety.SetActive(value: false);
			break;
		case HaiJudgeMark.Danger:
		case HaiJudgeMark.Warning:
			break;
		}
	}

	public HaiJudgeMark GetHaiJudgeMark()
	{
		if (haiJudgeOpenDanger.activeSelf)
		{
			return HaiJudgeMark.OpenDanger;
		}
		if (haiJudgeDanger.activeSelf)
		{
			return HaiJudgeMark.Danger;
		}
		if (haiJudgeWarning.activeSelf)
		{
			return HaiJudgeMark.Warning;
		}
		if (haiJudgeSafety.activeSelf)
		{
			return HaiJudgeMark.Safety;
		}
		return HaiJudgeMark.None;
	}

	public void SetShantenMark(ShantenMark shantenMark)
	{
		switch (shantenMark)
		{
		case ShantenMark.Up:
			shantenUp.SetActive(value: true);
			shantenDown.SetActive(value: false);
			break;
		case ShantenMark.Down:
			shantenUp.SetActive(value: false);
			shantenDown.SetActive(value: true);
			break;
		case ShantenMark.None:
			shantenUp.SetActive(value: false);
			shantenDown.SetActive(value: false);
			break;
		}
	}

	public void SetHaiTexture(Pai pai)
	{
		haiMesh = base.transform.Find("Hai").GetComponent<MeshFilter>().mesh;
		Vector2[] uv = haiMesh.uv;
		bool flag = false;
		if (GameDataManager.Mahjong.IsKirisameMode)
		{
			flag = MahjongUtility.IsAkahai(pai);
			pai = MahjongUtility.AkaToNormal(pai);
		}
		else
		{
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
		}
		pai--;
		float num = 0.1f * (float)pai;
		float num2 = 0.125f * (float)((int)pai / 10 + (flag ? 4 : 0));
		uv[3].x = num;
		uv[3].y = num2;
		uv[2].x = num + 0.1f;
		uv[2].y = num2;
		uv[1].x = num;
		uv[1].y = num2 + 0.125f;
		uv[0].x = num + 0.1f;
		uv[0].y = num2 + 0.125f;
		haiMesh.uv = uv;
	}

	public void StartBlink()
	{
		IsBlink = true;
		SetColorType(ColorType.Blink);
	}

	public void EndBlink()
	{
		IsBlink = false;
		if (IsTsumogiri)
		{
			SetColorType(ColorType.Gray);
		}
		else
		{
			SetColorType(ColorType.Normal);
		}
	}

	public void Destruct()
	{
		UnityEngine.Object.Destroy(meshRendererHai.material);
		UnityEngine.Object.Destroy(meshRendererCube.material);
		UnityEngine.Object.Destroy(meshRendererSe.material);
		UnityEngine.Object.Destroy(meshRendererKira.material);
		UnityEngine.Object.Destroy(haiMesh);
	}

	public void SetKeyTex(int index)
	{
		try
		{
			_keyboardText.transform.GetComponent<TextMeshPro>().text = MahjongUtility.KeyBoard.KeyCodeText[index];
		}
		catch
		{
		}
	}

	public void KeyTexActiveControll(bool isActive)
	{
		_keyboardText.SetActive(isActive);
	}

	public void StartTenpaiEffect(bool isMangan, bool isRestart = false)
	{
		if (isRestart)
		{
			tenpaiStrengthEffect.SetActive(value: false);
			tenpaiEffect.SetActive(value: false);
		}
		if (isMangan)
		{
			tenpaiStrengthEffect.SetActive(value: true);
			tenpaiEffect.SetActive(value: false);
		}
		else
		{
			tenpaiStrengthEffect.SetActive(value: false);
			tenpaiEffect.SetActive(value: true);
		}
	}

	public void StopTenpaiEffect()
	{
		tenpaiEffect.SetActive(value: false);
		tenpaiStrengthEffect.SetActive(value: false);
	}

	public void DoraOpenEffect_SetDisp(bool isDisp)
	{
		DoraOpenEffect_SetDisp(isDisp, isPlayOpenSe: true, 0f).Forget();
	}

	public async UniTask DoraOpenEffect_SetDisp(bool isDisp, bool isPlayOpenSe = true, float delaySec = 0f)
	{
		if (delaySec > 0f)
		{
			await UniTask.Delay(TimeSpan.FromSeconds(delaySec));
		}
		if (MahjongUtility.AccelDora.IsEffectiveTaiyoku())
		{
			if (isDisp && isPlayOpenSe)
			{
				SoundManager.PlaySe("SFX_MJG_DISPLAY24").Forget();
			}
			_accelDoraOpenEffect.SetActive(isDisp);
		}
		else if (isDisp)
		{
			StartTenpaiEffect(isMangan: true);
		}
		else
		{
			StopTenpaiEffect();
		}
	}

	public void StartRinshanEffect()
	{
		rinshanTsumoEffect.SetActive(value: true);
	}

	public void StopRinshanEffect()
	{
		rinshanTsumoEffect.SetActive(value: false);
	}

	private void OnDestroy()
	{
		Destruct();
	}

	public void Dump()
	{
		foreach (Transform item in base.transform)
		{
			item.gameObject.layer = 0;
		}
		SetTenpaiTargetIcon(set: false);
		SetOpenTargetIcon(set: false);
		StopTenpaiEffect();
		StopRinshanEffect();
		SetHaiJudgeMark(HaiJudgeMark.None);
		SetShantenMark(ShantenMark.None);
		SetAllGreenMark(set: false);
		KeyTexActiveControll(isActive: false);
		GetComponent<BoxCollider>().enabled = false;
		SetLayer(isJichaTehai: false);
	}

	public void Meld()
	{
		SetTenpaiTargetIcon(set: false);
		SetOpenTargetIcon(set: false);
		StopTenpaiEffect();
		StopRinshanEffect();
		IsTsumogiri = false;
		SetDark(set: false);
		SetTsumogiriMark(set: false);
		SetAnpaiMark(set: false);
		SetTenpaiTargetIcon(set: false);
		SetOpenTargetIcon(set: false);
		SetKikenhaiMark(set: false);
		SetAllGreenMark(set: false);
		SetHaiJudgeMark(HaiJudgeMark.None);
		SetShantenMark(ShantenMark.None);
		KeyTexActiveControll(isActive: false);
		GetComponent<BoxCollider>().enabled = false;
		if (m_tweener != null)
		{
			m_tweener.Kill(complete: true);
		}
	}

	public void Selected(bool isSelected)
	{
		m_isSelected = isSelected;
		BoxCollider component = GetComponent<BoxCollider>();
		if (m_isSelected)
		{
			component.center = new Vector3(0f, 0f, -0.04f);
			component.size = new Vector3(0.2f, 0.15f, 0.34f);
		}
		else
		{
			component.center = new Vector3(0f, 0f, 0f);
			component.size = new Vector3(0.2f, 0.15f, 0.26f);
		}
	}

	public void ResetParameter()
	{
		SetLayer(isJichaTehai: false);
		m_doraEffect.Pause();
		m_yakuChanceEffect.Pause();
		Selected(isSelected: false);
		m_enableDiscard = true;
		m_isOpen = false;
		m_isSide = false;
		IsTsumogiri = false;
		IsBlink = false;
		IsMoving = false;
		m_tweener = null;
		IsDiscardStart = false;
		IsTenpaiTarget = false;
		SetColorType(ColorType.Normal);
		SetTsumogiriMark(set: false);
		SetAnpaiMark(set: false);
		SetTenpaiTargetIcon(set: false);
		SetOpenTargetIcon(set: false);
		SetKikenhaiMark(set: false);
		SetAllGreenMark(set: false);
		SetHaiJudgeMark(HaiJudgeMark.None);
		SetShantenMark(ShantenMark.None);
		KeyTexActiveControll(isActive: false);
		StopTenpaiEffect();
		StopRinshanEffect();
		DoraOpenEffect_SetDisp(isDisp: false);
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
		TileManager instance = TileManager.GetInstance();
		if (instance == null)
		{
			return;
		}
		switch (m_colorType)
		{
		case ColorType.Normal:
			meshRendererHai.material = instance.GetMaterial(TileManager.MaterialType.Hai);
			meshRendererCube.material = instance.GetMaterial(TileManager.MaterialType.Cube);
			meshRendererSe.material = instance.GetMaterial(TileManager.MaterialType.Se);
			meshRendererKira.sharedMaterial = instance.GetMaterial(TileManager.MaterialType.Kira);
			break;
		case ColorType.Gray:
			meshRendererHai.material = instance.GetMaterial(TileManager.MaterialType.HaiGray);
			meshRendererCube.material = instance.GetMaterial(TileManager.MaterialType.CubeGray);
			meshRendererSe.material = instance.GetMaterial(TileManager.MaterialType.SeGray);
			meshRendererKira.sharedMaterial = instance.GetMaterial(TileManager.MaterialType.KiraGray);
			break;
		case ColorType.Dark:
			meshRendererHai.material = instance.GetMaterial(TileManager.MaterialType.HaiDark);
			meshRendererCube.material = instance.GetMaterial(TileManager.MaterialType.CubeDark);
			meshRendererSe.material = instance.GetMaterial(TileManager.MaterialType.SeDark);
			meshRendererKira.sharedMaterial = instance.GetMaterial(TileManager.MaterialType.KiraDark);
			break;
		case ColorType.Blink:
			meshRendererHai.material = instance.GetMaterial(TileManager.MaterialType.HaiBlink);
			meshRendererCube.material = instance.GetMaterial(TileManager.MaterialType.CubeBlink);
			meshRendererSe.material = instance.GetMaterial(TileManager.MaterialType.SeBlink);
			meshRendererKira.sharedMaterial = instance.GetMaterial(TileManager.MaterialType.KiraBlink);
			break;
		}
		string text = instance.paiItemID.ToString().Replace("TileSurface", "");
		int i;
		for (i = 0; i < 3 && text[i].Equals("0"); i++)
		{
		}
		string text2 = text;
		int num = i;
		if (int.TryParse(text2.Substring(num, text2.Length - num), out var tileSurfaceNum))
		{
			KiraColor kiraColor = kiraColors?.Find((KiraColor x) => x.tileSurfaceNum == tileSurfaceNum) ?? null;
			if (kiraColor != null)
			{
				ParticleSystem.MainModule main = kiraParticle.main;
				ParticleSystem.MainModule main2 = kiraGlow.main;
				ParticleSystem.MainModule main3 = kiraGlowLoop.main;
				main.startColor = new ParticleSystem.MinMaxGradient(kiraColor.color);
				main2.startColor = new ParticleSystem.MinMaxGradient(kiraColor.color);
				main3.startColor = new ParticleSystem.MinMaxGradient(kiraColor.color);
			}
		}
	}
}
