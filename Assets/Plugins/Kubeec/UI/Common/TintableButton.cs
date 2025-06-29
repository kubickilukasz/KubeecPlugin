using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI {

    public class TintableButton : Button, IAction{

        public event Action onAction;

        [SerializeField] protected Graphic[] graphics;
        [SerializeField] protected ColorDataReference normal;
        [SerializeField] protected ColorDataReference hover;
        [SerializeField] protected ColorDataReference click;

        protected ColorDataReference defaultColor;
        protected bool isInside = false;

        
#if UNITY_EDITOR
        protected override void OnValidate() {
            base.OnValidate();
            transition = Transition.None;
        }
#endif

        protected override void Start() {
            base.Start();
            defaultColor = normal;
            UpdateColors(defaultColor);
        }

        public override void OnPointerEnter(PointerEventData eventData) {
            base.OnPointerEnter(eventData);
            UpdateColors(hover);
            isInside = true;
        }

        public override void OnPointerExit(PointerEventData eventData) {
            base.OnPointerExit(eventData);
            UpdateColors(defaultColor);
            isInside = false;
        }

        public override void OnPointerDown(PointerEventData eventData) {
            base.OnPointerDown(eventData);
            UpdateColors(click);
            OnClick();
        }

        public override void OnPointerUp(PointerEventData eventData) {
            base.OnPointerUp(eventData);
            if (isInside) {
                UpdateColors(hover);
            } else {
                UpdateColors(defaultColor);
            }
        }

        public virtual void Click() {
            UpdateColors(click);
            OnClick();
        }

        protected void UpdateColors(ColorDataReference color) {
            Color _color = color.Get();
            for (int i = 0; i < graphics.Length; i++) {
                graphics[i].color = _color;
            }
        }

        protected virtual void OnClick() {
            onAction?.Invoke();
        }

    }


#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(TintableButton))]
    public class TintableButtonEditor : UnityEditor.Editor {

        UnityEditor.SerializedProperty graphics;
        UnityEditor.SerializedProperty normal;
        UnityEditor.SerializedProperty hover;
        UnityEditor.SerializedProperty click;

        void OnEnable() {
            graphics = serializedObject.FindProperty("graphics");
            normal = serializedObject.FindProperty("normal");
            hover = serializedObject.FindProperty("hover");
            click = serializedObject.FindProperty("click");
        }

        public override void OnInspectorGUI() {
            //DrawDefaultInspector();
            TintableButton targetMenuButton = (TintableButton)target;
            serializedObject.Update();
            UnityEditor.EditorGUILayout.PropertyField(graphics);
            UnityEditor.EditorGUILayout.PropertyField(normal);
            UnityEditor.EditorGUILayout.PropertyField(hover);
            UnityEditor.EditorGUILayout.PropertyField(click);
            serializedObject.ApplyModifiedProperties();
            
        }
    }
#endif
}


