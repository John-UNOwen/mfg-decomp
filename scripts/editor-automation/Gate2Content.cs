// Editor automation for the WebGL content-state reconstruction (docs/21 section 10 policy).
//
// The shipped game used addressables v1.21.19 with a packed catalog that no longer has an editor
// content state (AssetRipper does not reconstruct AddressableAssetSettings). scripts/catalog_decode.py
// + scripts/catalog_rebuild_spec.py decode the shipped catalog.json blobs into a rebuild spec TSV:
// one row per asset entry with the shipped primary address, the shipped label keys, and the shipped
// bundle membership. This file replays that spec into fresh AddressableAssetSettings (group per
// shipped bundle so the packer regenerates a dependency-equivalent bundle graph) and then builds the
// WebGL player; the 1.21 player-build hook (AddressablesPlayerBuildProcessor.PrepareForBuild) packs
// content for the active target automatically and adds Library/com.unity.addressables/aa to the
// player's StreamingAssets.
//
// Run headless:
//   Unity.exe -batchmode -projectPath C:\mfg\build\unity-project `
//       -executeMethod MfgRebuild.Gate2Content.BuildContentState -logFile <log>
//   ... -executeMethod MfgRebuild.Gate2Content.BuildWebGLPlayer -logFile <log>
//
// Exit codes: 0 ok, 2 fatal precondition, 3 target switch failed, 4 no scenes, 5 player build failed.

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build.Reporting;

namespace MfgRebuild
{
    public static class Gate2Content
    {
        private const string SpecPath = @"C:\mfgtmp\rebuild_spec_assets.tsv";
        private const string SideLog = @"C:\mfgtmp\side.log.txt";

        private static void Side(string msg)
        {
            try
            {
                System.IO.File.AppendAllText(SideLog,
                    DateTime.Now.ToString("HH:mm:ss") + " " + msg + "\n");
            }
            catch { }
        }

        private static AddressableAssetSettings EnsureSettings()
        {
            // The default-object getter can NRE when a previous run registered the config
            // object and the asset was since deleted outside Unity; treat that as "none".
            AddressableAssetSettings settings = null;
            try { settings = AddressableAssetSettingsDefaultObject.Settings; } catch { settings = null; }
            if (settings != null) return settings;
            settings = AddressableAssetSettings.Create(
                "Assets/AddressableAssetsData", "AddressableAssetSettings", true, true);
            try { if (AddressableAssetSettingsDefaultObject.Settings == null) AddressableAssetSettingsDefaultObject.Settings = settings; }
            catch { }
            if (settings == null)
            {
                try { settings = AddressableAssetSettingsDefaultObject.Settings; } catch { }
            }
            return settings;
        }

        // Replays the rebuild spec into editor content state: one group per shipped bundle,
        // entry address = shipped primary key, entry labels = shipped label keys.
        public static void BuildContentState()
        {
            if (!File.Exists(SpecPath))
            {
                Side("content build aborted: missing " + SpecPath);
                EditorApplication.Exit(2);
                return;
            }
            var settings = EnsureSettings();
            if (settings == null)
            {
                Side("content build aborted: could not create AddressableAssetSettings");
                EditorApplication.Exit(2);
                return;
            }
            Side($"content state start settings={AssetDatabase.GetAssetPath(settings)} persisted={settings.IsPersisted}");

            var schemaTypes = new[] { typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema) };
            var groups = new Dictionary<string, AddressableAssetGroup>(StringComparer.Ordinal);
            var labelSeen = new HashSet<string>(StringComparer.Ordinal);
            int rows = 0, entriesMoved = 0, guidMissing = 0, labelsAdded = 0, groupsCreated = 0;

            // Without batching, each group CreateAsset triggers a full AssetDatabase import
            // cycle (~1.5 s x 28k groups = days). Defer all imports to one flush at Stop.
            AssetDatabase.StartAssetEditing();
            try
            {
                using (var sr = new StreamReader(SpecPath, System.Text.Encoding.UTF8))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.Length == 0 || line[0] == '#') continue;
                        var f = line.Split('\t');
                        if (f.Length < 3) { guidMissing++; continue; }
                        string path = f[0], addr = f[1], bundle = f[2];

                        AddressableAssetGroup group;
                        if (!groups.TryGetValue(bundle, out group))
                        {
                            group = settings.CreateGroup(bundle, false, false, false, null, schemaTypes);
                            groups[bundle] = group;
                            groupsCreated++;
                        }

                        string guid = AssetDatabase.AssetPathToGUID(path, AssetPathToGUIDOptions.OnlyExistingAssets);
                        if (string.IsNullOrEmpty(guid)) { guidMissing++; continue; }

                        var entry = settings.CreateOrMoveEntry(guid, group, false, false);
                        entry.address = addr;
                        entriesMoved++;

                        if (f.Length > 3 && f[3].Length > 0)
                        {
                            foreach (var label in f[3].Split('|'))
                            {
                                if (label.Length == 0) continue;
                                if (labelSeen.Add(label))
                                {
                                    settings.AddLabel(label, false);
                                    labelsAdded++;
                                }
                                entry.SetLabel(label, true, false, false);
                            }
                        }

                        rows++;
                        if (rows % 5000 == 0)
                            Side($"content state progress rows={rows} groups={groupsCreated} labels={labelsAdded} guidMissing={guidMissing}");
                    }
                }

                EditorUtility.SetDirty(settings);
                settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true, true);
                AssetDatabase.SaveAssets();
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            int entryCount = 0;
            foreach (var g in settings.groups)
                if (g != null) entryCount += g.entries.Count;
            Side($"content state done rows={rows} entries={entriesMoved} groups={groupsCreated} labels={labelsAdded} " +
                $"guidMissing={guidMissing} liveEntries={entryCount} labelCount={settings.GetLabels().Count}");
            EditorApplication.Exit(0);
        }

        // Verifies the persisted content state, switches to WebGL, and builds the player.
        // Addressables content packing for WebGL runs inside BuildPipeline via the package hook.
        public static void BuildWebGLPlayer()
        {
            var settings = EnsureSettings();
            if (settings == null)
            {
                Side("webgl build aborted: no settings");
                EditorApplication.Exit(2);
                return;
            }
            int entryCount = 0;
            foreach (var g in settings.groups)
                if (g != null) entryCount += g.entries.Count;
            Side($"webgl build start groups={settings.groups.Count} entries={entryCount} labels={settings.GetLabels().Count} target={EditorUserBuildSettings.activeBuildTarget}");

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
            {
                bool ok = EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
                Side($"switch webgl ok={ok} now={EditorUserBuildSettings.activeBuildTarget}");
                if (!ok) { EditorApplication.Exit(3); return; }
            }

            var scenes = new List<string>();
            foreach (var s in EditorBuildSettings.scenes)
            {
                if (!s.enabled) continue;
                if (File.Exists(s.path)) scenes.Add(s.path);
                else Side($"scene missing from disk: {s.path}");
            }
            Side($"scenes enabled={scenes.Count}");
            if (scenes.Count == 0) { EditorApplication.Exit(4); return; }

            var options = new BuildPlayerOptions
            {
                scenes = scenes.ToArray(),
                locationPathName = @"C:/mfg/build/unity-project/out/webgl",
                target = BuildTarget.WebGL,
                targetGroup = BuildTargetGroup.WebGL,
                options = BuildOptions.None,
            };
            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;
            Side($"webgl build result={summary.result} time={summary.totalTime} out={options.locationPathName}");
            EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 5);
        }
    }
}
