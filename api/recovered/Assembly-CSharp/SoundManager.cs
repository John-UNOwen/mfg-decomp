using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Konami.VirtualSurround;
using MFG.GameData;
using MFG.MasterGameData;
using MFG.Types;
using MFG.Types.Customize.CustomizeItem;
using TestModeSkeleton;
using TestModeSkeleton.CabinetDef;
using USnd;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;

public class SoundManager
{
	private static VirtualSurroundGain _virtualSurroundGain = null;

	private static SemaphoreSlim _playSemaphore = new SemaphoreSlim(1, 1);

	private static string _lastCalledMyVoiceLabel = "";

	private static List<CharaType> ExVoiceEnableCharas = new List<CharaType>
	{
		CharaType.Chara01,
		CharaType.Chara02,
		CharaType.Chara04,
		CharaType.Chara05,
		CharaType.Chara06,
		CharaType.Chara07,
		CharaType.Chara08,
		CharaType.Chara13,
		CharaType.Chara14,
		CharaType.Chara15,
		CharaType.Chara16,
		CharaType.Chara17,
		CharaType.Chara18,
		CharaType.Chara19,
		CharaType.Chara20,
		CharaType.Chara21
	};

	public static async UniTask Initialize(AudioMixer audioMixer, bool isCloudStreaming)
	{
		AudioManager.Initialize(0, isCloudStreaming);
		AudioManager.SetAudioMixer(audioMixer);
		AudioManager.LoadBinaryTable((Resources.Load("Sound/XML/MasterSettings") as TextAsset).bytes);
		AudioManager.LoadBinaryTable((Resources.Load("Sound/XML/CategorySettings") as TextAsset).bytes);
		AudioManager.LoadBinaryTable((Resources.Load("Sound/XML/LabelSettings") as TextAsset).bytes);
		if (true)
		{
			AudioManager.AddAudioClip((await Addressables.LoadAssetsAsync<AudioClip>("Sfx", null)).ToArray());
			AudioManager.AddAudioClip((await Addressables.LoadAssetsAsync<AudioClip>("SfxMjg", null)).ToArray());
			AudioManager.AddAudioClip((await Addressables.LoadAssetsAsync<AudioClip>("SfxSys", null)).ToArray());
		}
		AudioManager.SetAudioClipToLabelAll();
		_virtualSurroundGain = new VirtualSurroundGain
		{
			mixer = audioMixer
		};
	}

	public static void SetSpeakerVolume(float val)
	{
		if (val == 0f)
		{
			val = 0.0001f;
		}
		float num = (float)(20.0 * Math.Log10(val));
		if (SystemConfigGameData.CabinetType == CabinetType.UKS)
		{
			_virtualSurroundGain.ch1 = num;
			_virtualSurroundGain.ch2 = num;
			_virtualSurroundGain.ch3 = -80f;
		}
		else if (SystemConfigGameData.CabinetType == CabinetType.TBS || SystemConfigGameData.CabinetType == CabinetType.UJK)
		{
			_virtualSurroundGain.ch1 = num;
			_virtualSurroundGain.ch2 = num;
		}
		else
		{
			_virtualSurroundGain.ch3 = num;
			_virtualSurroundGain.ch4 = num;
		}
	}

	public static void SetHeadPhoneVolume(float val)
	{
		if (val == 0f)
		{
			val = 0.0001f;
		}
		float num = (float)(20.0 * Math.Log10(val));
		if (SystemConfigGameData.CabinetType == CabinetType.UKS)
		{
			_virtualSurroundGain.ch5 = num;
			_virtualSurroundGain.ch6 = num;
			_virtualSurroundGain.ch7 = num;
			_virtualSurroundGain.ch8 = num;
		}
		else if (SystemConfigGameData.CabinetType == CabinetType.TBS || SystemConfigGameData.CabinetType == CabinetType.UJK)
		{
			Bi2xInput.Instance.SetHeadPhoneVolume(val);
		}
		else
		{
			_virtualSurroundGain.ch1 = num;
			_virtualSurroundGain.ch2 = num;
		}
	}

