using UnityEngine;
using UnityEngine.UI;

public class ScopeGraphicRaycaster : GraphicRaycaster
{
	private Camera m_camera;

	public override Camera eventCamera => m_camera;

	public void SetTargetCanvas(Canvas canvas)
	{
		if (canvas.renderMode == RenderMode.ScreenSpaceOverlay || (canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null))
		{
			m_camera = null;
			return;
		}
		m_camera = canvas.worldCamera ?? Camera.main;
		m_camera.targetDisplay = canvas.targetDisplay;
	}
}
