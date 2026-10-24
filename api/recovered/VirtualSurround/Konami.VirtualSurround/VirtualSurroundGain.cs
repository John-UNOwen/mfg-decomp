using UnityEngine.Audio;

namespace Konami.VirtualSurround;

public class VirtualSurroundGain
{
	public AudioMixer mixer;

	public static readonly string[] exposedParams = new string[8] { "Gain1", "Gain2", "Gain3", "Gain4", "Gain5", "Gain6", "Gain7", "Gain8" };

	public float ch1
	{
		get
		{
			return GetGain(0);
		}
		set
		{
			SetGain(0, value);
		}
	}

	public float ch2
	{
		get
		{
			return GetGain(1);
		}
		set
		{
			SetGain(1, value);
		}
	}

	public float ch3
	{
		get
		{
			return GetGain(2);
		}
		set
		{
			SetGain(2, value);
		}
	}

	public float ch4
	{
		get
		{
			return GetGain(3);
		}
		set
		{
			SetGain(3, value);
		}
	}

	public float ch5
	{
		get
		{
			return GetGain(4);
		}
		set
		{
			SetGain(4, value);
		}
	}

	public float ch6
	{
		get
		{
			return GetGain(5);
		}
		set
		{
			SetGain(5, value);
		}
	}

	public float ch7
	{
		get
		{
			return GetGain(6);
		}
		set
		{
			SetGain(6, value);
		}
	}

	public float ch8
	{
		get
		{
			return GetGain(7);
		}
		set
		{
			SetGain(7, value);
		}
	}

	public float speakerL
	{
		get
		{
			return ch3;
		}
		set
		{
			ch3 = value;
		}
	}

	public float speakerR
	{
		get
		{
			return ch4;
		}
		set
		{
			ch4 = value;
		}
	}

	public float speakerC
	{
		get
		{
			return ch5;
		}
		set
		{
			ch5 = value;
		}
	}

	public float speakerLFE
	{
		get
		{
			return ch6;
		}
		set
		{
			ch6 = value;
		}
	}

	public float speakerSideL
	{
		get
		{
			return ch1;
		}
		set
		{
			ch1 = value;
		}
	}

	public float speakerSideR
	{
		get
		{
			return ch2;
		}
		set
		{
			ch2 = value;
		}
	}

	public float speakerRearL
	{
		get
		{
			return ch7;
		}
		set
		{
			ch7 = value;
		}
	}

	public float speakerRearR
	{
		get
		{
			return ch8;
		}
		set
		{
			ch8 = value;
		}
	}

	public void SetGain(int channelIndex, float gainDecibel)
	{
		if ((bool)mixer)
		{
			mixer.SetFloat(exposedParams[channelIndex], gainDecibel);
		}
	}

	public float GetGain(int channelIndex)
	{
		float value = 0f;
		if ((bool)mixer)
		{
			mixer.GetFloat(exposedParams[channelIndex], out value);
		}
		return value;
	}

	public void SetFrontGain(float gainDecibel)
	{
		speakerL = gainDecibel;
		speakerR = gainDecibel;
	}

	public void SetSurroundGain(float gainDecibel)
	{
		speakerSideL = gainDecibel;
		speakerSideR = gainDecibel;
	}

	public void SetWooferGain(float gainDecibel)
	{
	}
}
