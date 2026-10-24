using UnityEngine;
using UnityEngine.UI;

public class CharacterCameraController : MonoBehaviour
{
	[SerializeField]
	private uint m_SkipFrame = 1u;

	[SerializeField]
	private GameObject m_3dTargetTexture;

	[SerializeField]
	private RenderTexture[] m_RenderTextures;

	[SerializeField]
	private Camera m_3dCameraMyChara;

	private Camera m_3dCameraBack;

	private uint m_uFrameCounter;

	private uint m_uTextureCounter;

	private bool m_bRendered;

	private void Start()
	{
		Application.targetFrameRate = 30;
		m_3dCameraBack = GetComponent<Camera>();
		if (m_SkipFrame < 1)
		{
			m_SkipFrame = 1u;
		}
	}

	private void OnPreCull()
	{
		m_3dCameraBack.clearFlags = CameraClearFlags.Nothing;
		if (!m_bRendered)
		{
			switch (m_uFrameCounter % (1 + m_SkipFrame))
			{
			case 0u:
				m_3dCameraBack.cullingMask = (1 << LayerMask.NameToLayer("Chara3DCenter")) | (1 << LayerMask.NameToLayer("Chara3DLeft")) | (1 << LayerMask.NameToLayer("Chara3DRight")) | (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("UI")) | (1 << LayerMask.NameToLayer("Front3D"));
				break;
			case 1u:
				m_3dCameraBack.cullingMask = 0;
				m_bRendered = true;
				break;
			default:
				m_3dCameraBack.cullingMask = 0;
				break;
			}
		}
		else
		{
			m_3dCameraBack.cullingMask = 0;
		}
	}

	private void OnPreRender()
	{
		if (!m_bRendered && m_uFrameCounter % (1 + m_SkipFrame) == 0)
		{
			long num = m_uTextureCounter % m_RenderTextures.Length;
			long num2 = (m_uTextureCounter + 1) % m_RenderTextures.Length;
			m_3dTargetTexture.GetComponent<RawImage>().texture = m_RenderTextures[num];
			m_3dCameraBack.targetTexture = m_RenderTextures[num2];
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = m_3dCameraBack.targetTexture;
			GL.Clear(clearDepth: true, clearColor: true, m_3dCameraBack.backgroundColor);
			m_3dCameraBack.Render();
			RenderTexture.active = active;
		}
	}

	private void Update()
	{
		m_uFrameCounter++;
		switch (m_uFrameCounter % (1 + m_SkipFrame))
		{
		case 0u:
			m_uTextureCounter++;
			m_bRendered = false;
			m_3dCameraMyChara.enabled = false;
			break;
		case 1u:
			m_3dCameraMyChara.enabled = true;
			break;
		default:
			m_3dCameraMyChara.enabled = false;
			break;
		}
	}
}