	public static void SetAttractVolume(float val)
	{
		float volumeCurve = GetVolumeCurve(val);
		AudioManager.SetCategoryVolume("BGM", volumeCurve);
		AudioManager.SetCategoryVolume("SE", volumeCurve);
		AudioManager.SetCategoryVolume("VOICE", volumeCurve);
	}

	public static void SetUserHeadPhoneVolume(float val)
	{
		SetHeadPhoneVolume(val);
	}

	public static void SetDemoVolume()
	{
		float speakerVolume = (float)CabinetManager.Sound.Data.SoundVolume / (float)SoundOptionsDef.SoundVolumeSetting.Max;
		float attractVolume = (float)CabinetManager.Sound.Data.SoundVolumeInAttractMode / 100f;
		SetSpeakerVolume(speakerVolume);
		SetAttractVolume(attractVolume);
		SetHeadPhoneVolume(GameUtility.IsKonastaOrNetcafeBehavior ? 0.5f : 0f);
	}

	public static void SetGamePlayVolume()
	{
		SetSpeakerVolume((float)CabinetManager.Sound.Data.SoundVolume / (float)SoundOptionsDef.SoundVolumeSetting.Max);
		SetAttractVolume(1f);
		SetHeadPhoneVolume(GameUtility.IsKonastaOrNetcafeBehavior ? 0.5f : 0f);
	}

	public static async UniTask<int> PlayVoice(string label, float delay = -1f, bool isCancelPrevVoice = true, float pan = -2000f)
	{
		if (isCancelPrevVoice && IsPlay(_lastCalledMyVoiceLabel))
		{
			await StopVoice(_lastCalledMyVoiceLabel);
		}
		if (IsPlay(label))
		{
			await StopVoice(label);
		}
		await _playSemaphore.WaitAsync();
		try
		{
			_lastCalledMyVoiceLabel = label;
			await AudioManager.PrepareAudioClip(label);
			int num = await AudioManager.Play(label, delay, pan);
			AssertLabelError(label, num != AudioDefine.INSTANCE_ID_ERROR);
			return num;
		}
		finally
		{
			_playSemaphore.Release();
		}
	}

	public static async UniTask<int> PlayVoiceCha(string label, CharaType type, Cha cha, float delay = -1f)
	{
		string _label = GetCharaVoiceLabel(label, type);
		float volume = AudioDefine.VOLUME_MAX;
		float pan = 0f;
		if (cha == Cha.Jicha)
		{
			if (IsPlay(_lastCalledMyVoiceLabel))
			{
				await StopVoice(_lastCalledMyVoiceLabel);
			}
			_lastCalledMyVoiceLabel = label;
		}
		if (IsPlay(label))
		{
			await StopVoice(label);
		}
		await _playSemaphore.WaitAsync();
		try
		{
			switch (cha)
			{
			case Cha.Jicha:
				volume = AudioDefine.VOLUME_MAX;
				pan = 0f;
				break;
			case Cha.Simocha:
				volume = AudioDefine.VOLUME_MAX;
				pan = 0.7f;
				break;
			case Cha.Toimen:
				volume = AudioDefine.VOLUME_MAX;
				pan = 0f;
				break;
			case Cha.Kamicha:
				volume = AudioDefine.VOLUME_MAX;
				pan = -0.7f;
				break;
			}
			await AudioManager.PrepareAudioClip(_label);
			int num = await AudioManager.PlayOption(_label, volume, AudioDefine.DEFAULT_FADE, pan, AudioDefine.DEFAULT_PITCH, delay);
			AssertLabelError(label, num != AudioDefine.INSTANCE_ID_ERROR);
			return num;
		}
		finally
		{
			_playSemaphore.Release();
		}
	}

