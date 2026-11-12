using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MirageXR.EditorTools
{
    /// <summary>
    /// Batch-mode cross-check for the offline (Python) unused-asset analysis.
    ///
    /// Input : Library/MirageUnused/candidates.txt   (one GUID per line)
    ///         Library/MirageUnused/inuse.txt        (one GUID per line; optional)
    ///
    /// Output: Library/MirageUnused/closure_start.txt (guids reachable from the Start scene)
    ///         Library/MirageUnused/referrers.txt    (candidate TAB referrer, or NONE)
    ///         Library/MirageUnused/summary.txt
    ///
    /// Methods:
    ///   Run     -> fast: Start-scene closure + cross-check vs candidates/inuse
    ///   RunFull -> slow: also computes true reverse references for every candidate
    ///
    ///   Unity -batchmode -quit -projectPath . -executeMethod MirageXR.EditorTools.MirageUnusedCrossCheck.Run
    ///   Unity -batchmode -quit -projectPath . -executeMethod MirageXR.EditorTools.MirageUnusedCrossCheck.RunFull
    /// </summary>
    public static class MirageUnusedCrossCheck
    {
        const string Dir = "Library/MirageUnused";
        static readonly string[] StartScenes =
        {
            "Assets/MirageXR/Scenes/Start.unity",
        };
        const int MaxReferrersPerCandidate = 8;

        static Dictionary<string, string> _guidCache = new Dictionary<string, string>();
        static readonly Regex GuidRx =
            new Regex(@"^guid: ([0-9a-f]{32})", RegexOptions.Multiline);

        [MenuItem("MirageXR/Unused Cross-Check (fast)")]
        public static void Run()
        {
            RunCore(fullReferrerScan: false);
        }

        [MenuItem("MirageXR/Unused Cross-Check (full)")]
        public static void RunFull()
        {
            RunCore(fullReferrerScan: true);
        }

        static string GetGuid(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return null;
            }
            if (!assetPath.EndsWith(".meta", StringComparison.Ordinal))
            {
                assetPath += ".meta";
            }
            if (_guidCache.TryGetValue(assetPath, out var cached))
            {
                return cached;
            }
            string guid = null;
            try
            {
                var m = GuidRx.Match(File.ReadAllText(assetPath));
                if (m.Success)
                {
                    guid = m.Groups[1].Value;
                }
            }
            catch (Exception)
            {
                guid = null;
            }
            _guidCache[assetPath] = guid;
            return guid;
        }

        static void RunCore(bool fullReferrerScan)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            Directory.CreateDirectory(Dir);
            var candidatesFile = Path.Combine(Dir, "candidates.txt");
            if (!File.Exists(candidatesFile))
            {
                Debug.LogError("[CrossCheck] candidates.txt not found at " + candidatesFile);
                return;
            }

            var candidates = new SortedSet<string>(ReadGuids(candidatesFile));
            var inUseFile = Path.Combine(Dir, "inuse.txt");
            var inUse = File.Exists(inUseFile) ? new HashSet<string>(ReadGuids(inUseFile)) : null;
            Debug.Log($"[CrossCheck] candidates={candidates.Count} inuse={inUse?.Count ?? 0} full={fullReferrerScan}");

            // ---- Part A: start-scene closure --------------------------------
            var closure = new HashSet<string>();
            foreach (var scenePath in StartScenes)
            {
                var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
                if (sceneAsset == null)
                {
                    Debug.LogWarning($"[CrossCheck] cannot load scene {scenePath}");
                    continue;
                }
                var loaded = EditorSceneManager.LoadSceneInPlayMode(scenePath, new LoadSceneParameters(LoadSceneMode.Single));
                if (!loaded.IsValid())
                {
                    Debug.LogWarning($"[CrossCheck] failed to load {scenePath}");
                    continue;
                }
                foreach (var root in loaded.GetRootGameObjects())
                {
                    foreach (var dep in SafeCollect(root))
                    {
                        var guid = GetGuid(AssetDatabase.GetMainAssetPath(dep));
                        if (!string.IsNullOrEmpty(guid))
                        {
                            closure.Add(guid);
                        }
                    }
                }
                EditorSceneManager.UnloadAllScenes();
            }
            File.WriteAllLines(Path.Combine(Dir, "closure_start.txt"), closure.ToArray());
            Debug.Log($"[CrossCheck] part A: start closure = {closure.Count} guids");

            // ---- Part B: optional true reverse references -------------------
            var found = new Dictionary<string, List<string>>();
            if (fullReferrerScan)
            {
                foreach (var c in candidates)
                {
                    found[c] = new List<string>();
                }
                var ids = AssetDatabase.FindAssets("t:Object");
                var seenMain = new HashSet<string>();
                int checkedCount = 0;
                var t0 = System.Diagnostics.Stopwatch.StartNew();
                foreach (var id in ids)
                {
                    var mainPath = AssetDatabase.GetMainAssetPath(id);
                    if (mainPath.Length == 0 || seenMain.Contains(mainPath))
                    {
                        continue;
                    }
                    seenMain.Add(mainPath);
                    var src = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(mainPath);
                    if (src == null)
                    {
                        continue;
                    }
                    var srcGuid = GetGuid(mainPath);
                    foreach (var dep in SafeCollect(src))
                    {
                        var depGuid = GetGuid(AssetDatabase.GetAssetPath(dep));
                        if (depGuid == null || depGuid == srcGuid || !candidates.Contains(depGuid))
                        {
                            continue;
                        }
                        var list = found[depGuid];
                        if (list.Count < MaxReferrersPerCandidate && !list.Contains(mainPath))
                        {
                            list.Add(mainPath);
                        }
                    }
                    checkedCount++;
                    if (checkedCount % 2000 == 0)
                    {
                        Debug.Log($"[CrossCheck] part B: {checkedCount} sources ({(int)t0.Elapsed.TotalSeconds}s)");
                    }
                }
            }

            // ---- Write results ------------------------------------------------
            if (fullReferrerScan)
            {
                var sb = new StringBuilder();
                int none = 0;
                foreach (var kv in found)
                {
                    if (kv.Value.Count == 0)
                    {
                        none++;
                        sb.AppendLine(kv.Key + "\tNONE");
                    }
                    else
                    {
                        foreach (var r in kv.Value)
                        {
                            sb.AppendLine(kv.Key + "\t" + r);
                        }
                    }
                }
                File.WriteAllText(Path.Combine(Dir, "referrers.txt"), sb.ToString());
            }

            // ---- Cross-check summary ------------------------------------------
            var candInClosure = candidates.Where(c => closure.Contains(c)).ToList();
            var inUseInClosure = inUse != null
                ? inUse.Count(g => closure.Contains(g)) : 0;
            int inUseTotal = inUse?.Count ?? 0;
            // in-use assets NOT in the start closure are fine (referenced by other things);
            // the critical invariant: no UNUSED candidate should be in the start closure.

            var sum = new StringBuilder();
            sum.AppendLine($"full_referrer_scan={fullReferrerScan}");
            sum.AppendLine($"candidates={candidates.Count}");
            sum.AppendLine($"start_closure_guids={closure.Count}");
            sum.AppendLine($"candidates_in_start_closure={candInClosure.Count}");
            if (inUse != null)
            {
                sum.AppendLine($"inuse_total={inUseTotal}");
                sum.AppendLine($"inuse_in_start_closure={inUseInClosure}");
                sum.AppendLine($"inuse_not_in_start_closure={inUseTotal - inUseInClosure}");
            }
            if (fullReferrerScan)
            {
                int withRefs = found.Values.Count(v => v.Count > 0);
                sum.AppendLine($"candidates_with_true_referrers={withRefs}");
                sum.AppendLine($"candidates_without_true_referrers={candidates.Count - withRefs}");
            }
            sum.AppendLine($"elapsed_s={(int)sw.Elapsed.TotalSeconds}");
            File.WriteAllText(Path.Combine(Dir, "summary.txt"), sum.ToString());

            if (candInClosure.Count > 0)
            {
                File.WriteAllLines(Path.Combine(Dir, "candidates_in_closure.txt"), candInClosure);
                Debug.LogWarning($"[CrossCheck] WARNING: {candInClosure.Count} unused candidates ARE reachable from the Start scene! See candidates_in_closure.txt");
            }
            else
            {
                Debug.Log("[CrossCheck] OK: no unused candidate is reachable from the Start scene");
            }
            Debug.Log("[CrossCheck] DONE " + sum.ToString().Replace("\n", " "));
        }

        static IEnumerable<UnityEngine.Object> SafeCollect(UnityEngine.Object src)
        {
            try
            {
                return EditorUtility.CollectDependencies(src, true);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[CrossCheck] CollectDependencies failed: {e.Message}");
                return Enumerable.Empty<UnityEngine.Object>();
            }
        }

        static string[] ReadGuids(string path)
        {
            var list = new List<string>();
            foreach (var line in File.ReadAllLines(path))
            {
                var g = line.Trim().ToLowerInvariant();
                if (g.Length == 32)
                {
                    list.Add(g);
                }
            }
            return list.ToArray();
        }
    }
}
