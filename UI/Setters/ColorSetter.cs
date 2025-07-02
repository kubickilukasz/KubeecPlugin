using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace UI {

    public abstract class ColorSetter : MonoBehaviour {

        [OnValueChanged("UpdateValues")]
        [SerializeField] protected ColorData colorData;
        [OnValueChanged("UpdateValues")]
        [SerializeField] protected ColorData.Variant variant;

        public abstract void UpdateColor();

        [Button]
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
