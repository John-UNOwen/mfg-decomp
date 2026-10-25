using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

// Headless Gate 2 automation, added by the rebuild (not shipped code). All entry points are run
// with Unity 2022.3.13f1 -batchmode once a license exists:
//   Unity.exe -batchmode -projectPath build\unity-project -executeMethod MfgRebuild.Automation.ImportAndQuit -logFile <repo>\tmp\import.log
//   Unity.exe -batchmode -projectPath build\unity-project -executeMethod MfgRebuild.Automation.PlayGate2 -FRONTURL http://127.0.0.1:8080/aog -logFile <repo>\tmp\gate2.log
//   Unity.exe -batchmode -projectPath build\unity-project -executeMethod MfgRebuild.Automation.BuildStandalone -logFile <repo>\tmp\build.log
// -FRONTURL is consumed by the shipping ResidentFlow.ApplyCommandLineArgs path (it reads
// Environment.GetCommandLineArgs in the editor too), so play mode talks to mock/provider directly.
namespace MfgRebuild
{
    public static class Automation
    {
        private const double PlayTimeoutS = 420.0;   // import already done at this point; boot should reach Boot/Title well inside
        private static double startedAt = -1.0;
        private static double lastHeartbeat = -15.0;

        // Import pass: asset/database import + script compilation happen before executeMethod runs.
        // Play mode keeps full domain+scene reload: ResidentFlow boots in Awake/Start, and with
        // Enter Play Mode Options those callbacks are skipped (observed stall right after play entry).
        public static void ImportAndQuit()
        {
            EditorSettings.enterPlayModeOptionsEnabled = false;
            AssetDatabase.SaveAssets();
            Debug.Log($"AUTOMATION import done dataPath={Application.dataPath} scenes_in_build={EditorBuildSettings.scenes.Length}");
            EditorApplication.Exit(0);
        }

        private const string PrefsKey = "MfgRebuil******Key";

        // Play-mode boot proof: enter play mode, then pump until the Boot/Title scene is live or the
        // timeout hits. Exit code 0 = boot reached, 2 = timeout. With domain reload on, this domain's
        // statics/subscription are wiped at play entry, so PlayGate2 only arms the run (EditorPrefs
        // survives the reload) and PlayHook re-attaches the pump inside the fresh play domain.
        public static void PlayGate2()
        {
            EditorSettings.enterPlayModeOptionsEnabled = false;
            EditorPrefs.SetBool(PrefsKey, true);
            try { System.IO.File.Delete(@"C:\mfgtmp\play.log.txt"); } catch { }
            Debug.Log("AUTOMATION entering play mode");
            EditorApplication.EnterPlaymode();
        }

        // BeforeSceneLoad so the sink is live before ResidentFlow's Awake throws anything.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void PlayHook()
        {
            if (!EditorPrefs.GetBool(PrefsKey, false)) return;
            Debug.Log("AUTOMATION play-domain pump attached");
            var ta = Resources.Load("certs/DigiCert") as TextAsset;
            var ta2 = Resources.Load("certs/digicert") as TextAsset;
            Debug.Log($"AUTOMATION playcertprobe exact={(ta == null ? "NULL" : ta.text.Length + " chars")} lower={(ta2 == null ? "NULL" : ta2.text.Length + " chars")}");
            Application.logMessageReceived += (msg, stack, type) =>
            {
                try
                {
                    System.IO.File.AppendAllText(@"C:\mfgtmp\play.log.txt", $"[{type}] {msg}\n{stack}\n---\n");
                }
                catch { }
            };
            EditorApplication.update += Pump;
        }

        private static void Pump()
        {
            if (startedAt < 0) startedAt = EditorApplication.timeSinceStartup;
            double elapsed = EditorApplication.timeSinceStartup - startedAt;
            string active = SceneManager.sceneCount > 0 && SceneManager.GetActiveScene().IsValid()
                ? SceneManager.GetActiveScene().name : "<none>";
            if (elapsed > 10.0 && (active == "Boot" || active == "Title" || active == "TitleDemo"))
            {
                Debug.Log($"AUTOMATION GATE2_BOOT_OK active_scene={active} elapsed={elapsed:F1}s");
                EditorPrefs.SetBool(PrefsKey, false);
                EditorApplication.Exit(0);
                return;
            }
            if (elapsed > PlayTimeoutS)
            {
                Debug.Log($"AUTOMATION GATE2_BOOT_TIMEOUT active_scene={active} elapsed={elapsed:F1}s");
                EditorPrefs.SetBool(PrefsKey, false);
                EditorApplication.Exit(2);
                return;
            }
            if (elapsed - lastHeartbeat > 15.0)
            {
                lastHeartbeat = elapsed;
                Debug.Log($"AUTOMATION heartbeat t={elapsed:F0}s active_scene={active} scenes={SceneManager.sceneCount}");
            }
        }

        // Diagnostics: does Resources actually contain the certs in this project copy?
        public static void LoadCertProbe()
        {
            int okCount = 0;
            foreach (string p in new[] { "certs/DigiCert", "certs/GeoTrustRSACA_2018", "certs/RapidSSL_RSA_CA_2018" })
            {
                var ta = Resources.Load(p) as TextAsset;
                Debug.Log($"AUTOMATION certprobe {p} = {(ta == null ? "NULL" : ta.text.Length + " chars")}");
                if (ta != null) okCount++;
            }
            var any = Resources.Load("DebugMenu") as GameObject;
            Debug.Log($"AUTOMATION resprobe DebugMenu = {(any == null ? "NULL" : "ok")}");
            EditorApplication.Exit(okCount == 3 ? 0 : 1);
        }

        // Standalone Windows64 player build (for the kyoku-level tap automation).
        public static void BuildStandalone()
        {
            var scenes = new System.Collections.Generic.List<string>();
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
                scenes.Add(EditorBuildSettings.scenes[i].path);
            string outDir = System.IO.Path.Combine(new System.IO.DirectoryInfo(Application.dataPath).Parent.FullName, "out", "standalone");
            var opts = new BuildPlayerOptions
            {
                scenes = scenes.ToArray(),
                locationPathName = System.IO.Path.Combine(outDir, "MFG.exe"),
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None,
            };
            BuildReport report = BuildPipeline.BuildPlayer(opts);
            var summary = report.summary;
            Debug.Log($"AUTOMATION build result={summary.result} total={summary.totalSize} bytes errors={summary.totalErrors}");
            EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
        }
    }
}
