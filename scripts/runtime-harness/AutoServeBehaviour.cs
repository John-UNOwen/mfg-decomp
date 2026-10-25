using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MfgRebuild
{
    // Rebuild harness for headless play: screenshots come back black in this environment and the
    // addressable UI canvases scale in ways that make coordinate guessing flaky, so drive the UI
    // from code instead: with -AUTOSERVE every 2 s inspect the active flow scene, log the live
    // buttons (so the next step can be chosen from evidence), and advance known gates:
    //   Title            -> set private _loginRequest (ScreenTouch needs a cabinet touch panel)
    //   Login            -> BtGuestStart onClick (guest entry)
    //   FirstCharaSelect -> CharaSelectWindow decide button onClick (closes the select window)
    //   Continue         -> first interactable button (pre-kyoku continue screen)
    // Each gate is served once per scene instance counter, so screens advance exactly once.
    public class AutoServeBehaviour : MonoBehaviour
    {
        private static AutoServeBehaviour instance;
        private float timer;
        private int polls;
        private readonly HashSet<string> served = new HashSet<string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            if (!Array.Exists(Environment.GetCommandLineArgs(), a => a == "-AUTOSERVE")) return;
            var go = new GameObject("MfgAutoServe");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<AutoServeBehaviour>();
            Debug.Log("AUTOSERVE installed");
        }

        private void Update()
        {
            timer += Time.unscaledDeltaTime;
            if (timer < 2f) return;
            timer = 0f;
            polls++;
            string active = ActiveFlowScene();
            var buttons = LiveButtons(active);
            if (polls % 3 == 1)
            {
                Debug.Log($"AUTOSERVE poll active='{active}' buttons=[{string.Join(",", buttons.Keys)}]");
            }
            Advance(active, buttons);
        }

        private void Advance(string active, Dictionary<string, Button> buttons)
        {
            switch (active)
            {
                case "Title" when !served.Contains("Title"):
                {
                    var flow = FindFlow<TitleFlow>(active);
                    if (flow == null) return;
                    var f = typeof(TitleFlow).GetField("_loginRequest", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    f.SetValue(flow, true);
                    served.Add("Title");
                    Debug.Log("AUTOSERVE title login request set");
                    return;
                }
                case "Login" when !served.Contains("Login"):
                {
                    if (!TryInvoke(buttons, "BtGuestStart", "Login")) return;
                    served.Add("Login");
                    return;
                }
                case "FirstCharaSelect" when !served.Contains("FirstCharaSelect"):
                {
                    // The window close trigger lives on the decide Button inside CharaSelectWindow;
                    // find any Button named BtDecide (scene naming) else poke the flow's window field.
                    foreach (string name in buttons.Keys)
                    {
                        if (name.IndexOf("Decide", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            if (TryInvoke(buttons, name, active)) served.Add(active);
                            return;
                        }
                    }
                    var flow = FindFlow<FirstCharaSelectFlow>(active);
                    if (flow == null) return;
                    var wf = typeof(FirstCharaSelectFlow).GetField("_charaSelectWindow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var win = wf.GetValue(flow) as MonoBehaviour;
                    if (win == null) return;
                    var bf = win.GetType().GetField("_decideButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var btn = bf.GetValue(win) as Button;
                    if (btn == null) return;
                    try
                    {
                        btn.onClick.Invoke();
                        served.Add(active);
                        Debug.Log("AUTOSERVE invoked chara window decide (reflection)");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"AUTOSERVE invoke failed {e.GetType().Name}: {e.Message}");
                    }
                    return;
                }
                case "Continue" when !served.Contains("Continue"):
                {
                    foreach (string name in buttons.Keys)
                    {
                        if (name.IndexOf("Decide", StringComparison.OrdinalIgnoreCase) >= 0 || name.StartsWith("Bt"))
                        {
                            if (TryInvoke(buttons, name, active)) served.Add(active);
                            return;
                        }
                    }
                    return;
                }
            }
        }

        private bool TryInvoke(Dictionary<string, Button> buttons, string name, string scene)
        {
            if (!buttons.TryGetValue(name, out Button btn)) return false;
            try
            {
                btn.onClick.Invoke();
                Debug.Log($"AUTOSERVE invoked {name} onClick in {scene}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"AUTOSERVE invoke failed {e.GetType().Name}: {e.Message}");
                return false;
            }
        }

        private static string ActiveFlowScene()
        {
            // ResidentFlow.LoadScene unloads the previous flow scene, so at most one flow scene
            // (plus Resident) is loaded; take the last non-Resident loaded scene name.
            string name = "";
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene s = SceneManager.GetSceneAt(i);
                if (s.IsValid() && s.name != "Resident") name = s.name;
            }
            return name;
        }

        private static Dictionary<string, Button> LiveButtons(string sceneName)
        {
            var result = new Dictionary<string, Button>();
            if (sceneName == "") return result;
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid() || !scene.isLoaded) return result;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Collect(root, result);
            }
            return result;
        }

        private static void Collect(GameObject go, Dictionary<string, Button> result)
        {
            Button btn = go.GetComponent<Button>();
            if (btn != null && go.activeInHierarchy && btn.interactable && !result.ContainsKey(go.name))
            {
                result[go.name] = btn;
            }
            Transform trs = go.transform;
            for (int i = 0; i < trs.childCount; i++) Collect(trs.GetChild(i).gameObject, result);
        }

        private static T FindFlow<T>(string sceneName) where T : MonoBehaviour
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid() || !scene.isLoaded) return null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                T flow = root.GetComponentInChildren<T>(true);
                if (flow != null) return flow;
            }
            return null;
        }
    }
}
