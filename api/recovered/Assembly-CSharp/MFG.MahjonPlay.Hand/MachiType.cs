using System;

namespace MFG.MahjonPlay.Hand;

[Flags]
public enum MachiType
{
	None = 0,
	ShantenUp = 1,
	Ryanmen = 2,
	Shabo = 4,
	Kanchan = 8,
	Penchan = 0x10,
	Tanki = 0x20,
	Other = 0x40
}
