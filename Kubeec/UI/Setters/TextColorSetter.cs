using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI {

    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TextColorSetter : ColorSetter {

        TextMeshProUGUI _text;
        TextMeshProUGUI text {
            get {
                if (_text == null) {
                    _text = GetComponent<TextMeshProUGUI>();
                }
                return _text;
            }
        }

        public override void UpdateColor() {
            text.color = colorData.Get(variant);
        }

    }

}
