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

        // Import pass: asset/database import + script compilation happen before executeMethod runs.
        // Also turns off play-mode domain/scene reload so the PlayGate2 pump survives entering play.
        public static void ImportAndQuit()
        {
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions =
                EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
            AssetDatabase.SaveAssets();
            Debug.Log($"AUTOMATION import done dataPath={Application.dataPath} scenes_in_build={EditorBuildSettings.scenes.Length}");
            EditorApplication.Exit(0);
        }

        // Play-mode boot proof: enter play mode, then pump until the Boot/Title scene is live or the
        // timeout hits. Exit code 0 = boot reached, 2 = timeout. kyoku interaction is proven in the
        // standalone player (mouse automation), not headless.
        public static void PlayGate2()
        {
            Debug.Log("AUTOMATION entering play mode");
            EditorApplication.EnterPlaymode();
            startedAt = EditorApplication.timeSinceStartup;
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
                EditorApplication.Exit(0);
                return;
            }
            if (elapsed > PlayTimeoutS)
            {
                Debug.Log($"AUTOMATION GATE2_BOOT_TIMEOUT active_scene={active} elapsed={elapsed:F1}s");
                EditorApplication.Exit(2);
            }
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
