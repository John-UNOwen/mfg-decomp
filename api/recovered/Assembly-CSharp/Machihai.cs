using MFG.Types;
using UnityEngine;
using UnityEngine.UI;

public class Machihai : MonoBehaviour
{
	public Pai m_pai;

	public void SetPai(Pai pai)
	{
		GetComponent<Image>().sprite = TileManager.GetInstance().GetTileSprite(pai);
		m_pai = pai;
	}

	public void Reflesh()
	{
		GetComponent<Image>().sprite = TileManager.GetInstance().GetTileSprite(m_pai);
	}
}
