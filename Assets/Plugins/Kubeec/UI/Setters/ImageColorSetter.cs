using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI {

    [RequireComponent(typeof(Image))]
    public class ImageColorSetter : ColorSetter {

        Image _image;
        Image image {
            get {
                if (_image == null) {
                    _image = GetComponent<Image>();
                }
                return _image;
            }
        }

        public override void UpdateColor() {
            image.color = colorData.Get(variant);
        }

    }
}
