using System.Collections.Generic;
using UnityEngine;

public class MyEditorSettings{

    static MyEditorSettings _instane;

    Dictionary<int, object> cachedValues = new Dictionary<int, object>();

    SetupSetting[] setupSettings = new SetupSetting[0];

    public static MyEditorSettings Instance { 
        get {
            if (_instane == null) {
                _instane = new MyEditorSettings();
            }
            return _instane; 
        } 
    }

    MyEditorSettings() {
        setupSettings = new SetupSetting[]{
            new(TypeSetting.Other, false),
            new(TypeSetting.ShowInteractionGizmos, true),
            new(TypeSetting.AlwaysShowGhostHandInteraction, false),
            new(TypeSetting.ShowBothGhostHandsInteractions, false),
            new(TypeSetting.ShowCurrentHitPoints, false)
        };
        ReadValuesFromPrefs();
    }

    public void SetToggle(TypeSetting setting, bool value) {
        SetBool((int)setting, value);
#if UNITY_EDITOR
        UnityEditor.EditorPrefs.SetBool(((int)setting).ToString(), value);
        UnityEditor.SceneView.RepaintAll();
#endif
    }

    public bool GetToggle(TypeSetting setting) {
        int iKey = (int)setting;
        if (cachedValues.ContainsKey(iKey)) {
            return (bool)cachedValues[iKey];
        }
        return false;
    }

    void ReadValuesFromPrefs() {
        cachedValues = new Dictionary<int, object>();
        for (int i = 0; i < setupSettings.Length; i++) {
            ReadBool((int)setupSettings[i].typeSetting, setupSettings[i].defaultValue);
        }
    }

    void SetBool(int key, bool value) {
        if (cachedValues.ContainsKey(key)) {
            cachedValues[key] = value;
        } else {
            cachedValues.Add(key, value);
        }
    }

    void ReadBool(int key, bool defaultValue) {
        string keys = key.ToString();
#if UNITY_EDITOR
        if (UnityEditor.EditorPrefs.HasKey(keys)) {
            defaultValue = UnityEditor.EditorPrefs.GetBool(keys);
        }
#endif
        SetBool(key, defaultValue);
    }

    public enum TypeSetting {
        Other = 0, 
        ShowInteractionGizmos,
        AlwaysShowGhostHandInteraction,
        ShowBothGhostHandsInteractions,
        ShowCurrentHitPoints,
    }

    class SetupSetting {
        public TypeSetting typeSetting;
        public bool defaultValue;

        public SetupSetting(TypeSetting typeSetting, bool defaultValue) {
            this.typeSetting = typeSetting;
            this.defaultValue = defaultValue;
        }
    }

}

public static class MyEditorSettingsExt{

    public static bool GetToggle(this MyEditorSettings.TypeSetting setting) {
        return MyEditorSettings.Instance.GetToggle(setting);
    }

    public static void SetToggle(this MyEditorSettings.TypeSetting setting, bool value) {
        MyEditorSettings.Instance.SetToggle(setting, value);
    }

}
