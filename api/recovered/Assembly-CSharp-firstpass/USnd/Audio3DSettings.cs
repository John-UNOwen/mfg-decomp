using System;
using UnityEngine;

namespace USnd;

[CreateAssetMenu(menuName = "USnd/ Create Audio3DSettings Instance")]
public class Audio3DSettings : ScriptableObject, ICloneable
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

	public AnimationCurve customRolloffCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

	public AnimationCurve spatialBlendCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public AnimationCurve reverbZoneMixCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

	public AnimationCurve spreadCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public object Clone()
	{
		Audio3DSettings audio3DSettings = ScriptableObject.CreateInstance<Audio3DSettings>();
		audio3DSettings.spatialName = (string)spatialName.Clone();
		audio3DSettings.spatialBlend = spatialBlend;
		audio3DSettings.reverbZoneMix = reverbZoneMix;
		audio3DSettings.dopplerLevel = dopplerLevel;
		audio3DSettings.spread = spread;
		audio3DSettings.rolloffMode = rolloffMode;
		audio3DSettings.minDistance = minDistance;
		audio3DSettings.maxDistance = maxDistance;
		audio3DSettings.customRolloffCurve = new AnimationCurve(customRolloffCurve.keys);
		audio3DSettings.spatialBlendCurve = new AnimationCurve(spatialBlendCurve.keys);
		audio3DSettings.reverbZoneMixCurve = new AnimationCurve(reverbZoneMixCurve.keys);
		audio3DSettings.spreadCurve = new AnimationCurve(spreadCurve.keys);
		return audio3DSettings;
	}

	public void Copy(Audio3DSettings newParam)
	{
		spatialName = (string)newParam.spatialName.Clone();
		spatialBlend = newParam.spatialBlend;
		reverbZoneMix = newParam.reverbZoneMix;
		dopplerLevel = newParam.dopplerLevel;
		spread = newParam.spread;
		rolloffMode = newParam.rolloffMode;
		minDistance = newParam.minDistance;
		maxDistance = newParam.maxDistance;
		customRolloffCurve = new AnimationCurve(newParam.customRolloffCurve.keys);
		spatialBlendCurve = new AnimationCurve(newParam.spatialBlendCurve.keys);
		reverbZoneMixCurve = new AnimationCurve(newParam.reverbZoneMixCurve.keys);
		spreadCurve = new AnimationCurve(newParam.spreadCurve.keys);
	}
}
