using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.Types;
using UnityEngine;

namespace MFG.MahjongPlay.Character.Arm;

public class CharacterArmController : IDisposable
{
	public enum MotionID
	{
		None,
		Away,
		Naki
	}

	private Player m_player;

	private Camera m_discardTileCamera;

	private Camera m_meldTileCamera;

	private GameObject m_parent;

	private IDisposableAsset<GameObject> m_awayPrefab;

	private GameObject m_awayInstance;

	private IDisposableAsset<GameObject> m_nakiPrefab;

	private GameObject m_nakiInstance;

	public async UniTask Initialize(Player player, GameObject parent, Camera discardTileCamera, Camera meldTileCamera, Vector3 discardedTileRenderTextureDiff, Vector3 meldedTileRenderTextureDiff, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		m_player = player;
		m_discardTileCamera = discardTileCamera;
		m_meldTileCamera = meldTileCamera;
		m_parent = parent;
		m_awayPrefab = await AssetLoader.LoadAssetAsync<GameObject>(AssetPathResolver.Resolve(PathType.CharaArmAwayModel, m_player.m_charaType, m_player.m_cha), null, cancellationToken);
		m_awayInstance = UnityEngine.Object.Instantiate(m_awayPrefab.Asset, parent.transform);
		m_awayInstance.GetComponent<RectTransform>().localPosition = discardedTileRenderTextureDiff;
		m_nakiPrefab = await AssetLoader.LoadAssetAsync<GameObject>(AssetPathResolver.Resolve(PathType.CharaArmNakiModel, m_player.m_charaType, m_player.m_cha), null, cancellationToken);
		m_nakiInstance = UnityEngine.Object.Instantiate(m_nakiPrefab.Asset, parent.transform);
		m_nakiInstance.GetComponent<RectTransform>().localPosition = ((m_player.m_cha == Cha.Jicha) ? meldedTileRenderTextureDiff : discardedTileRenderTextureDiff);
	}

	public void Dispose()
	{
		m_awayPrefab?.Dispose();
		m_nakiPrefab?.Dispose();
	}

	public void PlayMotion(MotionID motionID, Vector3 tileWorldPos)
	{
		if (motionID != 0)
		{
			Vector3 vector = default(Vector3);
			vector.z = 0f;
			Camera camera = ((m_player.m_cha == Cha.Jicha) ? m_meldTileCamera : m_discardTileCamera);
			switch (motionID)
			{
			case MotionID.Away:
				vector = m_discardTileCamera.WorldToViewportPoint(tileWorldPos);
				m_awayInstance.GetComponent<RectTransform>().anchorMax = vector;
				m_awayInstance.GetComponent<RectTransform>().anchorMin = vector;
				m_awayInstance.GetComponent<Animator>().SetTrigger("Away");
				break;
			case MotionID.Naki:
				vector = camera.WorldToViewportPoint(tileWorldPos);
				m_nakiInstance.GetComponent<RectTransform>().anchorMax = vector;
				m_nakiInstance.GetComponent<RectTransform>().anchorMin = vector;
				m_nakiInstance.GetComponent<Animator>().SetTrigger("Naki");
				break;
			}
		}
	}
}
