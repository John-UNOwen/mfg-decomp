using System;

// Off by default; turned on by the -LOGDATA launch argument.
// The game logs every GameData Save/Load as a full JSON dump (PlayerGameData and
// CustomizeGameData are ~8 KB each), plus every matching push and per-frame boot
// progress. During multiplayer matching updates that reaches ~16 full state dumps
// per second - the string building alone drops frames. Keep those logs available
// for diagnostics behind the flag, silent on the shipped lane.
public static class VerboseLog
{
	public static readonly bool Data = Array.Exists(Environment.GetCommandLineArgs(), a => a == "-LOGDATA");
}
