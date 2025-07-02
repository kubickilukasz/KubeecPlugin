using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

namespace UI {

    [ExecuteAlways]
    public class ChildGraphicsColorSetter : ColorSetter {

        [SerializeField] [HideInInspector] Graphic[] graphics = new Graphic[0];
        [SerializeField] protected bool overrdieAlpha = true;

        [Button]
        void Reset() {
            graphics = GetComponentsInChildren<Graphic>(true);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        public override void UpdateColor() {
            if (graphics != null) {
                if (overrdieAlpha) {
                    foreach (Graphic item in graphics) {
                        item.color = colorData.Get(variant);
                    }
                } else {
                    foreach (Graphic item in graphics) {
                        Color color = colorData.Get(variant);
                        item.color = ColorData.GetWithAlpha(item.color.a, color);
                    }
                }
            }
        }

    }

}
