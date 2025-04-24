using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MirageXR;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class Screenshot : EditorWindow
{

    private struct Screen
    {
        public Vector2Int resolution;
        public DeviceFormat format;
    }

    private readonly List<Screen> _sizes = new List<Screen>
    {
        new Screen
        {
            resolution = new Vector2Int(1242, 2688), //6.5 inch
            format = DeviceFormat.Phone,
        },
        new Screen
        {
            resolution = new Vector2Int(1242, 2208), //5.5 inch
            format = DeviceFormat.Phone,
        },
        new Screen
        {
            resolution = new Vector2Int(2732, 2048), //12.9 inch
            format = DeviceFormat.Tablet,
        },
    };

    private string _path = string.Empty;
    private Camera _camera;
    private ViewCamera _viewCamera;

    [MenuItem("Tools/Instant Screenshot")]
    public static void ShowWindow()
    {
        var editorWindow = GetWindow(typeof(Screenshot));
        editorWindow.autoRepaintOnSceneChange = true;
        editorWindow.Show();
        editorWindow.titleContent = new GUIContent("Screenshot");
    }

    private void OnGUI()
    {
        GUILayout.Label("Save Path", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.TextField(_path, GUILayout.ExpandWidth(false));
        if (GUILayout.Button("Browse", GUILayout.ExpandWidth(false)))
        {
            _path = EditorUtility.SaveFolderPanel("Path to Save Images", _path, Application.dataPath);
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.HelpBox("Choose the folder in which to save the screenshots ", MessageType.None);
        EditorGUILayout.Space();

        if (GUILayout.Button("Take Screenshot", GUILayout.MaxWidth(200), GUILayout.MinHeight(60)))
        {
            if (_path == string.Empty)
            {
                _path = EditorUtility.SaveFolderPanel("Path to Save Images", _path, Application.dataPath);
            }

            TakeScreenShots(_sizes);
        }

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Open Folder", GUILayout.MaxWidth(200), GUILayout.MinHeight(60)))
        {
            Application.OpenURL($"file://{_path}");
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.HelpBox("In case of any error, make sure you have Unity Pro as the plugin requires Unity Pro to work.", MessageType.Info);
    }

    private async void TakeScreenShots(IEnumerable<Screen> screens)
    {
        try
        {
            _viewCamera = RootView_v2.Instance.viewCamera;
            _camera = RootView_v2.Instance.viewCamera.GetComponent<Camera>();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return;
        }

        var oldSize = GetResolution();
        var oldFormat = _viewCamera.format;

        foreach (var screen in screens)
        {
            await ApplyScreen(screen);

            var path = Path.Combine(_path, $"screen_{screen.resolution.x}x{screen.resolution.y}");
            Directory.CreateDirectory(path);
            var filename = Path.Combine(path, $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png");
            CaptureScreenshot(_camera, screen.resolution.x, screen.resolution.y, filename);
        }

        await ApplyScreen(new Screen { resolution = oldSize, format = oldFormat });
    }

    private async Task ApplyScreen(Screen screen)
    {
        await SetResolution(screen.resolution);
        await _viewCamera.SetupFormat(screen.format);
    }

    private static async Task SetResolution(Vector2Int size)
    {
        var window = (EditorWindow)Resources.FindObjectsOfTypeAll(typeof(EditorWindow)).FirstOrDefault(t => t.GetType().FullName == "UnityEditor.GameView");
        if (window == null)
        {
            return;
        }

        var type = window.GetType();

        const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | 
                                          BindingFlags.Static | BindingFlags.Instance | 
                                          BindingFlags.DeclaredOnly;

        var methods = type.GetMethods(bindingFlags);
        var onLostFocus = methods.FirstOrDefault(t => t.Name == "OnLostFocus");
        var onResized = methods.FirstOrDefault(t => t.Name == "OnResized");

        var gameViewSizeProperty = GetAllProperties(type).FirstOrDefault(t => t.PropertyType.FullName == "UnityEditor.GameViewSize");
        if (TryGetPropertyValue<object>(window, gameViewSizeProperty, out var gameViewSize))
        {
            var properties = GetAllProperties(gameViewSize.GetType()).ToArray();
            var widthProperty = properties.FirstOrDefault(t => t.Name == "width");
            var heightProperty = properties.FirstOrDefault(t => t.Name == "height");

            TrySetPropertyValue(gameViewSize, widthProperty, size.x);
            TrySetPropertyValue(gameViewSize, heightProperty, size.y);

            TryCallMethod(window, onLostFocus);
            TryCallMethod(window, onResized);
        }

        window.Repaint();

        await Task.Delay(500);
    }

    private static Vector2Int GetResolution()
    {
        var window = (EditorWindow)Resources.FindObjectsOfTypeAll(typeof(EditorWindow)).FirstOrDefault(t => t.GetType().FullName == "UnityEditor.GameView");
        if (window == null) return Vector2Int.zero;

        var type = window.GetType();

        var gameViewSizeProperty = GetAllProperties(type).FirstOrDefault(t => t.PropertyType.FullName == "UnityEditor.GameViewSize");
        if (TryGetPropertyValue<object>(window, gameViewSizeProperty, out var gameViewSize))
        {
            var properties = GetAllProperties(gameViewSize.GetType()).ToArray();
            var widthProperty = properties.FirstOrDefault(t => t.Name == "width");
            var heightProperty = properties.FirstOrDefault(t => t.Name == "height");

            TryGetPropertyValue<int>(gameViewSize, widthProperty, out var width);
            TryGetPropertyValue<int>(gameViewSize, heightProperty, out var height);

            return new Vector2Int(width, height);
        }

        return Vector2Int.zero;
    }

    private static void CaptureScreenshot(Camera cam, int width, int height, string filePath)
    {
        var bakCamTargetTexture = cam.targetTexture;
        var bakCamClearFlags = cam.clearFlags;
        var bakRenderTextureActive = RenderTexture.active;

        var texTransparent = new Texture2D(width, height, TextureFormat.ARGB32, false);
        var renderTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
        var grabArea = new Rect(0, 0, width, height);

        RenderTexture.active = renderTexture;
        cam.targetTexture = renderTexture;
        cam.clearFlags = CameraClearFlags.SolidColor;

        cam.backgroundColor = Color.clear;
        cam.Render();
        texTransparent.ReadPixels(grabArea, 0, 0);
        texTransparent.Apply();

        var pngShot = texTransparent.EncodeToPNG();
        File.WriteAllBytes(filePath, pngShot);

        cam.clearFlags = bakCamClearFlags;
        cam.targetTexture = bakCamTargetTexture;
        RenderTexture.active = bakRenderTextureActive;
        RenderTexture.ReleaseTemporary(renderTexture);

        Destroy(texTransparent);
    }

    #region Help functions

    private static IEnumerable<FieldInfo> GetAllFields(Type type)
    {
        const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | 
                                          BindingFlags.Static | BindingFlags.Instance | 
                                          BindingFlags.DeclaredOnly;

        return type == null ? Enumerable.Empty<FieldInfo>() : type.GetFields(bindingFlags).Concat(GetAllFields(type.BaseType));
    }

    private static IEnumerable<PropertyInfo> GetAllProperties(Type type)
    {
        const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | 
                                          BindingFlags.Static | BindingFlags.Instance | 
                                          BindingFlags.DeclaredOnly;

        return type == null ? Enumerable.Empty<PropertyInfo>() : type.GetProperties(bindingFlags).Concat(GetAllProperties(type.BaseType));
    }

    private static IEnumerable<MethodInfo> GetAllMethods(Type type)
    {
        const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | 
                                          BindingFlags.Static | BindingFlags.Instance | 
                                          BindingFlags.DeclaredOnly;

        return type == null ? Enumerable.Empty<MethodInfo>() : type.GetMethods(bindingFlags).Concat(GetAllMethods(type.BaseType));
    }

    private static FieldInfo GetFieldFromTypeByName(Type type, string fieldName)
    {
        return GetAllFields(type).FirstOrDefault(t => t.Name == fieldName);
    }

    private static Dictionary<string, HashSet<string>> GetAllTypesAndPropertiesNamesFromBaseType<T>() where T : class
    {
        var dictionary = new Dictionary<string, HashSet<string>>();
        var types = GetAllSubclassesOfType<T>();
        foreach (var type in types)
        {
            var names = GetAllProperties(type).Select(t => t.Name).
                Concat(GetAllMethods(type).Select(t => t.Name));

            dictionary.Add(type.FullName, new HashSet<string>(names));
        }

        return dictionary;
    }
 
    private static IEnumerable<Type> GetAllSubclassesOfType<T>() where T : class
    {
        return typeof(T).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(T)));
    }
 
    private static bool TryGetFieldValue<T>(object instance, FieldInfo fieldInfo, out T value)
    {
        try
        {
            value = (T)fieldInfo.GetValue(instance);
            return true;
        }
        catch (Exception)
        {
            value = default;
            return false;
        }
    }

    private static bool TrySetFieldValue(object instance, FieldInfo fieldInfo, object value)
    {
        try
        {
            fieldInfo.SetValue(instance, value);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static bool TryGetPropertyValue<T>(object instance, PropertyInfo propertyInfo, out T value)
    {
        try
        {
            value = (T)propertyInfo.GetValue(instance);
            return true;
        }
        catch (Exception)
        {
            value = default;
            return false;
        }
    }

    private static bool TrySetPropertyValue(object instance, PropertyInfo propertyInfo, object value)
    {
        try
        {
            propertyInfo.SetValue(instance, value);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static bool TryCallMethod(object instance, MethodInfo methodInfo, params object[] values)
    {
        try
        {
            methodInfo.Invoke(instance, values);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
    #endregion
}