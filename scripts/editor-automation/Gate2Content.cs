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

        // Template capture for the YAML-generation route (scripts/gen_content_state.py): makes one
        // sample group with three real entries + labels via the editor API, saves, and copies every
        // produced AddressableAssetsData file (+ metas, EditorBuildSettings.asset) to C:\mfgtmp\template
        // so the generator can replay exact serialization shapes for all 28k groups without paying
        // per-asset editor import/refresh costs.
        public static void CaptureTemplates()
        {
            var settings = EnsureSettings();
            if (settings == null)
            {
                Side("template capture: no settings");
                EditorApplication.Exit(2);
                return;
            }
            var schemaTypes = new[] { typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema) };
            var group = settings.CreateGroup("zz_sample_group", false, false, false, null, schemaTypes);
            string[] samplePaths =
            {
                "Assets/Bundle/Common/MatCap/matcap_test.png",
                "Assets/DFG-HeiSeiGothic-W7 Bingo.mat",
                "Assets/MFG/Addressables/Banner/Animations/BtGachaSeriesBase.controller",
            };
            int made = 0;
            foreach (var p in samplePaths)
            {
                string guid = AssetDatabase.AssetPathToGUID(p, AssetPathToGUIDOptions.OnlyExistingAssets);
                if (string.IsNullOrEmpty(guid)) { Side($"sample missing path {p}"); continue; }
                var entry = settings.CreateOrMoveEntry(guid, group, false, false);
                entry.address = "sample/" + System.IO.Path.GetFileName(p);
                settings.AddLabel("sample_label_alpha", false);
                settings.AddLabel("sample_label_beta", false);
                entry.SetLabel("sample_label_alpha", true, false, false);
                entry.SetLabel("sample_label_beta", true, false, false);
                made++;
            }
            EditorUtility.SetDirty(settings);
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true, true);
            AssetDatabase.SaveAssets();

            const string dst = @"C:\mfgtmp\template";
            Directory.CreateDirectory(dst);
            CopyTreeWithMetas("Assets/AddressableAssetsData", dst, ref made);
            File.Copy("EditorBuildSettings.asset", System.IO.Path.Combine(dst, "EditorBuildSettings.asset"), true);
            Side($"template capture done files copied (entries made={made})");
            EditorApplication.Exit(0);
        }

        private static void CopyTreeWithMetas(string srcFolder, string dstRoot, ref int counter)
        {
            if (!Directory.Exists(srcFolder)) return;
            foreach (var f in Directory.GetFiles(srcFolder, "*", SearchOption.AllDirectories))
            {
                if (f.EndsWith(".meta")) continue;
                string rel = f.Substring("Assets/".Length).Replace('/', '\\');
                string dstFile = System.IO.Path.Combine(dstRoot, rel);
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(dstFile));
                File.Copy(f, dstFile, true);
                counter++;
            }
            foreach (var f in Directory.GetFiles(srcFolder, "*.meta", SearchOption.AllDirectories))
            {
                string rel = f.Substring("Assets/".Length).Replace('/', '\\');
                string dstFile = System.IO.Path.Combine(dstRoot, rel);
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(dstFile));
                File.Copy(f, dstFile, true);
                counter++;
            }
        }

        // Load the generated content state after a plain import pass and cross-check counts
        // against C:\mfgtmp\gen_counts.txt written by scripts/gen_content_state.py.
        public static void VerifyContentState()
        {
            var settings = EnsureSettings();
            if (settings == null)
            {
                Side("verify: no settings (config object lost?)");
                EditorApplication.Exit(2);
                return;
            }
            int entryTotal = 0, schemaMissing = 0;
            foreach (var g in settings.groups)
            {
                if (g == null) continue;
                entryTotal += g.entries.Count;
                if (g.Schemas.Count == 0) schemaMissing++;
            }
            string expected = "<?>";
            try { expected = File.ReadAllText(@"C:\mfgtmp\gen_counts.txt").Trim(); } catch { }
            Side($"verify groups={settings.groups.Count} entries={entryTotal} " +
                $"labels={settings.GetLabels().Count} schemalessGroups={schemaMissing} expected[{expected}]");
            // dump one non-built-in group entry for eyeballing
            foreach (var g in settings.groups)
            {
                if (g == null || g.entries.Count == 0) continue;
                if (g.Name == "Built In Data" || g.Name == "Default Local Group") continue;
                AddressableAssetEntry e = null;
                foreach (var en in g.entries) { e = en; break; }
                if (e == null) continue;
                Side($"sample group={g.Name} entry guid={e.guid} addr={e.address} labels={e.labels.Count} schemas={g.Schemas.Count}");
                break;
            }
            EditorApplication.Exit(0);
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

            ApplyWebGLPlayerSettings();
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
            DumpBuildReport(report);
            EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 5);
        }

        // The AssetRipper-exported ProjectSettings carries an all-zero WebGL section
        // (webGLTemplate empty, memorySize 0). An empty template list makes
        // BuildPipeline.BuildPlayer abort with a bare IndexOutOfRangeException in
        // BuildPlayerInternalNoCheck right after level packaging (observed 2026-10-25,
        // webglbuild2.log L35413), producing zero output files. Pick safe, browser-serving
        // defaults: the shipped-by-editor default template, uncompressed artifacts (the
        // webfront serves .br sidecars, so container compression is redundant), a legacy
        // 256 MB heap, and full exception support so the browser console shows real stacks.
        private static void ApplyWebGLPlayerSettings()
        {
            try { PlayerSettings.WebGL.template = "APPLICATION:Default"; } catch (Exception e) { Side($"webgl template: {e.Message}"); }
            try { PlayerSettings.WebGL.memorySize = 256; } catch (Exception e) { Side($"webgl memorySize: {e.Message}"); }
            // AssetRipper left the 2022.2+ incremental-memory fields at zero, which broke the
            // emcc link (INITIAL_MEMORY=0) and later caused wasm OOM aborts at mission init
            // with a fixed heap. Use 512 MB initial with geometric growth to 3584 MB.
            // PlayerSettings.WebGL is a nested static class, so bind its static properties.
            try
            {
                var webglT = typeof(PlayerSettings).GetNestedType("WebGL", System.Reflection.BindingFlags.Public);
                if (webglT == null) { Side("webgl nested class missing"); }
                else
                {
                    var allP = webglT.GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                    var names = new string[allP.Length]; for (int i = 0; i < allP.Length; i++) names[i] = allP[i].Name;
                    Array.Sort(names);
                    Side("webgl static props: " + string.Join(",", names));
                }
                if (webglT != null) foreach (var pair in new object[][] { new object[] { "initialMemorySize", 512 }, new object[] { "maximumMemorySize", 3584 }, new object[] { "memoryGrowthMode", 2 }, new object[] { "geometricMemoryGrowthStep", 8 }, new object[] { "memoryGeometricGrowthCap", 512 } })
                {
                    var p = webglT.GetProperty((string)pair[0], System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                    if (p == null || !p.CanWrite) { Side($"webgl {pair[0]}: property missing"); continue; }
                    try
                    {
                        if (p.PropertyType.IsEnum) p.SetValue(null, Enum.ToObject(p.PropertyType, Convert.ToInt32(pair[1])));
                        else p.SetValue(null, Convert.ChangeType(pair[1], p.PropertyType));
                        Side($"webgl {pair[0]}={pair[1]}");
                    }
                    catch (Exception e) { Side($"webgl {pair[0]}: {e.Message}"); }
                }
            }
            catch (Exception e) { Side($"webgl memory fields: {e.Message}"); }
            foreach (var name in new[] { "Disabled", "Gzip" })
            {
                try
                {
                    var fmt = Enum.Parse(typeof(WebGLCompressionFormat), name);
                    PlayerSettings.WebGL.compressionFormat = (WebGLCompressionFormat)fmt;
                    Side($"webgl compression={name}");
                    break;
                }
                catch { }
            }
            // 2022.3 renamed the exception enum members, so discover the names at runtime
            // and pick whichever full-support mode exists; None would stub ___cxa_* with
            // abort() and every caught C# exception becomes a wasm abort.
            var eT = typeof(WebGLExceptionSupport);
            var eNames = Enum.GetNames(eT);
            Side($"exception enum names: {string.Join(",", eNames)}");
            foreach (var name in new[] { "FullWithStacktrace", "FullWithStackTraces", "FullWithExplicitStackTraces", "Full", "ExtractStackTraceOnly" })
            {
                if (Array.IndexOf(eNames, name) < 0) continue;
                try
                {
                    PlayerSettings.WebGL.exceptionSupport = (WebGLExceptionSupport)Enum.Parse(eT, name);
                    Side($"webgl exceptions={name}");
                    break;
                }
                catch (Exception e) { Side($"webgl exceptions={name}: {e.Message}"); }
            }
            try { AddAlwaysIncludedShaders(); } catch (Exception e) { Side($"always-included shaders: {e.Message}"); }
        }

        // The ripped game loads its character (UnityChanToonShader) and TextMeshPro shaders
        // only from addressable bundles, so Unity's scene-based automatic variant stripping
        // drops their keyword variants in the WebGL player: models render as untextured
        // silhouettes and all TMP text disappears. Always-included shaders ship every variant
        // Unity can enumerate, which is the documented fix for Addressables/runtime keywords.
        private static void AddAlwaysIncludedShaders()
        {
            var wanted = new System.Collections.Generic.List<UnityEngine.Shader>();
            // Only runtime shaders that live in addressable bundles and are otherwise stripped:
            // the 12 UnityChanToon character shaders + TextMeshPro shaders + character eye/mouth/
            // outline. Deliberately excludes editor-only junk a folder scan would pull in
            // (ProBuilder, Hidden/Shader Graph FallbackError, utility blend shaders) which
            // deadlocked the WebGL player at boot when force-included.
            var paths = new[] {
                "Assets/Toon/Shader/Toon_DoubleShadeWithFeather.shader",
                "Assets/Toon/Shader/Toon_DoubleShadeWithFeather_Clipping.shader",
                "Assets/Toon/Shader/Toon_DoubleShadeWithFeather_Clipping_StencilMask.shader",
                "Assets/Toon/Shader/Toon_DoubleShadeWithFeather_Clipping_StencilOut.shader",
                "Assets/Toon/Shader/Toon_DoubleShadeWithFeather_StencilMask.shader",
                "Assets/Toon/Shader/Toon_DoubleShadeWithFeather_TransClipping.shader",
                "Assets/Toon/Shader/Toon_DoubleShadeWithFeather_TransClipping_StencilMask.shader",
                "Assets/Toon/Shader/Toon_DoubleShadeWithFeather_TransClipping_StencilOut.shader",
                "Assets/Toon/Shader/Toon_ShadingGradeMap_StencilMask.shader",
                "Assets/Toon/Shader/Toon_ShadingGradeMap_TransClipping_StencilMask.shader",
                "Assets/Toon/Shader/ToonColor_DoubleShadeWithFeather.shader",
                "Assets/Toon/Shader/ToonColor_DoubleShadeWithFeather_Clipping.shader",
                "Assets/Shader/TextMeshPro_Distance Field.shader",
                "Assets/Shader/TextMeshPro_Mobile_Distance Field.shader",
                "Assets/Shader/TextMeshPro_Sprite.shader",
                "Assets/TextMesh Pro/Shaders/TMP_SDF.shader",
                "Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile.shader",
                "Assets/TextMesh Pro/Shaders/TMP_Sprite.shader",
                "Assets/Shaders/sg_cha_outline.shader",
                "Assets/Shaders/Character/Eye.shader",
                "Assets/Shaders/Character/Mouth.shader",
            };
            foreach (var path in paths)
            {
                var shader = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Shader>(path);
                if (shader != null) wanted.Add(shader); else Side("always-include missing: " + path);
            }
            var settings = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset");
            if (settings == null || settings.Length == 0) { Side("graphics settings object missing"); return; }
            var so = new UnityEditor.SerializedObject(settings[0]);
            var list = so.FindProperty("m_AlwaysIncludedShaders");
            // Build27-era runs persisted a broad folder-scan set (editor ProBuilder/Shader-Graph
            // junk) into this asset; strip any always-included entry that is a project asset
            // outside the allowlist. Built-in shaders report an empty asset path and are kept.
            var allow = new System.Collections.Generic.HashSet<UnityEngine.Object>();
            foreach (var s in wanted) allow.Add(s);
            for (int i = list.arraySize - 1; i >= 0; i--)
            {
                var obj = list.GetArrayElementAtIndex(i).objectReferenceValue;
                if (obj == null) { list.DeleteArrayElementAtIndex(i); continue; }
                var ep = UnityEditor.AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(ep) && ep.StartsWith("Assets/") && !allow.Contains(obj))
                    list.DeleteArrayElementAtIndex(i);
            }
            int added = 0;
            foreach (var shader in wanted)
            {
                bool present = false;
                for (int i = 0; i < list.arraySize; i++)
                    if (list.GetArrayElementAtIndex(i).objectReferenceValue == (UnityEngine.Object)shader) { present = true; break; }
                if (present) continue;
                list.InsertArrayElementAtIndex(0);
                list.GetArrayElementAtIndex(0).objectReferenceValue = shader;
                added++;
            }
            so.ApplyModifiedProperties();
            UnityEditor.EditorUtility.SetDirty(settings[0]);
            UnityEditor.AssetDatabase.SaveAssets();
            Side($"always-included shaders wanted={wanted.Count} added={added} total={list.arraySize}");
        }

        // GetAllWarningsAndErrors is editor-internal here, so read the report through the
        // public GetTotalMessages/GetPageOfMessages pair via reflection and side-log every
        // message the engine recorded for the failed phase.
        private static void DumpBuildReport(BuildReport report)
        {
            try
            {
                var t = report.GetType();
                int total = 0;
                var getTotal = t.GetMethod("GetTotalMessages", new[] { typeof(uint) });
                if (getTotal != null) total = (int)(uint)getTotal.Invoke(report, new object[] { uint.MaxValue });
                var getPage = t.GetMethod("GetPageOfMessages", new[] { typeof(uint), typeof(uint) });
                Side($"report messages total={total}");
                if (getPage == null) return;
                uint idx = 0;
                while (idx < (uint)total && idx < 60)
                {
                    var page = getPage.Invoke(report, new object[] { idx, 20u }) as System.Collections.IEnumerable;
                    if (page == null) break;
                    foreach (var m in page)
                    {
                        var mt = m.GetType();
                        string type = "?", msg = "?";
                        try { type = mt.GetField("type")?.GetValue(m)?.ToString() ?? mt.GetProperty("type")?.GetValue(m)?.ToString() ?? "?"; } catch { }
                        try { msg = (string)(mt.GetField("message")?.GetValue(m) ?? mt.GetProperty("message")?.GetValue(m)); } catch { }
                        if (msg == null) continue;
                        Side($"report[{type}]: {msg.Substring(0, Math.Min(220, msg.Length))}");
                        idx++;
                    }
                }
            }
            catch (Exception e)
            {
                Side($"report dump failed: {e.Message}");
            }
        }
    }
}