	public static async UniTask PlayVoiceCha(bool isPlay, string label, CharaType type, Cha cha, float delay = -1f)
	{
		if (isPlay)
		{
			await PlayVoiceCha(label, type, cha, delay);
		}
		else
		{
			await StopVoice(label);
		}
	}

	public static async UniTask<int> PlayVoice(string label, CharaType type, float delay = -1f, bool isCancelPrevVoice = true, float pan = -2000f)
	{
		return await PlayVoice(GetCharaVoiceLabel(label, type), delay, isCancelPrevVoice, pan);
	}

	public static async UniTask PlayVoice(bool isPlay, string label, CharaType type, float delay = -1f, float pan = -2000f)
	{
		if (isPlay)
		{
			await PlayVoice(label, type, delay, isCancelPrevVoice: true, pan);
		}
		else
		{
			await StopVoice(label);
		}
	}

	public static async UniTask<int> PlayMyCharaVoiceRandom(List<string> playVoices, float pan = -2000f)
	{
		int index = UnityEngine.Random.Range(0, playVoices.Count);
		return await PlayMyCharaVoice(playVoices[index], -1f, pan);
	}

	public static async UniTask<int> PlayMyCharaVoice(string label, float delay = -1f, float pan = -2000f)
	{
		return await PlayVoice(GetMyCharaVoiceLabel(label), delay, isCancelPrevVoice: true, pan);
	}

	public static async void PlayAllCharaVoice(string label, float delay = -1f, float pan = -2000f)
	{
		await _playSemaphore.WaitAsync();
		try
		{
			List<CharaType> m_charaTypes = new List<CharaType>();
			for (int i = 0; i < MahjongUtility.GetPlayerNum(GameDataManager.Mahjong.TakuType); i++)
			{
				CharaType selectChara = GameDataManager.Taikyoku.PlayerInfos[i].PlayerGameData.SelectChara;
				m_charaTypes.Add(selectChara);
			}
			foreach (CharaType item in m_charaTypes.Distinct())
			{
				await AudioManager.PrepareAudioClip(GetCharaVoiceLabel(label, item));
			}
			foreach (CharaType item2 in m_charaTypes.Distinct())
			{
				string _label = GetCharaVoiceLabel(label, item2);
				AssertLabelError(_label, await AudioManager.Play(_label, delay, pan) != AudioDefine.INSTANCE_ID_ERROR);
			}
		}
		finally
		{
			_playSemaphore.Release();
		}
	}

	public static string GetCharaVoiceLabel(string label, CharaType type)
	{
		if (type < CharaType.Chara01 || Enum.GetNames(typeof(CharaType)).Length <= (int)type)
		{
			return null;
		}
		label = GetExtraVoiceLabel(label, type);
		return label.Replace("GXX", $"G{(int)(type + 1):D2}");
	}

	public static string GetExtraVoiceLabel(string label, CharaType type)
	{
		if (GameUtility.IsNetcafeBehavior && ExVoiceEnableCharas.Contains(type))
		{
			switch (label)
			{
			case "VOX_GXX_MENU1":
			{
				int num = UnityEngine.Random.Range(0, 3);
				if (num != 0)
				{
					return label + $"N_{num + 1}";
				}
				return label + "N";
			}
			case "VOX_GXX_TOUCH_1":
			case "VOX_GXX_TOUCH_2":
			case "VOX_GXX_TOUCH_3":
			case "VOX_GXX_CONTINUE1":
			case "VOX_GXX_CONTINUE2":
			case "VOX_GXX_END1":
				return label + "N";
			}
		}
		return label;
	}

	public static string GetMyCharaVoiceLabel(string label)
	{
		CharaType selectChara = GameDataManager.Player.SelectChara;
		return GetCharaVoiceLabel(label, selectChara);
	}

	public static async UniTask StopVoiceAll(float fade_time = 0f)
	{
		await _playSemaphore.WaitAsync();
		try
		{
			await AudioManager.StopCategory("VOICE", fade_time);
		}
		finally
		{
			_playSemaphore.Release();
		}
	}

