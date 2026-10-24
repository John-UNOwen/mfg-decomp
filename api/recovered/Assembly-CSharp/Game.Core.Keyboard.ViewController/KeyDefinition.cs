using System;

namespace Game.Core.Keyboard.ViewController;

[Serializable]
public struct KeyDefinition
{
	public KeyOperationType operationType;

	public string paramter;

	public string displayText;

	public InputMode inputMode;

	public bool IsEnable(InputMode inputMode)
	{
		if ((this.inputMode & inputMode) == this.inputMode)
		{
			return this.inputMode != InputMode.None;
		}
		return false;
	}
}
