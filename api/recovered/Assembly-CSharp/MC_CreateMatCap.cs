using UnityEngine;

[ExecuteInEditMode]
public class MC_CreateMatCap : MonoBehaviour
{
	public Camera screenshotCamera;

	public Material previewMaterial;

	[Tooltip("View the saved PNG in the file browser on save")]
	public bool revealOnSave = true;
}
