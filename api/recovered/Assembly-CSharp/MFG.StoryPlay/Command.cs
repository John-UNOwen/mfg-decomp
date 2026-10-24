using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MFG.StoryPlay;

public class Command
{
	public CommandType type;

	public string charaName;

	public float sec;

	public bool enable1 = true;

	public bool enable2 = true;

	public string targetName;

	public bool wait;

	public string label;

	public int LineNo;

	public CancellationToken token;

	public virtual async UniTask Parse(string[] values)
	{
		Debug.Log($"LineNo:{LineNo} type:{type} Parse");
		label = values[27];
	}

	public virtual async UniTask Exec()
	{
	}

	public virtual void ForceDecideChoice()
	{
	}
}
