using System.Linq;
using UnityEngine;

namespace Game.Core.Keyboard.ViewController;

public class KeyboardLayout : MonoBehaviour
{
	[SerializeField]
	public KeyboardLayoutType type;

	[SerializeField]
	public KeyDefinition[] KeyList;

	public bool HasEnableKey(InputMode inputMode)
	{
		return KeyList.Any((KeyDefinition o) => o.IsEnable(inputMode));
	}
}
