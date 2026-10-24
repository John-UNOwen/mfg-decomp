using System.Collections;
using System.Collections.Generic;
using KAMUNITY;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace TestModeSkeleton;

public class SceneFlowBase : MonoBehaviour
{
	public class UserData
	{
		public object sceneArg;

		public object sceneReturn;
	}

	public enum CoSceneOption
	{
		LoadRequest,
		LoadEnd,
		SceneEnd
	}

	private class SceneLoadedCallback
	{
		private UnityAction<Scene, LoadSceneMode> m_Func;

		private string m_Name;

		public SceneLoadedCallback(string name, UnityAction<Scene, LoadSceneMode> func)
		{
			m_Name = name;
			m_Func = func;
			SceneManager.sceneLoaded += Callback;
		}

		public void Callback(Scene scene, LoadSceneMode mode)
		{
			if (scene.name.CompareTo(m_Name) == 0)
			{
				m_Func(scene, mode);
				SceneManager.sceneLoaded -= Callback;
			}
		}
	}

	private class SceneUnloadedCallback
	{
		private UnityAction<Scene> m_Func;

		private string m_Name;

		public SceneUnloadedCallback(string name, UnityAction<Scene> func)
		{
			m_Name = name;
			m_Func = func;
			SceneManager.sceneUnloaded += Callback;
		}

		public void Callback(Scene scene)
		{
			if (scene.name.CompareTo(m_Name) == 0)
			{
				m_Func(scene);
				SceneManager.sceneUnloaded -= Callback;
			}
		}
	}

	protected static readonly List<string> DUPLICATION_PERMIT_SCENE = new List<string>();

	private UserData m_userData = new UserData();

	private float timestampCreate;

	public UserData userData => m_userData;

	public float timestampSinceCreate => timestampCreate;

	protected void Awake()
	{
		timestampCreate = Time.realtimeSinceStartup;
	}

	protected virtual void Update()
	{
	}

	protected void OnDestroy()
	{
	}

	public virtual void Exit()
	{
		StartCoroutine(CoExit());
	}

	public virtual IEnumerator CoExit()
	{
		KAMUNITY.Logger.Log("exit scene : " + base.gameObject.scene.name);
		yield return vOnBeforeUnload();
		if (base.gameObject.scene.name != "TestModeMain")
		{
			yield return SceneManager.UnloadSceneAsync(base.gameObject.scene);
		}
	}

	protected virtual IEnumerator vOnBeforeUnload()
	{
		yield break;
	}

	public static IEnumerator co_scene(string scene_name, CoSceneOption option = CoSceneOption.SceneEnd, UnityAction<Scene, LoadSceneMode> funcSceneLoaded = null, UnityAction<Scene> funcSceneUnloaded = null, object sceneArg = null)
	{
		Scene sceneByName = SceneManager.GetSceneByName(scene_name);
		if (sceneByName.IsValid() && !DUPLICATION_PERMIT_SCENE.Contains(sceneByName.name))
		{
			KAMUNITY.Logger.LogWarning("already load scene : " + scene_name);
			yield break;
		}
		if (funcSceneLoaded != null)
		{
			new SceneLoadedCallback(scene_name, funcSceneLoaded);
		}
		if (funcSceneUnloaded != null)
		{
			new SceneUnloadedCallback(scene_name, funcSceneUnloaded);
		}
		if (sceneArg != null)
		{
			new SceneLoadedCallback(scene_name, delegate(Scene _scene, LoadSceneMode _funcSceneLoaded)
			{
				GameObject[] rootGameObjects = _scene.GetRootGameObjects();
				for (int i = 0; i < rootGameObjects.Length; i++)
				{
					SceneFlowBase componentInChildren = rootGameObjects[i].GetComponentInChildren<SceneFlowBase>();
					if ((bool)componentInChildren)
					{
						componentInChildren.userData.sceneArg = sceneArg;
						break;
					}
				}
			});
		}
		KAMUNITY.Logger.Log("load scene : " + scene_name);
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(scene_name, LoadSceneMode.Additive);
		if (asyncOperation == null)
		{
			KAMUNITY.Logger.LogError("【デバッグ用メッセージ】\nシーン[" + scene_name + "]がロードできませんでした\nEditorの場合はBuildSettingを確認してください\n端末の場合はAssetBundleが古いかリソース不足の可能性があります\nOKを押すと終了します");
			Application.Quit();
			while (true)
			{
				yield return null;
			}
		}
		if (option == CoSceneOption.LoadRequest)
		{
			yield break;
		}
		yield return asyncOperation;
		KAMUNITY.Logger.Log("loaded scene : " + scene_name);
		if (option != CoSceneOption.LoadEnd)
		{
			yield return new WaitWhile(() => SceneManager.GetSceneByName(scene_name).isLoaded);
			KAMUNITY.Logger.Log("unloaded scene : " + scene_name);
		}
	}

	public static IEnumerator CoUnloadScenesAsc(List<string> whiteList)
	{
		List<string> unloadList = new List<string>();
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			Scene scene = SceneManager.GetSceneAt(i);
			if (whiteList.FindIndex((string name) => string.Compare(name, scene.name) == 0) < 0)
			{
				unloadList.Add(scene.name);
			}
		}
		for (int loop = 0; loop < unloadList.Count; loop++)
		{
			KAMUNITY.Logger.LogFormat("Unload scene : {0}", unloadList[loop]);
			yield return SceneManager.UnloadSceneAsync(unloadList[loop]);
		}
	}

	public static IEnumerator CoUnloadScenesDesc(List<string> whiteList)
	{
		List<string> unloadList = new List<string>();
		for (int num = SceneManager.sceneCount - 1; num >= 0; num--)
		{
			Scene scene = SceneManager.GetSceneAt(num);
			if (whiteList.FindIndex((string name) => string.Compare(name, scene.name) == 0) < 0)
			{
				unloadList.Add(scene.name);
			}
		}
		for (int loop = 0; loop < unloadList.Count; loop++)
		{
			KAMUNITY.Logger.LogFormat("Unload scene : {0}", unloadList[loop]);
			yield return SceneManager.UnloadSceneAsync(unloadList[loop]);
		}
	}
}
