using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using GLTFast.Export;

public class AssetBundleToGlbExporter : EditorWindow
{
    private string bundlesRelativeFolder = "Assets/MirageXR/Common/AssetBundles";
    private string exportDestinationFolder = "~/Documents/denkfabrik/wekitecs/mirage/avatars-old";
    private bool isExporting = false;

    [MenuItem("MirageXR/Utilities/Batch Export Assets from AssetBundles to GLB")]
    public static void ShowWindow()
    {
        GetWindow<AssetBundleToGlbExporter>("glTFast Bundle Exporter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Batch AssetBundle to GLB Converter (glTFast)", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        GUI.enabled = !isExporting;
        bundlesRelativeFolder = EditorGUILayout.TextField("Source Folder", bundlesRelativeFolder);
        exportDestinationFolder = EditorGUILayout.TextField("Export Folder", exportDestinationFolder);
        EditorGUILayout.Space();

        if (GUILayout.Button(isExporting ? "Exporting... Please wait" : "Batch Export All Bundles", GUILayout.Height(35)))
        {
            _ = RunBatchExportAsync();
        }
        GUI.enabled = true;
    }

    private async Task RunBatchExportAsync()
    {
        isExporting = true;

        try
        {
            // 1. Resolve source path (relative to Unity project root)
            string fullSourcePath = Path.Combine(Directory.GetCurrentDirectory(), bundlesRelativeFolder);
            if (!Directory.Exists(fullSourcePath))
            {
                Debug.LogError($"[glTFast Exporter] Source folder not found: {fullSourcePath}");
                return;
            }

            // 2. Resolve destination path (expanding '~')
            string resolvedTargetPath = ResolveHomeDirectory(exportDestinationFolder);
            if (!Directory.Exists(resolvedTargetPath))
            {
                Directory.CreateDirectory(resolvedTargetPath);
            }

            // 3. Filter bundle files (exclude .meta, .manifest, compound .manifest.meta, and hidden files)
            string[] allFiles = Directory.GetFiles(fullSourcePath, "*", SearchOption.TopDirectoryOnly);
            var bundleFiles = allFiles.Where(filePath =>
            {
                string fileName = Path.GetFileName(filePath);
                if (fileName.StartsWith(".")) return false;
                if (fileName.EndsWith(".meta", StringComparison.OrdinalIgnoreCase) ||
                    fileName.EndsWith(".manifest", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                return true;
            }).ToArray();

            if (bundleFiles.Length == 0)
            {
                Debug.LogWarning($"[glTFast Exporter] No valid AssetBundle files found in: {fullSourcePath}");
                return;
            }

            Debug.Log($"[glTFast Exporter] Starting batch export of {bundleFiles.Length} bundle(s)...");

            int successCount = 0;
            int failureCount = 0;

            for (int i = 0; i < bundleFiles.Length; i++)
            {
                string bundleFilePath = bundleFiles[i];
                string bundleName = Path.GetFileName(bundleFilePath);

                EditorUtility.DisplayProgressBar(
                    "Exporting AssetBundles to GLB (glTFast)",
                    $"Processing ({i + 1}/{bundleFiles.Length}): {bundleName}",
                    (float)(i + 1) / bundleFiles.Length
                );

                bool result = await ExportSingleBundleAsync(bundleFilePath, resolvedTargetPath, bundleName);
                if (result)
                    successCount++;
                else
                    failureCount++;
            }

            Debug.Log($"<color=green>[glTFast Exporter] Export complete.</color> Successful: {successCount}, Failed: {failureCount}. Output: {resolvedTargetPath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[glTFast Exporter] Unexpected error during batch: {ex.Message}\n{ex.StackTrace}");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            isExporting = false;
            AssetDatabase.Refresh();
            Repaint();
        }
    }

    private static async Task<bool> ExportSingleBundleAsync(string fullBundlePath, string outputDir, string bundleName)
    {
        AssetBundle bundle = AssetBundle.LoadFromFile(fullBundlePath);
        if (bundle == null)
        {
            Debug.LogError($"[glTFast Exporter] Failed to load AssetBundle '{bundleName}'.");
            return false;
        }

        try
        {
            GameObject[] gameObjects = bundle.LoadAllAssets<GameObject>();
            if (gameObjects == null || gameObjects.Length == 0)
            {
                Debug.LogWarning($"[glTFast Exporter] No GameObject assets found in bundle '{bundleName}'.");
                return false;
            }

            foreach (GameObject prefab in gameObjects)
            {
                GameObject instance = UnityEngine.Object.Instantiate(prefab);
                instance.name = prefab.name;

                // Strip any mesh components or objects named 'pen'
                RemovePenComponents(instance);

                // Safely convert all remaining meshes to readable memory instances
                MakeAllMeshesReadable(instance);

                string targetGlbPath = Path.Combine(outputDir, $"{instance.name}.glb");

                try
                {
                    var exportSettings = new ExportSettings
                    {
                        Format = GltfFormat.Binary
                    };

                    var gameObjectExportSettings = new GameObjectExportSettings
                    {
                        OnlyActiveInHierarchy = false,
                        DisabledComponents = false
                    };

                    var export = new GameObjectExport(exportSettings, gameObjectExportSettings);
                    export.AddScene(new[] { instance }, instance.name);

                    bool success = await export.SaveToFileAndDispose(targetGlbPath);
                    if (success)
                    {
                        Debug.Log($"[glTFast Exporter] Successfully exported: {targetGlbPath}");
                    }
                    else
                    {
                        Debug.LogError($"[glTFast Exporter] glTFast failed to write file: {targetGlbPath}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[glTFast Exporter] Exception exporting '{instance.name}' from '{bundleName}': {ex.Message}");
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(instance);
                }
            }

            return true;
        }
        finally
        {
            bundle.Unload(false);
        }
    }

    private static void RemovePenComponents(GameObject root)
    {
        // 1. Remove SkinnedMeshRenderers named "pen" or referencing a "pen" mesh
        var skinnedRenderers = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (var smr in skinnedRenderers)
        {
            bool matchName = smr.gameObject.name.IndexOf("pen", StringComparison.OrdinalIgnoreCase) >= 0;
            bool matchMesh = smr.sharedMesh != null && smr.sharedMesh.name.IndexOf("pen", StringComparison.OrdinalIgnoreCase) >= 0;

            if (matchName || matchMesh)
            {
                // If the entire GameObject is just the Pen, destroy the GameObject; otherwise remove the component
                if (smr.transform.childCount == 0 && smr.gameObject != root)
                {
                    UnityEngine.Object.DestroyImmediate(smr.gameObject);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(smr);
                }
            }
        }

        // 2. Remove MeshFilters / MeshRenderers named "pen" or referencing a "pen" mesh
        var meshFilters = root.GetComponentsInChildren<MeshFilter>(true);
        foreach (var mf in meshFilters)
        {
            if (mf == null) continue;

            bool matchName = mf.gameObject.name.IndexOf("pen", StringComparison.OrdinalIgnoreCase) >= 0;
            bool matchMesh = mf.sharedMesh != null && mf.sharedMesh.name.IndexOf("pen", StringComparison.OrdinalIgnoreCase) >= 0;

            if (matchName || matchMesh)
            {
                var mr = mf.GetComponent<MeshRenderer>();
                if (mr != null) UnityEngine.Object.DestroyImmediate(mr);

                if (mf.transform.childCount == 0 && mf.gameObject != root)
                {
                    UnityEngine.Object.DestroyImmediate(mf.gameObject);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(mf);
                }
            }
        }
    }

    private static void MakeAllMeshesReadable(GameObject root)
    {
        // 1. Handle SkinnedMeshRenderers (Body, rigged clothing, animated accessories)
        var skinnedRenderers = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (var smr in skinnedRenderers)
        {
            if (smr != null && smr.sharedMesh != null && !smr.sharedMesh.isReadable)
            {
                Mesh bakedMesh = new Mesh();
                smr.BakeMesh(bakedMesh, false);
                bakedMesh.name = smr.sharedMesh.name + "_Readable";
                smr.sharedMesh = bakedMesh;
            }
        }

        // 2. Handle static MeshFilters (props, static accessories)
        var meshFilters = root.GetComponentsInChildren<MeshFilter>(true);
        foreach (var mf in meshFilters)
        {
            if (mf != null && mf.sharedMesh != null && !mf.sharedMesh.isReadable)
            {
                Mesh readableMesh = ConvertStaticMeshToReadableIsolated(mf.sharedMesh);
                if (readableMesh != null)
                {
                    mf.sharedMesh = readableMesh;
                }
            }
        }
    }

    private static Mesh ConvertStaticMeshToReadableIsolated(Mesh nonReadableMesh)
    {
        // Use a temporary isolated GameObject to avoid component collision on existing objects
        GameObject tempHost = new GameObject("Temp_Mesh_Baker");
        tempHost.hideFlags = HideFlags.HideAndDontSave;

        try
        {
            SkinnedMeshRenderer tempSmr = tempHost.AddComponent<SkinnedMeshRenderer>();
            tempSmr.sharedMesh = nonReadableMesh;

            Mesh readableMesh = new Mesh();
            tempSmr.BakeMesh(readableMesh, false);
            readableMesh.name = nonReadableMesh.name + "_Readable";

            return readableMesh;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[glTFast Exporter] Failed to bake static mesh '{nonReadableMesh.name}': {ex.Message}");
            return null;
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(tempHost);
        }
    }

    private static string ResolveHomeDirectory(string path)
    {
        if (string.IsNullOrEmpty(path)) return path;

        if (path.StartsWith("~"))
        {
            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(home, path.Substring(1).TrimStart('/', '\\'));
        }

        return Path.GetFullPath(path);
    }
}