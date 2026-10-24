using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace KAMUNITY;

public class KamunityInstance : MonoBehaviour
{
	private static KamunityInstance s_Instance;

	private IntPtr m_hDll;

	private static bool s_isActive;

	public static IntPtr hDLL
	{
		get
		{
			if (s_Instance != null)
			{
				return s_Instance.m_hDll;
			}
			return IntPtr.Zero;
		}
	}

	public static bool isLocalBoot { get; private set; }

	public static Action shutdownCallback { get; set; }

	public static bool isActive
	{
		get
		{
			if (s_Instance == null)
			{
				return false;
			}
			return s_isActive;
		}
	}

	public static bool isEa3Boot
	{
		get
		{
			if (!isActive)
			{
				return false;
			}
			return true;
		}
	}

	public static T GetProcDelegate<T>(string methodName) where T : class
	{
		return MarshalUtil.GetProcDelegate<T>(hDLL, methodName);
	}

	public static Delegate GetProcDelegate(string methodName, Type type)
	{
		return MarshalUtil.GetProcDelegate(hDLL, methodName, type);
	}

	public static GameObject Startup()
	{
		if (s_Instance != null)
		{
			Logger.LogWarning("already startup KamunityInstance");
			return null;
		}
		GameObject obj = new GameObject("KamunityInstance", typeof(KamunityInstance));
		UnityEngine.Object.DontDestroyOnLoad(obj);
		obj.GetComponent<KamunityInstance>().Boot();
		return obj;
	}

	private void OnApplicationQuit()
	{
		Shutdown();
	}

	private void Boot()
	{
		s_Instance = this;
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		m_hDll = IntPtr.Zero;
		int num = Array.IndexOf(commandLineArgs, "-hp");
		if (num >= 0 && num + 1 < commandLineArgs.Length)
		{
			long value = Convert.ToInt64(commandLineArgs[num + 1], 16);
			m_hDll = new IntPtr(value);
		}
		else if (m_hDll == IntPtr.Zero)
		{
			m_hDll = NativeMethods.LoadLibraryA("kamunity.dll");
		}
		if (m_hDll != IntPtr.Zero)
		{
			foreach (Type item in GetKamunityApiClass())
			{
				SetDelegateProperty(item);
			}
			foreach (PropertyInfo item2 in GetKamunityApiProperty())
			{
				SetDelegateProperty(item2);
			}
		}
		s_isActive = m_hDll != IntPtr.Zero;
	}

	private void Shutdown()
	{
		s_isActive = false;
		if (s_Instance == null)
		{
			return;
		}
		if (Utility.IsSafeDelegate(shutdownCallback))
		{
			shutdownCallback();
			shutdownCallback = null;
		}
		foreach (Type item in GetKamunityApiClass())
		{
			ClearDelegateProperty(item);
		}
		foreach (PropertyInfo item2 in GetKamunityApiProperty())
		{
			ClearDelegateProperty(item2);
		}
		s_Instance = null;
	}

	private static List<Type> GetKamunityApiClass()
	{
		List<Type> list = new List<Type>();
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			if (assembly.FullName.StartsWith("System.") || assembly.FullName.StartsWith("System") || assembly.FullName.StartsWith("Unity") || assembly.FullName.StartsWith("Microsoft."))
			{
				continue;
			}
			Type[] types = assembly.GetTypes();
			foreach (Type type in types)
			{
				if (type.IsClass && type.GetCustomAttributes(typeof(KamunityDllApiClassAttribute), inherit: true).Length != 0)
				{
					list.Add(type);
				}
			}
		}
		return list;
	}

	private static List<PropertyInfo> GetKamunityApiProperty()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			if (assembly.FullName.StartsWith("System.") || assembly.FullName.StartsWith("System") || assembly.FullName.StartsWith("Unity") || assembly.FullName.StartsWith("Microsoft."))
			{
				continue;
			}
			Type[] types = assembly.GetTypes();
			foreach (Type type in types)
			{
				if (!type.IsClass)
				{
					continue;
				}
				PropertyInfo[] properties = type.GetProperties();
				foreach (PropertyInfo propertyInfo in properties)
				{
					if ((propertyInfo.PropertyType.IsSubclassOf(typeof(Delegate)) || propertyInfo.PropertyType.Equals(typeof(Delegate))) && propertyInfo.GetCustomAttributes(typeof(KamunityDllApiAttribute), inherit: true).Length != 0)
					{
						list.Add(propertyInfo);
					}
				}
			}
		}
		return list;
	}

	private static void SetDelegateProperty(Type type)
	{
		PropertyInfo[] properties = type.GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (propertyInfo.PropertyType.IsSubclassOf(typeof(Delegate)) || propertyInfo.PropertyType.Equals(typeof(Delegate)))
			{
				Delegate procDelegate = GetProcDelegate(propertyInfo.Name, propertyInfo.PropertyType);
				propertyInfo.SetValue(null, procDelegate);
			}
		}
	}

	private static void SetDelegateProperty(PropertyInfo prop)
	{
		object[] customAttributes = prop.GetCustomAttributes(typeof(KamunityDllApiAttribute), inherit: true);
		if (customAttributes.Length != 0)
		{
			KamunityDllApiAttribute kamunityDllApiAttribute = customAttributes[0] as KamunityDllApiAttribute;
			Delegate procDelegate = GetProcDelegate(string.IsNullOrEmpty(kamunityDllApiAttribute.functionName) ? prop.Name : kamunityDllApiAttribute.functionName, prop.PropertyType);
			prop.SetValue(null, procDelegate);
		}
	}

	private static void ClearDelegateProperty(Type type)
	{
		PropertyInfo[] properties = type.GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			if ((propertyInfo.PropertyType.IsSubclassOf(typeof(Delegate)) || propertyInfo.PropertyType.Equals(typeof(Delegate))) && propertyInfo.GetGetMethod().IsStatic)
			{
				propertyInfo.SetValue(null, null);
			}
		}
	}

	private static void ClearDelegateProperty(PropertyInfo prop)
	{
		if ((prop.PropertyType.IsSubclassOf(typeof(Delegate)) || prop.PropertyType.Equals(typeof(Delegate))) && prop.GetGetMethod().IsStatic)
		{
			prop.SetValue(null, null);
		}
	}
}
