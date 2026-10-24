using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class Help : MonoBehaviour
{
	[SerializeField]
	private GameObject m_helpOption;

	[SerializeField]
	private Button m_helpOptionIconButton;

	[SerializeField]
	private Button m_helpOptionExplainButton;

	[SerializeField]
	private GameObject m_helpNavi;

	[SerializeField]
	private Button m_helpNaviIconButton;

	[SerializeField]
	private Button m_helpNaviExplainButton;

	[SerializeField]
	private GameObject m_helpSyanten;

	[SerializeField]
	private Button m_helpSyantenIconButton;

	[SerializeField]
	private Button m_helpSyantenExplainButton;

	[SerializeField]
	private GameObject m_helpAnpai;

	[SerializeField]
	private Button m_helpAnpaiIconButton;

	[SerializeField]
	private Button m_helpAnpaiExplainButton;

	private void Start()
	{
		m_helpOptionIconButton.OnClickAsObservable().Subscribe(delegate
		{
			m_helpOption.GetComponent<Animator>().SetTrigger("ExplainIn");
		}).AddTo(this);
		m_helpOptionExplainButton.OnClickAsObservable().Subscribe(delegate
		{
			m_helpOption.GetComponent<Animator>().SetTrigger("Out");
		}).AddTo(this);
		m_helpNaviIconButton.OnClickAsObservable().Subscribe(delegate
		{
			m_helpNavi.GetComponent<Animator>().SetTrigger("ExplainIn");
		}).AddTo(this);
		m_helpNaviExplainButton.OnClickAsObservable().Subscribe(delegate
		{
			m_helpNavi.GetComponent<Animator>().SetTrigger("Out");
		}).AddTo(this);
		m_helpSyantenIconButton.OnClickAsObservable().Subscribe(delegate
		{
			m_helpSyanten.GetComponent<Animator>().SetTrigger("ExplainIn");
		}).AddTo(this);
		m_helpSyantenExplainButton.OnClickAsObservable().Subscribe(delegate
		{
			m_helpSyanten.GetComponent<Animator>().SetTrigger("Out");
		}).AddTo(this);
		m_helpAnpaiIconButton.OnClickAsObservable().Subscribe(delegate
		{
			m_helpAnpai.GetComponent<Animator>().SetTrigger("ExplainIn");
		}).AddTo(this);
		m_helpAnpaiExplainButton.OnClickAsObservable().Subscribe(delegate
		{
			m_helpAnpai.GetComponent<Animator>().SetTrigger("Out");
		}).AddTo(this);
	}

	public void IconInOption()
	{
		if (!GameDataManager.Mahjong.IsHaifuPlay && !GameDataManager.PlayerConfig.IsDispHelpOption)
		{
			m_helpOption.SetActive(value: true);
			m_helpOption.GetComponent<Animator>().SetTrigger("IconIn");
			GameDataManager.PlayerConfig.IsDispHelpOption = true;
		}
	}

	public void IconInNavi()
	{
		if (!GameDataManager.Mahjong.IsHaifuPlay && !GameDataManager.PlayerConfig.IsDispHelpNavi)
		{
			m_helpNavi.SetActive(value: true);
			m_helpNavi.GetComponent<Animator>().SetTrigger("IconIn");
			GameDataManager.PlayerConfig.IsDispHelpNavi = true;
		}
	}

	public void IconInSyanten()
	{
		if (!GameDataManager.Mahjong.IsHaifuPlay && !GameDataManager.PlayerConfig.IsDispHelpSyanten)
		{
			m_helpSyanten.SetActive(value: true);
			m_helpSyanten.GetComponent<Animator>().SetTrigger("ExplainIn");
			GameDataManager.PlayerConfig.IsDispHelpSyanten = true;
		}
	}

	public void IconInAnpai()
	{
		if (!GameDataManager.Mahjong.IsHaifuPlay && !GameDataManager.PlayerConfig.IsDispHelpAnpai)
		{
			m_helpAnpai.SetActive(value: true);
			m_helpAnpai.GetComponent<Animator>().SetTrigger("IconIn");
			GameDataManager.PlayerConfig.IsDispHelpAnpai = true;
		}
	}

	private void Update()
	{
	}
}
