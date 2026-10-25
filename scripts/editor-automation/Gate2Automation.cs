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
        private const string DumpPrefsKey = "MfgRebu****Prefs";

        // Play-mode catalog dump: enter play, pump Addressables.InitializeAsync to completion (the
        // shipped catalog parses fine in play; only bundle opening has the editor-layout quirk), then
        // dump every locator key + resolved location to C:\mfgtmp\catalog_manifest.tsv for the
        // WebGL content-state reconstruction. TSV: key \t providerId \t internalId \t typeName \t deps.
        public static void PlayCatalogDump()
        {
            EditorSettings.enterPlayModeOptionsEnabled = false;
            ProvisionEditorAddressables();
            EditorPrefs.SetBool(DumpPrefsKey, true);
            EditorPrefs.SetBool(PrefsKey, false);
            try { System.IO.File.Delete(@"C:\mfgtmp\play.log.txt"); } catch { }
            try { System.IO.File.Delete(@"C:\mfgtmp\side.log.txt"); } catch { }
            try { System.IO.File.Delete(@"C:\mfgtmp\catalog_manifest.tsv"); } catch { }
            Debug.Log("AUTOMATION entering play mode (catalog dump)");
            EditorApplication.EnterPlaymode();
        }

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
        // fails with 'Invalid path in TextDataProvider'. Junction the editor package layout directory
        // onto the shipped Assets/StreamingAssets/aa tree: settings + catalog resolve there, and a
        // junction is skipped by the player build's file collection, so it cannot conflict the way a
        // copied catalog does (SetupDataFiles 'callback provided streaming assets file conflicts').
        private static void ProvisionEditorAddressables()
        {
            try
            {
                string project = System.IO.Directory.GetParent(Application.dataPath).FullName;
                string link = System.IO.Path.Combine(project, "Library", "com.unity.addressables", "aa");
                if (System.IO.Directory.Exists(System.IO.Path.Combine(link, "Windows")))
                {
                    Side("aa provision present");
                    return;
                }
                if (System.IO.Directory.Exists(link))
                {
                    // Earlier runs copied real directories here; those files conflict with the
                    // player build's streaming assets merge, so replace them with the junction.
                    System.IO.Directory.Delete(link, recursive: true);
                }
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(link));
                string target = System.IO.Path.Combine(project, "Assets", "StreamingAssets", "aa");
                int rc = Symlink(link, target);
                Side(rc == 0 ? "aa junction created" : $"aa mklink rc={rc}");
            }
            catch (System.Exception e)
            {
                Side($"aa provision failed {e.Message}");
            }
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
        private static extern bool CreateSymbolicLinkW(string lpSymlinkFileName, string lpTargetFileName, uint dwFlags);

        // mklink /J semantics: FLAG_DIRECTORY | FLAG_ABSOLUTE. cmd fallback for sessions without
        // SeCreateSymbolicLink privilege; junctions need no privilege.
        private static int Symlink(string link, string target)
        {
            if (CreateSymbolicLinkW(link, target, 0x1 | 0x2)) return 0;
            var psi = new System.Diagnostics.ProcessStartInfo("cmd.exe",
                "/c mklink /J \"" + link + "\" \"" + target + "\"")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            };
            using (var p = System.Diagnostics.Process.Start(psi))
            {
                p.StandardOutput.ReadToEnd();
                p.WaitForExit(10000);
                return p.ExitCode;
            }
        }

        // BeforeSceneLoad so the sink is live before ResidentFlow's Awake throws anything.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void PlayHook()
        {
            bool dumpMode = EditorPrefs.GetBool(DumpPrefsKey, false);
            if (!dumpMode && !EditorPrefs.GetBool(PrefsKey, false)) return;
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
            if (dumpMode)
            {
                // Reflection only: Assembly-CSharp-Editor cannot see UnityEngine.ResourceManager /
                // .AddressableAssets (package asmdefs are autoReferenced=false), and the shipped game
                // reaches addressables without a compile-time reference too (S3: no Addressables. in
                // recovered code). Start init through the public Addressables class, then everything
                // below runs late-bound on the play domain's objects.
                var initMi = typeof(UnityEngine.AddressableAssets.Addressables).GetMethod("InitializeAsync",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
                    null, Type.EmptyTypes, null);
                if (initMi == null)
                {
                    Side("Addressables.InitializeAsync() not found");
                    EditorPrefs.SetBool(DumpPrefsKey, false);
                    EditorApplication.Exit(2);
                    return;
                }
                dumpHandle = initMi.Invoke(null, null);
                EditorApplication.update += DumpPump;
                return;
            }
            EditorApplication.update += Pump;
        }

        private static object dumpHandle;

        private static void DumpPump()
        {
            try
            {
                var ht = dumpHandle.GetType();
                var isDone = (bool)ht.GetProperty("IsDone").GetValue(dumpHandle);
                if (!isDone)
                {
                    if (startedAt < 0)
                    {
                        startedAt = EditorApplication.timeSinceStartup;
                        Side("catalog init started");
                    }
                    else if (EditorApplication.timeSinceStartup - startedAt > 300.0)
                    {
                        Side("catalog init timeout");
                        EditorPrefs.SetBool(DumpPrefsKey, false);
                        EditorApplication.Exit(2);
                    }
                    return;
                }
                if (startedAt < 0) startedAt = EditorApplication.timeSinceStartup;
                string status = ht.GetProperty("Status").GetValue(dumpHandle).ToString();
                object locator = ht.GetProperty("Result").GetValue(dumpHandle);
                if (locator == null || status != "Succeeded")
                {
                    var errProp = ht.GetProperty("OperationException");
                    object err = errProp == null ? null : errProp.GetValue(dumpHandle);
                    Side($"catalog init failed status={status} error={(err == null ? "<none>" : Truncate(err.ToString(), 300))}");
                    EditorPrefs.SetBool(DumpPrefsKey, false);
                    EditorApplication.Exit(2);
                    return;
                }
                object locateState = null; // [locator, locateMi] once resolved
                int written = 0, failed = 0;
                var sw = new System.IO.StreamWriter(@"C:\mfgtmp\catalog_manifest.tsv", false, System.Text.Encoding.UTF8);
                using (sw)
                {
                    sw.WriteLine("#key\tproviderId\tinternalId\ttypeName\tdeps");
                    foreach (object key in (System.Collections.IEnumerable)locator.GetType()
                        .GetProperty("Keys").GetValue(locator))
                    {
                        string keyStr = (key ?? "<null>").ToString();
                        if (keyStr.Length == 0 || keyStr.IndexOf('\t') >= 0) { failed++; continue; }
                        if (locateState == null)
                        {
                            System.Reflection.MethodInfo mi = null;
                            foreach (var m in locator.GetType().GetMethods())
                            {
                                var ps = m.GetParameters();
                                if (m.Name == "Locate" && ps.Length == 3 && ps[0].ParameterType == typeof(object)
                                    && ps[1].ParameterType == typeof(Type) && ps[2].ParameterType.IsByRefType)
                                { mi = m; break; }
                            }
                            if (mi == null)
                            {
                                Side($"Locate(object,Type,out) not found on {locator.GetType().Name}");
                                EditorPrefs.SetBool(DumpPrefsKey, false);
                                EditorApplication.Exit(2);
                                return;
                            }
                            locateState = new object[] { locator, mi };
                        }
                        var mi2 = (System.Reflection.MethodInfo)((object[])locateState)[1];
                        var args = new object[] { key, null, null };
                        object ok;
                        try { ok = mi2.Invoke(locator, args); }
                        catch { failed++; continue; }
                        if (!(ok is bool flag && flag) || args[2] == null) { failed++; continue; }
                        object loc = args[2];
                        var lt = loc.GetType();
                        var deps = new System.Text.StringBuilder();
                        try
                        {
                            var depsValue = lt.GetProperty("Dependencies").GetValue(loc) as System.Collections.IEnumerable;
                            if (depsValue != null)
                                foreach (object d in depsValue)
                                    deps.Append(';').Append(d == null ? "?" : d.GetType().GetProperty("InternalId")?.GetValue(d) ?? "?");
                        }
                        catch { }
                        string tname = (lt.GetProperty("PrimaryKeyType").GetValue(loc) as Type)?.Name ?? "";
                        sw.WriteLine($"{keyStr}\t{lt.GetProperty("ProviderId").GetValue(loc)}\t{lt.GetProperty("InternalId").GetValue(loc)}\t{tname}\t{deps}");
                        written++;
                    }
                }
                Side($"catalog dump done written={written} failed={failed} status={status} elapsed={EditorApplication.timeSinceStartup - startedAt:F1}s");
                EditorPrefs.SetBool(DumpPrefsKey, false);
                EditorApplication.Exit(0);
            }
            catch (System.Exception e)
            {
                Side($"catalog dump threw {e.GetType().Name}: {Truncate(e.ToString(), 1200)}");
                EditorPrefs.SetBool(DumpPrefsKey, false);
                EditorApplication.Exit(2);
            }
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

        // Compile-only pass: script errors stop the play boot silently (player domain reload
        // fails, no marker), so check compilation the same headless way as the import pass.
        public static void CompileCheck()
        {
            Debug.Log("AUTOMATION compilecheck begin");
            EditorApplication.Exit(0);
        }
    }
}