	public static async UniTask StopVoiceAllWithFadeout(float fade_time = 1f)
	{
		await _playSemaphore.WaitAsync();
		try
		{
			await AudioManager.StopCategory("VOICE", fade_time);
		}
		finally
		{
			_playSemaphore.Release();
		}
	}

	public static async UniTask StopVoice(string label, float fade_time = 0f)
	{
		await _playSemaphore.WaitAsync();
		try
		{
			await AudioManager.StopLabel(label, fade_time);
		}
		finally
		{
			_playSemaphore.Release();
		}
	}

	public static async UniTask StopVoice(string label, CharaType charaType, float fade_time = 0f)
	{
		await _playSemaphore.WaitAsync();
		try
		{
			await AudioManager.StopLabel(GetCharaVoiceLabel(label, charaType), fade_time);
		}
		finally
		{
			_playSemaphore.Release();
		}
	}

	public static async UniTask StopVoiceWithFadeout(string label, float fadeSec = 1f)
	{
		await _playSemaphore.WaitAsync();
		try
		{
			await AudioManager.StopLabel(label, fadeSec);
		}
		finally
		{
			_playSemaphore.Release();
		}
	}

	public static async UniTask<int> PlaySe(string label, float delay = -1f, float pan = -2000f)
	{
		await _playSemaphore.WaitAsync();
		try
		{
			await AudioManager.PrepareAudioClip(label);
			int num = await AudioManager.PlayOption(label, AudioDefine.DEFAULT_VOLUME, AudioDefine.DEFAULT_FADE, pan, AudioDefine.DEFAULT_PITCH, delay);
			AssertLabelError(label, num != AudioDefine.INSTANCE_ID_ERROR);
			return num;
		}
		finally
		{
			_playSemaphore.Release();
		}
	}

	public static async UniTask<int> PlaySeCha(string label, Cha cha, float delay = -1f)
	{
		float pan = 1f;
		switch (cha)
		{
		case Cha.Jicha:
			pan = 0f;
			break;
		case Cha.Simocha:
			pan = 0.7f;
			break;
		case Cha.Toimen:
			pan = 0f;
			break;
		case Cha.Kamicha:
			pan = -0.7f;
			break;
		}
		return await PlaySe(label, delay, pan);
	}

	public static async UniTask PlaySe(bool isPlay, string label, float delay = -1f, float pan = -2000f)
	{
		if (isPlay)
		{
			await PlaySe(label, delay, pan);
		}
		else
		{
			await StopSe(label);
		}
	}

	public static async UniTask StopSeAll(float fade_time = 0f)
	{
		await _playSemaphore.WaitAsync();
		try
		{
			await AudioManager.StopCategory("SE", fade_time);
		}
		finally
		{
			_playSemaphore.Release();
		}
	}

	public static async UniTask StopSe(string label, float fade_time = 0f)
	{
		await _playSemaphore.WaitAsync();
		try
		{
			await AudioManager.StopLabel(label, fade_time);
		}
		finally
		{
			_playSemaphore.Release();
		}
	}

	public static async UniTask StopSeWithFadeout(string label, float fadeSec = 1f)
	{
		await _playSemaphore.WaitAsync();
		try
		{
			await AudioManager.StopLabel(label, fadeSec);
		}
		finally
		{
			_playSemaphore.Release();
		}
	}

	public static async UniTask<int> PlayBgm(string label, float delay = -1f)
	{
		await _playSemaphore.WaitAsync();
		try
		{
			if (IsPlay(label))
			{
				return 0;
			}
			await AudioManager.PrepareAudioClip(label);
			int num = await AudioManager.Play(label, delay);
			AssertLabelError(label, num != AudioDefine.INSTANCE_ID_ERROR);
			return num;
		}
		finally
		{
			_playSemaphore.Release();
		}
	}

