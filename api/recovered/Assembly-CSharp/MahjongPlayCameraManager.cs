using MFG.Assets;
using MFG.MahjongPlay.Character;
using MFG.MasterGameData;
using UnityEngine;

public class MahjongPlayCameraManager : MonoBehaviour
{
	private Player player;

	private Vector3 ReserveCharaPosition;

	private Quaternion ReserveCharaRotation;

	private CharacterModelController modelController;

	private Vector3 parentEngle;

	private bool m_isCameraMyCharaAdulationbefore;

	private Camera camera => base.transform.GetComponent<Camera>();

	public void Init(Player p)
	{
		player = p;
		CharacterProperties characterProperties = MasterGameDataManager.Character.Properties(player.m_charaType);
		base.transform.GetComponent<TransformReserver>().RestoreSelf();
		Vector3 localPosition = base.transform.localPosition;
		localPosition.y += characterProperties.MahjongHeightOffsetParam;
		base.transform.localPosition = localPosition;
		ReserveCharaPosition = base.transform.localPosition;
		ReserveCharaRotation = base.transform.localRotation;
		modelController = player.character.m_characterModelController;
		parentEngle = ((base.transform.parent != null) ? base.transform.parent.eulerAngles : default(Vector3));
	}

	private void Update()
	{
		if (modelController != null && camera.enabled && modelController.IsCharaAdulation())
		{
			m_isCameraMyCharaAdulationbefore = true;
			Transform charaHead = modelController.GetCharaHead();
			if (charaHead != null)
			{
				Vector3 forward = charaHead.transform.position - base.transform.position;
				forward.y = 0f;
				Quaternion rotation = Quaternion.LookRotation(forward);
				base.transform.rotation = rotation;
				Vector3 eulerAngles = base.transform.eulerAngles;
				eulerAngles.x -= parentEngle.x;
				eulerAngles.z -= parentEngle.z;
				base.transform.eulerAngles = eulerAngles;
			}
		}
		else if (m_isCameraMyCharaAdulationbefore)
		{
			m_isCameraMyCharaAdulationbefore = false;
			base.transform.localRotation = ReserveCharaRotation;
		}
	}
}
