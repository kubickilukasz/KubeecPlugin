using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using static MyEditorSettings;
using UnityEngine.UIElements;


[DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
public class EditorSettingsWindow : EditorWindow {

    SetupData[] setupData;

    void Setup() {
        setupData = new SetupData[] { 
            new(TypeSetting.ShowInteractionGizmos, "Show Interaction Gizmos"),
            new(TypeSetting.AlwaysShowGhostHandInteraction, "Always ShowGhost Hand Interaction"),
            new(TypeSetting.ShowBothGhostHandsInteractions, "Show Both Ghost Hands Interactions"),
            new(TypeSetting.ShowCurrentHitPoints, "Show Current Hit Points"),
            new(TypeSetting.ShowGizmosLazyNavigation, "Show Gizmos Lazy Navigation"),
        };
    }


    [MenuItem("Tools/Custom Settings")]
    public static void Show() {
        EditorSettingsWindow wnd = GetWindow<EditorSettingsWindow>();
        wnd.titleContent = new GUIContent("Settings");

    }

    public void CreateGUI() {
        Setup();
        VisualElement root = rootVisualElement;

        for (int i = 0; i < setupData.Length; i++) {
            if (!setupData[i].basedSetting.HasValue || setupData[i].basedSetting.Value.GetToggle()) {
                CreateToggle(ref root, setupData[i].typeSetting, setupData[i].label);
            }
        }
    }

    string GetDebuggerDisplay() {
        return ToString();
    }

    void CreateToggle(ref VisualElement root, TypeSetting typeSetting, string label) {
        Toggle toggle = new Toggle();
        toggle.name = typeSetting.ToString();
        toggle.label = label;
        toggle.value = typeSetting.GetToggle();
        toggle.RegisterValueChangedCallback(x => {
            typeSetting.SetToggle(x.newValue);
        });
        root.Add(toggle);
    }

    class SetupData {
        public TypeSetting typeSetting;
        public string label;
        public TypeSetting? basedSetting;

        public SetupData(TypeSetting typeSetting, string label, TypeSetting? basedSetting = null) {
            this.typeSetting = typeSetting;
            this.label = label;
            this.basedSetting = basedSetting;
        }
    }
}