	public static async UniTask<int> PlayMyCharaMyRoomBGM(float delay = -1f, bool isPreview = false)
	{
		return await PlayBgm(GameDataManager.Player.GetCurrentChara().CustomizeItemEquip_GetItemAtRandom(Category.MyRoomBgm).MyRoomToMyRoomBgmLabel(isPreview), delay);
	}

	public static async UniTask<int> PlayMusicGachaBGM(GachaSeriesServerInfo info)
	{
		return await PlayBgm(CustomizeItemMaster.ToItemID(info.GetSortedMusicItems()[1].oid).ReachBgmToBgmLabel());
	}

	public static async UniTask StopBgm()
	{
		await _playSemaphore.WaitAsync();
		try
		{
			await AudioManager.StopCategory("BGM");
		}
		finally
		{
			_playSemaphore.Release();
		}
	}

	public static async UniTask PlayBgm(bool isPlay, string label, float delay = -1f)
	{
		if (isPlay)
		{
			await PlayBgm(label, delay);
		}
		else
		{
			await StopBgm();
		}
	}

	public static async UniTask StopBgmWithFadeoutDelay(float delay, double fadeSec = 1.0)
	{
		await UniTask.Delay(TimeSpan.FromSeconds(delay));
		await StopBgmWithFadeout(fadeSec);
	}

	public static async UniTask StopBgmWithFadeout(double fadeSec = 1.0)
	{
		await _playSemaphore.WaitAsync();
		try
		{
			await AudioManager.StopCategory("BGM", (float)fadeSec);
		}
		finally
		{
			_playSemaphore.Release();
		}
	}

	public static async UniTask PauseBgmWithFadeout(double fadeSec)
	{
		await _playSemaphore.WaitAsync();
		try
		{
			await AudioManager.OnPauseCategory("BGM", (float)fadeSec);
		}
		finally
		{
			_playSemaphore.Release();
		}
	}

	public static async UniTask UnpauseBgmWithFadein(double fadeSec)
	{
		await _playSemaphore.WaitAsync();
		try
		{
			await AudioManager.OffPauseCategory("BGM", (float)fadeSec);
		}
		finally
		{
			_playSemaphore.Release();
		}
	}

	public static bool IsItLoaded(string label)
	{
		bool result = false;
		string[] labelNameList = AudioManager.GetLabelNameList();
		if (labelNameList != null)
		{
			for (int i = 0; i < labelNameList.Length; i++)
			{
				if (labelNameList[i] == label)
				{
					result = true;
				}
			}
		}
		return result;
	}

	private static float GetVolumeCurve(float fval)
	{
		return (float)((Math.Pow(10.0, fval) - 1.0) / 9.0);
	}

	public static bool IsPlay(string label)
	{
		return AudioManager.IsPlayingLabel(label);
	}

	public static bool IsPlay(int instanceID)
	{
		if (AudioManager.GetInstanceStatus(instanceID) == AudioDefine.INSTANCE_STATUS.PLAY)
		{
			return true;
		}
		return false;
	}

	public static bool IsPlayLastCalledVoice()
	{
		return IsPlay(_lastCalledMyVoiceLabel);
	}

	public static string GetLastCalledMyVoiceLabel()
	{
		return _lastCalledMyVoiceLabel;
	}

	public static AudioDefine.INSTANCE_STATUS GetStatus(int instanceID)
	{
		return AudioManager.GetInstanceStatus(instanceID);
	}

	public static async UniTask StopAll()
	{
		await AudioManager.StopAll(0f);
		Debug.Log("Stop All -> called.");
	}

	private static void AssertLabelError(string label, bool condition)
	{
		if (condition)
		{
			if (DebugFlag.IsActive(DebugFlag.Flag.SeCallLog))
			{
				Debug.Log("SE:<color=cyan>" + label + " called</color>");
			}
		}
		else
		{
			Debug.Log("SE:<color=red>" + label + " is not found!</color>");
		}
	}
}
