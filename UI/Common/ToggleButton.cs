using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI {

    public class ToggleButton : TintableButton, IActionBool{

        public event Action<bool> onActionBool;

        [SerializeField] ColorDataReference clicked;
        [SerializeField] RectTransform[] clickedEnableElements =new RectTransform[0];
        [SerializeField] RectTransform[] clickedDisableElements =new RectTransform[0];
        [SerializeField] bool isClicked;
        [SerializeField] bool canUnclick = true;

        public bool IsClicked => isClicked;

        protected override void Start() {
            base.Start();
            Refresh();
        }

        public void SetValue(bool value, bool notify = true) {
            if (value == isClicked) {
                return;
            }
            isClicked = !isClicked;
            Refresh(notify);
        }

        public void Toggle() {
            if (isClicked) {
                Unclick();
            } else {
                Click();
            }
        }

        public void Unclick() {
            if (isClicked) {
                isClicked = false;
                Refresh();
            }
        }

        protected override void OnClick() {
            base.OnClick();
            if (isClicked && !canUnclick) {
                return;
            }
            isClicked = !isClicked;
            Refresh();
        }

        void Refresh(bool notify = true) {
            foreach (var element in clickedEnableElements) {
                element.gameObject.SetActive(isClicked);
            }
            foreach (var element in clickedDisableElements) {
                element.gameObject.SetActive(!isClicked);
            }
            defaultColor = isClicked ? clicked : normal;
            if (notify) {
                onActionBool?.Invoke(isClicked);
            }
        }

    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(ToggleButton))]
    public class ToggleButtonEditor : UnityEditor.Editor {

        UnityEditor.SerializedProperty clicked;
        UnityEditor.SerializedProperty clickedEnableElements;
        UnityEditor.SerializedProperty clickedDisableElements;
        UnityEditor.SerializedProperty isClicked;
        UnityEditor.SerializedProperty canUnclick;

        void OnEnable() {
            clicked = serializedObject.FindProperty("clicked");
            clickedEnableElements = serializedObject.FindProperty("clickedEnableElements");
            clickedDisableElements = serializedObject.FindProperty("clickedDisableElements");
            isClicked = serializedObject.FindProperty("isClicked");
            canUnclick = serializedObject.FindProperty("canUnclick");
        }

        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            ToggleButton targetMenuButton = (ToggleButton)target;
            serializedObject.Update();
            UnityEditor.EditorGUILayout.PropertyField(clicked);
            UnityEditor.EditorGUILayout.PropertyField(clickedEnableElements);
            UnityEditor.EditorGUILayout.PropertyField(clickedDisableElements);
            UnityEditor.EditorGUILayout.PropertyField(isClicked);
            UnityEditor.EditorGUILayout.PropertyField(canUnclick);
            serializedObject.ApplyModifiedProperties();

        }
    }
#endif

}
