using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI {

    public class TintableButton : Button {
        
        [SerializeField] Graphic[] graphics;
        [SerializeField] ColorDataReference normal;
        [SerializeField] ColorDataReference hover;
        [SerializeField] ColorDataReference click;

        bool isInside = false;

#if UNITY_EDITOR
        protected override void OnValidate() {
            base.OnValidate();
            transition = Transition.None;
        }
#endif

        protected override void Start() {
            base.Start();
            UpdateColors(normal);
        }

        public override void OnPointerEnter(PointerEventData eventData) {
            base.OnPointerEnter(eventData);
            UpdateColors(hover);
            isInside = true;
        }

        public override void OnPointerExit(PointerEventData eventData) {
            base.OnPointerExit(eventData);
            UpdateColors(normal);
            isInside = false;
        }

        public override void OnPointerDown(PointerEventData eventData) {
            base.OnPointerDown(eventData);
            UpdateColors(click);
        }

        public override void OnPointerUp(PointerEventData eventData) {
            base.OnPointerUp(eventData);
            if (isInside) {
                UpdateColors(hover);
            } else {
                UpdateColors(normal);
            }
        }

        void UpdateColors(ColorDataReference color) {
            for (int i = 0; i < graphics.Length; i++) {
                graphics[i].color = color.Get();
            }
        }

    }

}
