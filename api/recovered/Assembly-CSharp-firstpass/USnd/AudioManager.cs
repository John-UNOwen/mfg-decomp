using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;

namespace USnd;

public class AudioManager : MonoBehaviour
{
	[Serializable]
	public class AudioCategorySettings
	{
		public string categoryName;

		public int maxPlaybacksNum;

		public float volume = 1f;

		public string masterName;

		private AudioMasterSettings attachMaster;

		private AudioParamUpdater volumeUpdater = new AudioParamUpdater();

		private AudioParamUpdater duckingUpdater = new AudioParamUpdater();

		public float programVolume = 1f;

		public float duckingVolume = 1f;

		public AudioCategorySettings()
		{
			programVolume = 1f;
			duckingVolume = 1f;
		}

		public void CopySettings(AudioCategorySettings src)
		{
			if (categoryName.CompareTo(src.categoryName) == 0)
			{
				volume = src.volume;
				maxPlaybacksNum = src.maxPlaybacksNum;
				masterName = src.masterName;
			}
		}

		public void SetAttachMasterInstance(AudioMasterSettings master)
		{
			attachMaster = master;
		}

		public float GetVolumeFactor()
		{
			if (attachMaster == null)
			{
				return programVolume;
			}
			return volume * programVolume * duckingVolume * attachMaster.GetVolumeFactor();
		}

		public float GetCurrentVolume()
		{
			return programVolume;
		}

		public void SetVolumeUpdater(float start, float target, float time)
		{
			volumeUpdater.SetParam(start, target, time, isLow: false);
		}

		public void SetDuckingVolumeUpdater(float target, float time, bool isLow)
		{
			duckingUpdater.SetParam(duckingVolume, target, time, isLow);
		}

		public void ClearVolumeUpdater()
		{
			volumeUpdater.Clear();
		}

		public bool UpdateVolume()
		{
			if (volumeUpdater.active)
			{
				programVolume = volumeUpdater.Update();
				return true;
			}
			return false;
		}

		public bool UpdateDuckingVolume()
		{
			if (duckingUpdater.active)
			{
				duckingVolume = duckingUpdater.Update();
				return true;
			}
			return false;
		}
	}

	private class AudioDebugLog
	{
		private static bool isActive;

		public static void Break()
		{
			if (IsEnable())
			{
				Debug.Break();
			}
		}

		public static void Log(object message)
		{
			if (IsEnable())
			{
				Debug.Log(message);
			}
		}

		public static void Log(object message, UnityEngine.Object context)
		{
			if (IsEnable())
			{
				Debug.Log(message, context);
			}
		}

		public static void LogError(object message)
		{
			if (IsEnable())
			{
				Debug.LogError(message);
			}
		}

		public static void LogError(object message, UnityEngine.Object context)
		{
			if (IsEnable())
			{
				Debug.LogError(message, context);
			}
		}

		public static void LogWarning(object message)
		{
			if (IsEnable())
			{
				Debug.LogWarning(message);
			}
		}

		public static void LogWarning(object message, UnityEngine.Object context)
		{
			if (IsEnable())
			{
				Debug.LogWarning(message, context);
			}
		}

		private static bool IsEnable()
		{
			if (Debug.isDebugBuild)
			{
				return isActive;
			}
			return false;
		}
	}

	private class AudioInstance
	{
		private enum FADE_END_STATE
		{
			UNSET,
			PAUSE,
			STOP
		}

		private bool isAudioDebug;

		private AudioLabelSettings setting;

		private AudioSource source;

		private Transform sourceTransform;

		private float defaultVolume;

		private float defaultPan;

		private int defaultPitch;

		private int currentPitch;

		private float currentVolume;

		private float volumeFactor = 1f;

		private float ctrlVolumeFactor = 1f;

		private AudioParamUpdater volumeUpdater = new AudioParamUpdater();

		private AudioParamUpdater panUpdater = new AudioParamUpdater();

		private AudioParamUpdater pitchUpdater = new AudioParamUpdater();

		private AudioParamUpdater controlUpdater = new AudioParamUpdater();

		private int prevPlaySamples;

		private bool onPause;

		private int randomIndex;

		private int instanceId;

		public bool activeSelf;

		private FADE_END_STATE fadeStatus;

		private AudioDefine.INSTANCE_STATUS status = AudioDefine.INSTANCE_STATUS.PREPARE;

		private Transform targetTransform;

		private Vector3 defaultPos = new Vector3(0f, 0f, 0f);

		private bool isUpdateStart = true;

		public void SetActive(bool active)
		{
			activeSelf = active;
		}

		public void SetInstanceID(int id)
		{
			instanceId = id;
		}

		public int GetInstanceID()
		{
			return instanceId;
		}

		public void Reset(AudioMainPool pool)
		{
			if (pool != null)
			{
				pool.Deactive(source);
			}
			source = null;
			sourceTransform = null;
			targetTransform = null;
			setting = null;
			volumeUpdater.Clear();
			panUpdater.Clear();
			pitchUpdater.Clear();
			controlUpdater.Clear();
			defaultVolume = 0f;
			defaultPan = 0f;
			defaultPitch = 0;
			currentPitch = 0;
			currentVolume = 0f;
			volumeFactor = 1f;
			ctrlVolumeFactor = 1f;
			prevPlaySamples = 0;
			onPause = false;
			randomIndex = 0;
			instanceId = 0;
			isUpdateStart = true;
		}

		public int GetRandomIndex()
		{
			return randomIndex;
		}

		public void Init(AudioSource playSource, float factor, int index)
		{
			randomIndex = index;
			source = playSource;
			sourceTransform = playSource.transform;
			setting = source.GetComponent<AudioLabelSettings>();
			volumeFactor = factor;
			defaultPan = source.panStereo;
			defaultPitch = setting.pitchShiftCent;
			isUpdateStart = true;
			sourceTransform.position = defaultPos;
		}

		public void Init(AudioSource playSource, AudioLabelSettings labelInfo, float factor, int index)
		{
			randomIndex = index;
			source = playSource;
			sourceTransform = playSource.transform;
			setting = labelInfo;
			volumeFactor = factor;
			defaultPan = setting.pan;
			defaultPitch = setting.pitchShiftCent;
			isUpdateStart = true;
			sourceTransform.position = defaultPos;
		}

		public void UpdateVolumeFactor(float factor)
		{
			volumeFactor = factor;
			if (source != null && source.isPlaying && status == AudioDefine.INSTANCE_STATUS.PLAY)
			{
				source.volume = setting.volume * currentVolume * volumeFactor * ctrlVolumeFactor;
			}
		}

		public void Update()
		{
			if (!(source != null))
			{
				return;
			}
			if (targetTransform != null)
			{
				sourceTransform.position = targetTransform.position;
			}
			if (source.isPlaying && source.timeSamples != prevPlaySamples && !onPause)
			{
				if (!isUpdateStart)
				{
					isUpdateStart = true;
					volumeUpdater.UpdateStart();
					controlUpdater.UpdateStart();
					panUpdater.UpdateStart();
					pitchUpdater.UpdateStart();
				}
				if (volumeUpdater.active)
				{
					currentVolume = volumeUpdater.Update();
					source.volume = setting.volume * currentVolume * volumeFactor * ctrlVolumeFactor;
					if (isAudioDebug)
					{
						_ = volumeUpdater.active;
					}
				}
				if (controlUpdater.active)
				{
					ctrlVolumeFactor = controlUpdater.Update();
					source.volume = setting.volume * currentVolume * volumeFactor * ctrlVolumeFactor;
					_ = isAudioDebug;
					if (!controlUpdater.active)
					{
						_ = isAudioDebug;
						if (fadeStatus == FADE_END_STATE.STOP)
						{
							_ = isAudioDebug;
							prevPlaySamples = source.timeSamples;
							source.Stop();
							fadeStatus = FADE_END_STATE.UNSET;
							status = AudioDefine.INSTANCE_STATUS.STOP;
						}
						else if (fadeStatus == FADE_END_STATE.PAUSE)
						{
							_ = isAudioDebug;
							prevPlaySamples = source.timeSamples;
							source.Pause();
							fadeStatus = FADE_END_STATE.UNSET;
							status = AudioDefine.INSTANCE_STATUS.PAUSE;
							onPause = true;
						}
					}
				}
				if (panUpdater.active)
				{
					source.panStereo = panUpdater.Update();
					if (isAudioDebug)
					{
						_ = panUpdater.active;
					}
				}
				if (pitchUpdater.active)
				{
					currentPitch = (int)pitchUpdater.Update();
					_ = isAudioDebug;
					if (isAudioDebug)
					{
						_ = pitchUpdater.active;
					}
					source.pitch = calcPitchRatio(currentPitch);
				}
			}
			else if (!source.isPlaying && (status == AudioDefine.INSTANCE_STATUS.PLAY || status == AudioDefine.INSTANCE_STATUS.PAUSE_SOON || status == AudioDefine.INSTANCE_STATUS.STOP_SOON))
			{
				status = AudioDefine.INSTANCE_STATUS.STOP;
			}
			if (source.isPlaying)
			{
				prevPlaySamples = source.timeSamples;
			}
		}

		public void ResetPlayPosition()
		{
			prevPlaySamples = 0;
			source.timeSamples = 0;
		}

		public void Prepare(float volume, float fadeTime, float pan, int pitch, int playStartSample)
		{
			if (source == null)
			{
				return;
			}
			if (onPause)
			{
				OffPause(AudioDefine.DEFAULT_FADE);
				return;
			}
			if (source.isPlaying)
			{
				source.Stop();
			}
			fadeStatus = FADE_END_STATE.UNSET;
			pitchUpdater.Clear();
			panUpdater.Clear();
			setupPitch((pitch < AudioDefine.PITCH_MIN || AudioDefine.PITCH_MAX < pitch) ? defaultPitch : pitch);
			setupStereoPan((pan < -0.7f || 0.7f < pan) ? defaultPan : pan);
			volumeUpdater.Clear();
			source.timeSamples = 0;
			if (setting.isPlayLastSamples && source.loop)
			{
				_ = isAudioDebug;
				if (playStartSample < 0 || source.clip.samples < playStartSample)
				{
					source.timeSamples = 0;
				}
				else
				{
					source.timeSamples = playStartSample;
				}
				prevPlaySamples = source.timeSamples;
			}
			defaultVolume = ((volume < AudioDefine.VOLUME_MIN || AudioDefine.VOLUME_MAX < volume) ? 1f : volume);
			setupVolume(defaultVolume, fadeTime, setting.isPlayLastSamples);
			_ = isAudioDebug;
			status = AudioDefine.INSTANCE_STATUS.PREPARE;
		}

		public void Play(float delay)
		{
			status = AudioDefine.INSTANCE_STATUS.PLAY;
			volumeUpdater.UpdateStart();
			panUpdater.UpdateStart();
			pitchUpdater.UpdateStart();
			if (setting.isAndroidNative && Application.platform == RuntimePlatform.Android)
			{
				USndAndroidNativePlayer.Play(setting.GetAndroidSoundId(), source.volume, source.pitch);
				return;
			}
			if (delay == 0f)
			{
				source.Play();
				return;
			}
			isUpdateStart = false;
			source.PlayDelayed((delay < 0f) ? setting.playStartDelay : delay);
		}

		public void Stop(float fadeTime)
		{
			if (!(source != null))
			{
				return;
			}
			if (source.isPlaying)
			{
				float num = fadeTime;
				if (num < 0f)
				{
					num = setting.fadeOutTime;
				}
				if (num != 0f)
				{
					_ = isAudioDebug;
					if (source.timeSamples != 0)
					{
						controlUpdater.Clear();
						controlUpdater.SetParam(ctrlVolumeFactor, 0f, num, isLow: false);
						fadeStatus = FADE_END_STATE.STOP;
						status = AudioDefine.INSTANCE_STATUS.STOP_SOON;
					}
					else
					{
						prevPlaySamples = source.timeSamples;
						source.Stop();
						status = AudioDefine.INSTANCE_STATUS.STOP;
					}
				}
				else
				{
					if (isAudioDebug)
					{
						AudioDebugLog.Log("stop " + source.name);
					}
					prevPlaySamples = source.timeSamples;
					source.Stop();
					status = AudioDefine.INSTANCE_STATUS.STOP;
				}
			}
			else if (status == AudioDefine.INSTANCE_STATUS.PAUSE)
			{
				source.Stop();
				status = AudioDefine.INSTANCE_STATUS.STOP;
			}
		}

		public void ForceStop()
		{
			volumeUpdater.Clear();
			pitchUpdater.Clear();
			panUpdater.Clear();
			controlUpdater.Clear();
			source.Stop();
			status = AudioDefine.INSTANCE_STATUS.STOP;
		}

		public void OnPause(float fadeTime)
		{
			if (fadeStatus == FADE_END_STATE.UNSET && !onPause && source.isPlaying)
			{
				float num = fadeTime;
				if (num < 0f)
				{
					num = setting.fadeOutTimeOnPause;
				}
				if (num != 0f)
				{
					_ = isAudioDebug;
					controlUpdater.Clear();
					controlUpdater.SetParam(ctrlVolumeFactor, 0f, num, isLow: false);
					fadeStatus = FADE_END_STATE.PAUSE;
					status = AudioDefine.INSTANCE_STATUS.PAUSE_SOON;
				}
				else
				{
					_ = isAudioDebug;
					prevPlaySamples = source.timeSamples;
					source.Pause();
					ctrlVolumeFactor = 0f;
					onPause = true;
					status = AudioDefine.INSTANCE_STATUS.PAUSE;
				}
			}
		}

		public void OffPause(float fadeTime)
		{
			if (fadeStatus == FADE_END_STATE.PAUSE)
			{
				fadeStatus = FADE_END_STATE.UNSET;
				controlUpdater.Clear();
				controlUpdater.SetParam(ctrlVolumeFactor, 1f, fadeTime, isLow: false);
			}
			else if (onPause)
			{
				fadeStatus = FADE_END_STATE.UNSET;
				onPause = false;
				float num = fadeTime;
				if (num < 0f)
				{
					num = setting.fadeInTimeOffPause;
				}
				if (num != 0f)
				{
					ctrlVolumeFactor = 0f;
					controlUpdater.Clear();
					controlUpdater.SetParam(ctrlVolumeFactor, 1f, num, isLow: false);
					source.volume = 0f;
				}
				else
				{
					ctrlVolumeFactor = 1f;
					source.volume = setting.volume * currentVolume * volumeFactor * ctrlVolumeFactor;
				}
				onPause = false;
				source.Play();
			}
			status = AudioDefine.INSTANCE_STATUS.PLAY;
		}

		public void SetVolume(float newVolume, float moveTime)
		{
			float num = newVolume;
			if (newVolume < 0f || 1f < newVolume)
			{
				num = defaultVolume;
			}
			volumeUpdater.Clear();
			if (moveTime == 0f)
			{
				currentVolume = num;
				source.volume = setting.volume * num * volumeFactor * ctrlVolumeFactor;
				_ = isAudioDebug;
			}
			else
			{
				volumeUpdater.SetParam(currentVolume, num, moveTime, isLow: false);
				_ = isAudioDebug;
			}
		}

		public float GetCurrentVolume()
		{
			return currentVolume;
		}

		public float GetCalcVolume()
		{
			return source.volume;
		}

		public void SetPitch(int newPitch, float moveTime)
		{
			int num = newPitch;
			if (newPitch < -1200 || 1200 < newPitch)
			{
				num = defaultPitch;
			}
			pitchUpdater.Clear();
			if (moveTime == 0f)
			{
				currentPitch = num;
				source.pitch = calcPitchRatio(currentPitch);
				_ = isAudioDebug;
			}
			else
			{
				pitchUpdater.SetParam(currentPitch, num, moveTime, isLow: false);
				_ = isAudioDebug;
			}
		}

		public void SetPan(float newPan, float moveTime)
		{
			float num = newPan;
			if (newPan < -1f || 1f < newPan)
			{
				num = defaultPan;
			}
			pitchUpdater.Clear();
			if (moveTime == 0f)
			{
				source.panStereo = num;
				_ = isAudioDebug;
			}
			else
			{
				panUpdater.SetParam(source.panStereo, num, moveTime, isLow: false);
				_ = isAudioDebug;
			}
		}

		public void SetTrackingObject(GameObject target)
		{
			if (target != null)
			{
				SetTrackingObject(target.transform);
			}
		}

		public void SetTrackingObject(Transform target)
		{
			targetTransform = target;
			if (source != null && targetTransform != null)
			{
				sourceTransform.position = targetTransform.position;
			}
		}

		public void SetPosition(Vector3 position)
		{
			sourceTransform.position = position;
		}

		public bool IsPlaying()
		{
			if (source != null)
			{
				return source.isPlaying;
			}
			return false;
		}

		public AudioDefine.INSTANCE_STATUS GetStatus()
		{
			return status;
		}

		public int GetPrevPlaySamples()
		{
			return prevPlaySamples;
		}

		public float GetPlayTime()
		{
			if (source != null)
			{
				return source.time;
			}
			return -1f;
		}

		public int GetPlaySamples()
		{
			if (source != null)
			{
				return source.timeSamples;
			}
			return -1;
		}

		public void SetTime(float time)
		{
			if (source != null && source.clip.length >= time)
			{
				source.time = time;
			}
		}

		public void SetTimeSamples(int samples)
		{
			if (source != null && source.clip.samples >= samples)
			{
				source.timeSamples = samples;
			}
		}

		public bool GetSpectrumData(int instanceId, float[] sample, int channel, FFTWindow window)
		{
			if (source != null)
			{
				source.GetSpectrumData(sample, channel, window);
				return true;
			}
			return false;
		}

		private void setupVolume(float volume, float fadeTime, bool isPlayLastSamples)
		{
			if (source != null)
			{
				currentVolume = volume;
				float num = fadeTime;
				if (fadeTime < 0f)
				{
					num = ((!setting.isPlayLastSamples || prevPlaySamples == 0) ? setting.fadeInTime : setting.fadeInTimeOldSamples);
				}
				if (setting.isPlayLastSamples && num != 0f && prevPlaySamples != 0)
				{
					volumeUpdater.SetParam(0f, volume, num, isLow: false);
					source.volume = 0f;
					currentVolume = 0f;
				}
				else if (num != 0f)
				{
					volumeUpdater.SetParam(0f, volume, num, isLow: false);
					source.volume = 0f;
					currentVolume = 0f;
				}
				else
				{
					source.volume = setting.volume * volume * volumeFactor * ctrlVolumeFactor;
				}
				_ = isAudioDebug;
			}
		}

		private void setupPitch(int pitch)
		{
			if (source != null)
			{
				currentPitch = pitch;
				_ = isAudioDebug;
				if (setting.isMovePitch && setting.pitchMoveTime != 0f)
				{
					pitchUpdater.SetParam(setting.pitchStart, setting.pitchEnd, setting.pitchMoveTime, isLow: false);
					currentPitch = setting.pitchStart;
				}
				source.pitch = calcPitchRatio(currentPitch);
			}
		}

		private void setupStereoPan(float pan)
		{
			if (source != null)
			{
				source.panStereo = pan;
				_ = isAudioDebug;
				if (setting.isMovePan && setting.panMoveTime != 0f)
				{
					panUpdater.SetParam(setting.panStart, setting.panEnd, setting.panMoveTime, isLow: false);
					source.panStereo = setting.panStart;
				}
			}
		}

		private float getRandomValue(float min, float max, float unit, bool isconsecutive, float prevValue)
		{
			float num = 0f;
			do
			{
				if (unit != 0f)
				{
					float num2 = ((min > max) ? (min - max) : (max - min));
					num = Mathf.Round(UnityEngine.Random.Range(0f, num2 / unit)) * unit + min;
				}
				else
				{
					num = UnityEngine.Random.Range(min, max);
				}
			}
			while (isconsecutive && prevValue == num);
			return num;
		}

		private float calcPitchRatio(int cent)
		{
			if (cent < -1200 || 1200 < cent)
			{
				cent = 0;
			}
			return AudioDefine.PITCH_VALUES[cent + 1200];
		}

