using UnityEngine;

public class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
{
	private static T instance;

	private bool bInitialized;

	public static T Instance => init();

	private static T init()
	{
		if (instance == null)
		{
			T val = Object.FindObjectOfType(typeof(T)) as T;
			if (val == null)
			{
				val = new GameObject(typeof(T).FullName).AddComponent<T>();
			}
			if (val != null && !val.bInitialized)
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
		bInitialized = true;
	}

	protected virtual void Awake()
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
