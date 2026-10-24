using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace TestModeSkeleton.TestModeExtensions;

public static class TestModeGameObjectExtensions
{
	public static GameObject FindChild(this GameObject o, string name)
	{
		Transform transform = o.transform.Find(name);
		if (transform == null)
		{
			return null;
		}
		return transform.gameObject;
	}

	public static List<GameObject> FindChildGameObjects<ComponentType>(this GameObject gameObject, Predicate<string> predicate, bool isRecursive) where ComponentType : Component
	{
		List<GameObject> list = new List<GameObject>();
		foreach (Transform item in gameObject.transform)
		{
			ComponentType component = item.GetComponent<ComponentType>();
			if (component != null && predicate(component.name))
			{
				list.Add(item.gameObject);
			}
			if (isRecursive)
			{
				list.AddRange(item.gameObject.FindChildGameObjects<ComponentType>(predicate, isRecursive: true));
			}
		}
		return list;
	}

	public static List<ComponentType> FindChildComponents<ComponentType>(this GameObject gameObject, Predicate<string> predicate, bool isRecursive) where ComponentType : Component
	{
		List<ComponentType> list = new List<ComponentType>();
		foreach (Transform item in gameObject.transform)
		{
			ComponentType component = item.GetComponent<ComponentType>();
			if (component != null && predicate(component.name))
			{
				list.Add(component);
			}
			if (isRecursive)
			{
				list.AddRange(item.gameObject.FindChildComponents<ComponentType>(predicate, isRecursive: true));
			}
		}
		return list;
	}

	public static List<GameObject> FindChildGameObjectsRecursively<ComponentType>(this GameObject gameObject, Regex regex) where ComponentType : Component
	{
		return gameObject.FindChildGameObjects<ComponentType>((string s) => regex.IsMatch(s), isRecursive: true);
	}

	public static List<ComponentType> FindChildComponentsRecursively<ComponentType>(this GameObject gameObject, string name) where ComponentType : Component
	{
		return gameObject.FindChildComponents<ComponentType>((string s) => s == name, isRecursive: true);
	}

	public static List<ComponentType> FindChildComponentsRecursively<ComponentType>(this GameObject gameObject) where ComponentType : Component
	{
		return gameObject.FindChildComponents<ComponentType>((string s) => true, isRecursive: true);
	}

	public static List<ComponentType> FindChildComponents<ComponentType>(this GameObject gameObject, Regex regex) where ComponentType : Component
	{
		return gameObject.FindChildComponents<ComponentType>((string s) => regex.IsMatch(s), isRecursive: false);
	}
}
