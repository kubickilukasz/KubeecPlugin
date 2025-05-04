using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI {

    public abstract class ColorSetter : MonoBehaviour {

        [SerializeField] protected ColorData colorData;
        [SerializeField] protected ColorData.Variant variant;

        public abstract void UpdateColor();

        void UpdateValues() {
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(gameObject, "update color");
#endif
            UpdateColor();
#if UNITY_EDITOR
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(gameObject);
            UnityEditor.EditorUtility.SetDirty(gameObject);
            UnityEditor.AssetDatabase.SaveAssets();
#endif
        }

    }
}
