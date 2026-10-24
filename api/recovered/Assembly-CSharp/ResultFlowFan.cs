using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MFG.Assets;
using MFG.GameData;
using MFG.Types;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ResultFlowFan : MonoBehaviour
{
	[SerializeField]
	private GameObject m_resultAll;

	[SerializeField]
	private GameObject m_infoAll;

	[SerializeField]
	private GameObject m_popset;

	[SerializeField]
	private GameObject m_fanTotal;

	[SerializeField]
	private GameObject m_fanCharaSd;

	[SerializeField]
	private GameObject m_fanInfo;

	[SerializeField]
	private GameObject m_fanBonus;

	[SerializeField]
	private List<GameObject> m_fanManPrefabs = new List<GameObject>();

	[SerializeField]
	private List<GameObject> m_fanWomanPrefabs = new List<GameObject>();

	[SerializeField]
	private List<GameObject> m_fanPrefabs = new List<GameObject>();

	[SerializeField]
	private GameObject m_fanParent;

	[SerializeField]
	private List<GameObject> m_addNumObjs = new List<GameObject>();

	[SerializeField]
	private List<GameObject> m_add2NumObjs = new List<GameObject>();

	private float m_animeFrame = 7f;

	private float m_countUpFrame = 7f;

	[SerializeField]
	private int FanObjectMax = 30;

	private int[] m_fansDisp = new int[4];

	private int[] m_fansOriginal = new int[4];

	private int[] m_addFansDisp = new int[4];

	private int[] m_addFansOriginal = new int[4];

	private int[] m_addFansBonus = new int[4];

	private float m_bonusRatio = 1f;

	private List<FanType> m_instantiateFanTypes = new List<FanType>();

	private bool m_isExit;

	public CancellationTokenSource m_cancellationTokenSource;

	public CancellationToken m_cancellationToken;

	private int[] m_instantiatedFans = new int[4];

	private async void Start()
	{
	}

	private void Update()
	{
		RefleshText();
	}

	public async UniTask StartScene(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		await Init(cancellationToken);
		m_infoAll.transform.Find("FanAll").Find("LimitMessage").gameObject.SetActive(!GameDataManager.Player.GetCurrentChara(IsGetMyCharaData: true).IsDebut);
		m_infoAll.GetComponent<Animator>().SetTrigger("In2");
		m_fanCharaSd.GetComponent<Animator>().SetTrigger("In");
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(2.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResultFlow.Skip(), PlayerLoopTiming.Update, cancellationToken));
		int num = 0;
		int[] addFansOriginal = m_addFansOriginal;
		foreach (int num2 in addFansOriginal)
		{
			num += num2;
		}
		if (num > 0)
		{
			SoundManager.PlayMyCharaVoiceRandom(new List<string> { "VOX_GXX_FUN_1", "VOX_GXX_FUN_2", "VOX_GXX_FUN_3" }).Forget();
		}
		await FanBonus(cancellationToken);
		await CountUp(cancellationToken);
		await UniTask.Delay(TimeSpan.FromSeconds(0.20000000298023224));
		ResidentFlow.GetInstance().GetTimer().SetTimer((GameUtility.IsAppTerminatDecided || Cloudstrap.IsDisconnect) ? 5 : 20, disp: true);
		ResidentFlow.GetInstance().GetTimer().StartTimer();
		await UniTask.WhenAny(UniTask.WaitUntil(() => ResidentFlow.GetInstance().GetTimer().IsEnd()), UniTask.WaitUntil(() => ResultFlow.Skip()));
		ResidentFlow.GetInstance().GetTimer().ResetTimer(disp: false);
		m_resultAll.GetComponent<Animator>().SetTrigger("Out");
		CharaGameData currentChara = GameDataManager.Player.GetCurrentChara(IsGetMyCharaData: true);
		for (int j = 0; j < Enum.GetValues(typeof(FanType)).Length; j++)
		{
			currentChara.m_fans[j] = m_fansDisp[j];
		}
	}

	private async UniTask Init(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		CharaGameData currentChara = GameDataManager.Player.GetCurrentChara(IsGetMyCharaData: true);
		m_fansOriginal[0] = currentChara.m_fans[0];
		m_fansOriginal[1] = currentChara.m_fans[1];
		m_fansOriginal[2] = currentChara.m_fans[2];
		m_fansOriginal[3] = currentChara.m_fans[3];
		m_addFansOriginal[0] = Mathf.RoundToInt(GameDataManager.Mahjong.Fans.luck);
		m_addFansOriginal[1] = Mathf.RoundToInt(GameDataManager.Mahjong.Fans.speed);
		m_addFansOriginal[2] = Mathf.RoundToInt(GameDataManager.Mahjong.Fans.attack);
		m_addFansOriginal[3] = Mathf.RoundToInt(GameDataManager.Mahjong.Fans.defence);
		GameDataManager.Mahjong.Fans = (luck: 0f, speed: 0f, attack: 0f, defence: 0f);
		if (GameDataManager.FanBonusEventEnable())
		{
			m_bonusRatio = GameDataManager.GetFanBonusRatio();
		}
		for (int j = 0; j < Enum.GetValues(typeof(FanType)).Length; j++)
		{
			m_fansDisp[j] = m_fansOriginal[j];
			m_addFansDisp[j] = m_addFansOriginal[j];
			if (GameDataManager.FanBonusEventEnable())
			{
				m_addFansBonus[j] = (int)Math.Round(m_bonusRatio * (float)m_addFansOriginal[j]) - m_addFansOriginal[j];
			}
		}
		for (int k = 0; k < Enum.GetValues(typeof(FanType)).Length; k++)
		{
			for (int l = 0; l < m_addFansDisp[k] + m_addFansBonus[k]; l++)
			{
				m_instantiateFanTypes.Add((FanType)k);
			}
		}
		m_instantiateFanTypes = m_instantiateFanTypes.OrderBy((FanType i) => Guid.NewGuid()).ToList();
		int num = 0;
		for (int m = 0; m < Enum.GetValues(typeof(FanType)).Length; m++)
		{
			num += Math.Min(m_addFansDisp[m] + m_addFansBonus[m], FanObjectMax);
		}
		if (num > 0 && m_animeFrame / 60f * (float)num > 3f)
		{
			m_animeFrame = 180f / (float)num;
		}
		if (m_countUpFrame / 60f * (float)m_instantiateFanTypes.Count > 3f)
		{
			m_countUpFrame = 180f / (float)m_instantiateFanTypes.Count;
		}
		IDisposableAsset<Sprite> disposableAsset = await AssetLoader.LoadAssetAsync<Sprite>(AssetPathResolver.Resolve(PathType.Result, GameDataManager.Player.SelectChara, ResultAssetType.SdCharaTexture));
		disposableAsset.AddTo(this);
		m_fanCharaSd.transform.Find("Move").Find("CharaSd").GetComponent<Image>()
			.sprite = disposableAsset.Asset;
		for (int n = 0; n < Enum.GetValues(typeof(FanType)).Length; n++)
		{
			SetText(m_add2NumObjs[n], m_addFansOriginal[n]);
			m_add2NumObjs[n].SetActive(value: false);
		}
	}

	private async UniTask FanBonus(CancellationToken cancellationToken)
	{
		if (!GameDataManager.FanBonusEventEnable() || !m_addFansBonus.ToList().Exists((int x) => x != 0))
		{
			return;
		}
		cancellationToken.ThrowIfCancellationRequested();
		m_cancellationTokenSource = new CancellationTokenSource();
		m_cancellationToken = m_cancellationTokenSource.Token;
		m_fanBonus.SetActive(value: true);
		m_fanBonus.transform.Find("Banner").Find("Num").GetComponent<TextMeshProUGUI>()
			.text = m_bonusRatio.ToString();
		m_fanBonus.GetComponent<Animator>().SetTrigger("In1");
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResultFlow.Skip(), PlayerLoopTiming.Update, cancellationToken));
		float startTime = Time.time;
		await SoundManager.PlaySe("SFX_SYS_METER1");
		while (Time.time - startTime < 1f && !ResultFlow.Skip())
		{
			await UniTask.NextFrame();
			float num = Time.time - startTime;
			if (num >= 1f)
			{
				num = 1f;
			}
			for (int i = 0; i < Enum.GetValues(typeof(FanType)).Length; i++)
			{
				m_addFansDisp[i] = (int)Math.Round((float)m_addFansBonus[i] * num) + m_addFansOriginal[i];
				SetText(m_addNumObjs[i], m_addFansDisp[i]);
				SetText(m_add2NumObjs[i], m_addFansDisp[i]);
			}
		}
		for (int j = 0; j < Enum.GetValues(typeof(FanType)).Length; j++)
		{
			m_addFansDisp[j] = m_addFansOriginal[j] + m_addFansBonus[j];
			m_addFansOriginal[j] = m_addFansDisp[j];
			SetText(m_addNumObjs[j], m_addFansDisp[j]);
			SetText(m_add2NumObjs[j], m_addFansOriginal[j]);
		}
		SoundManager.StopSe("SFX_SYS_METER1").Forget();
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResultFlow.Skip(), PlayerLoopTiming.Update, cancellationToken));
		m_fanBonus.GetComponent<Animator>().SetTrigger("In2");
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResultFlow.Skip(), PlayerLoopTiming.Update, cancellationToken));
	}

	private async UniTask CountUp(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		m_cancellationTokenSource = new CancellationTokenSource();
		m_cancellationToken = m_cancellationTokenSource.Token;
		m_isExit = false;
		for (int i = 0; i < m_instantiateFanTypes.Count; i++)
		{
			InstantiateFanPrefab(m_instantiateFanTypes[i], i, m_cancellationToken).Forget();
			FanNumCountUp(m_instantiateFanTypes[i], i, i == m_instantiateFanTypes.Count - 1, m_cancellationToken).Forget();
		}
		if (m_instantiateFanTypes.Count == 0)
		{
			m_isExit = true;
		}
		await UniTask.WhenAny(UniTask.WaitUntil(() => m_isExit, PlayerLoopTiming.Update, m_cancellationToken), UniTask.WaitUntil(() => ResultFlow.Skip(), PlayerLoopTiming.Update, m_cancellationToken));
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(1.3333333730697632), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResultFlow.Skip(), PlayerLoopTiming.Update, cancellationToken));
		m_cancellationTokenSource.Cancel();
		foreach (Transform item in m_fanParent.transform)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		bool flag = false;
		for (int j = 0; j < Enum.GetValues(typeof(FanType)).Length; j++)
		{
			m_addFansDisp[j] = 0;
			m_fansDisp[j] = m_fansOriginal[j] + m_addFansOriginal[j];
			m_fansDisp[j] = Math.Min(m_fansDisp[j], GameDataManager.Player.GetCurrentChara(IsGetMyCharaData: true).MaxFan);
			if (m_fansDisp[j] == GameDataManager.Player.GetCurrentChara(IsGetMyCharaData: true).MaxFan)
			{
				flag = true;
			}
		}
		if (flag && !GameDataManager.PlayerConfig.IsDispHelpResultFanLimit)
		{
			GameDataManager.PlayerConfig.IsDispHelpResultFanLimit = true;
			await FanInfoDisp(cancellationToken);
		}
	}

	private async UniTask InstantiateFanPrefab(FanType type, int index, CancellationToken token = default(CancellationToken))
	{
		if (m_instantiatedFans[(int)type] >= FanObjectMax)
		{
			return;
		}
		m_instantiatedFans[(int)type]++;
		GameObject obj = UnityEngine.Object.Instantiate(m_fanPrefabs[UnityEngine.Random.Range(0, 3)], m_fanParent.transform);
		foreach (Transform item in obj.transform.Find("AniParent"))
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		if (UnityEngine.Random.Range(0, 2) == 0)
		{
			UnityEngine.Object.Instantiate(m_fanManPrefabs[(int)type], obj.transform.Find("AniParent"));
		}
		else
		{
			UnityEngine.Object.Instantiate(m_fanWomanPrefabs[(int)type], obj.transform.Find("AniParent"));
		}
		int num = m_instantiatedFans.Sum();
		await UniTask.Delay(TimeSpan.FromSeconds(m_animeFrame / 60f * (float)num), ignoreTimeScale: false, PlayerLoopTiming.Update, token);
		if (!token.IsCancellationRequested)
		{
			obj.GetComponent<Animator>().SetTrigger("In");
		}
	}

	private async UniTask FanNumCountUp(FanType type, int index, bool isLast, CancellationToken token = default(CancellationToken))
	{
		await UniTask.Delay(TimeSpan.FromSeconds(m_countUpFrame / 60f * (float)index), ignoreTimeScale: false, PlayerLoopTiming.Update, token);
		if (token.IsCancellationRequested)
		{
			return;
		}
		await UniTask.Delay(TimeSpan.FromSeconds(1.3333333730697632), ignoreTimeScale: false, PlayerLoopTiming.Update, token);
		if (!token.IsCancellationRequested)
		{
			m_fanTotal.GetComponent<Animator>().SetTrigger("In");
			m_addFansDisp[(int)type]--;
			m_fansDisp[(int)type]++;
			m_fansDisp[(int)type] = Math.Min(m_fansDisp[(int)type], GameDataManager.Player.GetCurrentChara(IsGetMyCharaData: true).MaxFan);
			if (isLast)
			{
				m_isExit = true;
			}
		}
	}

	private void RefleshText()
	{
		for (int i = 0; i < Enum.GetValues(typeof(FanType)).Length; i++)
		{
			SetText(m_addNumObjs[i], m_addFansDisp[i]);
			if (m_addFansDisp[i] == 0 && !m_add2NumObjs[i].activeSelf)
			{
				m_addNumObjs[i].SetActive(value: false);
				m_add2NumObjs[i].SetActive(value: true);
			}
		}
		m_infoAll.transform.Find("FanAll").Find("Total").Find("Num1")
			.GetComponent<Text>()
			.text = m_fansDisp[0].ToString("N0");
		m_infoAll.transform.Find("FanAll").Find("Total").Find("Num2")
			.GetComponent<Text>()
			.text = m_fansDisp[1].ToString("N0");
		m_infoAll.transform.Find("FanAll").Find("Total").Find("Num3")
			.GetComponent<Text>()
			.text = m_fansDisp[2].ToString("N0");
		m_infoAll.transform.Find("FanAll").Find("Total").Find("Num4")
			.GetComponent<Text>()
			.text = m_fansDisp[3].ToString("N0");
		int num = 0;
		for (int j = 0; j < Enum.GetValues(typeof(FanType)).Length; j++)
		{
			num += m_fansDisp[j];
		}
		m_infoAll.transform.Find("FanAll").Find("FanTotal").Find("Num")
			.GetComponent<TextMeshProUGUI>()
			.text = num.ToString("N0");
	}

	private void SetText(GameObject obj, int fanNum)
	{
		Text component = obj.GetComponent<Text>();
		if (fanNum > 0)
		{
			component.text = "＋" + fanNum.ToString("N0");
		}
		else
		{
			component.text = fanNum.ToString("N0");
		}
	}

	private async UniTask FanInfoDisp(CancellationToken cancellationToken = default(CancellationToken))
	{
		float timeLimit = ((GameUtility.IsAppTerminatDecided || Cloudstrap.IsDisconnect) ? 5 : 20);
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: true);
		m_fanInfo.SetActive(value: true);
		m_fanInfo.GetComponent<Animator>().SetTrigger("Start");
		await UniTask.Delay(TimeSpan.FromSeconds(2.0), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
		await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(timeLimit), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken), UniTask.WaitUntil(() => ResidentFlow.CustomInput.GetMouseButtonDown(0), PlayerLoopTiming.Update, cancellationToken));
		m_fanInfo.GetComponent<Animator>().SetTrigger("Skip");
		m_fanInfo.SetActive(value: false);
		ResidentFlow.GetInstance().GetTimer().SetPause(pause: false);
	}
}
