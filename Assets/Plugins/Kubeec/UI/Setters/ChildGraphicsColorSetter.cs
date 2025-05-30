using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

namespace UI {

    [ExecuteAlways]
    public class ChildGraphicsColorSetter : ColorSetter {

        [SerializeField] [HideInInspector] Graphic[] graphics;

        [Button]
        void Reset() {
            graphics = GetComponentsInChildren<Graphic>(true);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        public override void UpdateColor() {
            if (graphics != null) {
                foreach (Graphic item in graphics) {
                    item.color = colorData.Get(variant);
                }
            }
        }

    }

}
