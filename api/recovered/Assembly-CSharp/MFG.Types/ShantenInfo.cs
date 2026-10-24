using System;
using System.Collections.Generic;
using MFG.MahjonPlay.Hand;

namespace MFG.Types;

[Serializable]
public class ShantenInfo
{
	public int Shanten = 13;

	public ShantenDifference ShantenDifference;

	public List<HaiID> ShantenUpHais = new List<HaiID>();

	public MachiType MachiType;

	public int NormalHandShanten = 13;

	public int ChitoiHandShanten = 13;

	public int KokushiHandShanten = 13;
}
