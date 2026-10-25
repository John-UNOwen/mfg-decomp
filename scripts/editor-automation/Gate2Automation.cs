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
            ProvisionEditorAddressables();
            EditorPrefs.SetBool(PrefsKey, true);
            try { System.IO.File.Delete(@"C:\mfgtmp\play.log.txt"); } catch { }
            try { System.IO.File.Delete(@"C:\mfgtmp\side.log.txt"); } catch { }
            Debug.Log("AUTOMATION entering play mode");
            EditorApplication.EnterPlaymode();
        }

        // The project has no AddressableAssetsData (shipped game used a prebuilt catalog), so the
        // package's editor init looks for Library/com.unity.addressables/aa/Windows/settings.json and
        // fails with 'Invalid path in TextDataProvider'. The shipped settings.json resolves the
        // catalog and bundles via {RuntimePath} (= Assets/StreamingAssets/aa) in the editor too, so
        // dropping it at the expected editor location makes editor play serve the shipped layout.
        private static void ProvisionEditorAddressables()
        {
            try
            {
                string project = System.IO.Directory.GetParent(Application.dataPath).FullName;
                string srcDir = System.IO.Path.Combine(project, "Assets", "StreamingAssets", "aa");
                string dstDir = System.IO.Path.Combine(project, "Library", "com.unity.addressables", "aa", "Windows");
                System.IO.Directory.CreateDirectory(dstDir);
                foreach (string f in new[] { "settings.json", "catalog.json" })
                    System.IO.File.Copy(System.IO.Path.Combine(srcDir, f), System.IO.Path.Combine(dstDir, f), overwrite: true);
                Side($"aa files provisioned -> {dstDir}");
            }
            catch (System.Exception e)
            {
                Side($"aa settings provision failed {e.Message}");
            }
        }

        // BeforeSceneLoad so the sink is live before ResidentFlow's Awake throws anything.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void PlayHook()
        {
            if (!EditorPrefs.GetBool(PrefsKey, false)) return;
            Side("play-domain hook ran");
            // ResidentFlow.InitializeOnBoot silences the Unity logger for arcade cosmetics; the
            // editor needs it back or none of the boot progress logs are observable.
            UnityEngine.Debug.unityLogger.logEnabled = true;
            PreloadCertificates();
            var ta = Resources.Load("certs/DigiCert") as TextAsset;
            Side($"playcertprobe exact={(ta == null ? "NULL" : ta.text.Length + " chars")}");
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                Side($"UNHANDLED {e.ExceptionObject.GetType().Name}: {Truncate(e.ExceptionObject.ToString(), 900)}");
            Application.logMessageReceivedThreaded += (msg, stack, type) =>
            {
                try
                {
                    string line = $"[{type}] {Truncate(msg, 400)}";
                    if (type != UnityEngine.LogType.Log && type != UnityEngine.LogType.Exception)
                        line += $"\n{Truncate(stack, 600)}";
                    else if (type == UnityEngine.LogType.Exception)
                        line += $"\n{Truncate(stack, 400)}";
                    System.IO.File.AppendAllText(@"C:\mfgtmp\play.log.txt", line + "\n---\n");
                }
                catch { }
            };
            EditorApplication.update += Pump;
        }

        // The shipping boot calls CertificateValidation.Init from the network stack's worker thread.
        // Resources.Load only answers on the main thread inside the editor (players are fine), so the
        // cert list is prebuilt here on the main thread; Init() sees the field non-null and skips its
        // own loads. Editor-only shim, no shipping-code change.
        private static void PreloadCertificates()
        {
            try
            {
                var asm = typeof(RequestManager).Assembly;
                var t = asm.GetType("Konami.System.Net.CertificateValidation");
                var f = t?.GetField("s_CertificateList",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                if (f == null || f.GetValue(null) != null) { Side("certpreload skipped"); return; }
                var coll = new System.Security.Cryptography.X509Certificates.X509Certificate2Collection();
                foreach (string p in new[] { "certs/DigiCert", "certs/GeoTrustRSACA_2018", "certs/RapidSSL_RSA_CA_2018" })
                {
                    var ta = Resources.Load(p) as TextAsset;
                    if (ta == null) { Side($"certpreload missing {p}"); return; }
                    coll.Add(new System.Security.Cryptography.X509Certificates.X509Certificate2(
                        System.Text.Encoding.ASCII.GetBytes(ta.text)));
                }
                f.SetValue(null, coll);
                Side("certpreload done 3 certs");
            }
            catch (System.Exception e)
            {
                Side($"certpreload failed {e.GetType().Name}: {e.Message}");
            }
        }

        private static void Side(string message)
        {
            try { System.IO.File.AppendAllText(@"C:\mfgtmp\side.log.txt", $"{System.DateTime.Now:HH:mm:ss} {message}\n"); }
            catch { }
        }

        private static string Truncate(string s, int max)
        {
            if (s == null) return "<null>";
            return s.Length <= max ? s : s.Substring(0, max) + "...";
        }

        private static void Pump()
        {
            if (startedAt < 0) startedAt = EditorApplication.timeSinceStartup;
            double elapsed = EditorApplication.timeSinceStartup - startedAt;
            string active = SceneManager.sceneCount > 0 && SceneManager.GetActiveScene().IsValid()
                ? SceneManager.GetActiveScene().name : "<none>";
            if (elapsed > 10.0 && (active == "Boot" || active == "Title" || active == "TitleDemo"))
            {
                Side($"GATE2_BOOT_OK active_scene={active} elapsed={elapsed:F1}s");
                EditorPrefs.SetBool(PrefsKey, false);
                EditorApplication.Exit(0);
                return;
            }
            if (elapsed > PlayTimeoutS)
            {
                Side($"GATE2_BOOT_TIMEOUT active_scene={active} elapsed={elapsed:F1}s");
                EditorPrefs.SetBool(PrefsKey, false);
                EditorApplication.Exit(2);
                return;
            }
            if (elapsed - lastHeartbeat > 15.0)
            {
                lastHeartbeat = elapsed;
                var sb = new System.Text.StringBuilder();
                sb.Append($"heartbeat t={elapsed:F0}s active='{active}'");
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    var sc = SceneManager.GetSceneAt(i);
                    sb.Append($" | [{i}]'{sc.name}' roots={sc.rootCount}");
                    if (sc.IsValid() && sc.isLoaded)
                    {
                        var roots = sc.GetRootGameObjects();
                        for (int j = 0; j < roots.Length && j < 6; j++) sb.Append($" {roots[j].name}");
                    }
                }
                try
                {
                    sb.Append($" | rf={ResidentFlow.IsInitialized} net={JubeatPlus.NetworkService.IsInitialized}");
                }
                catch (System.Exception e) { sb.Append($" | state_err={e.GetType().Name}"); }
                Side(sb.ToString());
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
