using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CustomEditor(typeof(TintableButton))]
public class TintableButtonEditor : ButtonEditor {
    private SerializedProperty graphicsProperty;
    private SerializedProperty normalProperty;
    private SerializedProperty hoverProperty;
    private SerializedProperty clickProperty;

    protected override void OnEnable() {
        base.OnEnable();

        graphicsProperty = serializedObject.FindProperty("graphics");
        normalProperty = serializedObject.FindProperty("normal");
        hoverProperty = serializedObject.FindProperty("hover");
        clickProperty = serializedObject.FindProperty("click");
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        serializedObject.Update();

        EditorGUILayout.PropertyField(graphicsProperty);
        EditorGUILayout.PropertyField(normalProperty);
        EditorGUILayout.PropertyField(hoverProperty);
        EditorGUILayout.PropertyField(clickProperty);

        serializedObject.ApplyModifiedProperties();
    }
}
