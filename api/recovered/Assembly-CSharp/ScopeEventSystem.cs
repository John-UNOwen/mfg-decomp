using UnityEngine;
using UnityEngine.EventSystems;

[AddComponentMenu("Event/Scope Event System")]
[DisallowMultipleComponent]
public class ScopeEventSystem : EventSystem
{
	protected override void OnApplicationFocus(bool hasFocus)
	{
	}
}