		public void UpdateAudio3DSettings(Audio3DSettings audio3d)
		{
			if (setting.spatialGroup.Equals(audio3d.spatialName) && source != null)
			{
				source.spatialBlend = audio3d.spatialBlend;
				source.reverbZoneMix = audio3d.reverbZoneMix;
				source.dopplerLevel = audio3d.dopplerLevel;
				source.spread = audio3d.spread;
				source.rolloffMode = audio3d.rolloffMode;
				source.minDistance = audio3d.minDistance;
				source.maxDistance = audio3d.maxDistance;
				source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, audio3d.customRolloffCurve);
				source.SetCustomCurve(AudioSourceCurveType.SpatialBlend, audio3d.spatialBlendCurve);
				source.SetCustomCurve(AudioSourceCurveType.ReverbZoneMix, audio3d.reverbZoneMixCurve);
				source.SetCustomCurve(AudioSourceCurveType.Spread, audio3d.spreadCurve);
			}
		}
	}

	private class AudioInstancePool
	{
		private static AudioInstancePool _instance;

		private static int instanceIdNext = 1;

		private List<AudioInstance> pool = new List<AudioInstance>(AudioDefine.LIST_CAPACITY);

		public static AudioInstancePool instance => _instance;

		public static void Initialize()
		{
			if (_instance == null)
			{
				_instance = new AudioInstancePool();
			}
		}

		public void AddEmpty(int num)
		{
			for (int i = 0; i < num; i++)
			{
				AudioInstance audioInstance = new AudioInstance();
				pool.Add(audioInstance);
				audioInstance.SetActive(active: false);
			}
		}

		public AudioInstance AddComponent()
		{
			AudioInstance audioInstance = null;
			if (instanceIdNext == 0)
			{
				instanceIdNext = 1;
			}
			for (int i = 0; i < pool.Count; i++)
			{
				audioInstance = pool[i];
				if (!audioInstance.activeSelf)
				{
					audioInstance.SetActive(active: true);
					audioInstance.SetInstanceID(instanceIdNext++);
					return audioInstance;
				}
			}
			audioInstance = new AudioInstance();
			pool.Add(audioInstance);
			audioInstance.SetInstanceID(instanceIdNext++);
			audioInstance.SetActive(active: true);
			return audioInstance;
		}

		public void Deactive(AudioInstance instance)
		{
			instance.SetActive(active: false);
		}

		public void Clear()
		{
			int num = 0;
			while (num < pool.Count)
			{
				if (!pool[num].activeSelf)
				{
					pool[num] = null;
					pool.RemoveAt(num);
				}
				else
				{
					num++;
				}
			}
		}
	}

	[Serializable]
	public class AudioLabelSettings
	{
		public enum BEHAVIOR
		{
			STEAL_OLDEST,
			JUST_FAIL,
			QUEUE
		}

		public float volume = 1f;

		public BEHAVIOR maxPlaybacksBehavior;

		public int priority = 64;

		public string categoryName;

		public string singleGroup;

		public int maxPlaybacksNum;

		public bool isStealOldest = true;

		public string unityMixerName;

		public string spatialGroup;

		public float playStartDelay;

		public float playInterval;

		public float pan;

		public int pitchShiftCent;

		public bool isPlayLastSamples;

		public float fadeInTime;

		public float fadeOutTime;

		public float fadeInTimeOldSamples;

		public float fadeOutTimeOnPause;

		public float fadeInTimeOffPause;

		public bool isVolumeRandom;

		public bool inconsecutiveVolume;

		public float volumeRandomMin;

		public float volumeRandomMax;

		public float volumeRandomUnit;

		public bool isPitchRandom;

		public bool inconsecutivePitch;

		public int pitchRandomMin;

		public int pitchRandomMax;

		public int pitchRandomUnit;

		public bool isPanRandom;

		public bool inconsecutivePan;

		public float panRandomMin;

		public float panRandomMax;

		public float panRandomUnit;

		public bool isRandomPlay;

		public bool inconsecutiveSource;

		public string[] randomSource;

		public bool isMovePitch;

		public int pitchStart;

		public int pitchEnd;

		public float pitchMoveTime;

		public bool isMovePan;

		public float panStart;

		public float panEnd;

		public float panMoveTime;

		public string[] duckingCategories;

		public float duckingStartTime;

		public float duckingEndTime;

		public float duckingVolumeFactor;

		public bool autoRestoreDucking = true;

		public float restoreTime = -1f;

		public bool isAndroidNative;

		private int androidSoundId;

		public int loadId;

		public string name;

		private AudioCategorySettings attachCategory;

		public string clipName;

		public bool isLoop;

		public void SetAndroidSoundId(int soundId)
		{
			androidSoundId = soundId;
		}

		public int GetAndroidSoundId()
		{
			return androidSoundId;
		}

		public void SetLoop(bool loop)
		{
			isLoop = loop;
		}

		public bool GetLoop()
		{
			return isLoop;
		}

		public void SetClipName(string name)
		{
			clipName = name;
		}

		public string GetClipName()
		{
			return clipName;
		}

		public void SetAttachCategoryInstance(AudioCategorySettings category)
		{
			attachCategory = category;
		}

		public AudioCategorySettings GetAttachCategory()
		{
			return attachCategory;
		}

		public string GetCategoryName()
		{
			return categoryName;
		}
	}

	private class AudioMainPool : MonoBehaviour
	{
		private List<AudioSource> pool = new List<AudioSource>(AudioDefine.LIST_CAPACITY);

		private static AudioMainPool _instance;

		private static GameObject owner;

		private static Transform _cacheTransform;

		private static Transform CacheTransform
		{
			get
			{
				if (!(_cacheTransform != null))
				{
					return _cacheTransform = owner.transform;
				}
				return _cacheTransform;
			}
		}

		public static AudioMainPool instance => _instance;

		public static void Initialize(GameObject obj)
		{
			if (_instance == null)
			{
				_instance = obj.AddComponent<AudioMainPool>();
				owner = obj;
			}
		}

		public static void Terminate()
		{
			if (_instance != null)
			{
				UnityEngine.Object.Destroy(_instance);
				_instance = null;
				owner = null;
				_cacheTransform = null;
			}
		}

		public void AddEmpty(int num)
		{
			for (int i = 0; i < num; i++)
			{
				AudioSource audioSource = new GameObject().AddComponent<AudioSource>();
				audioSource.transform.parent = CacheTransform;
				audioSource.gameObject.SetActive(value: false);
				pool.Add(audioSource);
			}
		}

		public AudioSource GetClone()
		{
			AudioSource audioSource = null;
			for (int i = 0; i < pool.Count; i++)
			{
				audioSource = pool[i];
				if (!audioSource.gameObject.activeSelf)
				{
					audioSource.gameObject.SetActive(value: true);
					return audioSource;
				}
			}
			audioSource = new GameObject().AddComponent<AudioSource>();
			audioSource.transform.parent = CacheTransform;
			pool.Add(audioSource);
			return audioSource;
		}

		public void Deactive(AudioSource source)
		{
			source.gameObject.SetActive(value: false);
			source.clip = null;
		}

		public void Clear()
		{
			AudioSource audioSource = null;
			int num = 0;
			while (num < pool.Count)
			{
				audioSource = pool[num];
				if (!audioSource.gameObject.activeSelf)
				{
					UnityEngine.Object.Destroy(audioSource.gameObject);
					pool[num] = null;
					pool.RemoveAt(num);
				}
				else
				{
					num++;
				}
			}
		}
	}

	private enum RESULT
	{
		CONTINUE,
		EXECUTE,
		FINISH
	}

	[Serializable]
	private class Wrapper<T>
	{
		public T[] master;

		public T[] category;

		public T[] label;
	}

	[Serializable]
	private class AudioSourceWrapper
	{
		public string spatialName = "";

		[Range(0f, 1f)]
		public float spatialBlend = 1f;

		[Range(0f, 1.1f)]
		public float reverbZoneMix = 1f;

		[Range(0f, 5f)]
		public float dopplerLevel = 1f;

		[Range(0f, 360f)]
		public int spread;

		public AudioRolloffMode rolloffMode;

		public float minDistance = 1f;

		public float maxDistance = 500f;

		public AnimationCurve customRolloffCurve;

		public AnimationCurve spatialBlendCurve;

		public AnimationCurve reverbZoneMixCurve;

		public AnimationCurve spreadCurve;
	}

	[Serializable]
	public class AudioMasterSettings
	{
		public string masterName;

		public float volume = 1f;

		private AudioParamUpdater volumeUpdater = new AudioParamUpdater();

		public float programVolume = 1f;

		public float mute = 1f;

		private float manner = 1f;

		public AudioMasterSettings()
		{
			programVolume = 1f;
		}

		public void CopySettings(AudioMasterSettings src)
		{
			if (masterName.CompareTo(src.masterName) == 0)
			{
				volume = src.volume;
			}
		}

		public float GetCurrentVolume()
		{
			return programVolume;
		}

		public float GetVolumeFactor()
		{
			return programVolume * volume * mute * manner;
		}

		public void SetVolumeUpdater(float start, float target, float time)
		{
			volumeUpdater.SetParam(start, target, time, isLow: false);
		}

		public void ClearVolumeUpdater()
		{
			volumeUpdater.Clear();
		}

		public bool UpdateVolume()
		{
			if (volumeUpdater.active)
			{
				programVolume = volumeUpdater.Update();
				return true;
			}
			return false;
		}

		public void SetMute(bool onMute)
		{
			mute = ((!onMute) ? 1 : 0);
		}

		public void SetMannerMode(bool onMute)
		{
			manner = ((!onMute) ? 1 : 0);
		}
	}

	private class AudioMixerSettings
	{
		public AudioMixer mixer;

		public void SetAudioMixer(AudioMixer _mixer)
		{
			mixer = _mixer;
		}

		public AudioMixerGroup[] FindGroup(string groupName)
		{
			if (mixer == null)
			{
				return null;
			}
			return mixer.FindMatchingGroups(groupName);
		}

		public void SetSnapshot(string snapName, float time)
		{
			if (!(mixer == null))
			{
				AudioMixerSnapshot audioMixerSnapshot = mixer.FindSnapshot(snapName);
				if (audioMixerSnapshot != null)
				{
					audioMixerSnapshot.TransitionTo(time);
				}
			}
		}

		public void SetFloat(string paramName, float value)
		{
			if (!(mixer == null))
			{
				mixer.SetFloat(paramName, value);
			}
		}
	}

	private class AudioParamUpdater
	{
		private float target = 1f;

		private float current = 1f;

		private float unit;

		private float prevTime;

		private bool move;

		private bool _active;

		public bool active
		{
			get
			{
				return _active;
			}
			set
			{
				_active = value;
			}
		}

		public void SetParam(float _start, float _target, float moveTime, bool isLow)
		{
			if (_start != _target && (!isLow || !(target < _target)))
			{
				active = true;
				if (moveTime == 0f)
				{
					move = false;
					unit = _target - _start;
				}
				else
				{
					move = true;
					unit = (_target - _start) / moveTime;
				}
				current = _start;
				target = _target;
				prevTime = Time.unscaledTime;
			}
		}

		public void UpdateStart()
		{
			if (active)
			{
				prevTime = Time.unscaledTime;
			}
		}

		public float Update()
		{
			if (active)
			{
				float unscaledTime = Time.unscaledTime;
				current += unit * (unscaledTime - prevTime);
				if (unit > 0f && current >= target)
				{
					current = target;
					active = false;
				}
				else if (unit < 0f && current <= target)
				{
					current = target;
					active = false;
				}
				if (!move)
				{
					current = target;
					active = false;
				}
				prevTime = unscaledTime;
			}
			return current;
		}

		public void Clear()
		{
			target = 0f;
			current = 0f;
			unit = 0f;
			active = false;
		}
	}

	private class AudioPlayer
	{
		private class PlayData
		{
			public AudioClip clip;

			public AudioLabelSettings info;
		}

		private List<AudioInstance> playInstance = new List<AudioInstance>(4);

		private List<PlayData> playSource = new List<PlayData>();

		private AudioLabelSettings playSettings;

		private AudioClip playClip;

		private bool isSetClip;

		private int prevPlayIndex = -1;

		private float prevVolumeRandom = -10000f;

		private float prevPitchRandom = -10000f;

		private float prevPanRandom = -10000f;

		private List<int> prevPlaySamplesList = new List<int>(4);

		private string playerName;

		private AudioMixerGroup mixer;

		private Audio3DSettings spatialSettings;

		private float nextInterval;

		private float prevPlayTime;

		private bool force2D;

		public string PlayerName
		{
			get
			{
				return playerName;
			}
			set
			{
				playerName = value;
			}
		}

		public void SetAudioMixerGroup(AudioMixerGroup _mixer)
		{
			mixer = _mixer;
		}

		public void UpdateRandomSourceInfo(Dictionary<string, AudioPlayer> dict)
		{
			if (!playSettings.isRandomPlay)
			{
				return;
			}
			playSource.Clear();
			PlayData playData = new PlayData();
			playData.clip = playClip;
			playData.info = playSettings;
			playSource.Add(playData);
			if (playSettings.randomSource == null)
			{
				return;
			}
			for (int i = 0; i < playSettings.randomSource.Length; i++)
			{
				if (dict.TryGetValue(playSettings.randomSource[i], out var value))
				{
					AudioClip audioClip = value.GetPlayClip();
					AudioLabelSettings labelSettings = value.GetLabelSettings();
					PlayData playData2 = new PlayData();
					playData2.clip = null;
					playData2.info = null;
					if (audioClip != null)
					{
						playData2.clip = audioClip;
						playData2.info = labelSettings;
						playSource.Add(playData2);
						prevPlaySamplesList.Add(0);
					}
				}
			}
		}

		public AudioClip GetPlayClip()
		{
			return playClip;
		}

		public bool IsSetPlayClip()
		{
			return isSetClip;
		}

		public void SetPlayClip(AudioClip clip)
		{
			playClip = clip;
			isSetClip = true;
			if (playSource.Count != 0)
			{
				playSource[0].clip = clip;
				playSource[0].info = playSettings;
			}
		}

		public float GetClipLength()
		{
			if (playClip != null)
			{
				return playClip.length;
			}
			return 0f;
		}

		public int GetClipSamples()
		{
			if (playClip != null)
			{
				return playClip.samples;
			}
			return 0;
		}

		public bool Init(AudioClip clip, string name, AudioLabelSettings label, Dictionary<string, AudioPlayer> dict)
		{
			playClip = clip;
			if (playerName == null)
			{
				playerName = name;
			}
			PlayData playData = new PlayData();
			playData.clip = playClip;
			playData.info = label;
			playSource.Add(playData);
			prevPlaySamplesList.Add(0);
			nextInterval = 0f;
			playSettings = label;
			if (label != null && label.maxPlaybacksNum > 0)
			{
				AudioInstancePool.instance.AddEmpty(label.maxPlaybacksNum);
			}
			if (playSettings == null)
			{
				return false;
			}
			UpdateRandomSourceInfo(dict);
			initRandomSettins();
			return true;
		}

		private void initRandomSettins()
		{
			if (playSettings.isVolumeRandom && playSettings.volumeRandomMax < playSettings.volumeRandomMin)
			{
				float volumeRandomMin = playSettings.volumeRandomMin;
				playSettings.volumeRandomMin = playSettings.volumeRandomMax;
				playSettings.volumeRandomMax = volumeRandomMin;
			}
			if (playSettings.isPitchRandom && playSettings.pitchRandomMax < playSettings.pitchRandomMin)
			{
				int pitchRandomMin = playSettings.pitchRandomMin;
				playSettings.pitchRandomMin = playSettings.pitchRandomMax;
				playSettings.pitchRandomMax = pitchRandomMin;
			}
			if (playSettings.isPanRandom && playSettings.panRandomMax < playSettings.panRandomMin)
			{
				float panRandomMin = playSettings.panRandomMin;
				playSettings.panRandomMin = playSettings.panRandomMax;
				playSettings.panRandomMax = panRandomMin;
			}
		}

		public void ResetPlayClip()
		{
			playClip = null;
			isSetClip = false;
			for (int i = 0; i < playSource.Count; i++)
			{
				playSource[i].clip = null;
				playSource[i].info = null;
			}
		}

		public void Reset()
		{
			for (int i = 0; i < playInstance.Count; i++)
			{
				AudioInstance audioInstance = playInstance[i];
				audioInstance.ForceStop();
				audioInstance.Reset(AudioMainPool.instance);
				AudioInstancePool.instance.Deactive(audioInstance);
			}
			playInstance.Clear();
			for (int j = 0; j < playSource.Count; j++)
			{
				playSource[j].clip = null;
				playSource[j].info = null;
			}
			playSource.Clear();
			playSettings = null;
			playClip = null;
			isSetClip = false;
			mixer = null;
			nextInterval = 0f;
		}

		public void LoadAudioData()
		{
			for (int i = 0; i < playSource.Count; i++)
			{
				PlayData playData = playSource[i];
				if (playData.clip != null)
				{
					playData.clip.LoadAudioData();
				}
			}
		}

		public void UnloadAudioData()
		{
			for (int i = 0; i < playSource.Count; i++)
			{
				PlayData playData = playSource[i];
				if (playData.clip != null && !playData.clip.preloadAudioData)
				{
					playData.clip.UnloadAudioData();
				}
			}
		}

		private float getRandomValue(float min, float max, float unit, bool isconsecutive, float prevValue)
		{
			float num = 0f;
			bool flag = isconsecutive;
			if (min == max)
			{
				flag = false;
			}
			do
			{
				if (unit != 0f)
				{
					float num2 = ((min > max) ? (min - max) : (max - min));
					num = Mathf.Round(UnityEngine.Random.Range(0f, num2 / unit)) * unit + min;
				}
				else
				{
					num = UnityEngine.Random.Range(min, max);
				}
			}
			while (flag && prevValue == num);
			return num;
		}

		public void ResetPlayPosition()
		{
			for (int i = 0; i < prevPlaySamplesList.Count; i++)
			{
				prevPlaySamplesList[i] = 0;
			}
		}

		public int GetPlayingNum()
		{
			int num = 0;
			for (int i = 0; i < playInstance.Count; i++)
			{
				AudioDefine.INSTANCE_STATUS status = playInstance[i].GetStatus();
				if (status == AudioDefine.INSTANCE_STATUS.STOP_SOON || status == AudioDefine.INSTANCE_STATUS.STOP)
				{
					num++;
				}
			}
			return playInstance.Count - num;
		}

		public int GetPlayingTrueNum()
		{
			return playInstance.Count;
		}

		public int GetMaxPlaybacksNum()
		{
			if (playSettings == null)
			{
				return 0;
			}
			return playSettings.maxPlaybacksNum;
		}

		public bool IsStealOldest()
		{
			if (playSettings == null)
			{
				return true;
			}
			return playSettings.isStealOldest;
		}

		public string GetCategoryName()
		{
			if (playSettings == null)
			{
				return null;
			}
			if (playSettings.GetAttachCategory() == null)
			{
				return null;
			}
			return playSettings.GetAttachCategory().categoryName;
		}

		public int GetCategoryMaxPlaybacksNum()
		{
			if (playSettings == null)
			{
				return 0;
			}
			return playSettings.GetAttachCategory().maxPlaybacksNum;
		}

		public int GetPriority()
		{
			if (playSettings == null)
			{
				return 0;
			}
			return playSettings.priority;
		}

		public AudioLabelSettings.BEHAVIOR GetMaxPlaybacksBehavior()
		{
			if (playSettings == null)
			{
				return AudioLabelSettings.BEHAVIOR.STEAL_OLDEST;
			}
			return playSettings.maxPlaybacksBehavior;
		}

		public float GetFadeOutTime()
		{
			if (playSettings == null)
			{
				return 0f;
			}
			return playSettings.fadeOutTime;
		}

		public AudioCategorySettings GetCategorySettings()
		{
			if (playSettings == null)
			{
				return null;
			}
			return playSettings.GetAttachCategory();
		}

		public AudioLabelSettings GetLabelSettings()
		{
			return playSettings;
		}

		public AudioDefine.INSTANCE_STATUS GetInstanceStatus(int instanceId)
		{
			for (int i = 0; i < playInstance.Count; i++)
			{
				AudioInstance audioInstance = playInstance[i];
				if (audioInstance.GetInstanceID() == instanceId)
				{
					return audioInstance.GetStatus();
				}
			}
			return AudioDefine.INSTANCE_STATUS.STOP;
		}

		public void StopOldInstance()
		{
			for (int i = 0; i < playInstance.Count; i++)
			{
				AudioInstance audioInstance = playInstance[i];
				AudioDefine.INSTANCE_STATUS status = audioInstance.GetStatus();
				if (status == AudioDefine.INSTANCE_STATUS.PLAY || status == AudioDefine.INSTANCE_STATUS.PAUSE || status == AudioDefine.INSTANCE_STATUS.PAUSE_SOON)
				{
					audioInstance.Stop(AudioDefine.DEFAULT_FADE);
					break;
				}
			}
		}

		private int prepareImpl(float volume, float fadeTime, float pan, int pitch, float delay, bool isStart, bool isForce2D)
		{
			int num = 0;
			if (playSettings.isRandomPlay && playSource.Count > 1)
			{
				do
				{
					num = (int)Mathf.Round(UnityEngine.Random.Range(0, playSource.Count));
				}
				while (playSettings.inconsecutiveSource && prevPlayIndex == num);
			}
			prevPlayIndex = num;
			AudioClip audioClip = null;
			AudioSource audioSource = null;
			AudioLabelSettings audioLabelSettings = null;
			if (playSource[num].clip != null)
			{
				audioClip = playSource[num].clip;
				audioLabelSettings = playSource[num].info;
			}
			else
			{
				num = 0;
				audioClip = playSource[num].clip;
				audioLabelSettings = playSettings;
				if (audioSource == null && audioClip == null)
				{
					return AudioDefine.INSTANCE_ID_ERROR;
				}
			}
			bool flag = false;
			audioSource = AudioMainPool.instance.GetClone();
			audioSource.clip = audioClip;
			audioSource.playOnAwake = false;
			audioSource.loop = audioLabelSettings.GetLoop();
			audioSource.spatialBlend = 0f;
			force2D = isForce2D;
			if (spatialSettings != null && !isForce2D)
			{
				audioSource.spatialBlend = spatialSettings.spatialBlend;
				audioSource.reverbZoneMix = spatialSettings.reverbZoneMix;
				audioSource.dopplerLevel = spatialSettings.dopplerLevel;
				audioSource.spread = spatialSettings.spread;
				audioSource.rolloffMode = spatialSettings.rolloffMode;
				audioSource.minDistance = spatialSettings.minDistance;
				audioSource.maxDistance = spatialSettings.maxDistance;
				audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, spatialSettings.customRolloffCurve);
				audioSource.SetCustomCurve(AudioSourceCurveType.SpatialBlend, spatialSettings.spatialBlendCurve);
				audioSource.SetCustomCurve(AudioSourceCurveType.ReverbZoneMix, spatialSettings.reverbZoneMixCurve);
				audioSource.SetCustomCurve(AudioSourceCurveType.Spread, spatialSettings.spreadCurve);
			}
			flag = true;
			if (mixer != null)
			{
				audioSource.outputAudioMixerGroup = mixer;
			}
			else
			{
				audioSource.outputAudioMixerGroup = null;
			}
			if (audioSource.clip == null)
			{
				return AudioDefine.INSTANCE_ID_ERROR;
			}
			AudioInstance audioInstance = AudioInstancePool.instance.AddComponent();
			float volume2 = volume;
			if (audioLabelSettings.isVolumeRandom)
			{
				volume2 = (prevVolumeRandom = getRandomValue(audioLabelSettings.volumeRandomMin, audioLabelSettings.volumeRandomMax, audioLabelSettings.volumeRandomUnit, audioLabelSettings.inconsecutiveVolume, prevVolumeRandom));
			}
			float pan2 = pan;
			if (audioLabelSettings.isPanRandom)
			{
				pan2 = (prevPanRandom = getRandomValue(audioLabelSettings.panRandomMin, playSettings.panRandomMax, audioLabelSettings.panRandomUnit, audioLabelSettings.inconsecutivePan, prevPanRandom));
			}
			int num2 = pitch;
			if (audioLabelSettings.isPitchRandom)
			{
				num2 = (int)getRandomValue(audioLabelSettings.pitchRandomMin, audioLabelSettings.pitchRandomMax, audioLabelSettings.pitchRandomUnit, audioLabelSettings.inconsecutivePitch, prevPitchRandom);
				prevPitchRandom = num2;
			}
			float delay2 = delay;
			if (delay < 0f)
			{
				delay2 = audioLabelSettings.playStartDelay;
			}
			if (flag)
			{
				audioInstance.Init(audioSource, audioLabelSettings, (playSettings.GetAttachCategory() != null) ? playSettings.GetAttachCategory().GetVolumeFactor() : 1f, prevPlayIndex);
			}
			audioSource.gameObject.SetActive(value: true);
			playInstance.Add(audioInstance);
			audioInstance.Prepare(volume2, fadeTime, pan2, num2, prevPlaySamplesList[prevPlayIndex]);
			if (isStart)
			{
				audioInstance.Play(delay2);
				setInterval();
			}
			return audioInstance.GetInstanceID();
		}

		public int Prepare(float volume, float fadeTime, float pan, int pitch, bool isForce2D)
		{
			return prepareImpl(volume, fadeTime, pan, pitch, 0f, isStart: false, isForce2D);
		}

		public int Play(float volume, float fadeTime, float pan, int pitch, float delay, bool isForce2D)
		{
			return prepareImpl(volume, fadeTime, pan, pitch, delay, isStart: true, isForce2D);
		}

		public void PlayInstance(int instanceId, float delay = 0f)
		{
			for (int i = 0; i < playInstance.Count; i++)
			{
				AudioInstance audioInstance = playInstance[i];
				if (audioInstance.GetInstanceID() == instanceId)
				{
					audioInstance.Play(delay);
					setInterval();
					break;
				}
			}
		}

		public void SetTrackingObject(int instanceId, GameObject target)
		{
			for (int i = 0; i < playInstance.Count; i++)
			{
				AudioInstance audioInstance = playInstance[i];
				if (audioInstance.GetInstanceID() == instanceId)
				{
					audioInstance.SetTrackingObject(target);
					break;
				}
			}
		}

		public void SetTrackingObject(int instanceId, Transform target)
		{
			for (int i = 0; i < playInstance.Count; i++)
			{
				AudioInstance audioInstance = playInstance[i];
				if (audioInstance.GetInstanceID() == instanceId)
				{
					audioInstance.SetTrackingObject(target);
					break;
				}
			}
		}

		public void Stop(int instanceId, float fadeTime = -1f)
		{
			for (int i = 0; i < playInstance.Count; i++)
			{
				AudioInstance audioInstance = playInstance[i];
				if (audioInstance.GetInstanceID() == instanceId)
				{
					audioInstance.Stop(fadeTime);
					break;
				}
			}
		}

		public void StopAll(float fadeTime = -1f)
		{
			for (int i = 0; i < playInstance.Count; i++)
			{
				playInstance[i].Stop(fadeTime);
			}
		}

		public void OnPause(int instanceId, float fadeTime = -1f)
		{
			for (int i = 0; i < playInstance.Count; i++)
			{
				AudioInstance audioInstance = playInstance[i];
				if (audioInstance.GetInstanceID() == instanceId)
				{
					audioInstance.OnPause(fadeTime);
					break;
				}
			}
		}

		public void OnPauseAll(float fadeTime = -1f)
		{
			for (int i = 0; i < playInstance.Count; i++)
			{
				playInstance[i].OnPause(fadeTime);
			}
		}

		public void OffPause(int instanceId, float fadeTime = -1f)
		{
			for (int i = 0; i < playInstance.Count; i++)
			{
				AudioInstance audioInstance = playInstance[i];
				if (audioInstance.GetInstanceID() == instanceId)
				{
					audioInstance.OffPause(fadeTime);
					break;
				}
			}
		}

		public void OffPauseAll(float fadeTime = -1f)
		{
			for (int i = 0; i < playInstance.Count; i++)
			{
				playInstance[i].OffPause(fadeTime);
			}
		}

		public void SetVolume(int instanceId, float newVolume, float moveTime)
		{
			for (int i = 0; i < playInstance.Count; i++)
			{
				AudioInstance audioInstance = playInstance[i];
				if (audioInstance.GetInstanceID() == instanceId)
				{
					audioInstance.SetVolume(newVolume, moveTime);
					break;
				}
			}
		}

		public float GetCurrentVolume(int instanceId)
		{
			for (int i = 0; i < playInstance.Count; i++)
			{
				AudioInstance audioInstance = playInstance[i];
				if (audioInstance.GetInstanceID() == instanceId)
				{
					return audioInstance.GetCurrentVolume();
				}
			}
			return 0f;
		}

		public float GetCalcVolume(int instanceId)
		{
			for (int i = 0; i < playInstance.Count; i++)
			{
				AudioInstance audioInstance = playInstance[i];
				if (audioInstance.GetInstanceID() == instanceId)
				{
					return audioInstance.GetCalcVolume();
				}
			}
			return 0f;
		}

		public void SetVolumeAll(float newVolume, float moveTime)
		{
			for (int i = 0; i < playInstance.Count; i++)
			{
				playInstance[i].SetVolume(newVolume, moveTime);
			}
		}

		public void SetPitch(int instanceId, int newPitch, float moveTime)
		{
			for (int i = 0; i < playInstance.Count; i++)
			{
				AudioInstance audioInstance = playInstance[i];
				if (audioInstance.GetInstanceID() == instanceId)
				{
					audioInstance.SetPitch(newPitch, moveTime);
					break;
				}
			}
		}

		public void SetPitchAll(int newPitch, float moveTime)
		{
			for (int i = 0; i < playInstance.Count; i++)
			{
				playInstance[i].SetPitch(newPitch, moveTime);
			}
		}

		public void SetPan(int instanceId, float newPan, float moveTime)
		{
			for (int i = 0; i < playInstance.Count; i++)
			{
				AudioInstance audioInstance = playInstance[i];
				if (audioInstance.GetInstanceID() == instanceId)
				{
					audioInstance.SetPan(newPan, moveTime);
					break;
				}
			}
		}

		public void SetPanAll(float newPan, float moveTime)
		{
			for (int i = 0; i < playInstance.Count; i++)
			{
				playInstance[i].SetPan(newPan, moveTime);
			}
		}

		public void SetPosition(int instanceId, Vector3 position)
		{
			for (int i = 0; i < playInstance.Count; i++)
			{
				AudioInstance audioInstance = playInstance[i];
				if (audioInstance.GetInstanceID() == instanceId)
				{
					audioInstance.SetPosition(position);
					break;
				}
			}
		}

		public void SetPositionAll(Vector3 position)
		{
			for (int i = 0; i < playInstance.Count; i++)
			{
				playInstance[i].SetPosition(position);
			}
		}

		public void UpdateVolumeFactor(float volumeFactor)
		{
			for (int i = 0; i < playInstance.Count; i++)
			{
				playInstance[i].UpdateVolumeFactor(volumeFactor);
			}
		}

		public float GetPlayTime(int instanceId)
		{
			for (int i = 0; i < playInstance.Count; i++)
			{
				AudioInstance audioInstance = playInstance[i];
				if (audioInstance.GetInstanceID() == instanceId)
				{
					return audioInstance.GetPlayTime();
				}
			}
			return -1f;
		}

		public float GetPlayTime()
		{
			if (playInstance.Count != 0)
			{
				return playInstance[playInstance.Count - 1].GetPlayTime();
			}
			return -1f;
		}

		public int GetPlaySamples(int instanceId)
		{
			for (int i = 0; i < playInstance.Count; i++)
			{
				AudioInstance audioInstance = playInstance[i];
				if (audioInstance.GetInstanceID() == instanceId)
				{
					return audioInstance.GetPlaySamples();
				}
			}
			return -1;
		}

		public void SetTime(int instanceId, float time)
		{
			if (playInstance.Count != 0)
			{
				playInstance[playInstance.Count - 1].SetTime(time);
			}
		}

		public void SetTimeSamples(int instanceId, int samples)
		{
			for (int i = 0; i < playInstance.Count; i++)
			{
				AudioInstance audioInstance = playInstance[i];
				if (audioInstance.GetInstanceID() == instanceId)
				{
					audioInstance.SetTimeSamples(samples);
				}
			}
		}

		public bool GetSpectrumData(int instanceId, float[] sample, int channel, FFTWindow window)
		{
			for (int i = 0; i < playInstance.Count; i++)
			{
				AudioInstance audioInstance = playInstance[i];
				if (audioInstance.GetInstanceID() == instanceId)
				{
					return audioInstance.GetSpectrumData(instanceId, sample, channel, window);
				}
			}
			return false;
		}

		public void Update()
		{
			int num = 0;
			while (num < playInstance.Count)
			{
				AudioInstance audioInstance = playInstance[num];
				audioInstance.Update();
				if (playSettings.isPlayLastSamples && num == playInstance.Count - 1)
				{
					prevPlaySamplesList[audioInstance.GetRandomIndex()] = audioInstance.GetPrevPlaySamples();
				}
				if (audioInstance.GetStatus() == AudioDefine.INSTANCE_STATUS.STOP)
				{
					audioInstance.Reset(AudioMainPool.instance);
					playInstance.RemoveAt(num);
					AudioInstancePool.instance.Deactive(audioInstance);
					if (playInstance.Count > 0)
					{
						continue;
					}
					for (int i = 0; i < playSource.Count; i++)
					{
						if (playSource[i].clip != null)
						{
							_ = playSource[i].clip.preloadAudioData;
						}
					}
				}
				else
				{
					num++;
				}
			}
		}

		public string GetSpatialGroup()
		{
			if (playSettings == null)
			{
				return null;
			}
			return playSettings.spatialGroup;
		}

		public bool IsSetSpatialGroup()
		{
			if (!(spatialSettings == null))
			{
				return true;
			}
			return false;
		}

		public void SetAudio3DSettings(Audio3DSettings setting)
		{
			spatialSettings = setting;
		}

		public bool IsPlayInterval()
		{
			bool result = true;
			if (playSettings.playInterval > 0f)
			{
				if (nextInterval > 0f)
				{
					float unscaledTime = Time.unscaledTime;
					nextInterval -= unscaledTime - prevPlayTime;
					prevPlayTime = unscaledTime;
				}
				if (nextInterval <= 0f)
				{
					result = true;
					nextInterval = 0f;
				}
				else
				{
					result = false;
				}
			}
			return result;
		}

		private void setInterval()
		{
			if (playSettings.playInterval > 0f)
			{
				prevPlayTime = Time.unscaledTime;
				nextInterval = playSettings.playInterval;
			}
		}

		public void UpdateAudio3DSettings(Audio3DSettings settings)
		{
			for (int i = 0; i < playInstance.Count; i++)
			{
				AudioInstance audioInstance = playInstance[i];
				if (!force2D)
				{
					audioInstance.UpdateAudio3DSettings(settings);
				}
			}
		}
	}

	private class AudioXmlLoad
	{
		public static XmlDocument Load(Stream xml)
		{
			XmlDocument xmlDocument = new XmlDocument();
			XmlReader xmlReader = XmlReader.Create(xml);
			try
			{
				xmlDocument.Load(xmlReader);
			}
			catch (Exception)
			{
			}
			finally
			{
				xmlReader.Close();
			}
			return xmlDocument;
		}

		public static XmlDocument Load(Stream xsd, Stream xml)
		{
			XmlDocument xmlDocument = new XmlDocument();
			XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
			xmlReaderSettings.ValidationType = ValidationType.Schema;
			xmlReaderSettings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
			XmlSchema schema = XmlSchema.Read(xsd, ValidationCallback);
			xmlReaderSettings.Schemas.Add(schema);
			XmlReader xmlReader = XmlReader.Create(xml, xmlReaderSettings);
			try
			{
				xmlDocument.Load(xmlReader);
			}
			catch (Exception)
			{
			}
			finally
			{
				xmlReader.Close();
			}
			return xmlDocument;
		}

		private static void DebugParse(XmlDocument xmlDoc)
		{
			XmlNodeList elementsByTagName = xmlDoc.GetElementsByTagName("MasterSet");
			if (elementsByTagName == null)
			{
				Debug.Log("not found");
				return;
			}
			Debug.Log("count:" + elementsByTagName.Count);
			for (int i = 0; i < elementsByTagName.Count; i++)
			{
				XmlNode xmlNode = elementsByTagName[i];
				for (int j = 0; j < xmlNode.ChildNodes.Count; j++)
				{
					XmlNode xmlNode2 = xmlNode.ChildNodes[j];
					Debug.Log("dataNode: " + xmlNode2.Name);
					if (xmlNode2.HasChildNodes)
					{
						XmlNode xmlNode3 = xmlNode2.ChildNodes[0];
						Debug.Log("4444444444444444444444444444444");
						Debug.Log("NodeType; " + xmlNode3.NodeType);
						Debug.Log("LocalName; " + xmlNode3.LocalName);
						Debug.Log("Name; " + xmlNode3.Name);
						Debug.Log("Value; " + xmlNode3.Value);
					}
				}
			}
		}

		private static void OutputXml(XmlDocument xmlDoc)
		{
			XmlElement documentElement = xmlDoc.DocumentElement;
			Debug.Log("NodeType; " + documentElement.NodeType);
			Debug.Log("LocalName; " + documentElement.LocalName);
			Debug.Log("Name; " + documentElement.Name);
			if (!documentElement.HasChildNodes)
			{
				return;
			}
			for (XmlNode xmlNode = documentElement.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
			{
				Debug.Log("111111111111111111111111111111");
				Debug.Log("NodeType; " + documentElement.NodeType);
				Debug.Log("LocalName; " + documentElement.LocalName);
				Debug.Log("Name; " + documentElement.Name);
				if (xmlNode.HasChildNodes)
				{
					for (int i = 0; i < xmlNode.ChildNodes.Count; i++)
					{
						XmlNode xmlNode2 = xmlNode.ChildNodes[i];
						Debug.Log("22222222222222222222222222222222");
						Debug.Log("NodeType; " + xmlNode2.NodeType);
						Debug.Log("LocalName; " + xmlNode2.LocalName);
						Debug.Log("Name; " + xmlNode2.Name);
						for (int j = 0; j < xmlNode2.Attributes.Count; j++)
						{
							Debug.Log("3333333333333333333333333333333");
							XmlAttribute xmlAttribute = xmlNode2.Attributes[j];
							Debug.Log("NodeType; " + xmlAttribute.NodeType);
							Debug.Log("LocalName; " + xmlAttribute.LocalName);
							Debug.Log("Name; " + xmlAttribute.Name);
						}
						if (xmlNode2.HasChildNodes)
						{
							XmlNode xmlNode3 = xmlNode2.ChildNodes[0];
							Debug.Log("4444444444444444444444444444444");
							Debug.Log("NodeType; " + xmlNode3.NodeType);
							Debug.Log("LocalName; " + xmlNode3.LocalName);
							Debug.Log("Name; " + xmlNode3.Name);
							Debug.Log("Value; " + xmlNode3.Value);
						}
					}
				}
			}
		}

		private static void ValidationCallback(object sender, ValidationEventArgs args)
		{
		}
	}

	private static AudioManager manager;

	private bool IsOnMute;

	private int AndroidSoundPoolNum = 6;

	private Dictionary<string, AudioPlayer> sourceDict = new Dictionary<string, AudioPlayer>();

	private Dictionary<int, AudioPlayer> playAudioDict = new Dictionary<int, AudioPlayer>();

	private Dictionary<string, List<int>> playCategoryDict = new Dictionary<string, List<int>>();

	private Dictionary<string, AudioCategorySettings> categoryDict = new Dictionary<string, AudioCategorySettings>();

	private Dictionary<string, AudioMasterSettings> masterDict = new Dictionary<string, AudioMasterSettings>();

	private Dictionary<string, List<string>> playDuckingTrigger = new Dictionary<string, List<string>>();

	private List<int> playAudioRemoveKey = new List<int>();

	private HashSet<AudioPlayer> playerHashSet = new HashSet<AudioPlayer>();

	private Dictionary<string, AudioClip> audioClipDict = new Dictionary<string, AudioClip>();

	private Dictionary<string, Audio3DSettings> audio3DSettings = new Dictionary<string, Audio3DSettings>();

	private AudioMixerSettings mixerSettings = new AudioMixerSettings();

	private Transform _cacheTransform;

	private Thread jsonThread;

	private bool jsonThreadFlag;

	private string jsonStr;

	private AudioMasterSettings[] tmpMaster;

	private AudioCategorySettings[] tmpCategory;

	private AudioLabelSettings[] tmpLabel;

	private AudioDefine.LOAD_XML_STATUS loadXmlStatus;

	private AudioDefine.LOAD_JSON_STATUS loadJsonStatus;

	private Transform CacheTransform
	{
		get
		{
			if (!(_cacheTransform != null))
			{
				return _cacheTransform = base.transform;
			}
			return _cacheTransform;
		}
	}

	public static bool IsInitialized()
	{
		if (!manager)
		{
			return false;
		}
		return true;
	}

	public static void Initialize(int defaultSampleRate = 0, bool isCloudStreaming = false)
	{
		if (manager == null)
		{
			if (defaultSampleRate >= 0 && !isCloudStreaming)
			{
				AudioConfiguration configuration = AudioSettings.GetConfiguration();
				configuration.sampleRate = ((defaultSampleRate == 0) ? AudioDefine.DEFAULT_SAMPLE_RATE : defaultSampleRate);
				AudioSettings.Reset(configuration);
			}
			manager = new GameObject().AddComponent<AudioManager>();
			UnityEngine.Object.DontDestroyOnLoad(manager);
		}
	}

	public static void Terminate()
	{
		if (manager != null)
		{
			manager.removeAll();
			AudioMainPool.Terminate();
			UnityEngine.Object.Destroy(manager.gameObject);
			UnityEngine.Object.Destroy(manager);
			manager = null;
		}
	}

	public static async UniTask PrepareAudioClip(string label)
	{
		string audioClipName = GetAudioClipName(label);
		if (audioClipName != null && !IsExistAudioClip(audioClipName))
		{
			AddAudioClip(await Addressables.LoadAssetAsync<AudioClip>(audioClipName + ".wav"));
			SetAudioClipToLabel(label);
		}
	}

	public static void SetAudioMixer(AudioMixer mixer)
	{
		if (manager != null)
		{
			manager.setAudioMixer(mixer);
		}
	}

	public static void UnsetAudioMixer()
	{
		if (manager != null)
		{
			manager.unsetAudioMixer();
		}
	}

	public static void SetSnapshot(string snapName, float time)
	{
		if (manager != null)
		{
			manager.setSnapshot(snapName, time);
		}
	}

	public static void SetAudioMixerExposedParam(string paramName, float value)
	{
		if (manager != null)
		{
			manager.setAudioMixerExposedParam(paramName, value);
		}
	}

	public static void SetAudio3DSettingsFromJson(string jsonStr)
	{
		if (manager != null)
		{
			manager.setAudio3DSettingsFromJson(jsonStr);
		}
	}

	public static void SetAudio3DSettings(Audio3DSettings setting)
	{
		if (manager != null)
		{
			manager.setAudio3DSettings(setting);
		}
	}

	public static void SetAudio3DSettings(Audio3DSettings[] settings)
	{
		if (manager != null)
		{
			manager.setAudio3DSettings(settings);
		}
	}

	public static bool LoadBinaryTable(byte[] tableData, int loadId = 0)
	{
		if (manager != null)
		{
			return manager.loadBinaryTable(tableData, loadId);
		}
		return false;
	}

	public static bool LoadJson(string tableData, int loadId = 0)
	{
		if (manager != null)
		{
			return manager.loadJson(tableData, loadId);
		}
		return false;
	}

	public static void AddAudioClip(AudioClip[] clips)
	{
		if (manager != null)
		{
			manager.addAudioClip(clips);
		}
	}

	public static void AddAudioClip(AudioClip clip)
	{
		if (manager != null)
		{
			manager.addAudioClip(clip);
		}
	}

	public static bool IsExistAudioClip(string clipName)
	{
		if (manager != null)
		{
			return manager.isExistAudioClip(clipName);
		}
		return false;
	}

	public static void RemoveAudioClip(string clipName)
	{
		if (manager != null)
		{
			manager.removeAudioClip(clipName);
		}
	}

	public static void RemoveAudioClipAll()
	{
		if (manager != null)
		{
			manager.removeAudioClipAll();
		}
	}

	public static bool FindLabel(string name)
	{
		if (manager != null)
		{
			return manager.findLabel(name);
		}
		return false;
	}

	public static bool FindCategory(string name)
	{
		if (manager != null)
		{
			return manager.findCategory(name);
		}
		return false;
	}

	public static bool FindMaster(string name)
	{
		if (manager != null)
		{
			return manager.findMaster(name);
		}
		return false;
	}

	public static bool CanRemoveLabel(string labelName)
	{
		if (manager != null)
		{
			return manager.canRemoveLabel(labelName);
		}
		return false;
	}

	public static bool UnsetAudioClipToLabel(string labelName)
	{
		if (manager != null)
		{
			return manager.unsetAudioClipToLabel(labelName);
		}
		return false;
	}

	public static void UnsetAudioClipToLabelLoadId(int loadId)
	{
		if (manager != null)
		{
			manager.unsetAudioClipToLabelLoadId(loadId);
		}
	}

	public static void UnsetAudioClipToLabelAll()
	{
		if (manager != null)
		{
			manager.unsetAudioClipToLabelAll();
		}
	}

	public static bool RemoveLabel(string labelName)
	{
		if (manager != null)
		{
			return manager.removeLabel(labelName);
		}
		return false;
	}

	public static void RemoveLabelLoadId(int loadId)
	{
		if (manager != null)
		{
			manager.removeLabelLoadId(loadId);
		}
	}

	public static void RemoveLabelAll()
	{
		if (manager != null)
		{
			manager.removeLabelAll();
		}
	}

	public static void RemoveAll()
	{
		if (manager != null)
		{
			manager.removeAll();
		}
	}

	public static void UpdateRandomSourceInfo(string labelName)
	{
		if (manager != null)
		{
			manager.updateRandomSourceInfo(labelName);
		}
	}

	public static void UpdateRandomSourceInfoAll()
	{
		if (manager != null)
		{
			manager.updateRandomSourceInfoAll();
		}
	}

	public static void LoadAudioData(string labelName)
	{
		if (manager != null)
		{
			manager.loadAudioData(labelName);
		}
	}

	public static void LoadAudioDataLoadId(int loadId)
	{
		if (manager != null)
		{
			manager.loadAudioDataLoadId(loadId);
		}
	}

	public static void UnloadAudioData(string labelName)
	{
		if (manager != null)
		{
			manager.unloadAudioData(labelName);
		}
	}

	public static void UnloadAudioDataAll()
	{
		if (manager != null)
		{
			manager.unloadAudioDataAll();
		}
	}

	public static void UnloadAudioDataLoadId(int loadId)
	{
		if (manager != null)
		{
			manager.unloadAudioDataLoadId(loadId);
		}
	}

	public static AudioDefine.LOAD_XML_STATUS GetLoadXmlStatus()
	{
		if (manager != null)
		{
			return manager.getLoadXmlStatus();
		}
		return AudioDefine.LOAD_XML_STATUS.ERROR;
	}

	public static AudioDefine.LOAD_JSON_STATUS GetLoadJsonStatus()
	{
		if (manager != null)
		{
			return manager.getLoadJsonStatus();
		}
		return AudioDefine.LOAD_JSON_STATUS.ERROR;
	}

	public static bool LoadMasterXml(Stream xml, Stream xsd = null)
	{
		if (manager != null)
		{
			return manager.loadMasterXml(xml, xsd);
		}
		return false;
	}

	public static bool LoadCategoryXml(Stream xml, Stream xsd = null)
	{
		if (manager != null)
		{
			return manager.loadCategoryXml(xml, xsd);
		}
		return false;
	}

	public static bool LoadLabelXml(int loadId, Stream xml, Stream xsd = null)
	{
		if (manager != null)
		{
			return manager.loadLabelXml(loadId, xml, xsd);
		}
		return false;
	}

	public static void SetDucking(string categoryName, float targetVolumeFactor, float fadeTime)
	{
		if (manager != null)
		{
			manager.setDucking(categoryName, targetVolumeFactor, fadeTime);
		}
	}

	public static void ResetDucking(string categoryName, float fadeTime)
	{
		if (manager != null)
		{
			manager.resetDucking(categoryName, fadeTime);
		}
	}

	public static void ResetDuckingAll(float fadeTime)
	{
		if (manager != null)
		{
			manager.resetDuckingAll(fadeTime);
		}
	}

	public static void ForceResetDucking(string categoryName, float fadeTime)
	{
		if (manager != null)
		{
			manager.forceResetDucking(categoryName, fadeTime);
		}
	}

	public static void ForceResetDuckingAll(float fadeTime)
	{
		if (manager != null)
		{
			manager.forceResetDuckingAll(fadeTime);
		}
	}

	public static async UniTask<int> Play(string labelName, float delay = -1f, float pan = -2000f)
	{
		if (manager != null)
		{
			return manager.play(labelName, delay, pan);
		}
		return AudioDefine.INSTANCE_ID_ERROR;
	}

	public static async UniTask<int> PlayOption(string labelName, float volume, float fadeTime, float pan, int pitch, float delay = -1f)
	{
		if (manager != null)
		{
			return manager.playOption(labelName, volume, fadeTime, pan, pitch, delay);
		}
		return AudioDefine.INSTANCE_ID_ERROR;
	}

	public static int Prepare(string labelName)
	{
		if (manager != null)
		{
			return manager.prepare(labelName);
		}
		return AudioDefine.INSTANCE_ID_ERROR;
	}

	public static int PrepareOption(string labelName, float volume, float fadeTime, float pan, int pitch)
	{
		if (manager != null)
		{
			return manager.prepareOption(labelName, volume, fadeTime, pan, pitch, isForce2D: false);
		}
		return AudioDefine.INSTANCE_ID_ERROR;
	}

	public static void PlayInstance(int instanceId, float delay = -1f)
	{
		if (manager != null)
		{
			manager.playInstance(instanceId, delay);
		}
	}

	public static int Play3D(string labelName, GameObject target, float delay = -1f)
	{
		if (manager != null)
		{
			return manager.play3D(labelName, target, delay);
		}
		return AudioDefine.INSTANCE_ID_ERROR;
	}

	public static int Play3D(string labelName, Vector3 position, float delay = -1f)
	{
		if (manager != null)
		{
			return manager.play3D(labelName, position, delay);
		}
		return AudioDefine.INSTANCE_ID_ERROR;
	}

	public static int Play3D(string labelName, Transform target, float delay = -1f)
	{
		if (manager != null)
		{
			return manager.play3D(labelName, target, delay);
		}
		return AudioDefine.INSTANCE_ID_ERROR;
	}

	public static int Play2D(string labelName, float delay = -1f)
	{
		if (manager != null)
		{
			return manager.play2D(labelName, delay);
		}
		return AudioDefine.INSTANCE_ID_ERROR;
	}

	public static void SetTrackingObject(int instanceId, GameObject target)
	{
		if (manager != null)
		{
			manager.setTrackingObject(instanceId, target);
		}
	}

	public static void SetTrackingObject(int instanceId, Transform target)
	{
		if (manager != null)
		{
			manager.setTrackingObject(instanceId, target);
		}
	}

	public static async UniTask Stop(int instanceId, float fadeTime = -1f)
	{
		if (manager != null)
		{
			manager.stop(instanceId, fadeTime);
		}
	}

	public static async UniTask StopLabel(string labelName, float fadeTime = -1f)
	{
		if (manager != null)
		{
			manager.stopLabel(labelName, fadeTime);
		}
	}

	public static async UniTask StopAll(float fadeTime = -1f)
	{
		if (manager != null)
		{
			manager.stopAll(fadeTime);
		}
	}

	public static void OnPause(int instanceId, float fadeTime = -1f)
	{
		if (manager != null)
		{
			manager.onPause(instanceId, fadeTime);
		}
	}

	public static void OnPauseAll(float fadeTime = -1f)
	{
		if (manager != null)
		{
			manager.onPauseAll(fadeTime);
		}
	}

	public static void OffPause(int instanceId, float fadeTime = -1f)
	{
		if (manager != null)
		{
			manager.offPause(instanceId, fadeTime);
		}
	}

	public static void OffPauseAll(float fadeTime = -1f)
	{
		if (manager != null)
		{
			manager.offPauseAll(fadeTime);
		}
	}

	public static void SetVolume(int instanceId, float newVolume, float moveTime)
	{
		if (manager != null)
		{
			manager.setVolume(instanceId, newVolume, moveTime);
		}
	}

	public static void SetVolume(string labelName, float newVolume, float moveTime)
	{
		if (manager != null)
		{
			manager.setVolume(labelName, newVolume, moveTime);
		}
	}

	public static void SetPitch(int instanceId, int newPitch, float moveTime)
	{
		if (manager != null)
		{
			manager.setPitch(instanceId, newPitch, moveTime);
		}
	}

	public static void SetPitch(string labelName, int newPitch, float moveTime)
	{
		if (manager != null)
		{
			manager.setPitch(labelName, newPitch, moveTime);
		}
	}

	public static void SetPan(int instanceId, float newPan, float moveTime)
	{
		if (manager != null)
		{
			manager.setPan(instanceId, newPan, moveTime);
		}
	}

	public static void SetPan(string labelName, float newPan, float moveTime)
	{
		if (manager != null)
		{
			manager.setPan(labelName, newPan, moveTime);
		}
	}

	public static void SetPosition(int instanceId, Vector3 position)
	{
		if (manager != null)
		{
			manager.setPosition(instanceId, position);
		}
	}

	public static void SetPosition(string labelName, Vector3 position)
	{
		if (manager != null)
		{
			manager.setPosition(labelName, position);
		}
	}

	public static void ResetPlayPosition(string labelName)
	{
		if (manager != null)
		{
			manager.resetPlayPosition(labelName);
		}
	}

	public static void ResetPlayPositionAll()
	{
		if (manager != null)
		{
			manager.resetPlayPositionAll();
		}
	}

	public static float GetInstanceVolume(int instanceId)
	{
		if (manager != null)
		{
			return manager.getInstanceVolume(instanceId);
		}
		return 0f;
	}

	public static float GetInstanceCalcVolume(int instanceId)
	{
		if (manager != null)
		{
			return manager.getInstanceCalcVolume(instanceId);
		}
		return 0f;
	}

	public static void SetMasterVolume(string masterName, float volume, float moveTime = 0f)
	{
		if (manager != null)
		{
			manager.setMasterVolume(masterName, volume, moveTime);
		}
	}

	public static float GetMasterVolume(string masterName)
	{
		if (manager != null)
		{
			return manager.getMasterVolume(masterName);
		}
		return 1f;
	}

	public static void SetCategoryVolume(string categoryName, float volume, float moveTime = 0f)
	{
		if (manager != null)
		{
			manager.setCategoryVolume(categoryName, volume, moveTime);
		}
	}

	public static float GetCategoryVolume(string categoryName)
	{
		if (manager != null)
		{
			return manager.getCategoryVolume(categoryName);
		}
		return 0f;
	}

	public static float GetLabelVolume(string labelName)
	{
		if (manager != null)
		{
			return manager.getLabelVolume(labelName);
		}
		return 0f;
	}

	public static void StopMaster(string masterName, float fadeTime = -1f)
	{
		if (manager != null)
		{
			manager.stopMaster(masterName, fadeTime);
		}
	}

	public static void OnPauseMaster(string masterName, float fadeTime = -1f)
	{
		if (manager != null)
		{
			manager.onPauseMaster(masterName, fadeTime);
		}
	}

	public static void OffPauseMaster(string masterName, float fadeTime = -1f)
	{
		if (manager != null)
		{
			manager.offPauseMaster(masterName, fadeTime);
		}
	}

	public static async UniTask StopCategory(string categoryName, float fadeTime = -1f)
	{
		if (manager != null)
		{
			manager.stopCategory(categoryName, fadeTime);
		}
	}

	public static void OnPauseLabel(string labelName, float fadeTime = -1f)
	{
		if (manager != null)
		{
			manager.onPauseLabel(labelName, fadeTime);
		}
	}

	public static void OffPauseLabel(string labelName, float fadeTime = -1f)
	{
		if (manager != null)
		{
			manager.offPauseLabel(labelName, fadeTime);
		}
	}

	public static async UniTask OnPauseCategory(string categoryName, float fadeTime = -1f)
	{
		if (manager != null)
		{
			manager.onPauseCategory(categoryName, fadeTime);
		}
	}

	public static async UniTask OffPauseCategory(string categoryName, float fadeTime = -1f)
	{
		if (manager != null)
		{
			manager.offPauseCategory(categoryName, fadeTime);
		}
	}

	public static void OffPauseCategory(string categoryName, List<int> instanceList, float fadeTime = -1f)
	{
		if (manager != null)
		{
			manager.offPauseCategory(categoryName, instanceList, fadeTime);
		}
	}

	public static AudioDefine.INSTANCE_STATUS GetInstanceStatus(int instanceId)
	{
		if (manager != null)
		{
			return manager.getInstanceStatus(instanceId);
		}
		return AudioDefine.INSTANCE_STATUS.STOP;
	}

	public static bool IsPlayingLabel(string labelName)
	{
		if (manager != null)
		{
			return manager.isPlayingLabel(labelName);
		}
		return false;
	}

	public static int GetLabelNum()
	{
		if (manager != null)
		{
			return manager.getLabelNum();
		}
		return 0;
	}

	public static string[] GetLabelNameList()
	{
		if (manager != null)
		{
			return manager.getLabelNameList();
		}
		return null;
	}

	public static int GetCategoryNum()
	{
		if (manager != null)
		{
			return manager.getCategoryNum();
		}
		return 0;
	}

	public static string[] GetCategoryNameList()
	{
		if (manager != null)
		{
			return manager.getCategoryNameList();
		}
		return null;
	}

	public static int GetMasterNum()
	{
		if (manager != null)
		{
			return manager.getMasterNum();
		}
		return 0;
	}

	public static string[] GetMasterNameList()
	{
		if (manager != null)
		{
			return manager.getMasterNameList();
		}
		return null;
	}

	public static string GetCategoryNameSettingOfLabel(string labelName)
	{
		if (manager != null)
		{
			return manager.getCategoryNameSettingOfLabel(labelName);
		}
		return null;
	}

	public static string GetMasterNameSettingOfCategory(string categoryName)
	{
		if (manager != null)
		{
			return manager.getMasterNameSettingOfCategory(categoryName);
		}
		return null;
	}

	public static float GetPlayTime(int instanceId)
	{
		if (manager != null)
		{
			return manager.getPlayTime(instanceId);
		}
		return -1f;
	}

	public static int GetPlaySamples(int instanceId)
	{
		if (manager != null)
		{
			return manager.getPlaySamples(instanceId);
		}
		return -1;
	}

	public static void SetTime(int instanceId, float time)
	{
		if (manager != null)
		{
			manager.setTime(instanceId, time);
		}
	}

	public static void SetTimeSamples(int instanceId, int samples)
	{
		if (manager != null)
		{
			manager.setTimeSamples(instanceId, samples);
		}
	}

	public static void SetMute(bool onMute)
	{
		if (manager != null)
		{
			manager.setMute(onMute);
		}
	}

	public static bool GetMuteStatus()
	{
		if (manager != null)
		{
			return manager.getMuteStatus();
		}
		return false;
	}

	public static string[] GetAudioClipNameLoadId(int loadId)
	{
		if (manager != null)
		{
			return manager.getAudioClipNameLoadId(loadId);
		}
		return null;
	}

	public static string[] GetAudioClipNameAll()
	{
		if (manager != null)
		{
			return manager.getAudioClipNameAll();
		}
		return null;
	}

	public static string GetAudioClipName(string labelName)
	{
		if (manager != null)
		{
			return manager.getAudioClipName(labelName);
		}
		return null;
	}

	public static string[] GetAudioClipNames(string labelName)
	{
		if (manager != null)
		{
			return manager.getAudioClipNames(labelName);
		}
		return null;
	}

	public static string[] GetRandomSourceNames(string labelName)
	{
		if (manager != null)
		{
			return manager.getRandomSourceNames(labelName);
		}
		return null;
	}

	public static void SetAudioClipToLabelLoadId(int loadId)
	{
		if (manager != null)
		{
			manager.setAudioClipToLabelLoadId(loadId);
		}
	}

	public static void SetAudioClipToLabelAll()
	{
		if (manager != null)
		{
			manager.setAudioClipToLabelAll();
		}
	}

	public static void SetAudioClipToLabel(string labelName)
	{
		if (manager != null)
		{
			manager.setAudioClipToLabel(labelName);
		}
	}

	public static void SetAndroidNativeToLabel(string labelName, string filePath, string className, string funcName)
	{
		if (manager != null)
		{
			manager.setAndroidNativeToLabel(labelName, filePath, className, funcName);
		}
	}

	public static void ClearObjectPool()
	{
		if (manager != null)
		{
			manager.clearObjectPool();
		}
	}

	public static float GetLabelLength(string labelName)
	{
		if (manager != null)
		{
			return manager.getLabelLength(labelName);
		}
		return 0f;
	}

	public static int GetLabelSamples(string labelName)
	{
		if (manager != null)
		{
			return manager.getLabelSamples(labelName);
		}
		return 0;
	}

	public static bool GetSpectrumData(int instanceId, float[] sample, int channel, FFTWindow window)
	{
		if (manager != null)
		{
			return manager.getSpectrumData(instanceId, sample, channel, window);
		}
		return false;
	}

	public static bool IsLoop(string labelName)
	{
		if (manager != null)
		{
			return manager.isLoop(labelName);
		}
		return false;
	}

	public static int GetLabelMaxPlaybacksNum(string labelName)
	{
		if (manager != null)
		{
			return manager.getLabelMaxPlaybacksNum(labelName);
		}
		return -1;
	}

	public static int GetCategoryMaxPlaybacksNum(string categoryName)
	{
		if (manager != null)
		{
			return manager.getCategoryMaxPlaybacksNum(categoryName);
		}
		return -1;
	}

	public static int GetCategoryMaxPlaybacksNumFromLabel(string labelName)
	{
		if (manager != null)
		{
			return manager.getCategoryMaxPlaybacksNumFromLabel(labelName);
		}
		return -1;
	}

	public static bool IsInterval(string labelName)
	{
		if (manager != null)
		{
			return manager.isInterval(labelName);
		}
		return false;
	}

	public static int GetCurrentPlayNum()
	{
		if (manager != null)
		{
			return manager.getCurrentPlayNum();
		}
		return 0;
	}

	private void OnDestroy()
	{
		USndAndroidNativePlayer.Terminate();
	}

	private void OnApplicationPause(bool status)
	{
	}

	private void OnApplicationFocus(bool status)
	{
		if (Application.platform == RuntimePlatform.Android && status)
		{
			USndPlugin.SetAudioFocus();
		}
		SetMannerMode();
	}

	private void onHeadsetPlugCallback(string status)
	{
		if (Application.platform != RuntimePlatform.Android || !AudioDefine.ANDROID_MANNER_MODE_MUTE)
		{
			return;
		}
		if (status.CompareTo("mute_on") == 0 || status.CompareTo("mute_off") == 0)
		{
			SetMannerMode();
		}
		else if (USndPlugin.IsMannerMode())
		{
			if (status.CompareTo("noisy") == 0)
			{
				SetMannerMode(onMute: true);
			}
			else if (status.CompareTo("false") == 0)
			{
				SetMannerMode();
			}
			else
			{
				SetMannerMode(onMute: false);
			}
		}
		else
		{
			SetMannerMode(onMute: false);
		}
	}

	private void SetMannerMode(bool onMute)
	{
		if (!AudioDefine.ANDROID_MANNER_MODE_MUTE)
		{
			return;
		}
		foreach (KeyValuePair<string, AudioMasterSettings> item in masterDict)
		{
			item.Value.SetMannerMode(onMute);
		}
	}

	private void SetMannerMode()
	{
		if (!AudioDefine.ANDROID_MANNER_MODE_MUTE)
		{
			return;
		}
		if (USndPlugin.IsMannerMode())
		{
			if (USndPlugin.IsSpeaker())
			{
				SetMannerMode(onMute: true);
			}
			else
			{
				SetMannerMode(onMute: false);
			}
		}
		else
		{
			SetMannerMode(onMute: false);
		}
	}

	private void Awake()
	{
		base.name = "USndAudioManager";
		AudioInstancePool.Initialize();
		USndPlugin.Init(base.name, "onHeadsetPlugCallback");
		USndAndroidNativePlayer.Initialize(AndroidSoundPoolNum);
		SetMannerMode();
		AudioMainPool.Initialize(base.gameObject);
	}

	public void setAudioMixer(AudioMixer mixer)
	{
		if (mixer != null)
		{
			mixerSettings.SetAudioMixer(mixer);
		}
	}

	public void unsetAudioMixer()
	{
		mixerSettings.SetAudioMixer(null);
	}

	public void setSnapshot(string snapName, float time)
	{
		if (mixerSettings != null)
		{
			mixerSettings.SetSnapshot(snapName, time);
		}
	}

	public void setAudioMixerExposedParam(string paramName, float value)
	{
		if (mixerSettings != null)
		{
			mixerSettings.SetFloat(paramName, value);
		}
	}

	public void setAudio3DSettingsFromJson(string jsonStr)
	{
		Audio3DSettings audio3DSettings = D3SettingsFromJson(jsonStr);
		if (audio3DSettings.spatialName != "")
		{
			this.audio3DSettings.Add(audio3DSettings.spatialName, audio3DSettings);
		}
	}

	public void setAudio3DSettings(Audio3DSettings setting)
	{
		if (setting.spatialName != "")
		{
			audio3DSettings.Add(setting.spatialName, setting);
		}
	}

	public void setAudio3DSettings(Audio3DSettings[] settings)
	{
		for (int i = 0; i < settings.Length; i++)
		{
			if (settings[i].spatialName != "")
			{
				audio3DSettings.Add(settings[i].spatialName, settings[i]);
			}
		}
	}

	private string getChunk(byte[] tableData, int startIndex)
	{
		byte[] array = new byte[4];
		Buffer.BlockCopy(tableData, startIndex, array, 0, 4);
		return Encoding.UTF8.GetString(array);
	}

	public bool loadBinaryTable(byte[] tableData, int loadId)
	{
		int num = 0;
		string chunk = getChunk(tableData, 0);
		num += 4;
		if (chunk.CompareTo("ver ") != 0)
		{
			return false;
		}
		int num2 = BitConverter.ToInt32(tableData, num);
		num += 4;
		int tableVer = num2;
		if (num2 < AudioDefine.TABLE_LOWER_VERSION || AudioDefine.TABLE_UPPER_VERSION < num2)
		{
			return false;
		}
		num2 = BitConverter.ToInt32(tableData, num);
		num += 4;
		getString(tableData, ref num);
		chunk = getChunk(tableData, num);
		num += 4;
		if (chunk.CompareTo("mstr") == 0)
		{
			return loadMasterBinary(tableData, ref num);
		}
		if (chunk.CompareTo("ctgr") == 0)
		{
			return loadCategoryBinary(tableData, ref num);
		}
		if (chunk.CompareTo("lbl ") == 0)
		{
			return loadLabelBinary(tableData, ref num, loadId, tableVer);
		}
		return false;
	}

	private string getString(byte[] tableData, ref int startIndex)
	{
		int num = BitConverter.ToInt32(tableData, startIndex);
		startIndex += 4;
		if (num == 0)
		{
			return null;
		}
		byte[] array = new byte[num];
		Buffer.BlockCopy(tableData, startIndex, array, 0, num);
		startIndex += num;
		return Encoding.UTF8.GetString(array);
	}

	public bool loadJson(string tableData, int loadId)
	{
		if (tableData != "" && loadJsonStatus != AudioDefine.LOAD_JSON_STATUS.LOADING)
		{
			loadJsonStatus = AudioDefine.LOAD_JSON_STATUS.LOADING;
			StartCoroutine(loadJsonImpl(tableData, loadId));
			return true;
		}
		if (tableData == "" && loadJsonStatus != AudioDefine.LOAD_JSON_STATUS.LOADING)
		{
			loadJsonStatus = AudioDefine.LOAD_JSON_STATUS.ERROR;
		}
		return false;
	}

	private IEnumerator loadJsonImpl(string tableData, int loadId)
	{
		jsonThreadFlag = true;
		jsonStr = tableData;
		tmpMaster = null;
		tmpCategory = null;
		tmpLabel = null;
		jsonThread = new Thread(jsonParse);
		jsonThread.Start();
		while (jsonThreadFlag)
		{
			yield return null;
		}
		if (tmpMaster != null)
		{
			addMasterSettings(tmpMaster);
			tmpMaster = null;
		}
		else if (tmpCategory != null)
		{
			addCategorySettings(tmpCategory);
			tmpCategory = null;
		}
		else if (tmpLabel != null)
		{
			loadLabelJson(loadId);
			tmpLabel = null;
		}
		loadJsonStatus = AudioDefine.LOAD_JSON_STATUS.FINISH;
	}

	private void jsonParse()
	{
		if (jsonStr.IndexOf("{\"master\":[") >= 0)
		{
			tmpMaster = MasterFromJson<AudioMasterSettings>(jsonStr);
		}
		else if (jsonStr.IndexOf("{\"category\":[") >= 0)
		{
			tmpCategory = CategoryFromJson<AudioCategorySettings>(jsonStr);
		}
		else if (jsonStr.IndexOf("{\"label\":[") >= 0)
		{
			tmpLabel = LabelFromJson<AudioLabelSettings>(jsonStr);
		}
		jsonThreadFlag = false;
	}

	private T[] MasterFromJson<T>(string json)
	{
		return JsonUtility.FromJson<Wrapper<T>>(json).master;
	}

	private T[] CategoryFromJson<T>(string json)
	{
		return JsonUtility.FromJson<Wrapper<T>>(json).category;
	}

	private T[] LabelFromJson<T>(string json)
	{
		return JsonUtility.FromJson<Wrapper<T>>(json).label;
	}

	private Audio3DSettings D3SettingsFromJson(string json)
	{
		AudioSourceWrapper audioSourceWrapper = JsonUtility.FromJson<AudioSourceWrapper>(json);
		Audio3DSettings obj = ScriptableObject.CreateInstance<Audio3DSettings>();
		obj.spatialName = audioSourceWrapper.spatialName;
		obj.spatialBlend = audioSourceWrapper.spatialBlend;
		obj.reverbZoneMix = audioSourceWrapper.reverbZoneMix;
		obj.dopplerLevel = audioSourceWrapper.dopplerLevel;
		obj.spread = audioSourceWrapper.spread;
		obj.rolloffMode = audioSourceWrapper.rolloffMode;
		obj.minDistance = audioSourceWrapper.minDistance;
		obj.maxDistance = audioSourceWrapper.maxDistance;
		obj.customRolloffCurve = audioSourceWrapper.customRolloffCurve;
		obj.spatialBlendCurve = audioSourceWrapper.spatialBlendCurve;
		obj.reverbZoneMixCurve = audioSourceWrapper.reverbZoneMixCurve;
		obj.spreadCurve = audioSourceWrapper.spreadCurve;
		return obj;
	}

	private bool loadLabelJson(int loadId)
	{
		int num = tmpLabel.Length;
		for (int i = 0; i < num; i++)
		{
			string text = tmpLabel[i].name;
			AudioPlayer value = null;
			AudioLabelSettings audioLabelSettings = null;
			if (sourceDict.TryGetValue(text, out value))
			{
				value.StopAll(0f);
				removeLabel(text);
			}
			value = new AudioPlayer();
			value.PlayerName = text;
			audioLabelSettings = tmpLabel[i];
			audioLabelSettings.loadId = loadId;
			bool flag = false;
			AudioClip clip = null;
			if (categoryDict.TryGetValue(audioLabelSettings.GetCategoryName(), out var _))
			{
				audioLabelSettings.SetAttachCategoryInstance(categoryDict[audioLabelSettings.GetCategoryName()]);
			}
			else
			{
				flag = true;
			}
			if (flag)
			{
				deleteAudioSource(value);
				continue;
			}
			sourceDict.Add(value.PlayerName, value);
			value.Init(clip, audioLabelSettings.GetClipName(), audioLabelSettings, sourceDict);
			setUnityAudioMixer(value);
		}
		updateRandomSourceInfoAll();
		return true;
	}

	private bool loadMasterBinary(byte[] tableData, ref int startIndex)
	{
		int num = BitConverter.ToInt32(tableData, startIndex);
		startIndex += 4;
		AudioMasterSettings[] array = new AudioMasterSettings[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = new AudioMasterSettings();
			array[i].masterName = getString(tableData, ref startIndex);
			array[i].volume = BitConverter.ToSingle(tableData, startIndex);
			startIndex += 4;
		}
		addMasterSettings(array);
		return true;
	}

	private bool loadCategoryBinary(byte[] tableData, ref int startIndex)
	{
		int num = BitConverter.ToInt32(tableData, startIndex);
		startIndex += 4;
		AudioCategorySettings[] array = new AudioCategorySettings[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = new AudioCategorySettings();
			array[i].categoryName = getString(tableData, ref startIndex);
			array[i].volume = BitConverter.ToSingle(tableData, startIndex);
			startIndex += 4;
			array[i].maxPlaybacksNum = BitConverter.ToInt32(tableData, startIndex);
			startIndex += 4;
			array[i].masterName = getString(tableData, ref startIndex);
		}
		addCategorySettings(array);
		return true;
	}

	private bool loadLabelBinary(byte[] tableData, ref int startIndex, int loadId, int tableVer)
	{
		int num = BitConverter.ToInt32(tableData, startIndex);
		startIndex += 4;
		for (int i = 0; i < num; i++)
		{
			string @string = getString(tableData, ref startIndex);
			AudioPlayer value = null;
			AudioLabelSettings audioLabelSettings = null;
			bool flag = false;
			if (sourceDict.TryGetValue(@string, out value))
			{
				value.StopAll(0f);
				audioLabelSettings = value.GetLabelSettings();
			}
			else
			{
				value = new AudioPlayer();
				value.PlayerName = @string;
				audioLabelSettings = new AudioLabelSettings();
				audioLabelSettings.name = @string;
				flag = true;
			}
			audioLabelSettings.loadId = loadId;
			bool flag2 = false;
			AudioClip value2 = null;
			audioLabelSettings.clipName = getString(tableData, ref startIndex);
			audioClipDict.TryGetValue(audioLabelSettings.clipName, out value2);
			audioLabelSettings.isLoop = ((BitConverter.ToInt32(tableData, startIndex) != 0) ? true : false);
			startIndex += 4;
			audioLabelSettings.volume = BitConverter.ToSingle(tableData, startIndex);
			startIndex += 4;
			audioLabelSettings.maxPlaybacksBehavior = (AudioLabelSettings.BEHAVIOR)BitConverter.ToInt32(tableData, startIndex);
			startIndex += 4;
			audioLabelSettings.priority = BitConverter.ToInt32(tableData, startIndex);
			startIndex += 4;
			audioLabelSettings.categoryName = getString(tableData, ref startIndex);
			if (categoryDict.TryGetValue(audioLabelSettings.GetCategoryName(), out var _))
			{
				audioLabelSettings.SetAttachCategoryInstance(categoryDict[audioLabelSettings.GetCategoryName()]);
			}
			else
			{
				flag2 = true;
			}
			audioLabelSettings.singleGroup = getString(tableData, ref startIndex);
			audioLabelSettings.maxPlaybacksNum = BitConverter.ToInt32(tableData, startIndex);
			startIndex += 4;
			audioLabelSettings.isStealOldest = ((BitConverter.ToInt32(tableData, startIndex) != 0) ? true : false);
			startIndex += 4;
			audioLabelSettings.unityMixerName = getString(tableData, ref startIndex);
			audioLabelSettings.spatialGroup = getString(tableData, ref startIndex);
			audioLabelSettings.playStartDelay = BitConverter.ToSingle(tableData, startIndex);
			startIndex += 4;
			if (tableVer >= AudioDefine.TABLE_ADD_INTERVAL_VERSION)
			{
				audioLabelSettings.playInterval = BitConverter.ToSingle(tableData, startIndex);
				startIndex += 4;
			}
			audioLabelSettings.pan = BitConverter.ToSingle(tableData, startIndex);
			startIndex += 4;
			audioLabelSettings.pitchShiftCent = BitConverter.ToInt32(tableData, startIndex);
			startIndex += 4;
			audioLabelSettings.isPlayLastSamples = ((BitConverter.ToInt32(tableData, startIndex) != 0) ? true : false);
			startIndex += 4;
			audioLabelSettings.fadeInTime = BitConverter.ToSingle(tableData, startIndex);
			startIndex += 4;
			audioLabelSettings.fadeOutTime = BitConverter.ToSingle(tableData, startIndex);
			startIndex += 4;
			audioLabelSettings.fadeInTimeOldSamples = BitConverter.ToSingle(tableData, startIndex);
			startIndex += 4;
			audioLabelSettings.fadeOutTimeOnPause = BitConverter.ToSingle(tableData, startIndex);
			startIndex += 4;
			audioLabelSettings.fadeInTimeOffPause = BitConverter.ToSingle(tableData, startIndex);
			startIndex += 4;
			audioLabelSettings.isVolumeRandom = ((BitConverter.ToInt32(tableData, startIndex) != 0) ? true : false);
			startIndex += 4;
			audioLabelSettings.inconsecutiveVolume = ((BitConverter.ToInt32(tableData, startIndex) != 0) ? true : false);
			startIndex += 4;
			audioLabelSettings.volumeRandomMin = BitConverter.ToSingle(tableData, startIndex);
			startIndex += 4;
			audioLabelSettings.volumeRandomMax = BitConverter.ToSingle(tableData, startIndex);
			startIndex += 4;
			audioLabelSettings.volumeRandomUnit = BitConverter.ToSingle(tableData, startIndex);
			startIndex += 4;
			audioLabelSettings.isPitchRandom = ((BitConverter.ToInt32(tableData, startIndex) != 0) ? true : false);
			startIndex += 4;
			audioLabelSettings.inconsecutivePitch = ((BitConverter.ToInt32(tableData, startIndex) != 0) ? true : false);
			startIndex += 4;
			audioLabelSettings.pitchRandomMin = BitConverter.ToInt32(tableData, startIndex);
			startIndex += 4;
			audioLabelSettings.pitchRandomMax = BitConverter.ToInt32(tableData, startIndex);
			startIndex += 4;
			audioLabelSettings.pitchRandomUnit = BitConverter.ToInt32(tableData, startIndex);
			startIndex += 4;
			audioLabelSettings.isPanRandom = ((BitConverter.ToInt32(tableData, startIndex) != 0) ? true : false);
			startIndex += 4;
			audioLabelSettings.inconsecutivePan = ((BitConverter.ToInt32(tableData, startIndex) != 0) ? true : false);
			startIndex += 4;
			audioLabelSettings.panRandomMin = BitConverter.ToSingle(tableData, startIndex);
			startIndex += 4;
			audioLabelSettings.panRandomMax = BitConverter.ToSingle(tableData, startIndex);
			startIndex += 4;
			audioLabelSettings.panRandomUnit = BitConverter.ToSingle(tableData, startIndex);
			startIndex += 4;
			audioLabelSettings.isRandomPlay = ((BitConverter.ToInt32(tableData, startIndex) != 0) ? true : false);
			startIndex += 4;
			audioLabelSettings.inconsecutiveSource = ((BitConverter.ToInt32(tableData, startIndex) != 0) ? true : false);
			startIndex += 4;
			int num2 = BitConverter.ToInt32(tableData, startIndex);
			startIndex += 4;
			if (num2 != 0)
			{
				audioLabelSettings.randomSource = new string[num2];
				for (int j = 0; j < num2; j++)
				{
					audioLabelSettings.randomSource[j] = getString(tableData, ref startIndex);
				}
			}
			audioLabelSettings.isMovePitch = ((BitConverter.ToInt32(tableData, startIndex) != 0) ? true : false);
			startIndex += 4;
			audioLabelSettings.pitchStart = BitConverter.ToInt32(tableData, startIndex);
			startIndex += 4;
			audioLabelSettings.pitchEnd = BitConverter.ToInt32(tableData, startIndex);
			startIndex += 4;
			audioLabelSettings.pitchMoveTime = BitConverter.ToSingle(tableData, startIndex);
			startIndex += 4;
			audioLabelSettings.isMovePan = ((BitConverter.ToInt32(tableData, startIndex) != 0) ? true : false);
			startIndex += 4;
			audioLabelSettings.panStart = BitConverter.ToSingle(tableData, startIndex);
			startIndex += 4;
			audioLabelSettings.panEnd = BitConverter.ToSingle(tableData, startIndex);
			startIndex += 4;
			audioLabelSettings.panMoveTime = BitConverter.ToSingle(tableData, startIndex);
			startIndex += 4;
			num2 = BitConverter.ToInt32(tableData, startIndex);
			startIndex += 4;
			if (num2 != 0)
			{
				audioLabelSettings.duckingCategories = new string[num2];
				for (int k = 0; k < num2; k++)
				{
					audioLabelSettings.duckingCategories[k] = getString(tableData, ref startIndex);
				}
			}
			audioLabelSettings.duckingStartTime = BitConverter.ToSingle(tableData, startIndex);
			startIndex += 4;
			audioLabelSettings.duckingEndTime = BitConverter.ToSingle(tableData, startIndex);
			startIndex += 4;
			audioLabelSettings.duckingVolumeFactor = BitConverter.ToSingle(tableData, startIndex);
			startIndex += 4;
			audioLabelSettings.autoRestoreDucking = ((BitConverter.ToInt32(tableData, startIndex) != 0) ? true : false);
			startIndex += 4;
			audioLabelSettings.restoreTime = BitConverter.ToSingle(tableData, startIndex);
			startIndex += 4;
			if (tableVer >= AudioDefine.TABLE_ADD_IS_ANDROID_NATIVE_VERSION)
			{
				audioLabelSettings.isAndroidNative = ((BitConverter.ToInt32(tableData, startIndex) != 0) ? true : false);
				startIndex += 4;
			}
			if (flag2)
			{
				deleteAudioSource(value);
				continue;
			}
			if (flag)
			{
				sourceDict.Add(value.PlayerName, value);
				value.Init(value2, audioLabelSettings.GetClipName(), audioLabelSettings, sourceDict);
			}
			setUnityAudioMixer(value);
		}
		updateRandomSourceInfoAll();
		return true;
	}

	public void addCategorySettings(AudioCategorySettings[] list)
	{
		if (list != null)
		{
			foreach (AudioCategorySettings category in list)
			{
				addCategorySettings(category);
			}
		}
	}

	public bool addCategorySettings(AudioCategorySettings category)
	{
		if (category == null)
		{
			return false;
		}
		if (!categoryDict.TryGetValue(category.categoryName, out var value))
		{
			string categoryName = category.categoryName;
			categoryDict.Add(categoryName, category);
			attachMasterSettings(category);
			if (!playCategoryDict.ContainsKey(categoryName))
			{
				List<int> value2 = new List<int>((category.maxPlaybacksNum > 0) ? category.maxPlaybacksNum : AudioDefine.LIST_CAPACITY);
				playCategoryDict.Add(categoryName, value2);
			}
			if (!playDuckingTrigger.ContainsKey(categoryName))
			{
				List<string> value3 = new List<string>();
				playDuckingTrigger.Add(categoryName, value3);
			}
			if (category.maxPlaybacksNum > 0)
			{
				AudioMainPool.instance.AddEmpty(category.maxPlaybacksNum);
			}
			return true;
		}
		value.CopySettings(category);
		attachMasterSettings(value);
		return false;
	}

	public void addMasterSettings(AudioMasterSettings[] list)
	{
		if (list != null)
		{
			foreach (AudioMasterSettings master in list)
			{
				addMasterSettings(master);
			}
		}
	}

	public bool addMasterSettings(AudioMasterSettings master)
	{
		if (master == null)
		{
			return false;
		}
		if (!masterDict.TryGetValue(master.masterName, out var value))
		{
			masterDict.Add(master.masterName, master);
			master.SetMute(IsOnMute);
			SetMannerMode();
			return true;
		}
		value.CopySettings(master);
		value.SetMute(IsOnMute);
		SetMannerMode();
		return false;
	}

	public void addAudioClip(AudioClip[] clips)
	{
		for (int i = 0; i < clips.Length; i++)
		{
			addAudioClip(clips[i]);
		}
	}

	public void addAudioClip(AudioClip clip)
	{
		if (!audioClipDict.ContainsKey(clip.name))
		{
			audioClipDict.Add(clip.name, clip);
		}
		else
		{
			audioClipDict[clip.name] = clip;
		}
	}

	public bool isExistAudioClip(string clipName)
	{
		return audioClipDict.ContainsKey(clipName);
	}

	public void removeAudioClip(string clipName)
	{
		if (audioClipDict.ContainsKey(clipName))
		{
			audioClipDict.Remove(clipName);
		}
	}

	public void removeAudioClipAll()
	{
		audioClipDict.Clear();
	}

	public bool findLabel(string name)
	{
		return sourceDict.ContainsKey(name);
	}

	public bool findCategory(string name)
	{
		return categoryDict.ContainsKey(name);
	}

	public bool findMaster(string name)
	{
		return masterDict.ContainsKey(name);
	}

	public bool canRemoveLabel(string labelName)
	{
		if (sourceDict.TryGetValue(labelName, out var value) && value.GetPlayingTrueNum() != 0)
		{
			return false;
		}
		return true;
	}

	public bool unsetAudioClipToLabel(string labelName)
	{
		if (sourceDict.TryGetValue(labelName, out var value))
		{
			AudioLabelSettings labelSettings = value.GetLabelSettings();
			if (labelSettings.isAndroidNative && Application.platform == RuntimePlatform.Android)
			{
				USndAndroidNativePlayer.Unload(labelSettings.GetAndroidSoundId());
			}
			if (value.GetPlayingTrueNum() != 0)
			{
				return false;
			}
			AudioClip playClip = value.GetPlayClip();
			if (playClip != null)
			{
				audioClipDict.Remove(playClip.name);
			}
			value.ResetPlayClip();
			return true;
		}
		return true;
	}

	public void unsetAudioClipToLabelLoadId(int loadId)
	{
		foreach (KeyValuePair<string, AudioPlayer> item in sourceDict)
		{
			AudioLabelSettings labelSettings = item.Value.GetLabelSettings();
			if (labelSettings.loadId != loadId)
			{
				continue;
			}
			if (labelSettings.isAndroidNative && Application.platform == RuntimePlatform.Android)
			{
				USndAndroidNativePlayer.Unload(labelSettings.GetAndroidSoundId());
			}
			AudioPlayer value = item.Value;
			if (value.GetPlayingTrueNum() == 0)
			{
				AudioClip playClip = value.GetPlayClip();
				if (playClip != null)
				{
					audioClipDict.Remove(playClip.name);
				}
				value.ResetPlayClip();
			}
		}
	}

	public void unsetAudioClipToLabelAll()
	{
		foreach (KeyValuePair<string, AudioPlayer> item in sourceDict)
		{
			AudioPlayer value = item.Value;
			if (value.GetPlayingTrueNum() == 0)
			{
				AudioClip playClip = value.GetPlayClip();
				if (playClip != null)
				{
					audioClipDict.Remove(playClip.name);
				}
				value.ResetPlayClip();
			}
		}
	}

	public bool removeLabel(string labelName)
	{
		if (sourceDict.TryGetValue(labelName, out var value))
		{
			if (value.GetPlayingTrueNum() != 0)
			{
				return false;
			}
			resetDuckingBeforeUpdate(value);
			sourceDict.Remove(labelName);
			AudioClip playClip = value.GetPlayClip();
			if (playClip != null)
			{
				audioClipDict.Remove(playClip.name);
			}
			deleteAudioSource(value);
			return true;
		}
		return true;
	}

	public void removeLabelLoadId(int loadId)
	{
		List<string> list = new List<string>(sourceDict.Count);
		foreach (KeyValuePair<string, AudioPlayer> item in sourceDict)
		{
			if (item.Value.GetLabelSettings().loadId != loadId)
			{
				continue;
			}
			AudioPlayer value = item.Value;
			if (value.GetPlayingTrueNum() == 0)
			{
				resetDuckingBeforeUpdate(value);
				list.Add(item.Key);
				AudioClip playClip = value.GetPlayClip();
				if (playClip != null)
				{
					audioClipDict.Remove(playClip.name);
				}
				deleteAudioSource(item.Value);
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			sourceDict.Remove(list[i]);
		}
	}

	public void removeLabelAll()
	{
		stopAll();
		List<string> list = new List<string>(sourceDict.Count);
		foreach (KeyValuePair<string, AudioPlayer> item in sourceDict)
		{
			AudioPlayer value = item.Value;
			if (value.GetPlayingTrueNum() == 0)
			{
				resetDuckingBeforeUpdate(value);
				list.Add(item.Key);
				AudioClip playClip = value.GetPlayClip();
				if (playClip != null)
				{
					audioClipDict.Remove(playClip.name);
				}
				deleteAudioSource(item.Value);
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			sourceDict.Remove(list[i]);
		}
	}

	public void removeAll()
	{
		stopAll(0f);
		removeLabelAll();
		removeAudioClipAll();
		masterDict.Clear();
		categoryDict.Clear();
		clearObjectPool();
	}

	private void deleteAudioSource(AudioPlayer player)
	{
		player.Reset();
	}

	public void updateRandomSourceInfo(string labelName)
	{
		if (sourceDict.TryGetValue(labelName, out var value))
		{
			value.UpdateRandomSourceInfo(sourceDict);
		}
	}

	public void updateRandomSourceInfoAll()
	{
		foreach (KeyValuePair<string, AudioPlayer> item in sourceDict)
		{
			item.Value.UpdateRandomSourceInfo(sourceDict);
		}
	}

	public void loadAudioData(string labelName)
	{
		if (sourceDict.TryGetValue(labelName, out var value))
		{
			value.LoadAudioData();
		}
	}

	public void loadAudioDataLoadId(int loadId)
	{
		foreach (KeyValuePair<string, AudioPlayer> item in sourceDict)
		{
			AudioPlayer value = item.Value;
			if (value.GetLabelSettings().loadId == loadId)
			{
				value.LoadAudioData();
			}
		}
	}

	public void unloadAudioData(string labelName)
	{
		if (sourceDict.TryGetValue(labelName, out var value))
		{
			value.UnloadAudioData();
		}
	}

	public void unloadAudioDataAll()
	{
		foreach (KeyValuePair<string, AudioPlayer> item in sourceDict)
		{
			item.Value.UnloadAudioData();
		}
	}

	public void unloadAudioDataLoadId(int loadId)
	{
		foreach (KeyValuePair<string, AudioPlayer> item in sourceDict)
		{
			AudioPlayer value = item.Value;
			if (value.GetLabelSettings().loadId == loadId)
			{
				value.UnloadAudioData();
			}
		}
	}

	private void setUnityAudioMixer(AudioPlayer player)
	{
		AudioLabelSettings labelSettings = player.GetLabelSettings();
		if (mixerSettings != null && labelSettings.unityMixerName != null)
		{
			AudioMixerGroup[] array = mixerSettings.FindGroup(labelSettings.unityMixerName);
			if (array != null && array.Length != 0)
			{
				player.SetAudioMixerGroup(array[0]);
			}
		}
	}

	public AudioDefine.LOAD_XML_STATUS getLoadXmlStatus()
	{
		return loadXmlStatus;
	}

	public AudioDefine.LOAD_JSON_STATUS getLoadJsonStatus()
	{
		return loadJsonStatus;
	}

	public bool loadMasterXml(Stream xml, Stream xsd = null)
	{
		if (loadXmlStatus == AudioDefine.LOAD_XML_STATUS.LOADING)
		{
			return false;
		}
		loadXmlStatus = AudioDefine.LOAD_XML_STATUS.LOADING;
		StartCoroutine(loadMasterXmlCoroutine(xml, xsd));
		xml.Dispose();
		xsd?.Dispose();
		return true;
	}

	private IEnumerator loadMasterXmlCoroutine(Stream xml, Stream xsd)
	{
		XmlDocument xmlDocument = ((xsd != null) ? AudioXmlLoad.Load(xsd, xml) : AudioXmlLoad.Load(xml));
		if (xmlDocument == null)
		{
			loadXmlStatus = AudioDefine.LOAD_XML_STATUS.ERROR;
			yield break;
		}
		XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName("MasterSet");
		if (elementsByTagName == null)
		{
			loadXmlStatus = AudioDefine.LOAD_XML_STATUS.ERROR;
			yield break;
		}
		for (int i = 0; i < elementsByTagName.Count; i++)
		{
			XmlNode xmlNode = elementsByTagName[i];
			if (!xmlNode.HasChildNodes)
			{
				continue;
			}
			XmlNode xmlNode2 = xmlNode.ChildNodes[0];
			if (xmlNode2.Name.CompareTo("MasterName") != 0)
			{
				continue;
			}
			XmlNode xmlNode3 = xmlNode2.ChildNodes[0];
			AudioMasterSettings value = null;
			bool flag = false;
			if (xmlNode3 == null)
			{
				continue;
			}
			if (!masterDict.TryGetValue(xmlNode3.Value, out value) && xmlNode2.HasChildNodes)
			{
				value = new AudioMasterSettings();
				value.masterName = xmlNode3.Value;
				flag = true;
			}
			if (value != null && xmlNode.ChildNodes.Count > 1)
			{
				xmlNode2 = xmlNode.ChildNodes[1];
				if (xmlNode2.Name.CompareTo("Volume") != 0)
				{
					continue;
				}
				if (xmlNode2.HasChildNodes)
				{
					xmlNode3 = xmlNode2.ChildNodes[0];
					value.volume = float.Parse(xmlNode3.Value, CultureInfo.InvariantCulture);
				}
			}
			if (flag)
			{
				addMasterSettings(value);
			}
		}
		loadXmlStatus = AudioDefine.LOAD_XML_STATUS.FINISH;
	}

	private void attachMasterSettings(AudioCategorySettings category)
	{
		if (masterDict.TryGetValue(category.masterName, out var value))
		{
			category.SetAttachMasterInstance(value);
		}
	}

	public bool loadCategoryXml(Stream xml, Stream xsd = null)
	{
		if (loadXmlStatus == AudioDefine.LOAD_XML_STATUS.LOADING)
		{
			return false;
		}
		loadXmlStatus = AudioDefine.LOAD_XML_STATUS.LOADING;
		StartCoroutine(loadCategoryXmlCoroutine(xml, xsd));
		xml.Dispose();
		xsd?.Dispose();
		return true;
	}

	private IEnumerator loadCategoryXmlCoroutine(Stream xml, Stream xsd)
	{
		XmlDocument xmlDocument = ((xsd != null) ? AudioXmlLoad.Load(xsd, xml) : AudioXmlLoad.Load(xml));
		if (xmlDocument == null)
		{
			yield break;
		}
		XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName("CategorySet");
		if (elementsByTagName == null)
		{
			yield break;
		}
		for (int i = 0; i < elementsByTagName.Count; i++)
		{
			XmlNode xmlNode = elementsByTagName[i];
			if (!xmlNode.HasChildNodes)
			{
				continue;
			}
			XmlNode xmlNode2 = xmlNode.ChildNodes[0];
			if (xmlNode2.Name.CompareTo("CategoryName") != 0)
			{
				continue;
			}
			XmlNode xmlNode3 = xmlNode2.ChildNodes[0];
			AudioCategorySettings value = null;
			bool flag = false;
			if (xmlNode3 == null)
			{
				continue;
			}
			if (!categoryDict.TryGetValue(xmlNode3.Value, out value) && xmlNode2.HasChildNodes)
			{
				value = new AudioCategorySettings();
				value.categoryName = xmlNode3.Value;
				flag = true;
			}
			if (value != null && xmlNode.ChildNodes.Count > 1)
			{
				for (int j = 1; j < xmlNode.ChildNodes.Count; j++)
				{
					xmlNode2 = xmlNode.ChildNodes[j];
					if (xmlNode2.HasChildNodes)
					{
						xmlNode3 = xmlNode2.ChildNodes[0];
						switch (xmlNode2.Name)
						{
						case "Volume":
							value.volume = float.Parse(xmlNode3.Value, CultureInfo.InvariantCulture);
							break;
						case "MaxNum":
							value.maxPlaybacksNum = int.Parse(xmlNode3.Value);
							break;
						case "MasterName":
							value.masterName = xmlNode3.Value;
							attachMasterSettings(value);
							break;
						}
					}
				}
			}
			if (flag)
			{
				addCategorySettings(value);
			}
		}
		loadXmlStatus = AudioDefine.LOAD_XML_STATUS.FINISH;
	}

	public bool loadLabelXml(int loadId, Stream xml, Stream xsd = null)
	{
		if (loadXmlStatus == AudioDefine.LOAD_XML_STATUS.LOADING)
		{
			return false;
		}
		loadXmlStatus = AudioDefine.LOAD_XML_STATUS.LOADING;
		StartCoroutine(loadLabelXmlCoroutine(loadId, xml, xsd));
		xml.Dispose();
		xsd?.Dispose();
		return true;
	}

	private IEnumerator loadLabelXmlCoroutine(int loadId, Stream xml, Stream xsd)
	{
		XmlDocument xmlDocument = ((xsd != null) ? AudioXmlLoad.Load(xsd, xml) : AudioXmlLoad.Load(xml));
		if (xmlDocument == null)
		{
			yield break;
		}
		XmlNodeList nodeList = xmlDocument.GetElementsByTagName("LabelSet");
		if (nodeList == null)
		{
			yield break;
		}
		int i = 0;
		while (i < nodeList.Count)
		{
			XmlNode xmlNode = nodeList[i];
			if (xmlNode.HasChildNodes)
			{
				XmlNode xmlNode2 = xmlNode.ChildNodes[0];
				if (xmlNode2.Name.CompareTo("LabelName") == 0)
				{
					XmlNode xmlNode3 = xmlNode2.ChildNodes[0];
					AudioLabelSettings audioLabelSettings = null;
					AudioPlayer value = null;
					AudioClip value2 = null;
					bool flag = false;
					if (xmlNode3 != null)
					{
						if (sourceDict.TryGetValue(xmlNode3.Value, out value))
						{
							value.StopAll(0f);
							audioLabelSettings = value.GetLabelSettings();
						}
						else if (xmlNode2.HasChildNodes)
						{
							value = new AudioPlayer();
							value.PlayerName = xmlNode3.Value;
							audioLabelSettings = new AudioLabelSettings();
							audioLabelSettings.name = xmlNode3.Value;
							flag = true;
						}
						if (audioLabelSettings != null)
						{
							audioLabelSettings.loadId = loadId;
						}
						bool flag2 = false;
						bool flag3 = false;
						if (audioLabelSettings != null && xmlNode.ChildNodes.Count > 1)
						{
							for (int j = 1; j < xmlNode.ChildNodes.Count; j++)
							{
								xmlNode2 = xmlNode.ChildNodes[j];
								if (!xmlNode2.HasChildNodes)
								{
									continue;
								}
								xmlNode3 = xmlNode2.ChildNodes[0];
								switch (xmlNode2.Name)
								{
								case "FileName":
									audioClipDict.TryGetValue(xmlNode3.Value, out value2);
									audioLabelSettings.SetClipName(xmlNode3.Value);
									flag3 = true;
									break;
								case "Loop":
								{
									bool loop = bool.Parse(xmlNode3.Value);
									audioLabelSettings.SetLoop(loop);
									break;
								}
								case "Volume":
									audioLabelSettings.volume = float.Parse(xmlNode3.Value, CultureInfo.InvariantCulture);
									break;
								case "CategoryBehavior":
									if (AudioLabelSettings.BEHAVIOR.STEAL_OLDEST.ToString().CompareTo(xmlNode3.Value) == 0)
									{
										audioLabelSettings.maxPlaybacksBehavior = AudioLabelSettings.BEHAVIOR.STEAL_OLDEST;
									}
									else if (AudioLabelSettings.BEHAVIOR.JUST_FAIL.ToString().CompareTo(xmlNode3.Value) == 0)
									{
										audioLabelSettings.maxPlaybacksBehavior = AudioLabelSettings.BEHAVIOR.JUST_FAIL;
									}
									else if (AudioLabelSettings.BEHAVIOR.QUEUE.ToString().CompareTo(xmlNode3.Value) == 0)
									{
										audioLabelSettings.maxPlaybacksBehavior = AudioLabelSettings.BEHAVIOR.QUEUE;
									}
									break;
								case "Priority":
									audioLabelSettings.priority = int.Parse(xmlNode3.Value);
									break;
								case "CategoryName":
								{
									audioLabelSettings.categoryName = xmlNode3.Value;
									if (categoryDict.TryGetValue(audioLabelSettings.GetCategoryName(), out var value3))
									{
										audioLabelSettings.SetAttachCategoryInstance(value3);
									}
									else
									{
										flag2 = true;
									}
									break;
								}
								case "SingleGroup":
									audioLabelSettings.singleGroup = xmlNode3.Value;
									break;
								case "MaxNum":
									audioLabelSettings.maxPlaybacksNum = int.Parse(xmlNode3.Value);
									break;
								case "IsStealOldest":
									audioLabelSettings.isStealOldest = bool.Parse(xmlNode3.Value);
									break;
								case "UnityMixerName":
									audioLabelSettings.unityMixerName = xmlNode3.Value;
									break;
								case "SpatialGroup":
									audioLabelSettings.spatialGroup = xmlNode3.Value;
									break;
								case "Delay":
									audioLabelSettings.playStartDelay = float.Parse(xmlNode3.Value, CultureInfo.InvariantCulture);
									break;
								case "Interval":
									audioLabelSettings.playInterval = float.Parse(xmlNode3.Value, CultureInfo.InvariantCulture);
									break;
								case "Pan":
									audioLabelSettings.pan = float.Parse(xmlNode3.Value, CultureInfo.InvariantCulture);
									break;
								case "Pitch":
									audioLabelSettings.pitchShiftCent = int.Parse(xmlNode3.Value);
									break;
								case "IsLastSamples":
									audioLabelSettings.isPlayLastSamples = bool.Parse(xmlNode3.Value);
									break;
								case "FadeInTime":
									audioLabelSettings.fadeInTime = float.Parse(xmlNode3.Value, CultureInfo.InvariantCulture);
									break;
								case "FadeOutTime":
									audioLabelSettings.fadeOutTime = float.Parse(xmlNode3.Value, CultureInfo.InvariantCulture);
									break;
								case "FadeInOldSample":
									audioLabelSettings.fadeInTimeOldSamples = float.Parse(xmlNode3.Value, CultureInfo.InvariantCulture);
									break;
								case "FadeOutOnPause":
									audioLabelSettings.fadeOutTimeOnPause = float.Parse(xmlNode3.Value, CultureInfo.InvariantCulture);
									break;
								case "FadeInOffPause":
									audioLabelSettings.fadeInTimeOffPause = float.Parse(xmlNode3.Value, CultureInfo.InvariantCulture);
									break;
								case "IsVolRnd":
									audioLabelSettings.isVolumeRandom = bool.Parse(xmlNode3.Value);
									break;
								case "IncVol":
									audioLabelSettings.inconsecutiveVolume = bool.Parse(xmlNode3.Value);
									break;
								case "VolRndMin":
									audioLabelSettings.volumeRandomMin = float.Parse(xmlNode3.Value, CultureInfo.InvariantCulture);
									break;
								case "VolRndMax":
									audioLabelSettings.volumeRandomMax = float.Parse(xmlNode3.Value, CultureInfo.InvariantCulture);
									break;
								case "VolRndUnit":
									audioLabelSettings.volumeRandomUnit = float.Parse(xmlNode3.Value, CultureInfo.InvariantCulture);
									break;
								case "IsPitchRnd":
									audioLabelSettings.isPitchRandom = bool.Parse(xmlNode3.Value);
									break;
								case "IncPitch":
									audioLabelSettings.inconsecutivePitch = bool.Parse(xmlNode3.Value);
									break;
								case "PitchRndMin":
									audioLabelSettings.pitchRandomMin = int.Parse(xmlNode3.Value);
									break;
								case "PitchRndMax":
									audioLabelSettings.pitchRandomMax = int.Parse(xmlNode3.Value);
									break;
								case "PitchRndUnit":
									audioLabelSettings.pitchRandomUnit = int.Parse(xmlNode3.Value);
									break;
								case "IsPanRnd":
									audioLabelSettings.isPanRandom = bool.Parse(xmlNode3.Value);
									break;
								case "IncPan":
									audioLabelSettings.inconsecutivePan = bool.Parse(xmlNode3.Value);
									break;
								case "PanRndMin":
									audioLabelSettings.panRandomMin = float.Parse(xmlNode3.Value, CultureInfo.InvariantCulture);
									break;
								case "PanRndMax":
									audioLabelSettings.panRandomMax = float.Parse(xmlNode3.Value, CultureInfo.InvariantCulture);
									break;
								case "PanRndUnit":
									audioLabelSettings.panRandomUnit = float.Parse(xmlNode3.Value, CultureInfo.InvariantCulture);
									break;
								case "IsRndSrc":
									audioLabelSettings.isRandomPlay = bool.Parse(xmlNode3.Value);
									break;
								case "IncSrc":
									audioLabelSettings.inconsecutiveSource = bool.Parse(xmlNode3.Value);
									break;
								case "RndSrc":
									if (xmlNode3.Value != null)
									{
										string[] array2 = xmlNode3.Value.Split("\n"[0]);
										audioLabelSettings.randomSource = new string[array2.Length];
										for (int l = 0; l < array2.Length; l++)
										{
											audioLabelSettings.randomSource[l] = array2[l];
										}
									}
									break;
								case "IsMovePitch":
									audioLabelSettings.isMovePitch = bool.Parse(xmlNode3.Value);
									break;
								case "PitchStart":
									audioLabelSettings.pitchStart = int.Parse(xmlNode3.Value);
									break;
								case "PitchEnd":
									audioLabelSettings.pitchEnd = int.Parse(xmlNode3.Value);
									break;
								case "PitchMoveTime":
									audioLabelSettings.pitchMoveTime = float.Parse(xmlNode3.Value, CultureInfo.InvariantCulture);
									break;
								case "IsMovePan":
									audioLabelSettings.isMovePan = bool.Parse(xmlNode3.Value);
									break;
								case "PanStart":
									audioLabelSettings.panStart = float.Parse(xmlNode3.Value, CultureInfo.InvariantCulture);
									break;
								case "PanEnd":
									audioLabelSettings.panEnd = float.Parse(xmlNode3.Value, CultureInfo.InvariantCulture);
									break;
								case "PanMoveTime":
									audioLabelSettings.panMoveTime = float.Parse(xmlNode3.Value, CultureInfo.InvariantCulture);
									break;
								case "DuckingCategory":
									if (xmlNode3.Value != null)
									{
										string[] array = xmlNode3.Value.Split("\n"[0]);
										audioLabelSettings.duckingCategories = new string[array.Length];
										for (int k = 0; k < array.Length; k++)
										{
											audioLabelSettings.duckingCategories[k] = array[k];
										}
									}
									break;
								case "DuckStart":
									audioLabelSettings.duckingStartTime = float.Parse(xmlNode3.Value, CultureInfo.InvariantCulture);
									break;
								case "DuckEnd":
									audioLabelSettings.duckingEndTime = float.Parse(xmlNode3.Value, CultureInfo.InvariantCulture);
									break;
								case "DuckVol":
									audioLabelSettings.duckingVolumeFactor = float.Parse(xmlNode3.Value, CultureInfo.InvariantCulture);
									break;
								case "AutoRestore":
									audioLabelSettings.autoRestoreDucking = bool.Parse(xmlNode3.Value);
									break;
								case "RestoreTime":
									audioLabelSettings.restoreTime = float.Parse(xmlNode3.Value, CultureInfo.InvariantCulture);
									break;
								case "IsAndroidNative":
									audioLabelSettings.isAndroidNative = bool.Parse(xmlNode3.Value);
									break;
								}
							}
						}
						if (flag2 || !flag3)
						{
							deleteAudioSource(value);
						}
						else
						{
							if (flag)
							{
								sourceDict.Add(value.PlayerName, value);
								value.Init(value2, audioLabelSettings.GetClipName(), audioLabelSettings, sourceDict);
							}
							setUnityAudioMixer(value);
						}
						if (i % 30 == 0)
						{
							yield return null;
						}
					}
				}
			}
			int num = i + 1;
			i = num;
		}
		updateRandomSourceInfoAll();
		loadXmlStatus = AudioDefine.LOAD_XML_STATUS.FINISH;
	}

	private void orderCategoryInstanceList(List<int> playerList)
	{
		int num = 0;
		while (num < playerList.Count)
		{
			int num2 = playerList[num];
			if (playAudioDict.TryGetValue(num2, out var value))
			{
				if (value != null)
				{
					AudioDefine.INSTANCE_STATUS instanceStatus = value.GetInstanceStatus(num2);
					if (instanceStatus == AudioDefine.INSTANCE_STATUS.STOP || instanceStatus == AudioDefine.INSTANCE_STATUS.STOP_SOON)
					{
						playerList.RemoveAt(num);
					}
					else
					{
						num++;
					}
				}
				else
				{
					num++;
				}
			}
			else
			{
				playerList.RemoveAt(num);
			}
		}
	}

	private void addPlayInfo(AudioPlayer player, int instanceId)
	{
		playAudioDict.Add(instanceId, player);
		playerHashSet.Add(player);
		string categoryName = player.GetCategoryName();
		if (playCategoryDict.TryGetValue(categoryName, out var value))
		{
			value.Add(instanceId);
		}
	}

	private void stopSameSingleGroup(string singleGroup, string playLabelName)
	{
		if (singleGroup == null)
		{
			return;
		}
		foreach (KeyValuePair<int, AudioPlayer> item in playAudioDict)
		{
			AudioPlayer value = item.Value;
			if (value != null && playLabelName != null && playLabelName.CompareTo(item.Value.GetLabelSettings().name) != 0 && singleGroup.CompareTo(value.GetLabelSettings().singleGroup) == 0)
			{
				value.Stop(item.Key);
			}
		}
	}

	private RESULT checkLabelPlaybacksNum(AudioPlayer player)
	{
		if (player.GetMaxPlaybacksNum() > 0 && player.GetPlayingNum() + 1 > player.GetMaxPlaybacksNum())
		{
			if (player.IsStealOldest())
			{
				player.StopOldInstance();
				return RESULT.EXECUTE;
			}
			return RESULT.FINISH;
		}
		return RESULT.CONTINUE;
	}

	private RESULT checkCategoryPlaybacksNum(AudioPlayer player, ref float time, ref bool queueOn)
	{
		string categoryName = player.GetCategoryName();
		if (!playCategoryDict.TryGetValue(categoryName, out var value))
		{
			return RESULT.FINISH;
		}
		AudioCategorySettings audioCategorySettings = categoryDict[categoryName];
		orderCategoryInstanceList(value);
		int count = value.Count;
		queueOn = false;
		if (count + 1 > audioCategorySettings.maxPlaybacksNum)
		{
			if (player.GetMaxPlaybacksBehavior() == AudioLabelSettings.BEHAVIOR.JUST_FAIL)
			{
				return RESULT.FINISH;
			}
			if (player.GetLabelSettings().singleGroup != null)
			{
				for (int i = 0; i < value.Count; i++)
				{
					int num = value[i];
					AudioPlayer audioPlayer = playAudioDict[num];
					if (audioPlayer != null && player.GetLabelSettings().singleGroup.CompareTo(audioPlayer.GetLabelSettings().singleGroup) == 0)
					{
						audioPlayer.Stop(num);
						return RESULT.EXECUTE;
					}
				}
			}
			for (int j = 0; j < value.Count; j++)
			{
				int num2 = value[j];
				AudioPlayer audioPlayer2 = playAudioDict[num2];
				if (audioPlayer2 != null && player.GetPriority() <= audioPlayer2.GetPriority())
				{
					audioPlayer2.Stop(num2);
					if (player.GetMaxPlaybacksBehavior() == AudioLabelSettings.BEHAVIOR.QUEUE)
					{
						time = audioPlayer2.GetFadeOutTime();
						queueOn = true;
					}
					return RESULT.EXECUTE;
				}
			}
			return RESULT.FINISH;
		}
		return RESULT.EXECUTE;
	}

	private void startDucking(AudioPlayer player, int instanceId)
	{
		AudioLabelSettings labelSettings = player.GetLabelSettings();
		if (labelSettings.duckingCategories == null)
		{
			return;
		}
		for (int i = 0; i < labelSettings.duckingCategories.Length; i++)
		{
			string key = labelSettings.duckingCategories[i];
			if (playDuckingTrigger.TryGetValue(key, out var value))
			{
				if (labelSettings.autoRestoreDucking && !value.Contains(player.PlayerName))
				{
					value.Add(player.PlayerName);
				}
				categoryDict[key].SetDuckingVolumeUpdater(labelSettings.duckingVolumeFactor, labelSettings.duckingStartTime, isLow: true);
			}
		}
	}

	public void setDucking(string categoryName, float targetVolumeFactor, float fadeTime)
	{
		if (categoryDict.TryGetValue(categoryName, out var value))
		{
			value.SetDuckingVolumeUpdater(targetVolumeFactor, fadeTime, isLow: true);
		}
	}

	public void resetDucking(string categoryName, float fadeTime)
	{
		if (categoryDict.TryGetValue(categoryName, out var value))
		{
			value.SetDuckingVolumeUpdater(1f, fadeTime, isLow: false);
		}
	}

	public void resetDuckingAll(float fadeTime)
	{
		foreach (KeyValuePair<string, AudioCategorySettings> item in categoryDict)
		{
			item.Value.SetDuckingVolumeUpdater(1f, fadeTime, isLow: false);
		}
	}

	public void forceResetDucking(string categoryName, float fadeTime)
	{
		if (categoryDict.TryGetValue(categoryName, out var value))
		{
			value.SetDuckingVolumeUpdater(1f, fadeTime, isLow: false);
		}
		if (playDuckingTrigger.TryGetValue(categoryName, out var value2))
		{
			value2.Clear();
		}
	}

	public void forceResetDuckingAll(float fadeTime)
	{
		foreach (KeyValuePair<string, AudioCategorySettings> item in categoryDict)
		{
			item.Value.SetDuckingVolumeUpdater(1f, fadeTime, isLow: false);
			if (playDuckingTrigger.TryGetValue(item.Value.categoryName, out var value))
			{
				value.Clear();
			}
		}
	}

	public int play(string labelName, float delay = -1f, float pan = -2000f)
	{
		return playOption(labelName, AudioDefine.DEFAULT_VOLUME, AudioDefine.DEFAULT_FADE, pan, AudioDefine.DEFAULT_PITCH, delay);
	}

	private int prepareInstance(string labelName, float volume, float fadeTime, float pan, int pitch, float delay, ref AudioPlayer player, ref float time, ref bool queueOn, bool isForce2D)
	{
		_ = AudioDefine.INSTANCE_ID_ERROR;
		if (!sourceDict.TryGetValue(labelName, out player))
		{
			return AudioDefine.INSTANCE_ID_ERROR;
		}
		if (player == null)
		{
			return AudioDefine.INSTANCE_ID_ERROR;
		}
		if (!player.IsPlayInterval())
		{
			return AudioDefine.INSTANCE_ID_ERROR;
		}
		if (!player.IsSetPlayClip())
		{
			AudioLabelSettings labelSettings = player.GetLabelSettings();
			if (audioClipDict.TryGetValue(labelSettings.GetClipName(), out var _))
			{
				player.SetPlayClip(audioClipDict[labelSettings.GetClipName()]);
				updateRandomSourceInfo(labelName);
			}
		}
		if (!player.IsSetSpatialGroup())
		{
			string spatialGroup = player.GetSpatialGroup();
			if (spatialGroup != null && audio3DSettings.TryGetValue(spatialGroup, out var value2))
			{
				player.SetAudio3DSettings(value2);
			}
		}
		if (player.GetMaxPlaybacksNum() > 0)
		{
			switch (checkLabelPlaybacksNum(player))
			{
			case RESULT.EXECUTE:
			{
				int result = player.Prepare(volume, fadeTime, pan, pitch, isForce2D);
				_ = AudioDefine.INSTANCE_ID_ERROR;
				return result;
			}
			case RESULT.FINISH:
				return AudioDefine.INSTANCE_ID_ERROR;
			}
		}
		if (player.GetCategoryMaxPlaybacksNum() > 0)
		{
			RESULT rESULT = checkCategoryPlaybacksNum(player, ref time, ref queueOn);
			if (rESULT == RESULT.CONTINUE || rESULT == RESULT.FINISH)
			{
				return AudioDefine.INSTANCE_ID_ERROR;
			}
		}
		return player.Prepare(volume, fadeTime, pan, pitch, isForce2D);
	}

	public int playOption(string labelName, float volume, float fadeTime, float pan, int pitch, float delay)
	{
		int iNSTANCE_ID_ERROR = AudioDefine.INSTANCE_ID_ERROR;
		float time = 0f;
		bool queueOn = false;
		AudioPlayer player = null;
		iNSTANCE_ID_ERROR = prepareInstance(labelName, volume, fadeTime, pan, pitch, delay, ref player, ref time, ref queueOn, isForce2D: false);
		if (iNSTANCE_ID_ERROR != AudioDefine.INSTANCE_ID_ERROR)
		{
			stopSameSingleGroup(player.GetLabelSettings().singleGroup, labelName);
			addPlayInfo(player, iNSTANCE_ID_ERROR);
			startDucking(player, iNSTANCE_ID_ERROR);
		}
		playInstance(iNSTANCE_ID_ERROR, queueOn ? time : delay);
		return iNSTANCE_ID_ERROR;
	}

	public int prepare(string labelName, bool isForce2D = false)
	{
		return prepareOption(labelName, AudioDefine.DEFAULT_VOLUME, AudioDefine.DEFAULT_FADE, -2000f, AudioDefine.DEFAULT_PITCH, isForce2D);
	}

	public int prepareOption(string labelName, float volume, float fadeTime, float pan, int pitch, bool isForce2D)
	{
		int iNSTANCE_ID_ERROR = AudioDefine.INSTANCE_ID_ERROR;
		float time = 0f;
		bool queueOn = false;
		AudioPlayer player = null;
		iNSTANCE_ID_ERROR = prepareInstance(labelName, volume, fadeTime, pan, pitch, 0f, ref player, ref time, ref queueOn, isForce2D);
		if (iNSTANCE_ID_ERROR != AudioDefine.INSTANCE_ID_ERROR)
		{
			addPlayInfo(player, iNSTANCE_ID_ERROR);
		}
		return iNSTANCE_ID_ERROR;
	}

	public void playInstance(int instanceId, float delay = -1f)
	{
		if (playAudioDict.TryGetValue(instanceId, out var value) && value != null)
		{
			stopSameSingleGroup(value.GetLabelSettings().singleGroup, value.GetLabelSettings().name);
			startDucking(value, instanceId);
			value.PlayInstance(instanceId, delay);
		}
	}

	public int play3D(string labelName, GameObject target, float delay = -1f)
	{
		int iNSTANCE_ID_ERROR = AudioDefine.INSTANCE_ID_ERROR;
		iNSTANCE_ID_ERROR = prepare(labelName);
		if (iNSTANCE_ID_ERROR != AudioDefine.INSTANCE_ID_ERROR)
		{
			setTrackingObject(iNSTANCE_ID_ERROR, target);
			playInstance(iNSTANCE_ID_ERROR, delay);
		}
		return iNSTANCE_ID_ERROR;
	}

	public int play3D(string labelName, Vector3 position, float delay = -1f)
	{
		int iNSTANCE_ID_ERROR = AudioDefine.INSTANCE_ID_ERROR;
		iNSTANCE_ID_ERROR = prepare(labelName);
		if (iNSTANCE_ID_ERROR != AudioDefine.INSTANCE_ID_ERROR)
		{
			setPosition(iNSTANCE_ID_ERROR, position);
			playInstance(iNSTANCE_ID_ERROR, delay);
		}
		return iNSTANCE_ID_ERROR;
	}

	public int play3D(string labelName, Transform target, float delay = -1f)
	{
		int iNSTANCE_ID_ERROR = AudioDefine.INSTANCE_ID_ERROR;
		iNSTANCE_ID_ERROR = prepare(labelName);
		if (iNSTANCE_ID_ERROR != AudioDefine.INSTANCE_ID_ERROR)
		{
			setTrackingObject(iNSTANCE_ID_ERROR, target);
			playInstance(iNSTANCE_ID_ERROR, delay);
		}
		return iNSTANCE_ID_ERROR;
	}

	public int play2D(string labelName, float delay = -1f)
	{
		int iNSTANCE_ID_ERROR = AudioDefine.INSTANCE_ID_ERROR;
		iNSTANCE_ID_ERROR = prepare(labelName, isForce2D: true);
		if (iNSTANCE_ID_ERROR != AudioDefine.INSTANCE_ID_ERROR)
		{
			playInstance(iNSTANCE_ID_ERROR, delay);
		}
		return iNSTANCE_ID_ERROR;
	}

	public void setTrackingObject(int instanceId, GameObject target)
	{
		if (playAudioDict.TryGetValue(instanceId, out var value))
		{
			value?.SetTrackingObject(instanceId, target);
		}
	}

	public void setTrackingObject(int instanceId, Transform target)
	{
		if (playAudioDict.TryGetValue(instanceId, out var value))
		{
			value?.SetTrackingObject(instanceId, target);
		}
	}

	public void stop(int instanceId, float fadeTime = -1f)
	{
		if (playAudioDict.TryGetValue(instanceId, out var value))
		{
			value?.Stop(instanceId, fadeTime);
		}
	}

	public void stopLabel(string labelName, float fadeTime = -1f)
	{
		if (sourceDict.TryGetValue(labelName, out var value))
		{
			value?.StopAll(fadeTime);
		}
	}

	public void stopAll(float fadeTime = -1f)
	{
		foreach (KeyValuePair<int, AudioPlayer> item in playAudioDict)
		{
			if (item.Value != null)
			{
				item.Value.Stop(item.Key, fadeTime);
			}
		}
	}

	public void onPause(int instanceId, float fadeTime = -1f)
	{
		if (playAudioDict.TryGetValue(instanceId, out var value))
		{
			value?.OnPause(instanceId, fadeTime);
		}
	}

	public void onPauseAll(float fadeTime = -1f)
	{
		foreach (KeyValuePair<int, AudioPlayer> item in playAudioDict)
		{
			if (item.Value != null)
			{
				item.Value.OnPause(item.Key, fadeTime);
			}
		}
	}

	public void offPause(int instanceId, float fadeTime = -1f)
	{
		if (playAudioDict.TryGetValue(instanceId, out var value))
		{
			value?.OffPause(instanceId, fadeTime);
		}
	}

	public void offPauseAll(float fadeTime = -1f)
	{
		foreach (KeyValuePair<int, AudioPlayer> item in playAudioDict)
		{
			if (item.Value != null)
			{
				item.Value.OffPause(item.Key, fadeTime);
			}
		}
	}

	public void setVolume(int instanceId, float newVolume, float moveTime)
	{
		if (playAudioDict.TryGetValue(instanceId, out var value))
		{
			value?.SetVolume(instanceId, newVolume, moveTime);
		}
	}

	public void setVolume(string labelName, float newVolume, float moveTime)
	{
		if (sourceDict.TryGetValue(labelName, out var value))
		{
			value.SetVolumeAll(newVolume, moveTime);
		}
	}

	public void setPitch(int instanceId, int newPitch, float moveTime)
	{
		if (playAudioDict.TryGetValue(instanceId, out var value))
		{
			value?.SetPitch(instanceId, newPitch, moveTime);
		}
	}

	public void setPitch(string labelName, int newPitch, float moveTime)
	{
		if (sourceDict.TryGetValue(labelName, out var value))
		{
			value.SetPitchAll(newPitch, moveTime);
		}
	}

	public void setPan(int instanceId, float newPan, float moveTime)
	{
		if (playAudioDict.TryGetValue(instanceId, out var value))
		{
			value?.SetPan(instanceId, newPan, moveTime);
		}
	}

	public void setPan(string labelName, float newPan, float moveTime)
	{
		if (sourceDict.TryGetValue(labelName, out var value))
		{
			value.SetPanAll(newPan, moveTime);
		}
	}

	public void setPosition(int instanceId, Vector3 position)
	{
		if (playAudioDict.TryGetValue(instanceId, out var value))
		{
			value?.SetPosition(instanceId, position);
		}
	}

	public void setPosition(string labelName, Vector3 position)
	{
		if (sourceDict.TryGetValue(labelName, out var value))
		{
			value.SetPositionAll(position);
		}
	}

	public void resetPlayPosition(string labelName)
	{
		if (sourceDict.TryGetValue(labelName, out var value))
		{
			value.ResetPlayPosition();
		}
	}

	public void resetPlayPositionAll()
	{
		foreach (KeyValuePair<string, AudioPlayer> item in sourceDict)
		{
			if (item.Value != null)
			{
				item.Value.ResetPlayPosition();
			}
		}
	}

	public float getInstanceVolume(int instanceId)
	{
		if (playAudioDict.TryGetValue(instanceId, out var value))
		{
			return value.GetCurrentVolume(instanceId);
		}
		return 0f;
	}

	public float getInstanceCalcVolume(int instanceId)
	{
		if (playAudioDict.TryGetValue(instanceId, out var value))
		{
			return value.GetCalcVolume(instanceId);
		}
		return 0f;
	}

	public void setMasterVolume(string masterName, float volume, float moveTime = 0f)
	{
		if (masterDict.TryGetValue(masterName, out var value))
		{
			value.SetVolumeUpdater(value.GetCurrentVolume(), volume, moveTime);
			if (moveTime <= 0f)
			{
				value.UpdateVolume();
			}
		}
	}

	public float getMasterVolume(string masterName)
	{
		if (masterDict.TryGetValue(masterName, out var value))
		{
			return value.GetCurrentVolume();
		}
		return 1f;
	}

	public void setCategoryVolume(string categoryName, float volume, float moveTime = 0f)
	{
		if (categoryDict.TryGetValue(categoryName, out var value))
		{
			value.SetVolumeUpdater(value.GetCurrentVolume(), volume, moveTime);
			if (moveTime <= 0f)
			{
				value.UpdateVolume();
			}
		}
	}

	public float getCategoryVolume(string categoryName)
	{
		if (categoryDict.TryGetValue(categoryName, out var value))
		{
			return value.GetCurrentVolume();
		}
		return 1f;
	}

	public float getLabelVolume(string labelName)
	{
		if (!sourceDict.TryGetValue(labelName, out var value))
		{
			return 0f;
		}
		return value.GetLabelSettings().volume;
	}

	public void stopMaster(string masterName, float fadeTime = -1f)
	{
		foreach (KeyValuePair<string, AudioCategorySettings> item in categoryDict)
		{
			string masterName2 = item.Value.masterName;
			if (masterName2 != null && masterName.CompareTo(masterName2) == 0)
			{
				stopCategory(item.Key, fadeTime);
			}
		}
	}

	public void onPauseMaster(string masterName, float fadeTime = -1f)
	{
		foreach (KeyValuePair<string, AudioCategorySettings> item in categoryDict)
		{
			string masterName2 = item.Value.masterName;
			if (masterName2 != null && masterName.CompareTo(masterName2) == 0)
			{
				onPauseCategory(item.Key, fadeTime);
			}
		}
	}

	public void offPauseMaster(string masterName, float fadeTime = -1f)
	{
		foreach (KeyValuePair<string, AudioCategorySettings> item in categoryDict)
		{
			string masterName2 = item.Value.masterName;
			if (masterName2 != null && masterName.CompareTo(masterName2) == 0)
			{
				offPauseCategory(item.Key, fadeTime);
			}
		}
	}

	public void stopCategory(string categoryName, float fadeTime = -1f)
	{
		if (playCategoryDict.TryGetValue(categoryName, out var value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				stop(value[i], fadeTime);
			}
		}
	}

	public void onPauseLabel(string labelName, float fadeTime = -1f)
	{
		if (sourceDict.TryGetValue(labelName, out var value))
		{
			value?.OnPauseAll(fadeTime);
		}
	}

	public void offPauseLabel(string labelName, float fadeTime = -1f)
	{
		if (sourceDict.TryGetValue(labelName, out var value))
		{
			value?.OffPauseAll(fadeTime);
		}
	}

	public void onPauseCategory(string categoryName, float fadeTime = -1f)
	{
		if (playCategoryDict.TryGetValue(categoryName, out var value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				onPause(value[i], fadeTime);
			}
		}
	}

	public void offPauseCategory(string categoryName, float fadeTime = -1f)
	{
		if (playCategoryDict.TryGetValue(categoryName, out var value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				offPause(value[i], fadeTime);
			}
		}
	}

	public void offPauseCategory(string categoryName, List<int> instanceList, float fadeTime = -1f)
	{
		bool flag = false;
		if (!playCategoryDict.TryGetValue(categoryName, out var value))
		{
			return;
		}
		for (int i = 0; i < value.Count; i++)
		{
			for (int j = 0; j < instanceList.Count; j++)
			{
				if (instanceList[j] == value[i])
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				offPause(value[i], fadeTime);
			}
		}
	}

	public AudioDefine.INSTANCE_STATUS getInstanceStatus(int instanceId)
	{
		if (playAudioDict.TryGetValue(instanceId, out var value))
		{
			return value.GetInstanceStatus(instanceId);
		}
		return AudioDefine.INSTANCE_STATUS.STOP;
	}

	public bool isPlayingLabel(string labelName)
	{
		if (sourceDict.TryGetValue(labelName, out var value) && value.GetPlayingNum() > 0)
		{
			return true;
		}
		return false;
	}

	public int getLabelNum()
	{
		return sourceDict.Count;
	}

	public string[] getLabelNameList()
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, AudioPlayer> item in sourceDict)
		{
			list.Add(item.Key);
		}
		return list.ToArray();
	}

	public int getCategoryNum()
	{
		return categoryDict.Count;
	}

	public string[] getCategoryNameList()
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, AudioCategorySettings> item in categoryDict)
		{
			list.Add(item.Key);
		}
		return list.ToArray();
	}

	public int getMasterNum()
	{
		return masterDict.Count;
	}

	public string[] getMasterNameList()
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, AudioMasterSettings> item in masterDict)
		{
			list.Add(item.Key);
		}
		return list.ToArray();
	}

	public int getAudio3DSettingsNum()
	{
		return audio3DSettings.Count;
	}

	public string[] getAudio3DSettingsNameList()
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, Audio3DSettings> audio3DSetting in audio3DSettings)
		{
			list.Add(audio3DSetting.Key);
		}
		return list.ToArray();
	}

	public void updateUnityMixerName(string labelName, string newMixerName)
	{
		if (sourceDict.TryGetValue(labelName, out var value) && mixerSettings != null)
		{
			AudioMixerGroup[] array = mixerSettings.FindGroup(newMixerName);
			if (array != null && array.Length != 0)
			{
				value.SetAudioMixerGroup(array[0]);
			}
		}
	}

	public void updateSpatialGroupName(string labelName, string newGroupName)
	{
		if (sourceDict.TryGetValue(labelName, out var value) && base.name != null && audio3DSettings.TryGetValue(newGroupName, out var value2))
		{
			value.SetAudio3DSettings(value2);
		}
	}

	public string getCategoryNameSettingOfLabel(string labelName)
	{
		if (sourceDict.TryGetValue(labelName, out var value))
		{
			return value.GetCategoryName();
		}
		return null;
	}

	public string getMasterNameSettingOfCategory(string categoryName)
	{
		if (categoryDict.TryGetValue(categoryName, out var value))
		{
			return value.masterName;
		}
		return null;
	}

	public float getPlayTime(int instanceId)
	{
		if (playAudioDict.TryGetValue(instanceId, out var value) && value != null)
		{
			return value.GetPlayTime(instanceId);
		}
		return -1f;
	}

	public int getPlaySamples(int instanceId)
	{
		if (playAudioDict.TryGetValue(instanceId, out var value) && value != null)
		{
			return value.GetPlaySamples(instanceId);
		}
		return -1;
	}

	public void setTime(int instanceId, float time)
	{
		if (playAudioDict.TryGetValue(instanceId, out var value))
		{
			value?.SetTime(instanceId, time);
		}
	}

	public void setTimeSamples(int instanceId, int samples)
	{
		if (playAudioDict.TryGetValue(instanceId, out var value))
		{
			value?.SetTimeSamples(instanceId, samples);
		}
	}

	public void setMute(bool onMute)
	{
		IsOnMute = onMute;
		foreach (KeyValuePair<string, AudioMasterSettings> item in masterDict)
		{
			item.Value.SetMute(onMute);
		}
	}

	public bool getMuteStatus()
	{
		return IsOnMute;
	}

	public string[] getAudioClipNameLoadId(int loadId)
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, AudioPlayer> item in sourceDict)
		{
			AudioLabelSettings labelSettings = item.Value.GetLabelSettings();
			if (labelSettings.loadId == loadId)
			{
				list.Add(labelSettings.GetClipName());
			}
		}
		return list.ToArray();
	}

	public string[] getAudioClipNameAll()
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, AudioPlayer> item in sourceDict)
		{
			AudioLabelSettings labelSettings = item.Value.GetLabelSettings();
			list.Add(labelSettings.GetClipName());
		}
		return list.ToArray();
	}

	public string getAudioClipName(string labelName)
	{
		if (sourceDict.TryGetValue(labelName, out var value))
		{
			return value.GetLabelSettings().GetClipName();
		}
		return null;
	}

	public string[] getAudioClipNames(string labelName)
	{
		if (sourceDict.TryGetValue(labelName, out var value))
		{
			AudioLabelSettings labelSettings = value.GetLabelSettings();
			List<string> list = new List<string>();
			list.Add(labelSettings.GetClipName());
			if (labelSettings.isRandomPlay && labelSettings.randomSource != null)
			{
				for (int i = 0; i < labelSettings.randomSource.Length; i++)
				{
					string audioClipName = getAudioClipName(labelSettings.randomSource[i]);
					if (audioClipName != null)
					{
						list.Add(audioClipName);
					}
				}
			}
			return list.ToArray();
		}
		return null;
	}

	public string[] getRandomSourceNames(string labelName)
	{
		if (sourceDict.TryGetValue(labelName, out var value))
		{
			AudioLabelSettings labelSettings = value.GetLabelSettings();
			List<string> list = new List<string>();
			list.Add(labelSettings.name);
			if (labelSettings.isRandomPlay && labelSettings.randomSource != null)
			{
				for (int i = 0; i < labelSettings.randomSource.Length; i++)
				{
					if (!list.Contains(labelSettings.randomSource[i]))
					{
						list.Add(labelSettings.randomSource[i]);
					}
				}
			}
			return list.ToArray();
		}
		return null;
	}

	public void setAudioClipToLabelLoadId(int loadId)
	{
		foreach (KeyValuePair<string, AudioPlayer> item in sourceDict)
		{
			AudioPlayer value = item.Value;
			AudioLabelSettings labelSettings = value.GetLabelSettings();
			if (labelSettings.loadId == loadId && audioClipDict.TryGetValue(labelSettings.GetClipName(), out var value2))
			{
				value.SetPlayClip(value2);
			}
		}
		updateRandomSourceInfoAll();
	}

	public void setAudioClipToLabelAll()
	{
		foreach (KeyValuePair<string, AudioPlayer> item in sourceDict)
		{
			AudioPlayer value = item.Value;
			AudioLabelSettings labelSettings = value.GetLabelSettings();
			if (audioClipDict.TryGetValue(labelSettings.GetClipName(), out var value2))
			{
				value.SetPlayClip(value2);
			}
		}
		updateRandomSourceInfoAll();
	}

	public void setAudioClipToLabel(string labelName)
	{
		if (sourceDict.TryGetValue(labelName, out var value))
		{
			AudioLabelSettings labelSettings = value.GetLabelSettings();
			if (audioClipDict.TryGetValue(labelSettings.GetClipName(), out var _))
			{
				value.SetPlayClip(audioClipDict[labelSettings.GetClipName()]);
				updateRandomSourceInfo(labelName);
			}
		}
	}

	public void setAndroidNativeToLabel(string labelName, string filePath, string className, string funcName)
	{
		if (sourceDict.TryGetValue(labelName, out var value))
		{
			value.GetLabelSettings().SetAndroidSoundId(USndAndroidNativePlayer.LoadData(filePath, className, funcName));
		}
	}

	public void clearObjectPool()
	{
		if (AudioMainPool.instance != null)
		{
			AudioMainPool.instance.Clear();
		}
	}

	public float getLabelLength(string labelName)
	{
		if (!sourceDict.TryGetValue(labelName, out var value))
		{
			return 0f;
		}
		return value.GetClipLength();
	}

	public int getLabelSamples(string labelName)
	{
		if (!sourceDict.TryGetValue(labelName, out var value))
		{
			return 0;
		}
		return value.GetClipSamples();
	}

	public bool getSpectrumData(int instanceId, float[] sample, int channel, FFTWindow window)
	{
		if (playAudioDict.TryGetValue(instanceId, out var value) && value != null)
		{
			return value.GetSpectrumData(instanceId, sample, channel, window);
		}
		return false;
	}

	public bool isLoop(string labelName)
	{
		if (sourceDict.TryGetValue(labelName, out var value))
		{
			return value.GetLabelSettings().GetLoop();
		}
		return false;
	}

	public int getLabelMaxPlaybacksNum(string labelName)
	{
		if (sourceDict.TryGetValue(labelName, out var value))
		{
			return value.GetLabelSettings().maxPlaybacksNum;
		}
		return -1;
	}

	public int getCategoryMaxPlaybacksNum(string categoryName)
	{
		if (categoryDict.TryGetValue(categoryName, out var value))
		{
			return value.maxPlaybacksNum;
		}
		return -1;
	}

	public int getCategoryMaxPlaybacksNumFromLabel(string labelName)
	{
		if (sourceDict.TryGetValue(labelName, out var value))
		{
			AudioLabelSettings labelSettings = value.GetLabelSettings();
			if (categoryDict.TryGetValue(labelSettings.categoryName, out var value2))
			{
				return value2.maxPlaybacksNum;
			}
		}
		return -1;
	}

	public bool isInterval(string labelName)
	{
		if (sourceDict.TryGetValue(labelName, out var value))
		{
			if (!value.IsPlayInterval())
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public int getCurrentPlayNum()
	{
		return playAudioDict.Count;
	}

	private void Update()
	{
		foreach (KeyValuePair<string, List<string>> item in playDuckingTrigger)
		{
			if (item.Value.Count == 0 || !categoryDict.TryGetValue(item.Key, out var value))
			{
				continue;
			}
			int num = 0;
			while (num < item.Value.Count)
			{
				if (sourceDict.TryGetValue(item.Value[num], out var value2))
				{
					AudioLabelSettings labelSettings = value2.GetLabelSettings();
					if (value2.GetPlayingNum() == 0)
					{
						if (item.Value.Count - 1 == 0 && labelSettings.autoRestoreDucking)
						{
							value.SetDuckingVolumeUpdater(1f, labelSettings.duckingEndTime, isLow: false);
						}
						item.Value.RemoveAt(num);
						continue;
					}
					if (labelSettings.autoRestoreDucking && labelSettings.restoreTime >= 0f && value2.GetPlayTime() > labelSettings.restoreTime)
					{
						if (item.Value.Count - 1 == 0)
						{
							value.SetDuckingVolumeUpdater(1f, labelSettings.duckingEndTime, isLow: false);
							item.Value.RemoveAt(num);
							continue;
						}
						item.Value.RemoveAt(num);
					}
					num++;
				}
				else
				{
					num++;
				}
			}
		}
		foreach (KeyValuePair<string, AudioMasterSettings> item2 in masterDict)
		{
			item2.Value.UpdateVolume();
		}
		foreach (KeyValuePair<string, AudioCategorySettings> item3 in categoryDict)
		{
			AudioCategorySettings value3 = item3.Value;
			value3.UpdateDuckingVolume();
			value3.UpdateVolume();
		}
		foreach (KeyValuePair<string, List<int>> item4 in playCategoryDict)
		{
			List<int> value4 = item4.Value;
			orderCategoryInstanceList(value4);
		}
		if (playAudioDict.Count == 0)
		{
			return;
		}
		playAudioRemoveKey.Clear();
		foreach (AudioPlayer item5 in playerHashSet)
		{
			if (item5 != null)
			{
				item5.Update();
				AudioCategorySettings audioCategorySettings = categoryDict[item5.GetCategoryName()];
				if (audioCategorySettings != null)
				{
					item5?.UpdateVolumeFactor(audioCategorySettings.GetVolumeFactor());
				}
			}
		}
		foreach (KeyValuePair<int, AudioPlayer> item6 in playAudioDict)
		{
			AudioPlayer value5 = item6.Value;
			if (value5 != null && value5.GetInstanceStatus(item6.Key) == AudioDefine.INSTANCE_STATUS.STOP)
			{
				playAudioRemoveKey.Add(item6.Key);
				if (item6.Value.GetPlayingTrueNum() == 0)
				{
					playerHashSet.Remove(item6.Value);
				}
			}
		}
		for (int i = 0; i < playAudioRemoveKey.Count; i++)
		{
			playAudioDict.Remove(playAudioRemoveKey[i]);
		}
	}

	private void resetDuckingBeforeUpdate(AudioPlayer player)
	{
		AudioLabelSettings labelSettings = player.GetLabelSettings();
		string value = labelSettings.name;
		if (labelSettings.duckingCategories == null || !labelSettings.autoRestoreDucking)
		{
			return;
		}
		for (int i = 0; i < labelSettings.duckingCategories.Length; i++)
		{
			string key = labelSettings.duckingCategories[i];
			if (!playDuckingTrigger.TryGetValue(key, out var value2))
			{
				continue;
			}
			for (int j = 0; j < value2.Count; j++)
			{
				if (value2[j].Equals(value))
				{
					if (value2.Count - 1 == 0 && labelSettings.autoRestoreDucking)
					{
						categoryDict[key]?.SetDuckingVolumeUpdater(1f, labelSettings.duckingEndTime, isLow: false);
					}
					value2.RemoveAt(j);
					break;
				}
			}
		}
	}
}
