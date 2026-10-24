using System;
using System.Collections.Generic;
using MFG.Types;

[Serializable]
public class Tsumikomi
{
	public List<List<Pai>> YonmaTiles = new List<List<Pai>>();

	public List<Pai> TonPais;

	public List<Pai> NanPais;

	public List<Pai> ShaPais;

	public List<Pai> PePais;

	public List<Pai> Tsumo0Pais;

	public List<Pai> Tsumo1Pais;

	public List<Pai> Tsumo2Pais;

	public List<Pai> Tsumo3Pais;

	public List<Pai> Tsumo4Pais;

	public List<Pai> WanPais;

	public Tsumikomi(List<List<Pai>> pais)
	{
		YonmaTiles = pais;
	}

	public void YonmaTilesToPais()
	{
		TonPais = YonmaTiles[0];
		NanPais = YonmaTiles[1];
		ShaPais = YonmaTiles[2];
		PePais = YonmaTiles[3];
		Tsumo0Pais = YonmaTiles[4];
		Tsumo1Pais = YonmaTiles[5];
		Tsumo2Pais = YonmaTiles[6];
		Tsumo3Pais = YonmaTiles[7];
		Tsumo4Pais = YonmaTiles[8];
		WanPais = YonmaTiles[9];
	}

	public void PaisToYonmaTiles()
	{
		YonmaTiles.Clear();
		YonmaTiles.Add(TonPais);
		YonmaTiles.Add(NanPais);
		YonmaTiles.Add(ShaPais);
		YonmaTiles.Add(PePais);
		YonmaTiles.Add(Tsumo0Pais);
		YonmaTiles.Add(Tsumo1Pais);
		YonmaTiles.Add(Tsumo2Pais);
		YonmaTiles.Add(Tsumo3Pais);
		YonmaTiles.Add(Tsumo4Pais);
		YonmaTiles.Add(WanPais);
	}

	public Tsumikomi Clone()
	{
		return (Tsumikomi)MemberwiseClone();
	}
}
