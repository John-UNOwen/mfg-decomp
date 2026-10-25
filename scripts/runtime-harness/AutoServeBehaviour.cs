using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MfgRebuild
{
    // Rebuild harness for headless play: screenshots come back black in this environment and the
    // addressable UI canvases scale in ways that make coordinate guessing flaky, so drive the UI
    // from code instead. With -AUTOSERVE: every 2 s log the active flow scene plus its live
    // interactable buttons, advance hard-coded field-injection gates (Title, Login guest,
    // FirstCharaSelect window), and apply rules from C:\mfgtmp\autocode.cfg (re-read every poll,
    // so the path can be steered from outside while the player runs).
    //
    // Rule line format:  scene|match|pattern[ rep]
    //   match = exact | contains
    //   pattern = button name substring/exact; ' rep' suffix re-invokes every poll (for turn loops)
    //   without ' rep' a (scene, button) pair fires at most once.
    public class AutoServeBehaviour : MonoBehaviour
    {
        private const string CfgPath = @"C:\mfgtmp\autocode.cfg";

        private static AutoServeBehaviour instance;
        private float timer;
        private int polls;
        private readonly HashSet<string> served = new HashSet<string>();

        private class Rule
        {
            public string Scene;
            public bool Contains;
            public string Pattern;
            public bool Repeat;
            public bool Force;
        }

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
            Builtins(active, buttons);
            Dictionary<string, Button> forceButtons = null;
            foreach (Rule rule in LoadRules())
            {
                if (rule.Scene != active) continue;
                Dictionary<string, Button> pool = buttons;
                if (rule.Force)
                {
                    forceButtons = forceButtons ?? LiveButtons(active, force: true);
                    pool = forceButtons;
                }
                foreach (KeyValuePair<string, Button> kv in pool)
                {
                    bool hit = rule.Contains ? kv.Key.IndexOf(rule.Pattern, StringComparison.OrdinalIgnoreCase) >= 0 : kv.Key == rule.Pattern;
                    if (!hit) continue;
                    string key = rule.Repeat ? "rep:" + active + ":" + kv.Key : active + ":" + kv.Key;
                    if (!rule.Repeat && served.Contains(key)) continue;
                    if (TryInvoke(kv.Value, kv.Key, active))
                    {
                        served.Add(key);
                        break; // one button per rule per poll
                    }
                }
            }
        }

        private void Builtins(string active, Dictionary<string, Button> buttons)
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
                    if (TryInvokeGet(buttons, "BtGuestStart", active)) served.Add("Login");
                    return;
                }
                case "FirstCharaSelect" when !served.Contains("FirstCharaSelect"):
                {
                    var flow = FindFlow<FirstCharaSelectFlow>(active);
                    if (flow == null) return;
                    var wf = typeof(FirstCharaSelectFlow).GetField("_charaSelectWindow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var win = wf.GetValue(flow) as MonoBehaviour;
                    if (win == null) return;
                    var bf = win.GetType().GetField("_decideButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var btn = bf.GetValue(win) as Button;
                    if (btn == null) return;
                    if (TryInvoke(btn, "chara window decide (reflection)", active)) served.Add(active);
                    return;
                }
            }
        }

        private bool TryInvokeGet(Dictionary<string, Button> buttons, string name, string scene)
        {
            return buttons.TryGetValue(name, out Button btn) && TryInvoke(btn, name, scene);
        }

        private bool TryInvoke(Button btn, string label, string scene)
        {
            try
            {
                btn.onClick.Invoke();
                Debug.Log($"AUTOSERVE invoked {label} onClick in {scene}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"AUTOSERVE invoke failed {label}: {e.GetType().Name}: {e.Message}");
                return false;
            }
        }

        private static List<Rule> LoadRules()
        {
            var rules = new List<Rule>();
            try
            {
                if (!File.Exists(CfgPath)) return rules;
                foreach (string raw in File.ReadAllLines(CfgPath))
                {
                    string line = raw.Trim();
                    if (line.Length == 0 || line.StartsWith("//")) continue;
                    string[] parts = line.Split('|');
                    if (parts.Length < 3) continue;
                    var rule = new Rule { Scene = parts[0].Trim(), Pattern = parts[2].Trim() };
                    string mode = parts[1].Trim();
                    rule.Force = mode.EndsWith("f");
                    if (rule.Force) mode = mode.Substring(0, mode.Length - 1);
                    rule.Contains = mode.Equals("contains", StringComparison.OrdinalIgnoreCase);
                    if (rule.Pattern.EndsWith(" rep"))
                    {
                        rule.Repeat = true;
                        rule.Pattern = rule.Pattern.Substring(0, rule.Pattern.Length - 4).Trim();
                    }
                    if (rule.Scene.Length > 0 && rule.Pattern.Length > 0) rules.Add(rule);
                }
            }
            catch (Exception e)
            {
                Debug.Log($"AUTOSERVE cfg read failed: {e.Message}");
            }
            return rules;
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

        private static Dictionary<string, Button> LiveButtons(string sceneName, bool force = false)
        {
            var result = new Dictionary<string, Button>();
            if (sceneName == "") return result;
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid() || !scene.isLoaded) return result;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Collect(root, result, force);
            }
            return result;
        }

        private static void Collect(GameObject go, Dictionary<string, Button> result, bool force)
        {
            Button btn = go.GetComponent<Button>();
            if (btn != null && go.activeInHierarchy && (force || btn.interactable) && !result.ContainsKey(go.name))
            {
                result[go.name] = btn;
            }
            Transform trs = go.transform;
            for (int i = 0; i < trs.childCount; i++) Collect(trs.GetChild(i).gameObject, result, force);
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
