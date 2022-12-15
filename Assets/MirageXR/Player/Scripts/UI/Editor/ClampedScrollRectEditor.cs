#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof (ClampedScrollRect), true)]
[CanEditMultipleObjects]
public class ClampedScrollRectEditor : ScrollRectEditor
{
    private SerializedProperty _onItemChanged;
    private SerializedProperty _curve;
    private SerializedProperty _moveTime;

    protected override void OnEnable() {
        base.OnEnable();
        _onItemChanged = serializedObject.FindProperty("_onItemChanged");
        _curve = serializedObject.FindProperty("_curve");
        _moveTime = serializedObject.FindProperty("_moveTime");
    }

    public override void OnInspectorGUI() {
        serializedObject.ApplyModifiedProperties();
        base.OnInspectorGUI();
        EditorGUILayout.PropertyField(_onItemChanged);
        EditorGUILayout.PropertyField(_moveTime);
        EditorGUILayout.PropertyField(_curve);
        serializedObject.ApplyModifiedProperties();
    }
}
#endif