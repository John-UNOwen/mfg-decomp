using UnityEngine;

namespace TestModeSkeleton;

public class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
{
	private static T instance;

	public static T Instance => init();

	private static T init()
	{
		if (instance == null)
		{
			T val = Object.FindObjectOfType(typeof(T)) as T;
			if (val != null)
			{
				val.initialize();
			}
			instance = val;
			_ = instance == null;
		}
		return instance;
	}

	protected virtual void initialize()
	{
	}

	private void Awake()
	{
		init();
	}

	protected virtual void OnDestroy()
	{
		if (instance == this)
		{
			instance = null;
		}
	}
}
